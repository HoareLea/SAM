using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper;
using SAM.Architectural;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateLevels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c1837abc-924f-405f-9f50-e613743d82c6");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateLevels()
          : base("SAMAnalytical.CreateLevels", "SAMAnalytical.CreateLevels",
              "Create SAM Architectural Levels from \n Panel, Aperture, \n * Partion, Opening ",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_", NickName = "_analytical", Description = "SAM Analytical", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            //IList<IGH_Param> @params = Params.Input[0].Sources;
            //if(@params != null && @params.Count != 0)
            //{
            //    Params.Input[0].NickName = Params.Input[0].Sources[0].NickName;
            //    Params.Input[0].Name = Params.Input[0].Sources[0].Name;
            //}

            List<IAnalyticalObject> analyticalObjects = new List<IAnalyticalObject>();
            if (!dataAccess.GetDataList(0, analyticalObjects) || analyticalObjects == null)
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

            HashSet<double> elevations_Min = new HashSet<double>();
            HashSet<double> elevations_Max = new HashSet<double>();
            foreach (IAnalyticalObject analyticalObject in analyticalObjects)
            {
                if(analyticalObject is Panel)
                {
                    Panel panel = (Panel)analyticalObject;

                    elevations_Min.Add(Core.Query.Round(panel.MinElevation(), tolerance));
                    elevations_Min.Add(panel.MaxElevation());

                    continue;
                }

                if (analyticalObject is Space)
                {
                    Space space = (Space)analyticalObject;

                    elevations_Min.Add(Core.Query.Round(space.Location.Z, tolerance));
                    elevations_Min.Add(space.Location.Z);

                    continue;
                }

                if (analyticalObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)analyticalObject;

                    List<Panel> panels = adjacencyCluster.GetPanels();
                    if(panels != null)
                    {
                        foreach(Panel panel in panels)
                        {
                            elevations_Min.Add(Core.Query.Round(panel.MinElevation(), tolerance));
                            elevations_Min.Add(panel.MaxElevation());
                        }
                    }
                    continue;
                }

                if (analyticalObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = (AnalyticalModel)analyticalObject;

                    List<Panel> panels = analyticalModel.AdjacencyCluster?.GetPanels();
                    if (panels != null)
                    {
                        foreach (Panel panel in panels)
                        {
                            elevations_Min.Add(Core.Query.Round(panel.MinElevation(), tolerance));
                            elevations_Min.Add(panel.MaxElevation());
                        }
                    }
                    continue;
                }

            }

            index = Params.IndexOfOutputParam("levels");
            if(index != -1)
            {
                List<Level> levels = elevations_Min.ToList().ConvertAll(x => Architectural.Create.Level(x));
                dataAccess.SetDataList(index, levels?.ConvertAll(x => new GooLevel(x)));
            }

            index = Params.IndexOfOutputParam("topLevel");
            if(index != -1)
            {
                dataAccess.SetData(index, Architectural.Create.Level(Core.Query.Round(elevations_Max.ToList().Max(), tolerance)));
            }

        }
    }
}