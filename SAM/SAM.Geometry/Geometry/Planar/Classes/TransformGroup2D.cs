using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public class TransformGroup2D : ITransform2D, IEnumerable<ITransform2D>
    {
        private List<ITransform2D> transform2Ds;

        public TransformGroup2D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public TransformGroup2D(IEnumerable<ITransform2D> transform2Ds)
        {
            if(transform2Ds != null)
            {
                this.transform2Ds = new List<ITransform2D>();
                foreach(ITransform2D transform2D in transform2Ds)
                {
                    if(transform2D == null)
                    {
                        continue;
                    }

                    this.transform2Ds.Add(transform2D.Clone());
                }
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Transform2Ds"))
            {
                transform2Ds = new List<ITransform2D>();
                foreach(JObject jObject_Transform2D in jObject.Value<JArray>("Transform2Ds"))
                {
                    transform2Ds.Add(Core.Query.IJSAMObject<ITransform2D>(jObject_Transform2D));
                }
            }

            return true;
        }

        public IEnumerator<ITransform2D> GetEnumerator()
        {
            return transform2Ds?.ConvertAll(x => x.Clone()).GetEnumerator();
        }

        public void Inverse()
        {
            if(transform2Ds == null)
            {
                return;
            }

            transform2Ds.Reverse();

            foreach(ITransform2D transform2D in transform2Ds)
            {
                transform2D.Inverse();
            }
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(transform2Ds != null)
            {
                JArray jArray = new JArray();
                foreach(Transform2D transform2D in transform2Ds)
                {
                    if(transform2D == null)
                    {
                        continue;
                    }

                    jArray.Add(transform2D.ToJObject());
                }

                jObject.Add("Transform2Ds", jArray);
            }

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}