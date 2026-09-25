// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

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
