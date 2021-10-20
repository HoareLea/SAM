using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IOpening> Split(this IOpening opening, IEnumerable<Face3D> face3Ds, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(opening == null || face3Ds == null)
            {
                return null;
            }

            Face3D face3D = opening.Face3D;
            if(face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            List<Face3D> face3Ds_Split = Geometry.Spatial.Query.Split(face3D, face3Ds, tolerance_Snap, tolerance_Angle, tolerance_Distance);
            if(face3Ds_Split == null)
            {
                face3Ds_Split = new List<Face3D>() { face3D};
            }

            List<IOpening> result = new List<IOpening>();
            foreach(Face3D face3D_Split in face3Ds_Split)
            {
                IOpening opening_Split = Create.Opening(opening.Type(), face3D_Split);
                if(opening_Split == null)
                {
                    continue;
                }

                result.Add(opening_Split);
            }

            return result;
        }
    }
}