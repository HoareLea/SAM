using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAirPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5f0e4a26-2ac3-434a-8915-434b93a2c0b4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAirPanels()
          : base("SAMAnalytical.AddAirPanels", "SAMAnalytical.AddAirPanels",
              "Add Air Panels to Analytical Model or AdjacencyCluster",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_planes", NickName = "_planes", Description = "SAM Geometry Planes", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "airPanels", NickName = "airPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
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
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("_planes");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Plane> planes = new List<Plane>();

            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                Plane plane = null;

                object @object = objectWrapper.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object is double)
                {
                    plane = Geometry.Spatial.Create.Plane((double)@object);
                }
                else if (@object is string)
                {
                    if (double.TryParse((string)@object, out double elevation_Temp))
                    {
                        plane = Geometry.Spatial.Create.Plane(elevation_Temp);
                    }
                }
                else if (@object is Rhino.Geometry.Plane)
                {
                    plane = Geometry.Grasshopper.Convert.ToSAM(((Rhino.Geometry.Plane)@object));
                }
                else if (@object is Plane)
                {
                    plane = (Plane)@object;
                }

                if(plane == null)
                {
                    continue;
                }

                planes.Add(plane);
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

            AdjacencyCluster adjacencyCluster = sAMObject is AnalyticalModel ? ((AnalyticalModel)sAMObject).AdjacencyCluster : sAMObject as AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                if(sAMObject is AdjacencyCluster)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                }

                panels = adjacencyCluster.AddAirPanels(planes, spaces, Tolerance.MacroDistance, Tolerance.Angle, Tolerance.Distance, Tolerance.MacroDistance);
                if (sAMObject is AnalyticalModel)
                {
                    sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                }
                else
                {
                    sAMObject = adjacencyCluster;
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("airPanels");
            if (index != -1)
                dataAccess.SetDataList(index, panels);
        }
    }
}