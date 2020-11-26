namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// Creates Emitter based on https://www.edsl.net/htmlhelp/Building_Simulator/ [Radiant Property Parameters]
        /// </summary>
        /// <param name="emitterType">EmitterType</param>
        /// <returns>Emitter</returns>
        public static Emitter Emitter(this EmitterType emitterType)
        {
            if (emitterType == EmitterType.Undefined)
                return null;

            switch(emitterType)
            {
                case EmitterType.WarmAirHeater:
                    return new Emitter(System.Guid.Parse("2f47bb76-6703-4c4a-82f6-f6492815720b"), emitterType.Text(), 0, double.NaN);
                case EmitterType.AirConditioning_Heating:
                    return new Emitter(System.Guid.Parse("d20f238e-4f6e-40bd-87bf-0cceb2965ba1"), emitterType.Text(), 0, double.NaN);
                case EmitterType.NaturalConvector:
                    return new Emitter(System.Guid.Parse("93687c29-878c-45fb-b0cb-25ecb4421b5e"), emitterType.Text(), 0.1, 0.248);
                case EmitterType.RadiatorTypeConvector:
                    return new Emitter(System.Guid.Parse("54aff72d-3a97-43ed-846c-2a9d3d784ad7"), emitterType.Text(), 0.1, 0.248);
                case EmitterType.MultiColumnRadiator:
                    return new Emitter(System.Guid.Parse("ee45aea3-227e-4fe8-acd0-0c54843756d1"), emitterType.Text(), 0.2, 0.248);
                case EmitterType.DoubleOrTrebleRadiator:
                    return new Emitter(System.Guid.Parse("d53f7163-efb2-4522-8e8a-67959c91c161"), emitterType.Text(), 0.3, 0.248);
                case EmitterType.DoubleColumnRadiator:
                    return new Emitter(System.Guid.Parse("f68dd981-ec30-4167-bb64-b347ffe7db66"), emitterType.Text(), 0.3, 0.248);
                case EmitterType.SingleColumnRadiator:
                    return new Emitter(System.Guid.Parse("21515fba-7799-4aa9-be7c-7f8e53da1527"), emitterType.Text(), 0.5, 0.248);
                case EmitterType.FloorWarmingSystem:
                    return new Emitter(System.Guid.Parse("c8b43329-64df-463e-9e53-2f14f42e60a0"), emitterType.Text(), 0.5, 0.26);
                case EmitterType.BlockStorageHeater:
                    return new Emitter(System.Guid.Parse("3a105d59-cbd6-42ba-b8f4-ffd34f837e35"), emitterType.Text(), 0.15, 0.248);
                case EmitterType.WallPanelHeater:
                    return new Emitter(System.Guid.Parse("b4277457-fd58-403d-bbb6-9b25dcfc8eab"), emitterType.Text(), 0.67, 0.496);
                case EmitterType.CeilingPanelHeater:
                    return new Emitter(System.Guid.Parse("8a215cb4-7381-40fe-aae6-f74b7b8f6cd5"), emitterType.Text(), 0.67, 0.519);
                case EmitterType.HighTemperatureRadiantSystem:
                    return new Emitter(System.Guid.Parse("162af7d2-8f97-4070-a586-cee7259219f2"), emitterType.Text(), 0.9, 0.816);
                case EmitterType.AirConditioning_Cooling:
                    return new Emitter(System.Guid.Parse("8e879b59-daf9-40c0-96d6-42c9b3431f2c"), emitterType.Text(), 0, double.NaN);
                case EmitterType.WallPanelCooler:
                    return new Emitter(System.Guid.Parse("6f990638-08a5-485c-a08b-84a2e44189a4"), emitterType.Text(), 0.58, 0.496);
                case EmitterType.CeilingPanelCooler:
                    return new Emitter(System.Guid.Parse("8ae22257-8674-4c91-9ca0-31878fbccc22"), emitterType.Text(), 0.58, 0.519);
                case EmitterType.ChilledBeam:
                    return new Emitter(System.Guid.Parse("7b1ddb91-7401-4a50-9a10-a763e748fb6b"), emitterType.Text(), 0.15, 0.519);
                case EmitterType.TungstenCeilingLight:
                    return new Emitter(System.Guid.Parse("8eace2a3-5c17-477e-b6ce-25023ec19731"), emitterType.Text(), 0.8, 0.509);
                case EmitterType.TungstenTaskLight:
                    return new Emitter(System.Guid.Parse("191965ec-1a63-4d07-a3ad-fe32a99ccc30"), emitterType.Text(), 0.8, 0.182);
                case EmitterType.FluorescentCeilingLight:
                    return new Emitter(System.Guid.Parse("31b19b11-2190-448c-abc3-9770751fc4d1"), emitterType.Text(), 0.48, 0.490);
                case EmitterType.FluorescentTaskLight:
                    return new Emitter(System.Guid.Parse("613c31b8-ccd1-4fcd-9a8c-abdc47e263b3"), emitterType.Text(), 0.48, 0.175);
                case EmitterType.Occupant:
                    return new Emitter(System.Guid.Parse("59561a2f-96c1-4444-b944-774e8ba18e58"), emitterType.Text(), 0.20, 0.227);
                case EmitterType.EquipmentDistributed:
                    return new Emitter(System.Guid.Parse("c72898b5-d262-46cb-9bf2-b782cbf8a56b"), emitterType.Text(), 0.10, 0.124);
                case EmitterType.EquipmentDistributedOver:
                    return new Emitter(System.Guid.Parse("1c2a9d04-fe4f-4883-a4b4-6ded12b468e5"), emitterType.Text(), 0.10, 0.372);
            }


            return null;
        }
    }
}