using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetEnums(string id, out FlowClassification flowClassification, out Direction direction)
        {
            flowClassification = FlowClassification.Undefined;
            direction = Direction.Undefined;

            if(string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            foreach(FlowClassification flowClassification_Temp in System.Enum.GetValues(typeof(FlowClassification)))
            {
                if(id.StartsWith(flowClassification_Temp.Description()))
                {
                    flowClassification = flowClassification_Temp;
                    break;
                }
            }

            foreach (Direction direction_Temp in System.Enum.GetValues(typeof(Direction)))
            {
                if (id.EndsWith(direction_Temp.Description()))
                {
                    direction = direction_Temp;
                    break;
                }
            }

            return flowClassification != FlowClassification.Undefined && direction != Direction.Undefined;
        }
    }
}