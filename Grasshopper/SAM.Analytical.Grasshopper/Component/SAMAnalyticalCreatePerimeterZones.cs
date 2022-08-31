using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePerimeterZones : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("53e9688b-57a8-408d-ab0c-b66eaa1a41a8");

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
        public SAMAnalyticalCreatePerimeterZones()
          : base("SAMAnalytical.CreatePerimeterZones", "SAMAnalytical.CreatePerimeterZones",
              "Creae Perimeter Zones in Analytical Model or AdjacencyCluster",
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

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_offset_", NickName = "_offset_", Description = "Offset", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(6.0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_minOffset_", NickName = "_minOffset_", Description = "Minimal Offset", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(1.0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "panels_", NickName = "panels_", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "zones", NickName = "zones", Description = "SAM Analytical Zones", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
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
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalObject is AnalyticalModel ? ((AnalyticalModel)analyticalObject).AdjacencyCluster : analyticalObject as AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            }


            double offset = 6.0;
            index = Params.IndexOfInputParam("_offset_");
            if (index != -1)
            {
                double offset_Temp = double.NaN;
                if(dataAccess.GetData(index, ref offset_Temp) && !double.IsNaN(offset_Temp))
                {
                    offset = offset_Temp;
                }
            }

            double minOffset = 6.0;
            index = Params.IndexOfInputParam("_minOffset_");
            if (index != -1)
            {
                double minOffset_Temp = double.NaN;
                if (dataAccess.GetData(index, ref minOffset_Temp) && !double.IsNaN(minOffset_Temp))
                {
                    minOffset = minOffset_Temp;
                }
            }

            List<Space> spaces = null;
            index = Params.IndexOfInputParam("spaces_");
            if(index != -1)
            {
                List<Space> spaces_Temp = new List<Space>();

                if (dataAccess.GetDataList(index, spaces_Temp) && spaces_Temp != null && spaces_Temp.Count != 0)
                {
                    spaces = spaces_Temp;
                }
            }

            List<Panel> panels = null;
            index = Params.IndexOfInputParam("panels_");
            if (index != -1)
            {
                List<Panel> panels_Temp = new List<Panel>();

                if (dataAccess.GetDataList(index, panels_Temp) && panels_Temp != null && panels_Temp.Count != 0)
                {
                    panels = panels_Temp;
                }
            }

            if (spaces == null)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            List<Zone> zones = null;

            if(spaces != null)
            {

                zones = new List<Zone>();
                foreach (Space space in spaces)
                {
                    Func<Panel, double> func = new Func<Panel, double>((Panel panel) => 
                    {
                        if(panel == null)
                        {
                            return 0;
                        }

                        if(panels != null)
                        {
                            return panels.Find(x => x.Guid == panel.Guid) != null ? offset : 0;
                        }

                        if(!adjacencyCluster.External(panel))
                        {
                            return 0;
                        }

                        List<Aperture> apertures = panel.Apertures;

                        if(apertures == null || apertures.Count == 0)
                        {
                            return 0;
                        }

                        return offset; 
                    });


                    List<Space> spaces_Split = adjacencyCluster.SplitSpace(space.Guid, func, minOffset);
                    if(spaces_Split == null || spaces_Split.Count < 2)
                    {
                        continue;
                    }

                    Zone zone = new Zone(space.Name);

                    adjacencyCluster.AddObject(zone);
                    foreach(Space space_Split in spaces_Split)
                    {
                        adjacencyCluster.AddRelation(zone, space_Split);
                    }

                    zones.Add(zone);
                }
            }

            if(analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if(analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("zones");
            if (index != -1)
            {
                dataAccess.SetDataList(index, zones?.ConvertAll(x => new GooGroup(x)));
            }
        }
    }
}