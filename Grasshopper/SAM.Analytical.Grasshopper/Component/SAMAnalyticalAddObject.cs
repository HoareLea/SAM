using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddObject : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("069a51b6-9db4-4bbd-8461-6709d12b162f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddObject()
          : base("SAMAnalytical.AddObject", "SAMAnalytical.AddObject",
              "Add Object to AdjacencyCluster or AnalyticalModel",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "objects_", NickName = "objects_", Description = "Object to be added", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            
            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("objects_");
            if (index != -1)
                dataAccess.GetDataList(index, sAMObjects);

            if(sAMObjects != null && sAMObjects.Count != 0)
            {
                if (sAMObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                    sAMObjects.FindAll(x => x is IMaterial).ForEach(x => analyticalModel.AddMaterial((IMaterial)x));
                    sAMObjects.FindAll(x => x is Profile).ForEach(x => analyticalModel.AddProfile((Profile)x));

                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    if(adjacencyCluster != null)
                    {
                        List<SAMObject> sAMObjects_AdjacencyCluster = sAMObjects.FindAll(x => adjacencyCluster.IsValid(x));
                        if(sAMObjects_AdjacencyCluster != null && sAMObjects_AdjacencyCluster.Count > 0)
                        {
                            sAMObjects_AdjacencyCluster.ForEach(x => adjacencyCluster.AddObject(x));
                            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
                        }
                    }

                    sAMObject = analyticalModel;
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                    foreach (SAMObject sAMObject_Object in sAMObjects)
                    {
                        if (adjacencyCluster.IsValid(sAMObject_Object))
                            adjacencyCluster.AddObject(sAMObject_Object);
                    }

                    sAMObject = adjacencyCluster;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            index = Params.IndexOfOutputParam("Analytical");
            if(index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}