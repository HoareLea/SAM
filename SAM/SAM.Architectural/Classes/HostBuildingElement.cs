using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class HostBuildingElement : BuildingElement
    {
        private List<Opening> openings;
        
        public HostBuildingElement(HostBuildingElement hostBuildingElement)
            : base(hostBuildingElement)
        {

        }

        public HostBuildingElement(JObject jObject)
            : base(jObject)
        {

        }

        public HostBuildingElement(HostBuildingElementType hostBuildingElementType, Face3D face3D)
            : base(hostBuildingElementType, face3D)
        {

        }

        public List<Opening> Openings
        {
            get
            {
                if (openings == null)
                    return null;

                return openings.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public bool AddOpening(Opening opening)
        {
            if (opening == null)
                return false;

            if (!Query.IsValid(this, opening))
                return false;

            if (openings == null)
                openings = new List<Opening>();

            openings.Add(opening);
            return true;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }


            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
