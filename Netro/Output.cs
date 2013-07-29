using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Netro
{
    public class Output
    {
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

        private static readonly Dictionary<NetroStatus.Type, string> TypeFormats = new Dictionary
            <NetroStatus.Type, string>
            {
                {NetroStatus.Type.Proxy, "Proxy ({0} -> {1}"},
                {NetroStatus.Type.ReverseClient, "Reverse client ({0} -> {1})"},
                {NetroStatus.Type.ReverseServer, "Reverse server ({0} -> {1})"}
            };

        private readonly NetroStatus _status;

        public Output(NetroStatus netroStatus)
        {
            _status = netroStatus;

            Console.Clear();
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Write("Loading...");

            DrawLogo();
            SetConnectedStatus(false);
            SetType();
            _status.OnConnect(SetConnectedStatus);
        }

        private static void DrawLogo()
        {
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

        private static void ClearRect(int x, int y, int width, int height)
        {
            Console.ResetColor();
            Console.CursorSize = height;
            Console.SetCursorPosition(x, y);
            Console.Write(string.Join("", Enumerable.Range(0, width).Select(s => " ").ToArray()));
            Console.CursorSize = 1;
        }

        public void SetType()
        {
            var type = string.Format(TypeFormats[_status.CurrentType], _status.From, _status.To);
            ClearRect(0, 0, Console.WindowWidth, 1);

            Console.ResetColor();
            Console.SetCursorPosition(1, 3);
            Console.WriteLine("Type: {0}", type);
        }

        public void SetConnectedStatus(bool connected)
        {
            ClearRect(1, 1, 10, 1);
            Console.SetCursorPosition(1, 1);
            Console.BackgroundColor = connected ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            var text = connected ? "CONNECTED" : "WAITING";
            Console.WriteLine(text);
        }

        public void Reset()
        {
            Console.SetCursorPosition(0, 20);
            Console.ResetColor();
            Console.CursorVisible = true;
        }
    }
}