using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetApertures : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a5a1e858-0db7-46db-bf50-da74f24a5a8f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetApertures()
          : base("SAMAnalytical.GetApertures", "SAMAnalytical.GetApertures",
              "Get Apertures from SAM Analytical Model",
              "SAM", "Analytical01")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_objects", NickName = "_objects", Description = "Objects such as Point, SAM Analytical Construction, Aperture etc.", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooApertureParam { Name = "apertures", NickName = "aperutes", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            
            SAMObject sAMObject_Temp = null;
            if(!dataAccess.GetData(index, ref sAMObject_Temp) || sAMObject_Temp == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (sAMObject_Temp is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)sAMObject_Temp;
            }
            else if (sAMObject_Temp is AnalyticalModel)
            {
                analyticalModel = (AnalyticalModel)sAMObject_Temp;
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }
                

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_objects");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList(index, objects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DataTree<GooAperture> dataTree = new DataTree<GooAperture>();
            List<Tuple<GH_Path, Geometry.Spatial.Point3D>> tuples = new List<Tuple<GH_Path, Geometry.Spatial.Point3D>>();

            for (int i=0; i < objects.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                object @object = objects[i];
                if (@object is IGH_Goo)
                    @object = ((dynamic)@object).Value;

                if (@object is Aperture)
                {
                    Panel panel = adjacencyCluster.GetPanel((Aperture)@object);
                    if(panel != null)
                    {
                        Aperture aperture = panel.GetAperture(((Aperture)@object).Guid);

                        dataTree.Add(new GooAperture(aperture), path);
                    }
                }
                else if (@object is ApertureConstruction)
                {
                    List<Aperture> apertures = adjacencyCluster.GetApertures((ApertureConstruction)@object);
                    apertures?.ForEach(x => dataTree.Add(new GooAperture(x), path));
                }
                else if (@object is Construction)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels((Construction)@object);
                    if(panels != null)
                    {
                        foreach (Panel panel in panels)
                        {
                            List<Aperture> apertures = panel.GetApertures((ApertureConstruction)@object);
                            if(apertures != null)
                            {
                                apertures?.ForEach(x => dataTree.Add(new GooAperture(x), path));
                            }
                        }
                    }
                    
                }
                else if (@object is Geometry.Spatial.Point3D)
                {
                    tuples.Add(new Tuple<GH_Path, Geometry.Spatial.Point3D>(path, (Geometry.Spatial.Point3D)@object));
                }
                else if (@object is Geometry.Planar.Point2D)
                {
                    Geometry.Planar.Point2D point2D = (Geometry.Planar.Point2D)@object;
                    tuples.Add(new Tuple<GH_Path, Geometry.Spatial.Point3D>(path, new Geometry.Spatial.Point3D(point2D.X, point2D.Y, 0)));
                }
                else if (@object is GH_Point)
                {
                    Geometry.Spatial.Point3D point3D = Geometry.Grasshopper.Convert.ToSAM((GH_Point)@object);
                    if (point3D != null)
                        tuples.Add(new Tuple<GH_Path, Geometry.Spatial.Point3D>(path, point3D));
                }
                else if(@object is global::Rhino.Geometry.Point3d)
                {
                    Geometry.Spatial.Point3D point3D = Geometry.Rhino.Convert.ToSAM((global::Rhino.Geometry.Point3d)@object);
                    if (point3D != null)
                        tuples.Add(new Tuple<GH_Path, Geometry.Spatial.Point3D>(path, point3D));
                }
            }

            if(tuples != null && tuples.Count != 0)
            {
                List<Panel> panels = adjacencyCluster.GetPanels();
                if(panels != null && panels.Count > 0)
                {
                    foreach (Tuple<GH_Path, Geometry.Spatial.Point3D> tuple in tuples)
                    {
                        Geometry.Spatial.Point3D point3D = tuple.Item2;
                        if (point3D == null || tuple.Item1 == null)
                            continue;

                        foreach (Panel panel in panels)
                        {
                            List<Aperture> apertures = panel.Apertures;
                            if(apertures != null)
                            {
                                foreach(Aperture aperture in apertures)
                                {
                                    Geometry.Spatial.IClosedPlanar3D closedPlanar3D = aperture?.Face3D.GetExternalEdge3D();
                                    if(closedPlanar3D != null)
                                    {
                                        Geometry.Spatial.Face3D face3D = new Geometry.Spatial.Face3D(closedPlanar3D);
                                        if (face3D == null)
                                        {
                                            continue;
                                        }

                                        if (face3D.InRange(point3D) || face3D.Inside(point3D))
                                        {
                                            dataTree.Add(new GooAperture(aperture), tuple.Item1);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree);
        }
    }
}