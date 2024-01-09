using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class ConstructionManager : IJSAMObject, IAnalyticalObject
    {
        public string Name { get; set; }
        public string Description { get; set; }

        private ApertureConstructionLibrary apertureConstructionLibrary;
        private ConstructionLibrary constructionLibrary;
        
        private MaterialLibrary materialLibrary;

        public ConstructionManager(ApertureConstructionLibrary apertureConstructionLibrary, ConstructionLibrary constructionLibrary, MaterialLibrary materialLibrary)
        {
            this.apertureConstructionLibrary = apertureConstructionLibrary;
            this.constructionLibrary = constructionLibrary;
            this.materialLibrary = materialLibrary;
        }

        public ConstructionManager(IEnumerable<ApertureConstruction> apertureConstructions, IEnumerable<Construction> constructions, MaterialLibrary materialLibrary)
        {
            this.materialLibrary = materialLibrary == null ? null : new MaterialLibrary(materialLibrary);    

            apertureConstructionLibrary = new ApertureConstructionLibrary("Default ApertureConstruction Library");
            if(apertureConstructions != null)
            {
                foreach(ApertureConstruction apertureConstruction in apertureConstructions)
                {
                    apertureConstructionLibrary.Add(apertureConstruction);
                }
            }

            constructionLibrary = new ConstructionLibrary("Default Construction Library");
            if (constructions != null)
            {
                foreach (Construction construction in constructions)
                {
                    constructionLibrary.Add(construction);
                }
            }

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

                Name = constructionManager.Name;
                Description = constructionManager.Description;
            }
        }

        public ConstructionManager()
        {

        }

        public List<ApertureConstruction> ApertureConstructions
        {
            get
            {
                return apertureConstructionLibrary?.GetApertureConstructions()?.ConvertAll(x => x?.Clone());
            }
        }

        public List<Construction> Constructions
        {
            get
            {
                return constructionLibrary?.GetConstructions()?.ConvertAll(x => x?.Clone());
            }
        }

        public List<IMaterial> Materials
        {
            get
            {
                return materialLibrary?.GetMaterials()?.ConvertAll(x => x?.Clone());
            }
        }

        public MaterialLibrary MaterialLibrary
        {
            get
            {
                return materialLibrary == null ? null : new MaterialLibrary(materialLibrary);
            }
        }

        public bool Remove(IMaterial material)
        {
            if (material == null || materialLibrary == null)
            {
                return false;
            }

            return materialLibrary.Remove(material);
        }

        public bool Remove(ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null || apertureConstructionLibrary == null)
            {
                return false;
            }

            return apertureConstructionLibrary.Remove(apertureConstruction);
        }

        public bool Remove(Construction construction)
        {
            if (construction == null || constructionLibrary == null)
            {
                return false;
            }

            return constructionLibrary.Remove(construction);
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

        public bool Add(Construction construction, PanelType panelType)
        {
            if(construction == null)
            {
                return false;
            }

            Construction construction_New = new Construction(construction);
            construction_New.SetValue(ConstructionParameter.DefaultPanelType, panelType);

            return Add(construction_New);
        }

        public bool Add(ApertureConstruction apertureConstruction, PanelType panelType)
        {
            if (apertureConstruction == null)
            {
                return false;
            }

            ApertureConstruction apertureConstruction_New = new ApertureConstruction(apertureConstruction);
            apertureConstruction_New.SetValue(ApertureConstructionParameter.DefaultPanelType, panelType);

            return Add(apertureConstruction_New);
        }

        public List<Construction> GetConstructions(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            return constructionLibrary?.GetConstructions(text, textComparisonType, caseSensitive);
        }

        public List<Construction> GetConstructions(PanelType panelType)
        {
            List<Construction> constructions = constructionLibrary?.GetConstructions();
            if (constructions == null)
            {
                return null;
            }

            List<Construction> result = new List<Construction>(constructions);
            foreach(Construction construction in constructions)
            {
                if(construction == null)
                {
                    continue;
                }

                if(!construction.TryGetValue(ConstructionParameter.DefaultPanelType, out string panelTypeString) || string.IsNullOrWhiteSpace(panelTypeString))
                {
                    continue;
                }

                if(!Core.Query.TryGetEnum(panelTypeString, out PanelType panelType_Temp))
                {
                    continue;
                }

                if(panelType_Temp == panelType)
                {
                    result.Add(construction);
                }
            }

            return result;
        }

        public List<ApertureConstruction> GetApertureConstructions(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            return apertureConstructionLibrary?.GetApertureConstructions(text, textComparisonType, caseSensitive);
        }

        public List<ApertureConstruction> GetApertureConstructions(ApertureType apertureType, string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            return apertureConstructionLibrary?.GetApertureConstructions(text, textComparisonType, caseSensitive, apertureType: apertureType);
        }

        public List<ApertureConstruction> GetApertureConstructions(ApertureType apertureType, PanelType panelType)
        {
            List<ApertureConstruction> apertureConstructions = apertureConstructionLibrary?.GetApertureConstructions();
            if (apertureConstructions == null)
            {
                return null;
            }

            List<ApertureConstruction> result = new List<ApertureConstruction>(apertureConstructions);
            foreach (ApertureConstruction apertureConstruction in apertureConstructions)
            {
                if (apertureConstruction == null)
                {
                    continue;
                }

                if (apertureConstruction.ApertureType != apertureType)
                {
                    continue;
                }

                if (!apertureConstruction.TryGetValue(ApertureConstructionParameter.DefaultPanelType, out string panelTypeString) || string.IsNullOrWhiteSpace(panelTypeString))
                {
                    continue;
                }

                if (!Core.Query.TryGetEnum(panelTypeString, out PanelType panelType_Temp))
                {
                    continue;
                }

                if (panelType_Temp == panelType)
                {
                    result.Add(apertureConstruction);
                }
            }

            return result;
        }

        public IMaterial GetMaterial(string name)
        {
            return materialLibrary?.GetMaterial(name);
        }

        public List<T> GetMaterials<T>(Construction construction) where T: IMaterial
        {
            if(construction == null || materialLibrary == null)
            {
                return null;
            }

            List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
            if(constructionLayers == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = materialLibrary.GetMaterial(constructionLayer.Name);
                if(material is T)
                {
                    result.Add((T)material);
                }
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Name"))
            {
                Name = jObject.Value<string>("Name");
            }

            if (jObject.ContainsKey("Description"))
            {
                Description = jObject.Value<string>("Description");
            }

            if (jObject.ContainsKey("ApertureConstructionLibrary"))
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

            if (Name != null)
            {
                jObject.Add("Name", Name);
            }

            if (Description != null)
            {
                jObject.Add("Description", Description);
            }

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