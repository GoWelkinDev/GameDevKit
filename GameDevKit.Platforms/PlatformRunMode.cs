// SPDX-License-Identifier: MPL-2.0
using System.ComponentModel;

namespace GameDevKit.Platforms.Abstractions
{
    public enum PlatformRunMode
    {
        [Description("Client")]
        CLIENT,
        [Description("Server")]
        SERVER
    }
}
