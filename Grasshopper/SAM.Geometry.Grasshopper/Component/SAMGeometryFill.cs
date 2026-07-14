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
    public class SAMGeometryFill : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4c2d4620-26c2-4b49-9b63-572dcd3ebbf5");

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
        public SAMGeometryFill()
          : base("SAMGeometry.Fill", "SAMGeometry.Fill",
              "Fill Face3D",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3D", NickName = "_face3D", Description = "SAM Geometry Face3D bo be filled", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3Ds", NickName = "_face3Ds", Description = "Filling SAM Geometry Face3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "face3Ds", NickName = "face3Ds", Description = "SAM Geometry Face3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Successful", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_face3D");
            GH_ObjectWrapper objectWrapper = null;
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

            List<Face3D> face3Ds_Temp = null;

            Face3D face3D = null;
            if (Query.TryGetSAMGeometries(objectWrapper, out face3Ds_Temp))
            {
                face3D = face3Ds_Temp.FirstOrDefault();
            }

            if (face3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_face3Ds");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            face3Ds_Temp = null;

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
            {
                if (Query.TryGetSAMGeometries(objectWrapper_Temp, out face3Ds_Temp))
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            if (face3Ds != null && face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("Successful");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            List<Face3D> result = face3D.Fill(face3Ds, 0.1, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            index = Params.IndexOfOutputParam("face3Ds");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooSAMGeometry(x)));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
