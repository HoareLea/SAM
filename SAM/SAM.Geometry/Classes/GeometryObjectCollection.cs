using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Classes
{
    public class GeometryObjectCollection : IEnumerable<ISAMGeometryObject>
    {
        private List<ISAMGeometryObject> sAMGeometryObjects;

        public GeometryObjectCollection()
        {
            sAMGeometryObjects = new List<ISAMGeometryObject>();
        }

        public GeometryObjectCollection(IEnumerable<ISAMGeometryObject> sAMGeometryObjects)
        {
            if (sAMGeometryObjects != null)
            {
                sAMGeometryObjects = new List<ISAMGeometryObject>(sAMGeometryObjects);
            }
        }

        public GeometryObjectCollection(ISAMGeometryObject sAMGeometryObject)
        {
            sAMGeometryObjects = new List<ISAMGeometryObject>() { sAMGeometryObject };
        }

        public void Add(ISAMGeometryObject sAMGeometryObject)
        {
            sAMGeometryObjects.Add(sAMGeometryObject);
        }

        public IEnumerator<ISAMGeometryObject> GetEnumerator()
        {
            return sAMGeometryObjects?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sAMGeometryObjects?.GetEnumerator();
        }
    }
}
