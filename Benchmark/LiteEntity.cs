using System;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;

namespace Benchmark
{
    public class LiteEntityId
    {
        public Id<LiteEntityId> Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityLid
    {
        public Lid<LiteEntityLid> Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityGuid
    {
        public Guid Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityUlid
    {
        public Ulid Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityInt
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public class LiteEntityLong
    {
        public long Id { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public int Amount { get; set; }
    }
    
    public interface IHasUlidValue
    {
        public Ulid Value { get; }
    }
    
    public struct Lid<T> : IEquatable<Lid<T>>, IHasUlidValue
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