using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class SAMGeometry3DGroup : ISAMGeometry3D, IEnumerable<ISAMGeometry3D>
    {
        private CoordinateSystem3D coordinateSystem3D;
        private List<ISAMGeometry3D> sAMGeometry3Ds;

        public SAMGeometry3DGroup()
        {
            coordinateSystem3D = CoordinateSystem3D.World;
        }

        public SAMGeometry3DGroup(SAMGeometry3DGroup sAMGeometry3DGroup)
        {
            if(sAMGeometry3DGroup != null)
            {
                coordinateSystem3D = sAMGeometry3DGroup.coordinateSystem3D == null ? null : new CoordinateSystem3D(sAMGeometry3DGroup.coordinateSystem3D);
                sAMGeometry3Ds = sAMGeometry3DGroup.sAMGeometry3Ds?.ConvertAll(x => x?.Clone() as ISAMGeometry3D);
            }
        }

        public SAMGeometry3DGroup(CoordinateSystem3D coordinateSystem3D)
        {
            this.coordinateSystem3D = coordinateSystem3D == null ? null : new CoordinateSystem3D(coordinateSystem3D); 
        }

        private SAMGeometry3DGroup(CoordinateSystem3D coordinateSystem3D, IEnumerable<ISAMGeometry3D> sAMGeometry3Ds)
        {
            this.coordinateSystem3D = coordinateSystem3D == null ? null : new CoordinateSystem3D(coordinateSystem3D);
            this.sAMGeometry3Ds = sAMGeometry3Ds == null ? null : sAMGeometry3Ds.ToList().ConvertAll(x => x.Clone() as ISAMGeometry3D);
        }

        public SAMGeometry3DGroup(IEnumerable<ISAMGeometry3D> sAMGeometry3Ds)
        {
            coordinateSystem3D = CoordinateSystem3D.World;
            this.sAMGeometry3Ds = sAMGeometry3Ds == null ? null : sAMGeometry3Ds.ToList().ConvertAll(x => x.Clone() as ISAMGeometry3D); 
        }

        public SAMGeometry3DGroup(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(ISAMGeometry3D sAMGeometry3D)
        {
            if(sAMGeometry3D == null || coordinateSystem3D == null)
            {
                return false;
            }

            Transform3D transform3D = Transform3D.GetCoordinateSystem3DToCoordinateSystem3D(CoordinateSystem3D.World, coordinateSystem3D);
            if(transform3D == null)
            {
                return false;
            }

            sAMGeometry3Ds.Add(sAMGeometry3D.GetTransformed(transform3D));
            return true;
        }

        public ISAMGeometry Clone()
        {
            return new SAMGeometry3DGroup(this);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("CoordinateSystem3D"))
            {
                coordinateSystem3D = new CoordinateSystem3D(jObject.Value<JObject>("CoordinateSystem3D"));
            }

            if(jObject.ContainsKey("SAMGeometry3Ds"))
            {
                JArray jArray = jObject.Value<JArray>("SAMGeometry3Ds");
                if(jArray != null)
                {
                    sAMGeometry3Ds = new List<ISAMGeometry3D>();
                    foreach(JObject jObject_Temp in jArray)
                    {
                        sAMGeometry3Ds.Add(Core.Query.IJSAMObject<ISAMGeometry3D>(jObject_Temp));
                    }
                }
            }

            return true;
        }

        public IEnumerator<ISAMGeometry3D> GetEnumerator()
        {
            List<ISAMGeometry3D> sAMGeometry3Ds_Temp = new List<ISAMGeometry3D>();
            if(sAMGeometry3Ds != null)
            {
                Transform3D transform3D = Transform3D.GetCoordinateSystem3DToCoordinateSystem3D(coordinateSystem3D, CoordinateSystem3D.World);
                foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                {
                    sAMGeometry3Ds_Temp.Add(sAMGeometry3D.GetTransformed(transform3D));
                }
            }

            return sAMGeometry3Ds_Temp.GetEnumerator();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            if(vector3D == null)
            {
                return null;
            }

            Transform3D transform3D = Transform3D.GetTranslation(vector3D);

            return GetTransformed(transform3D);
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if(transform3D == null)
            {
                return null;
            }

            CoordinateSystem3D coordinateSystem3D_New = coordinateSystem3D.Transform(transform3D);

            return new SAMGeometry3DGroup(coordinateSystem3D_New, sAMGeometry3Ds);
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if(coordinateSystem3D != null)
            {
                result.Add("CoordinateSystem3D", coordinateSystem3D.ToJObject());
            }

            if(sAMGeometry3Ds != null)
            {
                JArray jArray = new JArray();
                foreach(ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                {
                    if(sAMGeometry3D == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometry3D.ToJObject());
                }

                result.Add("SAMGeometry3Ds", jArray);
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}