// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One design ventilation terminal: a supply or extract terminal serving a space, with the airflow
    /// the design gives it.
    /// <para>
    /// <b>This is the design realization, not the regulatory requirement.</b>
    /// <see cref="PartFVentilationTerminalRequirement"/> says what Approved Document F requires of a
    /// room; this says what the design has put in the room to satisfy it. They are different statements
    /// and they are held apart deliberately: the requirement is recalculated from the Approved Document
    /// and rebuilt from scratch on every Part F run, while a terminal is authored, moved, subdivided and
    /// eventually given a product, and must keep its identity through all of that.
    /// </para>
    /// <para>
    /// <b>Zero, one or many per space per direction.</b> Nothing here or in
    /// <see cref="AdjacencyCluster"/> imposes one terminal per space. A requirement for 20 l/s of supply
    /// may be realized as one 20 l/s terminal, or two of 10, or four of 5, and the requirement, the
    /// space, the system and any Approved Document O scenario over them are unchanged by that choice -
    /// the space's design duty is the sum of its terminals, never the count of them. A space may hold
    /// supply and extract terminals at once.
    /// </para>
    /// <para>
    /// <b>Related, not referenced.</b> The space a terminal serves and the system it belongs to are
    /// <see cref="AdjacencyCluster"/> relations, which is how every other ownership in this model is
    /// expressed. There is deliberately no <c>SpaceGuid</c> property: a stored guid beside a relation is
    /// a second answer to the same question, and the two can disagree.
    /// </para>
    /// <para>
    /// <b>Identity does not depend on placement.</b> <see cref="Location"/> is optional and absent is a
    /// legal state - Iteration 1a establishes duties long before anyone has laid out a diffuser.
    /// <see cref="Move(Vector3D)"/> and <see cref="Transform(Transform3D)"/> change where the terminal
    /// is and never what it is, following <see cref="Space"/>, which solved exactly this.
    /// </para>
    /// <para>
    /// <b>Not an equipment section.</b> <see cref="IAnalyticalEquipment"/> is implemented so the terminal
    /// is valid in an <see cref="AdjacencyCluster"/>. <c>ISimpleEquipment</c> and <c>ISection</c> are
    /// deliberately <i>not</i>: those are the members of an <see cref="AirHandlingUnit"/>'s internal
    /// supply and extract chain - a filter, a coil, a fan - and a room terminal is not one of them.
    /// </para>
    /// </summary>
    public class VentilationTerminal : SAMObject, IAnalyticalEquipment
    {
        private Point3D location;
        private FlowClassification flowClassification = FlowClassification.Undefined;
        private double? designFlowRate_Lps;

        public VentilationTerminal(string name, FlowClassification flowClassification, double? designFlowRate_Lps)
            : base(name)
        {
            this.flowClassification = flowClassification;
            this.designFlowRate_Lps = designFlowRate_Lps;
        }

        public VentilationTerminal(string name, FlowClassification flowClassification, double? designFlowRate_Lps, Point3D location)
            : base(name)
        {
            this.flowClassification = flowClassification;
            this.designFlowRate_Lps = designFlowRate_Lps;

            if (location is not null)
            {
                this.location = new Point3D(location);
            }
        }

        public VentilationTerminal(Guid guid, string name, FlowClassification flowClassification, double? designFlowRate_Lps)
            : base(guid, name)
        {
            this.flowClassification = flowClassification;
            this.designFlowRate_Lps = designFlowRate_Lps;
        }

        public VentilationTerminal(VentilationTerminal ventilationTerminal)
            : base(ventilationTerminal)
        {
            if (ventilationTerminal is not null)
            {
                flowClassification = ventilationTerminal.flowClassification;
                designFlowRate_Lps = ventilationTerminal.designFlowRate_Lps;
                location = ventilationTerminal.location is null ? null : new Point3D(ventilationTerminal.location);
            }
        }

        public VentilationTerminal(Guid guid, VentilationTerminal ventilationTerminal)
            : base(guid, ventilationTerminal)
        {
            if (ventilationTerminal is not null)
            {
                flowClassification = ventilationTerminal.flowClassification;
                designFlowRate_Lps = ventilationTerminal.designFlowRate_Lps;
                location = ventilationTerminal.location is null ? null : new Point3D(ventilationTerminal.location);
            }
        }

        public VentilationTerminal(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Whether the terminal supplies air to its space or extracts air from it.
        /// <para>
        /// Two values, on purpose. Approved Document F's separation of local kitchen extract from general
        /// wet room extract is a regulatory distinction, not a flow one - both are physically extract
        /// terminals - so it stays on the requirement and in
        /// <see cref="PartFTerminalReference.TerminalRole"/> rather than being pushed into a generic
        /// classification that other MEP work reads.
        /// </para>
        /// </summary>
        public FlowClassification FlowClassification
        {
            get
            {
                return flowClassification;
            }
        }

        /// <summary>
        /// The airflow [l/s] the design gives this terminal at the continuous design condition.
        /// <para>
        /// Litres per second, matching the requirement it realizes, so that a design duty and a Part F
        /// rate can be compared without a conversion in between. The conversion to m3/s happens once, at
        /// the runtime boundary, exactly where it already happens for the applied airflows.
        /// </para>
        /// <para>
        /// Settable, because subdividing or re-balancing terminals is design work. Null means the duty
        /// has not been established, which is not the same as zero.
        /// </para>
        /// </summary>
        public double? DesignFlowRate_Lps
        {
            get
            {
                return designFlowRate_Lps;
            }

            set
            {
                designFlowRate_Lps = value;
            }
        }

        /// <summary>
        /// Where the terminal is, or null where nobody has said. Absent is a legal state and it is the
        /// normal one until detailed design.
        /// </summary>
        public Point3D Location
        {
            get
            {
                return location is null ? null : new Point3D(location);
            }

            set
            {
                location = value is null ? null : new Point3D(value);
            }
        }

        /// <summary>Whether this terminal has been given a usable position.</summary>
        public bool IsPlaced()
        {
            return location is not null && location.IsValid();
        }

        /// <summary>Moves the terminal. Its identity is untouched - a terminal that moves is the same terminal.</summary>
        public void Move(Vector3D vector3D)
        {
            location = location?.GetMoved(vector3D) as Point3D;
        }

        /// <summary>Transforms the terminal's position. Its identity is untouched.</summary>
        public void Transform(Transform3D transform3D)
        {
            if (location is not null)
            {
                location = location.Transform(transform3D);
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("FlowClassification"))
            {
                flowClassification = Core.Query.Enum<FlowClassification>(jsonObject["FlowClassification"]?.GetValue<string>());
            }

            //Read as nullable: an absent duty and a zero duty are different answers, and a terminal
            //sized to nothing is a real state a schedule has to be able to report.
            designFlowRate_Lps = null;
            if (jsonObject.ContainsKey("DesignFlowRate_Lps"))
            {
                designFlowRate_Lps = jsonObject["DesignFlowRate_Lps"]?.GetValue<double>();
            }

            location = null;
            if (jsonObject["Location"] is JsonObject jsonObject_Location)
            {
                location = new Point3D((JsonObject)jsonObject_Location.DeepClone());
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            result["FlowClassification"] = flowClassification.ToString();

            if (designFlowRate_Lps.HasValue && !double.IsNaN(designFlowRate_Lps.Value))
            {
                result["DesignFlowRate_Lps"] = designFlowRate_Lps.Value;
            }

            if (location?.ToJsonObject() is JsonObject jsonObject_Location)
            {
                result["Location"] = jsonObject_Location.DeepClone();
            }

            return result;
        }
    }
}
