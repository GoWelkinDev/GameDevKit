// SPDX-License-Identifier: MPL-2.0
using System.ComponentModel;

namespace GameDevKit.Platforms.Abstractions
{
    public enum PlatformType
    {
        [Description("Standalone")]
        NONE,
        [Description("Steam")]
        STEAM
    }
}
