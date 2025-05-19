using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelSpacing : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32fcd1a9-7926-45de-bd27-3dde667ba0d0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPanelSpacing()
          : base("SAMAnalytical.PanelSpacing", "SAMAnalytical.PanelSpacing",
              "Calculates Spacing between Panels",
              "SAM", "Analytical03")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_max_", NickName = "_max_", Description = "Maximal distance to be checked", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_min_", NickName = "_min_", Description = "Minimal distance to be checked", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "points", NickName = "points", Description = "Points", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
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
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_max_");
            double max = Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref max);
            }

            index = Params.IndexOfInputParam("_min_");
            double min = Tolerance.Distance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref min);
            }

            Dictionary<Point3D, List<Panel>> dictionary = Analytical.Query.SpacingDictionary(panels, max, min);

            index = Params.IndexOfOutputParam("points");
            if(index != -1)
            {
                dataAccess.SetDataList(index, dictionary?.Keys.ToList().ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x)));
            }

            index = Params.IndexOfOutputParam("panels");
            if(index != -1)
            {
                DataTree<GooPanel> dataTree_Panel = new DataTree<GooPanel>();

                int count = 0;
                foreach (KeyValuePair<Point3D, List<Panel>> keyValuePair in dictionary)
                {
                    GH_Path path = new GH_Path(count);
                    keyValuePair.Value?.ForEach(x => dataTree_Panel.Add(new GooPanel(x), path));
                    count++;
                }

                dataAccess.SetDataTree(index, dataTree_Panel);
            }
        }
    }
}