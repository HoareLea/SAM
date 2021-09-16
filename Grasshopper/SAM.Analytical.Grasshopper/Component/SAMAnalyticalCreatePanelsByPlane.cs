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
    public class SAMAnalyticalCreatePanelsByPlane : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7bde6d08-c2e1-4564-973b-a1837499b6f7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject;

                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM ANalytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_plane", NickName = "_plane", Description = "SAM Geometry Plane", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() {Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreatePanelsByPlane()
          : base("SAMAnalytical.CreatePanelsByPlane", "SAMAnalytical.CreatePanelsByPlane",
              "Create Panels By Adjacency cluster or ANalyticalModel and Plane",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_plane");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = null;

            object @object = objectWrapper.Value;
            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value;
            }

            if (@object is double)
            {
                plane = Geometry.Spatial.Create.Plane((double)@object);
            }
            else if (@object is string)
            {
                if (double.TryParse((string)@object, out double elevation_Temp))
                {
                    plane = Geometry.Spatial.Create.Plane(elevation_Temp);
                }
            }
            else if (@object is Rhino.Geometry.Plane)
            {
                plane = Geometry.Grasshopper.Convert.ToSAM(((Rhino.Geometry.Plane)@object));
            }
            else if (@object is Plane)
            {
                plane = ((Plane)@object);
            }

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<Panel> result = null;
            if(sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
            {
                result = Create.Panels(sAMObject as dynamic, plane, tolerance_Distance: tolerance);
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}