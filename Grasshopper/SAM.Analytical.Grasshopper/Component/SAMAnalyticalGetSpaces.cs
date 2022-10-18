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
    public class SAMAnalyticalGetSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a831d8de-7746-4390-9ed2-91247d102d01");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetSpaces()
          : base("SAMAnalytical.GetSpaces", "SAMAnalytical.GetSpaces",
              "Get Spaces from SAM Analytical Model",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_objects", NickName = "_objects", Description = "Objects such as Point, SAM Analytical InternalCondition etc.", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "shells", NickName = "shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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

            DataTree<GooSpace> dataTree_Spaces = new DataTree<GooSpace>();
            DataTree<Geometry.Grasshopper.GooSAMGeometry> dataTree_Shells = new DataTree<Geometry.Grasshopper.GooSAMGeometry>();
            List<Tuple<GH_Path, Geometry.Spatial.Point3D>> tuples = new List<Tuple<GH_Path, Geometry.Spatial.Point3D>>();

            for (int i=0; i < objects.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                object @object = objects[i];
                if (@object is IGH_Goo)
                    @object = ((dynamic)@object).Value;

                if (@object is InternalCondition)
                {
                    List<Space> spaces = adjacencyCluster.GetSpaces();
                    if (spaces == null || spaces.Count == 0)
                        return;

                    foreach (Space space in spaces)
                    {
                        InternalCondition internalCondition = space?.InternalCondition;
                        if (internalCondition == null)
                            continue;

                        if (!internalCondition.Guid.Equals(((InternalCondition)@object).Guid))
                            continue;

                        dataTree_Spaces.Add(new GooSpace(space), path);
                        dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                    }
                }
                else if (@object is Panel)
                {
                    List<Space> spaces = adjacencyCluster.GetSpaces((Panel)@object);
                    if (spaces != null)
                    {
                        foreach(Space space in spaces)
                        {
                            dataTree_Spaces.Add(new GooSpace(space), path);
                            dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                        }
                    }

                }
                else if (@object is Aperture)
                {
                    Panel panel = adjacencyCluster.GetPanel((Aperture)@object);
                    List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                    if (spaces != null)
                    {
                        foreach (Space space in spaces)
                        {
                            dataTree_Spaces.Add(new GooSpace(space), path);
                            dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                        }
                    }
                }
                else if (@object is ApertureConstruction)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels((ApertureConstruction)@object);
                    if (panels != null && panels.Count != 0)
                    {
                        Dictionary<Guid, Space> dictionary = new Dictionary<Guid, Space>();
                        foreach (Panel panel in panels)
                        {
                            List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                            if (spaces == null || spaces.Count == 0)
                                continue;

                            spaces.ForEach(x => dictionary[x.Guid] = x);
                        }

                        foreach (Space space in dictionary.Values)
                        {
                            dataTree_Spaces.Add(new GooSpace(space), path);
                            dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                        }
                    }
                }
                else if (@object is Construction)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels((Construction)@object);
                    if (panels != null && panels.Count != 0)
                    {
                        Dictionary<Guid, Space> dictionary = new Dictionary<Guid, Space>();
                        foreach (Panel panel in panels)
                        {
                            List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                            if (spaces == null || spaces.Count == 0)
                                continue;

                            spaces.ForEach(x => dictionary[x.Guid] = x);
                        }

                        foreach (Space space in dictionary.Values)
                        {
                            dataTree_Spaces.Add(new GooSpace(space), path);
                            dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                        }
                    }
                }
                else if (@object is Profile && analyticalModel != null)
                {
                    List<Space> spaces = adjacencyCluster.GetSpaces();
                    if (spaces == null || spaces.Count == 0)
                        return;

                    ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

                    foreach (Space space in spaces)
                    {
                        Dictionary<ProfileType, Profile> dictionary = space.InternalCondition?.GetProfileDictionary(profileLibrary, true);
                        if (dictionary == null || dictionary.Count == 0)
                            continue;

                        foreach (Profile profile in dictionary.Values)
                        {
                            if (profile.Guid == ((Profile)@object).Guid)
                            {
                                dataTree_Spaces.Add(new GooSpace(space), path);
                                dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(space)), path);
                                break;
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
                Dictionary<Space, Geometry.Spatial.Shell> dictionary = adjacencyCluster?.ShellDictionary();
                if(dictionary != null && dictionary.Count > 0)
                {
                    foreach (Tuple<GH_Path, Geometry.Spatial.Point3D> tuple in tuples)
                    {
                        Geometry.Spatial.Point3D point3D = tuple.Item2;
                        if (point3D == null || tuple.Item1 == null)
                            continue;

                        foreach (KeyValuePair<Space, Geometry.Spatial.Shell> keyValuePair in dictionary)
                        {
                            if (keyValuePair.Key == null)
                                continue;
                            
                            if (keyValuePair.Value.InRange(point3D) || keyValuePair.Value.Inside(point3D))
                            {
                                dataTree_Spaces.Add( new GooSpace(keyValuePair.Key), tuple.Item1);
                                dataTree_Shells.Add(new Geometry.Grasshopper.GooSAMGeometry(adjacencyCluster.Shell(keyValuePair.Key)), tuple.Item1);
                                break;
                            }
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_Spaces);

            index = Params.IndexOfOutputParam("shells");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_Shells);
        }
    }
}