using System;
using System.Reflection;

namespace Algorithm_BlockBuilder
{
    public static class MethodTimeLogger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
            log.Info($"{methodBase.Name}: {milliseconds} ms {message}");
        }
    }
}
