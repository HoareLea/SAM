using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryCutShell : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("86f1a382-5e52-48ef-a374-9e8d580fe112");

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
        public SAMGeometryCutShell()
          : base("Geometry.CutShell", "Geometry.CutShell",
              "Cut Shell by Plane",
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

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_plane", NickName = "_plane", Description = "SAM Geometry Plane", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "shells", NickName = "shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Shell> shells = new List<Shell>();
            if (Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells_Temp) && shells_Temp != null && shells_Temp.Count > 0)
                shells.AddRange(shells_Temp);

            if (shells.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_plane");
            objectWrapper = null;
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
                plane = Spatial.Create.Plane((double)@object);
            }
            else if (@object is string)
            {
                if (double.TryParse((string)@object, out double elevation_Temp))
                {
                    plane = Spatial.Create.Plane(elevation_Temp);
                }
            }
            else if(@object is Rhino.Geometry.Plane)
            {
                plane = ((Rhino.Geometry.Plane)@object).ToSAM();
            }
            else if(@object is Plane)
            {
                plane = ((Plane)@object);
            }

            if(plane == null)
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

            shells = shells.ConvertAll(x => new Shell(x));

            List<Shell> shells_Result = new List<Shell>();
            foreach(Shell shell in shells)
            {
                List<Shell> shells_Result_Temp = shell.Cut(plane);
                if(shells_Result_Temp == null || shells_Result_Temp.Count == 0)
                {
                    continue;
                }

                shells_Result.AddRange(shells_Result_Temp);
            }

            index = Params.IndexOfOutputParam("shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells_Result);
        }
    }
}