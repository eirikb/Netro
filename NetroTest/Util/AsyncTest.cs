using System;
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
            while (doRun)
            {
            }
        }

        public void Until(Action<int, Action<bool>> run)
        {
            var doRun = true;
            run(_port, done => doRun = done);
            while (doRun)
            {
            }
        }
    }
}