using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public abstract class BuildingElement : SAMInstance
    {
        private Face3D face3D;
        
        public BuildingElement(BuildingElement buildingElement)
            : base(buildingElement)
        {

        }

        public BuildingElement(JObject jObject)
            : base(jObject)
        {

        }

        public BuildingElement(BuildingElementType buildingElementType, Face3D face3D)
            : base(buildingElementType?.Name, buildingElementType)
        {
            this.face3D = face3D;
        }

        public Face3D Face3D
        {
            get
            {
                if (face3D == null)
                    return null;

                return new Face3D(face3D);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Face3D"))
            {
                face3D = Geometry.Create.ISAMGeometry<Face3D>(jObject.Value<JObject>("Face3D"));
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

            if (face3D != null)
            {
                jObject.Add("Face3D", face3D.ToJObject());
            }

            return jObject;
        }

    }
}
