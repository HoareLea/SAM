using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<IAirMovementObject> AddAirMovementObjects(this AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetMechanicalSystems<VentilationSystem>();
            if (ventilationSystems == null)
            {
                return null;
            }

            List<AirHandlingUnit> airHandlingUnits_All = adjacencyCluster.GetObjects<AirHandlingUnit>();
            if(airHandlingUnits_All == null)
            {
                return null;
            }

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            List<IAirMovementObject> result = new List<IAirMovementObject>();

            List<AirHandlingUnit> airHandlingUnits = new List<AirHandlingUnit>(); 
            foreach(VentilationSystem ventilationSystem in ventilationSystems)
            {
                if(ventilationSystem == null)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem);
                if(spaces == null || spaces.Count == 0)
                {
                    continue;
                }

                if(ventilationSystem.TryGetValue(VentilationSystemParameter.SupplyUnitName, out string supplyName))
                {
                    AirHandlingUnit airHandlingUnit = airHandlingUnits_All.Find(x => x.Name == supplyName);
                    if(airHandlingUnit != null)
                    {
                        if(airHandlingUnits.Find(x => x.Guid == airHandlingUnit.Guid) == null)
                        {
                            airHandlingUnits.Add(airHandlingUnit);
                        }
                        
                        ObjectReference objectReference_AirHandlingUnit = new ObjectReference(airHandlingUnit);

                        foreach (Space space in spaces)
                        {
                            double airflow = space.CalculatedSupplyAirFlow();

                            Profile profile = space.InternalCondition?.GetProfile(ProfileType.Ventilation, profileLibrary);

                            ObjectReference objectReference_Space = new ObjectReference(space);

                            SpaceAirMovement spaceAirMovement = null;

                            spaceAirMovement = profile == null ? new SpaceAirMovement(space.Name, airflow, objectReference_AirHandlingUnit.ToString(), objectReference_Space.ToString()) : new SpaceAirMovement(space.Name, airflow, profile, objectReference_AirHandlingUnit.ToString(), objectReference_Space.ToString());
                            adjacencyCluster.AddObject(spaceAirMovement);
                            result.Add(spaceAirMovement);

                            adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                            adjacencyCluster.AddRelation(spaceAirMovement, space);

                            spaceAirMovement = profile == null ? new SpaceAirMovement(space.Name, airflow, objectReference_Space.ToString(), null) : new SpaceAirMovement(space.Name, airflow, profile, objectReference_Space.ToString(), null);
                            adjacencyCluster.AddObject(spaceAirMovement);

                            adjacencyCluster.AddRelation(spaceAirMovement, space);
                            result.Add(spaceAirMovement);
                        }
                    }
                }
            }

            List<double> densities = new List<double>() { FluidProperty.Air.Density };
            List<double> humidifications = new List<double>() { 100 };
            List<double> dehumidifications = new List<double>() { 0 };

            foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
            {
                Profile profile_Heating = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Heating), ProfileType.Heating, new double[] { airHandlingUnit.WinterSupplyTemperature });
                Profile profile_Cooling = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Cooling), ProfileType.Cooling, new double[] { airHandlingUnit.SummerSupplyTemperature });


                Profile density = new Profile(string.Format("{0} Air Density", airHandlingUnit.Name), densities);
                Profile humidification = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Humidification), ProfileType.Humidification, humidifications);
                Profile dehumidification = new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Dehumidification), ProfileType.Dehumidification, dehumidifications);

                AirHandlingUnitAirMovement airHandlingUnitAirMovement = new AirHandlingUnitAirMovement(airHandlingUnit.Name, profile_Heating, profile_Cooling, humidification, dehumidification, density);

                adjacencyCluster.AddObject(airHandlingUnitAirMovement);
                result.Add(airHandlingUnitAirMovement);

                adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);
            }

            return result;
        }
    }
}