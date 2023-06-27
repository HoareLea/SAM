using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateSpace : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c6eaf1ad-22bb-4a3f-8c3d-9d8ac483214d");

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
        public SAMAnalyticalCreateSpace()
          : base("SAMAnalytical.CreateSpace", "SAMAnalytical.CreateSpace",
              "Create SAM Space, if nothing connect default values: _name = Space_Default, _locationPoint = (0, 0, 0.75)",
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

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Space Name, Default = Space_Default", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("Space_Default");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                GooSAMGeometryParam gooSAMGeometryParam = new GooSAMGeometryParam() { Name = "_locationPoint", NickName = "_locationPoint", Description = "Space Location Point, Default = (0,0,0.75)", Access = GH_ParamAccess.item, Optional = true };
                gooSAMGeometryParam.SetPersistentData(new GooSAMGeometry(new Point3D(0, 0, 0.75)));
                result.Add(new GH_SAMParam(gooSAMGeometryParam, ParamVisibility.Binding));

                GooInternalConditionParam gooInternalConditionParam = new GooInternalConditionParam() { Name = "internalCondition_", NickName = "internalCondition_", Description = "Space InternalCondition", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooInternalConditionParam, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "volume_", NickName = "volume_", Description = "Space Volume [m3]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "area_", NickName = "area_", Description = "Space Area [m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "space", NickName = "space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            string name = null;
            index = Params.IndexOfInputParam("_name");
            if (index == -1 || !dataAccess.GetData(index, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMGeometry location = null;
            index = Params.IndexOfInputParam("_locationPoint");
            if (index == -1 || !dataAccess.GetData(1, ref location) || !(location is Point3D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            InternalCondition internalCondition = null;
            index = Params.IndexOfInputParam("internalCondition_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref internalCondition);
            }

            double volume = double.NaN;
            index = Params.IndexOfInputParam("volume_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref volume);
            }

            double area = double.NaN;
            index = Params.IndexOfInputParam("area_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref area);
            }

            Space space = new Space(name, (Point3D)location);
            if(internalCondition != null)
            {
                space.InternalCondition = internalCondition;
            }

            if(!double.IsNaN(volume))
            {
                space.SetValue(SpaceParameter.Volume, volume);
            }

            if (!double.IsNaN(area))
            {
                space.SetValue(SpaceParameter.Area, area);
            }

            index = Params.IndexOfInputParam("space");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSpace(space));
            }
        }
    }
}