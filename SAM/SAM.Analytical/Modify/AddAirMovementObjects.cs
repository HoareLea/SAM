// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Builds the air movement objects for every ventilation system in the model.
        /// <para>
        /// <b>Caution: this overload works on a copy.</b> <c>AnalyticalModel.AdjacencyCluster</c> returns a
        /// clone, so the objects and relations created here do not reach the model the caller holds - only
        /// the returned list does. That is long-standing behaviour and is deliberately left alone here;
        /// work that needs the movements to land on the model calls
        /// <see cref="AddAirMovementObjects(AdjacencyCluster, ProfileLibrary)"/> on a cluster it owns and
        /// rebuilds the model from it.
        /// </para>
        /// </summary>
        public static List<IAirMovementObject> AddAirMovementObjects(this AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                return null;
            }

            return adjacencyCluster.AddAirMovementObjects(analyticalModel.ProfileLibrary);
        }

        /// <summary>
        /// Builds the air movement objects for every ventilation system in
        /// <paramref name="adjacencyCluster"/>, <b>in place</b>: one movement into each served space from
        /// the system's air handling unit, one movement out of it, and the unit's own supply-condition
        /// profiles.
        ///
        /// <para><b>Where the airflow comes from, and why there are two answers</b></para>
        /// <para>
        /// Where a space carries design <see cref="VentilationTerminal"/>s, they are authoritative and the
        /// two directions are read separately - the supply movement from the space's supply terminals, the
        /// extract movement from its extract terminals. This is what lets a bedroom be supplied and not
        /// extracted while a bathroom is extracted and not supplied, which is what a balanced heat
        /// recovery dwelling actually is.
        /// </para>
        /// <para>
        /// Where a space carries none, the airflow is the space's calculated supply airflow in both
        /// directions, exactly as it has always been. That branch is unchanged: a model without design
        /// terminals must not notice that the other branch exists.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Modified in place.</b></param>
        /// <param name="profileLibrary">
        /// The library a space's ventilation profile name is resolved through. A space with no resolvable
        /// profile gets a movement with the flat default one, as before.
        /// </param>
        public static List<IAirMovementObject> AddAirMovementObjects(this AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary)
        {
            return adjacencyCluster.AddAirMovementObjects(profileLibrary, null);
        }

        /// <summary>
        /// The same, for <b>one</b> ventilation system rather than every system in the model.
        /// <para>
        /// <b>Why a scope exists.</b> A model routinely arrives carrying the system-template assignment it
        /// was built with, and those systems may serve the same rooms as the one the caller is realizing -
        /// the Approved Document O acceptance model splits its rooms across an <c>NV</c>, an <c>MV</c> and a
        /// <c>UV</c> system while the assessment states one MVHR route for the whole dwelling. Walking every
        /// system there would give each shared room two sets of air movements, and the dwelling would be
        /// ventilated twice. Naming the system says which design is being realized without touching, reading
        /// or rewriting the others.
        /// </para>
        /// </summary>
        /// <param name="ventilationSystem">
        /// The system to realize. <b>Null means every ventilation system in the model</b>, which is the
        /// unscoped behaviour every existing caller has.
        /// </param>
        public static List<IAirMovementObject> AddAirMovementObjects(this AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, VentilationSystem ventilationSystem)
        {
            if (adjacencyCluster == null)
            {
                return null;
            }

            List<VentilationSystem> ventilationSystems = ventilationSystem == null
                ? adjacencyCluster.GetMechanicalSystems<VentilationSystem>()
                : [ventilationSystem];

            if (ventilationSystems == null)
            {
                return null;
            }

            List<AirHandlingUnit> airHandlingUnits_All = adjacencyCluster.GetObjects<AirHandlingUnit>();
            if (airHandlingUnits_All == null)
            {
                return null;
            }


            List<IAirMovementObject> result = new List<IAirMovementObject>();

            List<AirHandlingUnit> airHandlingUnits = new List<AirHandlingUnit>();
            foreach (VentilationSystem ventilationSystem_Temp in ventilationSystems)
            {
                if (ventilationSystem_Temp == null)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem_Temp);
                if (spaces == null || spaces.Count == 0)
                {
                    continue;
                }

                if (ventilationSystem_Temp.TryGetValue(VentilationSystemParameter.SupplyUnitName, out string supplyName))
                {
                    AirHandlingUnit airHandlingUnit = airHandlingUnits_All.Find(x => x.Name == supplyName);
                    if (airHandlingUnit != null)
                    {
                        if (airHandlingUnits.Find(x => x.Guid == airHandlingUnit.Guid) == null)
                        {
                            airHandlingUnits.Add(airHandlingUnit);
                        }

                        ObjectReference objectReference_AirHandlingUnit = new ObjectReference(airHandlingUnit);

                        //The extract movements built below, so the unit's exhaust can be sized from them
                        //after the loop. Collected rather than summed because the exhaust runs on the same
                        //profile as the extract it carries away.
                        List<SpaceAirMovement> spaceAirMovements_Extract = new List<SpaceAirMovement>();

                        foreach (Space space in spaces)
                        {
                            Profile profile = space.InternalCondition?.GetProfile(ProfileType.Ventilation, profileLibrary);

                            ObjectReference objectReference_Space = new ObjectReference(space);

                            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(space);

                            if (ventilationTerminals == null || ventilationTerminals.Count == 0)
                            {
                                //No design terminals on this space, so the answer is the one this method has
                                //always given: the space's calculated supply airflow, in both directions, and
                                //an outward movement with no stated destination. Unchanged on purpose - every
                                //model without design terminals reaches this branch and none of them may
                                //notice that the branch below exists.
                                double airflow = space.CalculatedSupplyAirFlow();

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

                                continue;
                            }

                            //Design terminals are authoritative, and the two directions are read SEPARATELY.
                            //
                            //A balanced heat recovery system balances at the SYSTEM, not in each room: a
                            //bedroom is supplied and not extracted, a bathroom is extracted and not supplied,
                            //and the air moves between them as transfer air. Deriving both directions from
                            //the supply figure - which is what the branch above does, because it has nothing
                            //better to read - extracts from every bedroom and supplies every bathroom, so the
                            //model moves roughly the right total amount of air through the wrong rooms.
                            double? supply_Lps = ventilationTerminals.VentilationTerminalDesignDuty_Lps(FlowClassification.Supply);
                            double? extract_Lps = ventilationTerminals.VentilationTerminalDesignDuty_Lps(FlowClassification.Extract);

                            //A direction with no terminal gets no air movement at all, rather than one of
                            //zero: an air movement that moves nothing is indistinguishable downstream from
                            //one that was meant to move air and failed to.
                            if (supply_Lps.HasValue && !double.IsNaN(supply_Lps.Value) && supply_Lps.Value != 0)
                            {
                                double airflow = supply_Lps.Value / 1000.0;

                                string name = string.Format("{0} supply", space.Name);

                                SpaceAirMovement spaceAirMovement = profile == null ? new SpaceAirMovement(name, airflow, objectReference_AirHandlingUnit.ToString(), objectReference_Space.ToString()) : new SpaceAirMovement(name, airflow, profile, objectReference_AirHandlingUnit.ToString(), objectReference_Space.ToString());
                                adjacencyCluster.AddObject(spaceAirMovement);
                                result.Add(spaceAirMovement);

                                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                                adjacencyCluster.AddRelation(spaceAirMovement, space);
                            }

                            if (extract_Lps.HasValue && !double.IsNaN(extract_Lps.Value) && extract_Lps.Value != 0)
                            {
                                double airflow = extract_Lps.Value / 1000.0;

                                string name = string.Format("{0} extract", space.Name);

                                //TO the air handling unit, not to an unstated destination. Extract air goes to
                                //the unit - that is what heat recovery recovers from - and it is also the only
                                //form the destination can usefully take downstream: a TBD inter-zone air
                                //movement always moves air INTO the zones it is assigned to, so an extract has
                                //to be an air movement on the UNIT, sourced from the room.
                                SpaceAirMovement spaceAirMovement = profile == null ? new SpaceAirMovement(name, airflow, objectReference_Space.ToString(), objectReference_AirHandlingUnit.ToString()) : new SpaceAirMovement(name, airflow, profile, objectReference_Space.ToString(), objectReference_AirHandlingUnit.ToString());
                                adjacencyCluster.AddObject(spaceAirMovement);
                                result.Add(spaceAirMovement);

                                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                                adjacencyCluster.AddRelation(spaceAirMovement, space);

                                spaceAirMovements_Extract.Add(spaceAirMovement);
                            }
                        }

                        AddAirHandlingUnitExhaust(adjacencyCluster, airHandlingUnit, objectReference_AirHandlingUnit, spaceAirMovements_Extract, result);
                    }
                }
            }

            List<double> densities = new List<double>() { FluidProperty.Air.Density };

            //The unit does not humidify and does not dehumidify, and these two profiles are how that is
            //said - as limits that can never be reached, not as an absence.
            //
            //They become the humidistat on the unit's TAS zone: Humidification is the humidity LOWER limit
            //(TBD Profiles.ticHLL) and Dehumidification the UPPER limit (ticHUL). "No humidity control" is
            //therefore a lower limit of 0% and an upper limit of 100% - exactly the pair the shipped
            //profile library states as "No Humidification" (0) and "No Dehumidification" (100), and exactly
            //the pair TAS's own new-internal-condition defaults carry.
            //
            //Written the other way round - lower 100%, upper 0% - the humidistat is asked to hold the air
            //above 100% and below 0% at the same time, and TAS refuses the model before it simulates:
            //"Internal Condition 'MVHR-01' humidistat has overlapping limits." That is what these two lists
            //held until 2026-09; the two values were transposed.
            List<double> humidifications = new List<double>() { 0 };
            List<double> dehumidifications = new List<double>() { 100 };

            foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
            {
                //A supply temperature the unit does not state is not written as a setpoint.
                //
                //These profiles become the unit's TAS zone thermostat, so a NaN reaches the file as a NaN
                //temperature limit and the annual simulation fails outright - which is what a unit built by
                //Create.AirHandlingUnit produces, because it states no winter supply temperature at all.
                //An absent supply temperature means the unit does not condition its supply air, and leaving
                //the setpoint out is how that is said.
                Profile profile_Heating = double.IsNaN(airHandlingUnit.WinterSupplyTemperature)
                    ? null
                    : new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Heating), ProfileType.Heating, new double[] { airHandlingUnit.WinterSupplyTemperature });

                Profile profile_Cooling = double.IsNaN(airHandlingUnit.SummerSupplyTemperature)
                    ? null
                    : new Profile(string.Format("{0} {1}", airHandlingUnit.Name, ProfileType.Cooling), ProfileType.Cooling, new double[] { airHandlingUnit.SummerSupplyTemperature });


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

        /// <summary>
        /// Adds the air handling unit's exhaust: the extract air it has drawn out of the rooms, leaving the
        /// building.
        ///
        /// <para>
        /// The unit's TAS zone receives its outside intake and every room's extract, and delivers the supply.
        /// Without the exhaust it gains the whole extract duty and never loses it, and TAS refuses to
        /// simulate a zone whose air movements do not balance. The exhaust is the object that says where
        /// that air goes; a destination of <b>null</b> is how "outside" is said, and the TBD writer turns it
        /// into an inter-zone air movement on the unit's zone with no source zone and no from-outside flag,
        /// which is exactly the shape TAS itself authors for a zone that discharges to outside.
        /// </para>
        /// <para>
        /// <b>Nothing is added where there is no extract</b>, so a model whose spaces carry no design
        /// terminals - which produces no extract back to the unit - reaches this and leaves with the air
        /// movements it has always had.
        /// </para>
        /// </summary>
        private static void AddAirHandlingUnitExhaust(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, ObjectReference objectReference_AirHandlingUnit, List<SpaceAirMovement> spaceAirMovements_Extract, List<IAirMovementObject> result)
        {
            if (spaceAirMovements_Extract == null || spaceAirMovements_Extract.Count == 0)
            {
                return;
            }

            //Summed the same way Query.AirFlow sums the intake, so the exhaust follows the extract hour by
            //hour rather than only at the design condition.
            Profile profile = null;

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements_Extract)
            {
                Profile profile_Temp = spaceAirMovement.Profile;
                if (profile_Temp == null)
                {
                    continue;
                }

                profile_Temp.Multiply(spaceAirMovement.AirFlow);

                if (profile == null)
                {
                    profile = profile_Temp;
                }
                else
                {
                    profile.Sum(profile_Temp);
                }
            }

            if (profile == null)
            {
                return;
            }

            double airFlow = profile.MaxValue;
            if (double.IsNaN(airFlow) || airFlow <= 0)
            {
                return;
            }

            profile.Divide(airFlow);

            string name = string.Format("{0} exhaust", airHandlingUnit.Name);

            SpaceAirMovement spaceAirMovement_Exhaust = new SpaceAirMovement(name, airFlow, profile, objectReference_AirHandlingUnit.ToString(), null);

            adjacencyCluster.AddObject(spaceAirMovement_Exhaust);
            result.Add(spaceAirMovement_Exhaust);

            //Related to the unit and to no space: it is the unit's own movement, and relating it to a space
            //would have the TBD writer look for the space's zone to put it on.
            adjacencyCluster.AddRelation(spaceAirMovement_Exhaust, airHandlingUnit);
        }
    }
}
