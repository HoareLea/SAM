using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Area(this Shell shell, Plane plane, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
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

        public static double Area(this Shell shell, double offset, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
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

        public static double Area(this Face3D face3D, Range<double> range, int dimensionIndex = 2, double tolerance = Tolerance.Distance)
        {
            List<Face3D> face3Ds = face3D.Cut(range, dimensionIndex, tolerance);
            if(face3Ds == null)
            {
                return double.NaN;
            }

            if(face3Ds.Count == 0)
            {
                return 0;
            }

            double result = 0;
            foreach(Face3D face3D_Temp in face3Ds)
            {
                if(face3D_Temp == null)
                {
                    continue;
                }

                double area = face3D_Temp.GetArea();
                if(double.IsNaN(area))
                {
                    continue;
                }

                result += area;
            }

            return result;
        }
    }
}