using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreConvert : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b06eefea-cf86-49a9-b805-2463a20b41c6");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreConvert()
          : base("SAMCore.Convert", "SAMCore.Convert",
              "Converts Rhino Geometry to SAM",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Geometry() { Name = "_geometry", NickName = "_geometry", Description = "Geometry", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "SAMObject", NickName = "SAMObject", Description = "SAM Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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