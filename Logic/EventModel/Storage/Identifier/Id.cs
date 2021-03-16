using System;
using Newtonsoft.Json;

namespace maxbl4.Race.Logic.EventModel.Storage.Identifier
{
    [JsonConverter(typeof(IdJsonConverter))]
    public struct Id<T> : IEquatable<Id<T>>, IGuidValue, IComparable<Id<T>>, IComparable, IFormattable
    {
        #region Equality && Relational

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

        public int CompareTo(Id<T> other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            return obj is Id<T> other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Id<T>)}");
        }

        public static bool operator <(Id<T> left, Id<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Id<T> left, Id<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Id<T> left, Id<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Id<T> left, Id<T> right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion
       
        public static Id<T> NewId()
        {
            return new(SequentialGuid.SequentialGuidGenerator.Instance.NewGuid());
        }

        public static readonly Id<T> Empty = new(Guid.Empty);
            
        public Id(Guid value)
        {
            Value = value;
        }
            
        public Guid Value { get; }
            
        public static implicit operator Guid(Id<T> id)
        {
            return id.Value;
        }
        
        public static implicit operator LiteDB.BsonValue(Id<T> id)
        {
            return id.ToString();
        }
            
        public static implicit operator Id<T>(Guid id)
        {
            return new(id);
        }

        public override string ToString()
        {
            return Value.ToString("N");
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return Value.ToString(format, provider);
        }
    }
}