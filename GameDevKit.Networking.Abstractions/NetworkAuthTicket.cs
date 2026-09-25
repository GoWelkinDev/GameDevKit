// SPDX-License-Identifier: MPL-2.0
using MessagePack;

namespace GameDevKit.Networking.Abstractions
{
    [MessagePackObject]
    public class NetworkAuthTicket
    {
        [Key(0)] public byte[] Data { get; set; }
        [Key(1)] public uint Handle { get; set; }

        public NetworkAuthTicket(byte[] data, uint handle)
        {
            Data = data;
            Handle = handle;
        }
    }
}
