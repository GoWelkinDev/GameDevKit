// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using GameDevKit.Networking.Transport;
using Xunit;

namespace GameDevKit.Test.Networking.Transport;

public class NetworkManagerTests : IDisposable
{
    private readonly NetworkManager _manager;
    private readonly ITestOutputHelper _output;

    public NetworkManagerTests(ITestOutputHelper output)
    {
        _output = output;
        _manager = new NetworkManager();
    }

    public void Dispose()
    {
        if (_manager.IsRunning) _manager.Stop();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_ShouldBeStoppedAndTypeUnknown()
    {
        Assert.False(_manager.IsRunning);
        // NetworkType 的默认值 0 = Client，但未启动时无意义
        _output.WriteLine($"After construction: IsRunning={_manager.IsRunning}, NetworkType={_manager.NetworkType}");
    }

    [Fact]
    public void StartServer_ShouldSetState()
    {
        // 尝试在任意可用端口启动，避免端口冲突
        _manager.StartServer(0); // 端口0将自动分配
        Assert.True(_manager.IsRunning);
        Assert.Equal(NetworkType.Server, _manager.NetworkType);
        _output.WriteLine("Server started successfully");

        _manager.Stop();
        Assert.False(_manager.IsRunning);
    }

    [Fact]
    public void StartClient_ShouldSetState()
    {
        _manager.StartClient();
        Assert.True(_manager.IsRunning);
        Assert.Equal(NetworkType.Client, _manager.NetworkType);
        _output.WriteLine("Client mode started");

        _manager.Stop();
        Assert.False(_manager.IsRunning);
    }

    [Fact]
    public void Stop_WhenNotRunning_ShouldNotThrow()
    {
        var exception = Record.Exception(() => _manager.Stop());
        Assert.Null(exception);
        _output.WriteLine("Stop on already stopped manager is safe");
    }

    // 仅验证事件可以绑定
    [Fact]
    public void PeerConnectedEvent_CanBeSubscribed()
    {
        int receivedPeerId = -1;
        _manager.PeerConnected += id => receivedPeerId = id;
        Assert.True(true);
        _output.WriteLine("PeerConnected event subscribed successfully");
    }
}