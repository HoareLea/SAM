// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalHeatTransferCoefficientByDefaultGasType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7c3c44e3-3a12-40ff-8ace-742e39d92e7d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalHeatTransferCoefficientByDefaultGasType()
          : base("SAMAnalytical.HeatTransferCoefficientByDefaultGasType", "SAMAnalyticalCreate.HeatTransferCoefficientByDefaultGasType",
              "Calculate Heat Transfer Coefficient By DefaultGasType",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject;
                param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_defaultGasType_", NickName = "_defaultGasType_", Description = "DefaultGasType", Access = GH_ParamAccess.item, Optional = true };
                param_GenericObject.PersistentData.Append(new GH_ObjectWrapper(DefaultGasType.Argon.ToString()));
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_width_", NickName = "_width_", Description = "Cavity width [m]", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.012);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_meanTemperature_", NickName = "_meanTemperature_", Description = "Mean Temperature [K]", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(283);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_temperatureDifference_", NickName = "_temperatureDifference_", Description = "Mean temperature difference across the cavity [K]", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(15);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_angle_", NickName = "_angle_", Description = "Angle in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(System.Math.PI / 2);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "HeatTransferCoefficient", NickName = "HeatTransferCoefficient", Description = "Heat Transfer Coefficient [W/m2K]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_defaultGasType_");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DefaultGasType defaultGasType = DefaultGasType.Undefined;
            if (!Enum.TryParse(objectWrapper.Value?.ToString(), out defaultGasType) || defaultGasType == DefaultGasType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double width = double.NaN;
            index = Params.IndexOfInputParam("_width_");
            if (index == -1 || !dataAccess.GetData(index, ref width) || double.IsNaN(width))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double meanTemperature = double.NaN;
            index = Params.IndexOfInputParam("_meanTemperature_");
            if (index == -1 || !dataAccess.GetData(index, ref meanTemperature) || double.IsNaN(meanTemperature))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double temperatureDifference = double.NaN;
            index = Params.IndexOfInputParam("_temperatureDifference_");
            if (index == -1 || !dataAccess.GetData(index, ref temperatureDifference) || double.IsNaN(temperatureDifference))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double angle = double.NaN;
            index = Params.IndexOfInputParam("_angle_");
            if (index == -1 || !dataAccess.GetData(index, ref angle) || double.IsNaN(angle))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GasMaterial gasMaterial = Analytical.Query.DefaultGasMaterial(defaultGasType);

            double HeatTransferCoefficient = Analytical.Query.HeatTransferCoefficient(gasMaterial, temperatureDifference, width, meanTemperature, angle);

            index = Params.IndexOfOutputParam("HeatTransferCoefficient");
            if (index != -1)
                dataAccess.SetData(index, HeatTransferCoefficient);
        }
    }
}
