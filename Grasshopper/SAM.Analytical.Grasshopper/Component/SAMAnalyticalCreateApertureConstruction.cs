using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateApertureConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("92dadf21-4b38-4f76-8e24-3de677eaeb1a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateApertureConstruction()
          : base("SAMAnalytical.CreateApertureConstruction", "SAMAnalytical.CreateApertureConstruction",
              "Create Aperture Construction \n*The layers should be ordered from inside to outside",
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

                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstruction_", NickName = "apertureConstruction_", Description = "Source SAM Analytical ApertureConstruction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Space Name, Default = Space_Default", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "apertureType_", NickName = "apertureType_", Description = "ApertureType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "paneConstructionLayers_", NickName = "paneConstructionLayers_", Description = "SAM Pane Contruction Layers \n* order from Inside to Outside", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "frameConstructionLayers_", NickName = "frameConstructionLayers_", Description = "SAM Frame Contruction Layers", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstruction", NickName = "apertureConstruction", Description = "SAM Analytical ApertureConstruction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ApertureConstruction apertureConstruction = null;
            index = Params.IndexOfInputParam("apertureConstruction_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstruction);
            }

            if (apertureConstruction == null)
            {
                apertureConstruction = Analytical.Query.DefaultApertureConstruction(PanelType.WallExternal, ApertureType.Window);
            }
            else
            {
                apertureConstruction = new ApertureConstruction(Guid.NewGuid(), apertureConstruction, apertureConstruction.Name);
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1 && dataAccess.GetData(index, ref name) && name != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, name);
            }

            string apertureType_String = null;
            index = Params.IndexOfInputParam("apertureType_");
            if(index != -1 && dataAccess.GetData(index, ref apertureType_String) && !string.IsNullOrWhiteSpace(apertureType_String))
            {
                ApertureType apertureType = Analytical.Query.ApertureType(apertureType_String);
                if(apertureType != ApertureType.Undefined)
                {
                    apertureConstruction = new ApertureConstruction(apertureConstruction, apertureType);
                }
            }

            index = Params.IndexOfInputParam("paneConstructionLayers_");
            if(index != -1)
            {
                List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();
                if (dataAccess.GetDataList(index, constructionLayers))
                {
                    apertureConstruction = new ApertureConstruction(apertureConstruction, constructionLayers, apertureConstruction.FrameConstructionLayers);
                }
            }

            index = Params.IndexOfInputParam("frameConstructionLayers_");
            if (index != -1)
            {
                List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();
                if (dataAccess.GetDataList(index, constructionLayers))
                {
                    apertureConstruction = new ApertureConstruction(apertureConstruction, apertureConstruction.PaneConstructionLayers, constructionLayers);
                }
            }

            index = Params.IndexOfOutputParam("apertureConstruction");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooApertureConstruction(apertureConstruction));
            }

        }
    }
}