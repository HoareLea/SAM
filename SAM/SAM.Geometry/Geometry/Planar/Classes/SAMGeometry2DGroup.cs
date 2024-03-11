using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class SAMGeometry2DGroup : ISAMGeometry2D, IEnumerable<ISAMGeometry2D>
    {
        private CoordinateSystem2D coordinateSystem2D;
        private List<ISAMGeometry2D> sAMGeometry2Ds;

        public SAMGeometry2DGroup()
        {
            coordinateSystem2D = CoordinateSystem2D.World;
        }

        public SAMGeometry2DGroup(SAMGeometry2DGroup sAMGeometry2DGroup)
        {
            if(sAMGeometry2DGroup != null)
            {
                coordinateSystem2D = sAMGeometry2DGroup.coordinateSystem2D == null ? null : new CoordinateSystem2D(sAMGeometry2DGroup.coordinateSystem2D);
                sAMGeometry2Ds = sAMGeometry2DGroup.sAMGeometry2Ds?.ConvertAll(x => x?.Clone() as ISAMGeometry2D);
            }
        }

        public SAMGeometry2DGroup(CoordinateSystem2D coordinateSystem2D)
        {
            this.coordinateSystem2D = coordinateSystem2D == null ? null : new CoordinateSystem2D(coordinateSystem2D); 
        }

        private SAMGeometry2DGroup(CoordinateSystem2D coordinateSystem2D, IEnumerable<ISAMGeometry2D> sAMGeometry2Ds)
        {
            this.coordinateSystem2D = coordinateSystem2D == null ? null : new CoordinateSystem2D(coordinateSystem2D);
            this.sAMGeometry2Ds = sAMGeometry2Ds == null ? null : sAMGeometry2Ds.ToList().ConvertAll(x => x.Clone() as ISAMGeometry2D);
        }

        public SAMGeometry2DGroup(IEnumerable<ISAMGeometry2D> sAMGeometry2Ds)
        {
            coordinateSystem2D = CoordinateSystem2D.World;
            this.sAMGeometry2Ds = sAMGeometry2Ds == null ? null : sAMGeometry2Ds.ToList().ConvertAll(x => x.Clone() as ISAMGeometry2D); 
        }

        public SAMGeometry2DGroup(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(ISAMGeometry2D sAMGeometry2D)
        {
            if(sAMGeometry2D == null || coordinateSystem2D == null)
            {
                return false;
            }

            Transform2D transform2D = Transform2D.GetCoordinateSystem2DToCoordinateSystem2D(CoordinateSystem2D.World, coordinateSystem2D);
            if(transform2D == null)
            {
                return false;
            }

            if(sAMGeometry2Ds == null)
            {
                sAMGeometry2Ds = new List<ISAMGeometry2D>();
            }

            sAMGeometry2Ds.Add(sAMGeometry2D.GetTransformed(transform2D));
            return true;
        }

        public int Count
        {
            get
            {
                if(sAMGeometry2Ds == null)
                {
                    return -1;
                }

                return sAMGeometry2Ds.Count;
            }
        }

        public ISAMGeometry2D this[int index]
        {
            get
            {
                Transform2D transform2D = Transform2D.GetCoordinateSystem2DToCoordinateSystem2D(coordinateSystem2D, CoordinateSystem2D.World);

                return sAMGeometry2Ds[index].GetTransformed(transform2D);
            }

            set
            {
                if(value == null)
                {
                    sAMGeometry2Ds[index] = null;
                    return;
                }

                Transform2D transform2D = Transform2D.GetCoordinateSystem2DToCoordinateSystem2D(CoordinateSystem2D.World, coordinateSystem2D);
                sAMGeometry2Ds[index] = value.GetTransformed(transform2D);
            }
        }

        public ISAMGeometry Clone()
        {
            return new SAMGeometry2DGroup(this);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("CoordinateSystem2D"))
            {
                coordinateSystem2D = new CoordinateSystem2D(jObject.Value<JObject>("CoordinateSystem2D"));
            }

            if(jObject.ContainsKey("SAMGeometry2Ds"))
            {
                JArray jArray = jObject.Value<JArray>("SAMGeometry2Ds");
                if(jArray != null)
                {
                    sAMGeometry2Ds = new List<ISAMGeometry2D>();
                    foreach(JObject jObject_Temp in jArray)
                    {
                        sAMGeometry2Ds.Add(Core.Query.IJSAMObject<ISAMGeometry2D>(jObject_Temp));
                    }
                }
            }

            return true;
        }

        public IEnumerator<ISAMGeometry2D> GetEnumerator()
        {
            List<ISAMGeometry2D> sAMGeometry2Ds_Temp = new List<ISAMGeometry2D>();
            if(sAMGeometry2Ds != null)
            {
                Transform2D transform2D = Transform2D.GetCoordinateSystem2DToCoordinateSystem2D(coordinateSystem2D, CoordinateSystem2D.World);
                foreach (ISAMGeometry2D sAMGeometry2D in sAMGeometry2Ds)
                {
                    sAMGeometry2Ds_Temp.Add(sAMGeometry2D.GetTransformed(transform2D));
                }
            }

            return sAMGeometry2Ds_Temp.GetEnumerator();
        }

        public ISAMGeometry2D GetMoved(Vector2D vector2D)
        {
            if(vector2D == null)
            {
                return null;
            }

            Transform2D transform3D = Transform2D.GetTranslation(vector2D);

            return GetTransformed(transform3D);
        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            if(transform2D == null)
            {
                return null;
            }

            CoordinateSystem2D coordinateSystem2D_New = coordinateSystem2D.Transform(transform2D);

            return new SAMGeometry2DGroup(coordinateSystem2D_New, sAMGeometry2Ds);
        }

        public bool Transform(Transform2D transform2D)
        {
            if (transform2D == null)
            {
                return false;
            }

            coordinateSystem2D = coordinateSystem2D.Transform(transform2D);
            return true; 
        }

        public virtual JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if(coordinateSystem2D != null)
            {
                result.Add("CoordinateSystem2D", coordinateSystem2D.ToJObject());
            }

            if(sAMGeometry2Ds != null)
            {
                JArray jArray = new JArray();
                foreach(ISAMGeometry2D sAMGeometry3D in sAMGeometry2Ds)
                {
                    if(sAMGeometry3D == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometry3D.ToJObject());
                }

                result.Add("SAMGeometry2Ds", jArray);
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}