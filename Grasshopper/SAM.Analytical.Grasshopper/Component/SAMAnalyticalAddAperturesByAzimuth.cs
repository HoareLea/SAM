using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByAzimuth : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("84d34834-8ce0-42cb-a3de-7366337bac4a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByAzimuth()
          : base("SAMAnalytical.AddAperturesByAzimuth", "SAMAnalytical.AddAperturesByAzimuth",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_ratios", "_ratios", "Ratios", GH_ParamAccess.list);
            inputParamManager.AddIntervalParameter("_azimuths", "_azimuths", "Azimuths Domains/Intervals if single number given ie. 90 it will be 0 to 90, so you need to make 90 To 90 in case just signle angle is required", GH_ParamAccess.list);

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
        ///// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(2, false);

            ApertureConstruction apertureConstruction = null;
            dataAccess.GetData(3, ref apertureConstruction);

            List<double> ratios = new List<double>();
            if (!dataAccess.GetDataList(1, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Rhino.Geometry.Interval> azimuths = new List<Rhino.Geometry.Interval>();
            if (!dataAccess.GetDataList(2, azimuths) || azimuths == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(azimuths.Count != ratios.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Dictionary<Rhino.Geometry.Interval, double> dictionary = new Dictionary<Rhino.Geometry.Interval, double>();
            for (int i = 0; i < ratios.Count; i++)
                if (azimuths[i] != null)
                    dictionary[azimuths[i]] = ratios[i];

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {

                //TODO: Rethink wthe way of filtering elements
                //if (((Panel)sAMObject).PanelType != PanelType.WallExternal)
                //    return;
                
                Panel panel = new Panel((Panel)sAMObject);

                double azimuth = panel.Azimuth();
                if (double.IsNaN(azimuth))
                    return;

                double ratio;
                if (!Core.Grasshopper.Query.TryGetValue<double>(dictionary, azimuth, out ratio))
                    return;

                ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                if (apertureConstruction_Temp == null)
                    apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);

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

                    double azimuth = panel.Azimuth();
                    if (double.IsNaN(azimuth))
                        continue;

                    double ratio;
                    if (!Core.Grasshopper.Query.TryGetValue(dictionary, azimuth, out ratio))
                        continue;

                    Panel panel_New = new Panel(panel);

                    ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                    if (apertureConstruction_Temp == null)
                        apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel_New, ApertureType.Window);

                    Aperture aperture = panel_New.AddAperture(apertureConstruction_Temp, ratio);
                    if (aperture == null)
                        continue;

                    tuples_Result.Add(new Tuple<Panel, Aperture>(panel_New, aperture));
                    adjacencyCluster.AddObject(panel_New);
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