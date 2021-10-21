using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static List<Level> Levels<T>(this List<T> face3DObjects, double tolerance = Core.Tolerance.MacroDistance) where T : Geometry.Spatial.IFace3DObject
        {
            if (face3DObjects == null)
                return null;

            HashSet<double> elevations = new HashSet<double>();
            foreach(T face3DObject in face3DObjects)
            {
                if (face3DObject == null)
                    continue;

                double elevation = face3DObject.Face3D.GetBoundingBox().Min.Z;
                if (double.IsNaN(elevation))
                    continue;

                elevation = Core.Query.Round(elevation, tolerance);

                elevations.Add(elevation);
            }

            List<Level> result = new List<Level>();
            foreach(double elevation in elevations)
                result.Add(Level(elevation));

            return result;
        }
    }
}