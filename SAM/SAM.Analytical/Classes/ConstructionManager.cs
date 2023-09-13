using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ConstructionManager : IJSAMObject, IAnalyticalObject
    {
        private ApertureConstructionLibrary apertureConstructionLibrary;
        private ConstructionLibrary constructionLibrary;
        
        private MaterialLibrary materialLibrary;

        public ConstructionManager(ApertureConstructionLibrary apertureConstructionLibrary, ConstructionLibrary constructionLibrary, MaterialLibrary materialLibrary)
        {
            this.apertureConstructionLibrary = apertureConstructionLibrary;
            this.constructionLibrary = constructionLibrary;
            this.materialLibrary = materialLibrary;
        }

        public ConstructionManager(JObject jObject)
        {
            FromJObject(jObject);
        }

        public ConstructionManager(ConstructionManager constructionManager)
        {
            if (constructionManager != null)
            {
                apertureConstructionLibrary = constructionManager.apertureConstructionLibrary?.Clone();
                constructionLibrary = constructionManager.constructionLibrary?.Clone();
                materialLibrary = constructionManager.materialLibrary?.Clone();
            }
        }

        public List<ApertureConstruction> ApertureConstructions
        {
            get
            {
                return apertureConstructionLibrary?.GetApertureConstructions().ConvertAll(x => x?.Clone());
            }
        }

        public List<Construction> Constructions
        {
            get
            {
                return constructionLibrary?.GetConstructions().ConvertAll(x => x?.Clone());
            }
        }

        public List<IMaterial> Materials
        {
            get
            {
                return materialLibrary?.GetMaterials().ConvertAll(x => x?.Clone());
            }
        }

        public bool Add(IMaterial material)
        {
            if(material == null)
            {
                return false;
            }

            if(materialLibrary == null)
            {
                materialLibrary = new MaterialLibrary(string.Empty);
            }

            return materialLibrary.Add(material);
        }

        public bool Add(ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
            {
                return false;
            }

            if (apertureConstructionLibrary == null)
            {
                apertureConstructionLibrary = new ApertureConstructionLibrary(string.Empty);
            }

            return apertureConstructionLibrary.Add(apertureConstruction);
        }

        public bool Add(Construction construction)
        {
            if (construction == null)
            {
                return false;
            }

            if (constructionLibrary == null)
            {
                constructionLibrary = new ConstructionLibrary(string.Empty);
            }

            return constructionLibrary.Add(construction);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("ApertureConstructionLibrary"))
            {
                apertureConstructionLibrary = Core.Query.IJSAMObject<ApertureConstructionLibrary>(jObject.Value<JObject>("ApertureConstructionLibrary"));
            }

            if (jObject.ContainsKey("ConstructionLibrary"))
            {
                constructionLibrary = Core.Query.IJSAMObject<ConstructionLibrary>(jObject.Value<JObject>("ConstructionLibrary"));
            }

            if (jObject.ContainsKey("MaterialLibrary"))
            {
                materialLibrary = Core.Query.IJSAMObject<MaterialLibrary>(jObject.Value<JObject>("MaterialLibrary"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (apertureConstructionLibrary != null)
            {
                jObject.Add("ApertureConstructionLibrary", apertureConstructionLibrary.ToJObject());
            }

            if (constructionLibrary != null)
            {
                jObject.Add("ConstructionLibrary", constructionLibrary.ToJObject());
            }

            if (materialLibrary != null)
            {
                jObject.Add("MaterialLibrary", materialLibrary.ToJObject());
            }

            return jObject;
        }
    }
}