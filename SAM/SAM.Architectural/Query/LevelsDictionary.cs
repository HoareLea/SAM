using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static Dictionary<Level, List<T>> LevelsDictionary<T>(this List<T> face3DObjects, double tolerance = Core.Tolerance.MacroDistance) where T : Geometry.Object.Spatial.IFace3DObject
        {
            if (face3DObjects == null)
            {
                return null;
            }

            List<Level> levels = Create.Levels(face3DObjects, tolerance);
            if(levels == null)
            {
                return null;
            }

            Dictionary<Level, List<T>> result = new Dictionary<Level, List<T>>();
            foreach(T face3Dobject in face3DObjects)
            {
                if(face3Dobject == null)
                {
                    continue;
                }

                double elevation = face3Dobject.Face3D.GetBoundingBox().Min.Z;
                if (double.IsNaN(elevation))
                {
                    continue;
                }

                elevation = Core.Query.Round(elevation, tolerance);

                double distance = double.MaxValue;
                Level level = null;
                foreach (Level level_Temp in levels)
                {
                    double distance_Temp = System.Math.Abs(level_Temp.Elevation - elevation);

                    if (distance_Temp < distance)
                    {
                        distance = distance_Temp;
                        level = level_Temp;
                    }
                }

                if(level == null)
                {
                    continue;
                }

                if(!result.TryGetValue(level, out List<T> face3DObjects_Level))
                {
                    face3DObjects_Level = new List<T>();
                    result[level] = face3DObjects_Level;
                }

                face3DObjects_Level.Add(face3Dobject);
            }

            return result;
        }
    }
}