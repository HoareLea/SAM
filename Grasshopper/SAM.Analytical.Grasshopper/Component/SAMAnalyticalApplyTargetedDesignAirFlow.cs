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
    /// Sets one room's design airflow and rebalances the dwelling around it, then - where a catalogue is
    /// offered - validates the serving air handling unit's selection against the recalculated duty.
    /// <para>
    /// Parameter reading and Grasshopper messaging only. Every decision - the rebalance, the Part F floor,
    /// the all-or-nothing write, whether the selected unit is kept, reselected or refused - is
    /// <c>SAM.Analytical.Modify.ApplyTargetedDesignAirFlow</c>'s, so what this component does is what
    /// <c>SAM.Tests</c> exercises.
    /// </para>
    /// </summary>
    public class SAMAnalyticalApplyTargetedDesignAirFlow : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2f6a9d3e-7c14-4e82-b6a5-9d1f3c8e5a47");

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
Sets ONE room's design airflow and rebalances the dwelling around it, as a single transaction - then, where ventilationUnitCapacityDescriptors_ is connected, validates the serving air handling unit's selection against the recalculated duty.

WHERE THIS SITS
SAMAnalytical.SystemVentilationUnitCatalogue (SAM_Systems)
  -> ventilationUnitCapacityDescriptors_
SAMAnalytical.PreparePartOIteration                                          [the prepared model]
  -> _analyticalModel
THIS COMPONENT                                                                [targets one room, rebalances, validates equipment]

TARGETED vs DERIVED
_space is the ONE room anybody chose. Every other room that moves does so only because the balanced network requires it - a derived consequence, never a second optimisation target. designAirFlowBefore/After and derivedAdjustments report the two kinds separately and always will: reporting them together would suggest a room was chosen for optimisation when it was not.

ALL OR NOTHING
The whole plan - the targeted room and every derived one - is checked against every Approved Document F floor before a single terminal is written. A dwelling that cannot be balanced, or a request below the Approved Document F requirement, refuses with NOTHING changed - analyticalModel comes back exactly as it went in, and successful is false.

EQUIPMENT IS VALIDATED, NEVER SELECTED, UNLESS A CATALOGUE IS CONNECTED
Leave ventilationUnitCapacityDescriptors_ unconnected to use this component exactly as if equipment validation did not exist: no unit is resolved, checked or reselected, and equipmentOutcome reads NotApplicable.
Connect it and, once the airflow change above has already committed: the serving unit's current selection is checked against the recalculated duty (Kept where it still suffices), the smallest capable product from the catalogue is selected in its place where it does not (Reselected), or - where none is capable - nothing is written to the unit and equipmentOutcome reads Refused.

THE AIRFLOW CHANGE IS NEVER ROLLED BACK BECAUSE EQUIPMENT COULD NOT BE VALIDATED
A design airflow change and an equipment adequacy check are separate engineering questions. successful describes the airflow change alone; equipmentOutcome sits BESIDE it, never instead of it. A Refused equipment outcome can appear next to successful = true - that is not a contradiction, it is the point: the dwelling's design is settled, and the plant it needs has not yet been specified.

