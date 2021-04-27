using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Architectural.Grasshopper;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateLevels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3adac138-0250-4c63-88bc-efaf7a36c2f5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateLevels()
          : base("SAMAnalytical.CreateLevels", "SAMAnalytical.CreateLevels",
              "Create SAM Architectural Levels from Panels",
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooLevelParam() { Name = "levels", NickName = "levels", Description = "SAM Architectural Levels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooLevelParam() { Name = "topLevel", NickName = "topLevel", Description = "SAM Architectural Level", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            if (!dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Core.Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            index = Params.IndexOfOutputParam("levels");
            if(index != -1)
            {
                List<Architectural.Level> levels = Create.Levels(panels, false, tolerance);
                dataAccess.SetDataList(index, levels?.ConvertAll(x => new GooLevel(x)));
            }

            index = Params.IndexOfOutputParam("topLevel");
            if(index != -1)
            {
                double elevation = double.MinValue;
                foreach(Panel panel in panels)
                {
                    double elevation_Panel = panel.MaxElevation();
                    if (double.IsNaN(elevation_Panel))
                    {
                        continue;
                    }

                    if(elevation_Panel > elevation)
                    {
                        elevation = elevation_Panel;
                    }
                }

                if(elevation != double.MinValue)
                {
                    dataAccess.SetData(index, Architectural.Create.Level(elevation));
                }
            }

        }
    }
}