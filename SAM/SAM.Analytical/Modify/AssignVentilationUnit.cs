// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// States that an air handling unit <b>is</b> a named ventilation unit product, because an engineer
        /// said so. The manual counterpart of <see cref="SelectVentilationUnit(AdjacencyCluster, AirHandlingUnit, IEnumerable{VentilationUnitCapacityDescriptor}, out List{string}, out List{string}, double)"/>.
        ///
        /// <para><b>An authority change, not an engineering calculation</b></para>
        /// <para>
        /// This writes one identity and nothing else. It runs <b>no selection rule</b>, reads no catalogue,
        /// compares no capacity, and touches no airflow of any kind - not the Approved Document F
        /// requirement, not the design airflow, not the design transfer airflow, and not any runtime
        /// airflow. Those four are different quantities from the capability of the box, and assigning a box
        /// is not a statement about any of them. See <see cref="VentilationUnitReference"/>.
        /// </para>
        ///
        /// <para><b>An insufficient product is assigned, not refused</b></para>
        /// <para>
        /// Deliberately. An engineer who assigns a 150 l/s unit to a 175 l/s dwelling has authored a design
        /// that does not work, and the honest answer is to hold that assignment and report it as
        /// insufficient - which <see cref="Query.IsVentilationUnitSufficient"/> does, and which the Part O
        /// preparation surfaces beside a suggestion. Refusing the write, silently substituting a bigger
        /// product, or reducing the design airflow to fit would each replace the engineer's decision with
        /// this method's. A deliberately <i>oversized</i> capable product is equally valid and equally
        /// untouched: nothing here prefers the smallest.
        /// </para>
        ///
        /// <para><b>Capacity is still never stored</b></para>
        /// <para>
        /// Only <paramref name="ventilationUnitReference"/> - the identity - is written, exactly as the
        /// automatic path writes it, so a manually assigned unit and an automatically selected one are
        /// indistinguishable to everything downstream. Iteration 2B, the capacity envelope and every report
        /// look the capability up from a catalogue handed in at the time. That is what lets a manual
        /// assignment act as a ceiling in 2B without 2B needing to know a human chose it.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The cluster that owns the unit.</param>
        /// <param name="airHandlingUnit">
        /// The unit to assign to. Resolved out of <paramref name="adjacencyCluster"/> by guid rather than
        /// used as handed in - <see cref="SelectVentilationUnit(AdjacencyCluster, AirHandlingUnit, IEnumerable{VentilationUnitCapacityDescriptor}, out List{string}, out List{string}, double)"/>
        /// says why a detached same-named unit must not be written through.
        /// </param>
        /// <param name="ventilationUnitReference">
        /// The product's identity. Expected to be a catalogue product's identity, though nothing here
        /// requires the catalogue to be readable: an identity is durable and a catalogue is not always to
        /// hand. An identity the current catalogue does not hold is reported by
        /// <see cref="Query.IsVentilationUnitSufficient"/> as an unknown capacity rather than as a pass.
        /// </param>
        /// <param name="notes">What was written, in the same voice as the automatic path's notes.</param>
        /// <param name="refusals">Why nothing was written, where nothing was.</param>
        /// <returns>True where the identity was written.</returns>
        public static bool AssignVentilationUnit(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, VentilationUnitReference ventilationUnitReference, out List<string> notes, out List<string> refusals)
        {
            notes = [];
            refusals = [];

            if (adjacencyCluster is null || airHandlingUnit is null)
            {
                refusals.Add("No air handling unit was supplied, so no ventilation unit product could be assigned.");

                return false;
            }

            //A product with no identity is not an assignment, and writing it would leave the unit claiming
            //to be a product nothing can resolve. Note that this is NOT how a dwelling comes to have no
            //product: that is simply a unit nothing has been assigned to yet, which the preparation reports
            //as such rather than inventing an assignment for.
            if (ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                refusals.Add(string.Format(
                    "Air handling unit '{0}' was not assigned a ventilation unit product, because the product offered states neither a manufacturer nor a model and so identifies nothing. The unit's existing selection is unchanged.",
                    airHandlingUnit.Name));

                return false;
            }

            AirHandlingUnit airHandlingUnit_Model = (adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x is not null && x.Guid == airHandlingUnit.Guid);

            if (airHandlingUnit_Model is null)
            {
                refusals.Add(string.Format(
                    "Air handling unit '{0}' is not in this model - no unit of that identity was found, though the model may well hold another unit of the same name. Assigning a product onto an object the model does not contain would leave the assignment where nothing can resolve it. Nothing was changed: take the unit from the model and assign onto that one.",
                    airHandlingUnit.Name));

                return false;
            }

            VentilationUnitReference ventilationUnitReference_Previous = airHandlingUnit_Model.SelectedVentilationUnitReference();

            //Written onto a replacement and re-added, not mutated in place - the same discipline as
            //Modify.SelectVentilationUnit, and for the same reason: the cluster's object graph is what
            //everything downstream reads, and a caller's copy is not part of it.
            AirHandlingUnit airHandlingUnit_Assigned = new AirHandlingUnit(airHandlingUnit_Model);

            airHandlingUnit_Assigned.SetValue(AirHandlingUnitParameter.VentilationUnitReference, new VentilationUnitReference(ventilationUnitReference));

            adjacencyCluster.AddObject(airHandlingUnit_Assigned);

            notes.Add(ventilationUnitReference_Previous is null
                ? string.Format(
                    "Air handling unit '{0}' was assigned as '{1}' by explicit selection. No airflow changed: the product's maximum is its capability ceiling, and the dwelling's design duty and Approved Document F requirement are unchanged.",
                    airHandlingUnit_Assigned.Name,
                    ventilationUnitReference)
                : string.Format(
                    "Air handling unit '{0}' was reassigned from '{1}' to '{2}' by explicit selection. No airflow changed: the product's maximum is its capability ceiling, and the dwelling's design duty and Approved Document F requirement are unchanged.",
                    airHandlingUnit_Assigned.Name,
                    ventilationUnitReference_Previous,
                    ventilationUnitReference));

            return true;
        }
    }
}
