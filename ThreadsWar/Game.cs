using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static System.Console;
using static ThreadsWar.PInvoker;

namespace ThreadsWar
{

    public class Game
    {
        private readonly IntPtr _consoleWindowOutput;
        private readonly IntPtr _consoleWindowInput;
        public Thread _mainThread;
        private readonly object _screenlocker = new object();
        public readonly object _fatherlocker = new object();
        private readonly object _gameover = new object();
        private int _enemiesKilled = 0;
        private int _enemiesSurvived = 0;
        private int _enemiesAlive = 0;
        public SemaphoreSlim Sem = new SemaphoreSlim(initialCount: 3, maxCount: 3);
        private readonly char[] _badchar = {'-','\\','|','/'};
        public bool PlayerMoved = false;
        private readonly Random rnd = new Random();

        public int RandomInt(int from, int to) => rnd.Next(from, to);

        public Game()
        {
            _consoleWindowOutput = GetStdHandle(-11); // -12 ошибка
            _consoleWindowInput = GetStdHandle(-10);
        }

        public void Bullet(short x, short y)
        {
            if (!Sem.Wait(0)) return;
            if (GetCharacterAt(x, y) == '+') return;
            while (--y > 0)
            {
                DrawCharacter("+", x, y);
                Thread.Sleep(100);
                DrawCharacter(" ", x, y);
            }
            Sem.Release();
        }

        public void DrawCharacter(string ch, int x, int y)
        {
            Monitor.Enter(_screenlocker);    
            try
            {
                SetCursorPosition(x, y);
                Write(ch);
            }
            finally
            {
                Monitor.Exit(_screenlocker);
            }
        }

        public char GetCharacterAt(short x, short y)
        {
            var readBuffer = new char[1] {' '};
            lock (_screenlocker)
                ReadConsoleOutputCharacter(_consoleWindowOutput, readBuffer, 1, new PInvoker.COORD() {X = x, Y = y}, out var readCount);
            return readBuffer[0];
        }

        public void BadGuy()
        {
            Interlocked.Increment(ref _enemiesAlive);
            short y = (short)RandomInt(2, WindowHeight - 2);
            short x = y % 2 == 0 ? (short) 1 : (short) (WindowWidth - 2);
            short dir = x == WindowWidth - 2 ? (short)-1 : (short)1;
            while (dir == 1 && x != WindowWidth || dir == -1 && x != 0)
            {
                bool Hitme = false;
                DrawCharacter(_badchar[RandomInt(0, 4)].ToString(), x, y);

                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(40);
                    if (GetCharacterAt(x, y) == '+')
                    {
                        Hitme = true;
                        break;
                    }
                }
                DrawCharacter(" ", x, y);

                if (Hitme)
                {
                    Interlocked.Increment(ref _enemiesKilled);
                    Interlocked.Decrement(ref _enemiesAlive);
                    DrawScore();
                    return;
                }

                x += dir;
            }

            Interlocked.Increment(ref _enemiesSurvived);
            DrawScore();
        }

        public void BadGuysFather()
        {
             Monitor.Enter(_fatherlocker);
             try
             {
                 Monitor.Wait(_fatherlocker, 15000);
                 PlayerMoved = true;
                 while (true) {
                     if (RandomInt(0, 100) < (_enemiesKilled + _enemiesSurvived) / 25 + 20)
                     {
                         new Thread(BadGuy) {Name = "Badguy"}.Start();
                         Thread.Sleep(1000);
                     }
                 }
             }
             finally
             {
                 Monitor.Exit(_fatherlocker);
             }
        }

        public void DrawScore()
        {
            lock (_screenlocker)
            {
                SetCursorPosition(0, 0);
                Write($"Убитых потоков: {_enemiesKilled}, Выживших потоков: {_enemiesSurvived}");
                if (_enemiesSurvived > 30 || _enemiesKilled > 10)
                {
                    var procentSurvived = (double) _enemiesSurvived / _enemiesAlive * 100;
                    _mainThread.Abort();
                    lock (_gameover)
                        MessageBox(IntPtr.Zero,
                            _enemiesKilled > 10
                                ? $"Вы выиграли!\n" +
                                  $"Выживших потоков: " + $"{procentSurvived:f1}%\n".Replace(',', '.')
                                  + (procentSurvived == 0
                                      ? "Отлично!"
                                      : procentSurvived > 0 && procentSurvived <= 25
                                          ? "Хорошо."
                                          : procentSurvived > 25 && procentSurvived <= 50
                                              ? "Неплохо."
                                              : procentSurvived > 50
                                                  ? "Неплохо, но можно лучше =)" : "")
                                : "Вы проиграли!", $"Успех {(_enemiesKilled > 10 ? $"на {100 - procentSurvived:f1}%".Replace(',','.') : "был близок        ")}", 0);
                    Environment.Exit(0);
                }
            }
        }

        public VirtualKeys GetCurrentKey()
        {
            INPUT_RECORD[] input = new INPUT_RECORD[128];
            while (true)
            {
                ReadConsoleInput(_consoleWindowInput, input, 128, out uint num);
                foreach (var inp in input)
                {
                    if (inp.EventType != INPUT_RECORD.KEY_EVENT) continue;
                    if (!inp.Event.KeyEvent.bKeyDown) continue;
                    return inp.Event.KeyEvent.wVirtualKeyCode;
                }
            }
        }
    }
}