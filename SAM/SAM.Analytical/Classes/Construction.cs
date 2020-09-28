using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class Construction : SAMType
    {
        private List<ConstructionLayer> constructionLayers;

        public Construction(string name)
            : base(name)
        {
        }

        public Construction(string name, IEnumerable<ConstructionLayer> constructionLayers)
            : base(name)
        {
            if (constructionLayers != null)
            {
                this.constructionLayers = new List<ConstructionLayer>();
                foreach (ConstructionLayer constructionLayer in constructionLayers)
                    if (constructionLayer != null)
                        this.constructionLayers.Add(constructionLayer.Clone());
            }
        }

        public Construction(Guid guid, string name, IEnumerable<ConstructionLayer> constructionLayers)
            : base(guid, name)
        {
            if(constructionLayers != null)
            {
                this.constructionLayers = new List<ConstructionLayer>();
                foreach (ConstructionLayer constructionLayer in constructionLayers)
                    if (constructionLayer != null)
                        this.constructionLayers.Add(constructionLayer.Clone());
            }
        }

        public Construction(Guid guid, string name)
            : base(guid, name)
        {
        }

        public Construction(Construction construction)
            : base(construction)
        {
            constructionLayers = construction?.constructionLayers?.ConvertAll(x => x.Clone());
        }

        public Construction(Construction construction, Guid guid)
            : base(construction, guid)
        {
            constructionLayers = construction?.constructionLayers?.ConvertAll(x => x.Clone());
        }

        public Construction(Construction construction, string name)
            : base(construction, name)
        {
            constructionLayers = construction.constructionLayers?.ConvertAll(x => x.Clone());
        }

        public Construction(JObject jObject)
            : base(jObject)
        {
        }

        public List<ConstructionLayer> ConstructionLayers
        {
            get
            {
                return constructionLayers?.ConvertAll(x => x.Clone());
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("ConstructionLayers"))
                constructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(jObject.Value<JArray>("ConstructionLayers"));

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(constructionLayers != null)
                jObject.Add("ConstructionLayers", Core.Create.JArray(constructionLayers));

            return jObject;
        }
    }
}