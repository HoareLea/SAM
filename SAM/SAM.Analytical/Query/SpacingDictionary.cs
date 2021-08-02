using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Point3D, List<Panel>> SpacingDictionary(this IEnumerable<Panel> panels, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if(panels == null)
            {
                return null;
            }    


            Dictionary<Face3D, Panel> dictionary = new Dictionary<Face3D, Panel>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                dictionary[face3D] = panel;
            }


            Dictionary<Point3D, List<Face3D>> dictionary_Face3D = Geometry.Spatial.Query.SpacingDictionary(dictionary.Keys, maxTolerance, minTolerance);
            if(dictionary == null)
            {
                return null;
            }

            Dictionary<Point3D, List<Panel>> result = new Dictionary<Point3D, List<Panel>>();
            foreach(KeyValuePair<Point3D, List<Face3D>> keyValuePair in dictionary_Face3D)
            {
                List<Panel> panels_Temp =  keyValuePair.Value.ConvertAll(x => dictionary[x]);
                if(panels_Temp != null || panels_Temp.Count != 0)
                {
                    result[keyValuePair.Key] = panels_Temp;
                }
            }

            return result;
        }
    }
}