// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

namespace GameDevKit.Serialization.Abstractions
{
    /// <summary>
    /// 设置管理器抽象基类，提供加载、保存、应用的标准流程。
    /// </summary>
    public abstract class SettingsManager<T> where T : SettingsManager<T>, new()
    {
        private static readonly Lazy<T> _instance = new();
        public static T Instance => _instance.Value;

        protected SettingsManager()
        {
            LoadConfiguration();
            ApplySettings();
        }

        /// <summary>
        /// 加载配置（若文件不存在则创建并写入默认值）
        /// </summary>
        protected abstract void LoadConfiguration();

        /// <summary>
        /// 将当前配置持久化
        /// </summary>
        protected abstract void SaveConfiguration();

        /// <summary>
        /// 将配置值应用到运行时环境
        /// </summary>
        protected abstract void ApplySettings();

        /// <summary>
        /// 保存配置并重新应用设置
        /// </summary>
        public void Save()
        {
            SaveConfiguration();
            ApplySettings();
        }

        /// <summary>
        /// 重新加载配置文件并应用设置
        /// </summary>
        public void Reload()
        {
            LoadConfiguration();
            ApplySettings();
        }
    }
}
