using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static bool TryConvertToPanelGeometries(this object @object, out List<ISAMGeometry3D> sAMGeometry3Ds, bool simplify = true)
        {
            sAMGeometry3Ds = null;
            
            if (@object is IGH_GeometricGoo)
            {
                sAMGeometry3Ds = ((IGH_GeometricGoo)@object).ToSAM(simplify).Cast<ISAMGeometry3D>().ToList();
                return true;
            }

            if(@object is ISAMGeometry3D)
            {
                sAMGeometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)@object };
                return true;
            }

            if (@object is GH_ObjectWrapper)
            {
                GH_ObjectWrapper objectWrapper_Temp = ((GH_ObjectWrapper)@object);
                if (objectWrapper_Temp.Value is ISAMGeometry3D)
                {
                    sAMGeometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)objectWrapper_Temp.Value };
                    return true;
                }
            }
            else if (@object is IGH_Goo)
            {
                Geometry.ISAMGeometry sAMGeometry = (@object as dynamic).Value as Geometry.ISAMGeometry;
                if (sAMGeometry is ISAMGeometry3D)
                {
                    sAMGeometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)sAMGeometry };
                    return true;
                }
                else if (sAMGeometry is Geometry.Planar.ISAMGeometry2D)
                {
                    sAMGeometry3Ds = new List<ISAMGeometry3D>() { Plane.WorldXY.Convert(sAMGeometry as dynamic) };
                    return true;
                }
            }

            return false;
        }
    }
}