using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an heat humidifier unit unit object in the analytical domain
    /// </summary>
    public abstract class ComplexEquipment : SAMObject, IComplexEquipment
    {
        protected ComplexEquipmentModel complexEquipmentModel;

        public ComplexEquipment(string name)
            : base(name)
        {

        }

        public ComplexEquipment(JObject jObject)
            : base(jObject)
        {

        }

        public ComplexEquipment(ComplexEquipment complexEquipment)
            : base(complexEquipment)
        {
            complexEquipmentModel = complexEquipment?.complexEquipmentModel == null ? null : complexEquipment?.complexEquipmentModel.Clone();
        }

        public ComplexEquipment(Guid guid, string name)
            : base(guid, name)
        {

        }

        public bool AddSimpleEquipments(FlowClassification flowClassification, params ISimpleEquipment[] simpleEquipments)
        {
            if(simpleEquipments == null || simpleEquipments.Length == 0)
            {
                return false;
            }

            if(complexEquipmentModel == null)
            {
                complexEquipmentModel = new ComplexEquipmentModel();
            }

            return complexEquipmentModel.AddRelations(flowClassification, simpleEquipments);
        }

        public bool InsertAfterSimpleEquipment(FlowClassification flowClassification, ISimpleEquipment simpleEquipment_ToBeInserted, ISimpleEquipment simpleEquipment)
        {
            if(complexEquipmentModel == null)
            {
                return false;
            }

            return complexEquipmentModel.InsertAfter(flowClassification, simpleEquipment_ToBeInserted, simpleEquipment);
        }

        public bool InsertBeforeSimpleEquipment(FlowClassification flowClassification, ISimpleEquipment simpleEquipment_ToBeInserted, ISimpleEquipment simpleEquipment)
        {
            if (complexEquipmentModel == null)
            {
                return false;
            }

            return complexEquipmentModel.InsertBefore(flowClassification, simpleEquipment_ToBeInserted, simpleEquipment);
        }

        public bool RemoveSimpleEquipment(ISimpleEquipment simpleEquipment)
        {
            if(simpleEquipment == null)
            {
                return false; 
            }

            if(complexEquipmentModel == null)
            {
                return false;
            }

            return complexEquipmentModel.Remove(simpleEquipment);
        }

        public List<ISimpleEquipment> GetSimpleEquipments(FlowClassification flowClassification, bool sort = true)
        {
            List<ISimpleEquipment> result = complexEquipmentModel?.GetSimpleEquipments(flowClassification);
            if(sort && result != null && result.Count > 1)
            {
                result = complexEquipmentModel.Sort(result, flowClassification, Direction.In);
            }

            return result;
        }

        public List<T> GetSimpleEquipments<T>(FlowClassification flowClassification) where T: ISimpleEquipment
        {
            return GetSimpleEquipments<T>(flowClassification, null);
        }

        public List<T> GetSimpleEquipments<T>() where T : ISimpleEquipment
        {
            return GetSimpleEquipments<T>();
        }

        public List<T> GetSimpleEquipments<T>(FlowClassification flowClassification, Func<T, bool> func) where T : ISimpleEquipment
        {
            List<ISimpleEquipment> simpleEquipments = complexEquipmentModel?.GetSimpleEquipments(flowClassification);
            if(simpleEquipments == null || simpleEquipments.Count == 0)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(ISimpleEquipment simpleEquipment in simpleEquipments)
            {
                if(!(simpleEquipment is T))
                {
                    continue;
                }

                T t = (T)simpleEquipment;

                if (func != null && !func.Invoke(t))
                {
                    continue;
                }

                result.Add(t);
            }

            return result;

        }

        public HashSet<FlowClassification> GetFlowClassifications()
        {
            return complexEquipmentModel?.GetFlowClassifications();
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("ComplexEquipmentModel"))
            {
                complexEquipmentModel = Core.Query.IJSAMObject<ComplexEquipmentModel>(jObject.Value<JObject>("ComplexEquipmentModel"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(complexEquipmentModel != null)
            {
                jObject.Add("ComplexEquipmentModel", complexEquipmentModel.ToJObject());
            }

            return jObject;
        }
    }
}
