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
    public class SAMAnalyticalAddApertures : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("64ee6364-37ba-4554-9ec2-39980afa92c3");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddApertures()
          : base("SAMAnalytical.AddApertures", "SAMAnalytical.AddApertures",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "_geometry", "Geometry incl Rhino geometry", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);

            int index = inputParamManager.AddParameter(new GooApertureConstructionParam(), "_apertureConstruction_", "_apertureConstruction_", "SAM Analytical Aperture Construction", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddNumberParameter("maxDistance_", "maxDistance_", "Maximal Distance", GH_ParamAccess.item, 0.1);
            inputParamManager.AddBooleanParameter("trimGeometry_", "trimGeometry_", "Trim Aperture Geometry", GH_ParamAccess.item, true);
            inputParamManager.AddNumberParameter("minArea_", "minArea_", "Minimal Acceptable area of Aperture", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
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
            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList(0, objects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            foreach (object @object in objects)
            {
                List<ISAMGeometry3D> geometry3Ds_Temp = null;
                if (@object is IGH_GeometricGoo)
                {
                    geometry3Ds_Temp = ((IGH_GeometricGoo)@object).ToSAM(true).Cast<ISAMGeometry3D>().ToList();
                }
                else if (@object is GH_ObjectWrapper)
                {
                    GH_ObjectWrapper objectWrapper_Temp = ((GH_ObjectWrapper)@object);
                    if (objectWrapper_Temp.Value is ISAMGeometry3D)
                        geometry3Ds_Temp = new List<ISAMGeometry3D>() { (ISAMGeometry3D)objectWrapper_Temp.Value };
                    else if (objectWrapper_Temp.Value is Geometry.Planar.ISAMGeometry2D)
                        geometry3Ds_Temp = new List<ISAMGeometry3D>() { Plane.WorldXY.Convert(objectWrapper_Temp.Value as dynamic) };
                }
                else if (@object is IGH_Goo)
                {
                    Geometry.ISAMGeometry sAMGeometry = (@object as dynamic).Value as Geometry.ISAMGeometry;
                    if (sAMGeometry is ISAMGeometry3D)
                        geometry3Ds_Temp = new List<ISAMGeometry3D>() { (ISAMGeometry3D)sAMGeometry };
                    else if (sAMGeometry is Geometry.Planar.ISAMGeometry2D)
                        geometry3Ds_Temp = new List<ISAMGeometry3D>() { Plane.WorldXY.Convert(sAMGeometry as dynamic) };
                }

                if (geometry3Ds_Temp != null && geometry3Ds_Temp.Count > 0)
                    geometry3Ds.AddRange(geometry3Ds_Temp);
            }

            List<IClosedPlanar3D> closedPlanar3Ds = Geometry.Spatial.Query.ClosedPlanar3Ds(geometry3Ds);
            if (closedPlanar3Ds == null || closedPlanar3Ds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool trimGeometry = true;
            if (!dataAccess.GetData(4, ref trimGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ApertureConstruction apertureConstruction = null;
            dataAccess.GetData(2, ref apertureConstruction);

            double maxDistance = Core.Tolerance.MacroDistance;
            dataAccess.GetData(3, ref maxDistance);

            double minArea = Core.Tolerance.MacroDistance;
            dataAccess.GetData(5, ref minArea);

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(1, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                Panel panel = new Panel((Panel)sAMObject);

                List<Aperture> apertures = new List<Aperture>();

                foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                {
                    ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                    if (apertureConstruction_Temp == null)
                        apertureConstruction_Temp = Analytical.Query.ApertureConstruction(panel, ApertureType.Window);

                    Aperture aperture = panel.AddAperture(apertureConstruction_Temp, closedPlanar3D, trimGeometry, minArea, maxDistance);
                    if (aperture != null)
                        apertures.Add(aperture);
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

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null)
            {
                List<Tuple<BoundingBox3D, IClosedPlanar3D>> tuples = new List<Tuple<BoundingBox3D, IClosedPlanar3D>>();
                foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                {
                    if (closedPlanar3D == null)
                        continue;

                    tuples.Add(new Tuple<BoundingBox3D, IClosedPlanar3D>(closedPlanar3D.GetBoundingBox(maxDistance), closedPlanar3D));
                }

                List<Tuple<Panel, Aperture>> tuples_Result = new List<Tuple<Panel, Aperture>>();
                foreach (Panel panel in panels)
                {
                    BoundingBox3D boundingBox3D = panel.GetBoundingBox(maxDistance);

                    foreach (Tuple<BoundingBox3D, IClosedPlanar3D> tuple in tuples)
                    {
                        if (!boundingBox3D.InRange(tuple.Item1))
                            continue;

                        if (!panel.ApertureHost(tuple.Item2, minArea, maxDistance))
                            continue;

                        ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                        if (apertureConstruction_Temp == null)
                            apertureConstruction_Temp = Analytical.Query.ApertureConstruction(panel, ApertureType.Window);

                        if (apertureConstruction_Temp == null)
                            continue;

                        Panel panel_Temp = new Panel(panel);

                        Aperture aperture = panel_Temp.AddAperture(apertureConstruction_Temp, tuple.Item2, trimGeometry, minArea, maxDistance);
                        if (aperture == null)
                            continue;

                        adjacencyCluster.AddObject(panel_Temp);
                        tuples_Result.Add(new Tuple<Panel, Aperture>(panel_Temp, aperture));
                    }
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