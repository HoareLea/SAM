// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMechanicalSystemTypes : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0b370d90-3c1b-4b20-8181-0a751611652e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMechanicalSystemTypes()
          : base("SAMAnalytical.SystemTypes", "SAMAnalytical.SystemTypes",
              "Gets Analytical System Types",
              "SAM", "Analytical02")
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooSystemTypeLibraryParam gooSystemTypeLibraryParam = new GooSystemTypeLibraryParam() { Name = "_systemTypeLibrary_", NickName = "_systemTypeLibrary_", Description = "SAM SystemTypeLibrary", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooSystemTypeLibraryParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "Ventilation", NickName = "Ventilation", Description = "SAM Analytical Ventilation System Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "Heating", NickName = "Heating", Description = "SAM Analytical Heating System Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "Cooling", NickName = "Cooling", Description = "SAM Analytical Cooling System Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SystemTypeLibrary systemTypeLibrary = null;
            index = Params.IndexOfInputParam("_systemTypeLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref systemTypeLibrary);

            if (systemTypeLibrary == null)
                systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);

            InternalCondition internalCondition = null;

            if (sAMObject is InternalCondition)
                internalCondition = (InternalCondition)sAMObject;
            else if (sAMObject is Space)
                internalCondition = ((Space)sAMObject).InternalCondition;

            if (internalCondition == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("Ventilation");
            if (index != -1)
                dataAccess.SetData(index, new GooSystemType(internalCondition.GetSystemType<VentilationSystemType>(systemTypeLibrary)));
            index = Params.IndexOfOutputParam("Heating");
            if (index != -1)
                dataAccess.SetData(index, new GooSystemType(internalCondition.GetSystemType<HeatingSystemType>(systemTypeLibrary)));
            index = Params.IndexOfOutputParam("Cooling");
            if (index != -1)
                dataAccess.SetData(index, new GooSystemType(internalCondition.GetSystemType<CoolingSystemType>(systemTypeLibrary)));
        }
    }
}
