// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Core.Reporting;
using SAM.Units;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    public static partial class Create
    {
        private const string NoInternalCondition = "No internal condition assigned";

        /// <summary>
        /// Reason on a Sizing multiplier set neither on the space nor on the model (reported as NotApplicable).
        /// </summary>
        public const string SizingFactorNotSet = "Not set: SAM_Tas applies no sizing multiplier";

        /// <summary>
        /// Collects the Phase 1 assumptions of one space from the saved model: identity, geometry, internal condition,
        /// design criteria, ventilation, systems, fabric and persisted design loads. No simulation results are read.
        /// <para>
        /// Data a model may legitimately lack becomes NotAvailable / NotApplicable with a reason, and a warning is
        /// added to <see cref="DocumentContext.Diagnostics"/>. Values that are present but physically invalid (NaN,
        /// negative area) become NotAvailable("invalid value in model"). Anything else - a null argument, a logic
        /// error - throws and aborts the document.
        /// </para>
        /// </summary>
        public static SpaceDocumentData SpaceDocumentData(DocumentContext documentContext, Space space)
        {
            if (documentContext == null)
            {
                throw new ArgumentNullException(nameof(documentContext));
            }

            if (space == null)
            {
                throw new ArgumentNullException(nameof(space));
            }

            Collector collector = new Collector(documentContext, space);

            SpaceGeometryData spaceGeometryData = collector.Geometry();

            return new SpaceDocumentData()
            {
                Identity = collector.Identity(),
                Geometry = spaceGeometryData,
                Occupancy = collector.Occupancy(),
                Lighting = collector.Lighting(),
                EquipmentSensible = collector.EquipmentSensible(),
                EquipmentLatent = collector.EquipmentLatent(),
                Infiltration = collector.Infiltration(),
                DesignCriteria = collector.DesignCriteria(),
                Ventilation = collector.Ventilation(spaceGeometryData),
                Systems = collector.Systems(),
                Fabric = collector.Fabric(),
                Sizing = collector.Sizing(spaceGeometryData),
                Provenance = documentContext.Provenance,
            };
        }

        /// <summary>
        /// Reads one space. Kept private so the collection rules (source, reason texts, warnings) stay in one place.
        /// </summary>
        private sealed class Collector
        {
            private const string InvalidValue = "invalid value in model";

            private readonly DocumentContext documentContext;
            private readonly Space space;
            private readonly InternalCondition internalCondition;
            private readonly AdjacencyCluster adjacencyCluster;

            public Collector(DocumentContext documentContext, Space space)
            {
                this.documentContext = documentContext;
                this.space = space;
                internalCondition = space.InternalCondition;
                adjacencyCluster = documentContext.AdjacencyCluster;
            }

            private string SpaceName => string.IsNullOrWhiteSpace(space.Name) ? space.Guid.ToString() : space.Name;

            public SpaceIdentityData Identity()
            {
                return new SpaceIdentityData()
                {
                    Guid = space.Guid,
                    Name = Text(space.Name, ReportValueSource.SAM, "Space has no name"),
                    LevelName = space.TryGetValue(SpaceParameter.LevelName, out string levelName) ? Text(levelName, ReportValueSource.SAM, "No level assigned") : ReportValue<string>.NotAvailable("No level assigned"),
                    InternalConditionName = internalCondition == null ? Warn(ReportValue<string>.NotAvailable(NoInternalCondition)) : Text(internalCondition.Name, ReportValueSource.SAM, "Internal condition has no name"),
                };
            }

            public SpaceGeometryData Geometry()
            {
                ReportValue<Quantity> area = Measure(
                    Analytical.Query.CalculatedArea(space, adjacencyCluster),
                    UnitType.SquareMeter,
                    HasStored(SpaceParameter.Area) ? ReportValueSource.SAM : ReportValueSource.Derived,
                    "Floor area",
                    "Floor area not available");

                ReportValue<Quantity> volume = Measure(
                    Analytical.Query.CalculatedVolume(space, adjacencyCluster),
                    UnitType.CubicMeter,
                    HasStored(SpaceParameter.Volume) ? ReportValueSource.SAM : ReportValueSource.Derived,
                    "Volume",
                    "Volume not available");

                ReportValue<Quantity> averageHeight = ReportValue<Quantity>.NotAvailable("Needs floor area and volume");
                if (area.TryGetValue(out Quantity quantity_Area) && volume.TryGetValue(out Quantity quantity_Volume) && quantity_Area.Value > 0)
                {
                    averageHeight = ReportValue<Quantity>.Available(new Quantity(quantity_Volume.Value / quantity_Area.Value, UnitType.Meter), ReportValueSource.Derived, note: "Average: volume ÷ floor area");
                }

                return new SpaceGeometryData()
                {
                    Area = area,
                    Volume = volume,
                    AverageHeight = averageHeight,
                };
            }

            public SpaceOccupancyData Occupancy()
            {
                if (internalCondition == null)
                {
                    ReportValue<Quantity> missing = ReportValue<Quantity>.NotAvailable(NoInternalCondition);
                    return new SpaceOccupancyData()
                    {
                        People = missing,
                        AreaPerPerson = missing,
                        SensibleGainPerPerson = missing,
                        LatentGainPerPerson = missing,
                        SensibleGain = missing,
                        LatentGain = missing,
                        Profile = ReportValue<string>.NotAvailable(NoInternalCondition),
                        OccupiedHoursPerYear = missing,
                    };
                }

                ReportValue<Quantity> areaPerPerson;
                if (!internalCondition.TryGetValue(InternalConditionParameter.AreaPerPerson, out double value_AreaPerPerson) || double.IsNaN(value_AreaPerPerson))
                {
                    areaPerPerson = ReportValue<Quantity>.NotAvailable("No area per person");
                }
                else if (value_AreaPerPerson == 0)
                {
                    areaPerPerson = ReportValue<Quantity>.NotApplicable("Unoccupied (area per person is 0)");
                }
                else
                {
                    areaPerPerson = Measure(value_AreaPerPerson, UnitType.SquareMeterPerPerson, ReportValueSource.SAM, "Area per person", "No area per person");
                }

                return new SpaceOccupancyData()
                {
                    People = Measure(Analytical.Query.CalculatedOccupancy(space), UnitType.Person, HasStored(SpaceParameter.Occupancy) ? ReportValueSource.SAM : ReportValueSource.Derived, "Occupancy", "Occupancy not available"),
                    AreaPerPerson = areaPerPerson,
                    SensibleGainPerPerson = Parameter(InternalConditionParameter.OccupancySensibleGainPerPerson, UnitType.WattPerPerson, "No occupancy sensible gain"),
                    LatentGainPerPerson = Parameter(InternalConditionParameter.OccupancyLatentGainPerPerson, UnitType.WattPerPerson, "No occupancy latent gain"),
                    SensibleGain = AuthoredGain(Analytical.Query.OccupancySensibleGain(space), "Occupancy sensible gain", "No occupancy sensible gain authored", InternalConditionParameter.OccupancySensibleGainPerPerson),
                    LatentGain = AuthoredGain(Analytical.Query.OccupancyLatentGain(space), "Occupancy latent gain", "No occupancy latent gain authored", InternalConditionParameter.OccupancyLatentGainPerPerson),
                    Profile = ProfileName(InternalConditionParameter.OccupancyProfileName),
                    OccupiedHoursPerYear = OccupiedHoursPerYear(),
                };
            }

            public SpaceGainData Lighting()
            {
                if (internalCondition == null)
                {
                    return MissingGain(true);
                }

                return new SpaceGainData()
                {
                    GainPerArea = Parameter(InternalConditionParameter.LightingGainPerArea, UnitType.WattPerSquareMeter, "No lighting gain per area"),
                    Gain = AuthoredGain(Analytical.Query.CalculatedLightingGain(space), "Lighting gain", "No lighting gain authored", InternalConditionParameter.LightingGainPerArea, InternalConditionParameter.LightingGain, InternalConditionParameter.LightingGainPerPerson),
                    Illuminance = Parameter(InternalConditionParameter.LightingLevel, UnitType.Lux, "No lighting level"),
                    Profile = ProfileName(InternalConditionParameter.LightingProfileName),
                };
            }

            public SpaceGainData EquipmentSensible()
            {
                if (internalCondition == null)
                {
                    return MissingGain(false);
                }

                return new SpaceGainData()
                {
                    GainPerArea = Parameter(InternalConditionParameter.EquipmentSensibleGainPerArea, UnitType.WattPerSquareMeter, "No equipment sensible gain per area"),
                    Gain = AuthoredGain(Analytical.Query.CalculatedEquipmentSensibleGain(space), "Equipment sensible gain", "No equipment sensible gain authored", InternalConditionParameter.EquipmentSensibleGainPerArea, InternalConditionParameter.EquipmentSensibleGain, InternalConditionParameter.EquipmentSensibleGainPerPerson),
                    Illuminance = ReportValue<Quantity>.NotApplicable("Equipment gain"),
                    Profile = ProfileName(InternalConditionParameter.EquipmentSensibleProfileName),
                };
            }

            public SpaceGainData EquipmentLatent()
            {
                if (internalCondition == null)
                {
                    return MissingGain(false);
                }

                return new SpaceGainData()
                {
                    GainPerArea = Parameter(InternalConditionParameter.EquipmentLatentGainPerArea, UnitType.WattPerSquareMeter, "No equipment latent gain per area"),
                    Gain = AuthoredGain(Analytical.Query.CalculatedEquipmentLatentGain(space), "Equipment latent gain", "No equipment latent gain authored", InternalConditionParameter.EquipmentLatentGainPerArea, InternalConditionParameter.EquipmentLatentGain),
                    Illuminance = ReportValue<Quantity>.NotApplicable("Equipment gain"),
                    Profile = ProfileName(InternalConditionParameter.EquipmentLatentProfileName),
                };
            }

            public SpaceInfiltrationData Infiltration()
            {
                if (internalCondition == null)
                {
                    return new SpaceInfiltrationData()
                    {
                        AirChangeRate = ReportValue<Quantity>.NotAvailable(NoInternalCondition),
                        AirFlow = ReportValue<Quantity>.NotAvailable(NoInternalCondition),
                        Profile = ReportValue<string>.NotAvailable(NoInternalCondition),
                    };
                }

                return new SpaceInfiltrationData()
                {
                    AirChangeRate = Parameter(InternalConditionParameter.InfiltrationAirChangesPerHour, UnitType.AirChangesPerHour, "No infiltration rate"),
                    AirFlow = Measure(Analytical.Query.CalculatedInfiltrationAirFlow(space), UnitType.CubicMeterPerSecond, ReportValueSource.Derived, "Infiltration air flow", "No infiltration air flow"),
                    Profile = ProfileName(InternalConditionParameter.InfiltrationProfileName),
                };
            }

            public SpaceDesignCriteriaData DesignCriteria()
            {
                ProfileLibrary profileLibrary = documentContext.ProfileLibrary;

                // Set points are read from the internal condition profiles directly. In Tas the humidification profile
                // is the zone's humidity lower limit (TBD ticHLL; "no humidification" = 0 %) and the dehumidification
                // profile its upper limit (ticHUL; "no dehumidification" = 100 %), so they are reported as the
                // humidification / dehumidification set points, not as heating / cooling RH. The SAM queries
                // HeatingDesignRelativeHumidity / CoolingDesignRelativeHumidity are not used: their names do not match
                // the profiles they read. A thermostat held at SAM's "No Heating" / "No Cooling" value all year is off,
                // not a set point: the heating value is the yearly maximum and the cooling one the yearly minimum, so
                // any schedule that ever controls still prints its set point.
                return new SpaceDesignCriteriaData()
                {
                    HeatingSetPoint = ProfileValue(profileLibrary, ProfileType.Heating, true, UnitType.Celsius, "No heating profile", x => Analytical.Query.IsHeatingOff(x) ? string.Format(CultureInfo.InvariantCulture, "No heating (thermostat at {0} °C all year)", Analytical.Query.NoHeatingSetPoint) : null),
                    CoolingSetPoint = ProfileValue(profileLibrary, ProfileType.Cooling, false, UnitType.Celsius, "No cooling profile", x => Analytical.Query.IsCoolingOff(x) ? string.Format(CultureInfo.InvariantCulture, "No cooling (thermostat at {0} °C all year)", Analytical.Query.NoCoolingSetPoint) : null),
                    HumidificationSetPoint = ProfileValue(profileLibrary, ProfileType.Humidification, true, UnitType.Percent, "No humidification profile", x => x <= 0 ? "No humidification (lower RH limit 0 %)" : null),
                    DehumidificationSetPoint = ProfileValue(profileLibrary, ProfileType.Dehumidification, false, UnitType.Percent, "No dehumidification profile", x => x >= 100 ? "No dehumidification (upper RH limit 100 %)" : null),
                    OutdoorHeatingDryBulb = DesignDayExtreme(AnalyticalModelParameter.HeatingDesignDays, false, out ReportValue<Quantity> heatingRelativeHumidity),
                    OutdoorHeatingRelativeHumidity = heatingRelativeHumidity,
                    OutdoorCoolingDryBulb = DesignDayExtreme(AnalyticalModelParameter.CoolingDesignDays, true, out ReportValue<Quantity> coolingRelativeHumidity),
                    OutdoorCoolingRelativeHumidity = coolingRelativeHumidity,
                };
            }

            public SpaceVentilationData Ventilation(SpaceGeometryData spaceGeometryData)
            {
                ReportValue<Quantity> supplyAirFlow = AirFlow(SpaceParameter.SupplyAirFlow, () => Analytical.Query.CalculatedSupplyAirFlow(space), "Supply air flow", "No supply air flow");
                ReportValue<Quantity> extractAirFlow = AirFlow(SpaceParameter.ExhaustAirFlow, () => Analytical.Query.CalculatedExhaustAirFlow(space), "Extract air flow", "No extract air flow");
                ReportValue<Quantity> outsideAirFlow = AirFlow(SpaceParameter.OutsideSupplyAirFlow, null, "Outside air flow", "No outside air flow");

                ReportValue<Quantity> supplyAirChangeRate = ReportValue<Quantity>.NotAvailable("Needs supply air flow and volume");
                if (supplyAirFlow.TryGetValue(out Quantity quantity_Supply) && spaceGeometryData.Volume.TryGetValue(out Quantity quantity_Volume) && quantity_Volume.Value > 0)
                {
                    double supply = quantity_Supply.ConvertTo(UnitType.CubicMeterPerSecond).Value;
                    supplyAirChangeRate = ReportValue<Quantity>.Available(new Quantity(supply * 3600 / quantity_Volume.Value, UnitType.AirChangesPerHour), ReportValueSource.Derived, note: "Supply air flow ÷ volume");
                }

                VentilationSystem ventilationSystem = adjacencyCluster == null ? null : Analytical.Query.Systems<VentilationSystem>(adjacencyCluster, space)?.FirstOrDefault();

                ReportValue<string> systemType = SystemTypeName(ventilationSystem, "No ventilation system assigned");
                ReportValue<string> supplyUnit = ReportValue<string>.NotAvailable("No ventilation system assigned");
                ReportValue<string> extractUnit = ReportValue<string>.NotAvailable("No ventilation system assigned");
                if (ventilationSystem != null)
                {
                    supplyUnit = ventilationSystem.TryGetValue(VentilationSystemParameter.SupplyUnitName, out string supplyUnitName) ? Text(supplyUnitName, ReportValueSource.SAM, "No supply unit") : ReportValue<string>.NotAvailable("No supply unit");
                    extractUnit = ventilationSystem.TryGetValue(VentilationSystemParameter.ExhaustUnitName, out string exhaustUnitName) ? Text(exhaustUnitName, ReportValueSource.SAM, "No extract unit") : ReportValue<string>.NotAvailable("No extract unit");
                }

                return new SpaceVentilationData()
                {
                    SupplyAirFlow = supplyAirFlow,
                    ExtractAirFlow = extractAirFlow,
                    OutsideAirFlow = outsideAirFlow,
                    SupplyAirChangeRate = supplyAirChangeRate,
                    SystemType = systemType,
                    SupplyUnit = supplyUnit,
                    ExtractUnit = extractUnit,
                };
            }

            public SpaceSystemsData Systems()
            {
                HeatingSystem heatingSystem = adjacencyCluster == null ? null : Analytical.Query.Systems<HeatingSystem>(adjacencyCluster, space)?.FirstOrDefault();
                CoolingSystem coolingSystem = adjacencyCluster == null ? null : Analytical.Query.Systems<CoolingSystem>(adjacencyCluster, space)?.FirstOrDefault();

                return new SpaceSystemsData()
                {
                    HeatingSystem = SystemTypeName(heatingSystem, "No heating system assigned"),
                    CoolingSystem = SystemTypeName(coolingSystem, "No cooling system assigned"),
                    VentilationRiser = Riser(SpaceParameter.VentilationRiserName),
                    HeatingRiser = Riser(SpaceParameter.HeatingRiserName),
                    CoolingRiser = Riser(SpaceParameter.CoolingRiserName),
                };
            }

            public SpaceFabricData Fabric()
            {
                List<Panel> panels = adjacencyCluster?.GetPanels(space);
                if (panels == null || panels.Count == 0)
                {
                    Warning("no panels bound the space, so fabric areas are not available");
                    return new SpaceFabricData() { Rows = new List<FabricAreaRow>().AsReadOnly() };
                }

                Dictionary<FabricCategory, double[]> areas = new Dictionary<FabricCategory, double[]>();
                foreach (FabricCategory fabricCategory in Enum.GetValues(typeof(FabricCategory)))
                {
                    // external, internal, external frame, internal frame
                    areas[fabricCategory] = new double[4];
                }

                foreach (Panel panel in panels)
                {
                    if (panel == null)
                    {
                        continue;
                    }

                    FabricCategory? fabricCategory = Category(panel.PanelType);
                    if (fabricCategory == null)
                    {
                        continue;
                    }

                    bool external = adjacencyCluster.External(panel);
                    int index = external ? 0 : 1;

                    double area = panel.GetAreaNet();
                    if (!double.IsNaN(area) && area > 0)
                    {
                        areas[fabricCategory.Value][index] += area;
                    }

                    List<Aperture> apertures = panel.Apertures;
                    if (apertures == null)
                    {
                        continue;
                    }

                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null || aperture.ApertureType == ApertureType.Undefined)
                        {
                            continue;
                        }

                        FabricCategory fabricCategory_Aperture = aperture.ApertureType == ApertureType.Door ? FabricCategory.Doors : FabricCategory.Windows;

                        Analytical.Query.Area(aperture, out double paneArea, out double frameArea);
                        if (!double.IsNaN(paneArea) && paneArea > 0)
                        {
                            areas[fabricCategory_Aperture][index] += paneArea;
                        }

                        if (!double.IsNaN(frameArea) && frameArea > 0)
                        {
                            areas[fabricCategory_Aperture][index + 2] += frameArea;
                        }
                    }
                }

                List<FabricAreaRow> fabricAreaRows = new List<FabricAreaRow>();
                foreach (KeyValuePair<FabricCategory, double[]> keyValuePair in areas)
                {
                    bool opening = keyValuePair.Key == FabricCategory.Windows || keyValuePair.Key == FabricCategory.Doors;
                    double[] values = keyValuePair.Value;

                    fabricAreaRows.Add(new FabricAreaRow()
                    {
                        Category = keyValuePair.Key,
                        ExternalArea = FabricArea(values[0]),
                        InternalArea = FabricArea(values[1]),
                        ExternalFrameArea = opening ? FabricArea(values[2]) : ReportValue<Quantity>.NotApplicable("Opaque element"),
                        InternalFrameArea = opening ? FabricArea(values[3]) : ReportValue<Quantity>.NotApplicable("Opaque element"),
                    });
                }

                return new SpaceFabricData() { Rows = fabricAreaRows.AsReadOnly() };
            }

            public SpaceSizingData Sizing(SpaceGeometryData spaceGeometryData)
            {
                ReportValue<Quantity> designHeatingLoad = DesignLoad(SpaceParameter.DesignHeatingLoad, "No design heating load in model (not sized in Tas)");
                ReportValue<Quantity> designCoolingLoad = DesignLoad(SpaceParameter.DesignCoolingLoad, "No design cooling load in model (not sized in Tas)");

                DesignLoadStatus designLoadStatus = designHeatingLoad.HasValue || designCoolingLoad.HasValue ? DesignLoadStatus.Unknown : DesignLoadStatus.None;
                if (designLoadStatus == DesignLoadStatus.None)
                {
                    Warning("no design loads in the model");
                }

                return new SpaceSizingData()
                {
                    DesignLoadStatus = designLoadStatus,
                    DesignHeatingLoad = designHeatingLoad,
                    DesignCoolingLoad = designCoolingLoad,
                    DesignHeatingLoadPerArea = LoadPerArea(designHeatingLoad, spaceGeometryData.Area),
                    DesignCoolingLoadPerArea = LoadPerArea(designCoolingLoad, spaceGeometryData.Area),
                    HeatingSizingFactor = SizingFactor(SpaceParameter.HeatingSizingFactor, AnalyticalModelParameter.HeatingSizingFactor),
                    CoolingSizingFactor = SizingFactor(SpaceParameter.CoolingSizingFactor, AnalyticalModelParameter.CoolingSizingFactor),
                };
            }

            // ---------- helpers ----------

            private bool HasStored(SpaceParameter spaceParameter)
            {
                return space.TryGetValue(spaceParameter, out double value) && !double.IsNaN(value);
            }

            private void Warning(string message)
            {
                documentContext.Diagnostics.Add("Space {0}: {1}", LogRecordType.Warning, SpaceName, message);
            }

            private ReportValue<T> Warn<T>(ReportValue<T> reportValue)
            {
                Warning(reportValue.Note);
                return reportValue;
            }

            private static ReportValue<string> Text(string value, ReportValueSource reportValueSource, string reason)
            {
                return string.IsNullOrWhiteSpace(value) ? ReportValue<string>.NotAvailable(reason) : ReportValue<string>.Available(value.Trim(), reportValueSource);
            }

            /// <summary>
            /// A physical value: NaN is missing data; a negative or non-finite value is a data defect.
            /// </summary>
            private ReportValue<Quantity> Measure(double value, UnitType unitType, ReportValueSource reportValueSource, string what, string reason)
            {
                if (double.IsNaN(value))
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                if (double.IsInfinity(value) || value < 0)
                {
                    Warning(string.Format("{0} is {1}, an {2}", what, value, InvalidValue));
                    return ReportValue<Quantity>.NotAvailable(InvalidValue);
                }

                return ReportValue<Quantity>.Available(new Quantity(value, unitType), reportValueSource);
            }

            private ReportValue<Quantity> Parameter(InternalConditionParameter internalConditionParameter, UnitType unitType, string reason)
            {
                if (!internalCondition.TryGetValue(internalConditionParameter, out double value))
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                return Measure(value, unitType, ReportValueSource.SAM, internalConditionParameter.ToString(), reason);
            }

            /// <summary>
            /// A total internal gain. SAM's Calculated*Gain and Occupancy*Gain queries return 0 W when nothing is authored (their
            /// TryGetValue out-parameter overwrites the NaN default), so authorship is read from the internal condition
            /// itself: no gain parameter at all is NotAvailable, while an authored 0 stays 0.
            /// </summary>
            private ReportValue<Quantity> AuthoredGain(double value, string what, string reason, params InternalConditionParameter[] internalConditionParameters)
            {
                bool authored = internalConditionParameters.Any(x => internalCondition.TryGetValue(x, out double value_Parameter) && !double.IsNaN(value_Parameter));
                if (!authored)
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                return Measure(value, UnitType.Watt, ReportValueSource.Derived, what, reason);
            }

            private ReportValue<string> ProfileName(InternalConditionParameter internalConditionParameter)
            {
                return internalCondition.TryGetValue(internalConditionParameter, out string name) ? Text(name, ReportValueSource.SAM, "No profile") : ReportValue<string>.NotAvailable("No profile");
            }

            private ReportValue<Quantity> OccupiedHoursPerYear()
            {
                Profile profile = internalCondition.GetProfile(ProfileType.Occupancy, documentContext.ProfileLibrary);
                double[] values = profile?.GetYearlyValues();
                if (values == null || values.Length == 0)
                {
                    return ReportValue<Quantity>.NotAvailable("Occupancy profile not found");
                }

                return ReportValue<Quantity>.Available(new Quantity(values.Count(x => !double.IsNaN(x) && x != 0), UnitType.Hour), ReportValueSource.Derived, note: "Hours with non-zero occupancy");
            }

            private SpaceGainData MissingGain(bool lighting)
            {
                return new SpaceGainData()
                {
                    GainPerArea = ReportValue<Quantity>.NotAvailable(NoInternalCondition),
                    Gain = ReportValue<Quantity>.NotAvailable(NoInternalCondition),
                    Illuminance = lighting ? ReportValue<Quantity>.NotAvailable(NoInternalCondition) : ReportValue<Quantity>.NotApplicable("Equipment gain"),
                    Profile = ReportValue<string>.NotAvailable(NoInternalCondition),
                };
            }

            /// <summary>
            /// The maximum or minimum of a profile over the year. The yearly expansion is used rather than
            /// Profile.MaxValue / MinValue because MinValue takes the maximum of each child profile, which is wrong for
            /// a profile built from other profiles (for example a year of day profiles). A value for which
            /// <paramref name="notApplicable"/> returns a reason means the control is off (Tas convention).
            /// </summary>
            private ReportValue<Quantity> ProfileValue(ProfileLibrary profileLibrary, ProfileType profileType, bool maximum, UnitType unitType, string reason, Func<double, string> notApplicable)
            {
                if (internalCondition == null)
                {
                    return ReportValue<Quantity>.NotAvailable(NoInternalCondition);
                }

                Profile profile = profileLibrary == null ? null : internalCondition.GetProfile(profileType, profileLibrary);
                if (profile == null)
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                double[] values = profile.GetYearlyValues()?.Where(x => !double.IsNaN(x) && !double.IsInfinity(x)).ToArray();
                if (values == null || values.Length == 0)
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                double value = maximum ? values.Max() : values.Min();

                string reason_NotApplicable = notApplicable?.Invoke(value);
                if (reason_NotApplicable != null)
                {
                    return ReportValue<Quantity>.NotApplicable(reason_NotApplicable);
                }

                return ReportValue<Quantity>.Available(new Quantity(value, unitType), ReportValueSource.Derived, note: string.Format("{0} of profile {1}", maximum ? "Maximum" : "Minimum", profile.Name));
            }

            /// <summary>
            /// The extreme dry bulb over all design days of one kind, with the relative humidity of the same hour.
            /// </summary>
            private ReportValue<Quantity> DesignDayExtreme(AnalyticalModelParameter analyticalModelParameter, bool maximum, out ReportValue<Quantity> relativeHumidity)
            {
                string reason = analyticalModelParameter == AnalyticalModelParameter.HeatingDesignDays ? "No heating design day in model" : "No cooling design day in model";

                relativeHumidity = ReportValue<Quantity>.NotAvailable(reason);

                if (!documentContext.AnalyticalModel.TryGetValue(analyticalModelParameter, out SAMCollection<DesignDay> designDays) || designDays == null || designDays.Count == 0)
                {
                    Warning(reason.Substring(0, 1).ToLowerInvariant() + reason.Substring(1));
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                double dryBulb = double.NaN;
                double relativeHumidity_Value = double.NaN;
                string name = null;
                foreach (DesignDay designDay in designDays)
                {
                    double[] dryBulbs = designDay?[WeatherDataType.DryBulbTemperature];
                    if (dryBulbs == null)
                    {
                        continue;
                    }

                    double[] relativeHumidities = designDay[WeatherDataType.RelativeHumidity];
                    for (int i = 0; i < dryBulbs.Length; i++)
                    {
                        double value = dryBulbs[i];
                        if (double.IsNaN(value) || double.IsInfinity(value))
                        {
                            continue;
                        }

                        if (double.IsNaN(dryBulb) || (maximum ? value > dryBulb : value < dryBulb))
                        {
                            dryBulb = value;
                            relativeHumidity_Value = relativeHumidities != null && i < relativeHumidities.Length ? relativeHumidities[i] : double.NaN;
                            name = designDay.Name;
                        }
                    }
                }

                if (double.IsNaN(dryBulb))
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                string note = string.Format("{0} of design day {1}", maximum ? "Maximum" : "Minimum", name);
                if (!double.IsNaN(relativeHumidity_Value) && !double.IsInfinity(relativeHumidity_Value))
                {
                    relativeHumidity = ReportValue<Quantity>.Available(new Quantity(relativeHumidity_Value, UnitType.Percent), ReportValueSource.SAM, note: "Coincident with the dry bulb");
                }

                return ReportValue<Quantity>.Available(new Quantity(dryBulb, UnitType.Celsius), ReportValueSource.SAM, note: note);
            }

            /// <summary>
            /// A design air flow: the value stored on the space, else the internal-condition calculation, else missing.
            /// </summary>
            private ReportValue<Quantity> AirFlow(SpaceParameter spaceParameter, Func<double> calculate, string what, string reason)
            {
                if (space.TryGetValue(spaceParameter, out double value) && !double.IsNaN(value))
                {
                    return Measure(value, UnitType.CubicMeterPerSecond, ReportValueSource.SAM, what, reason);
                }

                if (calculate == null || internalCondition == null)
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                return Measure(calculate(), UnitType.CubicMeterPerSecond, ReportValueSource.Derived, what, reason);
            }

            private ReportValue<string> SystemTypeName(MechanicalSystem mechanicalSystem, string reason)
            {
                if (mechanicalSystem == null)
                {
                    Warning(reason.Substring(0, 1).ToLowerInvariant() + reason.Substring(1));
                    return ReportValue<string>.NotAvailable(reason);
                }

                string name = mechanicalSystem.Type?.Name;
                return Text(string.IsNullOrWhiteSpace(name) ? mechanicalSystem.Name : name, ReportValueSource.SAM, "System has no type name");
            }

            private ReportValue<string> Riser(SpaceParameter spaceParameter)
            {
                return space.TryGetValue(spaceParameter, out string name) ? Text(name, ReportValueSource.SAM, "No riser assigned") : ReportValue<string>.NotAvailable("No riser assigned");
            }

            private static FabricCategory? Category(PanelType panelType)
            {
                switch (panelType)
                {
                    case PanelType.Wall:
                    case PanelType.WallExternal:
                    case PanelType.WallInternal:
                    case PanelType.CurtainWall:
                    case PanelType.UndergroundWall:
                        return FabricCategory.Walls;

                    case PanelType.Roof:
                    case PanelType.Ceiling:
                    case PanelType.UndergroundCeiling:
                        return FabricCategory.RoofsAndCeilings;

                    case PanelType.SlabOnGrade:
                    case PanelType.UndergroundSlab:
                        return FabricCategory.GroundFloors;

                    case PanelType.Floor:
                    case PanelType.FloorExposed:
                    case PanelType.FloorInternal:
                    case PanelType.FloorRaised:
                        return FabricCategory.OtherFloors;
                }

                // Shade, SolarPanel and Undefined do not bound the space's fabric.
                return null;
            }

            private static ReportValue<Quantity> FabricArea(double value)
            {
                return ReportValue<Quantity>.Available(new Quantity(value, UnitType.SquareMeter), ReportValueSource.Derived);
            }

            private ReportValue<Quantity> DesignLoad(SpaceParameter spaceParameter, string reason)
            {
                if (!space.TryGetValue(spaceParameter, out double value) || double.IsNaN(value))
                {
                    return ReportValue<Quantity>.NotAvailable(reason);
                }

                ReportValue<Quantity> result = Measure(value, UnitType.Watt, ReportValueSource.TBD, spaceParameter.ToString(), reason);

                // No record ties this value to a sizing run (Rev 3 R3.1): freshness is Unknown, and TSD provenance is
                // deliberately not consulted.
                return result.HasValue ? result.WithFreshness(Freshness.Unknown, "Tas sizing - date/currency not recorded") : result;
            }

            private static ReportValue<Quantity> LoadPerArea(ReportValue<Quantity> load, ReportValue<Quantity> area)
            {
                if (!load.TryGetValue(out Quantity quantity_Load))
                {
                    return ReportValue<Quantity>.NotAvailable(load.Note);
                }

                if (!area.TryGetValue(out Quantity quantity_Area) || quantity_Area.Value <= 0)
                {
                    return ReportValue<Quantity>.NotAvailable("Needs floor area");
                }

                double value = quantity_Load.ConvertTo(UnitType.Watt).Value / quantity_Area.ConvertTo(UnitType.SquareMeter).Value;

                // Derived from a TBD value, so it inherits that value's freshness.
                return ReportValue<Quantity>.Available(new Quantity(value, UnitType.WattPerSquareMeter), ReportValueSource.Derived, load.Freshness ?? Freshness.Unknown, note: "Design load ÷ floor area");
            }

            private ReportValue<Quantity> SizingFactor(SpaceParameter spaceParameter, AnalyticalModelParameter analyticalModelParameter)
            {
                if (space.TryGetValue(spaceParameter, out double value) && !double.IsNaN(value) && value != 0)
                {
                    return Measure(value, UnitType.Unitless, ReportValueSource.SAM, spaceParameter.ToString(), "No sizing factor set");
                }

                if (documentContext.AnalyticalModel.TryGetValue(analyticalModelParameter, out double value_Model) && !double.IsNaN(value_Model) && value_Model != 0)
                {
                    ReportValue<Quantity> result = Measure(value_Model, UnitType.Unitless, ReportValueSource.SAM, analyticalModelParameter.ToString(), "No sizing factor set");
                    return result.HasValue ? ReportValue<Quantity>.Available(result.Value, ReportValueSource.SAM, note: "Model default") : result;
                }

                // Set neither on the space nor on the model: SAM_Tas (Modify.UpdateSizingFactors) then leaves the zone
                // load unscaled. That is a known "no multiplier", not missing data.
                return ReportValue<Quantity>.NotApplicable(SizingFactorNotSet);
            }
        }
    }
}
