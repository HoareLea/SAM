using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class SpaceAreaFilter : NumberFilter, IAdjacencyClusterFilter
    {
        public double Offset { get; set; } = 0.01;
        
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public SpaceAreaFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public SpaceAreaFilter(SpaceAreaFilter spaceAreaFilter)
            : base(spaceAreaFilter)
        {

        }

        public SpaceAreaFilter(JObject jObject)
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

            number = shell.Area(Offset);
            return !double.IsNaN(number);
        }
    }
}