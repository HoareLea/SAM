// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// The <see cref="VentilationSystemType"/> the Approved Document O Base MVHR configuration uses.
        /// <para>
        /// A fixed guid, so the same model prepared twice - or on two machines - carries the same system
        /// type rather than a new one each time. The name is <c>MVHR</c> because that is what Approved
        /// Document F System 4 is, and because a system type name is read by generic MEP tooling that has
        /// never heard of Part O.
        /// </para>
        /// </summary>
        private static readonly Guid guid_VentilationSystemType_MVHR = new("5a4b1f2c-9d3e-4c7a-8b16-2f0d6e5c8a41");

        /// <summary>
        /// The fixed type guid of the Part O Base MVHR ventilation system - the signal
        /// <c>Query.PartOBaselineFindings</c> recognises a Part O materialisation by.
        /// </summary>
        internal static Guid Guid_VentilationSystemType_PartOMVHR => guid_VentilationSystemType_MVHR;

        private const string name_VentilationSystemType_MVHR = "MVHR";

        private const string name_AirHandlingUnit_Base = "MVHR-01";

        /// <summary>
        /// Creates, or reuses, the one generic ventilation system and air handling unit that the Approved
        /// Document O Base MVHR configuration serves the given spaces with, and connects the spaces and
        /// their design terminals to it.
        /// <para>
        /// <b>Generic plant, deliberately.</b> The unit is
        /// <see cref="Create.AirHandlingUnit(string, bool)"/>'s standard heat recovery arrangement. No
        /// manufacturer, no capacity, no summer bypass, no boost - Iteration 1a establishes the topology
        /// and the duty a real unit will later have to meet, and selecting that unit is Iteration 2's
        /// work. What this builds is what Iteration 2 extends, not what it replaces.
        /// </para>
        /// <para>
        /// <b>Reuse is by relation, never by name.</b> An existing system is recognised because it is
        /// related to the design terminals of these spaces, not because something is called the same
        /// thing. Two candidate systems refuse rather than one being picked.
        /// </para>
        /// <para>
        /// <b>Reuse reconciles membership to the current call's scope, not merely adds to it.</b> A system
        /// found this way may carry relations to spaces or terminals a WIDER or DIFFERENT earlier
        /// preparation gave it - a whole-model run followed by one naming a narrower <c>zones_</c> subset,
        /// for instance. Those out-of-scope relations are removed before the current scope is connected, so
        /// re-preparing the same model twice leaves the system's membership exactly what this call states
        /// rather than the accumulated union of every scope it was ever prepared over.
        /// </para>
        /// <para>
        /// <b>No ventilation metadata is written onto the model.</b> <c>Modify.AssignMechanicalSystem</c>
        /// is deliberately not used: it sets <c>InternalConditionParameter.VentilationSystemTypeName</c>
        /// on every space it touches, and mutating the metadata the Part O route was taken out of would
        /// put the decision straight back into it - see <c>documentation/PartO-ARCHITECTURE.md</c> §3. The
        /// relation is added directly instead, which is the rest of what that method does.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Modified in place.</b></param>
        /// <param name="spaces">The spaces the system serves. Null means every space in the cluster.</param>
        /// <param name="airHandlingUnit">The generic unit created or reused, or null on a refusal.</param>
        /// <param name="notes">What was created or reused.</param>
        /// <param name="warnings">
        /// Every ventilation system the model already relates to an assessed space and this iteration did not
        /// build - reported room by room, read for nothing and changed in no way.
        /// </param>
        /// <param name="refusals">Why nothing could be built, one sentence each.</param>
        /// <returns>The ventilation system, or null where <paramref name="refusals"/> is non-empty.</returns>
        public static VentilationSystem AddPartOBaseMVHRSystem(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces, out AirHandlingUnit airHandlingUnit, out List<string> notes, out List<string> warnings, out List<string> refusals)
        {
            return AddPartOBaseMVHRSystem(adjacencyCluster, spaces, null, null, out airHandlingUnit, out notes, out warnings, out refusals);
        }

        /// <summary>
        /// <see cref="AddPartOBaseMVHRSystem(AdjacencyCluster, IEnumerable{Space}, out AirHandlingUnit, out List{string}, out List{string}, out List{string})"/>
        /// with the created system's id and unit's name <b>stated by the caller</b> rather than handed out in
        /// call order.
        /// <para>
        /// The generic <c>MVHR-NN</c> unit name, and <c>Query.NextId</c>'s system id, follow the order dwellings
        /// are prepared in (PR0 P2), and the unit name is identity-bearing: it is the system -> unit link and it
        /// is inside the unit movement's profile names. A mixed materialisation derives both from the dwelling
        /// instead, so the same baseline and strategies give the same names whatever the processing order.
        /// Null for either keeps the legacy behaviour. Ignored on the reuse path, which keeps what it finds.
        /// </para>
        /// </summary>
        internal static VentilationSystem AddPartOBaseMVHRSystem(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces, string id_VentilationSystem, string name_AirHandlingUnit, out AirHandlingUnit airHandlingUnit, out List<string> notes, out List<string> warnings, out List<string> refusals)
        {
            notes = [];
            warnings = [];
            refusals = [];
            airHandlingUnit = null;

            if (adjacencyCluster is null)
            {
                refusals.Add("No model was supplied, so no ventilation system could be built.");

                return null;
            }

            //ONE snapshot of the model's space identities, taken before anything is built. The scope is
            //resolved and de-duplicated through it rather than by searching the whole space list once per
            //space, which is what made preparing a five thousand space model quadratic twice over.
            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces_Cluster = partFIndex.Spaces;

            List<Space> spaces_Temp = [];
            HashSet<Guid> guids_Temp = [];
            foreach (Space space in spaces ?? spaces_Cluster)
            {
                Space space_Cluster = space is null ? null : partFIndex.Space(space.Guid);
                if (space_Cluster is not null && guids_Temp.Add(space_Cluster.Guid))
                {
                    spaces_Temp.Add(space_Cluster);
                }
            }

            if (spaces_Temp.Count == 0)
            {
                refusals.Add("No space of the model was resolved, so there is nothing for a ventilation system to serve.");

                return null;
            }

            //Only the spaces that actually have a design terminal are connected. A corridor or a store
            //that Approved Document F did not size has no terminal, and putting it on the system would
            //claim the unit serves a room it moves no air into.
            List<VentilationTerminal> ventilationTerminals = [];
            List<Space> spaces_Served = [];

            foreach (Space space in spaces_Temp)
            {
                List<VentilationTerminal> ventilationTerminals_Space = adjacencyCluster.VentilationTerminals(space);
                if (ventilationTerminals_Space is null || ventilationTerminals_Space.Count == 0)
                {
                    continue;
                }

                spaces_Served.Add(space);
                ventilationTerminals.AddRange(ventilationTerminals_Space);
            }

            if (ventilationTerminals.Count == 0)
            {
                refusals.Add("No space carries a design ventilation terminal, so there is no duty for a ventilation system to be built around. Realize the Approved Document F requirements first.");

                return null;
            }

            // ---- Reuse or create the system ----------------------------------------------------------

            //Reuse is keyed on the DESIGN TERMINALS, which only this iteration creates - so re-preparing a
            //model finds the system it built last time and adds nothing. Recognised by relation, never by
            //name. Two candidates refuse rather than one being picked.
            //
            //Deliberately NOT keyed on the spaces. A model routinely arrives carrying the system-template
            //assignment it was built with, and that assignment is metadata about a previous design stage: the
            //licensed acceptance model has its rooms split across an "NV" system, an "MV" system and a "UV"
            //system while the assessment states one MVHR route for the whole dwelling. Attaching Base MVHR
            //terminals to a system typed NV would be untrue, and picking one of three would be a guess. What
            //those systems say is reported instead - see the warnings - and what is simulated is scoped to
            //the system this iteration builds.
            List<VentilationSystem> ventilationSystems_Existing = [];
            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                foreach (VentilationSystem ventilationSystem_Related in adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? [])
                {
                    if (ventilationSystem_Related is not null && ventilationSystems_Existing.Find(x => x.Guid == ventilationSystem_Related.Guid) is null)
                    {
                        ventilationSystems_Existing.Add(ventilationSystem_Related);
                    }
                }
            }

            if (ventilationSystems_Existing.Count > 1)
            {
                List<string> names = ventilationSystems_Existing.ConvertAll(x => string.Format("'{0}'", x.FullName));

                refusals.Add(string.Format(
                    "The design ventilation terminals of these spaces are already connected to {0} ventilation systems ({1}), so which one the Base MVHR configuration is being prepared over is ambiguous. Nothing was prepared.",
                    ventilationSystems_Existing.Count,
                    string.Join(", ", names)));

                return null;
            }

            //Every ventilation system the model already relates to an assessed space, named room by room.
            //
            //These are NOT read to decide anything and NOT rewritten - they are what the model was built
            //with, they may predate the assessment, and the Part O route is stated rather than derived from
            //them. They are reported because leaving them silently in place would hide a real contradiction:
            //a room this iteration supplies from a Base MVHR unit while the model still says it is on a
            //natural ventilation system. What reaches the simulation is scoped to the system built here, so
            //nothing is ventilated twice; reconciling the model's own systems is design work an engineer
            //does, or Iteration 2's when it selects a real unit.
            foreach (Space space in spaces_Served)
            {
                foreach (VentilationSystem ventilationSystem_Model in adjacencyCluster.GetRelatedObjects<VentilationSystem>(space) ?? [])
                {
                    if (ventilationSystem_Model is null || ventilationSystems_Existing.Find(x => x.Guid == ventilationSystem_Model.Guid) is not null)
                    {
                        continue;
                    }

                    warnings.Add(string.Format(
                        "Space '{0}' is still related to ventilation system '{1}' of type '{2}', which this iteration did not build and has not changed. The Base MVHR design terminals and the air movements the simulation reads belong to the system built here; that relation is left as the model authored it.",
                        space.Name,
                        ventilationSystem_Model.FullName,
                        ventilationSystem_Model.Type?.Name ?? "-"));
                }
            }

            VentilationSystem result;

            if (ventilationSystems_Existing.Count == 1)
            {
                result = ventilationSystems_Existing[0];

                airHandlingUnit = AirHandlingUnitByName(adjacencyCluster, result.GetValue<string>(VentilationSystemParameter.SupplyUnitName));

                if (airHandlingUnit is null)
                {
                    refusals.Add(string.Format(
                        "Ventilation system '{0}' is already connected to these terminals but names no air handling unit that the model contains, so the unit the Base MVHR configuration supplies from could not be resolved. Nothing was prepared.",
                        result.FullName));

                    return null;
                }

                //Reconciled to EXACTLY this call's scope before anything is added, not merely added to.
                //
                //A system reused across two preparations of the same model - a whole-model run followed by
                //one naming a narrower zones_ subset, or simply a re-run after the served spaces changed -
                //keeps every relation it was ever given: the "Connect" step below only ADDS the current
                //spaces_Served and ventilationTerminals, it never removed what an earlier, wider preparation
                //left behind. ReconcileVentilationSystemDesignDuty, PartFTransferAirSpaces and
                //AddAirMovementObjects all read the system's relations afterwards, so a stale out-of-scope
                //space or terminal would still be realized and balanced as if it were served here - the
                //model would report success while carrying the accumulated union of every scope it was ever
                //prepared over, not the current one.
                List<string> names_Removed = [];

                foreach (Space space_Related in adjacencyCluster.GetRelatedObjects<Space>(result) ?? [])
                {
                    if (space_Related is not null && spaces_Served.Find(x => x.Guid == space_Related.Guid) is null)
                    {
                        adjacencyCluster.RemoveRelation(result, space_Related);
                        names_Removed.Add(space_Related.Name);
                    }
                }

                foreach (VentilationTerminal ventilationTerminal_Related in adjacencyCluster.GetRelatedObjects<VentilationTerminal>(result) ?? [])
                {
                    if (ventilationTerminal_Related is not null && ventilationTerminals.Find(x => x.Guid == ventilationTerminal_Related.Guid) is null)
                    {
                        adjacencyCluster.RemoveRelation(result, ventilationTerminal_Related);
                    }
                }

                if (names_Removed.Count != 0)
                {
                    names_Removed.Sort(StringComparer.Ordinal);

                    notes.Add(string.Format(
                        "Ventilation system '{0}' was related to {1} space(s) outside this call's scope, left over from a wider or different previous preparation ({2}); those relations were removed so the system's membership reflects only what is being prepared now.",
                        result.FullName,
                        names_Removed.Count,
                        string.Join(", ", names_Removed)));
                }

                notes.Add(string.Format(
                    "Reused ventilation system '{0}' of type '{1}' and its air handling unit '{2}', found through the design terminals already connected to them. Nothing was added beside it.",
                    result.FullName,
                    result.Type?.Name ?? "-",
                    airHandlingUnit.Name));
            }
            else
            {
                VentilationSystemType ventilationSystemType = Create.VentilationSystemType(
                    guid_VentilationSystemType_MVHR,
                    name_VentilationSystemType_MVHR,
                    "Continuous mechanical supply and extract with heat recovery - Approved Document F, Volume 1: Dwellings (2021 edition), System 4.");

                result = Create.MechanicalSystem(ventilationSystemType, null, id_VentilationSystem ?? Query.NextId(adjacencyCluster, ventilationSystemType)) as VentilationSystem;
                if (result is null)
                {
                    refusals.Add("The Base MVHR ventilation system could not be created.");

                    return null;
                }

                airHandlingUnit = Create.AirHandlingUnit(name_AirHandlingUnit ?? UniqueAirHandlingUnitName(adjacencyCluster, spaces_Cluster));
                if (airHandlingUnit is null)
                {
                    refusals.Add("The generic Base MVHR air handling unit could not be created.");

                    return null;
                }

                //The Base MVHR unit states NO supply temperature, in either season.
                //
                //Create.AirHandlingUnit's standard arrangement carries a 23 degree summer supply temperature,
                //and a supply temperature is a setpoint the unit's supply air is conditioned TO - which is
                //active cooling. Iteration 1a is base provision: no tempering, no active cooling, no
                //manufacturer performance. Crediting a dwelling with a 23 degree supply during a cooling
                //season it is being assessed for overheating in would be Iteration 3's claim made at
                //Iteration 1a, and it would turn the answer in the building's favour.
                //
                //Absent, not zero: nothing here says the supply air is cold, only that this iteration does
                //not state what conditions it. See documentation/PartO-ARCHITECTURE.md section 5.
                airHandlingUnit.SummerSupplyTemperature = double.NaN;
                airHandlingUnit.WinterSupplyTemperature = double.NaN;

                //The existing binding between a ventilation system and its plant, kept exactly as every
                //other SAM workflow writes it so that Modify.AddAirMovementObjects and Create.TPD resolve
                //this unit the way they resolve any other. That the binding is a name rather than a
                //relation is pre-existing technical debt, recorded rather than migrated here.
                result.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);
                result.SetValue(VentilationSystemParameter.ExhaustUnitName, airHandlingUnit.Name);

                notes.Add(string.Format("Created generic ventilation system '{0}' and air handling unit '{1}' for the Base MVHR configuration. No manufacturer unit is selected at this iteration.", result.FullName, airHandlingUnit.Name));
            }

            adjacencyCluster.AddObject(result);
            adjacencyCluster.AddObject(airHandlingUnit);

            // ---- Connect ------------------------------------------------------------------------------

            foreach (Space space in spaces_Served)
            {
                adjacencyCluster.AddRelation(result, space);
            }

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                adjacencyCluster.AddRelation(result, ventilationTerminal);
            }

            notes.Add(string.Format(
                "Ventilation system '{0}' serves {1} space(s) through {2} design terminal(s).",
                result.FullName,
                spaces_Served.Count,
                ventilationTerminals.Count));

            return result;
        }

        private static AirHandlingUnit AirHandlingUnitByName(AdjacencyCluster adjacencyCluster, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x is not null && x.Name == name);
        }

        /// <summary>
        /// A unit name no other unit and no space is already using.
        /// <para>
        /// Spaces are checked as well as units because <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> gives
        /// the unit a TAS zone of its own and names that zone after the unit. A unit sharing a name with a
        /// room would collide with that room's zone, and the air movements of the two would be assigned to
        /// whichever the name lookup found.
        /// </para>
        /// </summary>
        private static string UniqueAirHandlingUnitName(AdjacencyCluster adjacencyCluster, List<Space> spaces)
        {
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
            {
                if (!string.IsNullOrWhiteSpace(airHandlingUnit?.Name))
                {
                    names.Add(airHandlingUnit.Name.Trim());
                }
            }

            foreach (Space space in spaces ?? [])
            {
                if (!string.IsNullOrWhiteSpace(space?.Name))
                {
                    names.Add(space.Name.Trim());
                }
            }

            if (!names.Contains(name_AirHandlingUnit_Base))
            {
                return name_AirHandlingUnit_Base;
            }

            int index = 2;
            while (names.Contains(string.Format("MVHR-{0:00}", index)))
            {
                index++;
            }

            return string.Format("MVHR-{0:00}", index);
        }
    }
}
