// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether <paramref name="analyticalModel"/> is a clean pre-Part-O baseline that mixed dwelling
        /// strategies may be materialised from - see <see cref="PartOBaselineFindings(AnalyticalModel)"/>.
        /// </summary>
        public static bool IsPartOCleanBaseline(this AnalyticalModel analyticalModel, out List<PartOMaterialisationRefusal> findings)
        {
            findings = PartOBaselineFindings(analyticalModel);

            return findings.Count == 0;
        }

        /// <summary>
        /// Every reason <paramref name="analyticalModel"/> is <b>not</b> a clean pre-Part-O baseline. Empty means
        /// clean. Owner decision D1: mixed materialisation starts from a clean baseline, and nothing here
        /// cleans, repairs or "adopts" a model - it only says why the model is refused. Cleaning is
        /// <c>Modify.RemovePartORunState</c> (owner decision, 30 Sep 2026): it removes what this query names as
        /// run output or Part O preparation, restores a per-space Part F internal condition only where the model
        /// proves what it replaced, and its copy is judged by this query afterwards, never by itself.
        ///
        /// <para><b>What a baseline IS</b></para>
        /// <para>
        /// The design layer: geometry, constructions, openings, authored internal conditions, zones with
        /// <c>IsDwelling</c>, Approved Document F requirements, design ventilation terminals the designer (or an
        /// accepted Iteration 2B outcome) stated, the model's weather and its <b>model-level</b> heating and
        /// cooling design days, project settings, and the dwelling strategy collection itself.
        /// </para>
        ///
        /// <para><b>What it is NOT - the two groups of signals</b></para>
        /// <list type="bullet">
        /// <item>
        /// <b>Materialisation</b> (<see cref="PartOMaterialisationRefusalReason.MaterialisedBaseline"/>): a
        /// ventilation system of the Part O MVHR type; an internal condition carrying the per-space Part F rate
        /// <c>Modify.ApplyPartFVentilationRates</c> writes (its <c>&lt;condition&gt; - &lt;space&gt;</c> name
        /// together with a supply or extract airflow); a <see cref="PartOMaterialisationRecord"/>; a
        /// <see cref="PartOIsolationContext"/> (an isolated derivative is a run artefact, not a building).
        /// </item>
        /// <item>
        /// <b>Run output</b> (<see cref="PartOMaterialisationRefusalReason.RunOutputBaseline"/>): overheating
        /// scenarios; a <see cref="SimulationResultProvenance"/>; <b>any</b> object in the cluster, or value on
        /// the model, deriving from <see cref="IResult"/> - the base type, not a list, so a later result type is
        /// covered the day it appears; and design-day records held <b>in the adjacency cluster</b>.
        /// </item>
        /// <item>
        /// <b>Unresolvable air movements</b> (<see cref="PartOMaterialisationRefusalReason.UnresolvedAirMovement"/>).
        /// Air movements as such are authored design data (inter-zone air movements an engineer builds with the
        /// IZAM components) and are accepted; their compatibility with each dwelling's strategy is the
        /// materialisation's question.
        /// </item>
        /// </list>
        ///
        /// <para><b>The DesignDay rule (the PR0 design gate, pinned by test)</b></para>
        /// <para>
        /// The model-level <c>AnalyticalModelParameter.HeatingDesignDays</c> / <c>CoolingDesignDays</c> are
        /// design inputs - derived from the model's weather, or an engineer's stated override - and a baseline
        /// may carry them. <c>DesignDay</c> objects <b>in the adjacency cluster</b> are written there by the TAS
        /// workflow after a run (<c>SAM_Tas Modify.ReplaceDesignDays</c> from <c>WorkflowCalculator</c>); no
        /// authoring path puts them there, and they are the records whose accumulation grew a re-run
        /// <c>.sam</c>. So they are run output and refuse. A false positive only refuses, which is the safe
        /// direction.
        /// </para>
        /// </summary>
        /// <returns>The findings, one per signal kind found; empty for a clean baseline.</returns>
        public static List<PartOMaterialisationRefusal> PartOBaselineFindings(this AnalyticalModel analyticalModel)
        {
            List<PartOMaterialisationRefusal> result = [];

            if (analyticalModel is null)
            {
                result.Add(new PartOMaterialisationRefusal(PartOMaterialisationRefusalReason.NoModel, "No analytical model was supplied, so there is no baseline to materialise from."));

                return result;
            }

            // ---- run output, model level -------------------------------------------------------------------

            if (analyticalModel.HasValue(AnalyticalModelParameter.OverheatingScenarios))
            {
                result.Add(RunOutput("The model carries overheating scenarios, so it has been through a Part O run: scenarios are stated for a run, not authored on a baseline."));
            }

            if (analyticalModel.HasValue(AnalyticalModelParameter.SimulationResultProvenance))
            {
                result.Add(RunOutput("The model carries a simulation result provenance record, so it is the output of a simulation, not a baseline."));
            }

            if (analyticalModel.HasValue(AnalyticalModelParameter.PartOBaselineReference))
            {
                result.Add(RunOutput("The model carries a Part O baseline reference, so it is a Part O result derived from another model, not a baseline."));
            }

            if (ModelResults(analyticalModel) is string name_Result)
            {
                result.Add(RunOutput(string.Format("The model carries simulation results as its own parameter '{0}', so it is the output of a simulation, not a baseline.", name_Result)));
            }

            // ---- materialisation, model level --------------------------------------------------------------

            if (analyticalModel.HasValue(AnalyticalModelParameter.PartOMaterialisationRecord))
            {
                result.Add(Materialised("The model carries a Part O materialisation record, so it is a materialised mixed model, not the baseline it was built from. Materialise again from that baseline instead."));
            }

            if (analyticalModel.HasValue(AnalyticalModelParameter.PartOIsolationContext))
            {
                result.Add(Materialised("The model carries a Part O isolation context, so it is an isolated derivative of a building prepared for a run, not the building itself."));
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster is null)
            {
                result.Add(new PartOMaterialisationRefusal(PartOMaterialisationRefusalReason.NoModel, "The model carries no adjacency cluster, so there is no baseline to materialise from."));

                return result;
            }

            // ---- run output, cluster -----------------------------------------------------------------------

            //By the TYPES the cluster stores, each tested against IResult: the cluster answers no interface
            //query (GetObjects<IResult>() is null), and a list of result classes would miss the next one.
            int count_Result = 0;
            string name_Type = null;
            foreach (Type type in adjacencyCluster.GetTypes() ?? [])
            {
                if (type is null || !typeof(IResult).IsAssignableFrom(type))
                {
                    continue;
                }

                int count = adjacencyCluster.GetObjects(type)?.Count ?? 0;
                if (count != 0)
                {
                    count_Result += count;
                    name_Type ??= type.Name;
                }
            }

            if (count_Result != 0)
            {
                result.Add(RunOutput(string.Format("The model's cluster carries {0} simulation result object(s) (for example {1}), so it is the output of a simulation, not a baseline. Accepting it would feed a run's output back in as its input.", count_Result, name_Type)));
            }

            List<DesignDay> designDays = adjacencyCluster.GetObjects<DesignDay>() ?? [];
            if (designDays.Count != 0)
            {
                result.Add(RunOutput(string.Format("The model's cluster carries {0} design-day record(s). The TAS workflow writes those into the cluster after a run (Modify.ReplaceDesignDays); a baseline states its design days as the model's own Heating/Cooling Design Days parameters, which are accepted.", designDays.Count)));
            }

            // ---- materialisation, cluster ------------------------------------------------------------------

            List<string> names_System = [];
            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem?.Type is not null && ventilationSystem.Type.Guid == Modify.Guid_VentilationSystemType_PartOMVHR)
                {
                    names_System.Add(ventilationSystem.FullName ?? ventilationSystem.Name);
                }
            }

            if (names_System.Count != 0)
            {
                names_System.Sort(StringComparer.Ordinal);

                result.Add(Materialised(string.Format("The model already carries Part O MVHR system(s) {0}, so it has been prepared for a Part O run. Remove the Part O run state into a cleaned copy (Modify.RemovePartORunState), or reopen the pre-Part-O source model.", string.Join(", ", names_System.ConvertAll(x => string.Format("'{0}'", x))))));
            }

            //Air movements are NOT materialisation state by themselves. An engineer authors inter-zone air movements
            //(SAMAnalytical.CreateIZAMBySpaces: unit -> space or space -> space; CreateIZAMBySetPoint: a unit's
            //plant-zone conditions) as ordinary design data, and a homogeneous Iteration 1b run carries them to TAS
            //unchanged. What the baseline cannot carry is a movement whose endpoints do not resolve: SAM_Tas
            //Modify.UpdateIZAMs resolves From/To by object reference and drops a movement whose source is missing
            //and sends the air of one whose destination is missing to OUTSIDE - a model that states one airflow and
            //simulates another. Movements generated by a Part O preparation come with the Part O MVHR system refused
            //above. Whether an authored movement is compatible with the selected strategies is decided per dwelling
            //by the materialisation (AuthoredAirMovementConflict), not here.
            List<string> names_Unresolved = [];
            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                if (spaceAirMovement is null)
                {
                    continue;
                }

                AirMovementEndpoint(adjacencyCluster, spaceAirMovement.From, out bool resolved_From);
                AirMovementEndpoint(adjacencyCluster, spaceAirMovement.To, out bool resolved_To);

                if (!resolved_From || !resolved_To || string.IsNullOrWhiteSpace(spaceAirMovement.From))
                {
                    names_Unresolved.Add(spaceAirMovement.Name ?? spaceAirMovement.Guid.ToString());
                }
            }

            if (names_Unresolved.Count != 0)
            {
                names_Unresolved.Sort(StringComparer.Ordinal);

                result.Add(new PartOMaterialisationRefusal(PartOMaterialisationRefusalReason.UnresolvedAirMovement, string.Format("{0} air movement(s) state a source or destination the model does not contain (for example '{1}'). TAS would drop such a movement, or send its air to outside, so the model would simulate an airflow it does not state. Correct or remove the movement in the source model.", names_Unresolved.Count, names_Unresolved[0])));
            }

            List<string> names_Space = [];
            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                if (IsPartFAppliedInternalCondition(space))
                {
                    names_Space.Add(space.Name);
                }
            }

            if (names_Space.Count != 0)
            {
                names_Space.Sort(StringComparer.Ordinal);

                result.Add(Materialised(string.Format("{0} space(s) carry the per-space internal condition Modify.ApplyPartFVentilationRates writes (for example '{1}'), so the model's authored internal conditions have already been replaced by a Part O preparation. Removing the Part O run state (Modify.RemovePartORunState) restores a condition only where the model proves what it replaced; otherwise reopen the pre-Part-O source model.", names_Space.Count, names_Space[0])));
            }

            return result;
        }

        /// <summary>
        /// Whether a sized space's internal condition is the per-space clone <c>Modify.ApplyPartFVentilationRates</c>
        /// wrote: its name is <c>&lt;condition&gt; - &lt;space&gt;</c> (optionally disambiguated
        /// <c>" (n)"</c>) AND it carries a supply or extract airflow. Both, so an authored condition that merely
        /// carries an airflow, or merely has such a name, is not mistaken for one.
        /// </summary>
        internal static bool IsPartFAppliedInternalCondition(Space space)
        {
            InternalCondition internalCondition = space?.InternalCondition;
            if (internalCondition is null || string.IsNullOrWhiteSpace(space.Name) || !space.HasValue(SpaceParameter.PartFSpaceData))
            {
                return false;
            }

            if (!internalCondition.HasValue(Analytical.InternalConditionParameter.SupplyAirFlow) && !internalCondition.HasValue(Analytical.InternalConditionParameter.ExhaustAirFlow))
            {
                return false;
            }

            string name = internalCondition.Name ?? string.Empty;
            string suffix = string.Format(" - {0}", space.Name);

            if (name.EndsWith(")", StringComparison.Ordinal))
            {
                int index = name.LastIndexOf(" (", StringComparison.Ordinal);
                if (index > 0 && int.TryParse(name.Substring(index + 2, name.Length - index - 3), out int _))
                {
                    name = name.Substring(0, index);
                }
            }

            return name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length;
        }

        /// <summary>
        /// The name of the condition a per-space Part F condition was cloned from: <paramref name="name_InternalCondition"/>
        /// without the <c>" (n)"</c> disambiguation and every trailing <c>" - &lt;space&gt;"</c> suffix of this
        /// space - the inverse of <c>Modify.ApplyPartFVentilationRates</c>' naming, which strips the same way
        /// before it appends. Null where nothing is left.
        /// </summary>
        internal static string PartFAppliedInternalConditionBaseName(string name_InternalCondition, string name_Space)
        {
            if (string.IsNullOrWhiteSpace(name_InternalCondition) || string.IsNullOrWhiteSpace(name_Space))
            {
                return null;
            }

            string suffix = string.Format(" - {0}", name_Space);
            string result = name_InternalCondition;

            if (result.EndsWith(")", StringComparison.Ordinal))
            {
                int index = result.LastIndexOf(" (", StringComparison.Ordinal);
                if (index > 0 && int.TryParse(result.Substring(index + 2, result.Length - index - 3), out int _) && result.Substring(0, index).EndsWith(suffix, StringComparison.Ordinal))
                {
                    result = result.Substring(0, index);
                }
            }

            while (result.EndsWith(suffix, StringComparison.Ordinal))
            {
                result = result.Substring(0, result.Length - suffix.Length);
            }

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        /// <summary>
        /// The object an air movement endpoint names, resolved exactly as SAM_Tas <c>Modify.UpdateIZAMs</c> does - by
        /// object reference. A null or blank endpoint is outside and is resolved (<paramref name="resolved"/> true,
        /// result null); a stated endpoint the cluster does not hold is not.
        /// </summary>
        internal static SAMObject AirMovementEndpoint(AdjacencyCluster adjacencyCluster, string reference, out bool resolved)
        {
            resolved = true;

            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(reference);

            SAMObject result = objectReference is null ? null : adjacencyCluster?.GetObjects<SAMObject>(objectReference)?.Find(x => x is not null);

            resolved = result is not null;

            return result;
        }

        /// <summary>The name of the first model-level parameter holding a simulation result, or null.</summary>
        private static string ModelResults(AnalyticalModel analyticalModel)
        {
            foreach (ParameterSet parameterSet in analyticalModel.GetParameterSets() ?? [])
            {
                foreach (string name in parameterSet?.Names ?? [])
                {
                    if (IsResultValue(parameterSet.ToObject(name)))
                    {
                        return name;
                    }
                }
            }

            return null;
        }

        /// <summary>Whether a parameter value is, or holds, a simulation result.</summary>
        internal static bool IsResultValue(object @object)
        {
            if (@object is IResult)
            {
                return true;
            }

            if (@object is IEnumerable enumerable && @object is not string)
            {
                foreach (object item in enumerable)
                {
                    if (item is IResult)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static PartOMaterialisationRefusal RunOutput(string message) => new(PartOMaterialisationRefusalReason.RunOutputBaseline, message);

        private static PartOMaterialisationRefusal Materialised(string message) => new(PartOMaterialisationRefusalReason.MaterialisedBaseline, message);
    }
}
