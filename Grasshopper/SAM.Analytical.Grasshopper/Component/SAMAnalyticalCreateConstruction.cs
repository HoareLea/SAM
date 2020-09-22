using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstruction : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("75ebc676-3401-481c-90e5-8f767b8b215b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstruction()
          : base("SAMAnalytical.CreateConstruction", "SAMAnalyticalCreate.Construction",
              "Create Construction, if nothing connect default values: _name = Basic Roof: SIM_EXT_SLD_Roof DA01 ",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            Construction construction = Analytical.Query.DefaultConstruction(PanelType.Roof);
            
            inputParamManager.AddTextParameter("_name_", "_name_", "Contruction Name", GH_ParamAccess.item, construction.Name);

            GooConstructionLayerParam gooConstructionLayerParam = new GooConstructionLayerParam();
            gooConstructionLayerParam.PersistentData.AppendRange(construction.ConstructionLayers.ConvertAll(x => new GooConstructionLayer(x)));
            inputParamManager.AddParameter(gooConstructionLayerParam, "constructionLayers_", "constructionLayers_", "SAM Contruction Layers", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionParam(), "Construction", "Construction", "SAM Analytical Construction", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData<string>(0, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            dataAccess.GetDataList(1, objectWrappers);

            List<ConstructionLayer> constructionLayers = null;
            if(objectWrappers != null)
            {
                constructionLayers = new List<ConstructionLayer>();
                foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    if(objectWrapper.Value is ConstructionLayer)
                    {
                        constructionLayers.Add((ConstructionLayer)objectWrapper.Value);
                    }
                    if(objectWrapper.Value is GooConstructionLayer)
                    {
                        constructionLayers.Add(((GooConstructionLayer)objectWrapper.Value).Value);

                    }
                }
            }

            dataAccess.SetData(0, new GooConstruction(new Construction(name, constructionLayers)));
        }
    }
}