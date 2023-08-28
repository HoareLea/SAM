using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AirHandlingUnit AirHandlingUnit(string name, bool frostCoil = false)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            AirHandlingUnit result = new AirHandlingUnit(name, 23, double.NaN);
            result.SetValue(AirHandlingUnitParameter.SummerHeatingCoil, false);

            List<ISimpleEquipment> simpleEquipments = null;

            HeatRecoveryUnit heatRecoveryUnit = new HeatRecoveryUnit("Heat Recovery Unit", 75, 0, 75, 0, double.NaN, double.NaN, double.NaN, double.NaN);
            MixingSection mixingSection = new MixingSection("Mixing Section");


            //Intake
            simpleEquipments = new List<ISimpleEquipment>();
            simpleEquipments.Add(new HeatingCoil("Frost Coil", double.NaN, double.NaN, 0.9, double.NaN));
            simpleEquipments.Add(new EmptySection());
            simpleEquipments.Add(new Filter("Intake Filter"));
            simpleEquipments.Add(mixingSection);
            simpleEquipments.Add(heatRecoveryUnit);
            result.AddSimpleEquipments(FlowClassification.Intake, simpleEquipments.ToArray());

            //Supply
            simpleEquipments = new List<ISimpleEquipment>();
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(new Fan("Supply Fan"));
            simpleEquipments.Add(new CoolingCoil("Cooling Coil", 6, 12, 0.9, double.NaN));
            simpleEquipments.Add(new Humidifier("Humidifier"));
            simpleEquipments.Add(new HeatingCoil("Heating Coil", double.NaN, double.NaN, 0.9, double.NaN));
            result.AddSimpleEquipments(FlowClassification.Supply, simpleEquipments.ToArray());

            //Exhaust
            simpleEquipments = new List<ISimpleEquipment>();
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(mixingSection);
            simpleEquipments.Add(new Silencer());
            result.AddSimpleEquipments(FlowClassification.Exhaust, simpleEquipments.ToArray());

            //Extract
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(new Filter("Extract Filter"));
            
            return result;
        }
    }
}