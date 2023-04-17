using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSAMLibraryAddObjects : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d4f5697f-9768-4da3-a1a4-2b830fe5e54d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreSAMLibraryAddObjects()
          : base("SAMLibrary.AddObjects", "SAMLibrary.AddObjects",
              "Add Objects to SAMLibrary",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooJSAMObjectParam<ISAMLibrary>(), "_sAMLibrary", "_sAMLibrary", "SAM Core Library", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_objects", "_objects", "SAM Objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<ISAMLibrary>(), "SAMLibrary", "SAMLibrary", "SAM Core Library", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            ISAMLibrary sAMLibrary = null;

            if (!dataAccess.GetData(0, ref sAMLibrary))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(1, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMLibrary sAMLibrary_Result = sAMLibrary.Clone();

            foreach(SAMObject sAMObject in sAMObjects)
            {
                //sAMLibrary_Result.Add(sAMObject);
            }
                


            dataAccess.SetData(0, sAMLibrary_Result);
        }
    }
}