WHAT THIS DOES NOT DO
- It never selects equipment on its own initiative - reselection only ever runs to keep an ALREADY-selected unit adequate for a design that grew, from the catalogue this call was actually given.
- It never invents a maximum airflow for a selected product - the catalogue supplies capacities; this reads them, never guesses them.
- It writes no Approved Document F requirement, and no runtime or profile airflow.
";

        public SAMAnalyticalApplyTargetedDesignAirFlow()
          : base("SAMAnalytical.ApplyTargetedDesignAirFlow", "SAMAnalytical.ApplyTargetedDesignAirFlow",
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel to change one room's design airflow on. The model you supply is not modified; an updated copy is returned - or, on a refusal, an unchanged copy.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "The ONE room being targeted.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_flowClassification", NickName = "_flowClassification", Description = "Which side of that room is being set: Supply or Extract. Anything else REFUSES.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_designAirFlow", NickName = "_designAirFlow", Description = "The room's new design airflow [l/s], as a total across its terminals. Refused where it is below the room's Approved Document F requirement.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "partFExtractAllocationStrategy_", NickName = "partFExtractAllocationStrategy_", Description = "How a derived extract change is shared out: MinimumFirstCookingPriority or VolumeWeighted.\n\nOptional: defaults to MinimumFirstCookingPriority, the same strategy Approved Document F sizing defaults to.", Access = GH_ParamAccess.item, Optional = true };
                @string.SetPersistentData(PartFExtractAllocationStrategy.MinimumFirstCookingPriority.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Flow rate tolerance [l/s] for every balance and sufficiency comparison this call makes.\n\nOptional: defaults to 0.001.", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(0.001);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "ventilationUnitCapacityDescriptors_", NickName = "ventilationUnitCapacityDescriptors_", Description = "Selectable VentilationUnitCapacityDescriptor products, from SAMAnalyticalSystem.VentilationUnitCatalogue (SAM_Systems) or another source of the same type. Optional: leave unconnected to change the design airflow exactly as if equipment validation did not exist - no unit is resolved, checked or reselected, and equipmentOutcome reads NotApplicable.\n\nWhere connected, the serving air handling unit's CURRENT selection is checked against the recalculated duty AFTER the airflow change has already committed, and is never the reason the airflow change itself is refused.", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "Updated copy with the targeted and derived design airflows applied - or, on a refusal, an unchanged copy. Feed this back into SAMAnalytical.PreparePartOIteration to recalculate the transfer network before simulating.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "space", NickName = "space", Description = "The targeted room, as resolved in the updated model.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "designAirFlowBefore l/s", NickName = "designAirFlowBefore l/s", Description = "The targeted room's design airflow before this call.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "designAirFlowAfter l/s", NickName = "designAirFlowAfter l/s", Description = "The targeted room's design airflow after this call. Equal to _designAirFlow on success.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooObjectParam() { Name = "derivedAdjustments", NickName = "derivedAdjustments", Description = "Every room that moved ONLY as a consequence of keeping the dwelling balanced - never a room chosen for optimisation. Empty where the targeted change needed no balancing consequence.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supply duty l/s", NickName = "supply duty l/s", Description = "The dwelling's design supply duty after this call.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "extract duty l/s", NickName = "extract duty l/s", Description = "The dwelling's design extract duty after this call.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "airHandlingUnit", NickName = "airHandlingUnit", Description = "The air handling unit this transaction's system supplies from, or nothing where none resolved.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "ventilationUnitReference", NickName = "ventilationUnitReference", Description = "The product identity airHandlingUnit is selected as after this call - unchanged on Kept or Refused, the newly chosen product on Reselected, nothing where none has ever been selected or no unit resolved.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "equipmentOutcome", NickName = "equipmentOutcome", Description = "What happened to the serving unit's selection: NotApplicable (no catalogue offered, no unit resolved, or nothing has ever been selected on it), Kept, Reselected, or Refused. Never affects successful - see the component description.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "equipmentReason", NickName = "equipmentReason", Description = "Why no product in the catalogue offered is capable, where equipmentOutcome is Refused. Nothing otherwise - a Kept or Reselected unit needs no explanation beyond notes.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "notes", NickName = "notes", Description = "What was applied, what it displaced, and what happened to the equipment.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "refusals", NickName = "refusals", Description = "Why nothing was written. Empty on a successful airflow change - equipment being Refused is never reported here.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Was the design airflow change applied? Describes the airflow change alone - read equipmentOutcome separately for what happened to the plant.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
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

            //Unconnected means exactly what it did before equipment validation existed: no catalogue, no
            //unit resolved, no reselection. A connected-but-empty wire is treated the same way rather than
            //as a catalogue that offers nothing, since nothing here can tell "wired to an empty list" apart
            //from "the upstream component produced nothing yet" - the same convention
            //SAMAnalytical.PreparePartOIteration's ventilationUnitCapacityDescriptors_ already uses.
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
                        //the underlying type. See SAMAnalyticalPreparePartOIteration's own fix for the
                        //same defect.
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

            //Every decision below this line is the library's, so what this component does and what
            //SAM.Tests exercises cannot drift apart.
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            DwellingDesignAirFlowChange change = adjacencyCluster.ApplyTargetedDesignAirFlow(space, flowClassification, designAirFlow_Lps, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);

            foreach (string warning in change.Warnings)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }

            if (!change.Successful)
            {
                foreach (string refusal in change.Refusals)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, refusal);
                }
            }

            AnalyticalModel analyticalModel_Updated = new(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel_Updated));
            }

            Space space_Updated = (analyticalModel_Updated.GetSpaces() ?? []).Find(x => x != null && x.Guid == space.Guid) ?? space;

            index = Params.IndexOfOutputParam("space");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSpace(space_Updated));
            }

            if (change.TargetedAdjustment != null)
            {
                index = Params.IndexOfOutputParam("designAirFlowBefore l/s");
                if (index != -1)
                {
                    dataAccess.SetData(index, change.TargetedAdjustment.Before_Lps);
                }

                index = Params.IndexOfOutputParam("designAirFlowAfter l/s");
                if (index != -1)
                {
                    dataAccess.SetData(index, change.TargetedAdjustment.After_Lps);
                }
            }

            index = Params.IndexOfOutputParam("derivedAdjustments");
            if (index != -1)
            {
                dataAccess.SetDataList(index, change.DerivedAdjustments.ConvertAll(x => new GooObject(x)));
            }

            index = Params.IndexOfOutputParam("supply duty l/s");
            if (index != -1 && !double.IsNaN(change.SupplyDuty_Lps))
            {
                dataAccess.SetData(index, change.SupplyDuty_Lps);
            }

            index = Params.IndexOfOutputParam("extract duty l/s");
            if (index != -1 && !double.IsNaN(change.ExtractDuty_Lps))
            {
                dataAccess.SetData(index, change.ExtractDuty_Lps);
            }

            index = Params.IndexOfOutputParam("airHandlingUnit");
            if (index != -1 && change.AirHandlingUnit != null)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(change.AirHandlingUnit));
            }

            index = Params.IndexOfOutputParam("ventilationUnitReference");
            if (index != -1 && change.VentilationUnitReference != null)
            {
                dataAccess.SetData(index, new GooSAMObject(change.VentilationUnitReference));
            }

            index = Params.IndexOfOutputParam("equipmentOutcome");
            if (index != -1)
            {
                dataAccess.SetData(index, change.VentilationUnitSelectionOutcome.ToString());
            }

            index = Params.IndexOfOutputParam("equipmentReason");
            if (index != -1 && change.VentilationUnitSelectionReason != null)
            {
                dataAccess.SetData(index, change.VentilationUnitSelectionReason);
            }

            index = Params.IndexOfOutputParam("notes");
            if (index != -1)
            {
                dataAccess.SetDataList(index, change.Notes);
            }

            index = Params.IndexOfOutputParam("refusals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, change.Refusals);
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, change.Successful);
            }
        }
    }
}
