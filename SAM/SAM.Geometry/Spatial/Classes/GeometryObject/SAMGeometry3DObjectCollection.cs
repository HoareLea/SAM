using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class SAMGeometry3DObjectCollection : IEnumerable<ISAMGeometry3DObject>, ISAMGeometry3DObject
    {
        private List<ISAMGeometry3DObject> sAMGeometry3DObjects;

        public SAMGeometry3DObjectCollection()
        {
            sAMGeometry3DObjects = new List<ISAMGeometry3DObject>();
        }

        public SAMGeometry3DObjectCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMGeometry3DObjectCollection(SAMGeometry3DObjectCollection sAMGeometryObject3DCollection)
        {
            if (sAMGeometryObject3DCollection != null)
            {
                List<ISAMGeometry3DObject> sAMGeometry3DObjects_Temp = sAMGeometryObject3DCollection.sAMGeometry3DObjects;
                if(sAMGeometry3DObjects_Temp != null)
                {
                    sAMGeometry3DObjects = new List<ISAMGeometry3DObject>();
                    foreach (ISAMGeometry3DObject sAMGeometry3DObject in sAMGeometry3DObjects_Temp)
                    {
                        ISAMGeometry3DObject sAMGeometry3DObject_Temp = Core.Query.Clone(sAMGeometry3DObject);
                        if(sAMGeometry3DObject_Temp != null)
                        {
                            sAMGeometry3DObjects.Add(sAMGeometry3DObject_Temp);
                        }
                    }
                }
            }
        }

        public SAMGeometry3DObjectCollection(IEnumerable<ISAMGeometry3DObject> sAMGeometry3DObjects)
        {
            if (sAMGeometry3DObjects != null)
            {
                this.sAMGeometry3DObjects = new List<ISAMGeometry3DObject>();
                foreach(ISAMGeometry3DObject sAMGeometry3DObject in sAMGeometry3DObjects)
                {
                    ISAMGeometry3DObject sAMGeometry3DObject_Temp = Core.Query.Clone(sAMGeometry3DObject);
                    if(sAMGeometry3DObject_Temp == null)
                    {
                        continue;
                    }

                    this.sAMGeometry3DObjects.Add(sAMGeometry3DObject_Temp);
                }
            }
        }

        public SAMGeometry3DObjectCollection(ISAMGeometry3DObject sAMGeometry3DObject)
        {
            ISAMGeometry3DObject sAMGeometry3DObject_Temp = Core.Query.Clone(sAMGeometry3DObject);

            if(sAMGeometry3DObject_Temp != null)
            {
                sAMGeometry3DObjects = new List<ISAMGeometry3DObject>() { sAMGeometry3DObject_Temp };
            }

            
        }

        public void Add(ISAMGeometry3DObject sAMGeometry3DObject)
        {
            if(sAMGeometry3DObject == null)
            {
                return;
            }

            ISAMGeometry3DObject sAMGeometry3DObject_Temp = Core.Query.Clone(sAMGeometry3DObject);
            if(sAMGeometry3DObject_Temp == null)
            {
                return;
            }

            if (sAMGeometry3DObjects == null)
            {
                sAMGeometry3DObjects = new List<ISAMGeometry3DObject>();
            }

            sAMGeometry3DObjects.Add(sAMGeometry3DObject_Temp);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Geometry3DObjects"))
            {
                sAMGeometry3DObjects = new List<ISAMGeometry3DObject>();

                JArray jArray = jObject.Value<JArray>("Geometry3DObjects");
                foreach(JObject jObject_GeometryObject in jArray)
                {
                    ISAMGeometry3DObject sAMGeometryObject = Core.Query.IJSAMObject(jObject_GeometryObject) as ISAMGeometry3DObject;
                    if(sAMGeometryObject != null)
                    {
                        sAMGeometry3DObjects.Add(sAMGeometryObject);
                    }
                }
            }

            return true;
        }

        public IEnumerator<ISAMGeometry3DObject> GetEnumerator()
        {
            return sAMGeometry3DObjects?.GetEnumerator();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(sAMGeometry3DObjects != null)
            {
                JArray jArray = new JArray();
                foreach(ISAMGeometry3DObject sAMGeometry3DObject in sAMGeometry3DObjects)
                {
                    if(sAMGeometry3DObject == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometry3DObject.ToJObject());
                }

                jObject.Add("Geometry3DObjects", jArray);
            }

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sAMGeometry3DObjects?.GetEnumerator();
        }
    }
}
