using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalVisualize : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d9f05fca-74ef-4ccc-b159-edcd6dd2085b");

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
        public SAMAnalyticalVisualize()
          : base("SAMAnalytical.Visualize", "SAMAnalytical.Visualize",
              "Panels Visualize",
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

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                //global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                //paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_elevations", NickName = "elevations", Description = "Elevations", Access = GH_ParamAccess.list };
                //result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                //paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_referenceElevation", NickName = "_referenceElevation", Description = "Reference Elevation", Access = GH_ParamAccess.item };
                //result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                //paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                //paramNumber.SetPersistentData(0.2);
                //result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                //paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                //paramNumber.SetPersistentData(Tolerance.Distance);
                //result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Line() { Name = "lines", NickName = "lines", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Mesh() { Name = "bucketSizes", NickName = "bucketSizes", Description = "BucketSizes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Mesh() { Name = "maxExtends", NickName = "maxExtends", Description = "maxExtends", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Mesh() { Name = "weights", NickName = "weights", Description = "weights", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if(index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            panels.RemoveAll(x => x == null);

            List<Mesh> bucketSizes = Rhino.Query.BucketSizeMeshes(panels);
            List<Tuple<Mesh, Mesh>> maxExtends = Rhino.Query.MaxExtendMeshes(panels);
            List<Mesh> weights = Rhino.Query.WeightsMeshes(panels);

            index = Params.IndexOfOutputParam("lines");
            if (index != -1)
            {
                List<Line> lines = new List<Line>();
                foreach(Panel panel in panels)
                {
                    Geometry.Spatial.BoundingBox3D boundingBox3D = panel?.GetBoundingBox();
                    if(boundingBox3D == null)
                    {
                        continue;
                    }

                    Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + 0.1);

                    Geometry.Spatial.Segment3D segment3D = Geometry.Spatial.Query.MaxIntersectionSegment3D(plane, panel);

                    lines.Add(Geometry.Rhino.Convert.ToRhino_Line(segment3D));
                }

                dataAccess.SetDataList(index, lines);
            }

            index = Params.IndexOfOutputParam("bucketSizes");
            if (index != -1)
            {
                dataAccess.SetDataList(index, bucketSizes);
            }

            index = Params.IndexOfOutputParam("maxExtends");
            if (index != -1)
            {
                List<Mesh> meshes = new List<Mesh>();
                foreach(Tuple<Mesh, Mesh> tuple in maxExtends)
                {
                    if(tuple == null)
                    {
                        continue;
                    }
                    
                    if(tuple.Item1 != null)
                    {
                        meshes.Add(tuple.Item1);
                    }

                    if (tuple.Item2 != null)
                    {
                        meshes.Add(tuple.Item2);
                    }
                }    

                dataAccess.SetDataList(index, meshes);
            }

            index = Params.IndexOfOutputParam("weights");
            if (index != -1)
            {
                dataAccess.SetDataList(index, weights);
            }
        }
    }
}