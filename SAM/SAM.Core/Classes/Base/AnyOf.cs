using System;
using System.Linq;

namespace SAM.Core
{
    public class AnyOf
    {
        private object value;
        
        public virtual Type[] Types => new Type[] { typeof(object) };

        public AnyOf(object value, Type type = null)
        {
            if (type == null)
            {
                if(IsValid(value))
                    this.value = value;

                return;
            }
            
            if(value == null || !value.GetType().IsAssignableFrom(type))
            {
                this.value = Query.IsNullable(type) ? null : Activator.CreateInstance(type);
                return;
            }

            this.value = value;
        }
        
        public object Value
        {
            get
            {
                return value;
            }
            set
            {
                if (IsValid(value))
                    this.value = value;
            }
        }

        public T GetValue<T>()
        {
            if(value is T)
            {
                return (T)(object)value;
            }

            return default;
        }

        public bool IsValid(object value)
        {
            if (Types == null || Types.Length == 0)
                return false;

            if (value == null)
            {
                foreach(Type type in Types)
                {
                    if (Query.IsNullable(type))
                        return true;
                }

                return false;
            }

            return Types.Contains(value?.GetType());
        }

        public override string ToString()
        {
            return value?.ToString();
        }

        public override bool Equals(object @object)
        {
            if (ReferenceEquals(this, null))
                return ReferenceEquals(@object, null) ? true : false;

            return value.Equals(@object);
        }

        public override int GetHashCode()
        {
            if (value == null)
                return 0;

            return value.GetHashCode();
        }

        public new Type GetType()
        {
            if (value == null)
                return Types?.First();

            return value.GetType();
        }

        public static bool operator ==(AnyOf anyOf, object @object)
        {
            if (ReferenceEquals(anyOf, null) || ReferenceEquals(anyOf.value, null))
                return ReferenceEquals(@object, null) ? true : false;

            return anyOf.value.Equals(@object);
        }

        public static bool operator !=(AnyOf anyOf, object @object)
        {
            return !(anyOf == @object);
        }

        public static implicit operator string(AnyOf value) => value?.ToString();
        public static implicit operator AnyOf(int value) => new AnyOf(value, typeof(int));
        public static implicit operator AnyOf(string value) => new AnyOf(value, typeof(string));
        public static implicit operator AnyOf(double value) => new AnyOf(value, typeof(double));
        public static implicit operator AnyOf(Guid value) => new AnyOf(value, typeof(Guid));
        public static implicit operator AnyOf(DateTime value) => new AnyOf(value, typeof(DateTime));
        public static implicit operator AnyOf(long value) => new AnyOf(value, typeof(long));
        public static implicit operator AnyOf(bool value) => new AnyOf(value, typeof(bool));
    }

    public class AnyOf<T> : AnyOf
    {
        public override Type[] Types => new Type[] { typeof(T) };

        public AnyOf(object value) 
            : base(value)
        {
        }

        public static implicit operator string(AnyOf<T> b) => b?.ToString();

    }

    public class AnyOf<T, K> : AnyOf
    {
        public override Type[] Types => new Type[] { typeof(T), typeof(K) };

        public AnyOf(object value) 
            : base(value)
        {
        }

        public static implicit operator string(AnyOf<T, K> value) => value?.ToString();
        public static implicit operator AnyOf<T, K>(T value) => new AnyOf<T, K>(value);
        public static implicit operator AnyOf<T, K>(K value) => new AnyOf<T, K>(value);
    }
}
