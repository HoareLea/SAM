using System;

namespace SAM.Core
{
    public class Connection<T, O>
    {
        private T @object;
        private Func<T, O, O> func;

        public Connection(T @object, O value)
        {
            this.@object = @object;
            func = new Func<T, O, O>((x, y) => value);
        }

        public Connection(T @object, Func<T, O, O> func)
        {
            this.@object = @object;
            this.func = new Func<T, O, O>(func);
        }

        public T Object
        {
            get
            {
                return @object;
            }
        }

        public O GetValue(O value)
        {
            return func.Invoke(@object, value);
        }
    }
}
