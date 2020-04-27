using System;
using System.Threading;
using static System.Console;
using static ThreadsWar.PInvoker;

namespace ThreadsWar
{
    class Program
    {
        public static void Main(string[] args)
        {
            CursorVisible = false;
            Title = "Threads War";
            using (new Mutex(true, "ThreadsWarMutex", out var notCreated)) {
                if (!notCreated) {
                    WriteLine("Threads War is running right now...");
                    ReadKey();
                    return;
                }

                var game = new Game();

                game.DrawScore();

                game._mainThread = new Thread(game.BadGuysFather) { Name = "BadguysFather" };
                game._mainThread.Start();

                short x = (short)(WindowWidth / 2);
                short y = (short)(WindowHeight - 1);

                while (true) {
                    game.DrawCharacter("|", x, y);
                    switch (game.GetCurrentKey()) {
                        case VirtualKeys.Left:
                            if (!game.PlayerMoved) lock (game._fatherlocker)
                                Monitor.Pulse(game._fatherlocker);
                            if (x > 0) {
                                game.DrawCharacter(" ", x, y);
                                x--;
                            }
                            break;
                        case VirtualKeys.Right:
                            if (!game.PlayerMoved) lock (game._fatherlocker)
                                Monitor.Pulse(game._fatherlocker);
                            if (x != WindowWidth - 2) {
                                game.DrawCharacter(" ", x, y);
                                x++;
                            }
                            break;
                        case VirtualKeys.Space:
                            new Thread(() => game.Bullet(x, Convert.ToInt16(y - 1))) { Name = "BadguysKiller" }.Start();
                            Thread.Sleep(100);
                            break;
                    }
                }
            }
        }
    }
}
