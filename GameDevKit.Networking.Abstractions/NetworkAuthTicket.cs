// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

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
