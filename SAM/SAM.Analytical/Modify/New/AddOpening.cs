using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IOpening AddOpening(this IHostPartition hostPartition, OpeningType openingType, double ratio, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(hostPartition == null || openingType == null || ratio == 0)
            {
                return null;
            }

            Face3D face3D = hostPartition.Face3D;
            if(face3D == null)
            {
                return null;
            }

            double area = face3D.GetArea();
            double area_Target = area * ratio;
            if (area_Target < tolerance_Distance)
                return null;

            if (area_Target < area)
            {
                Plane plane = face3D.GetPlane();
                Face2D face2D_HostPartition = Geometry.Spatial.Query.Convert(plane, face3D);
                if (face2D_HostPartition == null)
                    return null;

                Segment2D[] diagonals = face2D_HostPartition.GetBoundingBox()?.GetDiagonals();
                if (diagonals == null || diagonals.Length == 0)
                    return null;

                double factor = -1 * diagonals.ToList().ConvertAll(x => x.GetLength()).Max();
                if (double.IsNaN(factor) || System.Math.Abs(factor) < tolerance_Distance)
                    return null;

                List<Face2D> face2Ds = null;
                while (face2Ds == null && System.Math.Abs(factor) > tolerance_Area)
                {
                    face2Ds = face2D_HostPartition.Offset(factor, true, true, tolerance_Distance);
                    factor = factor / 2;
                }

                if (face2Ds == null || face2Ds.Count == 0)
                    return null;

                if (double.IsNaN(factor) || System.Math.Abs(factor) < tolerance_Distance)
                    return null;

                if (face2Ds.Count > 1)
                    face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                Face2D face2D = face2Ds.First();

                double offset = factor;

                double area_Current = face2D.GetArea();
                double difference = System.Math.Abs(area_Current - area_Target);
                while (!double.IsNaN(factor) && System.Math.Abs(factor) > tolerance_Distance && difference > tolerance_Area)
                {
                    face2Ds = face2D_HostPartition.Offset(offset, true, true, tolerance_Distance);

                    double factor_New = factor;
                    if (face2Ds != null && face2Ds.Count != 0)
                    {
                        if (face2Ds.Count > 1)
                            face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                        face2D = face2Ds.First();

                        area_Current = face2D.GetArea();
                        difference = System.Math.Abs(area_Current - area_Target);

                        factor_New = System.Math.Abs(factor_New);
                        if (area_Current > area_Target)
                            factor_New = -factor_New;

                        if (factor_New != factor)
                            factor = factor_New / 2;
                    }
                    else
                    {
                        factor = System.Math.Abs(factor_New / 2);
                    }

                    offset = offset + factor;
                }

                face3D = Geometry.Spatial.Query.Convert(plane, face2D);
            }

            if (face3D == null)
            {
                return null;
            }

            List<IOpening> openings = hostPartition.AddOpening(Create.Opening(openingType, face3D), tolerance_Distance);

            return openings?.FirstOrDefault();
        }

        public static IOpening AddOpening(this BuildingModel buildingModel, IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            if(TryAddOpening(buildingModel, opening, tolerance))
            {
                return opening;
            }

            return null;
        }
    }
}