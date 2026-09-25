// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using GameDevKit.Networking.Abstractions;

namespace GameDevKit.Platforms.Abstractions
{
    public interface IGamePlatform
    {
        string Name { get; }

        PlatformType PlatformType { get; }

        PlatformRunMode RunMode { get; }

        bool IsOnline { get; }
        ulong UserId { get; }
        string Username { get; }
        string Language { get; }

        /// <summary>
        /// 初始化游戏平台
        /// </summary>
        /// <returns>初始化成功返回 true，失败抛出异常</returns>
        /// <exception cref="InvalidOperationException">当游戏平台初始化模式不正确或已经初始化时抛出</exception>
        /// <exception cref="SteamInitException">当游戏平台初始化失败时抛出</exception>
        public bool Initialize();

        /// <summary>
        /// 获取游戏平台的身份验证票据
        /// <br></br>
        /// <br></br>
        /// 当平台内部已经存在认证票据时，调用此方法将刷新票据并返回新的票据
        /// </summary>
        /// <returns>包装了认证票据数据的 NetworkAuthTicket 对象或 null</returns>
        public Task<NetworkAuthTicket> GetAuthTicket();


        /// <summary>
        /// 取消当前的身份验证票据
        /// <br></br>
        /// 外部的 NetworkAuthTicket 对象将不再有效，平台内部的认证票据也将被清除
        /// </summary>
        public void CancelAuthTicket();

        /// <summary>
        /// 由服务器调用，通知平台结束指定客户端的会话
        /// </summary>
        /// <param name="steamId">需要结束会话的 PlatformUserId</param>
        public void EndSession(PlatformUserId steamId);

        public void RunCallbacks();
    }
}
