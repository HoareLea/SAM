using Newtonsoft.Json.Linq;
using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class TypeMap : SAMObject, IJSAMObject
    {
        private Dictionary<string, Type> dictionary;

        /// <summary>
        /// list of tuples where Item1 - Id_1, Item2 - Id_2, Item3 - name_1, Item4 - name_2, Item5 - formula_1, Item6 - formula_2
        /// </summary>
        private List<Tuple<string, string, string, string, string, string>> tuples;

        internal TypeMap(TypeMap typeMap)
            :base(typeMap)
        {
            tuples = typeMap?.tuples?.ConvertAll(x => new Tuple<string, string, string, string, string, string>(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));
        }
        
        internal TypeMap()
        {
            tuples = new List<Tuple<string, string, string, string, string, string>>();
        }

        internal TypeMap(JObject jObject)
            : base(jObject)
        {

        }

        public bool Add(Type type_1, Type type_2, string name_1, string name_2, string formula_1 = null, string formula_2 = null)
        {
            if (type_1 == null || type_2 == null || string.IsNullOrEmpty(name_1) || string.IsNullOrEmpty(name_2))
                return false;

            bool result = Add(GetId(type_1), GetId(type_2), name_1, name_2, formula_1, formula_2);
            if(result)
            {
                AddType(type_1);
                AddType(type_2);
            }
            return result;
        }

        public List<Type> Add(Enum @enum, Type type, string name, string formula_1 = null, string formula_2 = null)
        {
            if (type == null || string.IsNullOrEmpty(name))
                return null;

            AssociatedTypes associatedTypes = AssociatedTypes.Get(@enum.GetType());
            if (associatedTypes == null)
                return null;

            Type[] types = associatedTypes.Types;
            if (types == null || types.Length == 0)
                return null;

            string name_Temp = @enum.ToString();
            ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
            if(parameterProperties != null)
                name_Temp = parameterProperties.Name;

            if (name_Temp == null)
                return null;

            List<Type> result = new List<Type>();
            foreach (Type type_Temp in types)
                if (Add(type_Temp, type, name_Temp, name, formula_1, formula_2))
                    result.Add(type_Temp);

            return result;
        }
        
        public bool Add(string id_1, string id_2, string name_1, string name_2, string formula_1 = null, string formula_2 = null)
        {
            if (string.IsNullOrEmpty(id_1) || string.IsNullOrEmpty(id_2) || string.IsNullOrEmpty(name_1) || string.IsNullOrEmpty(name_2))
                return false;

            List<Tuple<string, string, string, string, string, string>> tuples_Temp = tuples.FindAll(x => x.Item1.Equals(id_1));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string, string, string>(id_1, id_2, name_1, name_2, formula_1, formula_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item2.Equals(id_2));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string, string, string>(id_1, id_2, name_1, name_2, formula_1, formula_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item3.Equals(name_1));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string, string, string>(id_1, id_2, name_1, name_2, formula_1, formula_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item4.Equals(name_2));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string, string, string>(id_1, id_2, name_1, name_2, formula_1, formula_2));
                return true;
            }

            return false;
        }

        public string GetName(string id_1, string id_2, string name_1)
        {
            return GetName(id_1, id_2, name_1, 2);
        }

        public string GetName(string id_1, string id_2, string name, int index)
        {
            if (string.IsNullOrEmpty(id_1) || string.IsNullOrEmpty(id_2) || string.IsNullOrEmpty(name))
                return null;

            return GetNames(id_1, id_2, name, index)?.FirstOrDefault();
        }

        public string GetName(Type type_1, Type type_2, string name_1)
        {
            return GetName(GetId(type_1), GetId(type_2), name_1, 2);
        }

        public string GetName(Type type_1, Type type_2, string name, int index)
        {
            if (type_1 == null || type_2 == null || string.IsNullOrEmpty(name))
                return null;

            return GetName(GetId(type_1), GetId(type_2), name, index);
        }

        public string GetName(Type type_1, Type type_2, Enum @enum)
        {
            return GetName(type_1, type_2, @enum.Name());
        }

        public List<string> GetNames(Type type_1, Type type_2)
        {
            if (type_1 == null || type_2 == null)
                return null;

            return GetNames(GetId(type_1), GetId(type_2));
        }

        public List<string> GetNames(Type type_1, Type type_2, string name)
        {
            return GetNames(GetId(type_1), GetId(type_2), name, 2);
        }

        public List<string> GetFormulas(Type type_1, Type type_2, string name)
        {
            return GetFormulas(GetId(type_1), GetId(type_2), name, 2);
        }

        public List<string> GetNames(Type type_1, Type type_2, int index)
        {
            if (type_1 == null || type_2 == null)
                return null;

            return GetNames(GetId(type_1), GetId(type_2), index);
        }

        public List<string> GetNames(string id_1, string id_2)
        {
            return GetNames(id_1, id_2, null, 1);
        }

        public List<string> GetNames(string id_1, string id_2, int index)
        {
            return GetNames(id_1, id_2, null, index);
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray_Map = new JArray();
            foreach (Tuple<string, string, string, string, string, string> tuple in tuples)
            {
                JArray jArray = new JArray();
                jArray.Add(tuple.Item1);
                jArray.Add(tuple.Item2);
                jArray.Add(tuple.Item3);
                jArray.Add(tuple.Item4);
                jArray_Map.Add(jArray);
            }

            jObject.Add("Map", jArray_Map);

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            tuples = new List<Tuple<string, string, string, string, string, string>>();

            JArray jArray_Map = jObject.Value<JArray>("Map");
            if(jArray_Map != null)
            {
                
                foreach(JArray jArray in jArray_Map)
                {
                    if (jArray.Count < 4)
                        continue;

                    string formula_1 = null;
                    string formula_2 = null;
                    if (jArray.Count > 4)
                    {
                        formula_1 = jArray[4].Value<string>();
                        if (jArray.Count > 5)
                            formula_2 = jArray[5].Value<string>();
                    }

                    Tuple<string, string, string, string, string, string> tuple = new Tuple<string, string, string, string, string, string>(jArray[0].Value<string>(), jArray[1].Value<string>(), jArray[2].Value<string>(), jArray[3].Value<string>(), formula_1, formula_2);

                    if (tuple != null)
                        tuples.Add(tuple);
                }

 
            }

            return true;
        }


        private List<int> GetIndexes(string id_1, string id_2, string name, int index)
        {
            if (string.IsNullOrWhiteSpace(id_1) || string.IsNullOrWhiteSpace(id_2))
                return null;

            Type type_1 = GetType(id_1, false);
            Type type_2 = GetType(id_2, false);

            List<int> result = new List<int>();
            for (int i = 0; i < tuples.Count; i++)
            {
                Tuple<string, string, string, string, string, string> tuple = tuples[i];

                bool valid;

                valid = false;
                if (type_1 != null)
                {
                    Type type_1_Tuple = GetType(tuple.Item1);
                    if (type_1_Tuple != null)
                        valid = type_1.Equals(type_1_Tuple) || type_1_Tuple.IsAssignableFrom(type_1);
                }

                if (!valid)
                    valid = tuple.Item1.Equals(id_1);

                if (!valid)
                    continue;

                valid = false;
                if (type_2 != null)
                {
                    Type type_2_Tuple = GetType(tuple.Item2);
                    if (type_2_Tuple != null)
                        valid = type_2.Equals(type_2_Tuple) || type_2_Tuple.IsAssignableFrom(type_2);
                }

                if (!valid)
                    valid = tuple.Item2.Equals(id_2);

                if (!valid)
                    continue;

                if (name == null)
                {
                    result.Add(i);
                    continue;
                }

                string name_Input = index == 1 ? tuple.Item4 : tuple.Item3;
                if (!name_Input.Equals(name))
                    continue;

                result.Add(i);
            }

            return result;
        }

        private List<string> GetNames(string id_1, string id_2, string name, int index)
        {
            List<int> indexes = GetIndexes(id_1, id_2, name, index);
            if (indexes == null)
                return null;

            List<string> result = new List<string>();
            foreach(int i in indexes)
                result.Add(index == 1 ? tuples[i].Item3 : tuples[i].Item4);

            return result;
        }

        private List<string> GetFormulas(string id_1, string id_2, string name, int index)
        {
            List<int> indexes = GetIndexes(id_1, id_2, name, index);
            if (indexes == null)
                return null;

            List<string> result = new List<string>();
            foreach (int i in indexes)
                result.Add(index == 1 ? tuples[i].Item5 : tuples[i].Item6);

            return result;
        }

        private bool AddType(Type type)
        {
            string id = GetId(type);
            if (string.IsNullOrEmpty(id) || !id.StartsWith("::"))
                return false;

            if (dictionary == null)
                dictionary = new Dictionary<string, Type>();

            dictionary[id] = type;
            return true;
        }

        private Type GetType(string id, bool includeInDictionary = true)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (!id.StartsWith("::"))
                return null;

            Type result = null;
            if (dictionary != null)
                if (!dictionary.TryGetValue(id, out result))
                    result = null;

            if (result != null)
                return result;

            result = Type.GetType(id.Substring(2), false);

            if(includeInDictionary)
            {
                if (dictionary == null)
                    dictionary = new Dictionary<string, Type>();

                dictionary[id] = result;
            }

            return result;
        }

        private static string GetId(Type type)
        {
            string fullName = Query.FullTypeName(type);
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = type.FullName;

            return string.Format("::{0}", fullName);
        }
    }
}
