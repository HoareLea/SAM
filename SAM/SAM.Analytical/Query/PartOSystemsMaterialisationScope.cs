// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Which ventilation systems a Part O Systems-route materialisation (SAM_Systems
        /// <c>Create.MechanicalVentilation</c>) may be handed, decided by <b>identity</b> - the one rule Iteration 3 and
        /// Mixed Design share (SAM_UI PR-1, Part O model-state architecture). It was Iteration 3's
        /// <c>PartOIteration3SystemScope</c> in SAM_UI, moved here unchanged.
        ///
        /// <para><b>The rule, in one sentence</b></para>
        /// <para>
        /// Keep exactly the systems Part O built; leave out an authored system that carries no effective mechanical
        /// duty anywhere in the thermal model; <b>refuse</b> - rather than choose - where an authored system does carry
        /// one.
        /// </para>
        ///
        /// <para><b>Why identity and not a rule over the model</b></para>
        /// <para>
        /// A real model reaches Part O carrying natural, uncontrolled and legacy mechanical systems (for example the
        /// <c>Modify.AddMechanicalSystems</c> template's <c>NV 1</c>, <c>UV 1</c> and <c>MV 1</c>). SAM_Systems requires
        /// every ventilation system it is handed to resolve an air handling unit, and natural and uncontrolled systems
        /// never have one, so handing it the whole model refuses a correct design. No rule recovers "which of these is
        /// the design under assessment" afterwards: the type does not (a legacy MV system is mechanical too), terminals
        /// do not (a competing design carries them), and the display name never does. The only moment the answer is
        /// known is when Part O builds its systems - Iteration 3's preparation, Mixed Design's
        /// <see cref="PartOMaterialisationRecord.VentilationSystemGuids"/>.
        /// </para>
        ///
        /// <para><b>Why the whole thermal domain is inspected, not just the dwellings in scope</b></para>
        /// <para>
        /// The Systems route's thermal source removes mechanical ventilation <b>model-wide</b> (the no-IZAM sweep), then
        /// reinstates, explicitly in TAS Systems, only the duty of the systems materialised here. So an authored
        /// mechanical system serving a room <i>outside</i> the assessed dwellings is not harmless context: its
        /// ventilation would simply be gone, that room's temperature moves, and it is coupled to the assessed rooms
        /// through fabric and air movement. The answer is to refuse, and not to broaden the materialisation to cover
        /// the extra system: materialising a system nobody asked to assess would put an invented design into TAS.
        /// </para>
        ///
        /// <para><b>Natural ventilation, uncontrolled ventilation and infiltration are not mechanical</b></para>
        /// <para>
        /// They are authored thermal behaviour, and nothing here removes them from anything that is simulated. A system
        /// of theirs with no effective design terminal is dropped from the <i>materialisation input only</i>, with a
        /// note saying so. Cooling and heating systems are not ventilation systems and are never read.
        /// </para>
        ///
        /// <para><b>Nothing is mutated</b></para>
        /// <para>
        /// <paramref name="adjacencyCluster"/> is read only; the working copy is a shallow copy of it (its own
        /// dictionaries, shared objects, none of which is touched).
        /// </para>
        ///
        /// <para><b>Scaling</b></para>
        /// <para>
        /// One pass over the dwelling scope, one pass over the ventilation systems, and for each system one pass over its
        /// own related terminals - linear in the model, and no list is scanned by name.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The derived calculation model's cluster (prepared or materialised). Read only.</param>
        /// <param name="guids_VentilationSystem_Built">The identities of the ventilation systems Part O built.</param>
        /// <param name="guids_Space_Dwelling">
        /// The rooms of the assessed dwellings. Used only to word a refusal: a competing design inside the dwellings and
        /// an unreinstated duty outside them both refuse, and a reader needs to know which they are looking at.
        /// </param>
        public static PartOSystemsMaterialisationScope PartOSystemsMaterialisationScope(AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids_VentilationSystem_Built, IEnumerable<Guid> guids_Space_Dwelling)
        {
            List<PartOSystemsScopeRefusal> refusals = [];

            if (adjacencyCluster is null)
            {
                refusals.Add(new PartOSystemsScopeRefusal(
                    PartOSystemsScopeRefusalReason.NoModel,
                    "No model was supplied, so there is no ventilation design to scope."));

                return new PartOSystemsMaterialisationScope(null, null, null, null, null, refusals);
            }

            HashSet<Guid> guids_Retained = [];
            foreach (Guid guid in guids_VentilationSystem_Built ?? [])
            {
                if (guid != Guid.Empty)
                {
                    guids_Retained.Add(guid);
                }
            }

            if (guids_Retained.Count == 0)
            {
                refusals.Add(new PartOSystemsScopeRefusal(
                    PartOSystemsScopeRefusalReason.NoIdentities,
                    "No identity of a ventilation system Part O built was supplied, so which of the model's ventilation systems is the design under assessment is not known."));

                return new PartOSystemsMaterialisationScope(null, null, null, null, null, refusals);
            }

            HashSet<Guid> guids_Dwelling = [];
            foreach (Guid guid in guids_Space_Dwelling ?? [])
            {
                guids_Dwelling.Add(guid);
            }

            //One pass each. Nothing below re-enumerates the model.
            Dictionary<Guid, VentilationSystem> dictionary_System = [];
            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem is not null && ventilationSystem.Guid != Guid.Empty)
                {
                    dictionary_System[ventilationSystem.Guid] = ventilationSystem;
                }
            }

            foreach (Guid guid in guids_Retained)
            {
                if (!dictionary_System.ContainsKey(guid))
                {
                    refusals.Add(new PartOSystemsScopeRefusal(
                        PartOSystemsScopeRefusalReason.IdentityNotOnModel,
                        string.Format("The ventilation system {0} Part O built is not on the model, so the design under assessment cannot be identified on it.", guid),
                        guid));
                }
            }

            if (refusals.Count != 0)
            {
                return new PartOSystemsMaterialisationScope(null, null, null, null, null, refusals);
            }

            List<Guid> guids_Removed = [];
            List<PartOSystemsScopeExclusion> exclusions = [];

            foreach (KeyValuePair<Guid, VentilationSystem> keyValuePair in dictionary_System)
            {
                Guid guid_System = keyValuePair.Key;

                if (guids_Retained.Contains(guid_System))
                {
                    continue;
                }

                VentilationSystem ventilationSystem = keyValuePair.Value;

                int count_Terminal = 0;
                int count_Duty = 0;

                foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.VentilationTerminals(ventilationSystem) ?? [])
                {
                    if (ventilationTerminal is null)
                    {
                        continue;
                    }

                    count_Terminal++;

                    if (!IsPartOEffectiveMechanicalDuty(ventilationTerminal))
                    {
                        continue;
                    }

                    count_Duty++;

                    List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationTerminal) ?? [];

                    if (spaces.Count == 0)
                    {
                        //Fail closed. A duty that serves no identified room cannot be shown to be outside the thermal
                        //case, and "probably harmless" is not a standard an assessment can be built on.
                        refusals.Add(new PartOSystemsScopeRefusal(
                            PartOSystemsScopeRefusalReason.DutyServesNoSpace,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Ventilation system '{0}' ({1}) carries the design terminal '{2}' ({3}) at {4:0.###} l/s, and that terminal is not related to any space - so it cannot be shown that leaving its mechanical ventilation out of the TAS Systems simulation does not change the thermal case. "
                                + "Relate the terminal to the room it serves, or remove its design airflow if it is not a real duty.",
                                ventilationSystem.FullName,
                                guid_System,
                                ventilationTerminal.Name,
                                ventilationTerminal.Guid,
                                ventilationTerminal.DesignFlowRate_Lps ?? double.NaN),
                            guid_System,
                            ventilationSystem,
                            ventilationTerminal));

                        continue;
                    }

                    foreach (Space space in spaces)
                    {
                        if (space is null)
                        {
                            continue;
                        }

                        bool inside = guids_Dwelling.Contains(space.Guid);

                        refusals.Add(new PartOSystemsScopeRefusal(
                            inside ? PartOSystemsScopeRefusalReason.DutyInsideDwellingScope : PartOSystemsScopeRefusalReason.DutyOutsideDwellingScope,
                            inside
                                ? string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Ventilation system '{0}' ({1}) carries a design {2} terminal of {3:0.###} l/s in '{4}' ({5}), which is inside the assessed dwellings. "
                                    + "That is a second mechanical ventilation design for a room Part O has designed, and choosing between two designs is not Part O's decision. "
                                    + "Resolve the model so one mechanical design serves the room, then run again.",
                                    ventilationSystem.FullName,
                                    guid_System,
                                    ventilationTerminal.FlowClassification,
                                    ventilationTerminal.DesignFlowRate_Lps ?? double.NaN,
                                    space.Name,
                                    space.Guid)
                                : string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Ventilation system '{0}' ({1}) carries a design {2} terminal of {3:0.###} l/s in '{4}' ({5}). That room is outside the assessed dwellings but is part of the same thermal model. "
                                    + "The TAS Systems route removes mechanical ventilation from the WHOLE model and reinstates only the systems Part O built, so this room's authored ventilation would be missing from the simulation - and the room is thermally coupled to the assessed rooms. "
                                    + "The assessment is refused rather than reported on a thermal model that differs from the design.",
                                    ventilationSystem.FullName,
                                    guid_System,
                                    ventilationTerminal.FlowClassification,
                                    ventilationTerminal.DesignFlowRate_Lps ?? double.NaN,
                                    space.Name,
                                    space.Guid),
                            guid_System,
                            ventilationSystem,
                            ventilationTerminal,
                            space));
                    }
                }

                if (count_Duty != 0)
                {
                    continue;
                }

                guids_Removed.Add(guid_System);

                exclusions.Add(new PartOSystemsScopeExclusion(
                    ventilationSystem,
                    count_Terminal,
                    string.Format(
                        "Ventilation system '{0}' ({1}) was left out of the TAS Systems materialisation input: Part O did not build it, and it carries {2} - so it states no mechanical duty the TAS Systems route has to recreate. "
                        + "It remains on the design and in the thermal model, where its authored behaviour is simulated as authored.",
                        ventilationSystem.FullName,
                        guid_System,
                        count_Terminal == 0 ? "no design ventilation terminal" : string.Format("{0} design ventilation terminal(s), none of which states an effective design airflow", count_Terminal))));
            }

            if (refusals.Count != 0)
            {
                return new PartOSystemsMaterialisationScope(null, null, null, null, null, refusals);
            }

            //Ordered before the removals so the working copy, the record and the evidence are the same on every
            //machine - a dictionary walk is not.
            guids_Removed.Sort();

            List<Guid> guids_Retained_Ordered = [.. guids_Retained];
            guids_Retained_Ordered.Sort();

            //A COPY. The removals below must never reach the model supplied. The shallow copy is the right one: it
            //rebuilds the cluster's own dictionaries, which is all that is written here, and shares the objects, none of
            //which is touched.
            AdjacencyCluster adjacencyCluster_Working = new(adjacencyCluster);

            foreach (Guid guid in guids_Removed)
            {
                VentilationSystem ventilationSystem = adjacencyCluster_Working.GetObject<VentilationSystem>(guid);

                if (ventilationSystem is null || !adjacencyCluster_Working.RemoveObject(ventilationSystem))
                {
                    refusals.Add(new PartOSystemsScopeRefusal(
                        PartOSystemsScopeRefusalReason.NotRemovable,
                        string.Format("Ventilation system {0} could not be left out of the materialisation input, so the systems handed to the materialisation are not the ones this scope decided on.", guid),
                        guid));
                }
            }

            if (refusals.Count != 0)
            {
                return new PartOSystemsMaterialisationScope(null, null, null, null, null, refusals);
            }

            string note_Summary = string.Format(
                "{0} ventilation system(s) built by Part O are the design under assessment; {1} authored system(s) were left out of the materialisation input and none of them states mechanical duty.",
                guids_Retained_Ordered.Count,
                guids_Removed.Count);

            return new PartOSystemsMaterialisationScope(adjacencyCluster_Working, guids_Retained_Ordered, guids_Removed, exclusions, note_Summary, null);
        }

        /// <summary>
        /// Whether one design terminal states mechanical duty that actually moves air - the Part O Systems scope's
        /// definition of effective duty, unchanged from Iteration 3.
        /// <para>
        /// A terminal with a stated, finite, non-zero design airflow does. A terminal with no stated airflow, with
        /// <see cref="double.NaN"/>, with an infinite value, or designed at nothing does not - there is no ventilation
        /// for the no-IZAM sweep to remove and none for the Systems route to reinstate, so refusing over it would refuse
        /// models nothing is wrong with. The flow classification is not read: a stated duty of unknown direction still
        /// counts, and refuses.
        /// </para>
        /// </summary>
        internal static bool IsPartOEffectiveMechanicalDuty(VentilationTerminal ventilationTerminal)
        {
            double? designFlowRate_Lps = ventilationTerminal?.DesignFlowRate_Lps;

            return designFlowRate_Lps.HasValue
                && !double.IsNaN(designFlowRate_Lps.Value)
                && !double.IsInfinity(designFlowRate_Lps.Value)
                && designFlowRate_Lps.Value != 0;
        }
    }
}
