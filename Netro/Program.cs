using System;
using System.Linq;

namespace Netro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var netro = new Netro();

            var hasListen = false;
            var hasClient = false;

            args.ToList().ForEach(arg =>
                {
                    int port;

                    var split = arg.Split(':');
                    if (split.Length > 1)
                    {
                        port = int.Parse(split[1]);

                        if (!hasClient) netro.SetClient(split[0], port);
                        else netro.ConnectReverse(split[0], port);

                        hasClient = true;
                        return;
                    }

                    port = int.Parse(arg);

                    if (!hasListen) netro.Listen(port);
                    else netro.ListenReverse(port);

                    hasListen = true;
                });


            var netroStatus = new NetroStatus(netro);

            if (netroStatus.CurrentType == NetroStatus.Type.None)
            {
                Console.WriteLine("Netro not running, missing arguments?");
                Console.WriteLine("Check https://github.com/eirikb/Netro for usage examples");
                return;
            }

            var output = new Output(netroStatus);
            Console.Read();
            output.Reset();
        }
    }
}