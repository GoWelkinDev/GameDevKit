// SPDX-License-Identifier: MPL-2.0
#nullable disable
namespace GameDevKit.Core
{
    public class ObjectId
    {
        private Guid guid { get; }

        public ObjectId()
        {
            guid = Guid.NewGuid();
        }

        public ObjectId(string str)
        {
            guid = new Guid(str);
        }

        public ObjectId(Guid guid)
        {
            this.guid = guid;
        }

        public static bool operator ==(ObjectId id1, ObjectId id2)
        {
            if (id1 is null)
            {
                if (id2 is null)
                {
                    return true;
                }
                return false;
            }
            return id1.Equals(id2);
        }

        public static bool operator !=(ObjectId id1, ObjectId id2)
        {
            return !(id1 == id2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ObjectId)obj);
        }

        public bool Equals(ObjectId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return guid.Equals(other.guid);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override string ToString()
        {
            return guid.ToString();
        }

        public int CompareTo(ObjectId other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return guid.CompareTo(other.guid);
        }

        public Guid ToGuid() => guid;
        public static ObjectId FromGuid(Guid g) => new ObjectId(g);
    }
}
