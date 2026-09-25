// SPDX-License-Identifier: MPL-2.0
namespace GameDevKit.Logging.Abstractions
{
    public static class Log
    {
        public static ILogWriter? Writer { get; set; }

        public static void Debug(string message)
        {
            Writer?.WriteDebug(message);
        }

        public static void Print(string message)
        {
            Writer?.WriteInformation(message);
        }

        public static void PrintWarning(string message)
        {
            Writer?.WriteWarning(message);
        }

        public static void PrintErr(string message, Exception? ex = null)
        {
            Writer?.WriteError(message, ex);
        }
    }
}
