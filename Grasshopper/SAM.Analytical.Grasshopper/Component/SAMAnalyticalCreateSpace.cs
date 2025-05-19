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
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateSpace()
          : base("SAMAnalytical.CreateSpace", "SAMAnalytical.CreateSpace",
              "Create SAM Space, if nothing connect default values: _name = Space_Default, _locationPoint = (0, 0, 0.75)",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "space_", NickName = "space_", Description = "Source SAM Analytical Space", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Space Name, Default = Space_Default", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "locationPoint_", NickName = "locationPoint_", Description = "Space Location Point, Default = (0,0,0.75)", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalCondition_", NickName = "internalCondition_", Description = "Space InternalCondition", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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

            Space space = null;
            index = Params.IndexOfInputParam("space_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref space);
            }

            if(space == null)
            {
                space = new Space("Space_Default", new Point3D(0, 0, 0.75));
            }
            else
            {
                space = new Space(Guid.NewGuid(), space);
            }


            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1 && dataAccess.GetData(index, ref name) && name != null)
            {
                space = new Space(space, name, space.Location);
            }

            ISAMGeometry location = null;
            index = Params.IndexOfInputParam("locationPoint_");
            if (index != -1 && dataAccess.GetData(index, ref location) && (location is Point3D))
            {
                space = new Space(space, space.Name, (Point3D)location);
            }

            InternalCondition internalCondition = null;
            index = Params.IndexOfInputParam("internalCondition_");
            if(index != -1 && dataAccess.GetData(index, ref internalCondition))
            {
                space.InternalCondition = internalCondition;
            }

            double volume = double.NaN;
            index = Params.IndexOfInputParam("volume_");
            if (index != -1 && dataAccess.GetData(index, ref volume))
            {
                space.SetValue(SpaceParameter.Volume, volume);
            }

            double area = double.NaN;
            index = Params.IndexOfInputParam("area_");
            if (index != -1 && dataAccess.GetData(index, ref area))
            {
                space.SetValue(SpaceParameter.Area, area);
            }

            index = Params.IndexOfOutputParam("space");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSpace(space));
            }
        }
    }
}