// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Enums;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Designs one room as close to a <b>requested</b> design airflow as the dwelling and its already
    /// selected ventilation unit will actually carry, and hands back the model that produces.
    /// <para>
    /// Parameter reading and Grasshopper messaging only. Every decision - the search, the clamp, the
    /// Approved Document F floors, what stopped it and whether anything may be adopted at all - is
    /// <c>SAM.Analytical.Modify.ResolveTargetedDesignAirFlow</c>'s, so what this component does is what
    /// <c>SAM.Tests</c> exercises.
    /// </para>
    /// </summary>
    public class SAMAnalyticalResolveTargetedDesignAirFlow : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8b41c7a2-52d6-4f39-91be-6c0d84f7ae13");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private const string description = @"
Designs ONE room as close to a REQUESTED design airflow as the dwelling and its ALREADY SELECTED ventilation unit will actually carry - and hands back the model that produces, leaving the model you supplied untouched.

WHERE THIS SITS
SAMAnalytical.SystemVentilationUnitCatalogue (SAM_Systems)
  -> ventilationUnitCapacityDescriptors_
SAMAnalytical.PreparePartOIteration                                          [the prepared model]
  -> _analyticalModel
THIS COMPONENT                                                                [resolves one room's request, reports what bounded it]

THIS IS A CLAMP, NOT AN OPTIMISER
Ask for a room at 40 l/s. Either the dwelling can be designed that way, or it cannot - and the useful answer to the second case is not 'no', it is '36.4 l/s, and here is what stopped it there'. achievedAirFlow is always at or between the room's existing design airflow and requestedAirFlow, so the request is a CEILING on an increase and a FLOOR on a reduction. Headroom nobody asked for is reported and never spent: a 50 l/s unit serving a request for 30 l/s answers 30, not 50.

HOW THIS DIFFERS FROM SAMAnalytical.ApplyTargetedDesignAirFlow
That component is the MANUAL seam - an engineer states a value, it is applied, and the equipment check that follows can report Refused BESIDE an airflow change that has already committed. This one is the AUTOMATIC seam: nothing commits until a whole design has been found feasible, so a request the selected unit cannot carry comes back CLAMPED to one it can, rather than applied and then flagged. Use that one to state a number; use this one to ask how much is available.

READ accepted AND requestSatisfied SEPARATELY - THEY ARE NOT THE SAME QUESTION
accepted = false          Nothing was feasible. No design was found, analyticalModel comes back exactly as it went in, and refusals says why.
accepted = true,          A feasible design was found, but not the one asked for. analyticalModel carries the CLAMPED design,
requestSatisfied = false  achievedAirFlow is what it reaches, and limitingReason is the bound that stopped it there.
accepted = true,          The request was met exactly.
requestSatisfied = true

TARGETED vs DERIVED
_space is the ONE room anybody chose. targetedAdjustment is that room. derivedAdjustments are the rooms that moved ONLY because the balanced network required it - a consequence, never a second optimisation target. They are reported separately and always will be.

EQUIPMENT IS A BOUND, NEVER A PURCHASE
The selected product is the constraint being resolved WITHIN. This component never reselects and never grows it, whatever the catalogue offers - a bigger unit is selected deliberately, on its own, before resolving again. Leave ventilationUnitCapacityDescriptors_ unconnected and equipment is simply not a constraint on the search at all.

WHAT THIS DOES NOT DO
- It never writes an Approved Document F requirement - those bound the answer and are never moved by it.
- It never writes a runtime or profile airflow. Design airflow and operating airflow stay separate authorities, and the operating airflow follows later, from the established preparation path.
- It never modifies _analyticalModel. The resolved design is a NEW model on analyticalModel, and adopting it is taking that wire.
";

        public SAMAnalyticalResolveTargetedDesignAirFlow()
          : base("SAMAnalytical.ResolveTargetedDesignAirFlow", "SAMAnalytical.ResolveTargetedDesignAirFlow",
              description,
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel to resolve one room's design airflow within. The model you supply is NEVER modified - a NEW model carrying the resolved design is returned where a feasible one was found, and this very model, unchanged, where none was.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "The ONE room being targeted.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_flowClassification", NickName = "_flowClassification", Description = "Which side of that room is being resolved: Supply or Extract. Anything else REFUSES.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_designAirFlow", NickName = "_designAirFlow", Description = "The design airflow [l/s] being REQUESTED for that room, as a total across its terminals. The answer never moves the room past this value, in either direction - read achievedAirFlow for what it actually reaches.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "partFExtractAllocationStrategy_", NickName = "partFExtractAllocationStrategy_", Description = "How a derived extract change is shared out: MinimumFirstCookingPriority or VolumeWeighted.\n\nOptional: defaults to MinimumFirstCookingPriority, the same strategy Approved Document F sizing defaults to.", Access = GH_ParamAccess.item, Optional = true };
                @string.SetPersistentData(PartFExtractAllocationStrategy.MinimumFirstCookingPriority.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Flow rate tolerance [l/s] for every balance, Approved Document F and capacity comparison this call makes - and the margin the search itself is resolved to.\n\nOptional: defaults to 0.001.", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(0.001);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "ventilationUnitCapacityDescriptors_", NickName = "ventilationUnitCapacityDescriptors_", Description = "Selectable VentilationUnitCapacityDescriptor products, from SAMAnalyticalSystem.VentilationUnitCatalogue (SAM_Systems) or another source of the same type. Optional: leave unconnected and the selected unit's rating is not a constraint on the search - equipmentOutcome then reads NotApplicable.\n\nWhere connected, the rating of the product the serving air handling unit is ALREADY selected as bounds how far the request can be met. Nothing is ever reselected.", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "A NEW model carrying the resolved design - the targeted room and every derived one - or, where nothing was feasible, _analyticalModel itself, unchanged and not a copy of it. Taking this wire is the commit, so read accepted first. Feed it into SAMAnalytical.PreparePartOIteration to recalculate the transfer network before simulating.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "space", NickName = "space", Description = "The targeted room, as resolved in the returned model.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "requestedAirFlow l/s", NickName = "requestedAirFlow l/s", Description = "The design airflow that was asked for - _designAirFlow, restated beside what it actually reached.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "achievedAirFlow l/s", NickName = "achievedAirFlow l/s", Description = "The design airflow the targeted room actually reaches in the returned model. Equal to requestedAirFlow where requestSatisfied is true, between designAirFlowBefore and requestedAirFlow where it is false, and nothing at all where accepted is false.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "designAirFlowBefore l/s", NickName = "designAirFlowBefore l/s", Description = "The targeted room's design airflow before this call - the other end of the move achievedAirFlow reports.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "requestSatisfied", NickName = "requestSatisfied", Description = "Was the request met EXACTLY, within tolerance_? False is a normal answer and not a refusal: it means achievedAirFlow is the clamped value and limitingReason says what clamped it.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "changed", NickName = "changed", Description = "Would adopting this actually move the targeted room? False where the room cannot be moved towards the request at all - the answer is then the design as it already stands, which is valid and adoptable but changes nothing.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooObjectParam() { Name = "targetedAdjustment", NickName = "targetedAdjustment", Description = "The ONE room that was chosen, and what the answer moves it from and to. Nothing where accepted is false.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooObjectParam() { Name = "derivedAdjustments", NickName = "derivedAdjustments", Description = "Every room that moved ONLY as a consequence of keeping the dwelling balanced - never a room chosen for optimisation. Empty where the targeted change needed no balancing consequence.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supply duty before l/s", NickName = "supply duty before l/s", Description = "The dwelling's design supply duty on the model this was resolved against.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supply duty after l/s", NickName = "supply duty after l/s", Description = "The dwelling's design supply duty the answer produces.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "extract duty before l/s", NickName = "extract duty before l/s", Description = "The dwelling's design extract duty on the model this was resolved against.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "extract duty after l/s", NickName = "extract duty after l/s", Description = "The dwelling's design extract duty the answer produces.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "airHandlingUnit", NickName = "airHandlingUnit", Description = "The air handling unit the resolved dwelling is served from, or nothing where none resolved.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "ventilationUnitReference", NickName = "ventilationUnitReference", Description = "The product that unit is CURRENTLY selected as. Never changed by resolving an airflow - the same identity before and after, whatever the catalogue offers.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooObjectParam() { Name = "ventilationUnitCapacityDescriptor", NickName = "ventilationUnitCapacityDescriptor", Description = "What that selected product can move, where the catalogue offered describes it - the rating the answer was bounded BY, never a design airflow. Nothing where no catalogue was offered, or where nothing has ever been selected.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supply headroom l/s", NickName = "supply headroom l/s", Description = "What the selected product would have left on the supply side had this answer been adopted. REPORTED, NEVER SPENT - the request is the ceiling, not the rating. Negative on a candidate that exceeded it.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "extract headroom l/s", NickName = "extract headroom l/s", Description = "The same on the extract side. See supply headroom l/s.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "equipmentOutcome", NickName = "equipmentOutcome", Description = "What the selected unit did: NotApplicable (no catalogue offered, no unit resolved, or nothing has ever been selected on it), or Kept. Never Reselected - this component does not buy equipment.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "limitingReason", NickName = "limitingReason", Description = "What stopped the answer short of the request: the selected unit's rating, an Approved Document F floor on the balancing side, or whatever else the engineering refused. Nothing where the request was met exactly.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "notes", NickName = "notes", Description = "What the search found, and what adopting the answer would do.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "warnings", NickName = "warnings", Description = "Design headroom and similar - legal, and not a reason to reject anything.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "refusals", NickName = "refusals", Description = "Why NOTHING at all was feasible. Empty where an answer was found, INCLUDING where that answer fell short of the request - a clamped answer is not a refusal, and limitingReason carries that instead.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "accepted", NickName = "accepted", Description = "Was a feasible design found, and is analyticalModel therefore the resolved model rather than the unchanged _analyticalModel that went in? Read requestSatisfied separately for whether that design is the one asked for.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index_Accepted = Params.IndexOfOutputParam("accepted");
            if (index_Accepted != -1)
            {
                dataAccess.SetData(index_Accepted, false);
            }

            int index_RequestSatisfied = Params.IndexOfOutputParam("requestSatisfied");
            if (index_RequestSatisfied != -1)
            {
                dataAccess.SetData(index_RequestSatisfied, false);
            }

            int index_Changed = Params.IndexOfOutputParam("changed");
            if (index_Changed != -1)
            {
                dataAccess.SetData(index_Changed, false);
            }

            int index;

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space = null;
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || !dataAccess.GetData(index, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_flowClassification");
            string text_FlowClassification = null;
            if (index == -1 || !dataAccess.GetData(index, ref text_FlowClassification) || string.IsNullOrWhiteSpace(text_FlowClassification))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Core.Query.TryGetEnum(text_FlowClassification, out FlowClassification flowClassification))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("'{0}' is not Supply or Extract.", text_FlowClassification));
                return;
            }

            index = Params.IndexOfInputParam("_designAirFlow");
            double designAirFlow_Lps = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref designAirFlow_Lps))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority;
            index = Params.IndexOfInputParam("partFExtractAllocationStrategy_");
            if (index != -1)
            {
                string text_Strategy = null;
                if (dataAccess.GetData(index, ref text_Strategy) && !string.IsNullOrWhiteSpace(text_Strategy))
                {
                    if (!Core.Query.TryGetEnum(text_Strategy, out partFExtractAllocationStrategy))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("'{0}' is not a Part F extract allocation strategy.", text_Strategy));
                        return;
                    }
                }
            }

            double tolerance_Lps = 0.001;
            index = Params.IndexOfInputParam("tolerance_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance_Lps);
            }

            //Unconnected means the selected unit's rating is not a constraint on the search - the same
            //backward-compatible meaning it has for SAMAnalytical.ApplyTargetedDesignAirFlow. A
            //connected-but-empty wire is treated the same way rather than as a catalogue that offers
            //nothing, since nothing here can tell "wired to an empty list" apart from "the upstream
            //component produced nothing yet".
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null;
            index = Params.IndexOfInputParam("ventilationUnitCapacityDescriptors_");
            if (index != -1)
            {
                List<GH_ObjectWrapper> objectWrappers = [];
                if (dataAccess.GetDataList(index, objectWrappers) && objectWrappers != null && objectWrappers.Count != 0)
                {
                    ventilationUnitCapacityDescriptors = [];
                    foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                    {
                        object @object = objectWrapper?.Value;

                        //A descriptor arrives wrapped as a GooObject (it is not an IJSAMObject, so it
                        //cannot ride GooAnalyticalObject/GooSAMObject) - unwrap once more before testing
                        //the underlying type. See SAMAnalyticalApplyTargetedDesignAirFlow's own fix for
                        //the same defect.
                        if (@object is IGH_Goo)
                        {
                            @object = (@object as dynamic).Value;
                        }

                        if (@object is VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor)
                        {
                            ventilationUnitCapacityDescriptors.Add(ventilationUnitCapacityDescriptor);
                        }
                    }
                }
            }

            //THE transaction boundary. AnalyticalModel.AdjacencyCluster hands out a copy, and the resolver
            //evaluates every candidate on copies of its own, so nothing the caller holds can be reached
            //from here at all - and the manual seam is never called on the authoritative model. Every
            //decision below this line is the library's, so what this component does and what SAM.Tests
            //exercises cannot drift apart.
            DwellingDesignAirFlowResolution resolution = analyticalModel.AdjacencyCluster.ResolveTargetedDesignAirFlow(space, flowClassification, designAirFlow_Lps, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);

            foreach (string warning in resolution.Warnings)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }

            if (!resolution.IsAccepted)
            {
                foreach (string refusal in resolution.Refusals)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, refusal);
                }
            }
            else if (!resolution.IsRequestSatisfied)
            {
                //A clamp is a normal answer and never an error - but it is emphatically not the number
                //that was asked for, and a canvas showing only the model wire has to be told so.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Requested {0:0.###} l/s and resolved to {1:0.###} l/s. {2}", resolution.Requested_Lps, resolution.Achieved_Lps, resolution.LimitingReason));
            }

            //Accepted hands back the model the answer produces; refused hands back the one that went in,
            //unchanged. There is deliberately no third case - nothing partially applied can come out of
            //here, because nothing was ever applied to anything the caller holds.
            AnalyticalModel analyticalModel_Resolved = resolution.IsAccepted ? new AnalyticalModel(analyticalModel, resolution.AdjacencyCluster) : analyticalModel;

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel_Resolved));
            }

            Space space_Resolved = (analyticalModel_Resolved.GetSpaces() ?? []).Find(x => x != null && x.Guid == space.Guid) ?? space;

            index = Params.IndexOfOutputParam("space");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSpace(space_Resolved));
            }

            index = Params.IndexOfOutputParam("requestedAirFlow l/s");
            if (index != -1 && !double.IsNaN(resolution.Requested_Lps))
            {
                dataAccess.SetData(index, resolution.Requested_Lps);
            }

            index = Params.IndexOfOutputParam("achievedAirFlow l/s");
            if (index != -1 && !double.IsNaN(resolution.Achieved_Lps))
            {
                dataAccess.SetData(index, resolution.Achieved_Lps);
            }

            if (resolution.TargetedAdjustment != null)
            {
                index = Params.IndexOfOutputParam("designAirFlowBefore l/s");
                if (index != -1)
                {
                    dataAccess.SetData(index, resolution.TargetedAdjustment.Before_Lps);
                }

                index = Params.IndexOfOutputParam("targetedAdjustment");
                if (index != -1)
                {
                    dataAccess.SetData(index, new GooObject(resolution.TargetedAdjustment));
                }
            }

            index = Params.IndexOfOutputParam("derivedAdjustments");
            if (index != -1)
            {
                dataAccess.SetDataList(index, resolution.DerivedAdjustments.ConvertAll(x => new GooObject(x)));
            }

            index = Params.IndexOfOutputParam("supply duty before l/s");
            if (index != -1 && !double.IsNaN(resolution.SupplyDuty_Before_Lps))
            {
                dataAccess.SetData(index, resolution.SupplyDuty_Before_Lps);
            }

            index = Params.IndexOfOutputParam("supply duty after l/s");
            if (index != -1 && !double.IsNaN(resolution.SupplyDuty_After_Lps))
            {
                dataAccess.SetData(index, resolution.SupplyDuty_After_Lps);
            }

            index = Params.IndexOfOutputParam("extract duty before l/s");
            if (index != -1 && !double.IsNaN(resolution.ExtractDuty_Before_Lps))
            {
                dataAccess.SetData(index, resolution.ExtractDuty_Before_Lps);
            }

            index = Params.IndexOfOutputParam("extract duty after l/s");
            if (index != -1 && !double.IsNaN(resolution.ExtractDuty_After_Lps))
            {
                dataAccess.SetData(index, resolution.ExtractDuty_After_Lps);
            }

            index = Params.IndexOfOutputParam("airHandlingUnit");
            if (index != -1 && resolution.AirHandlingUnit != null)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(resolution.AirHandlingUnit));
            }

            index = Params.IndexOfOutputParam("ventilationUnitReference");
            if (index != -1 && resolution.VentilationUnitReference != null)
            {
                dataAccess.SetData(index, new GooSAMObject(resolution.VentilationUnitReference));
            }

            index = Params.IndexOfOutputParam("ventilationUnitCapacityDescriptor");
            if (index != -1 && resolution.VentilationUnitCapacityDescriptor != null)
            {
                dataAccess.SetData(index, new GooObject(resolution.VentilationUnitCapacityDescriptor));
            }

            index = Params.IndexOfOutputParam("supply headroom l/s");
            if (index != -1 && !double.IsNaN(resolution.SupplyHeadroom_Lps))
            {
                dataAccess.SetData(index, resolution.SupplyHeadroom_Lps);
            }

            index = Params.IndexOfOutputParam("extract headroom l/s");
            if (index != -1 && !double.IsNaN(resolution.ExtractHeadroom_Lps))
            {
                dataAccess.SetData(index, resolution.ExtractHeadroom_Lps);
            }

            index = Params.IndexOfOutputParam("equipmentOutcome");
            if (index != -1)
            {
                dataAccess.SetData(index, (resolution.Candidate?.VentilationUnitSelectionOutcome ?? VentilationUnitSelectionOutcome.NotApplicable).ToString());
            }

            index = Params.IndexOfOutputParam("limitingReason");
            if (index != -1 && resolution.LimitingReason != null)
            {
                dataAccess.SetData(index, resolution.LimitingReason);
            }

            index = Params.IndexOfOutputParam("notes");
            if (index != -1)
            {
                dataAccess.SetDataList(index, resolution.Notes);
            }

            index = Params.IndexOfOutputParam("warnings");
            if (index != -1)
            {
                dataAccess.SetDataList(index, resolution.Warnings);
            }

            index = Params.IndexOfOutputParam("refusals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, resolution.Refusals);
            }

            if (index_RequestSatisfied != -1)
            {
                dataAccess.SetData(index_RequestSatisfied, resolution.IsRequestSatisfied);
            }

            if (index_Changed != -1)
            {
                dataAccess.SetData(index_Changed, resolution.IsChanged);
            }

            if (index_Accepted != -1)
            {
                dataAccess.SetData(index_Accepted, resolution.IsAccepted);
            }
        }
    }
}
