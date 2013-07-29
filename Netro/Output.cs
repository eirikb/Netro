using System;
using System.Linq;
using System.Reflection;

namespace Netro
{
    public class Output
    {
        private static readonly Output OutputInstance = new Output();

        private static readonly string[] Logo = new[]
            {
                @"    _......_    ",
                @" .'\|_|_|_|_/'.",
                @"/_\/` ____ `\/_\",
                @"|_|  [ ## ]  |_|",
                @"|_| | )||( | |_|",
                @"|_| \//()\\/ |_|",
                @"|_|  \\__//  |_|",
                @"|_| /`/  \`\ |_|",
                @"|_| \:____:/ |_|",
                @"   =//====\\=",
                @"  =//======\\=",
                @" =//========\\="
            };

        static Output()
        {
        }

        private Output()
        {
        }

        public static Output Instance
        {
            get { return OutputInstance; }
        }

        public bool Connected
        {
            set { SetStatus(value); }
        }

        public void Init()
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Write("Loading...");

            var x = Console.WindowWidth - Logo.First().Length - 1;
            var y = 1;
            Console.ForegroundColor = ConsoleColor.White;
            Logo.ToList().ForEach(line =>
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(line);
                    y++;
                });
            Console.SetCursorPosition(x, y);
            Console.Write(" Netro {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
        }

        public void ClearRect(int x, int y, int width, int height)
        {
            Console.ResetColor();
            Console.CursorSize = height;
            Console.SetCursorPosition(x, y);
            Console.Write(string.Join("", Enumerable.Range(0, width).Select(s => " ").ToArray()));
            Console.CursorSize = 1;
        }

        public void SetType(string type)
        {
            ClearRect(0, 0, Console.WindowWidth, 1);
            SetStatus(false);

            Console.ResetColor();
            Console.SetCursorPosition(1, 3);
            Console.WriteLine("Type: {0}", type);
        }

        public void SetStatus(bool connected)
        {
            ClearRect(1, 1, 10, 1);
            Console.SetCursorPosition(1, 1);
            Console.BackgroundColor = connected ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            var text = connected ? "CONNECTED" : "WAITING";
            Console.WriteLine(text);
        }

        public void SetReverse(ReverseAsyncSocket reverse)
        {
            var disconnect = new Action(() =>
                {
                    Reset();
                    Console.WriteLine("No connection to reverse");
                    Environment.Exit(0);
                });
            reverse.Connect(socket =>
                {
                    Connected = true;
                    socket.Disconnect(disconnect);
                });
            reverse.Disconnect(disconnect);
        }

        public void Reset()
        {
            Console.SetCursorPosition(0, 20);
            Console.ResetColor();
            Console.CursorVisible = true;
        }
    }
}