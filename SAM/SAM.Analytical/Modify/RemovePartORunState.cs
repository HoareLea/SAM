// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// The airflow parameters <c>Modify.ApplyPartFVentilationRates</c> writes onto a per-space internal
        /// condition: the two it sets to the Part F rate and the six bases it zeroes so they cannot add to it.
        /// </summary>
        private static readonly InternalConditionParameter[] internalConditionParameters_PartFAirflow =
        [
            InternalConditionParameter.SupplyAirFlow,
            InternalConditionParameter.ExhaustAirFlow,
            InternalConditionParameter.SupplyAirFlowPerPerson,
            InternalConditionParameter.SupplyAirFlowPerArea,
            InternalConditionParameter.SupplyAirChangesPerHour,
            InternalConditionParameter.ExhaustAirFlowPerPerson,
            InternalConditionParameter.ExhaustAirFlowPerArea,
            InternalConditionParameter.ExhaustAirChangesPerHour,
        ];

        /// <summary>
        /// A copy of <paramref name="analyticalModel"/> without the Part O run and preparation state that
        /// <see cref="Query.PartOBaselineFindings(AnalyticalModel)"/> refuses as a baseline - so a model that has
        /// been through Prepare &amp; Run can become the clean baseline Mixed Design starts from, without hunting
        /// for an older file.
        ///
        /// <para><b>Judged by the validator, never by this method.</b></para>
        /// <para>
        /// This removes, and says what it removed and what it kept. Whether the copy IS a clean baseline is
        /// asked of <see cref="Query.PartOBaselineFindings(AnalyticalModel)"/> afterwards, the same authority
        /// Mixed Design asks. Nothing here decides "clean".
        /// </para>
        ///
        /// <para><b>What is removed</b></para>
        /// <list type="bullet">
        /// <item>Run output: the overheating scenarios, the <see cref="SimulationResultProvenance"/>, the <see cref="PartOBaselineReference"/>, every
        /// <see cref="IResult"/> in the cluster or held as a model parameter, and the design-day records the TAS
        /// workflow writes into the cluster. The model-level Heating/Cooling Design Days parameters are design
        /// inputs and are kept.</item>
        /// <item>Part O preparation plant: every ventilation system of the Part O MVHR type, the air handling
        /// unit it names (unless a remaining system names it too), the design terminals connected to it, and the
        /// air movements the preparation built for its dwelling - the same scope
        /// <c>RemoveBaseMVHRAirMovementObjects</c> clears before a re-preparation.</item>
        /// <item>The per-space Part F internal condition, <b>only where the model proves what it replaced</b>
        /// (below).</item>
        /// </list>
        ///
        /// <para><b>What is kept</b></para>
        /// <para>
        /// Geometry, zones, constructions, openings, the Approved Document F requirements
        /// (<c>PartFSpaceData</c>), weather and model-level design days, authored ventilation, heating and cooling
        /// systems and their units, design terminals no Part O system is connected to (an accepted Iteration 2B
        /// design lives there), the dwelling strategies and the equipment selection. A
        /// <see cref="PartOMaterialisationRecord"/> and a <see cref="PartOIsolationContext"/> are kept too: a
        /// materialised mixed model is rebuilt from the baseline beside it, and an isolated model is missing the
        /// rest of the building, which nothing here can put back. The validator goes on refusing both.
        /// </para>
        ///
        /// <para><b>The per-space Part F internal condition: restored only on evidence</b></para>
        /// <para>
        /// <c>ApplyPartFVentilationRates</c> replaced each sized space's condition with a clone named
        /// <c>&lt;condition&gt; - &lt;space&gt;</c>, a fresh guid, the Part F supply and extract, and the six other
        /// airflow bases zeroed. What those bases held before is not stored, so the rewrite is reversed only where
        /// the model itself shows it: the base condition (by the name the clone was made from) is held in the
        /// model - in the cluster, as Map IC (TM59) leaves its library conditions, or on a space that was not
        /// rewritten - it is unambiguous, it states no non-zero airflow on any of the eight parameters, and the
        /// clone agrees with it on every other parameter it states. The restored condition is the clone with the
        /// base's name and the base's airflow state (absent, or zero). Not its guid: a space gives every condition
        /// assigned to it a fresh one (<c>Space.InternalCondition</c>), so the guid never carried identity. Everything else the space's own
        /// condition carried - its area per person, say - is kept. Any other space keeps its Part F condition and
        /// is named in <paramref name="kept"/>; the validator then still refuses the copy, and the pre-Part-O
        /// source model is the way forward.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">The model. <b>Not modified</b> - a cleaned copy is returned.</param>
        /// <param name="removed">What was removed, one line per kind, with counts and names.</param>
        /// <param name="kept">Part O state deliberately left in the copy, and why, one sentence each.</param>
        /// <returns>The cleaned copy, or null where no model was supplied.</returns>
        public static AnalyticalModel RemovePartORunState(this AnalyticalModel analyticalModel, out List<string> removed, out List<string> kept)
        {
            removed = [];
            kept = [];

            if (analyticalModel is null)
            {
                return null;
            }

            AnalyticalModel result = new(analyticalModel);

            // ---- run output, model level -----------------------------------------------------------------------

            if (result.HasValue(AnalyticalModelParameter.OverheatingScenarios))
            {
                result.RemoveValue(AnalyticalModelParameter.OverheatingScenarios);
                removed.Add("Overheating scenarios (the Part O run's scenario statement).");
            }

            if (result.HasValue(AnalyticalModelParameter.SimulationResultProvenance))
            {
                result.RemoveValue(AnalyticalModelParameter.SimulationResultProvenance);
                removed.Add("Simulation result provenance (the link to the run's TAS results file; the file itself is not touched).");
            }

            if (result.HasValue(AnalyticalModelParameter.PartOBaselineReference))
            {
                result.RemoveValue(AnalyticalModelParameter.PartOBaselineReference);
                removed.Add("Part O baseline reference (the link to the model the run was derived from; that model is not touched).");
            }

            foreach (ParameterSet parameterSet in result.GetParameterSets() ?? [])
            {
                foreach (string name in new List<string>(parameterSet?.Names ?? []))
                {
                    if (Query.IsResultValue(parameterSet.ToObject(name)) && parameterSet.Remove(name))
                    {
                        removed.Add(string.Format("Model parameter '{0}' (simulation results).", name));
                    }
                }
            }

            if (result.HasValue(AnalyticalModelParameter.PartOMaterialisationRecord))
            {
                kept.Add("The Part O materialisation record: this is a materialised mixed model. Open the baseline it was built from instead.");
            }

            if (result.HasValue(AnalyticalModelParameter.PartOIsolationContext))
            {
                kept.Add("The Part O isolation context: this model is an isolated part of the building, and the rest of the building cannot be put back. Open the source model instead.");
            }

            AdjacencyCluster adjacencyCluster = result.AdjacencyCluster;
            if (adjacencyCluster is null)
            {
                return result;
            }

            // ---- run output, cluster ---------------------------------------------------------------------------

            //By type against IResult, exactly as the validator counts them, so a later result type is removed the
            //day it appears.
            SortedDictionary<string, int> counts_Result = new(StringComparer.Ordinal);
            int count_Result = 0;
            foreach (Type type in adjacencyCluster.GetTypes() ?? [])
            {
                if (type is null || !typeof(IResult).IsAssignableFrom(type))
                {
                    continue;
                }

                foreach (object @object in adjacencyCluster.GetObjects(type) ?? [])
                {
                    if (@object is IJSAMObject jSAMObject && adjacencyCluster.RemoveObject(jSAMObject))
                    {
                        count_Result++;
                        counts_Result[type.Name] = counts_Result.TryGetValue(type.Name, out int count) ? count + 1 : 1;
                    }
                }
            }

            if (count_Result != 0)
            {
                List<string> texts = [];
                foreach (KeyValuePair<string, int> keyValuePair in counts_Result)
                {
                    texts.Add(string.Format("{0} {1}", keyValuePair.Value, keyValuePair.Key));
                }

                removed.Add(string.Format("{0} simulation result object(s): {1}.", count_Result, string.Join(", ", texts)));
            }

            int count_DesignDay = (adjacencyCluster.GetObjects<DesignDay>() ?? []).Count(x => x is not null && adjacencyCluster.RemoveObject(x));
            if (count_DesignDay != 0)
            {
                removed.Add(string.Format("{0} design-day record(s) the TAS workflow wrote into the cluster (the model's own design-day parameters are kept).", count_DesignDay));
            }

            // ---- Part O preparation plant ----------------------------------------------------------------------

            List<VentilationSystem> ventilationSystems_PartO = [];
            HashSet<Guid> guids_System_PartO = [];
            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem?.Type is not null && ventilationSystem.Type.Guid == Guid_VentilationSystemType_PartOMVHR)
                {
                    ventilationSystems_PartO.Add(ventilationSystem);
                    guids_System_PartO.Add(ventilationSystem.Guid);
                }
            }

            if (ventilationSystems_PartO.Count != 0)
            {
                //The unit names every REMAINING system still uses, so a unit shared with an authored system stays.
                HashSet<string> names_Unit_Kept = new(StringComparer.Ordinal);
                foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
                {
                    if (ventilationSystem is not null && !guids_System_PartO.Contains(ventilationSystem.Guid))
                    {
                        foreach (string name_Unit in UnitNames(ventilationSystem))
                        {
                            names_Unit_Kept.Add(name_Unit);
                        }
                    }
                }

                List<string> names_System = [];
                List<string> names_Unit = [];
                int count_Terminal = 0;
                int count_Movement_Before = (adjacencyCluster.GetObjects<SpaceAirMovement>()?.Count ?? 0) + (adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>()?.Count ?? 0);

                foreach (VentilationSystem ventilationSystem in ventilationSystems_PartO)
                {
                    names_System.Add(ventilationSystem.FullName ?? ventilationSystem.Name);

                    List<AirHandlingUnit> airHandlingUnits = [];
                    foreach (string name_Unit in UnitNames(ventilationSystem))
                    {
                        AirHandlingUnit airHandlingUnit = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x?.Name == name_Unit);
                        if (airHandlingUnit is not null && airHandlingUnits.Find(x => x.Guid == airHandlingUnit.Guid) is null)
                        {
                            airHandlingUnits.Add(airHandlingUnit);
                        }
                    }

                    //The dwelling the preparation routed this system's air over, settled as RealizeBaseMVHRDwelling
                    //settles it, so exactly the movements it built are cleared.
                    List<Space> spaces_Served = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [];
                    List<Space> spaces_Dwelling = adjacencyCluster.PartFTransferAirSpaces(spaces_Served, out _) ?? spaces_Served;

                    if (airHandlingUnits.Count == 0)
                    {
                        RemoveBaseMVHRAirMovementObjects(adjacencyCluster, ventilationSystem, null, spaces_Dwelling);
                    }

                    foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
                    {
                        RemoveBaseMVHRAirMovementObjects(adjacencyCluster, ventilationSystem, airHandlingUnit, spaces_Dwelling);
                    }

                    //A terminal goes only when every system it is connected to goes: one an authored system also
                    //uses is that system's.
                    foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem) ?? [])
                    {
                        List<VentilationSystem> ventilationSystems_Terminal = adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? [];
                        if (ventilationTerminal is not null && ventilationSystems_Terminal.TrueForAll(x => x is null || guids_System_PartO.Contains(x.Guid)) && adjacencyCluster.RemoveObject(ventilationTerminal))
                        {
                            count_Terminal++;
                        }
                    }

                    foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits)
                    {
                        if (!names_Unit_Kept.Contains(airHandlingUnit.Name) && adjacencyCluster.RemoveObject<AirHandlingUnit>(airHandlingUnit.Guid))
                        {
                            names_Unit.Add(airHandlingUnit.Name);
                        }
                    }

                    adjacencyCluster.RemoveObject<VentilationSystem>(ventilationSystem.Guid);
                }

                int count_Movement = count_Movement_Before - (adjacencyCluster.GetObjects<SpaceAirMovement>()?.Count ?? 0) - (adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>()?.Count ?? 0);

                names_System.Sort(StringComparer.Ordinal);
                names_Unit.Sort(StringComparer.Ordinal);

                removed.Add(string.Format("{0} Part O MVHR ventilation system(s) {1}.", names_System.Count, string.Join(", ", names_System.ConvertAll(x => string.Format("'{0}'", x)))));

                if (names_Unit.Count != 0)
                {
                    removed.Add(string.Format("{0} Part O air handling unit(s) {1}.", names_Unit.Count, string.Join(", ", names_Unit.ConvertAll(x => string.Format("'{0}'", x)))));
                }

                if (count_Terminal != 0)
                {
                    removed.Add(string.Format("{0} design terminal(s) connected to those systems (the Part F requirements they were realised from are kept).", count_Terminal));
                }

                if (count_Movement != 0)
                {
                    removed.Add(string.Format("{0} air movement(s) the Part O preparation built for those dwellings (supply, extract and internal transfer air).", count_Movement));
                }
            }

            // ---- the per-space Part F internal condition -------------------------------------------------------

            List<Space> spaces = adjacencyCluster.GetSpaces() ?? [];

            //The conditions the model holds that no Part O preparation wrote: the cluster's own (Map IC (TM59)
            //leaves its library there) and those of spaces that were not rewritten. The candidates a clone's base
            //is looked up among.
            List<InternalCondition> internalConditions_Held = [];
            foreach (InternalCondition internalCondition in adjacencyCluster.GetObjects<InternalCondition>() ?? [])
            {
                if (internalCondition is not null)
                {
                    internalConditions_Held.Add(internalCondition);
                }
            }

            foreach (Space space in spaces)
            {
                if (space?.InternalCondition is not null && !Query.IsPartFAppliedInternalCondition(space))
                {
                    internalConditions_Held.Add(space.InternalCondition);
                }
            }

            List<string> names_Restored = [];
            foreach (Space space in spaces)
            {
                if (space is null || !Query.IsPartFAppliedInternalCondition(space))
                {
                    continue;
                }

                InternalCondition internalCondition = space.InternalCondition;
                string name_Base = Query.PartFAppliedInternalConditionBaseName(internalCondition.Name, space.Name);

                string refusal = RestorablePartFInternalCondition(internalCondition, name_Base, internalConditions_Held, out InternalCondition internalCondition_Base);
                if (refusal is not null)
                {
                    kept.Add(string.Format("Space '{0}' keeps its Part F internal condition '{1}': {2}", space.Name, internalCondition.Name, refusal));

                    continue;
                }

                InternalCondition internalCondition_Restored = new(internalCondition_Base.Name, internalCondition);
                foreach (InternalConditionParameter internalConditionParameter in internalConditionParameters_PartFAirflow)
                {
                    if (internalCondition_Base.TryGetValue(internalConditionParameter, out double value))
                    {
                        internalCondition_Restored.SetValue(internalConditionParameter, value);
                    }
                    else
                    {
                        internalCondition_Restored.RemoveValue(internalConditionParameter);
                    }
                }

                //A copy, replaced by guid: the cluster copy above shares its Space objects with the caller's model.
                adjacencyCluster.AddObject(new Space(space) { InternalCondition = internalCondition_Restored });

                names_Restored.Add(string.Format("'{0}' -> '{1}'", space.Name, internalCondition_Base.Name));
            }

            if (names_Restored.Count != 0)
            {
                names_Restored.Sort(StringComparer.Ordinal);
                removed.Add(string.Format("{0} per-space Part F internal condition(s), each restored to the condition it was made from: {1}.", names_Restored.Count, string.Join(", ", names_Restored)));
            }

            return new AnalyticalModel(result, adjacencyCluster);
        }

        /// <summary>
        /// Why the per-space Part F condition <paramref name="internalCondition"/> cannot be restored to its base
        /// <paramref name="name_Base"/>, or null - with the base in <paramref name="internalCondition_Base"/> -
        /// where the model proves it. See <see cref="RemovePartORunState"/>.
        /// </summary>
        private static string RestorablePartFInternalCondition(InternalCondition internalCondition, string name_Base, List<InternalCondition> internalConditions_Held, out InternalCondition internalCondition_Base)
        {
            internalCondition_Base = null;

            if (string.IsNullOrWhiteSpace(name_Base))
            {
                return "its name does not state the condition it was made from.";
            }

            List<InternalCondition> internalConditions_Base = internalConditions_Held.FindAll(x => x.Name == name_Base);
            if (internalConditions_Base.Count == 0)
            {
                return string.Format("the model holds no condition named '{0}', so what the Part F rates replaced cannot be proven.", name_Base);
            }

            internalCondition_Base = internalConditions_Base[0];
            foreach (InternalCondition internalCondition_Other in internalConditions_Base)
            {
                if (ParameterDifference(internalCondition_Other, internalCondition_Base, null) is not null || ParameterDifference(internalCondition_Base, internalCondition_Other, null) is not null)
                {
                    internalCondition_Base = null;

                    return string.Format("the model holds more than one different condition named '{0}', so which one it was made from is ambiguous.", name_Base);
                }
            }

            foreach (InternalConditionParameter internalConditionParameter in internalConditionParameters_PartFAirflow)
            {
                if (internalCondition_Base.TryGetValue(internalConditionParameter, out double value) && value != 0)
                {
                    InternalCondition internalCondition_Found = internalCondition_Base;
                    internalCondition_Base = null;

                    return string.Format("the base condition '{0}' states {1} {2}, so the value the Part F rate displaced cannot be told apart from the base.", internalCondition_Found.Name, Core.Query.Name(internalConditionParameter), value);
                }
            }

            HashSet<string> names_Airflow = [];
            foreach (InternalConditionParameter internalConditionParameter in internalConditionParameters_PartFAirflow)
            {
                names_Airflow.Add(Core.Query.Name(internalConditionParameter));
            }

            string difference = ParameterDifference(internalCondition_Base, internalCondition, names_Airflow);
            if (difference is not null)
            {
                internalCondition_Base = null;

                return string.Format("it differs from '{0}' in {1}, so it is not proven to have been made from it.", name_Base, difference);
            }

            return null;
        }

        /// <summary>
        /// The first parameter <paramref name="parameterizedSAMObject"/> states that <paramref name="parameterizedSAMObject_Other"/>
        /// does not state with an equal value, ignoring <paramref name="names_Ignored"/>; null where there is none.
        /// </summary>
        private static string ParameterDifference(ParameterizedSAMObject parameterizedSAMObject, ParameterizedSAMObject parameterizedSAMObject_Other, HashSet<string> names_Ignored)
        {
            List<ParameterSet> parameterSets_Other = parameterizedSAMObject_Other.GetParameterSets() ?? [];

            foreach (ParameterSet parameterSet in parameterizedSAMObject.GetParameterSets() ?? [])
            {
                foreach (string name in parameterSet?.Names ?? [])
                {
                    if (names_Ignored is not null && names_Ignored.Contains(name))
                    {
                        continue;
                    }

                    ParameterSet parameterSet_Other = parameterSets_Other.Find(x => x?.Name == parameterSet.Name && x.Contains(name));
                    if (parameterSet_Other is null || !SameValue(parameterSet.ToObject(name), parameterSet_Other.ToObject(name)))
                    {
                        return string.Format("'{0}'", name);
                    }
                }
            }

            return null;
        }

        private static bool SameValue(object value, object value_Other)
        {
            if (value is null || value_Other is null)
            {
                return value is null && value_Other is null;
            }

            if (value is IJSAMObject jSAMObject && value_Other is IJSAMObject jSAMObject_Other)
            {
                return jSAMObject.ToJsonObject()?.ToJsonString() == jSAMObject_Other.ToJsonObject()?.ToJsonString();
            }

            if (value is double @double && value_Other is double double_Other)
            {
                return @double.Equals(double_Other);
            }

            if (value is IEnumerable enumerable && value is not string && value_Other is IEnumerable enumerable_Other && value_Other is not string)
            {
                List<object> objects = [.. enumerable.Cast<object>()];
                List<object> objects_Other = [.. enumerable_Other.Cast<object>()];

                if (objects.Count != objects_Other.Count)
                {
                    return false;
                }

                for (int i = 0; i < objects.Count; i++)
                {
                    if (!SameValue(objects[i], objects_Other[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return value.Equals(value_Other);
        }
    }
}
