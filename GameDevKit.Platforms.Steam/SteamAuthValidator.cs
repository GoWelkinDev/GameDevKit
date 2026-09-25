// SPDX-License-Identifier: MPL-2.0
using GameDevKit.Logging.Abstractions;
using GameDevKit.Networking.Abstractions;
using Steamworks;
using System.Collections.Concurrent;

namespace GameDevKit.Platforms.Steam
{
    /// <summary>
    /// Steam 会话票证验证器，使用 Steamworks API 验证客户端的会话票证
    /// </summary>
    public sealed class SteamAuthValidator : IDisposable
    {
        // 存储等待验证的任务完成源，键为声称的 SteamID
        private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _pendingValidations = new();

        /// <summary>
        /// 注册票证验证回调
        /// </summary>
        public SteamAuthValidator()
        {
            SteamServer.OnValidateAuthTicketResponse += OnValidateAuthTicketResponse;
        }

        /// <summary>
        /// 验证 Steam 会话票证（异步等待回调结果）
        /// </summary>
        /// <param name="steamId">客户端声称的 steamId</param>
        /// <param name="authTicket">票证数据</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <returns>验证是否成功</returns>
        public async Task<bool> ValidateAuthSession(ulong steamId, NetworkAuthTicket authTicket, int timeoutSeconds = 15)
        {
            if (authTicket?.Data is null || authTicket.Data.Length == 0)
            {
                Log.PrintWarning($"[Steamworks API Verifcation] Empty ticket for SteamId={steamId}@steam.");
                return false;
            }

            SteamServer.BeginAuthSession(authTicket.Data, steamId);

            var tcs = new TaskCompletionSource<bool>();

            // 以声称的 SteamID 为键存储（回调时使用实际 SteamID，若冒用则无法匹配，导致超时）
            if (!_pendingValidations.TryAdd(steamId, tcs))
            {
                Log.PrintWarning($"[Steamworks API Verifcation] Duplicate validation request for SteamId={steamId}@steam.");
                return false;
            }

            try
            {
                // 等待回调或超时
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
                if (completedTask != tcs.Task)
                {
                    Log.PrintWarning($"[Steamworks API Verifcation] Timeout validating SteamId={steamId}@steam.");
                    SteamServer.EndSession(steamId);
                    return false;
                }
                bool result = await tcs.Task;

                if (result) Log.Print($"[Steamworks API Verifcation] SteamId={steamId}@steam validated.");
                else Log.PrintWarning($"[Steamworks API Verifcation] SteamId={steamId}@steam validation failed.");

                return result;
            }
            finally
            {
                _pendingValidations.TryRemove(steamId, out _);
            }
        }

        /// <summary>
        /// 取消正在等待的验证
        /// </summary>
        public void Cancel(ulong steamId)
        {
            if (_pendingValidations.TryRemove(steamId, out var tcs))
            {
                tcs.TrySetResult(false);
                Log.PrintWarning($"[SteamAuth] Cancelled validation for SteamId={steamId}.");
            }
        }

        private void OnValidateAuthTicketResponse(SteamId steamId, SteamId ownerId, AuthResponse response)
        {
#if DEBUG
            Log.Debug($"[Steamworks API Verifcation] Callback: SteamId={steamId}, Owner={ownerId}, Response={response}");
#endif

            // 以票证内实际的 SteamID 为键查找等待任务
            if (_pendingValidations.TryRemove(steamId, out var tcs))
            {
                bool isValid = response == AuthResponse.OK;
                tcs.TrySetResult(isValid);
            }
        }

        /// <summary>
        /// 关闭票证验证系统
        /// </summary>
        public void Dispose()
        {
            SteamServer.OnValidateAuthTicketResponse -= OnValidateAuthTicketResponse;

            // 取消所有等待中的任务
            foreach (var tcs in _pendingValidations.Values)
                tcs.TrySetResult(false);
            _pendingValidations.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
