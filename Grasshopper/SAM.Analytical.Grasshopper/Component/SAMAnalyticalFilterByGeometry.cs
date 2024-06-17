using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c8a0fb53-360b-4ab7-be97-49336b516847");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.5";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByGeometry()
          : base("SAMAnalytical.FilterByGeometry", "SAMAnalytical.FilterByGeometry",
              "Filter Analytical Objects By Geometry, output Panels that are inside closed brep",
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

                GooAnalyticalObjectParam gooAnalyticalObjectParam = new GooAnalyticalObjectParam() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Object \nAnalytical Model, Adjacency Cluster, Panels or Spaces", Access = GH_ParamAccess.list };
                gooAnalyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Brep() { Name = "_brep", NickName = "_brep", Description = "Brep", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_insideOnly_", NickName = "_insideOnly_", Description = "Inside Only\nif True only fully inside element will be return", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "In", NickName = "In", Description = "SAM Analytical Objects In - Panels or Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Out", NickName = "Out", Description = "SAM Analytical Objects Out", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            int index_In = Params.IndexOfOutputParam("In");
            int index_Out = Params.IndexOfOutputParam("Out");

            index = Params.IndexOfInputParam("_analyticals");
            List<IAnalyticalObject> analyticalObjects = new List<IAnalyticalObject>();
            if (index == -1 || !dataAccess.GetDataList(index, analyticalObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_brep");
            global::Rhino.Geometry.Brep brep = null;
            if (index == -1 || !dataAccess.GetData(index, ref brep))
            {
                if (index_In != -1)
                    dataAccess.SetDataList(index_In, analyticalObjects);

                return;
            }

            index = Params.IndexOfInputParam("_insideOnly_");
            bool insideOnly = false;
            if (index == -1 || !dataAccess.GetData(2, ref insideOnly))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(3, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Shell shell = Geometry.Rhino.Convert.ToSAM_Shell(brep, true);
            if(shell == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = analyticalObjects.FindAll(x => x is Panel).Cast<Panel>().ToList();
            List<Space> spaces = analyticalObjects.FindAll(x => x is Space).Cast<Space>().ToList();

            List<AdjacencyCluster> adjacencyClusters = new List<AdjacencyCluster>();
            foreach(IAnalyticalObject analyticalObject in analyticalObjects)
            {
                List<Panel> panels_Temp = null;
                if(analyticalObject is AdjacencyCluster)
                {
                    panels_Temp = ((AdjacencyCluster)analyticalObject).GetPanels();
                }
                else if(analyticalObject is AnalyticalModel)
                {
                    panels_Temp = ((AnalyticalModel)analyticalObject).GetPanels();
                }

                if(panels_Temp == null || panels_Temp.Count == 0)
                {
                    continue;
                }

                if(panels == null)
                {
                    panels = new List<Panel>();
                }

                panels.AddRange(panels_Temp);
            }

            List<IAnalyticalObject> analyticalObjects_Result = new List<IAnalyticalObject>();
            if (insideOnly)
            {
                analyticalObjects_Result.AddRange(Geometry.Object.Spatial.Query.Inside(shell, panels, Tolerance.MacroDistance, tolerance));
                analyticalObjects_Result.AddRange(Analytical.Query.Inside(spaces, shell, Tolerance.MacroDistance, tolerance));
            }
            else
            {
                analyticalObjects_Result.AddRange(Geometry.Object.Spatial.Query.InRange(shell, panels, tolerance));
                analyticalObjects_Result.AddRange(Analytical.Query.InRange(spaces, shell, tolerance));
            }

            if (index_In != -1)
            {
                dataAccess.SetDataList(index_In, analyticalObjects_Result);
            }

            if (index_Out != -1)
            {
                if (analyticalObjects_Result == null || analyticalObjects_Result.Count == 0)
                {
                    dataAccess.SetDataList(index_Out, analyticalObjects);
                }
                else
                {
                    dataAccess.SetDataList(index_Out, analyticalObjects?.FindAll(x => !analyticalObjects_Result.Contains(x)));
                }
            }
        }
    }
}