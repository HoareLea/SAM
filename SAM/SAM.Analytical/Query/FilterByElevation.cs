using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    { 
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