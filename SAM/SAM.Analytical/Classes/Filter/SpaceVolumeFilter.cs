using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class SpaceVolumeFilter : NumberFilter, IAdjacencyClusterFilter
    {
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public SpaceVolumeFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public SpaceVolumeFilter(SpaceVolumeFilter spaceVolumeFilter)
            : base(spaceVolumeFilter)
        {

        }

        public SpaceVolumeFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (AdjacencyCluster == null)
            {
                return false;
            }

            Space space = jSAMObject as Space;
            if(space == null)
            {
                return false;
            }

            Shell shell = AdjacencyCluster.Shell(space);
            if(shell == null)
            {
                return false;
            }

            double volume = shell.Volume();
            if(double.IsNaN(volume))
            {
                return false;
            }


            return SAM.Core.Query.Compare(volume, Value, NumberComparisonType);
            
        }
    }
}