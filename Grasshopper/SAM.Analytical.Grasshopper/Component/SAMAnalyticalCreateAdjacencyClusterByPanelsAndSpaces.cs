﻿using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterByPanelsAndSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1f2d4717-f2ec-463e-84bd-f8c0f7f230c7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Grasshopper component that creates an <c>AdjacencyCluster</c> from SAM <c>Panels</c> and <c>Spaces</c>.
        /// </summary>
        /// <remarks>
        /// <para><b>Geometric assumption</b>: This method is valid only when the building envelope
        /// monotonically decreases (or remains constant) with height—i.e., no level has a larger
        /// footprint than the one below (no outward overhangs/cantilevers).</para>
        /// <para><b>Panel preparation</b>: Panels must be split per building level. For double-height or multi-storey voids,
        /// pre-process with <c>SAMAnalytical.SplitPanelsByElevations</c> and set <c>addMissingPanels_ = true</c> to auto-insert
        /// <c>AirPanels</c> where floor/ceiling separation is missing.</para>
        /// <para><b>Overlaps</b>: Overlapping/copanar Panels are supported; duplicates are resolved during clustering.</para>
        /// <para><b>No AirPanel subdivision?</b> Run <c>SAMAnalytical.MergeSpacesByAirPanels</c> after this component, and optionally
        /// <c>SAMAnalytical.Mer­geCoplanarPanelsBySpace</c>, to remove splits.</para>
        /// <para><b>Post-step</b>: Call <c>SAMAdjacencyCluster.UpdatePanelTypes</c> to normalise/repair <c>PanelTypes</c>.</para>
        /// </remarks>
        /// <example>
        /// Inputs:
        ///   Panels — IEnumerable&lt;SAM.Analytical.Panel&gt; (analytical surfaces; not the GH UI Panel)
        ///   Spaces — IEnumerable&lt;SAM.Analytical.Space&gt;
        /// Output:
        ///   AdjacencyCluster — SAM.Analytical.AdjacencyCluster
        /// </example>
        /// <seealso cref="SAMAdjacencyCluster.UpdatePanelTypes"/>
        /// <seealso cref="SAMAnalytical.SplitPanelsByElevations"/>
        /// <seealso cref="SAMAnalytical.MergeSpacesByAirPanels"/>
        /// <seealso cref="SAMAnalytical.MergeCoplanarPanelsBySpace"/>
        public SAMAnalyticalCreateAdjacencyClusterByPanelsAndSpaces()
          : base(
              "SAMAnalytical.CreateAdjacencyClusterByPanelsAndSpaces",
              "CreateAdjacencyCluster",
              "Create an AdjacencyCluster from SAM Panels and SAM Spaces.\n" +
               "Assumption: valid only when the building footprint decreases or stays the same with height (no outward overhangs/cantilevers).\n" +
              "\n" +
              "- Panels must be split per level. For double-height spaces, pre-process with " +
              "SAMAnalytical.SplitPanelsByElevations and set addMissingPanels_ = True to insert AirPanels " +
              "where floor/ceiling separation is missing.\n" +
              "- Overlapping / coplanar Panels are supported; duplicates are resolved during clustering.\n" +
              "- If you do not want subdivision by AirPanels, run SAMAnalytical.MergeSpacesByAirPanels after " +
              "this component, optionally followed by SAMAnalytical.MergeCoplanarPanelsBySpace to remove splits.\n" +
              "- After creation, use SAMAdjacencyCluster.UpdatePanelTypes to correct PanelTypes.\n" +
              "\n" +
              "Inputs:\n" +
              "  • Panels : SAM.Analytical.Panel (analytical surfaces, not GH UI Panel)\n" +
              "  • Spaces : SAM.Analytical.Space\n" +
              "Output:\n" +
              "  • AdjacencyCluster",
              "SAM",
              "Analytical")
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

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                GooSpaceParam spaceParam = new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true };
                spaceParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(spaceParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset from the floors for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "addMissingSpaces_", NickName = "addMissingSpaces_", Description = "Add Missing Spaces", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "addMissingPanels_", NickName = "addMissingPanels_", Description = "Add Missing Panels \n* will generate AirFloor if missing panels", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "snapTolerance_", NickName = "snapTolerance_", Description = "Snap Tolerance for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Area", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index != -1)
                dataAccess.GetDataList(index, spaces);

            index = Params.IndexOfInputParam("offset_");
            double offset = 0.1;
            if (index != -1)
                dataAccess.GetData(index, ref offset);

            index = Params.IndexOfInputParam("snapTolerance_");
            double snapTolerance = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref snapTolerance);

            index = Params.IndexOfInputParam("addMissingSpaces_");
            bool addMissingSpaces = true;
            if (index != -1)
                dataAccess.GetData(index, ref addMissingSpaces);

            index = Params.IndexOfInputParam("addMissingPanels_");
            bool addMissingPanels = false;
            if (index != -1)
                dataAccess.GetData(index, ref addMissingPanels);

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref silverSpacing);

            index = Params.IndexOfInputParam("minArea_");
            double minArea = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref minArea);

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(spaces, panels, offset, addMissingSpaces, addMissingPanels, 0.01, minArea, snapTolerance, Core.Tolerance.Angle, silverSpacing, tolerance, Core.Tolerance.Angle);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}