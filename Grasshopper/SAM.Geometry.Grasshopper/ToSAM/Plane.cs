using GH_IO.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Plane ToSAM(this Rhino.Geometry.Plane plane)
        {
            return new Spatial.Plane(plane.Origin.ToSAM(), plane.Normal.ToSAM());
        }

        public static Spatial.Plane ToSAM(this GH_Plane plane)
        {
            Spatial.Point3D origin = plane.Origin.ToSAM();

            Spatial.Vector3D vector3D_1 = plane.XAxis.ToSAM().ToVector3D();
            Spatial.Point3D point3D_1 = origin.GetMoved(vector3D_1);

            Spatial.Vector3D vector3D_2 = plane.YAxis.ToSAM().ToVector3D();
            Spatial.Point3D point3D_2 = origin.GetMoved(vector3D_2);

            return new Spatial.Plane(origin, point3D_1, point3D_2);
        }
    }
}
