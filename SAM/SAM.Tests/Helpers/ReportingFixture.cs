// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Core.Reporting;
using SAM.Geometry.Spatial;
using SAM.Units;
using SAM.Weather;
using System;
using System.Globalization;
using System.Linq;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// Small, fully specified analytical models for the reporting tests. Every value the Space Assumptions document
    /// shows is set explicitly, and every identifier that reaches the document (space GUID, generation time, SAM
    /// version) is fixed, so the golden snapshots are deterministic.
    /// </summary>
    public static class ReportingFixture
    {
        public static readonly Guid SpaceGuid = new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301");

        public const string SpaceName = "00_011 Office";

        public static readonly DateTime GeneratedAt = new DateTime(2026, 9, 25, 14, 2, 0);

        public const string SoftwareVersion = "2026.3.0-test";

        /// <summary>
        /// Options with the fixed generation time, version and culture used by every snapshot.
        /// </summary>
        public static DocumentOptions Options(UnitStyle unitStyle = UnitStyle.SI)
        {
            return new DocumentOptions()
            {
                UnitSystem = unitStyle,
                Culture = CultureInfo.GetCultureInfo("en-GB"),
                GeneratedAt = GeneratedAt,
                SoftwareVersion = SoftwareVersion,
                Metadata = new DocumentMetadata() { ProjectName = "Test Project", ProjectNumber = "P-001", PreparedBy = "SAM Tests" },
            };
        }

        /// <summary>
        /// A 30 m² / 90 m³ office with a full internal condition, set point profiles, design days, a ventilation /
        /// heating / cooling system, risers, fabric (external wall with a window, internal wall with a door, ground
        /// slab, roof) and persisted design loads of 779 W heating and 12 400 W cooling.
        /// </summary>
        public static AnalyticalModel Full(out Space space)
        {
            ProfileLibrary profileLibrary = new ProfileLibrary("Test Profiles");
            profileLibrary.Add(new Profile("Occ 8to19", ProfileType.Occupancy, Enumerable.Range(0, 24).Select(x => x >= 8 && x < 19 ? 1.0 : 0.0)));
            profileLibrary.Add(new Profile("Light 8to19", ProfileType.Lighting, Enumerable.Range(0, 24).Select(x => x >= 8 && x < 19 ? 1.0 : 0.0)));
            profileLibrary.Add(new Profile("Equip 8to19", ProfileType.EquipmentSensible, Enumerable.Range(0, 24).Select(x => x >= 8 && x < 19 ? 1.0 : 0.0)));
            profileLibrary.Add(new Profile("Inf Const", ProfileType.Infiltration, Enumerable.Repeat(1.0, 24)));
            profileLibrary.Add(new Profile("Heat 21", ProfileType.Heating, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 19 ? 21.0 : 16.0)));
            profileLibrary.Add(new Profile("Cool 24", ProfileType.Cooling, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 19 ? 24.0 : 28.0)));
            profileLibrary.Add(new Profile("Hum 40", ProfileType.Humidification, Enumerable.Repeat(40.0, 24)));
            profileLibrary.Add(new Profile("Dehum 60", ProfileType.Dehumidification, Enumerable.Repeat(60.0, 24)));

            InternalCondition internalCondition = new InternalCondition(new Guid("6f9619ff-8b86-d011-b42d-00c04fc964ff"), "S39_OfficeOpen");
            internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, 10.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, "Occ 8to19");
            internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, 75.0);
            internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, 55.0);
            internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, 8.0);
            internalCondition.SetValue(InternalConditionParameter.LightingLevel, 500.0);
            internalCondition.SetValue(InternalConditionParameter.LightingProfileName, "Light 8to19");
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, 25.0);
            internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, "Equip 8to19");
            internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, 0.2);
            internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, "Inf Const");
            internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, "Heat 21");
            internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, "Cool 24");
            internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, "Hum 40");
            internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, "Dehum 60");

            space = new Space(SpaceGuid, SpaceName, new Point3D(2, 2, 1.5));
            space.InternalCondition = internalCondition;
            space.SetValue(SpaceParameter.Area, 30.0);
            space.SetValue(SpaceParameter.Volume, 90.0);
            space.SetValue(SpaceParameter.LevelName, "Level 00");
            space.SetValue(SpaceParameter.SupplyAirFlow, 0.198);
            space.SetValue(SpaceParameter.ExhaustAirFlow, 0.180);
            space.SetValue(SpaceParameter.OutsideSupplyAirFlow, 0.040);
            space.SetValue(SpaceParameter.DesignHeatingLoad, 779.0);
            space.SetValue(SpaceParameter.DesignCoolingLoad, 12400.0);
            space.SetValue(SpaceParameter.HeatingSizingFactor, 1.2);
            space.SetValue(SpaceParameter.VentilationRiserName, "V R1");
            space.SetValue(SpaceParameter.HeatingRiserName, "H R2");

            Space space_Adjacent = new Space(new Guid("a8098c1a-f86e-11da-bd1a-00112444be1e"), "00_012 Corridor", new Point3D(2, -2, 1.5));

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(space_Adjacent);

            // External wall 4 m x 3 m with a 1.2 m x 2 m window.
            Panel panel_ExternalWall = AnalyticalCreate.Panel(new Construction(new Guid("11111111-1111-1111-1111-111111111111"), "External Wall"), PanelType.WallExternal, Rectangle(0, 4, 0, 3));
            panel_ExternalWall.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Window", ApertureType.Window), Rectangle(1, 2.2, 0.5, 2.5)));
            AddPanel(adjacencyCluster, panel_ExternalWall, space);

            // Internal wall 4 m x 3 m to the corridor, with a 0.9 m x 2 m door.
            Panel panel_InternalWall = AnalyticalCreate.Panel(new Construction(new Guid("22222222-2222-2222-2222-222222222222"), "Internal Partition"), PanelType.WallInternal, Rectangle(10, 14, 0, 3));
            panel_InternalWall.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Door", ApertureType.Door), Rectangle(10.5, 11.4, 0, 2)));
            AddPanel(adjacencyCluster, panel_InternalWall, space, space_Adjacent);

            AddPanel(adjacencyCluster, AnalyticalCreate.Panel(new Construction(new Guid("33333333-3333-3333-3333-333333333333"), "Ground Slab"), PanelType.SlabOnGrade, Horizontal(0, 6, 0, 5, 0)), space);
            AddPanel(adjacencyCluster, AnalyticalCreate.Panel(new Construction(new Guid("44444444-4444-4444-4444-444444444444"), "Roof"), PanelType.Roof, Horizontal(0, 6, 0, 5, 3)), space);

            VentilationSystem ventilationSystem = new VentilationSystem("VS1", new VentilationSystemType("VAV", "Variable air volume"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, "AHU1S");
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, "AHU1E");
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(space, ventilationSystem);

            HeatingSystem heatingSystem = new HeatingSystem("HS1", new HeatingSystemType("UFH", "Underfloor heating"));
            adjacencyCluster.AddObject(heatingSystem);
            adjacencyCluster.AddRelation(space, heatingSystem);

            CoolingSystem coolingSystem = new CoolingSystem("CS1", new CoolingSystemType("FCU", "Fan coil unit"));
            adjacencyCluster.AddObject(coolingSystem);
            adjacencyCluster.AddRelation(space, coolingSystem);

            AnalyticalModel analyticalModel = new AnalyticalModel("Office", null, null, null, adjacencyCluster, null, profileLibrary);
            analyticalModel.SetValue(AnalyticalModelParameter.CoolingSizingFactor, 1.1);
            analyticalModel.SetValue(AnalyticalModelParameter.HeatingDesignDays, new SAMCollection<DesignDay>() { DesignDay("Winter DD", -3.0, 86.9, false) });
            analyticalModel.SetValue(AnalyticalModelParameter.CoolingDesignDays, new SAMCollection<DesignDay>() { DesignDay("Summer DD", 32.1, 35.9, true) });

            return analyticalModel;
        }

        /// <summary>
        /// A space with nothing but a name: no internal condition, geometry, systems, panels, design days or loads.
        /// </summary>
        public static AnalyticalModel Minimal(out Space space)
        {
            space = new Space(SpaceGuid, SpaceName, new Point3D(0, 0, 0));

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);

            return new AnalyticalModel("Empty", null, null, null, adjacencyCluster, null, null);
        }

        /// <summary>
        /// The space as stored in the model (the model clones what it is given).
        /// </summary>
        public static Space Stored(AnalyticalModel analyticalModel)
        {
            return analyticalModel.AdjacencyCluster.GetObject<Space>(SpaceGuid);
        }

        /// <summary>
        /// A 24-hour design day whose dry bulb peaks (cooling) or bottoms out (heating) at 15:00 / 05:00, with the
        /// given relative humidity at that hour and 50 % elsewhere.
        /// </summary>
        private static DesignDay DesignDay(string name, double extremeDryBulb, double relativeHumidity, bool cooling)
        {
            DesignDay designDay = new DesignDay(name, 2018, (byte)(cooling ? 7 : 1), 15);

            int hour = cooling ? 15 : 5;
            double[] dryBulbs = Enumerable.Range(0, 24).Select(x => x == hour ? extremeDryBulb : extremeDryBulb + (cooling ? -5.0 : 4.0)).ToArray();
            double[] relativeHumidities = Enumerable.Range(0, 24).Select(x => x == hour ? relativeHumidity : 50.0).ToArray();

            designDay[WeatherDataType.DryBulbTemperature] = dryBulbs;
            designDay[WeatherDataType.RelativeHumidity] = relativeHumidities;

            return designDay;
        }

        private static void AddPanel(AdjacencyCluster adjacencyCluster, Panel panel, params Space[] spaces)
        {
            adjacencyCluster.AddObject(panel);
            foreach (Space space in spaces)
            {
                adjacencyCluster.AddRelation(space, panel);
            }
        }

        /// <summary>
        /// A vertical rectangle in the XZ plane.
        /// </summary>
        private static Face3D Rectangle(double x_Min, double x_Max, double z_Min, double z_Max)
        {
            return new Face3D(new Polygon3D(new[]
            {
                new Point3D(x_Min, 0, z_Min),
                new Point3D(x_Max, 0, z_Min),
                new Point3D(x_Max, 0, z_Max),
                new Point3D(x_Min, 0, z_Max),
            }));
        }

        private static Face3D Horizontal(double x_Min, double x_Max, double y_Min, double y_Max, double z)
        {
            return new Face3D(new Polygon3D(new[]
            {
                new Point3D(x_Min, y_Min, z),
                new Point3D(x_Max, y_Min, z),
                new Point3D(x_Max, y_Max, z),
                new Point3D(x_Min, y_Max, z),
            }));
        }
    }
}
