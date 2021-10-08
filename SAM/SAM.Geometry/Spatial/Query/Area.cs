using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Area(this Shell shell, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(shell == null || plane == null)
            {
                return double.NaN;
            }

            List<Face3D> face3Ds = shell.Section(plane, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return 0;
            }

            return face3Ds.ConvertAll(x => x.GetArea()).FindAll(x => !double.IsNaN(x)).Sum();
        }

        public static double Area(this Shell shell, double offset, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (shell == null || double.IsNaN(offset))
            {
                return double.NaN;
            }

            List<Face3D> face3Ds = shell.Section(offset, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return 0;
            }

            return face3Ds.ConvertAll(x => x.GetArea()).FindAll(x => !double.IsNaN(x)).Sum();
        }

        public static double Area(this IFace3DObject face3DObject)
        {
            Face3D face3D = face3DObject?.Face3D;
            if(face3D == null)
            {
                return double.NaN;
            }

            return face3D.GetArea();
        }
    }
}