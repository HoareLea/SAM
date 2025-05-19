using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAdjacentSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("575f2f39-a0a7-44a7-8660-e04e640166a4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAdjacentSpaces()
          : base("SAMAnalytical.AdjacentSpaces", "SAMAnalytical.AdjacentSpaces",
              "Get Adjacent Spaces",
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_panelGroups_", NickName = "_panelGroups_", Description = "PanelGroups", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces adjacent to space", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels adjacent to space", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spacesWithDoors", NickName = "spacesWithDoors", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panelsWithDoors", NickName = "panelsWithDoors", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spacesWithWindows", NickName = "spacesWithWindows", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panelsWithWindows", NickName = "panelsWithWindows", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            int index;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if(index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_space");
            Space space = null;
            if (index == -1 || !dataAccess.GetData(index, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HashSet<PanelGroup> panelGroups = null;
            index = Params.IndexOfInputParam("_panelGroups_");
            if(index != -1)
            {
                List<string> values = new List<string>();
                if(dataAccess.GetDataList(index, values) && values != null && values.Count != 0)
                {
                    panelGroups = new HashSet<PanelGroup>();
                    foreach(string value in values)
                    {
                        panelGroups.Add(SAM.Core.Query.Enum<PanelGroup>(value));
                    }
                }
            }

            if(panelGroups == null || panelGroups.Count == 0)
            {
                panelGroups = new HashSet<PanelGroup>();
                foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
                {
                    panelGroups.Add(panelGroup);
                }
            }

            AdjacencyCluster adjacencyCluster = null;
            if(analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            }
            else if(analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }

            List<Space> spaces = new List<Space>();
            List<Panel> panels = new List<Panel>();

            List<Space> spaces_Door = new List<Space>();
            List<Panel> panels_Door = new List<Panel>();

            List<Space> spaces_Windows = new List<Space>();
            List<Panel> panels_Windows = new List<Panel>();

            List<Panel> panels_Space = adjacencyCluster?.GetRelatedObjects<Panel>(space);
            if(panels_Space != null)
            {
                foreach(Panel panel in panels_Space)
                {
                    if(panel == null)
                    {
                        continue;
                    }

                    if(!panelGroups.Contains(panel.PanelGroup))
                    {
                        continue;
                    }

                    List<Space> spaces_Panel = adjacencyCluster?.GetRelatedObjects<Space>(panel);
                    if(spaces_Panel == null || spaces_Panel.Count == 0)
                    {
                        continue;
                    }

                    List<Aperture> apertures = panel.Apertures;

                    bool hasWindows = false;
                    bool hasDoors = false;
                    if(apertures != null)
                    {
                        hasDoors = apertures.Find(x => x != null && x.ApertureType == ApertureType.Door) != null;
                        hasWindows = apertures.Find(x => x != null && x.ApertureType == ApertureType.Window) != null;
                    }

                    foreach(Space space_Panel in spaces_Panel)
                    {
                        if(space_Panel.Guid == space.Guid)
                        {
                            continue;
                        }

                        spaces.Add(space_Panel);
                        panels.Add(panel);

                        if (hasDoors)
                        {
                            spaces_Door.Add(space_Panel);
                            panels_Door.Add(panel);
                        }

                        if (hasWindows)
                        {
                            spaces_Windows.Add(space_Panel);
                            panels_Windows.Add(panel);
                        }
                    }

                }
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces);
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels);
            }



            index = Params.IndexOfOutputParam("spacesWithDoors");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces_Door);
            }

            index = Params.IndexOfOutputParam("panelsWithDoors");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Door);
            }



            index = Params.IndexOfOutputParam("spacesWithWindows");
            if (index != -1)
            {
                dataAccess.SetDataList(index, spaces_Windows);
            }

            index = Params.IndexOfOutputParam("panelsWithWindows");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Windows);
            }
        }
    }
}