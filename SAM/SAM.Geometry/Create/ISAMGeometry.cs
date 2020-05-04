using Newtonsoft.Json.Linq;

namespace SAM.Geometry
{
    public static partial class Create
    {
        public static ISAMGeometry ISAMGeometry(this JObject jObject)
        {
            return Core.Create.IJSAMObject(jObject) as ISAMGeometry;

            //string typeName = Core.Query.TypeName(jObject);
            //if (string.IsNullOrWhiteSpace(typeName))
            //    return null;

            //Type type = Type.GetType(typeName);
            //if (type == null)
            //    return null;

            //ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            //if (constructorInfo == null)
            //    return null;

            //return constructorInfo.Invoke(new object[] { jObject }) as ISAMGeometry;
        }

        public static T ISAMGeometry<T>(this JObject jObject) where T : ISAMGeometry
        {
            return Core.Create.IJSAMObject<T>(jObject);

            //string typeName = Core.Query.TypeName(jObject);
            //if (string.IsNullOrWhiteSpace(typeName))
            //    return default;

            //Type type = Type.GetType(typeName);
            //if (type == null)
            //    return default;

            //ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            //if (constructorInfo == null)
            //    return default;

            //return (T)constructorInfo.Invoke(new object[] { jObject });
        }
    }
}