// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Enums;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Builds the Approved Document O <c>OverheatingScenario</c> set for one mitigation stage.
    /// <para>
    /// <b>This is the component that was missing.</b> Three components already accept
    /// <c>overheatingScenarios_</c> - <c>Tas.TSDQueryTM59Results</c>, <c>Tas.TPDQueryTM59Results</c> and
    /// <c>SAMAnalytical.CreateTBDByTM59</c> - but nothing produced one, so the scenario-authoritative path could
    /// not be driven from Grasshopper at all. This closes that, which is what makes an iteration testable by
    /// hand rather than only from a test fixture.
    /// </para>
    /// </summary>
    public class SAMAnalyticalCreateOverheatingScenarios : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3c1a7f52-9d4e-4b83-8a16-6e2f5c0d7b41");

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
Creates Approved Document O overheating scenarios - one per zone - for a single mitigation stage (iteration).

What a scenario is
- The statement of an assessment: WHICH zone, at WHAT mitigation stage, with WHICH ventilation strategy, under WHAT operating assumptions.
- Its key is DERIVED from those, so the same statement always identifies the same assessment and results can be attributed to it.
- Feed the output into Tas.TSDQueryTM59Results (or Tas.TPDQueryTM59Results / SAMAnalytical.CreateTBDByTM59).

Iterations
- BasePassive        - openings unrestricted, mechanical ventilation at its design rate. The base provision.
- AcousticRestricted - openings restricted for noise, boost and summer bypass available.
- ActiveTrimCooling  - NOT AVAILABLE YET. Refused rather than guessed at.

Scope is decided for you
- Dwelling vs CommonSpace comes from the same rule the Part F calculation uses, not from the zone's name, so a communal corridor is assessed on the corridor criterion instead of being attributed to a flat.
- A zone carrying no dwelling marking is refused, not guessed.

Ventilation strategy is REQUIRED
- NV, MV, MVRE or UV. Supply one value to apply to every zone, or one per zone in order.
- Where scenarios are supplied, the strategy they state is AUTHORITATIVE over the model's own data.
- A zone with no strategy is refused rather than defaulted - a silent default assessed a mechanically ventilated dwelling against the natural-ventilation criterion.

IMPORTANT - what this does not do
- It states intent. It does NOT change the simulation inputs: nothing here makes a model's openings actually operate without restriction, or set a ventilation rate in the TBD. Model the stage you are stating.
";

        public SAMAnalyticalCreateOverheatingScenarios()
          : base("SAMAnalytical.CreateOverheatingScenarios", "SAMAnalytical.CreateOverheatingScenarios",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "zones_", NickName = "zones_", Description = "SAM Analytical Zones to assess. Leave unconnected to assess every zone in the model.", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_partOIteration", NickName = "_partOIteration", Description = "Part O mitigation stage: BasePassive, AcousticRestricted", Access = GH_ParamAccess.item };
                @string.SetPersistentData(PartOIteration.BasePassive.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_ventilationStrategies", NickName = "_ventilationStrategies", Description = "Ventilation strategy per zone: NV, MV, MVRE or UV. One value applies to every zone.", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "overheatingScenarios", NickName = "overheatingScenarios", Description = "SAM Part O Overheating Scenarios", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "refusals", NickName = "refusals", Description = "Zones that produced no scenario, and why", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Were scenarios created for every zone with nothing refused?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Zone> zones_Model = null;
            if (analyticalObject is AnalyticalModel analyticalModel)
            {
                zones_Model = analyticalModel.GetZones();
            }
            else if (analyticalObject is AdjacencyCluster adjacencyCluster)
            {
                zones_Model = adjacencyCluster.GetZones();
            }

            if (zones_Model == null || zones_Model.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The supplied analytical object carries no zones, so there is nothing to assess.");
                return;
            }

            //Requested zones are matched back to the model's own zones BY GUID, so a zone rebuilt or renamed
            //upstream cannot silently select a different one - and two zones sharing a name cannot collide.
            //Connected-but-every-guid-invalid is NOT the same statement as "unconnected". Only an
            //UNCONNECTED port means "assess the whole model" - once a caller has explicitly named zones, a
            //selection that resolved to nothing is a stale or wrong reference, and silently broadening it to
            //every zone in the model would turn a targeted request into a whole-building assessment.
            bool zonesConnected = false;
            List<Zone> zones = null;
            index = Params.IndexOfInputParam("zones_");
            if (index != -1)
            {
                List<IAnalyticalObject> analyticalObjects = [];
                if (dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
                {
                    zonesConnected = true;
                    zones = [];

                    foreach (Zone zone in analyticalObjects.FindAll(x => x is Zone).ConvertAll(x => (Zone)x))
                    {
                        Zone zone_Model = zones_Model.Find(x => x != null && x.Guid == zone.Guid);
                        if (zone_Model == null)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Zone '{0}' is not in the supplied analytical object, so it produces no scenario.", zone.Name));
                            continue;
                        }

                        zones.Add(zone_Model);
                    }
                }
            }

            if (zonesConnected && zones.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "None of the zones supplied to zones_ are in the supplied analytical object, so no scenarios were created. Correct the selection, or disconnect zones_ to assess the whole model.");
                return;
            }

            //Unconnected means the whole model, which is the shape the dwelling category has: the flats and the
            //corridor together, each classified on its own.
            zones ??= zones_Model;

            index = Params.IndexOfInputParam("_partOIteration");
            string text_PartOIteration = null;
            if (index == -1 || !dataAccess.GetData(index, ref text_PartOIteration) || string.IsNullOrWhiteSpace(text_PartOIteration))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Core.Query.TryGetEnum(text_PartOIteration, out PartOIteration partOIteration))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("'{0}' is not a Part O iteration. Use BasePassive or AcousticRestricted.", text_PartOIteration));
                return;
            }

            List<string> ventilationStrategies = [];
            index = Params.IndexOfInputParam("_ventilationStrategies");
            if (index == -1 || !dataAccess.GetDataList(index, ventilationStrategies) || ventilationStrategies.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "A ventilation strategy is required for every zone - NV, MV, MVRE or UV. It is not defaulted, because a silent default assessed a mechanically ventilated dwelling against the natural-ventilation criterion.");
                return;
            }

            //One value applies to every zone; otherwise one per zone, in order. A partial list is refused rather
            //than padded, because padding would quietly assess the tail of the building on the wrong criterion.
            if (ventilationStrategies.Count != 1 && ventilationStrategies.Count != zones.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format("{0} ventilation strategies were supplied for {1} zones. Supply one value to apply to every zone, or exactly one per zone.", ventilationStrategies.Count, zones.Count));
                return;
            }

            Dictionary<Guid, string> dictionary_VentilationStrategy = [];
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] == null)
                {
                    continue;
                }

                dictionary_VentilationStrategy[zones[i].Guid] = ventilationStrategies.Count == 1 ? ventilationStrategies[0] : ventilationStrategies[i];
            }

            List<OverheatingScenario> overheatingScenarios = Create.OverheatingScenarios(zones, partOIteration, dictionary_VentilationStrategy, out List<string> refusals);

            //Reported, not swallowed: a zone missing from the set is an assessment the building will not get.
            foreach (string refusal in refusals)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, refusal);
            }

            index = Params.IndexOfOutputParam("overheatingScenarios");
            if (index != -1)
            {
                dataAccess.SetDataList(index, overheatingScenarios.ConvertAll(x => new GooAnalyticalObject(x)));
            }

            index = Params.IndexOfOutputParam("refusals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, refusals);
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, overheatingScenarios.Count != 0 && refusals.Count == 0);
            }
        }
    }
}
