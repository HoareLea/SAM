using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> FilterByElevation(this IEnumerable<Panel> panels, double elevation, out List<Panel> panels_Lower, out List<Panel> panels_Upper, double tolerance = Core.Tolerance.Distance)
        {
            panels_Lower = null;
            panels_Upper = null;

            if (panels == null)
                return null;

            panels_Lower = new List<Panel>();
            panels_Upper = new List<Panel>();

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                double min = panel.MinElevation();
                double max = panel.MaxElevation();

                if (min - tolerance <= elevation && max + tolerance >= elevation)
                {
                    if (System.Math.Abs(max - min) > tolerance && System.Math.Abs(max - elevation) < tolerance)
                    {
                        panels_Lower.Add(panel);
                        continue;
                    }


                    result.Add(panel);
                }
                else
                {
                    if (min >= elevation)
                        panels_Upper.Add(panel);
                    else
                        panels_Lower.Add(panel);
                }
            }

            return result;
        }

        public static List<Space> FilterByElevation(this IEnumerable<Space> spaces, double elevation, out List<Space> spaces_Lower, out List<Space> spaces_Upper, double tolerance = Core.Tolerance.Distance)
        {
            spaces_Lower = null;
            spaces_Upper = null;

            if (spaces == null)
                return null;

            spaces_Lower = new List<Space>();
            spaces_Upper = new List<Space>();

            List<Space> result = new List<Space>();
            foreach (Space space in spaces)
            {
                Geometry.Spatial.Point3D location = space?.Location;
                if (location == null)
                    continue;

                double z = location.Z;
                double difference = z - elevation;

                if (System.Math.Abs(difference) <= tolerance)
                    result.Add(space);
                else if (difference >= elevation)
                    spaces_Upper.Add(space);
                else
                    spaces_Lower.Add(space);
            }

            return result;
        }
    }
}