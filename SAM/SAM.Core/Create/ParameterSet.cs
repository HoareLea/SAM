using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ParameterSet ParameterSet(this object @object, IEnumerable<string> names)
        {
            if (@object == null || names == null)
                return null;

            ParameterSet result = new ParameterSet(@object.GetType()?.Assembly);

            foreach(string name in names)
            {
                object value = null; 

                if (!Query.TryGetValue(@object, name, out value))
                    continue;

                result.Add(name, value as dynamic);
            }

            return result;
        }

        public static ParameterSet ParameterSet(this object @object, Type type_destination, MapCluster mapCluster)
        {
            if (@object == null || mapCluster == null)
                return null;

            Type type_source = @object.GetType();

            List<string> names = mapCluster.GetNames(type_source, type_destination.GetType());
            if (names == null)
                return null;

            ParameterSet result = new ParameterSet(type_source.Assembly);

            foreach (string name in names)
            {
                string name_destination = mapCluster.GetName(type_source, type_destination, name);
                if (string.IsNullOrWhiteSpace(name_destination))
                    continue;

                object value = null;

                if (!Query.TryGetValue(@object, name, out value))
                    continue;

                result.Add(name_destination, value as dynamic);
            }

            return result;
        }
    }
}