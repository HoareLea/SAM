// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Sets one space's design airflow in one direction, leaving every other space alone.
        /// <para>
        /// <b>Space-targeted, because that is the engineering control Approved Document O needs.</b> An
        /// overheating iteration typically fails in one room: a single bedroom on a west facade, with the
        /// rest of the dwelling passing comfortably. Raising that bedroom from 20 to 24 l/s and leaving
        /// the other rooms at 20 and 40 is the correct answer, and a data model that could only scale
        /// every room together could not express it. Nothing here scales anything: a caller wanting a
        /// proportional strategy applies it room by room through this, so a strategy stays a strategy
        /// rather than becoming the only mechanism the model has.
        /// </para>
        /// <para>
        /// <b>The Approved Document F requirement is not touched, and is not allowed to be undercut.</b>
        /// The requirement lives on the space's <c>PartFSpaceData</c> and this never writes to it - it
        /// reads it, and will not record a design below it. That is the lower half of the Iteration 2
        /// invariant <c>PartFRequired &lt;= Design</c>: design airflow is a deliberate choice made
        /// <i>above</i> a regulatory floor, so lowering a room past the floor is not a design decision but
        /// a compliance failure.
        /// </para>
        /// <para>
        /// <b>Below the floor by more than the tolerance is refused; below it by less is raised to the
        /// floor exactly.</b> The two are different mistakes and deserve different answers. A caller
        /// asking for materially less than the Approved Document requires has made a design error and is
        /// told so. A caller whose arithmetic landed a rounding bit under the requirement asked for the
        /// requirement, and refusing a value the comparison cannot tell apart from a legal one would be
        /// noise - but writing it as asked would PERSIST a room designed below its regulatory floor, which
        /// is the thing the invariant exists to prevent. Tolerance decides whether two airflows are the
        /// same number; it is never permission to record a design below the floor.
        /// </para>
        /// <para>
        /// <b>Nothing runtime is written.</b> No profile, no internal condition airflow, no simulation
        /// operating state. The design airflow is what the dwelling's design network is recalculated from
        /// - see <c>Modify.PreparePartOIteration</c>, which rebuilds the air movements and the transfer
        /// air from these terminals - and turning a design number into an operating one is an explicit,
        /// separate step.
        /// </para>
        /// <para>
        /// <b>A subdivided room keeps its subdivision.</b> Where a space carries several terminals of the
        /// direction, the new total is distributed across them <i>in the proportions they already have</i>,
        /// so four diffusers stay four diffusers and a deliberate imbalance between them survives. Where
        /// the existing terminals total zero the share is equal, there being no proportion to preserve.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Modified in place</b> on success.</param>
        /// <param name="space">The room whose design airflow is being set.</param>
        /// <param name="flowClassification">Supply or extract. The two move independently and one call sets one of them.</param>
        /// <param name="designFlowRate_Lps">The new design airflow [l/s] for the room, as a total across its terminals.</param>
        /// <param name="notes">What changed, from what to what, and against which requirement.</param>
        /// <param name="refusals">Why nothing changed, one sentence. Empty on success.</param>
        /// <returns>The space's terminals of that direction as they now stand, or null on a refusal.</returns>
        public static List<VentilationTerminal> SetSpaceDesignFlowRate(this AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, double designFlowRate_Lps, out List<string> notes, out List<string> refusals, double tolerance_Lps = 0.001)
        {
            notes = [];
            refusals = [];

            //Checked before every comparison below - see Query.IsValidFlowRateTolerance.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                refusals.Add(Query.FlowRateToleranceRefusal(tolerance_Lps));

                return null;
            }

            if (adjacencyCluster is null || space is null)
            {
                refusals.Add("No space was supplied, so no design airflow could be set.");

                return null;
            }

            if (double.IsNaN(designFlowRate_Lps) || double.IsInfinity(designFlowRate_Lps) || designFlowRate_Lps < 0)
            {
                refusals.Add(string.Format("Space '{0}': {1} l/s is not a design airflow. Nothing was changed.", space.Name, designFlowRate_Lps));

                return null;
            }

            //Taken from the cluster rather than trusted as handed in: a caller may be holding a space from
            //before the Part F rates were applied, and that one carries a different parameter set.
            //
            //Through the cluster's own O(1) authority, and deliberately NOT through a PartFIndex. This is a
            //WRITE path - it replaces the space's terminals - and a snapshot shared across a loop of writes
            //would answer with the model as it stood before them. GetObject<Space> reads the live object
            //dictionary on every call, so a caller writing room after room is never handed a stale one, and
            //the resolution stops being linear in the model at the same time. It answers exactly what
            //GetSpaces().Find answers: GetSpaces() IS that dictionary flattened into a list, and the guid a
            //space is stored under is the guid Find compares.
            Space space_Cluster = adjacencyCluster.GetObject<Space>(space.Guid);
            if (space_Cluster is null)
            {
                refusals.Add(string.Format("Space '{0}' is not in the model, so its design airflow could not be set.", space.Name));

                return null;
            }

            List<VentilationTerminal> result = Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Cluster), flowClassification) ?? [];

            if (result.Count == 0)
            {
                refusals.Add(string.Format(
                    "Space '{0}' has no design {1} terminal, so there is no design airflow to set. Realize the Approved Document F requirements first, or add a terminal deliberately - creating one here would invent a duty the assessment did not size.",
                    space_Cluster.Name,
                    Core.Query.Description(flowClassification)));

                return null;
            }

            //The regulatory floor, read and never written. Null means the space was never sized, which is
            //not a floor of zero and not a reason to refuse - a designer may deliberately move air through
            //a room Approved Document F said nothing about.
            //
            //The ALREADY-RESOLVED reader, not the one-space query. The rule is the same one - both forms end
            //at Query.PartFRequiredFlowRate_Lps(Space, FlowClassification), which is where the rule lives -
            //and the only thing skipped is a second resolution of a space this method has just resolved out
            //of the same cluster with nothing written in between. The public query would have re-run
            //GetSpaces().Find over the whole model to arrive back at this very instance.
            double? requirement_Lps = Query.PartFRequiredFlowRate_Lps(space_Cluster, flowClassification);

            if (requirement_Lps.HasValue && designFlowRate_Lps < requirement_Lps.Value)
            {
                if (designFlowRate_Lps + tolerance_Lps < requirement_Lps.Value)
                {
                    refusals.Add(string.Format(
                        "Space '{0}': a design {1} airflow of {2:0.###} l/s is below the {3:0.###} l/s Approved Document F requires of that room. Design airflow is chosen above the regulatory minimum, never below it, so nothing was changed. Re-run the Part F calculation if the requirement itself is wrong.",
                        space_Cluster.Name,
                        Core.Query.Description(flowClassification),
                        designFlowRate_Lps,
                        requirement_Lps.Value));

                    return null;
                }

                //WITHIN tolerance below the floor - raised to the floor rather than written as asked.
                //
                //Refusing this would be wrong: a caller whose arithmetic landed a rounding bit under the
                //requirement asked for the requirement, and answering "no" to a value the comparison
                //cannot tell apart from a legal one is not compliance, it is noise. Writing it as asked
                //would be worse: the room is then PERSISTED below what Approved Document F requires of it,
                //and the Iteration 2 invariant is PartFRequired <= Design. So it is set to the floor,
                //exactly.
                //
                //Enforced HERE as well as in ApplyTargetedDesignAirFlow, because this is the setter every
                //design write goes through, and a floor that only holds when reached through the
                //transaction above it is not a floor. See ApplyTargetedDesignAirFlow, which snaps before
                //planning its balancing consequence so the two sides agree about the value being written -
                //by the time a room reaches here, its share of that plan is already decided.
                notes.Add(string.Format(
                    "Space '{0}': the requested design {1} airflow of {2:0.######} l/s is below the {3:0.###} l/s Approved Document F requires of that room by less than the {4:0.###} l/s tolerance, so it was set to exactly that requirement. Tolerance decides whether two airflows are the same number; it never permits a design airflow to be recorded below the regulatory floor.",
                    space_Cluster.Name,
                    Core.Query.Description(flowClassification),
                    designFlowRate_Lps,
                    requirement_Lps.Value,
                    tolerance_Lps));

                designFlowRate_Lps = requirement_Lps.Value;
            }

            //Every EXISTING terminal duty has to be a real, non-negative quantity before any of them is
            //redistributed, because each share is proportional to what that terminal already carries.
            //
            //DesignFlowRate_Lps is publicly settable and is deserialized without a range check, so an
            //infinite one is reachable. It makes the room total infinite, and each share then computes as
            //finite * Infinity / Infinity = NaN - which this method would write and then report as the
            //requested total, while VentilationTerminalDesignDuty_Lps afterwards skips the NaN and reads a
            //duty that is silently wrong. Checked before anything is written, so the model is never left
            //partly mutated.
            if (!IsRedistributable(result, space_Cluster, flowClassification, out string refusal_Terminal))
            {
                refusals.Add(refusal_Terminal);

                return null;
            }

            double total_Lps = Query.VentilationTerminalDesignDuty_Lps(result, flowClassification) ?? 0;

            List<VentilationTerminal> result_Updated = [];

            for (int i = 0; i < result.Count; i++)
            {
                VentilationTerminal ventilationTerminal = result[i];

                double share_Lps;

                if (total_Lps > tolerance_Lps)
                {
                    //Proportional to what the terminal already carries, so a subdivision a designer chose -
                    //and any deliberate imbalance within it - survives the change.
                    double existing_Lps = ventilationTerminal.DesignFlowRate_Lps.HasValue && !double.IsNaN(ventilationTerminal.DesignFlowRate_Lps.Value) ? ventilationTerminal.DesignFlowRate_Lps.Value : 0;

                    share_Lps = designFlowRate_Lps * existing_Lps / total_Lps;
                }
                else
                {
                    share_Lps = designFlowRate_Lps / result.Count;
                }

                //A COPY with the same guid, following Modify.RealizePartFVentilationTerminals: the cluster
                //this call was given is updated while the model the caller may still hold is not reached
                //through the shared object instance.
                VentilationTerminal ventilationTerminal_Updated = new(ventilationTerminal.Guid, ventilationTerminal)
                {
                    DesignFlowRate_Lps = share_Lps
                };

                adjacencyCluster.AddObject(ventilationTerminal_Updated);

                result_Updated.Add(ventilationTerminal_Updated);
            }

            notes.Add(string.Format(
                "Space '{0}': design {1} airflow set from {2:0.###} l/s to {3:0.###} l/s across {4} terminal(s). Approved Document F still requires {5} of that room and is unchanged; no other space was touched and no runtime airflow was written.",
                space_Cluster.Name,
                Core.Query.Description(flowClassification),
                total_Lps,
                designFlowRate_Lps,
                result_Updated.Count,
                requirement_Lps.HasValue ? string.Format("{0:0.###} l/s", requirement_Lps.Value) : "nothing"));

            return result_Updated;
        }

        /// <summary>
        /// Whether every one of a room's design terminals in one direction carries a real, non-negative
        /// quantity of air - the precondition for redistributing a room total across them.
        /// <para>
        /// <b>Shared, because two callers have to agree about it.</b> A room total is shared out in
        /// proportion to what each terminal already carries, so an infinite duty makes the total infinite
        /// and every share <c>finite * Infinity / Infinity</c> = <c>NaN</c>, and a NaN duty is skipped by
        /// <c>Query.VentilationTerminalDesignDuty_Lps</c> so a room total can look healthy while one
        /// terminal is nonsense. <see cref="ApplyTargetedDesignAirFlow"/> calls this over every room it
        /// plans to touch <b>before its first write</b>; discovering it here, one room in, would leave that
        /// transaction's target already mutated and its all-or-nothing promise broken.
        /// </para>
        /// </summary>
        internal static bool IsRedistributable(List<VentilationTerminal> ventilationTerminals, Space space, FlowClassification flowClassification, out string refusal)
        {
            refusal = null;

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals ?? [])
            {
                double? designFlowRate_Lps = ventilationTerminal?.DesignFlowRate_Lps;

                if (designFlowRate_Lps.HasValue && (double.IsNaN(designFlowRate_Lps.Value) || double.IsInfinity(designFlowRate_Lps.Value) || designFlowRate_Lps.Value < 0))
                {
                    refusal = string.Format(
                        "Space '{0}': design {1} terminal '{2}' carries {3} l/s, which is not a quantity of air. A room's design airflow is redistributed in proportion to what its terminals already carry, so nothing could be shared out from it and nothing was changed. Correct that terminal first.",
                        space.Name,
                        Core.Query.Description(flowClassification),
                        ventilationTerminal.Name,
                        designFlowRate_Lps.Value);

                    return false;
                }
            }

            return true;
        }
    }
}
