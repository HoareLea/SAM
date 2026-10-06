// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// Builds the small analytical models the Approved Document F tests need: spaces with an area and a
    /// volume, internal partitions that make two spaces adjacent, external walls with windows, and door
    /// apertures in those partitions.
    /// <para>
    /// Adjacency is what the Part F transfer air network is built from, so a test that exercises transfer
    /// air has to create real panels with real space relations rather than loose spaces. The rest of the
    /// Part F tests only need rates, and those still use loose spaces.
    /// </para>
    /// </summary>
    public class PartFModel
    {
        private double x = 0;

        private readonly Dictionary<string, Space> dictionary_Space = [];

        /// <summary>The model being built.</summary>
        public AdjacencyCluster AdjacencyCluster { get; } = new();

        /// <summary>Adds a space with a floor area and a volume.</summary>
        public PartFModel Space(string name, double area_M2, double volume_M3)
        {
            Space space = new(name, new Point3D(x, 0, 1.5));
            space.SetValue(SpaceParameter.Area, area_M2);
            space.SetValue(SpaceParameter.Volume, volume_M3);

            x += 10;

            dictionary_Space[name] = space;
            AdjacencyCluster.AddObject(space);

            return this;
        }

        /// <summary>Records how the local kitchen or cooker extract of a cooking space is provided.</summary>
        public PartFModel LocalExtractMethod(string name, Analytical.Enums.PartFExtractMethod partFExtractMethod)
        {
            Space space = dictionary_Space[name];
            space.SetValue(SpaceParameter.PartFLocalExtractMethod, partFExtractMethod.ToString());
            AdjacencyCluster.AddObject(space);

            return this;
        }

        /// <summary>Assigns a space to a dwelling zone of the given category.</summary>
        public PartFModel Zone(string name_Zone, string zoneCategory, bool? isDwelling, params string[] names_Space)
        {
            Zone zone = AdjacencyCluster.GetZones()?.Find(y => y.Name == name_Zone);
            if (zone is null)
            {
                zone = new Zone(name_Zone);
                zone.SetValue(ZoneParameter.ZoneCategory, zoneCategory);

                if (isDwelling is not null)
                {
                    zone.SetValue(ZoneParameter.IsDwelling, isDwelling.Value);
                }

                AdjacencyCluster.AddObject(zone);
            }

            foreach (string name_Space in names_Space)
            {
                AdjacencyCluster.AddRelation(zone, dictionary_Space[name_Space]);
            }

            return this;
        }

        /// <summary>
        /// Makes two spaces adjacent through an internal partition, optionally with a named door aperture
        /// in it. This is what creates an edge in the transfer air network.
        /// </summary>
        public PartFModel Partition(string name_1, string name_2, string name_Door = null, double doorWidth_M = 0.9)
        {
            double x_Panel = x;
            x += 10;

            Panel panel = AnalyticalCreate.Panel(new Construction(Guid.NewGuid(), "Internal Partition"), PanelType.WallInternal, Wall(x_Panel, 3));

            if (name_Door is not null)
            {
                //A door aperture whose bounding box is doorWidth_M wide, so the clear width the transfer
                //assessment reads from the geometry is a real number rather than a guess. An aperture takes
                //its name from its construction.
                panel.AddAperture(AnalyticalCreate.Aperture(
                    new ApertureConstruction(name_Door, ApertureType.Door),
                    Door(x_Panel, doorWidth_M)));
            }

            AdjacencyCluster.AddObject(panel);
            AdjacencyCluster.AddRelation(dictionary_Space[name_1], panel);
            AdjacencyCluster.AddRelation(dictionary_Space[name_2], panel);

            return this;
        }

        /// <summary>
        /// Gives a space an external wall, optionally with a window in it. An external element is one with
        /// a single adjacent space, which is what the purge assessment reads for a route to the outside.
        /// </summary>
        public PartFModel ExternalWall(string name, bool window = true)
        {
            double x_Panel = x;
            x += 10;

            Panel panel = AnalyticalCreate.Panel(new Construction(Guid.NewGuid(), "External Wall"), PanelType.WallExternal, Wall(x_Panel, 3));

            if (window)
            {
                panel.AddAperture(AnalyticalCreate.Aperture(new ApertureConstruction("Window", ApertureType.Window), Door(x_Panel, 1.2)));
            }

            AdjacencyCluster.AddObject(panel);
            AdjacencyCluster.AddRelation(dictionary_Space[name], panel);

            return this;
        }

        /// <summary>The space of the given name, so a test can read its Part F data back.</summary>
        public Space Get(string name)
        {
            return dictionary_Space[name];
        }

        /// <summary>
        /// Records the engineering inputs on a door: what transfer provision it has, how deep the undercut
        /// is, whether the floor finish is fitted, and any transfer flow override. These are exactly the
        /// values the calculation carries forward rather than overwriting.
        /// </summary>
        public PartFModel DoorInput(
            string name_Door,
            Analytical.Enums.PartFTransferDeviceType partFTransferDeviceType = Analytical.Enums.PartFTransferDeviceType.DoorUndercut,
            double? providedUndercutHeight_mm = null,
            double? providedFreeArea_mm2 = null,
            bool? isFloorFinishFitted = null,
            double? transferFlowRateOverride_Lps = null)
        {
            foreach (Panel panel in AdjacencyCluster.GetPanels() ?? [])
            {
                Aperture aperture = panel?.Apertures?.Find(x => x is not null && x.Name == name_Door);
                if (aperture is null)
                {
                    continue;
                }

                //Through the shared helper, because Panel.Apertures hands out clones and setting a
                //parameter on one changes a copy and nothing else.
                AdjacencyCluster.SetPartFDoorTransferData(aperture.Guid, new PartFDoorTransferData(name_Door)
                {
                    TransferDeviceType = partFTransferDeviceType,
                    ProvidedUndercutHeight_mm = providedUndercutHeight_mm,
                    ProvidedFreeArea_mm2 = providedFreeArea_mm2,
                    IsFloorFinishFitted = isFloorFinishFitted,
                    TransferFlowRateOverride_Lps = transferFlowRateOverride_Lps,
                });

                return this;
            }

            throw new System.ArgumentException(string.Format("No door aperture named '{0}' is in the model.", name_Door), nameof(name_Door));
        }

        /// <summary>Records the purge ventilation inputs of a habitable room.</summary>
        public PartFModel PurgeInput(
            string name_Space,
            Analytical.Enums.PartFPurgeMethod partFPurgeMethod = Analytical.Enums.PartFPurgeMethod.Openings,
            Analytical.Enums.PartFPurgeOpeningType partFPurgeOpeningType = Analytical.Enums.PartFPurgeOpeningType.Undefined,
            double? openableWindowArea_M2 = null,
            double? externalDoorOpeningArea_M2 = null,
            double? mechanicalPurgeCapacity_Lps = null,
            double? openingAngle_Degrees = null)
        {
            Space space = dictionary_Space[name_Space];

            //Carried on the space's Part F data, which is where the calculation reads it back from.
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData) ?? new PartFSpaceData();

            partFSpaceData.Purge = new PartFPurgeVentilationData(name_Space)
            {
                PurgeMethod = partFPurgeMethod,
                OpeningType = partFPurgeOpeningType,
                OpenableWindowArea_M2 = openableWindowArea_M2,
                ExternalDoorOpeningArea_M2 = externalDoorOpeningArea_M2,
                MechanicalPurgeCapacity_Lps = mechanicalPurgeCapacity_Lps,
                OpeningAngle_Degrees = openingAngle_Degrees,
            };

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);
            AdjacencyCluster.AddObject(space);

            return this;
        }

        /// <summary>Records commissioning evidence on a dwelling zone.</summary>
        public PartFModel Commissioning(string name_Zone, PartFCommissioningData partFCommissioningData)
        {
            Zone zone = AdjacencyCluster.GetZones().Find(x => x.Name == name_Zone);

            zone.SetValue(ZoneParameter.PartFCommissioningData, partFCommissioningData);
            AdjacencyCluster.AddObject(zone);

            return this;
        }

        private static Face3D Wall(double x, double height)
        {
            return new Face3D(new Polygon3D(
            [
                new Point3D(x, 0, 0),
                new Point3D(x + 4, 0, 0),
                new Point3D(x + 4, 0, height),
                new Point3D(x, 0, height),
            ]));
        }

        private static Face3D Door(double x, double width)
        {
            return new Face3D(new Polygon3D(
            [
                new Point3D(x + 0.5, 0, 0),
                new Point3D(x + 0.5 + width, 0, 0),
                new Point3D(x + 0.5 + width, 0, 2),
                new Point3D(x + 0.5, 0, 2),
            ]));
        }
    }
}
