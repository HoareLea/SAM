// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// <b>Whether an authored ventilation system is duty-bearing plant or inert scaffolding</b> - the effective-duty
        /// classification of Part O PR-2 (owner decision 1 of SAM_UI <c>documentation/PartO-ModelStateArchitecture.md</c>).
        ///
        /// <para><b>The case</b></para>
        /// <para>
        /// <c>Modify.AddMechanicalSystems</c> names a unit on every mechanical template system and creates it
        /// (<c>MV 1</c> naming <c>AHU1</c>), whether or not anyone ever designs it. Mixed Design counted that unit as
        /// plant merely because it existed, so a template <c>MV 1</c> related to two flats refused as a shared system.
        /// </para>
        ///
        /// <para><b>The rule</b></para>
        /// <para>
        /// The system is <b>active</b> where any of these is stated, on it or on a unit it names (supply or exhaust):
        /// </para>
        /// <list type="bullet">
        /// <item>a related design terminal with a finite, non-zero design airflow - the same test as the Systems scope
        /// (<see cref="IsPartOEffectiveMechanicalDuty(VentilationTerminal)"/>);</item>
        /// <item>a related <see cref="SpaceAirMovement"/> moving a finite, non-zero airflow. A unit's movement is found by
        /// relation or by an endpoint naming the unit, as <c>Modify.AddAirMovementObjects</c> writes it;</item>
        /// <item>a selected product on the unit (<see cref="AirHandlingUnitParameter.VentilationUnitReference"/>, read
        /// through <see cref="SelectedVentilationUnitReference"/>);</item>
        /// <item>any of the first two on another system that names the same unit. A unit is one piece of plant, so a
        /// duty on one of its systems makes it, and every system on it, duty-bearing.</item>
        /// </list>
        /// <para>
        /// With none of these the system is <b>inert</b>. A missing, zero, NaN or infinite airflow is not a duty. The
        /// system type, the names and the unit's supply temperatures are not read.
        /// </para>
        ///
        /// <para><b>No stated unit airflow</b></para>
        /// <para>
        /// An <see cref="AirHandlingUnit"/> stores no design airflow. Its duty is derived from the design terminals of
        /// its systems (<see cref="AirHandlingUnitDesignDuty"/>), and its intake from its space movements
        /// (<see cref="AirFlow(AdjacencyCluster, AirHandlingUnitAirMovement, out Profile)"/>). Both are covered by the
        /// terminal and movement tests above, so there is no separate unit-airflow test to make. The unit's supply
        /// condition (<see cref="AirHandlingUnitAirMovement"/>) states no airflow of its own, so on its own it is not
        /// duty: only the movements its airflow is summed from are.
        /// </para>
        ///
        /// <para>
        /// Nothing is modified. This says only whether the plant states duty. Whether duty-bearing plant is then
        /// refused (shared, natural over mechanical, unconnected) is the caller's rule.
        /// </para>
        /// </summary>
        /// <returns>Null where either argument is null.</returns>
        public static PartOAuthoredPlantDuty PartOAuthoredPlantDuty(this AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem)
        {
            if (adjacencyCluster is null || ventilationSystem is null)
            {
                return null;
            }

            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [];

            List<PartOMechanicalDutyEvidence> evidence = [];
            HashSet<Guid> guids_Seen = [];

            PartOSystemDutyEvidence(adjacencyCluster, ventilationSystem, evidence, guids_Seen);

            List<string> names_Unit = [];
            foreach (string name_Unit in new[] { ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName), ventilationSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName) })
            {
                if (string.IsNullOrWhiteSpace(name_Unit) || names_Unit.Contains(name_Unit) || !airHandlingUnits.Exists(x => x?.Name == name_Unit))
                {
                    continue;
                }

                names_Unit.Add(name_Unit);

                foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
                {
                    if (airHandlingUnit?.Name == name_Unit)
                    {
                        PartOUnitDutyEvidence(adjacencyCluster, airHandlingUnit, ventilationSystem.Guid, evidence, guids_Seen);
                    }
                }
            }

            return new PartOAuthoredPlantDuty(ventilationSystem.Guid, ventilationSystem.FullName, names_Unit, evidence);
        }

        /// <summary>
        /// The same classification for one air handling unit: its own product and movements, and the
        /// terminal and movement duty of every ventilation system that names it (supply or exhaust).
        /// </summary>
        /// <returns>Null where either argument is null.</returns>
        public static PartOAuthoredPlantDuty PartOAuthoredPlantDuty(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit)
        {
            if (adjacencyCluster is null || airHandlingUnit is null)
            {
                return null;
            }

            List<PartOMechanicalDutyEvidence> evidence = [];
            PartOUnitDutyEvidence(adjacencyCluster, airHandlingUnit, Guid.Empty, evidence, []);

            return new PartOAuthoredPlantDuty(airHandlingUnit.Guid, airHandlingUnit.Name, [], evidence);
        }

        /// <summary>A stated, finite, non-zero air movement. Null, 0, NaN and ±∞ are not.</summary>
        internal static bool IsPartOEffectiveAirMovement(SpaceAirMovement spaceAirMovement)
        {
            return spaceAirMovement is not null
                && !double.IsNaN(spaceAirMovement.AirFlow)
                && !double.IsInfinity(spaceAirMovement.AirFlow)
                && spaceAirMovement.AirFlow != 0;
        }

        /// <summary>The system's own duty: its effective design terminals, then its effective air movements.</summary>
        private static void PartOSystemDutyEvidence(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, List<PartOMechanicalDutyEvidence> evidence, HashSet<Guid> guids_Seen)
        {
            string name_Owner = ventilationSystem.FullName;

            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem) ?? [];
            ventilationTerminals.Sort(CompareByName);

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                if (!IsPartOEffectiveMechanicalDuty(ventilationTerminal) || !guids_Seen.Add(ventilationTerminal.Guid))
                {
                    continue;
                }

                double value = ventilationTerminal.DesignFlowRate_Lps.Value;

                evidence.Add(new PartOMechanicalDutyEvidence(
                    PartOMechanicalDutyEvidenceKind.TerminalDesignAirFlow,
                    ventilationTerminal.Guid,
                    ventilationTerminal.Name,
                    ventilationSystem.Guid,
                    name_Owner,
                    value,
                    string.Format(CultureInfo.InvariantCulture, "'{0}': design terminal '{1}' states {2:0.###} l/s", name_Owner, ventilationTerminal.Name, value)));
            }

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(ventilationSystem) ?? [];
            PartOAirMovementDutyEvidence(spaceAirMovements, ventilationSystem.Guid, name_Owner, evidence, guids_Seen);
        }

        /// <summary>
        /// The unit's duty: its product, its effective air movements, then the own duty of every
        /// other system that names it.
        /// </summary>
        private static void PartOUnitDutyEvidence(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, Guid guid_VentilationSystem_Asking, List<PartOMechanicalDutyEvidence> evidence, HashSet<Guid> guids_Seen)
        {
            string name_Owner = airHandlingUnit.Name;

            VentilationUnitReference ventilationUnitReference = SelectedVentilationUnitReference(airHandlingUnit);
            if (ventilationUnitReference is not null && guids_Seen.Add(airHandlingUnit.Guid))
            {
                evidence.Add(new PartOMechanicalDutyEvidence(
                    PartOMechanicalDutyEvidenceKind.SelectedProduct,
                    airHandlingUnit.Guid,
                    airHandlingUnit.Name,
                    airHandlingUnit.Guid,
                    name_Owner,
                    double.NaN,
                    string.Format("'{0}': selected product '{1}'", name_Owner, ventilationUnitReference)));
            }

            //The unit's own supply condition (AirHandlingUnitAirMovement) is NOT evidence by itself (owner, PR-2): it
            //states conditions, never an airflow. The airflow TAS gives it (Query.AirFlow) is summed from the unit's
            //space movements below, so a condition with a finite airflow is duty-bearing through those, and one
            //without is not.

            //By relation, and by an endpoint naming the unit - Modify.AddAirMovementObjects writes both, and the
            //materialiser's own movement rule reads both.
            List<SpaceAirMovement> spaceAirMovements = [.. adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(airHandlingUnit) ?? []];

            ObjectReference objectReference = new(airHandlingUnit);
            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                if (spaceAirMovement is null)
                {
                    continue;
                }

                foreach (string reference in new[] { spaceAirMovement.From, spaceAirMovement.To })
                {
                    if (!string.IsNullOrWhiteSpace(reference) && objectReference == Core.Convert.ComplexReference<ObjectReference>(reference))
                    {
                        spaceAirMovements.Add(spaceAirMovement);

                        break;
                    }
                }
            }

            PartOAirMovementDutyEvidence(spaceAirMovements, airHandlingUnit.Guid, name_Owner, evidence, guids_Seen);

            //The unit's other systems. Only their OWN duty is read, so this never recurses into further units.
            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetObjects<VentilationSystem>() ?? [];
            ventilationSystems.Sort((x, y) =>
            {
                int result = string.CompareOrdinal(x?.FullName, y?.FullName);
                return result != 0 ? result : (x?.Guid ?? Guid.Empty).CompareTo(y?.Guid ?? Guid.Empty);
            });

            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                if (ventilationSystem is null || ventilationSystem.Guid == guid_VentilationSystem_Asking)
                {
                    continue;
                }

                if (ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName) != airHandlingUnit.Name && ventilationSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName) != airHandlingUnit.Name)
                {
                    continue;
                }

                PartOSystemDutyEvidence(adjacencyCluster, ventilationSystem, evidence, guids_Seen);
            }
        }

        private static void PartOAirMovementDutyEvidence(List<SpaceAirMovement> spaceAirMovements, Guid guid_Owner, string name_Owner, List<PartOMechanicalDutyEvidence> evidence, HashSet<Guid> guids_Seen)
        {
            spaceAirMovements.Sort(CompareByName);

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                if (!IsPartOEffectiveAirMovement(spaceAirMovement) || !guids_Seen.Add(spaceAirMovement.Guid))
                {
                    continue;
                }

                double value = spaceAirMovement.AirFlow * 1000.0;

                evidence.Add(new PartOMechanicalDutyEvidence(
                    PartOMechanicalDutyEvidenceKind.SpaceAirMovement,
                    spaceAirMovement.Guid,
                    spaceAirMovement.Name,
                    guid_Owner,
                    name_Owner,
                    value,
                    string.Format(CultureInfo.InvariantCulture, "'{0}': air movement '{1}' moves {2:0.###} l/s", name_Owner, spaceAirMovement.Name, value)));
            }
        }

        private static int CompareByName(SAMObject sAMObject_1, SAMObject sAMObject_2)
        {
            int result = string.CompareOrdinal(sAMObject_1?.Name, sAMObject_2?.Name);
            return result != 0 ? result : (sAMObject_1?.Guid ?? Guid.Empty).CompareTo(sAMObject_2?.Guid ?? Guid.Empty);
        }
    }
}
