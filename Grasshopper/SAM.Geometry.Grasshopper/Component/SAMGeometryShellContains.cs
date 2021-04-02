using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryShellContains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("981a3a3a-60ec-49af-89e1-42497b6164f9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryShellContains()
          : base("SAMGeometry.ShellContains", "SAMGeometry.ShellContains",
              "Checks if Shell Contains given Point3D",
              "SAM", "Geometry")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject;

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shell", NickName = "_shell", Description = "SAM Geometry Shell", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_point", NickName = "_point", Description = "SAM Geometry Point3D", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean booleanParam = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "allowOnBoundary_", NickName = "allowOnBoundary_", Description = "Allow On Boundary", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(booleanParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Contains", NickName = "Contains", Description = "Contains", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("_shell");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Shell shell = null;
            if (Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells_Temp) && shells_Temp != null && shells_Temp.Count > 0)
                shell = shells_Temp.FirstOrDefault();

            if (shell == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            objectWrapper = null;
            index = Params.IndexOfInputParam("_point");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Point3D point3D = null;
            if (Query.TryGetSAMGeometries(objectWrapper, out List<Point3D> point3Ds_Temp) && point3Ds_Temp != null && point3Ds_Temp.Count > 0)
                point3D = point3Ds_Temp.FirstOrDefault();

            if (point3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
            {
                double silverSpacing_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref silverSpacing_Temp) && !double.IsNaN(silverSpacing_Temp))
                    silverSpacing = silverSpacing_Temp;
            }

            index = Params.IndexOfInputParam("silverSpacing_");
            bool allowOnBoundary = true;
            if (index != -1)
            {
                bool allowOnBoundary_Temp = true;
                if (!dataAccess.GetData(index, ref allowOnBoundary_Temp))
                    allowOnBoundary = allowOnBoundary_Temp;
            }

            bool contains = false;
            if(shell.Inside(point3D, silverSpacing, tolerance))
            {
                contains = true;
                if (!allowOnBoundary && shell.On(point3D, tolerance))
                    contains = false;
            }

            index = Params.IndexOfOutputParam("Contains");
            if (index != -1)
                dataAccess.SetData(index, contains);
        }
    }
}