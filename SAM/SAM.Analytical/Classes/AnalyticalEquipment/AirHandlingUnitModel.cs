using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class AirHandlingUnitModel : RelationModel<IAirHandlingUnitComponent>
    {
        public AirHandlingUnitModel()
        {

        }

        public bool Add(IAirHandlingUnitComponent airHandlingUnitComponent)
        {
            Reference? reference = AddObject(airHandlingUnitComponent);

            return reference != null && reference.HasValue && reference.Value.IsValid();
        }

        public bool AddRelation(FlowClassification flowClassification, IAirHandlingUnitComponent airHandlingUnitComponent_Out, IAirHandlingUnitComponent airHandlingUnitComponent_In)
        {
            if (airHandlingUnitComponent_Out == null || airHandlingUnitComponent_In == null)
            {
                return false;
            }

            return AddRelation(flowClassification, Direction.Out, airHandlingUnitComponent_Out, airHandlingUnitComponent_In);
        }

        public bool AddRelation(FlowClassification flowClassification, Direction direction, IAirHandlingUnitComponent airHandlingUnitComponent_1, IAirHandlingUnitComponent airHandlingUnitComponent_2)
        {
            if(flowClassification == FlowClassification.Undefined || airHandlingUnitComponent_1 == null || airHandlingUnitComponent_2 == null)
            {
                return false;
            }

            string id = null;

            id = Query.Id(flowClassification, direction);
            if(string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            bool result = AddRelation(id, airHandlingUnitComponent_1, airHandlingUnitComponent_2) != null;
            if(!result)
            {
                return false;
            }

            id = Query.Id(flowClassification, direction.Opposite());

            return AddRelation(id, airHandlingUnitComponent_2, airHandlingUnitComponent_1) != null;
        }

        public bool AddRelations(FlowClassification flowClassification, params IAirHandlingUnitComponent[] airHandlingUnitComponents)
        {
            if(airHandlingUnitComponents == null || airHandlingUnitComponents.Length < 2)
            {
                return false;
            }

            return AddRelations(flowClassification, Direction.Out, airHandlingUnitComponents);
        }

        public bool AddRelations(FlowClassification flowClassification, Direction direction, params IAirHandlingUnitComponent[] airHandlingUnitComponents)
        {
            if (airHandlingUnitComponents == null || airHandlingUnitComponents.Length < 2)
            {
                return false;
            }

            bool result = false;
            for (int i=0; i < airHandlingUnitComponents.Length - 1; i++)
            {
                if(AddRelation(flowClassification, direction, airHandlingUnitComponents[i], airHandlingUnitComponents[i + 1]))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool Remove(IAirHandlingUnitComponent airHandlingUnitComponent)
        {
            return RemoveObject(airHandlingUnitComponent);
        }

        public bool Replace(IAirHandlingUnitComponent airHandlingUnitComponent_ToBeReplaced, IAirHandlingUnitComponent airHandlingUnitComponent)
        {
            return base.Replace(airHandlingUnitComponent_ToBeReplaced, airHandlingUnitComponent);
        }

        public List<IAirHandlingUnitComponent> GetAirHandlingUnitComponents(FlowClassification flowClassification)
        {
            return GetObjects(x => HasFlowClassification(x, flowClassification));
        }

        public List<IAirHandlingUnitComponent> GetAirHandlingUnitComponents(IAirHandlingUnitComponent airHandlingUnitComponent, FlowClassification flowClassification, Direction direction)
        {
            if(airHandlingUnitComponent == null)
            {
                return null;
            }

            string id = Query.Id(flowClassification, direction);
            if(string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return GetRelatedObjects(airHandlingUnitComponent, id);
        }

        public HashSet<FlowClassification> GetFlowClassifications(IAirHandlingUnitComponent airHandlingUnitComponent)
        {
            RelationCollection relationCollection = GetRelations(airHandlingUnitComponent);
            if(relationCollection == null)
            {
                return null;
            }

            HashSet<FlowClassification> result = new HashSet<FlowClassification>();

            foreach (Relation relation in relationCollection)
            {
                if (!Query.TryGetEnums(relation.Id, out FlowClassification flowClassification, out Direction direction))
                {
                    continue;
                }

                result.Add(flowClassification);
            }

            return result;
        }

        public HashSet<FlowClassification> GetFlowClassifications()
        {
            List<IAirHandlingUnitComponent> airHandlingUnitComponents = GetObjects();
            if(airHandlingUnitComponents == null)
            {
                return null;
            }

            HashSet<FlowClassification> result = new HashSet<FlowClassification>();
            foreach(IAirHandlingUnitComponent airHandlingUnitComponent in airHandlingUnitComponents)
            {
                HashSet<FlowClassification> flowClassifications = GetFlowClassifications(airHandlingUnitComponent);
                if(flowClassifications == null || flowClassifications.Count == 0)
                {
                    continue;
                }

                foreach(FlowClassification flowClassification in flowClassifications)
                {
                    result.Add(flowClassification);
                }
            }

            return result;
        }

        public bool HasFlowClassification(IAirHandlingUnitComponent airHandlingUnitComponent, FlowClassification flowClassification)
        {
            HashSet<FlowClassification> flowClassifications = GetFlowClassifications(airHandlingUnitComponent);
            if(flowClassifications == null || flowClassifications.Count == 0)
            {
                return false;
            }

            return flowClassifications.Contains(flowClassification);

        }

        public HashSet<Direction> GetDirections(IAirHandlingUnitComponent airHandlingUnitComponent, FlowClassification flowClassification)
        {
            RelationCollection relationCollection = GetRelations_1(airHandlingUnitComponent);
            if (relationCollection == null)
            {
                return null;
            }

            HashSet<Direction> result = new HashSet<Direction>();

            foreach (Relation relation in relationCollection)
            {
                if (!Query.TryGetEnums(relation.Id, out FlowClassification flowClassification_Temp, out Direction direction))
                {
                    continue;
                }

                if(flowClassification_Temp != flowClassification)
                {
                    continue;
                }


                result.Add(direction);
            }

            return result;
        }

        public override Reference? GetReference(IAirHandlingUnitComponent @object)
        {
            return @object.Guid.ToString("N");
        }

        public List<IAirHandlingUnitComponent> Sort(IEnumerable<IAirHandlingUnitComponent> airHandlingUnitComponents, FlowClassification flowClassification, Direction direction)
        {
            if(airHandlingUnitComponents == null)
            {
                return null;
            }

            List<IAirHandlingUnitComponent> result = new List<IAirHandlingUnitComponent>();
            if(airHandlingUnitComponents.Count() == 0)
            {
                return result;
            }

            List<IAirHandlingUnitComponent> airHandlingUnitComponents_Temp = new List<IAirHandlingUnitComponent>(airHandlingUnitComponents);

            while (airHandlingUnitComponents_Temp != null && airHandlingUnitComponents_Temp.Count > 0)
            {
                IAirHandlingUnitComponent airHandlingUnitComponent_Current = null;
                foreach (IAirHandlingUnitComponent airHandlingUnitComponent_Temp in airHandlingUnitComponents_Temp)
                {
                    HashSet<Direction> directions = GetDirections(airHandlingUnitComponent_Temp, flowClassification);
                    if (directions != null && directions.Count == 1 && directions.Contains(direction.Opposite()))
                    {
                        airHandlingUnitComponent_Current = airHandlingUnitComponent_Temp;
                        break;
                    }
                }

                if(airHandlingUnitComponent_Current == null)
                {
                    airHandlingUnitComponent_Current = airHandlingUnitComponents_Temp[0];
                }

                airHandlingUnitComponents_Temp.Remove(airHandlingUnitComponent_Current);
                result.Add(airHandlingUnitComponent_Current);

                List<IAirHandlingUnitComponent> airHandlingUnitComponents_Related = GetAirHandlingUnitComponents(airHandlingUnitComponent_Current, flowClassification, direction);
                while(airHandlingUnitComponents_Related != null && airHandlingUnitComponents_Related.Count > 0)
                {
                    airHandlingUnitComponent_Current = airHandlingUnitComponents_Related[0];
                    airHandlingUnitComponents_Temp.Remove(airHandlingUnitComponent_Current);
                    result.Add(airHandlingUnitComponent_Current);
                    airHandlingUnitComponents_Related = GetAirHandlingUnitComponents(airHandlingUnitComponent_Current, flowClassification, direction);
                    if(airHandlingUnitComponents_Related != null && airHandlingUnitComponents_Related.Count > 0)
                    {
                        foreach (IAirHandlingUnitComponent airHandlingUnitComponent in result)
                        {
                            airHandlingUnitComponents_Related.Remove(airHandlingUnitComponent);
                        }
                    }
                }
            }

            return result;
        }
    }
}
