// SPDX-License-Identifier: MPL-2.0
using System.Text.Json.Serialization.Metadata;

namespace GameDevKit.Serialization.Interfaces
{
    /// <summary>
    /// 配置类必须实现此接口，以支持自动补全默认值
    /// </summary>
    /// <typeparam name="T">自身类型</typeparam>
    public interface IConfig<T> where T : class, IConfig<T>, new()
    {
        /// <summary>
        /// 使用提供的默认值填补自身的缺失字段
        /// </summary>
        void ApplyDefaults(T defaults);

        /// <summary>
        /// 返回该类型专用的 System.Text.Json 源生成器上下文
        /// </summary>
        static abstract JsonTypeInfo<T> JsonTypeInfo { get; }
    }
}
