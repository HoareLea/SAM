using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Geometry.Object.Spatial
{
    public class Geometry3DObjectCollection : SAMGeometry3DObjectCollection, ITaggable
    {
        public Tag Tag { get; set; }

        public Geometry3DObjectCollection()
            :base()
        {

        }

        public Geometry3DObjectCollection(JObject jObject)
            :base(jObject)
        {
            FromJObject(jObject);
        }

        public Geometry3DObjectCollection(Geometry3DObjectCollection geometryObjectCollection)
            :base(geometryObjectCollection)
        {
            Tag = geometryObjectCollection?.Tag;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            Tag = Core.Query.Tag(jObject);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            Core.Modify.Add(result, Tag);

            return result;
        }
    }
}
