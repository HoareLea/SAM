using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

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
        public override string LatestComponentVersion => "1.0.2";

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

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Brep() { Name = "_brep", NickName = "_brep", Description = "Brep", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_insideOnly", NickName = "_insideOnly", Description = "Inside Only", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance", NickName = "_tolerance", Description = "Tolerance", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "In", NickName = "In", Description = "SAM Analytical Panels In", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Out", NickName = "Out", Description = "SAM Analytical Panels Out", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_brep");
            Rhino.Geometry.Brep brep = null;
            if (index == -1 || !dataAccess.GetData(index, ref brep))
            {
                if (index_In != -1)
                    dataAccess.SetDataList(index_In, panels?.ConvertAll(x => new GooPanel(x)));

                return;
            }

            index = Params.IndexOfInputParam("_insideOnly");
            bool insideOnly = false;
            if (index == -1 || !dataAccess.GetData(2, ref insideOnly))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_tolerance");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(3, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Shell shell = brep.ToSAM_Shell(true);
            if(shell == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels_Result = null;

            if (insideOnly)
                panels_Result = Analytical.Query.Inside(panels, shell, Core.Tolerance.MacroDistance, tolerance);
            else
                panels_Result = Analytical.Query.InRange(panels, shell, tolerance);


            if (index_In != -1)
                dataAccess.SetDataList(index_In, panels_Result?.ConvertAll(x => new GooPanel(x)));

            if (index_Out != -1)
            {
                if (panels_Result == null || panels_Result.Count == 0)
                    dataAccess.SetDataList(index_Out, panels?.ConvertAll(x => new GooPanel(x)));
                else
                    dataAccess.SetDataList(index_Out, panels?.FindAll(x => !panels_Result.Contains(x)).ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}