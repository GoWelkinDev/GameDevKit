// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using GameDevKit.Logging.Abstractions;
using GameDevKit.Networking.Abstractions;
using GameDevKit.Platforms.Abstractions;
using Steamworks;
using System.Net;

namespace GameDevKit.Platforms.Steam
{
    public sealed class Steam : IGamePlatform, IDisposable
    {
        public string Name => nameof(Steam);
        public PlatformType PlatformType => PlatformType.STEAM;
        public uint AppId { get; }
        public PlatformRunMode RunMode { get; }
        public uint GamePort { get; }
        public uint QueryPort { get; }
        public bool IsOnline { get; private set; }
        public ulong UserId { get; private set; }
        public string Username { get; private set; }
        public string Language { get; private set; }
        public int BuildId { get; private set; }

        private AuthTicket _authTicket;

        /// <summary>
        /// 构造函数，初始化 Steam 平台
        /// </summary>
        /// <param name="platformRunMode">Steam 平台运行模式，客户端或服务器</param>
        /// <param name="appId">游戏的 AppId</param>
        /// <param name="gamePort">游戏服务器端口，默认为 21029，仅在服务器模式下使用</param>
        /// <param name="queryPort">查询服务器端口，默认为 21030，仅在服务器模式下使用</param>
        public Steam(PlatformRunMode platformRunMode, uint appId, uint gamePort = 21029, uint queryPort = 21030)
        {
            RunMode = platformRunMode;
            AppId = appId;
            GamePort = gamePort;
            QueryPort = queryPort;

            Environment.SetEnvironmentVariable("SteamAppId", AppId.ToString());
            Environment.SetEnvironmentVariable("SteamGameId", AppId.ToString());
        }


        /// <summary>
        /// 初始化 Steam 平台
        /// </summary>
        /// <returns>初始化成功返回 true，失败抛出异常</returns>
        /// <exception cref="InvalidOperationException">当 Steam 平台初始化模式不正确或已经初始化时抛出</exception>
        /// <exception cref="PlatformInitException">当 Steam 平台初始化失败时抛出</exception>
        public bool Initialize()
        {
            if (IsOnline) throw new InvalidOperationException("Already initialized");

            try
            {
                switch (RunMode)
                {
                    case PlatformRunMode.CLIENT:
                        {
                            SteamClient.Init(AppId, false);
                            break;
                        }
                    case PlatformRunMode.SERVER:
                        {
                            var init = new SteamServerInit()
                            {
                                DedicatedServer = true,
                                IpAddress = IPAddress.Any,
                                GamePort = (ushort)GamePort,
                                QueryPort = (ushort)QueryPort,
                                Secure = false,
                                VersionString = "1.0.0.0"
                            };

                            SteamServer.Init(AppId, init, false);
                            SteamServer.LogOnAnonymous();

                            break;
                        }
                    default:
                        throw new InvalidOperationException("Invalid Steam run mode.");
                }
            }
            catch (Exception e)
            {
                Log.PrintErr("SteamAPI Init failed! Is Steam running?\n", e);
                throw new PlatformInitException("Steam initialization failed.");
            }

            if (RunMode == PlatformRunMode.CLIENT)
            {
                IsOnline = SteamClient.IsLoggedOn;
                UserId = SteamClient.SteamId;
                Username = SteamClient.Name;
                Language = string.IsNullOrEmpty(SteamApps.GameLanguage) ? "schinese" : SteamApps.GameLanguage;
                BuildId = SteamApps.BuildId;

                if (!SteamClient.IsValid)
                {
                    Log.PrintErr("You don't seem to have purchased the game.");
                    IsOnline = false;
                    SteamClient.Shutdown();
                    throw new PlatformInitException("Game ownership check failed.");
                }
            }
            else
            {
                IsOnline = SteamServer.IsValid;

                if (!IsOnline)
                {
                    Log.PrintErr("Steam server is not valid after initialization.");
                    SteamServer.Shutdown();
                    throw new PlatformInitException("Steam server not valid.");
                }
            }

            return true;
        }

        /// <summary>
        /// 获取 Steam 网络认证票据
        /// 仅在客户端模式下有效，服务器模式下返回 null
        /// <br></br>
        /// <br></br>
        /// 当 Steam 平台内部已经存在认证票据时，调用此方法将刷新认证票据
        /// </summary>
        /// <returns>包装了认证票据数据的 NetworkAuthTicket 对象或 null</returns>
        public async Task<NetworkAuthTicket> GetAuthTicket()
        {
            if (RunMode == PlatformRunMode.CLIENT)
            {
                _authTicket = await SteamUser.GetAuthSessionTicketAsync();

                return new NetworkAuthTicket(_authTicket?.Data, _authTicket.Handle);
            }

            return null;
        }

        /// <summary>
        /// 取消 Steam 网络认证票据
        /// <br></br>
        /// 外部的 NetworkAuthTicket 对象在调用此方法后将失效
        /// </summary>
        public void CancelAuthTicket()
        {
            if (RunMode == PlatformRunMode.CLIENT)
            {
                _authTicket?.Cancel();
                SteamUser.EndAuthSession(UserId);
            }
        }

        /// <summary>
        /// 结束 Steam 会话，由服务器调用
        /// </summary>
        /// <param name="steamId">需要结束会话的 SteamId</param>
        public void EndSession(PlatformUserId steamId)
        {
            if (RunMode == PlatformRunMode.SERVER)
            {
                SteamServer.EndSession(steamId.Value);
            }
        }

        public void RunCallbacks()
        {
            if (RunMode == PlatformRunMode.CLIENT)
                SteamClient.RunCallbacks();
            else
                SteamServer.RunCallbacks();
        }

        public void Dispose()
        {
            if (RunMode == PlatformRunMode.CLIENT)
            {
                _authTicket?.Dispose();
                SteamClient.Shutdown();
            }
            else
            {
                SteamServer.Shutdown();
            }

            GC.SuppressFinalize(this);
        }
    }
}
