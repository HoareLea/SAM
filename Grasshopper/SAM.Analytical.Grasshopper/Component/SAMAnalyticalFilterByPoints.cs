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
        public override Guid ComponentGuid => new ("be65e704-b18c-4f31-a322-de789d49ba69");

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
        public SAMAnalyticalFilterByPoints()
          : base("SAMAnalytical.FilterByPoints", "SAMAnalytical.FilterByPoints",
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
                List<GH_SAMParam> result = [];

                GooAnalyticalObjectParam gooAnalyticalObjectParam = new () { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Object \nAnalytical Model, Adjacency Cluster, Panels or Apertures", Access = GH_ParamAccess.list };
                gooAnalyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "_points", NickName = "_points", Description = "Points", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber = new () { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));
                
                
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
                
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Panels_In", NickName = "Panels_In", Description = "SAM Analytical Panels In", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Panels_Out", NickName = "Panels_Out", Description = "SAM Analytical Panels Out", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Apertures_In", NickName = "Apertures_In", Description = "SAM Analytical Apertures In", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Apertures_Out", NickName = "Apertures_Out", Description = "SAM Analytical Apertures Out", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
                return [.. result];
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

            int index_Panels_In = Params.IndexOfOutputParam("Panels_In");
            int index_Panels_Out = Params.IndexOfOutputParam("Panels_Out");

            int index_Apertures_In = Params.IndexOfOutputParam("Apertures_In");
            int index_Apertures_Out = Params.IndexOfOutputParam("Apertures_Out");

            index = Params.IndexOfInputParam("_analyticals");
            List<IAnalyticalObject> analyticalObjects = [];
            if (index == -1 || !dataAccess.GetDataList(index, analyticalObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_points");
            List<GH_Point> points = [];
            if (index == -1 || !dataAccess.GetDataList(index, points))
            {
                if (index_Panels_In != -1)
                {
                    dataAccess.SetDataList(index_Panels_In, analyticalObjects);
                }

                return;
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = [.. analyticalObjects.FindAll(x => x is Panel).Cast<Panel>()];
            List<Aperture> apertures = [.. analyticalObjects.FindAll(x => x is Aperture).Cast<Aperture>()];

            List<AdjacencyCluster> adjacencyClusters = [];
            foreach(IAnalyticalObject analyticalObject in analyticalObjects)
            {
                List<Panel> panels_Temp = null;
                List<Aperture> apertures_Temp = null;
                if (analyticalObject is AdjacencyCluster adjacencyCluster_Temp)
                {
                    panels_Temp = adjacencyCluster_Temp.GetPanels();
                    apertures_Temp = adjacencyCluster_Temp.GetApertures();
                }
                else if(analyticalObject is AnalyticalModel analyticalModel)
                {
                    panels_Temp = analyticalModel.GetPanels();
                    apertures_Temp = analyticalModel.GetApertures(x => true);
                }

                if(panels_Temp != null && panels_Temp.Count != 0)
                {
                    panels.AddRange(panels_Temp);
                }

                if (apertures_Temp != null && apertures_Temp.Count != 0)
                {
                    apertures.AddRange(apertures_Temp);
                }
            }

            #region PANELS
            List<Tuple<Panel, Face3D>> tuples_Panel = panels.ConvertAll(x => new Tuple<Panel, Face3D>(x, x?.Face3D));
            tuples_Panel.RemoveAll(x => x.Item2 == null);

            Dictionary<Guid, Panel> dictionary_Panel = [];
            foreach(GH_Point point in points)
            {
                Point3D point3D = point?.Value.ToSAM();
                if(point3D == null)
                {
                    continue;
                }

                tuples_Panel.FindAll(x => x.Item2.Inside(point3D, tolerance)).ForEach(x => dictionary_Panel[x.Item1.Guid] = x.Item1);
            }

            if (index_Panels_In != -1)
            {
                dataAccess.SetDataList(index_Panels_In, dictionary_Panel.Values);
            }

            if (index_Panels_Out != -1)
            {
                if (dictionary_Panel.Values.Count == 0)
                {
                    dataAccess.SetDataList(index_Panels_Out, analyticalObjects);
                }
                else
                {
                    dataAccess.SetDataList(index_Panels_Out, tuples_Panel?.FindAll(x => !dictionary_Panel.ContainsKey(x.Item1.Guid)).ConvertAll(x => x.Item1));
                }
            }
            #endregion

            #region APERTURES
            List<Tuple<Aperture, Face3D>> tuples_Aperture = apertures.ConvertAll(x => new Tuple<Aperture, Face3D>(x, x?.Face3D));
            tuples_Aperture.RemoveAll(x => x.Item2 == null);

            Dictionary<Guid, Aperture> dictionary_Aperture = [];
            foreach (GH_Point point in points)
            {
                Point3D point3D = point?.Value.ToSAM();
                if (point3D == null)
                {
                    continue;
                }

                tuples_Aperture.FindAll(x => x.Item2.Inside(point3D, tolerance)).ForEach(x => dictionary_Aperture[x.Item1.Guid] = x.Item1);
            }

            if (index_Apertures_In != -1)
            {
                dataAccess.SetDataList(index_Apertures_In, dictionary_Aperture.Values);
            }

            if (index_Apertures_Out != -1)
            {
                if (dictionary_Aperture.Values.Count == 0)
                {
                    dataAccess.SetDataList(index_Apertures_Out, analyticalObjects);
                }
                else
                {
                    dataAccess.SetDataList(index_Apertures_Out, tuples_Aperture?.FindAll(x => !dictionary_Aperture.ContainsKey(x.Item1.Guid)).ConvertAll(x => x.Item1));
                }
            }
            #endregion



        }
    }
}