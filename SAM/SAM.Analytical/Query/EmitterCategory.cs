namespace SAM.Analytical
{
    public static partial class Query
    {
        public static EmitterCategory EmitterCategory(this EmitterType emitterType)
        {
            if (emitterType == EmitterType.Undefined)
                return Analytical.EmitterCategory.Undefined;

            switch(emitterType)
            {
                case EmitterType.AirConditioning_Heating:
                case EmitterType.WarmAirHeater:
                case EmitterType.NaturalConvector:
                case EmitterType.RadiatorTypeConvector:
                case EmitterType.MultiColumnRadiator:
                case EmitterType.DoubleOrTrebleRadiator:
                case EmitterType.DoubleColumnRadiator:
                case EmitterType.SingleColumnRadiator:
                case EmitterType.FloorWarmingSystem:
                case EmitterType.BlockStorageHeater:
                case EmitterType.WallPanelHeater:
                case EmitterType.CeilingPanelHeater:
                case EmitterType.HighTemperatureRadiantSystem:
                    return Analytical.EmitterCategory.Heating;
                case EmitterType.AirConditioning_Cooling:
                case EmitterType.CeilingPanelCooler:
                case EmitterType.WallPanelCooler:
                case EmitterType.ChilledBeam:
                    return Analytical.EmitterCategory.Cooling;
                case EmitterType.TungstenCeilingLight:
                case EmitterType.TungstenTaskLight:
                case EmitterType.FluorescentCeilingLight:
                case EmitterType.FluorescentTaskLight:
                    return Analytical.EmitterCategory.Light;
                case EmitterType.Occupant:
                    return Analytical.EmitterCategory.Occupant;
                case EmitterType.EquipmentDistributed:
                case EmitterType.EquipmentDistributedOver:
                    return Analytical.EmitterCategory.Equipment;
            }


            return Analytical.EmitterCategory.Undefined;
        }
    }
}