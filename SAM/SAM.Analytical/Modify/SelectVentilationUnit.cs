// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Chooses the smallest ventilation unit product that can move one air handling unit's current
        /// design duty, and records the choice on that unit.
        /// <para>
        /// <b>One dwelling, one unit, one selection.</b> The duty comes from the design terminals of the
        /// systems <i>this</i> unit supplies and from nothing else - there is no aggregation across
        /// dwellings, no shared plant and no borrowed capacity. Selecting for two dwellings is two calls,
        /// and the second cannot change the first's answer.
        /// </para>
        /// <para>
        /// <b>Only the identity is written.</b> The product's capacities are not copied onto the model:
        /// they stay in the catalogue and are looked up, so that "what the equipment can do" can never be
        /// mistaken for "what the design does". See <see cref="VentilationUnitReference"/>.
        /// </para>
        /// <para>
        /// <b>Nothing else on the model moves.</b> No design airflow is raised to the selected unit's
        /// rating, no Approved Document F requirement is touched, and no runtime or profile airflow is
        /// written. A 115 l/s dwelling fitted with a 150 l/s unit is still a 115 l/s dwelling; the
        /// remaining 35 l/s is design headroom for an Approved Document O iteration to spend
        /// deliberately, room by room, and never something a selection spends for it.
        /// </para>
        /// <para>
        /// <b>Re-selecting is how a unit is escalated.</b> Once optimisation has grown the design duty past
        /// the selected product's rating - <c>Query.IsVentilationUnitSufficient</c> says so - calling this
        /// again from the <i>current</i> design duty selects the next compliant product. The Approved
        /// Document F requirement is not reset and is not consulted: what grew is the design.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">
        /// The model. <b>Modified in place</b> on success, so hand it a cluster you already own -
        /// <c>AnalyticalModel.AdjacencyCluster</c> returns a copy, and <b>this cluster is as far as the
        /// change reaches</b>: the selection is written onto a guid-preserving replacement unit added over
        /// the model's own, never set on the shared object a sibling cluster would see it through.
        /// </param>
        /// <param name="airHandlingUnit">
        /// The unit to select a product for, identified by guid. <b>Not written to</b> - read the choice
        /// back from the returned selection, or re-resolve the unit from the cluster.
        /// </param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The products available to choose from. An argument rather than a library read, because which
        /// products exist is a fact about whoever is asking - see <c>Query.CapableVentilationUnits</c>.
        /// </param>
        /// <param name="notes">What was selected, against what duty, and with how much headroom.</param>
        /// <param name="refusals">Why nothing was selected, one sentence. Empty on success.</param>
        /// <returns>
        /// The selection, which is never null: a refusal is an outcome and carries its own reason.
        /// <b>Nothing is written to the model on a refusal</b> - a unit left with its previous selection,
        /// or with none, is an honest state, and half-selecting would be a model somebody schedules.
        /// </returns>
        public static VentilationUnitSelection SelectVentilationUnit(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, out List<string> notes, out List<string> refusals, double tolerance_Lps = 0.001)
        {
            notes = [];
            refusals = [];

            //Checked before every comparison below - see Query.IsValidFlowRateTolerance.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                VentilationUnitSelection result_Tolerance = VentilationUnitSelection.Refused(Query.FlowRateToleranceRefusal(tolerance_Lps));

                refusals.Add(result_Tolerance.Reason);

                return result_Tolerance;
            }

            if (adjacencyCluster is null || airHandlingUnit is null)
            {
                VentilationUnitSelection result_Null = VentilationUnitSelection.Refused("No air handling unit was supplied, so no ventilation unit product could be selected.");

                refusals.Add(result_Null.Reason);

                return result_Null;
            }

            if (!adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps))
            {
                VentilationUnitSelection result_NoDuty = VentilationUnitSelection.Refused(string.Format(
                    "Air handling unit '{0}' supplies no ventilation system carrying design terminals, so there is no design duty to select a product against. Realize the Approved Document F requirements and connect them to a system first.",
                    airHandlingUnit.Name));

                refusals.Add(result_NoDuty.Reason);

                return result_NoDuty;
            }

            VentilationUnitSelection result = ventilationUnitCapacityDescriptors.SelectSmallestCapableVentilationUnit(supplyDuty_Lps, extractDuty_Lps, tolerance_Lps);

            if (!result.IsSelected)
            {
                refusals.Add(string.Format("Air handling unit '{0}': {1}", airHandlingUnit.Name, result.Reason));

                return result;
            }

            //The cluster's OWN instance is the one written over, resolved by guid rather than trusted as
            //handed in: a caller holding a unit from before an earlier step would otherwise write the
            //selection onto an object the model has already replaced.
            //Resolved by GUID, and REFUSED where that fails rather than falling back to the caller's object.
            //
            //The duty above was resolved through the unit's NAME, which is how a ventilation system names
            //its plant, so a detached unit that merely shares a name with one in the model gets a duty and
            //looks selectable. Writing the selection onto that object and adding it would put a SECOND unit
            //of the same name into the cluster with the product reference on the wrong one, and every
            //name-based lookup afterwards - Query.VentilationSystems and the TAS export among them - could
            //resolve the original, unselected unit instead. A selection nothing can find again is worse
            //than no selection.
            AirHandlingUnit airHandlingUnit_Model = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x is not null && x.Guid == airHandlingUnit.Guid);

            if (airHandlingUnit_Model is null)
            {
                VentilationUnitSelection result_Detached = VentilationUnitSelection.Refused(string.Format(
                    "Air handling unit '{0}' is not in this model - no unit of that identity was found, though the model may well hold another unit of the same name. Selecting a product onto an object the model does not contain would leave the selection where nothing can resolve it. Nothing was changed: take the unit from the model and select onto that one.",
                    airHandlingUnit.Name));

                refusals.Add(result_Detached.Reason);

                return result_Detached;
            }

            //The selection is written onto a COPY and added OVER the model's instance, never set on that
            //instance in place.
            //
            //AdjacencyCluster's copy constructor copies the object dictionary and not the objects in it, so
            //several clusters routinely share one air handling unit object - an AnalyticalModel and the copy
            //AnalyticalModel.AdjacencyCluster hands out among them. Setting the parameter in place writes
            //this selection into every one of them at once, so a caller promised a cluster it could keep or
            //throw away would find the plant escalated on the model it kept, with nothing in the answer
            //saying so. The airflow half of Modify.ApplyTargetedDesignAirFlow has always written
            //replacements rather than mutating terminals for exactly this reason; the equipment half now
            //follows the same discipline.
            //
            //AirHandlingUnit's one-argument constructor is the copy this needs, and the whole of it:
            //SAMObject carries the guid and name across, ParameterizedSAMObject clones the parameter sets so
            //the write lands on this copy alone, and ComplexEquipment clones the internal equipment model.
            //AddObject therefore replaces the unit under its own identity and every relation to it survives.
            //
            //Reading the selection back through the object that was handed IN is consequently not a thing
            //this method ever promised - it already refused to write there. Take it from the returned
            //VentilationUnitSelection, or re-resolve from the cluster by guid.
            AirHandlingUnit airHandlingUnit_Selected = new AirHandlingUnit(airHandlingUnit_Model);

            airHandlingUnit_Selected.SetValue(AirHandlingUnitParameter.VentilationUnitReference, result.VentilationUnitReference);

            adjacencyCluster.AddObject(airHandlingUnit_Selected);

            notes.Add(string.Format(
                "Air handling unit '{0}' selected as '{1}' (supply {2:0.###} l/s, extract {3:0.###} l/s maximum), the smallest product offered that can move the dwelling's design duty of {4:0.###} l/s supply and {5:0.###} l/s extract. {6:0.###} l/s supply and {7:0.###} l/s extract remain as design headroom, and are deliberately NOT taken up: the design duty is unchanged.",
                airHandlingUnit.Name,
                result.VentilationUnitReference,
                result.Descriptor.MaximumSupplyFlowRate_Lps,
                result.Descriptor.MaximumExtractFlowRate_Lps,
                supplyDuty_Lps,
                extractDuty_Lps,
                result.SupplyHeadroom_Lps,
                result.ExtractHeadroom_Lps));

            return result;
        }
    }
}
