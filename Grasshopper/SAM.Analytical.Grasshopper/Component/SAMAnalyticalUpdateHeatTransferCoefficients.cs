using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateHeatTransferCoefficients : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d2238ee3-5cd1-4cc2-a89a-e7ff6d648200");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[1];
                result[0] = new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                return result;
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[3];
                result[0] = new GH_SAMParam(new GooAnalyticalModelParam() {Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooConstructionParam() { Name = "Constructions", NickName = "Constructions", Description = "Modified SAM Analytical Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                result[2] = new GH_SAMParam(new GooConstructionParam() { Name = "ApertureConstructions", NickName = "ApertureConstructions", Description = "Modified SAM Analytical ApertureConstructions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                return result;
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateHeatTransferCoefficients()
          : base("SAMAdjacencyCluster.UpdateHeatTransferCoefficients", "SAMAdjacencyCluster.UpdateHeatTransferCoefficients",
              "Updates Constructions Gas Material HeatTransferCoefficients",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(0, ref analyticalModel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Construction> constructions = null;
            List<ApertureConstruction> apertureConstructions = null;
            AnalyticalModel analyticalModel_Result = analyticalModel.UpdateHeatTransferCoefficients(out constructions, out apertureConstructions);

            int index = -1;

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel_Result));

            index = Params.IndexOfOutputParam("Constructions");
            if (index != -1)
                dataAccess.SetDataList(index, constructions?.ConvertAll(x => new GooConstruction(x)));

            index = Params.IndexOfOutputParam("ApertureConstructions");
            if (index != -1)
                dataAccess.SetDataList(index, apertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
        }
    }
}