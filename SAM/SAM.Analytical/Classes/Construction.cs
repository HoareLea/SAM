// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    public class Construction : SAMType, IAnalyticalObject
    {
        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
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
                        this.constructionLayers.Add(new ConstructionLayer(constructionLayer));
            }
        }

        public Construction(Guid guid, string name, IEnumerable<ConstructionLayer> constructionLayers)
            : base(guid, name)
        {
            if (constructionLayers != null)
            {
                this.constructionLayers = new List<ConstructionLayer>();
                foreach (ConstructionLayer constructionLayer in constructionLayers)
                    if (constructionLayer != null)
                        this.constructionLayers.Add(new ConstructionLayer(constructionLayer));
            }
        }

        public Construction(Guid guid, string name)
            : base(guid, name)
        {
        }

        public Construction(Construction construction)
            : base(construction)
        {
            constructionLayers = construction?.constructionLayers?.ConvertAll(x => new ConstructionLayer(x));
        }

        public Construction(Construction construction, Guid guid)
            : base(construction, guid)
        {
            constructionLayers = construction?.constructionLayers?.ConvertAll(x => new ConstructionLayer(x));
        }

        public Construction(Construction construction, string name)
            : base(construction, name)
        {
            constructionLayers = construction.constructionLayers?.ConvertAll(x => new ConstructionLayer(x));
        }

        public Construction(Guid guid, Construction construction, string name)
            : base(guid, name, construction)
        {
            constructionLayers = construction.constructionLayers?.ConvertAll(x => new ConstructionLayer(x));
        }

        public Construction(Construction construction, IEnumerable<ConstructionLayer> constructionLayers)
            : base(construction)
        {
            this.constructionLayers = constructionLayers?.ToList().ConvertAll(x => new ConstructionLayer(x));
        }
        public Construction(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        public List<ConstructionLayer> ConstructionLayers
        {
            get
            {
                return constructionLayers?.ConvertAll(x => new ConstructionLayer(x));
            }
        }

        public bool HasConstructionLayers()
        {
            return constructionLayers != null && constructionLayers.Count != 0;
        }

        public double GetThickness()
        {
            if (constructionLayers == null || constructionLayers.Count == 0)
                return double.NaN;

            return constructionLayers.ConvertAll(x => x.Thickness).Sum();
        }

        public double GetThickness(bool includeSoil)
        {
            return includeSoil ? GetThickness() : constructionLayers?.FindAll(x => !x.IsSoil)?.ConvertAll(x => x.Thickness)?.Sum() ?? double.NaN;
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
                return false;

            if (jsonObject["ConstructionLayers"] is JsonArray constructionLayersArray)
                constructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(constructionLayersArray);

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
                return null;

            if (constructionLayers != null)
            {
                JsonArray constructionLayersArray = new JsonArray();
                foreach (ConstructionLayer layer in constructionLayers)
                {
                    if (layer?.ToJsonObject() is JsonObject layerJson)
                    {
                        constructionLayersArray.Add(layerJson.DeepClone());
                    }
                }
                jsonObject["ConstructionLayers"] = constructionLayersArray;
            }

            return jsonObject;
        }
    }
}
