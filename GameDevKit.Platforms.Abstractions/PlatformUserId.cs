// SPDX-FileCopyrightText: 2026 穹空网络(GoWelkin Network)
//
// SPDX-License-Identifier: EPL-2.0
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// https://www.eclipse.org/legal/epl-2.0/

namespace GameDevKit.Platforms.Abstractions
{
    public readonly struct PlatformUserId : IEquatable<PlatformUserId>
    {
        public ulong Value { get; }
        public PlatformUserId(ulong value) => Value = value;

        public bool Equals(PlatformUserId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is PlatformUserId u && Equals(u);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
