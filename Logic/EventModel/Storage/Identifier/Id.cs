using System;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using SequentialGuid;

namespace maxbl4.Race.Logic.EventModel.Storage.Identifier
{
    [ModelBinder(BinderType = typeof(IdBinder))]
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
            return obj is Id<T> other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Id<T>)}");
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
            return new(SequentialGuidGenerator.Instance.NewGuid());
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

        public static implicit operator BsonValue(Id<T> id)
        {
            return id.ToString();
        }

        public static implicit operator Id<T>(Guid id)
        {
            return new(id);
        }
        
        public static implicit operator Id<T>(Guid? id)
        {
            return new(id ?? Guid.Empty);
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
    
    public class IdBinder : IModelBinder
    {
        public IdBinder()
        {
            
        }
        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (!Guid.TryParse(value, out var id))
            {
                // Non-integer arguments result in model state errors
                bindingContext.ModelState.TryAddModelError(
                    modelName, "Id must be Guid.");

                return Task.CompletedTask;
            }

            // object model;
            // if (bindingContext.ModelType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //     model = new Nullable<Id<EventDto>>();

            var model = Activator.CreateInstance(bindingContext.ModelType, id);
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
    
    public class IdBinderProvider : IModelBinderProvider
    {
        public IdBinderProvider()
        {
            
        }
        
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.ModelType.IsConstructedGenericType) return null;
            if (context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Id<>))
                return new BinderTypeModelBinder(typeof(IdBinder));
            
            if (context.Metadata.ModelType.GetGenericTypeDefinition() != typeof(Nullable<>))
                return null;
            var nullableArg = context.Metadata.ModelType.GetGenericArguments()[0];
            if (nullableArg.IsConstructedGenericType && nullableArg.GetGenericTypeDefinition() == typeof(Id<>))
                return new BinderTypeModelBinder(typeof(IdBinder));

            return null;
        }
    }
}