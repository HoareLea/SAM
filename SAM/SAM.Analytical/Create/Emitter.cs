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

            double radiantProportion = double.NaN;
            double viewCoefficient = double.NaN;
            System.Guid guid = System.Guid.Empty;
            
            switch(emitterType)
            {
                case EmitterType.WarmAirHeater:
                    radiantProportion = 0;
                    viewCoefficient = double.NaN;
                    guid = System.Guid.Parse("2f47bb76-6703-4c4a-82f6-f6492815720b");
                    break;
                case EmitterType.AirConditioning_Heating:
                    radiantProportion = 0;
                    viewCoefficient = double.NaN;
                    guid = System.Guid.Parse("d20f238e-4f6e-40bd-87bf-0cceb2965ba1");
                    break;
                case EmitterType.NaturalConvector:
                    radiantProportion = 0.1;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("93687c29-878c-45fb-b0cb-25ecb4421b5e");
                    break;
                case EmitterType.RadiatorTypeConvector:
                    radiantProportion = 0.1;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("54aff72d-3a97-43ed-846c-2a9d3d784ad7");
                    break;
                case EmitterType.MultiColumnRadiator:
                    radiantProportion = 0.2;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("ee45aea3-227e-4fe8-acd0-0c54843756d1");
                    break;
                case EmitterType.DoubleOrTrebleRadiator:
                    radiantProportion = 0.3;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("d53f7163-efb2-4522-8e8a-67959c91c161");
                    break;
                case EmitterType.DoubleColumnRadiator:
                    radiantProportion = 0.3;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("68dd981-ec30-4167-bb64-b347ffe7db66");
                    break;
                case EmitterType.SingleColumnRadiator:
                    radiantProportion = 0.5;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("21515fba-7799-4aa9-be7c-7f8e53da1527");
                    break;
                case EmitterType.FloorWarmingSystem:
                    radiantProportion = 0.5;
                    viewCoefficient = 0.26;
                    guid = System.Guid.Parse("c8b43329-64df-463e-9e53-2f14f42e60a0");
                    break;
                case EmitterType.BlockStorageHeater:
                    radiantProportion = 0.15;
                    viewCoefficient = 0.248;
                    guid = System.Guid.Parse("3a105d59-cbd6-42ba-b8f4-ffd34f837e35");
                    break;
                case EmitterType.WallPanelHeater:
                    radiantProportion = 0.67;
                    viewCoefficient = 0.496;
                    guid = System.Guid.Parse("b4277457-fd58-403d-bbb6-9b25dcfc8eab");
                    break;
                case EmitterType.CeilingPanelHeater:
                    radiantProportion = 0.67;
                    viewCoefficient = 0.519;
                    guid = System.Guid.Parse("8a215cb4-7381-40fe-aae6-f74b7b8f6cd5");
                    break;
                case EmitterType.HighTemperatureRadiantSystem:
                    radiantProportion = 0.9;
                    viewCoefficient = 0.816;
                    guid = System.Guid.Parse("162af7d2-8f97-4070-a586-cee7259219f2");
                    break;
                case EmitterType.AirConditioning_Cooling:
                    radiantProportion = 0;
                    viewCoefficient = double.NaN;
                    guid = System.Guid.Parse("8e879b59-daf9-40c0-96d6-42c9b3431f2c");
                    break;
                case EmitterType.WallPanelCooler:
                    radiantProportion = 0.58;
                    viewCoefficient = 0.496;
                    guid = System.Guid.Parse("6f990638-08a5-485c-a08b-84a2e44189a4");
                    break;
                case EmitterType.CeilingPanelCooler:
                    radiantProportion = 0.58;
                    viewCoefficient = 0.519;
                    guid = System.Guid.Parse("8ae22257-8674-4c91-9ca0-31878fbccc22");
                    break;
                case EmitterType.ChilledBeam:
                    radiantProportion = 0.15;
                    viewCoefficient = 0.519;
                    guid = System.Guid.Parse("7b1ddb91-7401-4a50-9a10-a763e748fb6b");
                    break;
                case EmitterType.TungstenCeilingLight:
                    radiantProportion = 0.8;
                    viewCoefficient = 0.509;
                    guid = System.Guid.Parse("8eace2a3-5c17-477e-b6ce-25023ec19731");
                    break;
                case EmitterType.TungstenTaskLight:
                    radiantProportion = 0.8;
                    viewCoefficient = 0.182;
                    guid = System.Guid.Parse("191965ec-1a63-4d07-a3ad-fe32a99ccc30");
                    break;
                case EmitterType.FluorescentCeilingLight:
                    radiantProportion = 0.48;
                    viewCoefficient = 0.490;
                    guid = System.Guid.Parse("31b19b11-2190-448c-abc3-9770751fc4d1");
                    break;
                case EmitterType.FluorescentTaskLight:
                    radiantProportion = 0.48;
                    viewCoefficient = 0.175;
                    guid = System.Guid.Parse("613c31b8-ccd1-4fcd-9a8c-abdc47e263b3");
                    break;
                case EmitterType.Occupant:
                    radiantProportion = 0.2;
                    viewCoefficient = 0.227;
                    guid = System.Guid.Parse("59561a2f-96c1-4444-b944-774e8ba18e58");
                    break;
                case EmitterType.EquipmentDistributed:
                    radiantProportion = 0.1;
                    viewCoefficient = 0.124;
                    guid = System.Guid.Parse("c72898b5-d262-46cb-9bf2-b782cbf8a56b");
                    break;
                case EmitterType.EquipmentDistributedOver:
                    radiantProportion = 0.1;
                    viewCoefficient = 0.372;
                    guid = System.Guid.Parse("1c2a9d04-fe4f-4883-a4b4-6ded12b468e5");
                    break;
            }

            if (guid == System.Guid.Empty)
                return null;

            return new Emitter(guid, emitterType.Text(), emitterType.EmitterCategory(), radiantProportion, viewCoefficient);
        }
    }
}