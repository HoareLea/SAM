// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSAMLibraryAddObjects : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d4f5697f-9768-4da3-a1a4-2b830fe5e54d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<ISAMLibrary>() { Name = "_sAMLibrary", NickName = "_sAMLibrary", Description = "SAM Core Library", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_objects", NickName = "_objects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<ISAMLibrary>() { Name = "SAMLibrary", NickName = "SAMLibrary", Description = "SAM Core Library", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ISAMLibrary sAMLibrary = null;
            index = Params.IndexOfInputParam("_sAMLibrary");
            if (index == -1 || !dataAccess.GetData(index, ref sAMLibrary))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("_objects");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMLibrary sAMLibrary_Result = sAMLibrary.Clone();

            foreach (SAMObject sAMObject in sAMObjects)
            {
                //sAMLibrary_Result.Add(sAMObject);
            }



            index = Params.IndexOfOutputParam("SAMLibrary");
            if (index != -1)
            {
                dataAccess.SetData(index, sAMLibrary_Result);
            }
        }
    }
}
