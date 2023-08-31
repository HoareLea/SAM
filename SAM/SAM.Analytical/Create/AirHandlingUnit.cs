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

            List<ISimpleEquipment> simpleEquipments = null;

            HeatRecoveryUnit heatRecoveryUnit = new HeatRecoveryUnit("Heat Recovery Unit", 76, 0, 76, 0, double.NaN, double.NaN, double.NaN, double.NaN);
            MixingSection mixingSection = new MixingSection("Mixing Section");


            //Intake
            simpleEquipments = new List<ISimpleEquipment>();
            if(frostCoil)
            {
                simpleEquipments.Add(new HeatingCoil("Frost Coil", double.NaN, double.NaN, 0.9, double.NaN));
                simpleEquipments.Add(new EmptySection());
            }
            simpleEquipments.Add(new Filter("Intake Filter"));
            simpleEquipments.Add(heatRecoveryUnit);
            result.AddSimpleEquipments(FlowClassification.Intake, simpleEquipments.ToArray());

            //Supply
            simpleEquipments = new List<ISimpleEquipment>();
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(mixingSection);
            simpleEquipments.Add(new CoolingCoil("Cooling Coil", 6, 12, 0.9, double.NaN));
            simpleEquipments.Add(new EmptySection());
            simpleEquipments.Add(new HeatingCoil("Heating Coil", 60, 30, 0.9, double.NaN, double.NaN));
            simpleEquipments.Add(new EmptySection());
            simpleEquipments.Add(new Humidifier("Humidifier"));
            simpleEquipments.Add(new Fan("Supply Fan"));
            result.AddSimpleEquipments(FlowClassification.Supply, simpleEquipments.ToArray());

            //Exhaust
            simpleEquipments = new List<ISimpleEquipment>();
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(new Silencer());
            result.AddSimpleEquipments(FlowClassification.Exhaust, simpleEquipments.ToArray());

            //Extract
            simpleEquipments.Add(heatRecoveryUnit);
            simpleEquipments.Add(new EmptySection());
            simpleEquipments.Add(new Fan("Extract Fan"));
            simpleEquipments.Add(new EmptySection());
            simpleEquipments.Add(new Filter("Extract Filter"));
            result.AddSimpleEquipments(FlowClassification.Extract, simpleEquipments.ToArray());

            return result;
        }
    }
}