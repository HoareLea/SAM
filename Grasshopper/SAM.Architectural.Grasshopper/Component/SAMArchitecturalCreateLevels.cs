using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using Grasshopper.Kernel.Types;

namespace SAM.Architectural.Grasshopper
{
    public class SAMArchitecturalCreateLevels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3adac138-0250-4c63-88bc-efaf7a36c2f5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalCreateLevels()
          : base("SAMArchitectural.CreateLevels", "SAMArchitectural.CreateLevels",
              "Create SAM Architectural Levels from \n Panel, Aperture, \n * Partion, Opening ",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3DObjects", NickName = "_face3DObjects", Description = "SAM Geometry Face3DObjects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooLevelParam() { Name = "levels", NickName = "levels", Description = "SAM Architectural Levels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooLevelParam() { Name = "topLevel", NickName = "topLevel", Description = "SAM Architectural Level", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            IList<IGH_Param> @params = Params.Input[0].Sources;
            if(@params != null && @params.Count != 0)
            {
                Params.Input[0].NickName = Params.Input[0].Sources[0].NickName;
                Params.Input[0].Name = Params.Input[0].Sources[0].Name;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(0, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Core.Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(!Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp) || face3Ds_Temp == null)
                {
                    continue;
                }

                face3Ds.AddRange(face3Ds_Temp);
            }

            index = Params.IndexOfOutputParam("levels");
            if(index != -1)
            {
                List<Level> levels = Create.Levels(face3Ds, tolerance);
                dataAccess.SetDataList(index, levels?.ConvertAll(x => new GooLevel(x)));
            }

            index = Params.IndexOfOutputParam("topLevel");
            if(index != -1)
            {
                double elevation = Geometry.Spatial.Query.MaxElevation(face3Ds);
                if(elevation != double.MinValue)
                {
                    dataAccess.SetData(index, Create.Level(elevation));
                }
            }

        }
    }
}