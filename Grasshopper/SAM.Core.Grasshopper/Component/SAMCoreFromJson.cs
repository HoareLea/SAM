// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreFromJson : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6d595ec1-d617-424c-92fa-6332250d94e1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreFromJson()
          : base("FromJson", "FromJson",
              "Reads SAM Objects from Json",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_pathJSON", NickName = "_pathJSON", Description = "JSON file path including extension .json or .JSON", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "SAMObjects", NickName = "SAMObjects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("Successful");

            index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, false);
                }
                return;
            }
            if (!run)
                return;

            index = Params.IndexOfInputParam("_pathJSON");
            string pathOrJson = null;
            if (index == -1 || !dataAccess.GetData(index, ref pathOrJson))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null or Empty value for Json");
                index = Params.IndexOfOutputParam("SAMObjects");
                if (index != -1)
                {
                    dataAccess.SetData(index, null);
                }
                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, false);
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(pathOrJson))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null or Empty value for Json");
                index = Params.IndexOfOutputParam("SAMObjects");
                if (index != -1)
                {
                    dataAccess.SetData(index, null);
                }
                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, false);
                }
                return;
            }

            List<IJSAMObject> jSAMObjects = Core.Convert.ToSAM(pathOrJson);
            if (jSAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not parse Json to SAM");
                index = Params.IndexOfOutputParam("SAMObjects");
                if (index != -1)
                {
                    dataAccess.SetData(index, null);
                }
                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, false);
                }
                return;
            }

            index = Params.IndexOfOutputParam("SAMObjects");
            if (index != -1)
            {
                if (jSAMObjects.Count == 1)
                    dataAccess.SetData(index, jSAMObjects[0]);
                else
                    dataAccess.SetDataList(index, jSAMObjects);
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, true);
            }
        }
    }
}
