using System;
using System.Threading;
using NUnit.Framework;

namespace NetroTest.Util
{
    public class AsyncTest
    {
        protected const string Host = "localhost";
        private static int _port = 12111;

        [SetUp]
        public void SetUp()
        {
            _port += 5;
        }

        public void Until(Action<int, Action> run)
        {
            var doRun = true;
            run(_port, () => doRun = false);
            while (doRun) Thread.Sleep(100);
        }

        public void Until(Action<int, Action<bool>> run)
        {
            var complete = false;
            run(_port, done => complete = done);
            while (!complete) Thread.Sleep(100);
        }
    }
}