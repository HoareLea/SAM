using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        /// <summary>
        /// Removes LineStrings whicha are shorter than given tolerance and adjusts points of the rest LineStrings to avoid discontinuity 
        /// </summary>
        /// <param name="lineStrings">NTS LineStrings</param>
        /// <param name="tolerance"> Tolerance</param>
        public static void Tighten(this List<LineString> lineStrings, double tolerance = Core.Tolerance.Distance)
        {
            if (lineStrings == null || lineStrings.Count == 0)
                return;
            
            List<LineString> lineStrings_Short = lineStrings.FindAll(x => x.Length < tolerance && x.Coordinates.Length > 1);
            if (lineStrings_Short == null || lineStrings_Short.Count == 0)
                return;

            lineStrings.RemoveAll(x => lineStrings_Short.Contains(x));
            for (int i = 0; i < lineStrings.Count; i++)
            {
                LineString lineString = lineStrings[i];

                bool update = false;
                Coordinate[] coordinates = lineString.Coordinates;
                foreach (LineString lineString_Short in lineStrings_Short)
                {
                    Coordinate coordinate_Main = lineString_Short[0];
                    for (int j = 1; j < lineString_Short.Count; j++)
                    {
                        Coordinate coordinate_ToReplace = lineString_Short[j];
                        for (int k = 0; k < coordinates.Length; k++)
                        {
                            if (coordinates[k].Distance(coordinate_ToReplace) < tolerance)
                            {
                                update = true;
                                coordinates[k] = coordinate_Main;
                            }
                        }
                    }
                }

                if (update)
                    lineStrings[i] = new LineString(coordinates);
            }

        }
    }
}