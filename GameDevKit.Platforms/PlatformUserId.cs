// SPDX-License-Identifier: MPL-2.0
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
