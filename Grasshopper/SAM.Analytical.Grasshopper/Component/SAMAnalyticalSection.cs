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
    public class SAMAnalyticalSection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("566ffb4a-03b1-46b3-8b30-32593146d4cf");

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
        public SAMAnalyticalSection()
          : base("SAMAnalytical.Section", "SAMAnalytical.Section",
              "Section through list of Panels, AdjacencyClusters, AnalyticalModels",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject paramGenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list };
                paramGenericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(paramGenericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_elevations", NickName = "_elevations", Description = "Section Elevations", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Optional = true, Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "elevations", NickName = "elevations", Description = "Elevations", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "geometries", NickName = "geometries", Description = "geometries", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_analyticals");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if(index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(objectWrapper.Value is Panel)
                {
                    panels.Add((Panel)objectWrapper.Value);
                }
                else if(objectWrapper.Value is AnalyticalModel)
                {
                    List<Panel> panels_Temp = ((AnalyticalModel)objectWrapper.Value).GetPanels();
                    if(panels_Temp != null)
                    {
                        panels.AddRange(panels_Temp);
                    }
                }
                else if(objectWrapper.Value is AdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((AdjacencyCluster)objectWrapper.Value).GetPanels();
                    if (panels_Temp != null)
                    {
                        panels.AddRange(panels_Temp);
                    }
                }
            }

            index = Params.IndexOfInputParam("_elevations");
            List<double> elevations = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, elevations) || elevations == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if(index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            Dictionary<Panel, Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>>> dictionary = new Dictionary<Panel, Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>>>();

            foreach(double elevation in elevations)
            {
                Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY.GetMoved(new Geometry.Spatial.Vector3D(0, 0, elevation)) as Geometry.Spatial.Plane;
                Dictionary<Panel, List<Geometry.Spatial.ISAMGeometry3D>> dictionary_Temp = Analytical.Query.SectionDictionary<Geometry.Spatial.ISAMGeometry3D>(panels, plane, tolerance);
                if(dictionary_Temp == null || dictionary_Temp.Count == 0)
                {
                    continue;
                }

                foreach(KeyValuePair<Panel, List<Geometry.Spatial.ISAMGeometry3D>> keyValuePair in dictionary_Temp)
                {
                    Panel panel = keyValuePair.Key;
                    
                    Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>> dictionary_Elevation = null;

                    if (!dictionary.TryGetValue(panel, out dictionary_Elevation))
                    {
                        dictionary_Elevation = new Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>>();
                        dictionary[panel] = dictionary_Elevation;
                    }

                    List<Geometry.Spatial.ISAMGeometry3D> sAMGeometry3Ds = null;
                    if (!dictionary_Elevation.TryGetValue(elevation, out sAMGeometry3Ds))
                    {
                        sAMGeometry3Ds = new List<Geometry.Spatial.ISAMGeometry3D>();
                        dictionary_Elevation[elevation] = sAMGeometry3Ds;
                    }

                    sAMGeometry3Ds.AddRange(keyValuePair.Value);
                }
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, dictionary.Keys);
            }

            index = Params.IndexOfOutputParam("elevations");
            if (index != -1)
            {
                DataTree<double> dataTree_Elevation = new DataTree<double>();
                int count = 0;
                foreach (KeyValuePair<Panel, Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>>> keyValuePair in dictionary)
                {
                    GH_Path path = new GH_Path(count);
                    foreach(double elevation in keyValuePair.Value.Keys)
                    {
                        dataTree_Elevation.Add(elevation, path);
                    }

                    count++;
                }

                dataAccess.SetDataTree(index, dataTree_Elevation);
            }

            index = Params.IndexOfOutputParam("geometries");
            if (index != -1)
            {
                DataTree<Geometry.Spatial.ISAMGeometry3D> dataTree_Geometries = new DataTree<Geometry.Spatial.ISAMGeometry3D>();
                int count_1 = 0;
                foreach (KeyValuePair<Panel, Dictionary<double, List<Geometry.Spatial.ISAMGeometry3D>>> keyValuePair in dictionary)
                {
                    int count_2 = 0;
                    foreach (double elevation in keyValuePair.Value.Keys)
                    {
                        GH_Path path = new GH_Path(count_1, count_2);
                        keyValuePair.Value[elevation].ForEach(x => dataTree_Geometries.Add(x, path));

                        count_2++;
                    }

                    count_1++;
                }

                dataAccess.SetDataTree(index, dataTree_Geometries);
            }
        }
    }
}