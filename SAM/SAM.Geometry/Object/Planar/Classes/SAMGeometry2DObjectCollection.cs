using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Planar
{
    public class SAMGeometry2DObjectCollection : IEnumerable<ISAMGeometry2DObject>, ISAMGeometry2DObject
    {
        private List<ISAMGeometry2DObject> sAMGeometry2DObjects;

        public SAMGeometry2DObjectCollection()
        {
            sAMGeometry2DObjects = new List<ISAMGeometry2DObject>();
        }

        public SAMGeometry2DObjectCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMGeometry2DObjectCollection(SAMGeometry2DObjectCollection sAMGeometryObject2DCollection)
        {
            if (sAMGeometryObject2DCollection != null)
            {
                List<ISAMGeometry2DObject> sAMGeometry2DObjects_Temp = sAMGeometryObject2DCollection.sAMGeometry2DObjects;
                if (sAMGeometry2DObjects_Temp != null)
                {
                    sAMGeometry2DObjects = new List<ISAMGeometry2DObject>();
                    foreach (ISAMGeometry2DObject sAMGeometry2DObject in sAMGeometry2DObjects_Temp)
                    {
                        ISAMGeometry2DObject sAMGeometry2DObject_Temp = Core.Query.Clone(sAMGeometry2DObject);
                        if (sAMGeometry2DObject_Temp != null)
                        {
                            sAMGeometry2DObjects.Add(sAMGeometry2DObject_Temp);
                        }
                    }
                }
            }
        }

        public SAMGeometry2DObjectCollection(IEnumerable<ISAMGeometry2DObject> sAMGeometry2DObjects)
        {
            if (sAMGeometry2DObjects != null)
            {
                this.sAMGeometry2DObjects = new List<ISAMGeometry2DObject>();
                foreach (ISAMGeometry2DObject sAMGeometry2DObject in sAMGeometry2DObjects)
                {
                    ISAMGeometry2DObject sAMGeometry2DObject_Temp = Core.Query.Clone(sAMGeometry2DObject);
                    if (sAMGeometry2DObject_Temp == null)
                    {
                        continue;
                    }

                    this.sAMGeometry2DObjects.Add(sAMGeometry2DObject_Temp);
                }
            }
        }

        public SAMGeometry2DObjectCollection(ISAMGeometry2DObject sAMGeometry2DObject)
        {
            ISAMGeometry2DObject sAMGeometry2DObject_Temp = Core.Query.Clone(sAMGeometry2DObject);

            if (sAMGeometry2DObject_Temp != null)
            {
                sAMGeometry2DObjects = new List<ISAMGeometry2DObject>() { sAMGeometry2DObject_Temp };
            }


        }

        public void Add(ISAMGeometry2DObject sAMGeometry2DObject)
        {
            if (sAMGeometry2DObject == null)
            {
                return;
            }

            ISAMGeometry2DObject sAMGeometry2DObject_Temp = Core.Query.Clone(sAMGeometry2DObject);
            if (sAMGeometry2DObject_Temp == null)
            {
                return;
            }

            if (sAMGeometry2DObjects == null)
            {
                sAMGeometry2DObjects = new List<ISAMGeometry2DObject>();
            }

            sAMGeometry2DObjects.Add(sAMGeometry2DObject_Temp);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Geometry2DObjects"))
            {
                sAMGeometry2DObjects = new List<ISAMGeometry2DObject>();

                JArray jArray = jObject.Value<JArray>("Geometry2DObjects");
                foreach (JObject jObject_GeometryObject in jArray)
                {
                    ISAMGeometry2DObject sAMGeometryObject = Core.Query.IJSAMObject(jObject_GeometryObject) as ISAMGeometry2DObject;
                    if (sAMGeometryObject != null)
                    {
                        sAMGeometry2DObjects.Add(sAMGeometryObject);
                    }
                }
            }

            return true;
        }

        public IEnumerator<ISAMGeometry2DObject> GetEnumerator()
        {
            return sAMGeometry2DObjects?.GetEnumerator();
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (sAMGeometry2DObjects != null)
            {
                JArray jArray = new JArray();
                foreach (ISAMGeometry2DObject sAMGeometry2DObject in sAMGeometry2DObjects)
                {
                    if (sAMGeometry2DObject == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometry2DObject.ToJObject());
                }

                jObject.Add("Geometry2DObjects", jArray);
            }

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sAMGeometry2DObjects?.GetEnumerator();
        }
    }
}
