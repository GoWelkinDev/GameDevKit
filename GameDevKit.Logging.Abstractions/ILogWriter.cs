// SPDX-License-Identifier: MPL-2.0
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
