// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Reporting;
using SAM.Core;
using SAM.Core.Reporting;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using SAM.Units;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AnalyticalCreate = SAM.Analytical.Create;
using ReportingCreate = SAM.Analytical.Reporting.Create;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// The representative spaces of the Space Assumptions visual design gate (2026-09-25): an open-plan office, a
    /// large atrium with long names and kW loads, and a sparse store with missing data. They are realistic models
    /// built with the SAM API, not hand-written report values, so the PDF tests render real builder output. The
    /// space GUIDs, generation time and version are fixed so the documents are deterministic.
    /// </summary>
    public static class ReportingDesignGateFixture
    {
        public const string OfficeName = "00_011 Open Plan Office";
        public const string AtriumName = "01_101 Atrium & Reception (double height)";
        public const string SparseName = "00_014 Store / Cleaner";

        private static readonly Guid officeGuid = new Guid("4bab0f86-f81c-441c-ad8b-ab3480468e90");
        private static readonly Guid atriumGuid = new Guid("9d1c6f2a-3b7e-4c55-8a0e-6f1b2c3d4e5f");
        private static readonly Guid sparseGuid = new Guid("1e2d3c4b-5a69-4788-9a0b-c1d2e3f40516");

        private static readonly DateTime generatedAt = new DateTime(2026, 9, 25, 14, 2, 0);

        /// <summary>
        /// The four design-gate cases: name, model, space name and unit system.
        /// </summary>
        public static IEnumerable<(string Name, Func<AnalyticalModel> Model, string SpaceName, UnitStyle UnitStyle)> Cases()
        {
            yield return ("office_SI", Office, OfficeName, UnitStyle.SI);
            yield return ("office_IP", Office, OfficeName, UnitStyle.Imperial);
            yield return ("atrium_SI", Atrium, AtriumName, UnitStyle.SI);
            yield return ("sparse_SI", Sparse, SparseName, UnitStyle.SI);
        }

        public static Document Document(string name)
        {
            (string _, Func<AnalyticalModel> model, string spaceName, UnitStyle unitStyle) = Cases().Single(x => x.Name == name);
            return Document(model(), spaceName, unitStyle);
        }

        public static Document Document(AnalyticalModel analyticalModel, string spaceName, UnitStyle unitStyle, DocumentStyle? documentStyle = null)
        {
            DocumentOptions documentOptions = new DocumentOptions()
            {
                UnitSystem = unitStyle,
                Culture = CultureInfo.GetCultureInfo("en-GB"),
                GeneratedAt = generatedAt,
                SoftwareVersion = "2026.3.182",
                Metadata = new DocumentMetadata() { ProjectName = "Riverside House Refurbishment", ProjectNumber = "P-2026-014", PreparedBy = "M. Engineer" },
            };

            if (documentStyle != null)
            {
                documentOptions.Style = documentStyle;
            }

            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, documentOptions);
            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Single(x => x.Name == spaceName);
            return ReportingCreate.SpaceAssumptions(documentContext, space);
        }

        /// <summary>
        /// A ground-floor open-plan office: two external walls with framed windows, two internal walls (one with a
        /// door), ground slab, internal ceiling. Week profiles made of day profiles (as imported from Tas), heating /
        /// cooling set-back, humidification disabled (0 %), dehumidification 60 %, equipment latent authored as 0.
        /// </summary>
        public static AnalyticalModel Office()
        {
            ProfileLibrary profileLibrary = Profiles(out string occupancy, out string lighting, out string equipment);

            InternalCondition internalCondition = new InternalCondition("S39_Office_OpenPlan_NCM");
            internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, 10.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, occupancy);
            internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, 75.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, 55.0);
            internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, 8.0);
            internalCondition.SetValue(InternalConditionParameter.LightingLevel, 500.0);
            internalCondition.SetValue(InternalConditionParameter.LightingProfileName, lighting);
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, 15.0);
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, equipment);
            internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, 0.0);
            internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, equipment);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, 0.25);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, "Infiltration Constant");
            internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, "Office Heating 21/16 C Week");
            internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, "Office Cooling 24/28 C Week");
            internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, "No Humidification");
            internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, "Office Dehumidification 60 % Week");

            Space space = new Space(officeGuid, OfficeName, new Point3D(4, 2.5, 1.5));
            space.InternalCondition = internalCondition;
            space.SetValue(SpaceParameter.Area, 39.3);
            space.SetValue(SpaceParameter.Volume, 174.7);
            space.SetValue(SpaceParameter.LevelName, "Level 00 (Ground)");
            space.SetValue(SpaceParameter.SupplyAirFlow, 0.198);
            space.SetValue(SpaceParameter.ExhaustAirFlow, 0.180);
            space.SetValue(SpaceParameter.OutsideSupplyAirFlow, 0.040);
            space.SetValue(SpaceParameter.DesignHeatingLoad, 779.0);
            space.SetValue(SpaceParameter.DesignCoolingLoad, 1429.0);
            space.SetValue(SpaceParameter.HeatingSizingFactor, 1.2);
            space.SetValue(SpaceParameter.VentilationRiserName, "V-R1");
            space.SetValue(SpaceParameter.HeatingRiserName, "H-R2");
            space.SetValue(SpaceParameter.CoolingRiserName, "C-R3");

            Space corridor = new Space(new Guid("00000000-0000-0000-0000-000000000012"), "00_012 Corridor", new Point3D(4, -2, 1.5));
            Space meetingRoom = new Space(new Guid("00000000-0000-0000-0000-000000000013"), "00_013 Meeting Room", new Point3D(10, 2.5, 1.5));
            Space officeAbove = new Space(new Guid("00000000-0000-0000-0000-000000000111"), "01_011 Open Plan Office", new Point3D(4, 2.5, 6));

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            foreach (Space space_Temp in new[] { space, corridor, meetingRoom, officeAbove })
            {
                adjacencyCluster.AddObject(space_Temp);
            }

            Panel wall_South = Wall(PanelType.WallExternal, "External Wall - Brick Cavity", 0, 8.0, 4.45);
            wall_South.AddAperture(Window(1.0, 1.8, 0.9, 2.1));
            wall_South.AddAperture(Window(4.5, 1.8, 0.9, 2.1));
            Add(adjacencyCluster, wall_South, space);

            Panel wall_West = Wall(PanelType.WallExternal, "External Wall - Brick Cavity", 20, 4.9, 4.45);
            wall_West.AddAperture(Window(21.5, 1.8, 0.9, 2.1));
            Add(adjacencyCluster, wall_West, space);

            Panel wall_Corridor = Wall(PanelType.WallInternal, "Internal Partition - Stud", 40, 8.0, 4.45);
            wall_Corridor.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Internal Door", ApertureType.Door), Rectangle(43, 44.0, 0, 2.1)));
            Add(adjacencyCluster, wall_Corridor, space, corridor);

            Add(adjacencyCluster, Wall(PanelType.WallInternal, "Internal Partition - Stud", 60, 4.9, 4.45), space, meetingRoom);
            Add(adjacencyCluster, AnalyticalCreate.Panel(new Construction("Ground Floor Slab"), PanelType.SlabOnGrade, Horizontal(8.0, 4.9125, 0)), space);
            Add(adjacencyCluster, AnalyticalCreate.Panel(new Construction("Intermediate Floor"), PanelType.Ceiling, Horizontal(8.0, 4.9125, 4.45)), space, officeAbove);

            Systems(adjacencyCluster, space, "VAV with heat recovery", "AHU-01 Supply", "AHU-01 Extract", "Underfloor heating", "Fan coil unit (4-pipe)");

            return Model(adjacencyCluster, profileLibrary);
        }

        /// <summary>
        /// A large double-height atrium: cooling load above 10 kW (kW switch), long names, extensive glazing.
        /// </summary>
        public static AnalyticalModel Atrium()
        {
            ProfileLibrary profileLibrary = Profiles(out string occupancy, out string lighting, out string equipment);

            InternalCondition internalCondition = new InternalCondition("S12_Reception_Atrium_NCM_CirculationArea");
            internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, 20.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, occupancy);
            internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, 70.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, 45.0);
            internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, 6.5);
            internalCondition.SetValue(InternalConditionParameter.LightingLevel, 200.0);
            internalCondition.SetValue(InternalConditionParameter.LightingProfileName, lighting);
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, 5.0);
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, equipment);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, 0.5);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, "Infiltration Constant");
            internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, "Office Heating 21/16 C Week");
            internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, "Office Cooling 24/28 C Week");
            internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, "Humidification 40 %");
            internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, "Office Dehumidification 60 % Week");

            Space space = new Space(atriumGuid, AtriumName, new Point3D(10, 6, 3));
            space.InternalCondition = internalCondition;
            space.SetValue(SpaceParameter.Area, 186.4);
            space.SetValue(SpaceParameter.Volume, 1491.2);
            space.SetValue(SpaceParameter.LevelName, "Level 00 (Ground)");
            space.SetValue(SpaceParameter.SupplyAirFlow, 1.250);
            space.SetValue(SpaceParameter.ExhaustAirFlow, 1.125);
            space.SetValue(SpaceParameter.OutsideSupplyAirFlow, 0.466);
            space.SetValue(SpaceParameter.DesignHeatingLoad, 8450.0);
            space.SetValue(SpaceParameter.DesignCoolingLoad, 24600.0);
            space.SetValue(SpaceParameter.VentilationRiserName, "V-R4");

            Space backOffice = new Space(new Guid("00000000-0000-0000-0000-000000000020"), "00_020 Back Office", new Point3D(30, 6, 1.5));

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(backOffice);

            Panel curtainWall = Wall(PanelType.CurtainWall, "Curtain Wall - Unitised", 0, 16.0, 8.0);
            for (int i = 0; i < 4; i++)
            {
                curtainWall.AddAperture(Window(0.5 + (i * 4.0), 3.0, 0.5, 7.0));
            }

            Add(adjacencyCluster, curtainWall, space);

            Panel wall_North = Wall(PanelType.WallExternal, "External Wall - Rainscreen", 20, 16.0, 8.0);
            wall_North.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Revolving Door", ApertureType.Door), Rectangle(26, 29, 0, 2.7)));
            Add(adjacencyCluster, wall_North, space);

            Add(adjacencyCluster, Wall(PanelType.WallInternal, "Internal Partition - Blockwork", 40, 11.65, 8.0), space, backOffice);
            Add(adjacencyCluster, AnalyticalCreate.Panel(new Construction("Ground Floor Slab"), PanelType.SlabOnGrade, Horizontal(16.0, 11.65, 0)), space);
            Add(adjacencyCluster, AnalyticalCreate.Panel(new Construction("Roof - Rooflight Zone"), PanelType.Roof, Horizontal(16.0, 11.65, 8.0)), space);

            Systems(adjacencyCluster, space, "Displacement ventilation", "AHU-02 Supply", "AHU-02 Extract", "Perimeter trench heating", "Chilled floor + AHU cooling");

            AnalyticalModel analyticalModel = Model(adjacencyCluster, profileLibrary);
            analyticalModel.SetValue(AnalyticalModelParameter.CoolingSizingFactor, 1.1);
            return analyticalModel;
        }

        /// <summary>
        /// A store: internal condition with no occupancy latent gain and no lighting level authored, no systems, no
        /// design loads, no design days in the model.
        /// </summary>
        public static AnalyticalModel Sparse()
        {
            ProfileLibrary profileLibrary = Profiles(out string occupancy, out string lighting, out string _);

            InternalCondition internalCondition = new InternalCondition("S70_Store_Unconditioned");
            internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, 50.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, occupancy);
            internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, 90.0);
            internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, 3.0);
            internalCondition.SetValue(InternalConditionParameter.LightingProfileName, lighting);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, 0.5);

            Space space = new Space(sparseGuid, SparseName, new Point3D(1, 1, 1.5));
            space.InternalCondition = internalCondition;
            space.SetValue(SpaceParameter.Area, 6.2);
            space.SetValue(SpaceParameter.Volume, 17.4);
            space.SetValue(SpaceParameter.LevelName, "Level 00 (Ground)");

            Space corridor = new Space(new Guid("00000000-0000-0000-0000-000000000012"), "00_012 Corridor", new Point3D(1, -2, 1.5));

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(corridor);

            Panel wall_Corridor = Wall(PanelType.WallInternal, "Internal Partition - Stud", 0, 2.5, 2.8);
            wall_Corridor.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Internal Door", ApertureType.Door), Rectangle(0.5, 1.4, 0, 2.1)));
            Add(adjacencyCluster, wall_Corridor, space, corridor);
            Add(adjacencyCluster, AnalyticalCreate.Panel(new Construction("Ground Floor Slab"), PanelType.SlabOnGrade, Horizontal(2.5, 2.48, 0)), space);

            return new AnalyticalModel("Riverside House", null, null, null, adjacencyCluster, null, profileLibrary);
        }

        // ---------------------------------------------------------------- helpers

        private static AnalyticalModel Model(AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary)
        {
            AnalyticalModel analyticalModel = new AnalyticalModel("Riverside House", null, null, null, adjacencyCluster, null, profileLibrary);
            analyticalModel.SetValue(AnalyticalModelParameter.HeatingDesignDays, new SAMCollection<DesignDay>() { DesignDay("CIBSE London Winter 99.6 %", -3.0, 86.9, false) });
            analyticalModel.SetValue(AnalyticalModelParameter.CoolingDesignDays, new SAMCollection<DesignDay>() { DesignDay("CIBSE London Summer 0.4 % (July)", 32.1, 35.9, true) });
            return analyticalModel;
        }

        private static void Systems(AdjacencyCluster adjacencyCluster, Space space, string ventilation, string supplyUnit, string extractUnit, string heating, string cooling)
        {
            VentilationSystem ventilationSystem = new VentilationSystem("VS", new VentilationSystemType(ventilation, ventilation));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnit);
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, extractUnit);
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(space, ventilationSystem);

            HeatingSystem heatingSystem = new HeatingSystem("HS", new HeatingSystemType(heating, heating));
            adjacencyCluster.AddObject(heatingSystem);
            adjacencyCluster.AddRelation(space, heatingSystem);

            CoolingSystem coolingSystem = new CoolingSystem("CS", new CoolingSystemType(cooling, cooling));
            adjacencyCluster.AddObject(coolingSystem);
            adjacencyCluster.AddRelation(space, coolingSystem);
        }

        /// <summary>
        /// Weekday / weekend day profiles assembled into week profiles, the way Tas-imported models carry them.
        /// </summary>
        private static ProfileLibrary Profiles(out string occupancy, out string lighting, out string equipment)
        {
            ProfileLibrary profileLibrary = new ProfileLibrary("Design-gate profiles");

            occupancy = "NCM Office Occupancy Wkday 07-19";
            lighting = "NCM Office Lighting Wkday 07-19";
            equipment = "NCM Office Equipment Wkday 07-19";

            profileLibrary.Add(Week(occupancy, ProfileType.Occupancy, h => h >= 7 && h < 19 ? 1.0 : 0.0, h => 0.0));
            profileLibrary.Add(Week(lighting, ProfileType.Lighting, h => h >= 7 && h < 19 ? 1.0 : 0.0, h => 0.0));
            profileLibrary.Add(Week(equipment, ProfileType.EquipmentSensible, h => h >= 7 && h < 19 ? 1.0 : 0.2, h => 0.2));
            profileLibrary.Add(new Profile("Infiltration Constant", ProfileType.Infiltration, Enumerable.Repeat(1.0, 24)));
            profileLibrary.Add(Week("Office Heating 21/16 C Week", ProfileType.Heating, h => h >= 7 && h < 19 ? 21.0 : 16.0, h => 16.0));
            profileLibrary.Add(Week("Office Cooling 24/28 C Week", ProfileType.Cooling, h => h >= 7 && h < 19 ? 24.0 : 28.0, h => 28.0));
            profileLibrary.Add(new Profile("No Humidification", ProfileType.Humidification, Enumerable.Repeat(0.0, 24)));
            profileLibrary.Add(new Profile("Humidification 40 %", ProfileType.Humidification, Enumerable.Repeat(40.0, 24)));
            profileLibrary.Add(Week("Office Dehumidification 60 % Week", ProfileType.Dehumidification, h => h >= 7 && h < 19 ? 60.0 : 100.0, h => 100.0));

            return profileLibrary;
        }

        private static Profile Week(string name, ProfileType profileType, Func<int, double> weekday, Func<int, double> weekend)
        {
            Profile profile_Weekday = new Profile(name + " (weekday)", profileType, Enumerable.Range(0, 24).Select(weekday));
            Profile profile_Weekend = new Profile(name + " (weekend)", profileType, Enumerable.Range(0, 24).Select(weekend));

            Profile result = new Profile(name, profileType);
            for (int i = 0; i < 5; i++)
            {
                result.Add(profile_Weekday);
            }

            result.Add(profile_Weekend);
            result.Add(profile_Weekend);
            return result;
        }

        private static DesignDay DesignDay(string name, double extremeDryBulb, double relativeHumidity, bool cooling)
        {
            DesignDay designDay = new DesignDay(name, 2018, (byte)(cooling ? 7 : 1), 15);
            int hour = cooling ? 15 : 5;
            designDay[WeatherDataType.DryBulbTemperature] = Enumerable.Range(0, 24).Select(x => x == hour ? extremeDryBulb : extremeDryBulb + (cooling ? -6.0 : 3.0)).ToArray();
            designDay[WeatherDataType.RelativeHumidity] = Enumerable.Range(0, 24).Select(x => x == hour ? relativeHumidity : 55.0).ToArray();
            return designDay;
        }

        private static void Add(AdjacencyCluster adjacencyCluster, Panel panel, params Space[] spaces)
        {
            adjacencyCluster.AddObject(panel);
            foreach (Space space in spaces)
            {
                adjacencyCluster.AddRelation(space, panel);
            }
        }

        private static Panel Wall(PanelType panelType, string construction, double x, double length, double height)
        {
            return AnalyticalCreate.Panel(new Construction(construction), panelType, Rectangle(x, x + length, 0, height));
        }

        /// <summary>
        /// A window with a 70 mm frame: an outer rectangle with the pane as an inner edge, so SAM reports pane and
        /// frame areas separately.
        /// </summary>
        private static Aperture Window(double x, double width, double sill, double head)
        {
            const double frame = 0.07;

            Plane plane = new Plane(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 0, 1));
            Polygon2D outer = plane.Convert(Polygon(x, x + width, sill, head));
            Polygon2D inner = plane.Convert(Polygon(x + frame, x + width - frame, sill + frame, head - frame));
            Face3D face3D = new Face3D(plane, SAM.Geometry.Planar.Create.Face2D(outer, new IClosed2D[] { inner }));

            return AnalyticalCreate.Aperture(new ApertureConstruction("Double Glazed Window", ApertureType.Window), face3D);
        }

        private static Polygon3D Polygon(double x_Min, double x_Max, double z_Min, double z_Max)
        {
            return new Polygon3D(new[] { new Point3D(x_Min, 0, z_Min), new Point3D(x_Max, 0, z_Min), new Point3D(x_Max, 0, z_Max), new Point3D(x_Min, 0, z_Max) });
        }

        private static Face3D Rectangle(double x_Min, double x_Max, double z_Min, double z_Max)
        {
            return new Face3D(Polygon(x_Min, x_Max, z_Min, z_Max));
        }

        private static Face3D Horizontal(double length, double width, double z)
        {
            return new Face3D(new Polygon3D(new[] { new Point3D(0, 0, z), new Point3D(length, 0, z), new Point3D(length, width, z), new Point3D(0, width, z) }));
        }
    }
}
