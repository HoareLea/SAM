using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetValuesByReference : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8177e8a8-fb22-4f24-91f7-89378724b0b9");

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
        public SAMAnalyticalGetValuesByReference()
          : base("SAMAnalytical.GetValuesByReference", "SAMAnalytical.GetValuesByReference",
              "Get Values By Reference",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_reference", NickName = "_reference", Description = "Reference", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "values", NickName = "values", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            
            IAnalyticalObject analyticalObject = null;
            if(!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
                

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_reference");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string reference = null;
            if (!dataAccess.GetData(index, ref reference) || string.IsNullOrWhiteSpace(reference))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            IComplexReference complexReference = Core.Convert.ComplexReference(reference);
            if(complexReference == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<object> values =  Core.Query.Values(complexReference, adjacencyCluster);

            index = Params.IndexOfOutputParam("values");
            if (index != -1)
            {
                dataAccess.SetDataList(index, values);
            }
        }
    }
}