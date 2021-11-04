using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Shell ToSAM_Shell(this Brep brep, bool simplify = true)
        {
            if (brep == null)
                return null;
            
            List<ISAMGeometry3D> sAMGeometry3Ds = ToSAM(brep, simplify);
            if (sAMGeometry3Ds == null || sAMGeometry3Ds.Count == 0)
                return null;

            Shell result = sAMGeometry3Ds.Find(x => x is Shell) as Shell;
            if (result != null)
                return result;

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
            {
                Face3D face3D = sAMGeometry3D as Face3D;
                if (face3D == null)
                    continue;

                face3Ds.Add(face3D);
            }

            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            return new Shell(face3Ds);
        }
    }
}