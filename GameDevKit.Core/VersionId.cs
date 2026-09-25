// SPDX-License-Identifier: MPL-2.0
#nullable disable
namespace GameDevKit.Core
{
    public class VersionId(int major, int minor, int patch) : IComparable<VersionId>, IEquatable<VersionId>
    {
        public int Major { get; } = major;
        public int Minor { get; } = minor;
        public int Patch { get; } = patch;

        /// <summary>
        /// 从字符串解析版本号，格式 "major.minor.patch"，支持缺失部分补0
        /// </summary>
        public static VersionId Parse(string version)
        {
            if (string.IsNullOrEmpty(version))
                return new VersionId(0, 0, 0);

            var parts = version.Split('.');
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            return new VersionId(major, minor, patch);
        }

        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        public int CompareTo(VersionId other)
        {
            if (other == null) return 1;
            if (Major != other.Major) return Major.CompareTo(other.Major);
            if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
            return Patch.CompareTo(other.Patch);
        }

        public bool Equals(VersionId other) => CompareTo(other) == 0;
        public override bool Equals(object obj) => obj is VersionId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);

        public static bool operator <(VersionId a, VersionId b) => a.CompareTo(b) < 0;
        public static bool operator >(VersionId a, VersionId b) => a.CompareTo(b) > 0;
        public static bool operator <=(VersionId a, VersionId b) => a.CompareTo(b) <= 0;
        public static bool operator >=(VersionId a, VersionId b) => a.CompareTo(b) >= 0;
        public static bool operator ==(VersionId a, VersionId b) => Equals(a, b);
        public static bool operator !=(VersionId a, VersionId b) => !Equals(a, b);
    }
}
