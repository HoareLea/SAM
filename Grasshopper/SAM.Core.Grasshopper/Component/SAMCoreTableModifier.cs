using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreTableModifier : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("09c0ceb3-2848-44d2-acde-94e01577b0f3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreTableModifier()
          : base("SAMCore.TableModifier ", "SAMCore.TableModifier ",
              "Gets TableModifier Properties",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooTableModifierParam() { Name = "_tableModifier", NickName = "_tableModifier", Description = "SAM Core TableModifier", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooTableModifierParam() { Name = "tableModifier", NickName = "tableModifier", Description = "SAM Core TableModifier", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Matrix() { Name = "matrixReloaded", NickName = "matrixReloaded", Description = "Matrix Reloaded", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "matrixAsTree", NickName = "matrixAsTree", Description = "Matrix as Tree", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "graphTree", NickName = "graphTree", Description = "Graph as Tree", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Line() { Name = "maxGraphLine", NickName = "maxGraphLine", Description = "Max Graph Line", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            index = Params.IndexOfInputParam("_geometry");

            Rhino.Geometry.GeometryBase geometryBase = null;
            if (index == -1 || !dataAccess.GetData(index, ref geometryBase))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IJSAMObject jSAMObject = null;

            NameValueCollection nwc = geometryBase.GetUserStrings();

            string @string = geometryBase.GetUserString("SAM");
            if (!string.IsNullOrWhiteSpace(@string))
            {
                List<IJSAMObject> jSAMObjects = Core.Convert.ToSAM(@string);
                jSAMObject = jSAMObjects?.FirstOrDefault();
            }

            index = Params.IndexOfOutputParam("SAMObject");
            if (index != -1)
                dataAccess.SetData(index, jSAMObject);
        }
    }
}