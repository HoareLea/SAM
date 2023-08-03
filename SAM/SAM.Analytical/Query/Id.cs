using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Id(this FlowClassification flowClassification, Direction direction)
        {
            if(flowClassification == FlowClassification.Undefined || direction == Direction.Undefined)
            {
                return null;
            }

            return string.Format("{0} {1}", flowClassification.Description(), direction.Description());
        }
    }
}