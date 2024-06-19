using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByGeometryAndConstructionManager : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0d7525e7-a1ad-4658-bac1-324b0721e8e3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByGeometryAndConstructionManager()
          : base("SAMAnalytical.AddAperturesByGeometryAndConstructionManager", "SAMAnalytical.AddAperturesByGeometryAndConstructionManager",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometries_", NickName = "_geometries_", Description = "Geometry incl Rhino geometries", Access = GH_ParamAccess.list , Optional = true, DataMapping = GH_DataMapping.Flatten};
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "_apertureConstruction_", NickName = "_apertureConstruction_", Description = "SAM Analytical Aperture Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "trimGeometry_", NickName = "trimGeometry_", Description = "Max Distance", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Acceptable area of Aperture", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "frameWidth_", NickName = "frameWidth_", Description = "Frame Width [m] \n*Min value is sum of frame layer thicknesses Default 0.05m so unable to dopt below this value unless. \nIf you want zero remove frame layer in aperture construction", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "framePercentage_", NickName = "framePercentage_", Description = "Frame Percentage [%] \nsee frameWidth_ description \nuse only one input frameWidth_ or framePercentage_", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "constructionManager_", NickName = "constructionManager_", Description = "SAM Analytical Construction Manager", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "materials", NickName = "materials", Description = "SAM Materials", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            index = Params.IndexOfInputParam("_geometries_");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();

            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp) && face3Ds_Temp != null)
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            bool trimGeometry = true;
            index = Params.IndexOfInputParam("trimGeometry_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref trimGeometry);
            }

            ApertureConstruction apertureConstruction = null;
            index = Params.IndexOfInputParam("_apertureConstruction_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstruction);
            }

            double maxDistance = Tolerance.MacroDistance;
            index = Params.IndexOfInputParam("maxDistance_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref maxDistance);
            }

            double minArea = Tolerance.MacroDistance;
            index = Params.IndexOfInputParam("minArea_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref minArea);
            }

            ConstructionManager constructionManager = null;
            index = Params.IndexOfInputParam("constructionManager_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionManager);
            }

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analyticalObject");
            if (!dataAccess.GetData(index, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool framePercentage = true;
            List<double> values = new List<double>();
            index = Params.IndexOfInputParam("frameWidth_");
            if (index != -1 && dataAccess.GetDataList(index, values))
            {
                framePercentage = false;
            }

            if (framePercentage)
            {
                index = Params.IndexOfInputParam("framePercentage_");
                if (index != -1)
                {
                    dataAccess.GetDataList(index, values);
                }
            }

            if (values != null && values.Count != 0)
            {
                face3Ds = Query.Offset(face3Ds, values, framePercentage);
            }

            List<IMaterial> materials = null;

            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            if (analyticalObject is Panel)
            {
                Panel panel = Create.Panel((Panel)analyticalObject);

                List<Aperture> apertures = new List<Aperture>();

                if(face3Ds != null)
                {
                    foreach (Face3D face3D in face3Ds)
                    {
                        ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                        if (apertureConstruction_Temp == null)
                        {
                            apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);
                        }

                        List<Aperture> apertures_Temp = panel.AddApertures(apertureConstruction_Temp, face3D, trimGeometry, minArea, maxDistance);
                        if (apertures_Temp != null)
                        {
                            apertures.AddRange(apertures_Temp);
                        }
                    }
                }

                index = Params.IndexOfOutputParam("analyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, panel);
                }

                index = Params.IndexOfOutputParam("apertures");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, apertures.ConvertAll(x => new GooAperture(x)));
                }

                index = Params.IndexOfOutputParam("materials");
                if (index != -1 && constructionManager != null) 
                {
                    foreach(Aperture aperture in apertures)
                    {
                        ApertureConstruction apertureConstruction_Temp = aperture?.ApertureConstruction;
                        if(apertureConstruction_Temp == null)
                        {
                            continue;
                        }

                        if(apertureConstructions.Find(x => x.Name == apertureConstruction_Temp.Name) != null)
                        {
                            continue;
                        }

                        apertureConstructions.Add(apertureConstruction_Temp);
                    }
                    
                    materials = new List<IMaterial>();
                    foreach(ApertureConstruction apertureConstruction_Temp in apertureConstructions)
                    {
                        IEnumerable<IMaterial> materials_Temp = Analytical.Query.Materials(apertureConstruction_Temp, constructionManager.MaterialLibrary);
                        if(materials_Temp != null)
                        {
                            foreach(IMaterial material in materials_Temp)
                            {
                                if(materials.Find(x => x.Name == material.Name) == null)
                                {
                                    materials.Add(material);
                                }
                            }
                        }
                    }

                    dataAccess.SetDataList(index, materials.ConvertAll(x => new GooMaterial(x)));
                }

                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }
            else if(analyticalObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)analyticalObject);
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
                foreach (Face3D face3D in face3Ds)
                {
                    if (face3D == null)
                        continue;

                    tuples.Add(new Tuple<BoundingBox3D, IClosedPlanar3D>(face3D.GetBoundingBox(maxDistance), face3D));
                }

                tuples_Result = new List<Tuple<Panel, Aperture>>();
                for (int i = 0; i < panels.Count; i++)
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

                        ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                        if (apertureConstruction_Temp == null)
                            apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);

                        if (apertureConstruction_Temp == null)
                            continue;

                        if (panel_Temp == null)
                            panel_Temp = Create.Panel(panel);

                        if(apertureConstructions.Find(x => x.Name == apertureConstruction_Temp?.Name) == null)
                        {
                            apertureConstructions.Add(apertureConstruction_Temp);
                        }

                        List<Aperture> apertures = panel_Temp.AddApertures(apertureConstruction_Temp, tuple.Item2, trimGeometry, minArea, maxDistance);
                        if (apertures == null)
                            continue;

                        apertures.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel_Temp, x)));
                    }

                    if (panel_Temp != null)
                    {
                        adjacencyCluster.AddObject(panel_Temp);
                    }
                }
            }

            materials = new List<IMaterial>();
            foreach (ApertureConstruction apertureConstruction_Temp in apertureConstructions)
            {
                IEnumerable<IMaterial> materials_Temp = Analytical.Query.Materials(apertureConstruction_Temp, constructionManager.MaterialLibrary);
                if (materials_Temp != null)
                {
                    foreach (IMaterial material in materials_Temp)
                    {
                        if (materials.Find(x => x.Name == material.Name) == null)
                        {
                            materials.Add(material);
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("analyticalObject");
            if(index != -1)
            {
                if (analyticalModel != null)
                {
                    MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
                    foreach(IMaterial material in materials)
                    {
                        if(materialLibrary.GetMaterial(material.Name) == null)
                        {
                            materialLibrary.Add(material);
                        }
                    }

                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);
                    dataAccess.SetData(index, analyticalModel_Result);
                }
                else
                {
                    dataAccess.SetData(index, adjacencyCluster);
                }
            }

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, tuples_Result?.ConvertAll(x => new GooAperture(x.Item2)));
            }

            index = Params.IndexOfOutputParam("materials");
            if (index != -1 && constructionManager != null)
            {
                dataAccess.SetDataList(index, materials.ConvertAll(x => new GooMaterial(x)));
            }

        }
    }
}