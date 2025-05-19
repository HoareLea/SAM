using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveAirMovementObjects : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("813800cd-6fbf-4197-bad7-6a097565541d");

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
        public SAMAnalyticalRemoveAirMovementObjects()
          : base("SAMAnalytical.RemoveIZAMs", "SAMAnalytical.RemoveIZAMs",
              "Remove IZAMs",
              "SAM", "Analytical03")
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

                Param_Boolean paramBoolean;

                paramBoolean = new Param_Boolean() { Name = "removeAHU_", NickName = "removeAHU_", Description = "Remove AHU", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                paramBoolean = new Param_Boolean() { Name = "run_", NickName = "run_", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Guid { Name = "guids", NickName = "guids", Description = "Guids of removed objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Boolean() { Name = "successful", NickName = "successful", Description = "Successful", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("run_");
            bool run = false;
            if (!dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!run)
            {
                return;
            }

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool removeAHU = false;
            index = Params.IndexOfInputParam("removeAHU_");
            if(index != -1)
            {
                if(!dataAccess.GetData(index, ref removeAHU))
                {
                    removeAHU = false;
                }
            }

            bool successful = false;
            List<Guid> guids = new List<Guid>();

            AdjacencyCluster adjacencyCluster = null;

            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);

                guids = adjacencyCluster.RemoveAirMovementObjects<IAirMovementObject>();
                successful = guids != null && guids.Count != 0;
            }
            else if(analyticalObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = (AnalyticalModel)analyticalObject;

                adjacencyCluster = analyticalModel.AdjacencyCluster;
                if(adjacencyCluster != null)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                }
            }

            if(adjacencyCluster != null)
            {
                if (removeAHU)
                {
                    List<AirHandlingUnitAirMovement> airHandlingUnitAirMovements = adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>();
                    if (airHandlingUnitAirMovements != null)
                    {
                        foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in airHandlingUnitAirMovements)
                        {
                            List<AirHandlingUnit> airHandlingUnits = adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement);
                            if (airHandlingUnits != null)
                            {
                                List<Guid> guids_AirHandlingUnits = adjacencyCluster.Remove(airHandlingUnits);
                                if (guids_AirHandlingUnits != null)
                                {
                                    guids.AddRange(guids_AirHandlingUnits);
                                }
                            }
                        }
                    }
                }

                List<Guid> guids_RemoveAirMovementObject = adjacencyCluster.RemoveAirMovementObjects<IAirMovementObject>();
                if (guids_RemoveAirMovementObject != null)
                {
                    guids.AddRange(guids_RemoveAirMovementObject);
                }

                successful = guids != null && guids.Count != 0;

                if (successful && analyticalObject is AnalyticalModel)
                {
                    analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("guids");
            if (index != -1)
            {
                dataAccess.SetDataList(index, guids);
            }

            index = Params.IndexOfOutputParam("successful");
            if (index != -1)
            {
                dataAccess.SetData(index, successful);
            }
        }
    }
}