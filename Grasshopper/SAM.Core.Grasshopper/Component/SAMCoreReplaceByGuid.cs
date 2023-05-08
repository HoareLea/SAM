using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreReplaceByGuid: GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2d866bb0-298a-4971-b6ca-7fb375a7a243");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreReplaceByGuid()
          : base("SAMCore.ReplaceByGuid", "SAMCore.ReplaceByGuid",
              "Replace SAM Objects from one list by SAM Object from another list by matching Guid",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObjects", NickName = "_sAMobjects", Description = "SAM Objects", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_replacementSAMObjects", NickName = "_replacementSAMObjects", Description = "SAM Objects", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "sAMObjects", NickName = "sAMObjects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index;

            index = Params.IndexOfInputParam("_sAMObjects");

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("_replacementSAMObjects");

            List<SAMObject> replacementSAMObjects = new List<SAMObject>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, replacementSAMObjects);
            }

            if(replacementSAMObjects != null && replacementSAMObjects.Count != 0)
            {
                foreach (SAMObject sAMObject in replacementSAMObjects)
                {
                    int index_Temp = sAMObjects.FindIndex(x => x.Guid == sAMObject.Guid);
                    if(index_Temp == -1)
                    {
                        sAMObjects.Add(sAMObject);
                    }
                    else
                    {
                        sAMObjects[index_Temp] = sAMObject;
                    }
                }
            }

            index = Params.IndexOfOutputParam("sAMObjects");
            if (index != -1)
                dataAccess.SetDataList(index, sAMObjects);
        }
    }
}