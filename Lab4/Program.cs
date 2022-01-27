using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab4
{
    internal static class Program
    {
        private static void Main()
        {
            var stopwatch = new Stopwatch();
            var hosts = new List<string> { "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/progs/srv-begin-end.cs" };
            var executor = new SyncTasksHttpExecutor(hosts);
            executor.Execute();
            
            var executorAsync = new AsyncTasksHttpExecutor(hosts);
            executorAsync.Execute();

            var executorCallback = new CallbacksHttpExecutor(hosts);
            executorCallback.Execute();
        }
    }
}
