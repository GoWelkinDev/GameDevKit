// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

namespace GameDevKit.Logging.Abstractions
{
    /// <summary>
    /// 日志写入器抽象，由引用端自行实现
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// 写入一条调试级别日志
        /// </summary>
        void WriteDebug(string message);

        /// <summary>
        /// 写入一条信息级别日志
        /// </summary>
        void WriteInformation(string message);

        /// <summary>
        /// 写入一条警告级别日志
        /// </summary>
        void WriteWarning(string message);

        /// <summary>
        /// 写入一条错误级别日志
        /// </summary>
        void WriteError(string message, Exception? ex = null);
    }
}
