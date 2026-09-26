// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

using LiteNetLib;
using System.Net;
using System.Net.Sockets;

namespace GameDevKit.Networking.Transport
{
    public class NetworkManager : INetEventListener
    {
        private readonly NetManager _netManager;
        private readonly Dictionary<int, NetPeer> _peers = new();
        private readonly int _maxConnections;

        public event Action<int>? PeerConnected;
        public event Action<int, DisconnectInfo>? PeerDisconnected;
        public event Action<int, byte[]>? DataReceived;

        public bool IsRunning { get; private set; }
        public int MaxConnections { get; }

        public NetworkType NetworkType { get; private set; }

        public NetworkManager(int maxConnections = 2048)
        {
            _maxConnections = maxConnections;
            MaxConnections = maxConnections;
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                UnsyncedEvents = false,
                UnsyncedReceiveEvent = false,
                UnconnectedMessagesEnabled = false
            };
        }

        public bool StartServer(int port)
        {
            if (!IsRunning)
            {
                _netManager.Start(port);
                IsRunning = true;
                NetworkType = NetworkType.Server;
                return true;
            }
            return false;
        }

        public bool StartClient()
        {
            if (!IsRunning)
            {
                _netManager.Start();
                IsRunning = true;
                NetworkType = NetworkType.Client;
                return true;
            }
            return false;
        }

        public NetPeer Connect(string ip, int port)
        {
            return _netManager.Connect(ip, port, string.Empty);
        }

        public void Stop()
        {
            _netManager?.Stop();
            IsRunning = false;
            _peers.Clear();
        }

        public void PollEvents()
        {
            _netManager?.PollEvents();
        }

        public void SendToPeer(int peerId, byte[] data, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
        {
            if (_peers.TryGetValue(peerId, out var peer))
                peer.Send(data, method);
        }

        public void SendToAll(byte[] data, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
        {
            if (NetworkType == NetworkType.Server)
                _netManager.SendToAll(data, method);
        }

        public void DisconnectPeer(int peerId)
        {
            if (_peers.TryGetValue(peerId, out var peer))
                peer.Disconnect();
        }

        public NetPeer? GetPeer(int peerId) => _peers.GetValueOrDefault(peerId);

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            if (_peers.Count >= _maxConnections)
            {
                request.Reject();
                return;
            }
            request.Accept();
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            _peers[peer.Id] = peer;
            PeerConnected?.Invoke(peer.Id);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _peers.Remove(peer.Id);
            PeerDisconnected?.Invoke(peer.Id, disconnectInfo);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            byte[] data = reader.GetRemainingBytes();
            DataReceived?.Invoke(peer.Id, data);
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    }
}