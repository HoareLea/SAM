// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSpaceByLocationAndName : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1571d29d-f01d-4f31-afbe-dd012b0f60c9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateSpaceByLocationAndName()
          : base("SAMAnalytical.UpdateSpaceByLocationAndName", "SAMAnalytical.UpdateSpaceByLocationAndName",
              "Update SAM Analytical Space By given Location Point or Name",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "SAM Analytcial Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject;

                param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_location_", NickName = "_location_", Description = "Location", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

                param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_name_", NickName = "_name_", Description = "Name", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

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

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Space", NickName = "Space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || !dataAccess.GetData(index, ref space))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            index = Params.IndexOfInputParam("_name_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            Point3D location = null;
            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_location_");
            if (index != -1 && dataAccess.GetData(index, ref objectWrapper))
            {
                if (objectWrapper.Value is GH_Point)
                    location = Geometry.Rhino.Convert.ToSAM(((GH_Point)objectWrapper.Value).Value);
                else if (objectWrapper.Value is GH_Point3D)
                    location = ((GH_Point3D)objectWrapper.Value).ToSAM();
                else if (objectWrapper.Value is GooSAMGeometry)
                    location = ((GooSAMGeometry)objectWrapper.Value).Value as Point3D;
            }

            if (name == null)
                name = space.Name;

            if (location == null)
                location = space.Location;

            Space space_Result = new Space(space, name, location);

            index = Params.IndexOfOutputParam("Space");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSpace(space_Result));
            }
        }
    }
}
