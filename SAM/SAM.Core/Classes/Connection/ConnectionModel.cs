using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class ConnectionModel<T, O>
    {
        private List<Connection<T, O>> connections;

        public ConnectionModel(T @object, O value)
        {
            connections = new List<Connection<T, O>>();
            connections.Add(new Connection<T, O>(@object, value));
        }

        public void Add(T @object, Func<T, O, O> func)
        {
            connections.Add(new Connection<T, O>(@object, func));
        }

        public Dictionary<T, O> GetValueDictionary()
        {
            Dictionary<T, O> result = new Dictionary<T, O>();

            O value = default;
            foreach (Connection<T, O> connection in connections)
            {
                value = connection.GetValue(value);
                result[connection.Object] = value;
            }

            return result;
        }

        public IEnumerable<O> GetValues()
        {
            return GetValueDictionary()?.Values;
        }
    }
}
