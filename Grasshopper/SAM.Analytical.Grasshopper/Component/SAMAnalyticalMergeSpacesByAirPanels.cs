using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Grasshopper component that merges SAM Analytical Spaces across AirPanels (a.k.a. AirWalls).
    /// </summary>
    /// <remarks>
    /// <para><b>Default (no Spaces input connected)</b>: All AirPanels in the model are removed. For each AirPanel,
    /// the two adjacent Spaces are merged into a single Space. The surviving Space identity is implementation-dependent;
    /// you cannot control which of the two original Spaces remains.</para>
    /// <para><b>With optional Spaces input</b>: Only AirPanels whose <i>both</i> adjacent Spaces are present in the
    /// provided set are removed; all other AirPanels remain. Use this to limit the scope (and count) of merges when multiple
    /// AirPanels exist.</para>
    /// <para><b>Tip</b>: After merging, you may want to normalise types with <c>SAMAdjacencyCluster.UpdatePanelTypes</c>
    /// and optionally clean coplanar boundaries with <c>SAMAnalytical.MergeCoplanarPanelsBySpace</c>.</para>
    /// </remarks>
    /// <example>
    /// Typical usage:
    ///   1) (Optional) Provide a list of Spaces to restrict which AirPanels are removed.
    ///   2) Run MergeSpacesByAirPanels to combine Spaces across the selected AirPanels.
    ///   3) (Optional) Run UpdatePanelTypes / MergeCoplanarPanelsBySpace for cleanup.
    /// </example>
    /// <seealso cref="SAMAdjacencyCluster.UpdatePanelTypes"/>
    /// <seealso cref="SAMAnalytical.MergeCoplanarPanelsBySpace"/>
    public class SAMAnalyticalMergeSpacesByAirPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("db0cc435-6057-47bd-af3e-eb0825070d96");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        /// <summary>Initializes the component with a concise nickname and detailed description.</summary>
        public SAMAnalyticalMergeSpacesByAirPanels()
          : base(
              "SAMAnalytical.MergeSpacesByAirPanels",
              "MergeSpacesByAirPanels",
              "Merge Analytical Spaces across AirPanels (AirWalls).\n" +
              "\n" +
              "Default (no Spaces input connected):\n" +
              "  • Removes all AirPanels and merges each adjacent Space pair into one.\n" +
              "  • The surviving Space identity is implementation-dependent (not user-controlled).\n" +
              "\n" +
              "With optional Spaces input:\n" +
              "  • Removes only AirPanels whose both adjacent Spaces are in the provided set.\n" +
              "  • Use this to limit how many AirPanels are removed (and thus how many merges occur).\n" +
              "\n" +
              "Post-step:\n" +
              "  • Consider SAMAdjacencyCluster.UpdatePanelTypes and SAMAnalytical.MergeCoplanarPanelsBySpace.",
              "SAM", "Analytical02")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                //result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(
                    new GooSpaceParam()
                    {
                        Name = "_spaces_",
                        NickName = "_spaces_",
                        Description =
                            "Optional filter of SAM Analytical Spaces.\n" +
                            "• When connected: remove only AirPanels whose both adjacent Spaces are in this set, and merge those pairs.\n" +
                            "• When not connected: remove all AirPanels and merge all adjacent Space pairs.",
                        Access = GH_ParamAccess.list,
                        Optional = true
                    },
                    ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean;

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "run_", NickName = "run_", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "Resulting Spaces after merges across removed AirPanels.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "Removed Air Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            if (spaces == null || spaces.Count == 0)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            List<Space> spaces_Result = adjacencyCluster.MergeSpaces(out List<Panel> panels, new PanelType[] { PanelType.Air }, spaces?.FindAll(x => x != null).ConvertAll(x => x.Guid));

            if (sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Result?.ConvertAll(x => new GooSpace(x)));

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}