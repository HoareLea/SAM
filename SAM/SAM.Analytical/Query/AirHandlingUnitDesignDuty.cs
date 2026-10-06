// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The air handling unit a ventilation system supplies from, resolved through the name the system
        /// carries.
        /// <para>
        /// <b>A name rather than a relation is pre-existing technical debt</b>, recorded here rather than
        /// migrated: <c>Modify.AddAirMovementObjects</c>, <c>Modify.AddVentilationSystem</c> and the TAS
        /// export all resolve the unit this way, and changing the binding would be a migration of every
        /// one of them. What this adds is a single place to ask, so the lookup is not written out again
        /// each time it is needed.
        /// </para>
        /// </summary>
        public static AirHandlingUnit AirHandlingUnit(this AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem)
        {
            if (adjacencyCluster is null || ventilationSystem is null)
            {
                return null;
            }

            string name = ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x is not null && x.Name == name);
        }

        /// <summary>
        /// Every ventilation system an air handling unit supplies. Plural on purpose - see
        /// <see cref="AirHandlingUnitDesignDuty"/>.
        /// </summary>
        public static List<VentilationSystem> VentilationSystems(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit)
        {
            List<VentilationSystem> result = [];

            if (adjacencyCluster is null || string.IsNullOrWhiteSpace(airHandlingUnit?.Name))
            {
                return result;
            }

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem is not null && ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName) == airHandlingUnit.Name)
                {
                    result.Add(ventilationSystem);
                }
            }

            return result;
        }

        /// <summary>
        /// What one air handling unit instance has to move at the current design, summed from the design
        /// terminals of every system it supplies.
        /// <para>
        /// <b>Derived, never stored on the unit.</b> A duty written onto the unit would be a second answer
        /// that goes stale the moment a terminal is re-balanced - and re-balancing one room's terminal is
        /// exactly what Approved Document O optimisation does. This is the same rule, for the same reason,
        /// as <see cref="VentilationSystemDesignDuty"/>.
        /// </para>
        /// <para>
        /// <b>This is the design duty, and it is the middle term of the Iteration 2 invariant</b>
        /// <c>PartFRequired &lt;= Design &lt;= SelectedCapacity</c>. It is not the Approved Document F
        /// requirement, which <see cref="PartFRequiredSystemDuty"/> reports and which no design change
        /// moves; and it is not the selected product's capacity, which stays in the catalogue. A unit that
        /// <i>can</i> move 150 l/s serving a dwelling designed at 137 l/s has a design duty of 137.
        /// </para>
        /// <para>
        /// <b>Summed over every system the unit supplies, not one.</b> The Approved Document O workflow
        /// gives each dwelling its own unit, so in that workflow there is exactly one - but the general
        /// MEP arrangement of one unit serving several zones and several systems is precisely what this
        /// architecture must not foreclose, and asking the unit rather than a system is what keeps it
        /// open.
        /// </para>
        /// </summary>
        /// <returns>Whether any design terminal at all was found to sum.</returns>
        public static bool AirHandlingUnitDesignDuty(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps)
        {
            supplyDuty_Lps = 0;
            extractDuty_Lps = 0;

            bool result = false;

            foreach (VentilationSystem ventilationSystem in VentilationSystems(adjacencyCluster, airHandlingUnit))
            {
                if (!adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_System_Lps, out double extractDuty_System_Lps))
                {
                    continue;
                }

                result = true;

                supplyDuty_Lps += supplyDuty_System_Lps;
                extractDuty_Lps += extractDuty_System_Lps;
            }

            return result;
        }

        /// <summary>
        /// The reusable ventilation unit product an air handling unit instance has been selected to be, or
        /// null where none has.
        /// </summary>
        public static VentilationUnitReference SelectedVentilationUnitReference(this AirHandlingUnit airHandlingUnit)
        {
            return airHandlingUnit?.GetValue<VentilationUnitReference>(AirHandlingUnitParameter.VentilationUnitReference);
        }

        /// <summary>
        /// The catalogue entry for the product an air handling unit has been selected to be, found among
        /// the products offered.
        /// <para>
        /// Resolved by <b>product identity</b> rather than by the reference's guid, which is an instance
        /// identity minted fresh each time the catalogue is read - see
        /// <see cref="Analytical.VentilationUnitReference.Matches"/>. Null where the unit has no selection
        /// or where the catalogue offered no longer contains it, which is a real state worth reporting
        /// rather than repairing.
        /// </para>
        /// </summary>
        public static VentilationUnitCapacityDescriptor SelectedVentilationUnitCapacityDescriptor(this AirHandlingUnit airHandlingUnit, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            VentilationUnitReference ventilationUnitReference = SelectedVentilationUnitReference(airHandlingUnit);
            if (ventilationUnitReference is null)
            {
                return null;
            }

            VentilationUnitCapacityDescriptor result = null;

            foreach (VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor in ventilationUnitCapacityDescriptors ?? [])
            {
                if (ventilationUnitCapacityDescriptor is null || !ventilationUnitReference.Matches(ventilationUnitCapacityDescriptor.VentilationUnitReference))
                {
                    continue;
                }

                if (result is null)
                {
                    result = ventilationUnitCapacityDescriptor;

                    continue;
                }

                //A second entry for the same identity. An exact repeat is a duplicated line and answers the
                //same question the same way, so it is ignored; one that disagrees means this identity has no
                //single capability, and returning EITHER would make the unit's adequacy depend on the order
                //the catalogue was read in. Null instead, which IsVentilationUnitSufficient reports as an
                //unknown capacity rather than a pass.
                //
                //Query.SelectSmallestCapableVentilationUnit refuses such a catalogue outright, so a
                //conflicting identity should never reach a unit in the first place. This is the second line
                //of defence, for a unit selected from one catalogue and later checked against another.
                if (ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps != result.MaximumSupplyFlowRate_Lps
                    || ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps != result.MaximumExtractFlowRate_Lps
                    || ventilationUnitCapacityDescriptor.Rank != result.Rank)
                {
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Whether the product an air handling unit is currently selected to be can still move the
        /// dwelling's design duty.
        /// <para>
        /// <b>The check Approved Document O optimisation runs after every design change.</b> Raising one
        /// room's design airflow raises the dwelling's duty, and the selected unit stays valid right up to
        /// its rating - a duty of 137 l/s on a 150 l/s unit is a correct, finished answer and the 13 l/s
        /// left is headroom, not a shortfall to fill. Once the duty passes the rating the unit is
        /// <b>exhausted</b>: this returns false, and the caller selects the next compliant product from
        /// the current design duty - never by resetting the Approved Document F requirements, which have
        /// not changed and are not what grew.
        /// </para>
        /// <para>
        /// A duty exactly at the rating is still valid, within
        /// <paramref name="tolerance_Lps"/>.
        /// </para>
        /// </summary>
        /// <param name="reason">Why the unit no longer suffices, naming both sides. Null while it does.</param>
        /// <returns>
        /// True where the selected product covers the current design duty. <b>False also where no product
        /// is selected or the catalogue does not contain the one that is</b> - in both cases the model
        /// cannot show that the plant is adequate, and <paramref name="reason"/> says which case it is.
        /// </returns>
        public static bool IsVentilationUnitSufficient(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, out string reason, double tolerance_Lps = 0.001)
        {
            reason = null;

            //Before any comparison - an unusable tolerance would otherwise decide adequacy by accident.
            //See Query.IsValidFlowRateTolerance.
            if (!IsValidFlowRateTolerance(tolerance_Lps))
            {
                reason = FlowRateToleranceRefusal(tolerance_Lps);

                return false;
            }

            if (adjacencyCluster is null || airHandlingUnit is null)
            {
                reason = "No air handling unit was supplied, so its selected ventilation unit could not be checked against the design duty.";

                return false;
            }

            //The CLUSTER's unit, resolved by identity - not the object the caller happens to be holding.
            //
            //The duty below is derived through the unit's NAME, so a detached same-named unit, or a stale
            //copy taken before the model re-selected, would have its own product reference read against the
            //model's duty. That can report a unit adequate on a selection the model does not hold, and
            //suppress exactly the escalation an outgrown unit needs. The two halves of this comparison have
            //to come from the same object.
            AirHandlingUnit airHandlingUnit_Cluster = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x is not null && x.Guid == airHandlingUnit.Guid);

            if (airHandlingUnit_Cluster is null)
            {
                reason = string.Format(
                    "Air handling unit '{0}' is not in this model - no unit of that identity was found, though the model may well hold another unit of the same name. Its adequacy cannot be checked against a duty the model derives for a different object.",
                    airHandlingUnit.Name);

                return false;
            }

            airHandlingUnit = airHandlingUnit_Cluster;

            VentilationUnitReference ventilationUnitReference = SelectedVentilationUnitReference(airHandlingUnit);
            if (ventilationUnitReference is null)
            {
                reason = string.Format("Air handling unit '{0}' has no ventilation unit product selected, so there is no capacity to check the design duty against.", airHandlingUnit.Name);

                return false;
            }

            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = SelectedVentilationUnitCapacityDescriptor(airHandlingUnit, ventilationUnitCapacityDescriptors);
            if (ventilationUnitCapacityDescriptor is null)
            {
                reason = string.Format("Air handling unit '{0}' is selected as '{1}', which is not among the ventilation unit products offered, so its capacity is unknown.", airHandlingUnit.Name, ventilationUnitReference);

                return false;
            }

            //NO duty is not a duty of zero, and a product is not adequate for it.
            //
            //A unit whose systems or design terminals have been removed or disconnected keeps its product
            //selection, and its duty derives as 0/0 - which every non-negative capacity satisfies. Reporting
            //that unit adequate would say the plant is fine for a dwelling that currently moves no air, and
            //would suppress the reselection or reconstruction the model actually needs.
            //Modify.SelectVentilationUnit already refuses this case; adequacy has to agree with it.
            if (!AirHandlingUnitDesignDuty(adjacencyCluster, airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps))
            {
                reason = string.Format(
                    "Air handling unit '{0}' supplies no ventilation system carrying design terminals, so there is no design duty to check its selected product '{1}' against. Adequacy is unknown rather than met.",
                    airHandlingUnit.Name,
                    ventilationUnitReference);

                return false;
            }

            if (ventilationUnitCapacityDescriptor.IsSufficientFor(supplyDuty_Lps, extractDuty_Lps, tolerance_Lps))
            {
                return true;
            }

            List<string> shortfalls = [];

            if (ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps + tolerance_Lps < supplyDuty_Lps)
            {
                shortfalls.Add(string.Format("supply {0:0.###} l/s against a maximum of {1:0.###} l/s", supplyDuty_Lps, ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps));
            }

            if (ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps + tolerance_Lps < extractDuty_Lps)
            {
                shortfalls.Add(string.Format("extract {0:0.###} l/s against a maximum of {1:0.###} l/s", extractDuty_Lps, ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps));
            }

            reason = string.Format(
                "Air handling unit '{0}' is selected as '{1}', and the current design duty has grown past what it can move: {2}. That unit is exhausted - select the next compliant product from the current design duty. The Approved Document F requirement has not changed and is not what grew.",
                airHandlingUnit.Name,
                ventilationUnitReference,
                string.Join("; ", shortfalls));

            return false;
        }
    }
}
