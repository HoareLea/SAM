using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateFeatureShade : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("1cc59b32-082e-4967-b05c-b992cd3239d2");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateFeatureShade()
          : base("SAMAnalytical.CreateFeatureShade", "SAMAnalytical.CreateFeatureShade",
              "Create create FeatureShade",
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
                List<GH_SAMParam> result = [];

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Name", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData(string.Empty);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_description_", NickName = "_description_", Description = "Description", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData(string.Empty);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_surfaceHeight_", NickName = "_surfaceHeight_", Description = "Surface Height", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_surfaceWidth_", NickName = "_surfaceWidth_", Description = "Surface Width", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinDepth_", NickName = "_leftFinDepth_", Description = "Left Fin Depth", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinOffset_", NickName = "_leftFinOffset_", Description = "Left Fin Offset", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinTransmittance_", NickName = "_leftFinTransmittance_", Description = "Left Fin Transmittance", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinDepth_", NickName = "_rightFinDepth_", Description = "Right Fin Depth", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinOffset_", NickName = "_rightFinOffset_", Description = "Right Fin Offset", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinTransmittance_", NickName = "_rightFinTransmittance_", Description = "Right Fin Transmittance", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangDepth_", NickName = "_overhangDepth_", Description = "Overhang Depth", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangOffset_", NickName = "_overhangOffset_", Description = "Overhang Offset", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangTransmittance_", NickName = "_overhangTransmittance_", Description = "Overhang Transmittance", Optional = true, Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooFeatureShadeParam() { Name = "featureShade", NickName = "featureShade", Description = "SAM Analytical FeatureShade", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return [.. result];
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

            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            index = Params.IndexOfInputParam("_description_");
            string description = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            index = Params.IndexOfInputParam("_surfaceHeight_");
            double surfaceHeight = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref surfaceHeight);
            }

            index = Params.IndexOfInputParam("_surfaceWidth_");
            double surfaceWidth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref surfaceWidth);
            }

            index = Params.IndexOfInputParam("_leftFinDepth_");
            double leftFinDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinDepth);
            }

            index = Params.IndexOfInputParam("_leftFinOffset_");
            double leftFinOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinOffset);
            }

            index = Params.IndexOfInputParam("_leftFinTransmittance_");
            double leftFinTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinTransmittance);
            }

            index = Params.IndexOfInputParam("_rightFinDepth_");
            double rightFinDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinDepth);
            }

            index = Params.IndexOfInputParam("_rightFinOffset_");
            double rightFinOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinOffset);
            }

            index = Params.IndexOfInputParam("_rightFinTransmittance_");
            double rightFinTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinTransmittance);
            }

            index = Params.IndexOfInputParam("_overhangDepth_");
            double overhangDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangDepth);
            }

            index = Params.IndexOfInputParam("_overhangOffset_");
            double overhangOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangOffset);
            }

            index = Params.IndexOfInputParam("_overhangTransmittance_");
            double overhangTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangTransmittance);
            }

            FeatureShade featureShade = new (name, description, surfaceHeight, surfaceWidth, leftFinDepth, leftFinOffset, leftFinTransmittance, rightFinDepth, rightFinOffset, rightFinTransmittance, overhangDepth, overhangOffset, overhangTransmittance);

            index = Params.IndexOfOutputParam("featureShade");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooFeatureShade(featureShade));
            }
        }
    }
}