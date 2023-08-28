using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ComplexEquipmentModel : RelationModel<ISimpleEquipment>, IJSAMObject
    {
        public ComplexEquipmentModel()
        {

        }

        public ComplexEquipmentModel(ComplexEquipmentModel complexEquipmentModel)
            :base(complexEquipmentModel)
        {

        }

        public ComplexEquipmentModel(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(ISimpleEquipment simpleEquipment)
        {
            Reference? reference = AddObject(simpleEquipment);

            return reference != null && reference.HasValue && reference.Value.IsValid();
        }

        public bool AddRelation(FlowClassification flowClassification, ISimpleEquipment simpleEquipment_Out, ISimpleEquipment simpleEquipment_In)
        {
            if (simpleEquipment_Out == null || simpleEquipment_In == null)
            {
                return false;
            }

            return AddRelation(flowClassification, Direction.Out, simpleEquipment_Out, simpleEquipment_In);
        }

        public bool AddRelation(FlowClassification flowClassification, Direction direction, ISimpleEquipment simpleEquipment_1, ISimpleEquipment simpleEquipment_2)
        {
            if(flowClassification == FlowClassification.Undefined || simpleEquipment_1 == null || simpleEquipment_2 == null)
            {
                return false;
            }

            string id = null;

            id = Query.Id(flowClassification, direction);
            if(string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            bool result = AddRelation(id, simpleEquipment_1, simpleEquipment_2) != null;
            if(!result)
            {
                return false;
            }

            id = Query.Id(flowClassification, direction.Opposite());

            return AddRelation(id, simpleEquipment_2, simpleEquipment_1) != null;
        }

        public bool AddRelations(FlowClassification flowClassification, params ISimpleEquipment[] simpleEquipments)
        {
            if(simpleEquipments == null || simpleEquipments.Length < 2)
            {
                return false;
            }

            return AddRelations(flowClassification, Direction.Out, simpleEquipments);
        }

        public bool AddRelations(FlowClassification flowClassification, Direction direction, params ISimpleEquipment[] simpleEquipments)
        {
            if (simpleEquipments == null || simpleEquipments.Length < 2)
            {
                return false;
            }

            bool result = false;
            for (int i=0; i < simpleEquipments.Length - 1; i++)
            {
                if(AddRelation(flowClassification, direction, simpleEquipments[i], simpleEquipments[i + 1]))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool Remove(ISimpleEquipment simpleEquipment)
        {
            return RemoveObject(simpleEquipment);
        }

        public bool Replace(ISimpleEquipment simpleEquipment_ToBeReplaced, ISimpleEquipment simpleEquipment)
        {
            return base.Replace(simpleEquipment_ToBeReplaced, simpleEquipment);
        }

        public bool InsertAfter(FlowClassification flowClassification, ISimpleEquipment simpleEquipment_ToBeInserted, ISimpleEquipment simpleEquipment)
        {
            if(simpleEquipment_ToBeInserted == null || simpleEquipment == null)
            {
                return false;
            }

            if(!Contains(simpleEquipment))
            {
                return false;
            }

            List<ISimpleEquipment> simpleEquipments = GetSimpleEquipments(flowClassification);
            if(simpleEquipments == null || simpleEquipments.Count == 0)
            {
                return false;
            }

            simpleEquipments = Sort(simpleEquipments, flowClassification, Direction.In);

            Reference reference = GetReference(simpleEquipment).Value;

            int index = simpleEquipments.FindIndex(x => GetReference(x).Value == reference);
            if(index == -1)
            {
                return false;
            }

            string id_In = Query.Id(flowClassification, Direction.In);
            if (string.IsNullOrWhiteSpace(id_In))
            {
                return false;
            }

            string id_Out = Query.Id(flowClassification, Direction.Out);
            if (string.IsNullOrWhiteSpace(id_Out))
            {
                return false;
            }

            RemoveRelations(id_In);
            RemoveRelations(id_Out);

            if (index == simpleEquipments.Count - 1)
            {
                simpleEquipments.Add(simpleEquipment_ToBeInserted);
            }
            else
            {
                index++;
                simpleEquipments.Insert(index, simpleEquipment_ToBeInserted);
            }

            return AddRelations(flowClassification, simpleEquipments.ToArray());
        }

        public bool InsertBefore(FlowClassification flowClassification, ISimpleEquipment simpleEquipment_ToBeInserted, ISimpleEquipment simpleEquipment)
        {
            if (simpleEquipment_ToBeInserted == null || simpleEquipment == null)
            {
                return false;
            }

            if (!Contains(simpleEquipment))
            {
                return false;
            }

            List<ISimpleEquipment> simpleEquipments = GetSimpleEquipments(flowClassification);
            if (simpleEquipments == null || simpleEquipments.Count == 0)
            {
                return false;
            }

            simpleEquipments = Sort(simpleEquipments, flowClassification, Direction.In);

            Reference reference = GetReference(simpleEquipment).Value;

            int index = simpleEquipments.FindIndex(x => GetReference(x).Value == reference);
            if (index == -1)
            {
                return false;
            }

            string id_In = Query.Id(flowClassification, Direction.In);
            if (string.IsNullOrWhiteSpace(id_In))
            {
                return false;
            }

            string id_Out = Query.Id(flowClassification, Direction.Out);
            if (string.IsNullOrWhiteSpace(id_Out))
            {
                return false;
            }

            RemoveRelations(id_In);
            RemoveRelations(id_Out);

            simpleEquipments.Insert(index, simpleEquipment_ToBeInserted);

            return AddRelations(flowClassification, simpleEquipments.ToArray());
        }

        public List<ISimpleEquipment> GetSimpleEquipments(FlowClassification flowClassification)
        {
            return GetObjects(x => HasFlowClassification(x, flowClassification));
        }

        public List<T> GetSimpleEquipments<T>(FlowClassification flowClassification) where T: ISimpleEquipment
        {
            List<ISimpleEquipment> simpleEquipments = GetSimpleEquipments(flowClassification);
            if(simpleEquipments == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(ISimpleEquipment simpleEquipment in simpleEquipments)
            {
                if(simpleEquipment is T)
                {
                    result.Add((T)simpleEquipment);
                }
            }

            return result;
        }

        public List<ISimpleEquipment> GetSimpleEquipments(ISimpleEquipment simpleEquipment, FlowClassification flowClassification, Direction direction)
        {
            if(simpleEquipment == null)
            {
                return null;
            }

            string id = Query.Id(flowClassification, direction);
            if(string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return GetRelatedObjects_1(simpleEquipment, id);
        }

        public HashSet<FlowClassification> GetFlowClassifications(ISimpleEquipment simpleEquipment)
        {
            RelationCollection relationCollection = GetRelations(simpleEquipment);
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
            List<ISimpleEquipment> simpleEquipments = GetObjects();
            if(simpleEquipments == null)
            {
                return null;
            }

            HashSet<FlowClassification> result = new HashSet<FlowClassification>();
            foreach(ISimpleEquipment simpleEquipment in simpleEquipments)
            {
                HashSet<FlowClassification> flowClassifications = GetFlowClassifications(simpleEquipment);
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

        public bool HasFlowClassification(ISimpleEquipment simpleEquipment, FlowClassification flowClassification)
        {
            HashSet<FlowClassification> flowClassifications = GetFlowClassifications(simpleEquipment);
            if(flowClassifications == null || flowClassifications.Count == 0)
            {
                return false;
            }

            return flowClassifications.Contains(flowClassification);

        }

        public HashSet<Direction> GetDirections(ISimpleEquipment simpleEquipment, FlowClassification flowClassification)
        {
            RelationCollection relationCollection = GetRelations_1(simpleEquipment);
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

        public override Reference? GetReference(ISimpleEquipment @object)
        {
            return @object.Guid.ToString("N");
        }

        /// <summary>
        /// Gets list of terminal simple equipments (first and last)
        /// </summary>
        /// <param name="flowClassification">Flow Calssification</param>
        /// <returns></returns>
        public List<T> TerminalSimpleEquipments<T>(FlowClassification flowClassification) where T: ISimpleEquipment
        {
            List<ISimpleEquipment> simpleEquipments = GetSimpleEquipments(flowClassification);
            if (simpleEquipments == null)
            {
                return null;
            }

            if(simpleEquipments.Count < 2)
            {
                return simpleEquipments.FindAll(x => x is T).ConvertAll(x => (T)x);
            }

            simpleEquipments = Sort(simpleEquipments, flowClassification, Direction.In);

            ISimpleEquipment simpleEquipment = default;

            List<T> result = new List<T>();

            simpleEquipment = simpleEquipments.First();
            if(simpleEquipment is T)
            {
                result.Add((T)simpleEquipment);
            }

            simpleEquipment = simpleEquipments.Last();
            if (simpleEquipment is T)
            {
                result.Add((T)simpleEquipment);
            }

            return result;
        }

        public List<ISimpleEquipment> Sort(IEnumerable<ISimpleEquipment> simpleEquipments, FlowClassification flowClassification, Direction direction)
        {
            if(simpleEquipments == null)
            {
                return null;
            }

            List<ISimpleEquipment> result = new List<ISimpleEquipment>();
            if(simpleEquipments.Count() == 0)
            {
                return result;
            }

            List<ISimpleEquipment> simpleEquipments_Temp = new List<ISimpleEquipment>(simpleEquipments);

            while (simpleEquipments_Temp != null && simpleEquipments_Temp.Count > 0)
            {
                ISimpleEquipment simpleEquipment_Current = null;
                foreach (ISimpleEquipment simpleEquipment_Temp in simpleEquipments_Temp)
                {
                    HashSet<Direction> directions = GetDirections(simpleEquipment_Temp, flowClassification);
                    if (directions != null && directions.Count == 1 && directions.Contains(direction.Opposite()))
                    {
                        simpleEquipment_Current = simpleEquipment_Temp;
                        break;
                    }
                }

                if(simpleEquipment_Current == null)
                {
                    simpleEquipment_Current = simpleEquipments_Temp[0];
                }

                simpleEquipments_Temp.Remove(simpleEquipment_Current);
                result.Add(simpleEquipment_Current);

                List<ISimpleEquipment> simpleEquipments_Related = GetSimpleEquipments(simpleEquipment_Current, flowClassification, direction);
                while(simpleEquipments_Related != null && simpleEquipments_Related.Count > 0)
                {
                    simpleEquipment_Current = simpleEquipments_Related[0];
                    simpleEquipments_Temp.Remove(simpleEquipment_Current);
                    result.Add(simpleEquipment_Current);
                    simpleEquipments_Related = GetSimpleEquipments(simpleEquipment_Current, flowClassification, direction);
                    if(simpleEquipments_Related != null && simpleEquipments_Related.Count > 0)
                    {
                        foreach (ISimpleEquipment simpleEquipment in result)
                        {
                            simpleEquipments_Related.Remove(simpleEquipment);
                        }
                    }
                }
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("SimpleEquipments"))
            {
                JArray jArray = jObject.Value<JArray>("SimpleEquipments");
                if(jArray != null)
                {
                    foreach(JObject jObject_SimpleEquipment in jArray)
                    {
                        if(jObject_SimpleEquipment == null)
                        {
                            continue;
                        }

                        ISimpleEquipment simpleEquipment = Core.Query.IJSAMObject<ISimpleEquipment>(jObject_SimpleEquipment);
                        if(simpleEquipment != null)
                        {
                            Add(simpleEquipment);
                        }
                    }
                }
            }

            if(jObject.ContainsKey("Relations"))
            {
                JArray jArray = jObject.Value<JArray>("Relations");
                if (jArray != null)
                {
                    foreach (JObject jObject_Relation in jArray)
                    {
                        if (jObject_Relation == null)
                        {
                            continue;
                        }

                        Relation relation = Core.Query.IJSAMObject<Relation>(jObject_Relation);
                        if (relation != null)
                        {
                            AddRelation(relation);
                        }
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            List<ISimpleEquipment> simpleEquipments =  GetObjects();
            if(simpleEquipments != null)
            {
                JArray jArray = new JArray();
                foreach(ISimpleEquipment simpleEquipment in simpleEquipments)
                {
                    if(simpleEquipment == null)
                    {
                        continue;
                    }

                    jArray.Add(simpleEquipment.ToJObject());
                }

                result.Add("SimpleEquipments", jArray);
            }

            RelationCollection relationCollection = GetRelations();
            if(relationCollection != null)
            {
                JArray jArray = new JArray();
                foreach(Relation relation in relationCollection)
                {
                    if(relation == null)
                    {
                        continue;
                    }

                    jArray.Add(relation.ToJObject());
                }
                result.Add("Relations", jArray);

            }

            return result;
        }
    }
}
