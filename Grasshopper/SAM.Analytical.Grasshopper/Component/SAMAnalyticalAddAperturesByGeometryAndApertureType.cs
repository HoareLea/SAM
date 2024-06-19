using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByGeometryAndApertureType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("14802f03-04a2-4179-8305-6b43f5fc19d4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByGeometryAndApertureType()
          : base("SAMAnalytical.AddAperturesByGeometryAndApertureType", "SAMAnalytical.AddAperturesByGeometryAndApertureType",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            index = inputParamManager.AddGenericParameter("_geometries_", "_geometries_", "Geometry incl Rhino geometry \nwhen using Polyline frame come from layer thickenss so default 0.05m if surface connected from two closed curves, thickness is frame hole will be pane", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);

            index = inputParamManager.AddTextParameter("_apertureType_", "_apertureType_", "SAM Analytical ApertureType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddNumberParameter("maxDistance_", "maxDistance_", "Maximal Distance", GH_ParamAccess.item, 0.1);
            inputParamManager.AddBooleanParameter("trimGeometry_", "trimGeometry_", "Trim Aperture Geometry", GH_ParamAccess.item, true);
            inputParamManager.AddNumberParameter("minArea_", "minArea_", "Minimal Acceptable area of Aperture", GH_ParamAccess.item, Tolerance.MacroDistance);

            index = inputParamManager.AddNumberParameter("frameWidth_", "frameWidth_", "Frame Width [m] \n*Min value is sum of frame layer thicknesses Default 0.05m so unable to dopt below this value unless. \nIf you want zero remove frame layer in aperture construction ", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddNumberParameter("framePercentage_", "framePercentage_", "Frame Percentage [%] \nsee frameWidth_ description \nuse only one input frameWidth_ or framePercentage_ ", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            dataAccess.GetDataList(0, objectWrappers);

            List<Face3D> face3Ds = new List<Face3D>();

            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp) && face3Ds_Temp != null)
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            //List<IClosedPlanar3D> closedPlanar3Ds = Geometry.Spatial.Query.ClosedPlanar3Ds(geometry3Ds);

            bool trimGeometry = true;
            if (!dataAccess.GetData(4, ref trimGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ApertureType apertureType = ApertureType.Window;

            string apertureTypeString = null;
            dataAccess.GetData(2, ref apertureTypeString);
            if(!string.IsNullOrWhiteSpace(apertureTypeString))
            {
                if (!Enum.TryParse(apertureTypeString, out apertureType))
                    apertureType = ApertureType.Window;
            }

            double maxDistance = Tolerance.MacroDistance;
            dataAccess.GetData(3, ref maxDistance);

            double minArea = Tolerance.MacroDistance;
            dataAccess.GetData(5, ref minArea);

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(1, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool framePercentage = true;
            List<double> values = new List<double>();
            if (dataAccess.GetDataList(6, values))
            {
                framePercentage = false;
            }
            if(framePercentage)
            {
                dataAccess.GetDataList(7, values);
            }

            if (values != null && values.Count != 0)
            {
                face3Ds = Query.Offset(face3Ds, values, framePercentage);
            }

            if (sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);

                List<Aperture> apertures = new List<Aperture>();

                if(face3Ds != null)
                {
                    foreach (Face3D face3D in face3Ds)
                    {
                        ApertureConstruction apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, apertureType);

                        List<Aperture> apertures_Temp = panel.AddApertures(apertureConstruction_Temp, face3D, trimGeometry, minArea, maxDistance);
                        if (apertures_Temp != null)
                            apertures.AddRange(apertures_Temp);
                    }
                }

                dataAccess.SetData(0, panel);
                dataAccess.SetDataList(1, apertures.ConvertAll(x => new GooAperture(x)));
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

            List<Tuple<Panel, Aperture>> tuples_Result = null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null)
            {
                List<Tuple<BoundingBox3D, IClosedPlanar3D>> tuples = new List<Tuple<BoundingBox3D, IClosedPlanar3D>>();
                foreach (IClosedPlanar3D closedPlanar3D in face3Ds)
                {
                    if (closedPlanar3D == null)
                        continue;

                    tuples.Add(new Tuple<BoundingBox3D, IClosedPlanar3D>(closedPlanar3D.GetBoundingBox(maxDistance), closedPlanar3D));
                }

                tuples_Result = new List<Tuple<Panel, Aperture>>();
                for(int i=0; i < panels.Count; i++)
                {
                    Panel panel = panels[i];
                    BoundingBox3D boundingBox3D = panel.GetBoundingBox(maxDistance);

                    Panel panel_Temp = null;

                    foreach (Tuple<BoundingBox3D, IClosedPlanar3D> tuple in tuples)
                    {
                        if (!boundingBox3D.InRange(tuple.Item1))
                            continue;

                        if (!panel.ApertureHost(tuple.Item2, minArea, maxDistance))
                            continue;

                        ApertureConstruction apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, apertureType);

                        if (apertureConstruction_Temp == null)
                            continue;

                        if(panel_Temp == null)
                            panel_Temp = Create.Panel(panel);

                        List<Aperture> apertures_Temp = panel_Temp.AddApertures(apertureConstruction_Temp, tuple.Item2, trimGeometry, minArea, maxDistance);
                        if (apertures_Temp == null)
                            continue;

                        apertures_Temp.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel_Temp, x)));
                    }

                    if(panel_Temp != null)
                        adjacencyCluster.AddObject(panel_Temp);
                }
            }

            if (analyticalModel != null)
            {
                AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                dataAccess.SetData(0, analyticalModel_Result);
            }
            else
            {
                dataAccess.SetData(0, adjacencyCluster);
            }

            dataAccess.SetDataList(1, tuples_Result?.ConvertAll(x => new GooAperture(x.Item2)));
        }
    }
}