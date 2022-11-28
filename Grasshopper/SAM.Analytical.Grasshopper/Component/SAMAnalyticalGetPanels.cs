using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d1914180-0be2-4982-8f26-c290889535c4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetPanels()
          : base("SAMAnalytical.GetPanels", "SAMAnalytical.GetPanels",
              "Get Panels from SAM Analytical Model",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_objects", NickName = "_objects", Description = "Objects such as Point, SAM Analytical Construction, Aperture etc.", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item};
                number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooPanelParam { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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
            
            IAnalyticalObject analyticalObject_Temp = null;
            if(!dataAccess.GetData(index, ref analyticalObject_Temp) || analyticalObject_Temp == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (analyticalObject_Temp is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject_Temp;
            }
            else if (analyticalObject_Temp is AnalyticalModel)
            {
                analyticalModel = (AnalyticalModel)analyticalObject_Temp;
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

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(index, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = Tolerance.MacroDistance;
            index = Params.IndexOfInputParam("_tolerance_");
            if(index != -1)
            {
                if(!dataAccess.GetData(index, ref tolerance))
                {
                    tolerance = Tolerance.MacroDistance;
                }
            }

            DataTree<GooPanel> dataTree = new DataTree<GooPanel>();
            List<Tuple<GH_Path, ISAMGeometry3D>> tuples = new List<Tuple<GH_Path, ISAMGeometry3D>>();

            for (int i=0; i < objectWrappers.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                GH_ObjectWrapper objectWrapper = objectWrappers[i];

                object @object = objectWrappers[i].Value;
                if (@object is IGH_Goo)
                {
                    @object = ((dynamic)@object).Value;
                }

                if (@object is Aperture)
                {
                    Panel panel = adjacencyCluster.GetPanel((Aperture)@object);
                    if(panel != null)
                    {
                        dataTree.Add(new GooPanel(Create.Panel(panel)), path);
                    }
                }
                else if (@object is ApertureConstruction)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels((ApertureConstruction)@object);
                    panels?.ForEach(x => dataTree.Add(new GooPanel(Create.Panel(x)), path));
                }
                else if (@object is Construction)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels((Construction)@object);
                    panels?.ForEach(x => dataTree.Add(new GooPanel(Create.Panel(x)), path));
                }
                else if (@object is Point3D)
                {
                    tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, (Point3D)@object));
                }
                else if (@object is Geometry.Planar.Point2D)
                {
                    Geometry.Planar.Point2D point2D = (Geometry.Planar.Point2D)@object;
                    tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, new Point3D(point2D.X, point2D.Y, 0)));
                }
                else if (@object is GH_Point)
                {
                    Point3D point3D = Geometry.Grasshopper.Convert.ToSAM((GH_Point)@object);
                    if (point3D != null)
                        tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, point3D));
                }
                else if(@object is global::Rhino.Geometry.Point3d)
                {
                    Point3D point3D = Geometry.Rhino.Convert.ToSAM((global::Rhino.Geometry.Point3d)@object);
                    if (point3D != null)
                        tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, point3D));
                }
                else
                {
                    if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds) && face3Ds != null)
                    {
                        face3Ds.ForEach(x => tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, x)));
                    }
                    else if(Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes) && planes != null)
                    {
                        planes.ForEach(x => tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, x)));
                    }
                }
            }

            if(tuples != null && tuples.Count != 0)
            {
                List<Tuple<Panel, Face3D>> tuples_Temp = adjacencyCluster.GetPanels()?.ConvertAll(x => new Tuple<Panel, Face3D>(x, x?.Face3D));
                tuples_Temp.RemoveAll(x => x.Item2 == null || x.Item1 == null);

                if(tuples_Temp != null && tuples_Temp.Count > 0)
                {
                    foreach (Tuple<GH_Path, ISAMGeometry3D> tuple_Geometry in tuples)
                    {
                        ISAMGeometry3D sAMGeometry3D = tuple_Geometry?.Item2;
                        if(sAMGeometry3D == null)
                        {
                            continue;
                        }

                        if(sAMGeometry3D is Point3D)
                        {
                            Point3D point3D = (Point3D)sAMGeometry3D;

                            foreach (Tuple<Panel, Face3D> tuple_Panel in tuples_Temp)
                            {
                                Face3D face3D = tuple_Panel.Item2;
                                if (face3D == null)
                                {
                                    continue;
                                }

                                if (face3D.InRange(point3D, tolerance) || face3D.Inside(point3D, tolerance))
                                {
                                    dataTree.Add(new GooPanel(tuple_Panel.Item1), tuple_Geometry.Item1);
                                }
                            }
                        }
                        else if (sAMGeometry3D is Face3D)
                        {
                            Face3D face3D = (Face3D)sAMGeometry3D;

                            foreach (Tuple<Panel, Face3D> tuple_Panel in tuples_Temp)
                            {
                                Face3D face3D_Panel = tuple_Panel.Item2;
                                if (face3D_Panel == null)
                                {
                                    continue;
                                }

                                if(face3D.Inside(face3D_Panel))
                                {
                                    dataTree.Add(new GooPanel(Create.Panel(tuple_Panel.Item1)), tuple_Geometry.Item1);
                                }
                            }
                        }
                        else if (sAMGeometry3D is Plane)
                        {
                            Plane plane = (Plane)sAMGeometry3D;

                            foreach (Tuple<Panel, Face3D> tuple_Panel in tuples_Temp)
                            {
                                Face3D face3D_Panel = tuple_Panel.Item2;
                                if (face3D_Panel == null)
                                {
                                    continue;
                                }

                                if(plane.On(face3D_Panel, tolerance))
                                {
                                    dataTree.Add(new GooPanel(Create.Panel(tuple_Panel.Item1)), tuple_Geometry.Item1);
                                }
                            }
                        }
                    }
                }
            }
            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree);
        }
    }
}