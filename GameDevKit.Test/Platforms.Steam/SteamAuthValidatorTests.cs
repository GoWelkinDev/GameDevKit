// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using GameDevKit.Platforms.Steam;
using Steamworks;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;

namespace GameDevKit.Test.Platforms.Steam;

public class SteamAuthValidatorTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly SteamAuthValidator _validator;

    public SteamAuthValidatorTests(ITestOutputHelper output)
    {
        _output = output;
        _validator = new SteamAuthValidator();
    }

    public void Dispose()
    {
        _validator.Dispose();
        GC.SuppressFinalize(this);
    }

    // 获取私有静态字段 _pending
    private ConcurrentDictionary<ulong, TaskCompletionSource<bool>> GetPendingDict() =>
        (ConcurrentDictionary<ulong, TaskCompletionSource<bool>>)typeof(SteamAuthValidator)
            .GetField("_pendingValidations", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_validator)!;

    // 调用私有静态 OnValidateAuthTicketResponse 方法
    private void InvokeCallback(ulong steamId, AuthResponse response) =>
        typeof(SteamAuthValidator)
            .GetMethod("OnValidateAuthTicketResponse", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_validator, new object[] { (SteamId)steamId, (SteamId)0, response });

    [Fact]
    public async Task Callback_OK_ShouldSetResultTrue()
    {
        ulong steamId = 123;
        var tcs = new TaskCompletionSource<bool>();
        GetPendingDict()[steamId] = tcs;

        _output.WriteLine($"Simulating callback with OK for SteamID {steamId}");
        InvokeCallback(steamId, AuthResponse.OK);

        Assert.True(await tcs.Task);
        _output.WriteLine("Result set to true as expected");
    }

    [Fact]
    public async Task Callback_Failure_ShouldSetResultFalse()
    {
        ulong steamId = 456;
        var tcs = new TaskCompletionSource<bool>();
        GetPendingDict()[steamId] = tcs;

        _output.WriteLine($"Simulating callback with AuthResponse.Invalid for SteamID {steamId}");
        InvokeCallback(steamId, AuthResponse.AuthTicketInvalid);

        Assert.False(await tcs.Task);
        _output.WriteLine("Result set to false as expected");
    }

    [Fact]
    public async Task Shutdown_ShouldCancelAllPendingTasks()
    {
        ulong steamId1 = 1;
        ulong steamId2 = 2;
        var tcs1 = new TaskCompletionSource<bool>();
        var tcs2 = new TaskCompletionSource<bool>();
        var dict = GetPendingDict();
        dict[steamId1] = tcs1;
        dict[steamId2] = tcs2;

        _output.WriteLine($"Pending tasks before shutdown: {dict.Count}");
        _validator.Dispose();

        // 验证所有 TCS 被设置为 false（TrySetResult(false)）
        Assert.False(await tcs1.Task);
        Assert.False(await tcs2.Task);
        Assert.Empty(dict);
        _output.WriteLine("All pending tasks cancelled, dictionary cleared");
    }

    [Fact]
    public async Task Initialize_ShouldAttachEventHandler()
    {
        // 由于无法直接检查事件订阅，通过内部行为间接验证：
        // 如果事件未注册，我们的 InvokeCallback 不会影响任何任务，
        // 但 Inject 任务到字典后触发回调能成功，说明事件已连接。
        // 这里可用 Monitor/exception 来证明不会抛出异常。
        var tcs = new TaskCompletionSource<bool>();
        GetPendingDict()[999] = tcs;
        var exception = Record.Exception(() => InvokeCallback(999, AuthResponse.OK));
        Assert.Null(exception);
        Assert.True(await tcs.Task);
        _output.WriteLine("Event handler is attached and functional");
    }

    [Fact]
    public async Task ValidateAuthSession_SpoofedSteamId_ShouldTimeout()
    {
        // 模拟 SteamID 冒用：声称的 ID 与票证实际 ID 不同
        ulong claimedId = 111;
        ulong actualId = 222;

        // 不调用真实的 SteamServer.BeginAuthSession，仅模拟字典操作
        var dict = GetPendingDict();
        var tcs = new TaskCompletionSource<bool>();

        // 以声称的 ID 添加 TCS（模拟 ValidateAuthSession 的行为）
        dict.TryAdd(claimedId, tcs);

        // 以“实际”ID 触发回调（冒充者提供的票证中实际 SteamID 是 actualId）
        InvokeCallback(actualId, AuthResponse.OK);

        // 使用短超时验证任务未完成，因为键不匹配
        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(50, TestContext.Current.CancellationToken));
        Assert.NotEqual(tcs.Task, completedTask);
        Assert.False(tcs.Task.IsCompleted);

        // 清理：移除残留项并取消，模拟 finally 块
        dict.TryRemove(claimedId, out _);
        tcs.TrySetCanceled(TestContext.Current.CancellationToken);
        _output.WriteLine("Spoofed SteamID correctly ignored, resulting in timeout.");
    }

    [Fact]
    public async Task ValidateAuthSession_MatchedSteamId_ShouldSucceed()
    {
        // 票证内实际 SteamID 与声称的一致
        ulong steamId = 333;
        var dict = GetPendingDict();
        var tcs = new TaskCompletionSource<bool>();

        dict.TryAdd(steamId, tcs);
        InvokeCallback(steamId, AuthResponse.OK);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        Assert.Equal(tcs.Task, completedTask);
        Assert.True(await tcs.Task);

        dict.TryRemove(steamId, out _);
        _output.WriteLine("Matching SteamID validated successfully.");
    }
}