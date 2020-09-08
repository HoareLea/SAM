using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class MaterialLibrary : SAMObject, IMaterial
    {
        List<IMaterial> materials;
        
        public MaterialLibrary(string name)
            : base(name)
        {

        }

        public MaterialLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public MaterialLibrary(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(materials != null && materials.Count != 0)
                jObject.Add("Materials", Create.JArray(materials.Cast<IJSAMObject>()));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (jObject.ContainsKey("Materials"))
                materials = Create.IJSAMObjects<IJSAMObject>(jObject.Value<JArray>("Materials"))?.Cast<IMaterial>().ToList();

            return jObject;
        }

        public IMaterial GetMaterial(string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(text) || materials == null || materials.Count == 0)
                return null;

            foreach(IMaterial material in materials)
            {
                if (Query.Compare(material.Name, text, textComparisonType, caseSensitive))
                    return material.Clone();
            }

            return null;
        }

        public List<IMaterial> GetMaterials(string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(text) || materials == null || materials.Count == 0)
                return null;

            List<IMaterial> result = new List<IMaterial>();
            foreach (IMaterial material in materials)
            {
                if (Query.Compare(material.Name, text, textComparisonType, caseSensitive))
                    result.Add(material.Clone());
            }

            return null;
        }

        public List<T> GetMaterials<T>() where T: IMaterial
        {
            if (materials == null || materials.Count == 0)
                return null;

            List<T> result = new List<T>();
            foreach (IMaterial material in materials)
            {
                if (material is T)
                    result.Add((T)material.Clone());
            }

            return null;
        }

        public bool Update(IMaterial material)
        {
            if (material == null || materials == null || materials.Count == 0)
                return false;

            int index = materials.FindIndex(x => x.Name == material.Name);
            if (index == -1)
                return false;

            materials[index] = material;
            return true;
        }

        public bool Add(IMaterial material)
        {
            if (material == null)
                return false;

            if (materials == null)
                materials = new List<IMaterial>();

            int index = materials.FindIndex(x => x.Name == material.Name);
            if (index == -1)
                materials.Add(material);
            else
                materials[index] = material;

            return true;
        }
    }
}