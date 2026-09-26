// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using System.Text.Json.Serialization.Metadata;

namespace GameDevKit.Serialization.Abstractions
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
