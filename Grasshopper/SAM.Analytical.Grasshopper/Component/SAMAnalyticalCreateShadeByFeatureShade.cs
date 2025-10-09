using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateShadeByFeatureShade : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f01ff376-ffc0-46d3-93ed-22ce7a49b001");

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
        public SAMAnalyticalCreateShadeByFeatureShade()
          : base("SAMAnalytical.CreateShadeByFeatureShade", "SAMAnalytical.CreateShadeByFeatureShade",
              "Create Shade by feature shade",
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

                GooApertureParam gooApertureParam = new GooApertureParam() { Name = "_aperture", NickName = "_aperture", Description = "SAM Aperture", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                GooFeatureShadeParam gooFeatureShadeParam = new GooFeatureShadeParam() { Name = "_featureShade", NickName = "_featureShade", Description = "SAM FeatureShade", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooFeatureShadeParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "SAM Name", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooFeatureShadeParam, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_description", NickName = "_description", Description = "SAM Description", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooFeatureShadeParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "surfaceHeight", NickName = "surfaceHeight", Description = "Surface Height [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "surfaceWidth", NickName = "surfaceWidth", Description = "Surface Width [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangDepth", NickName = "_overhangDepth", Description = "Overhang depth [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangOffset", NickName = "_overhangOffset", Description = "Overhang offset [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangTransmittance", NickName = "_overhangTransmittance", Description = "Overhang Transmittance", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinDepth", NickName = "_leftFinDepth", Description = "Left fin depth [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinOffset", NickName = "_leftFinOffset", Description = "Left fin offset [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinTransmittance", NickName = "_leftFinTransmittance", Description = "Left Fin Transmittance", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinDepth", NickName = "_rightFinDepth", Description = "Right fin depth [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinOffset", NickName = "_rightFinOffset", Description = "Right fin offset [m]", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinTransmittance", NickName = "_rightFinTransmittance", Description = "Right Fin Transmittance", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooFeatureShadeParam() { Name = "FeatureShade", NickName = "FeatureShade", Description = "FeatureShade", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Shades", NickName = "Shades", Description = "Shades", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            index = Params.IndexOfInputParam("_aperture");
            Aperture aperture = null;
            if (index == -1 || !dataAccess.GetData(index, ref aperture) || aperture == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_featureShade");
            FeatureShade featureShade = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref featureShade);
            }

            index = Params.IndexOfInputParam("_name");
            string name = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            index = Params.IndexOfInputParam("_description");
            string description = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            index = Params.IndexOfInputParam("surfaceHeight");
            double surfaceHeight = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref surfaceHeight);
            }

            index = Params.IndexOfInputParam("surfaceWidth");
            double surfaceWidth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref surfaceWidth);
            }

            index = Params.IndexOfInputParam("_overhangDepth");
            double overhangDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangDepth);
            }

            index = Params.IndexOfInputParam("_overhangOffset");
            double overhangOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangOffset);
            }

            index = Params.IndexOfInputParam("_overhangTransmittance");
            double overhangTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangTransmittance);
            }

            index = Params.IndexOfInputParam("_leftFinDepth");
            double leftFinDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinDepth);
            }

            index = Params.IndexOfInputParam("_leftFinOffset");
            double leftFinOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinOffset);
            }

            index = Params.IndexOfInputParam("_leftFinTransmittance");
            double leftFinTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinTransmittance);
            }


            index = Params.IndexOfInputParam("_rightFinDepth");
            double rightFinDepth = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinDepth);
            }

            index = Params.IndexOfInputParam("_rightFinOffset");
            double rightFinOffset = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinOffset);
            }

            index = Params.IndexOfInputParam("_rightFinTransmittance");
            double rightFinTransmittance = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinTransmittance);
            }

            FeatureShade featureShade_Result = new
            (
                name ?? featureShade?.Name,
                description ?? featureShade?.Description,
                !double.IsNaN(surfaceHeight) ? surfaceHeight : featureShade?.SurfaceHeight is null ? double.NaN : featureShade.SurfaceHeight,
                !double.IsNaN(surfaceWidth) ? surfaceWidth : featureShade?.SurfaceWidth is null ? double.NaN : featureShade.SurfaceWidth,
                !double.IsNaN(leftFinDepth) ? leftFinDepth : featureShade?.LeftFinDepth is null ? double.NaN : featureShade.LeftFinDepth,
                !double.IsNaN(leftFinOffset) ? leftFinOffset : featureShade?.LeftFinOffset is null ? double.NaN : featureShade.LeftFinOffset,
                !double.IsNaN(leftFinTransmittance) ? leftFinTransmittance : featureShade?.LeftFinTransmittance is null ? double.NaN : featureShade.LeftFinTransmittance,
                !double.IsNaN(rightFinDepth) ? rightFinDepth : featureShade?.RightFinDepth is null ? double.NaN : featureShade.RightFinDepth,
                !double.IsNaN(rightFinOffset) ? rightFinOffset : featureShade?.RightFinOffset is null ? double.NaN : featureShade.RightFinOffset,
                !double.IsNaN(rightFinTransmittance) ? rightFinTransmittance : featureShade?.RightFinTransmittance is null ? double.NaN : featureShade.RightFinTransmittance,
                !double.IsNaN(overhangDepth) ? overhangDepth : featureShade?.OverhangDepth is null ? double.NaN : featureShade.OverhangDepth,
                !double.IsNaN(overhangOffset) ? overhangOffset : featureShade?.OverhangOffset is null ? double.NaN : featureShade.OverhangOffset,
                !double.IsNaN(overhangTransmittance) ? overhangTransmittance : featureShade?.OverhangTransmittance is null ? double.NaN : featureShade.OverhangTransmittance
            );


            List<Panel> shades = Create.Panels_Shade(aperture, featureShade_Result);

            index = Params.IndexOfOutputParam("FeatureShade");
            if (index != -1)
            {
                dataAccess.SetData(index, featureShade_Result);
            }

            index = Params.IndexOfOutputParam("Shades");
            if (index != -1)
            {
                dataAccess.SetDataList(index, shades);
            }
        }
    }
}