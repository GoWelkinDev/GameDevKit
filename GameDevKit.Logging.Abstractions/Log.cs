// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

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
