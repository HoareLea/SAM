using Grasshopper.Kernel.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Query
    {
        public static bool TryGetSAMGeometries<T>(this GH_ObjectWrapper objectWrapper, out List<T> sAMGeometries) where T : SAMGeometry
        {
            sAMGeometries = null;

            if (objectWrapper == null || objectWrapper.Value == null)
                return false;

            if (objectWrapper.Value is T)
            {
                sAMGeometries = new List<T>() { (T)objectWrapper.Value };
                return true;
            }


            if (objectWrapper.Value is GooSAMGeometry)
            {
                ISAMGeometry sAMGeometry = ((GooSAMGeometry)objectWrapper.Value).Value;
                if(sAMGeometry is T)
                {
                    sAMGeometries = new List<T>() { (T)sAMGeometry };
                    return true;
                }
            }


            if (objectWrapper.Value is IGH_GeometricGoo)
            {
                object @object = Convert.ToSAM(objectWrapper.Value as dynamic);
                if (@object is IEnumerable)
                {
                    IEnumerable<ISAMGeometry> sAMGeometries_Temp = ((IEnumerable)@object).Cast<ISAMGeometry>();
                    if(sAMGeometries_Temp != null && sAMGeometries_Temp.Count() > 0)
                    {
                        sAMGeometries = sAMGeometries_Temp.ToList().FindAll(x => x is T).Cast<T>().ToList();
                        return sAMGeometries.Count > 0;
                    }

                }
                else if(@object is T)
                {
                    sAMGeometries = new List<T>() { (T)@object };
                }
            }

            return false;

        }
    }
}