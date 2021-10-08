using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static List<HostPartition> HostPartitions(this List<ISAMGeometry3D> geometry3Ds, double minArea = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            List<Face3D> face3Ds = Geometry.Spatial.Query.Face3Ds(geometry3Ds, tolerance_Distance);
            if (face3Ds == null)
                return null;

            List<HostPartition> result = new List<HostPartition>();
            foreach (Face3D face3D in face3Ds)
            {
                if (minArea != 0 && face3D.GetArea() < minArea)
                    continue;

                HostPartition hostPartition = HostPartition(face3D, null, tolerance_Angle);

                result.Add(hostPartition);
            }

            return result;
        }

        public static List<HostPartition> HostPartitions(this IEnumerable<HostPartition> hostPartitions, Plane plane, bool checkIntersection = true, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            if (hostPartitions == null || plane == null)
                return null;

            List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
            foreach (HostPartition hostPartition in hostPartitions)
            {
                Face3D face3D = hostPartition?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                closedPlanar3Ds.Add(face3D);
            }

            List<Polygon3D> polygon3Ds = Geometry.Spatial.Create.Polygon3Ds(closedPlanar3Ds, plane, checkIntersection, tolerance_Distance);
            if (polygon3Ds == null || polygon3Ds.Count == 0)
            {
                return null;
            }

            List<HostPartition> result = new List<HostPartition>();
            foreach (Polygon3D polygon3D in polygon3Ds)
            {
                if (polygon3D == null)
                {
                    continue;
                }

                Face3D face3D = new Face3D(polygon3D);

                HostPartition hostPartition = HostPartition(face3D, null, tolerance_Angle);
                if(hostPartition == null)
                {
                    continue;
                }

                result.Add(hostPartition);
            }

            return result;
        }
    
        public static List<HostPartition> HostPartitions(this Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            if (shell == null)
                return null;

            Shell shell_Temp = new Shell(shell);
            shell_Temp.OrientNormals(false, silverSpacing, tolerance_Distance);

            List<Face3D> face3Ds = shell_Temp.Face3Ds;
            if (face3Ds == null)
                return null;

            List<HostPartition> result = new List<HostPartition>();
            foreach(Face3D face3D in face3Ds)
            {
                HostPartition hostPartition = HostPartition(face3D, null, tolerance_Angle);
                if (hostPartition == null)
                {
                    continue;
                }

                result.Add(hostPartition);
            }

            return result;
        }
    }
}