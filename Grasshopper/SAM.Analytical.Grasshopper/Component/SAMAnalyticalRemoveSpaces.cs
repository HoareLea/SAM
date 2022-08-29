using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("56a2d7a1-de62-4996-be6d-df2894dc1600");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRemoveSpaces()
          : base("SAMAnalytical.RemoveSpaces", "SAMAnalytical.RemoveSpaces",
              "Remove Spaces from SAM Analytical Model",
              "SAM WIP", "Analytical")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point { Name = "_points", NickName = "_points", Description = "Points", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean { Name = "_removeExtPanels_", NickName = "_removeExtPanels_", Description = "Remove External Panels", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            
            SAMObject sAMObject_Temp = null;
            if(!dataAccess.GetData(index, ref sAMObject_Temp) || sAMObject_Temp == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (sAMObject_Temp is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject_Temp);
            }
            else if (sAMObject_Temp is AnalyticalModel)
            {
                analyticalModel = (AnalyticalModel)sAMObject_Temp;
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_points");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_Point> points = new List<GH_Point>();
            if (!dataAccess.GetDataList(index, points))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool removePanels = true;
            index = Params.IndexOfInputParam("_removeExtPanels");
            if(index != -1)
            {
                dataAccess.GetData(index, ref removePanels);
            }

            Dictionary<Space, Shell> dictionary = adjacencyCluster?.ShellDictionary();

            List<Space> result = new List<Space>();
            foreach (GH_Point point in points)
            {
                Point3D point3D = Geometry.Grasshopper.Convert.ToSAM(point);
                if(point3D == null)
                {
                    continue;
                }

                foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary)
                {
                    if (keyValuePair.Value.InRange(point3D) || keyValuePair.Value.Inside(point3D))
                    {

                        Space space = keyValuePair.Key;

                        List<Panel> panels = adjacencyCluster.GetPanels(space);
                        
                        result.Add(space);
                        dictionary.Remove(space);
                        adjacencyCluster.RemoveObject<Space>(space.Guid);

                        foreach(Panel panel in panels)
                        {
                            Panel panel_Temp = null;
                            if (adjacencyCluster.Shade(panel))
                            {
                                if(removePanels)
                                {
                                    adjacencyCluster.RemoveObject<Panel>(panel.Guid);
                                }
                                else
                                {
                                    panel_Temp = Create.Panel(panel, PanelType.Shade);
                                }
                            }
                            else if(adjacencyCluster.External(panel))
                            {
                                if(panel.PanelType == PanelType.WallInternal)
                                {
                                    panel_Temp = Create.Panel(panel, PanelType.WallExternal);
                                }
                                if (panel.PanelType == PanelType.FloorInternal)
                                {
                                    panel_Temp = Create.Panel(panel, PanelType.FloorExposed);
                                }
                            }

                            if(panel_Temp != null)
                            {
                                adjacencyCluster.AddObject(panel_Temp);
                            }
                        }

                        break;
                    }
                }
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, result?.ConvertAll(x => new GooSpace(x)));

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                if (sAMObject_Temp is AdjacencyCluster)
                {
                    sAMObject_Temp = adjacencyCluster;
                }
                else if (sAMObject_Temp is AnalyticalModel)
                {
                    sAMObject_Temp = new AnalyticalModel(analyticalModel, adjacencyCluster);
                }

                dataAccess.SetData(index, sAMObject_Temp);
            }

        }
    }
}