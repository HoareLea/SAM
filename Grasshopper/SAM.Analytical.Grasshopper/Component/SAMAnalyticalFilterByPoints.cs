using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Rhino;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByPoints : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be65e704-b18c-4f31-a322-de789d49ba69");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByPoints()
          : base("SAMAnalyticalPanel.FilterByPoints", "SAMAnalytical.FilterByPoints",
              "Filter Analytical Objects By Points",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "_points", NickName = "_points", Description = "Points", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("_points");
            List<GH_Point> points = new List<GH_Point>();
            if (index == -1 || !dataAccess.GetDataList(index, points))
            {
                if (index_In != -1)
                    dataAccess.SetDataList(index_In, analyticalObjects);

                return;
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = analyticalObjects.FindAll(x => x is Panel).Cast<Panel>().ToList();

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

            List<Tuple<Panel, Face3D>> tuples_Panel = panels.ConvertAll(x => new Tuple<Panel, Face3D>(x, x?.Face3D));
            tuples_Panel.RemoveAll(x => x.Item2 == null);

            Dictionary<Guid, Panel> dictionary_Panel = new Dictionary<Guid, Panel>();
            foreach(GH_Point point in points)
            {
                Point3D point3D = point?.Value.ToSAM();
                if(point3D == null)
                {
                    continue;
                }

                tuples_Panel.FindAll(x => x.Item2.Inside(point3D, tolerance)).ForEach(x => dictionary_Panel[x.Item1.Guid] = x.Item1);
            }

            if (index_In != -1)
            {
                dataAccess.SetDataList(index_In, dictionary_Panel.Values);
            }

            if (index_Out != -1)
            {
                if (dictionary_Panel.Values.Count == 0)
                {
                    dataAccess.SetDataList(index_Out, analyticalObjects);
                }
                else
                {
                    dataAccess.SetDataList(index_Out, tuples_Panel?.FindAll(x => !dictionary_Panel.ContainsKey(x.Item1.Guid)).ConvertAll(x => x.Item1));
                }
            }
        }
    }
}