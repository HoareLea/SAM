// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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
        public override string LatestComponentVersion => "1.1.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetPanels()
          : base("SAMAnalytical.GetPanels", "SAMAnalytical.GetPanels",
              "Get Panels from SAM Analytical Model",
              "SAM", "Analytical02")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Object such as AdjacencyCluster, AnalyticalModel or Panel", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_objects", NickName = "_objects", Description = "Objects such as Point, SAM Analytical Construction, Aperture etc.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
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

            index = Params.IndexOfInputParam("_analyticals");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IAnalyticalObject> analyticalObjects = new List<IAnalyticalObject>();
            if (!dataAccess.GetDataList(index, analyticalObjects) || analyticalObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            analyticalObjects.RemoveAll(x => x == null);
            if (analyticalObjects.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels_Source = new List<Panel>();
            List<AdjacencyCluster> adjacencyClusters = new List<AdjacencyCluster>();
            List<AnalyticalModel> analyticalModels = new List<AnalyticalModel>();

            foreach (IAnalyticalObject analyticalObject in analyticalObjects)
            {
                if (analyticalObject is Panel)
                {
                    panels_Source.Add((Panel)analyticalObject);
                }
                else if (analyticalObject is AdjacencyCluster)
                {
                    adjacencyClusters.Add((AdjacencyCluster)analyticalObject);
                }
                else if (analyticalObject is AnalyticalModel)
                {
                    analyticalModels.Add((AnalyticalModel)analyticalObject);
                }
            }

            List<Panel> panels_Pool = new List<Panel>();
            panels_Pool.AddRange(panels_Source);

            foreach (AnalyticalModel analyticalModel in analyticalModels)
            {
                List<Panel> panels = analyticalModel?.AdjacencyCluster?.GetPanels();
                if (panels != null && panels.Count > 0)
                    panels_Pool.AddRange(panels);
            }

            foreach (AdjacencyCluster adjacencyCluster in adjacencyClusters)
            {
                List<Panel> panels = adjacencyCluster?.GetPanels();
                if (panels != null && panels.Count > 0)
                    panels_Pool.AddRange(panels);
            }

            if (panels_Pool.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No panels found");
                return;
            }

            index = Params.IndexOfInputParam("_objects");
            if (index == -1)
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
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref tolerance))
                {
                    tolerance = Tolerance.MacroDistance;
                }
            }

            DataTree<GooPanel> dataTree = new DataTree<GooPanel>();
            List<Tuple<GH_Path, ISAMGeometry3D>> tuples = new List<Tuple<GH_Path, ISAMGeometry3D>>();
            bool hasGeometry = false;

            for (int i = 0; i < objectWrappers.Count; i++)
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
                    Aperture aperture = (Aperture)@object;
                    foreach (Panel panel in panels_Pool)
                    {
                        if (panel != null && panel.HasAperture(aperture.Guid))
                        {
                            dataTree.Add(new GooPanel(Create.Panel(panel)), path);
                            break;
                        }
                    }
                }
                else if (@object is ApertureConstruction)
                {
                    ApertureConstruction apertureConstruction = (ApertureConstruction)@object;
                    foreach (Panel panel in panels_Pool)
                    {
                        if (panel == null)
                            continue;

                        List<Aperture> apertures = panel.Apertures;
                        if (apertures == null || apertures.Count == 0)
                            continue;

                        if (apertures.Exists(x => x != null && x.TypeGuid == apertureConstruction.Guid))
                            dataTree.Add(new GooPanel(Create.Panel(panel)), path);
                    }
                }
                else if (@object is Construction)
                {
                    Construction construction = (Construction)@object;
                    foreach (Panel panel in panels_Pool)
                    {
                        if (panel != null && panel.TypeGuid == construction.Guid)
                            dataTree.Add(new GooPanel(Create.Panel(panel)), path);
                    }
                }
                else if (@object is Point3D)
                {
                    hasGeometry = true;
                    tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, (Point3D)@object));
                }
                else if (@object is Geometry.Planar.Point2D)
                {
                    hasGeometry = true;
                    Geometry.Planar.Point2D point2D = (Geometry.Planar.Point2D)@object;
                    tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, new Point3D(point2D.X, point2D.Y, 0)));
                }
                else if (@object is GH_Point)
                {
                    Point3D point3D = Geometry.Grasshopper.Convert.ToSAM((GH_Point)@object);
                    if (point3D != null)
                    {
                        hasGeometry = true;
                        tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, point3D));
                    }
                }
                else if (@object is global::Rhino.Geometry.Point3d)
                {
                    Point3D point3D = Geometry.Rhino.Convert.ToSAM((global::Rhino.Geometry.Point3d)@object);
                    if (point3D != null)
                    {
                        hasGeometry = true;
                        tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, point3D));
                    }
                }
                else
                {
                    if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds) && face3Ds != null && face3Ds.Count > 0)
                    {
                        hasGeometry = true;
                        face3Ds.ForEach(x => tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, x)));
                    }
                    else if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes) && planes != null && planes.Count > 0)
                    {
                        hasGeometry = true;
                        planes.ForEach(x => tuples.Add(new Tuple<GH_Path, ISAMGeometry3D>(path, x)));
                    }
                }
            }

            if (hasGeometry && tuples.Count > 0)
            {
                List<Tuple<Panel, Face3D>> tuples_Temp = new List<Tuple<Panel, Face3D>>();
                foreach (Panel panel in panels_Pool)
                {
                    if (panel == null)
                        continue;

                    Face3D face3D = panel.Face3D;
                    if (face3D == null)
                        continue;

                    tuples_Temp.Add(new Tuple<Panel, Face3D>(panel, face3D));
                }

                if (tuples_Temp.Count > 0)
                {
                    foreach (Tuple<GH_Path, ISAMGeometry3D> tuple_Geometry in tuples)
                    {
                        ISAMGeometry3D sAMGeometry3D = tuple_Geometry?.Item2;
                        if (sAMGeometry3D == null)
                        {
                            continue;
                        }

                        if (sAMGeometry3D is Point3D)
                        {
                            Point3D point3D = (Point3D)sAMGeometry3D;

                            foreach (Tuple<Panel, Face3D> tuple_Panel in tuples_Temp)
                            {
                                Face3D face3D = tuple_Panel.Item2;

                                if (face3D.InRange(point3D, tolerance) || face3D.Inside(point3D, tolerance))
                                {
                                    dataTree.Add(new GooPanel(Create.Panel(tuple_Panel.Item1)), tuple_Geometry.Item1);
                                }
                            }
                        }
                        else if (sAMGeometry3D is Face3D)
                        {
                            Face3D face3D = (Face3D)sAMGeometry3D;

                            foreach (Tuple<Panel, Face3D> tuple_Panel in tuples_Temp)
                            {
                                Face3D face3D_Panel = tuple_Panel.Item2;

                                if (face3D.Inside(face3D_Panel))
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

                                if (plane.On(face3D_Panel, tolerance))
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
