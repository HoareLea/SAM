using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByRatio : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6afa8baf-7cc3-4993-a6ec-43e5b674ff00");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByRatio()
          : base("SAMAnalytical.AddAperturesByRatio", "SAMAnalytical.AddAperturesByRatio",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddNumberParameter("_ratio", "_ratio", "Ratio", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);

            int index = inputParamManager.AddParameter(new GooApertureConstructionParam(), "_apertureConstruction_", "_apertureConstruction_", "SAM Analytical Aperture Construction", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(2, false);

            ApertureConstruction apertureConstruction = null;
            dataAccess.GetData(2, ref apertureConstruction);

            double ratio = double.NaN;
            if (!dataAccess.GetData(0, ref ratio))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(1, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                if (((Panel)sAMObject).PanelType != PanelType.WallExternal)
                    return;
                
                Panel panel = new Panel((Panel)sAMObject);

                ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                if (apertureConstruction_Temp == null)
                    apertureConstruction_Temp = Analytical.Query.ApertureConstruction(panel, ApertureType.Window);

                Aperture aperture = panel.AddAperture(apertureConstruction_Temp, ratio);

                dataAccess.SetData(0, panel);
                dataAccess.SetDataList(1, new List<GooAperture>() { new GooAperture(aperture) });
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)sAMObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null)
            {
                List<Tuple<Panel, Aperture>> tuples_Result = new List<Tuple<Panel, Aperture>>();

                foreach (Panel panel in panels)
                {
                    if (panel.PanelType != PanelType.WallExternal)
                        continue;

                    ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                    if (apertureConstruction_Temp == null)
                        apertureConstruction_Temp = Analytical.Query.ApertureConstruction(panel, ApertureType.Window);

                    Aperture aperture = panel.AddAperture(apertureConstruction_Temp, ratio);
                    if (aperture == null)
                        continue;

                    tuples_Result.Add(new Tuple<Panel, Aperture>(panel, aperture));
                    adjacencyCluster.AddObject(panel);
                }

                if(analyticalModel != null)
                {
                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(0, analyticalModel_Result);
                }
                else
                {
                    dataAccess.SetData(0, adjacencyCluster);
                }
                
                dataAccess.SetDataList(1, tuples_Result.ConvertAll(x => new GooAperture(x.Item2)));
            }
        }
    }
}