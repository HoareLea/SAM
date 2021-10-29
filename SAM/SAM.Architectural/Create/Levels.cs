using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static List<Level> Levels<T>(this List<T> face3DObjects, double tolerance = Core.Tolerance.MacroDistance) where T : Geometry.Spatial.IFace3DObject
        {
            return Levels(face3DObjects?.ConvertAll(x => x?.Face3D), tolerance);
        }

        public static List<Level> Levels(this List<Face3D> face3Ds, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (face3Ds == null)
                return null;

            HashSet<double> elevations = new HashSet<double>();
            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                double elevation = face3D.GetBoundingBox().Min.Z;
                if (double.IsNaN(elevation))
                    continue;

                elevation = Core.Query.Round(elevation, tolerance);

                elevations.Add(elevation);
            }

            List<Level> result = new List<Level>();
            foreach (double elevation in elevations)
                result.Add(Level(elevation));

            return result;
        }
    }
}