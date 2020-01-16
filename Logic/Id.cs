using System;

namespace maxbl4.Race.Logic
{
    public interface IHasIdValue
    {
        public Guid Value { get; }
    }
    
    public struct Id<T> : IEquatable<Id<T>>, IHasIdValue
    {
        #region Equality

        public bool Equals(Id<T> other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is Id<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Id<T> left, Id<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Id<T> left, Id<T> right)
        {
            return !left.Equals(right);
        }

        #endregion

        public static Id<T> NewId()
        {
            return new Id<T>(Guid.NewGuid());
        }
            
        public Id(Guid value)
        {
            Value = value;
        }
            
        public Guid Value { get; }

        public bool IsEmpty => Value == Guid.Empty;
            
        public static implicit operator Guid(Id<T> id)
        {
            return id.Value;
        }
            
        public static implicit operator Id<T>(Guid id)
        {
            return new Id<T>(id);
        }
    }
    
    public struct Lid<T> : IEquatable<Lid<T>>
    {
        #region Equality

        public bool Equals(Lid<T> other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is Lid<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Lid<T> left, Lid<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Lid<T> left, Lid<T> right)
        {
            return !left.Equals(right);
        }

        #endregion

        public static Lid<T> NewId()
        {
            return new Lid<T>(Ulid.NewUlid());
        }
            
        public Lid(Ulid value)
        {
            Value = value;
        }
            
        public Ulid Value { get; }

        public bool IsEmpty => Value == Ulid.Empty;
            
        public static implicit operator Ulid(Lid<T> id)
        {
            return id.Value;
        }
            
        public static implicit operator Lid<T>(Ulid id)
        {
            return new Lid<T>(id);
        }
    }
}