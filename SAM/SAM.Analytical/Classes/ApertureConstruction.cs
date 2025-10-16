﻿using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ApertureConstruction : SAMType, IAnalyticalObject
    {
        private ApertureType apertureType;
        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        private List<ConstructionLayer> frameConstructionLayers;
        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        private List<ConstructionLayer> paneConstructionLayers;

        public ApertureConstruction(string name, ApertureType apertureType)
            : base(name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(Guid guid, string name, ApertureType apertureType)
            : base(guid, name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction, string name)
            : base(apertureConstruction, name)
        {
            apertureType = apertureConstruction.apertureType;
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(Guid guid, ApertureConstruction apertureConstruction, string name)
            : base(guid, name, apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(Guid guid, string name, ApertureType apertureType, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers = null)
             : base(guid, name)
        {
            this.apertureType = apertureType;
            this.paneConstructionLayers = paneConstructionLayers?.ToList().ConvertAll(x => new ConstructionLayer(x));
            this.frameConstructionLayers = frameConstructionLayers?.ToList().ConvertAll(x => new ConstructionLayer(x));
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction, ApertureType apertureType)
            : base(apertureConstruction)
        {
            this.apertureType = apertureType;
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers = null)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;

            this.frameConstructionLayers = frameConstructionLayers?.ToList().ConvertAll(x => new ConstructionLayer(x));
            this.paneConstructionLayers = paneConstructionLayers?.ToList().ConvertAll(x => new ConstructionLayer(x));
        }

        public ApertureConstruction(JObject jObject)
            : base(jObject)
        {

        }

        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        public List<ConstructionLayer> FrameConstructionLayers
        {
            get
            {
                return frameConstructionLayers?.ConvertAll(x => new ConstructionLayer(x));
            }
        }

        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        public List<ConstructionLayer> PaneConstructionLayers
        {
            get
            {
                return paneConstructionLayers?.ConvertAll(x => new ConstructionLayer(x));
            }
        }

        public bool HasFrameConstructionLayers()
        {
            return frameConstructionLayers != null && frameConstructionLayers.Count != 0;
        }

        public bool HasPaneConstructionLayers()
        {
            return paneConstructionLayers != null && paneConstructionLayers.Count != 0;
        }

        public List<ConstructionLayer> GetConstructionLayers(AperturePart aperturePart)
        {
            if(aperturePart == AperturePart.Undefined)
            {
                return null;
            }

            switch(aperturePart)
            {
                case AperturePart.Frame:
                    return FrameConstructionLayers;

                case AperturePart.Pane:
                    return PaneConstructionLayers;
            }

            return null;
        }

        public double GetThickness(AperturePart aperturePart)
        {
            if(aperturePart == AperturePart.Undefined)
            {
                return double.NaN;
            }

            List<ConstructionLayer> constructionLayers = GetConstructionLayers(aperturePart);
            if(constructionLayers == null || constructionLayers.Count == 0)
            {
                return 0;
            }

            return constructionLayers.ConvertAll(x => x.Thickness).Sum();
        }

        public double GetThickness()
        {
            double paneThickness = GetPaneThickness();
            double frameThickness = GetFrameThickness();

            if(double.IsNaN(paneThickness) && double.IsNaN(frameThickness))
            {
                return double.NaN;
            }

            paneThickness = double.IsNaN(paneThickness) ? 0 : paneThickness;
            frameThickness = double.IsNaN(frameThickness) ? 0 : frameThickness;

            return System.Math.Max(paneThickness, frameThickness);
        }

        public double GetPaneThickness()
        {
            return GetThickness(AperturePart.Pane);
        }

        public double GetFrameThickness()
        {
            return GetThickness(AperturePart.Frame);
        }

        public ApertureType ApertureType
        {
            get
            {
                return apertureType;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("FrameConstructionLayers"))
                frameConstructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(jObject.Value<JArray>("FrameConstructionLayers"));

            if (jObject.ContainsKey("PaneConstructionLayers"))
                paneConstructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(jObject.Value<JArray>("PaneConstructionLayers"));

            if (jObject.ContainsKey("ApertureType"))
                Enum.TryParse(jObject.Value<string>("ApertureType"), out apertureType);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (paneConstructionLayers != null)
                jObject.Add("PaneConstructionLayers", Core.Create.JArray(paneConstructionLayers));

            if (frameConstructionLayers != null)
                jObject.Add("FrameConstructionLayers", Core.Create.JArray(frameConstructionLayers));

            jObject.Add("ApertureType", apertureType.ToString());

            return jObject;
        }
    }
}