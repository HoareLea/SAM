using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class MaterialLibrary : SAMLibrary
    {
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

        public MaterialLibrary(MaterialLibrary materialLibrary)
            : base(materialLibrary)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }

        public override string GetUniqueId(IJSAMObject jSAMObject)
        {
            IMaterial material = jSAMObject as IMaterial;
            if (material == null)
                return null;

            return material.Name;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is IMaterial;
        }

        public List<IMaterial> GetMaterials()
        {
            return GetObjects<IMaterial>();
        }

        public IMaterial GetMaterial(string name)
        {
            if(name == null)
            {
                return null;
            }

            List<IMaterial> materials = GetObjects<IMaterial>();
            if (materials == null)
            {
                return null;
            }

            return materials.Find(x => x.Name == name);
        }
    }
}