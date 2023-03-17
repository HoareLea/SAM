using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class SpaceElevationFilter : NumberFilter, IAdjacencyClusterFilter
    {
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public SpaceElevationFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public SpaceElevationFilter(SpaceElevationFilter spaceElevationFilter)
            : base(spaceElevationFilter)
        {

        }

        public SpaceElevationFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool TryGetNumber(IJSAMObject jSAMObject, out double number)
        {
            number = double.NaN;
            if (AdjacencyCluster == null)
            {
                return false;
            }

            Space space = jSAMObject as Space;
            if (space == null)
            {
                return false;
            }

            Shell shell = AdjacencyCluster.Shell(space);
            if (shell == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return false;
            }

            number = boundingBox3D.Min.Z;
            return !double.IsNaN(number);
        }
    }
}