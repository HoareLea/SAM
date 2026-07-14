// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class GeometrySnapByDistance : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1204fbdd-0e61-4dfd-8bb4-21b6eb461ae7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometrySnapByDistance()
          : base("Geometry.SnapByDistance", "GeometryEngine.SnapByDistance",
              "Snap Geometry or SAM Geometry by distance",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_Face3D_1", NickName = "F_1", Description = "SAM Geometry Face3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_Face3D_2", NickName = "F_2", Description = "SAM Geometry Face3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_snapDistance", NickName = "SnDist", Description = "Snapping Distance", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Geometry", NickName = "Geo", Description = "modified SAM Geometry", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }
            if (!run)
                return;

            index = Params.IndexOfInputParam("_snapDistance");
            double snapDistance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref snapDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            index = Params.IndexOfInputParam("_Face3D_1");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Geometry");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            Face3D face3D_1 = null;
            if (objectWrapper.Value is IGH_GeometricGoo)
                face3D_1 = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value)?.First() as Face3D;
            else if (objectWrapper.Value is Face3D)
                face3D_1 = (Face3D)objectWrapper.Value;

            if (face3D_1 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            index = Params.IndexOfInputParam("_Face3D_2");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            Face3D face3D_2 = null;
            if (objectWrapper.Value is IGH_GeometricGoo)
                face3D_2 = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value)?.First() as Face3D;
            else if (objectWrapper.Value is Face3D)
                face3D_2 = (Face3D)objectWrapper.Value;

            if (face3D_2 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            face3D_1 = Spatial.Query.Snap(face3D_1, face3D_2, snapDistance);
            face3D_2 = Spatial.Query.Snap(face3D_2, face3D_1, snapDistance);

            index = Params.IndexOfOutputParam("Geometry");
            if (index != -1)
            {
                dataAccess.SetDataList(index, new Face3D[] { face3D_1, face3D_2 });
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
