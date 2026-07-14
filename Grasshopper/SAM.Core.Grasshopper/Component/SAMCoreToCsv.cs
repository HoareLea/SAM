// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreToCsv : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2746ba21-b9ba-4b8b-9dc2-5f0f345170f1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreToCsv()
          : base("ToCsv", "ToCsv",
              "Writes SAM objects to Csv ",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject;
                param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_SAMObjects", NickName = "_SAMObjects", Description = "any SAM Objects", Access = GH_ParamAccess.list };
                param_GenericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_parameterNames", NickName = "_parameterNames", Description = "Csv file path", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeHeader_", NickName = "_includeHeader_", Description = "Include Header in csv", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "path_", NickName = "path_", Description = "Csv file path", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run, set to True to export JSON to given path", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Csv", NickName = "Csv", Description = "Csv", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            string path = null;
            index = Params.IndexOfInputParam("path_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref path);
            }

            bool includeHeader = true;
            index = Params.IndexOfInputParam("_includeHeader_");
            if (index == -1 || !dataAccess.GetData(index, ref includeHeader))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> propertyNames = new List<string>();
            index = Params.IndexOfInputParam("_parameterNames");
            if (index == -1 || !dataAccess.GetDataList(index, propertyNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_SAMObjects");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IJSAMObject> jSAMObjects = new List<IJSAMObject>();
            foreach (GH_ObjectWrapper gH_ObjectWrapper in objectWrapperList)
            {
                object @object = null;

                if (gH_ObjectWrapper.Value is IGooJSAMObject)
                    @object = ((IGooJSAMObject)gH_ObjectWrapper.Value).GetJSAMObject();
                else
                    @object = gH_ObjectWrapper.Value;

                if (@object is IJSAMObject)
                    jSAMObjects.Add((IJSAMObject)@object);
            }

            string csv = Core.Convert.ToCsv(jSAMObjects, propertyNames, true);

            if (!string.IsNullOrWhiteSpace(path))
                System.IO.File.WriteAllText(path, csv);

            int index_Csv = Params.IndexOfOutputParam("Csv");
            if (index_Csv != -1)
            {
                dataAccess.SetData(index_Csv, csv);
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, true);
            }
        }
    }
}
