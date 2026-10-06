// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometrySAMGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c82929ce-ada5-4e48-be04-9f024de4e6fc");

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
        public GeometrySAMGeometry()
          : base("Geometry.SAMGeometry", "Geometry.SAMGeometry",
              "Convert Rhino geometry to SAM geometry",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Geometry() { Name = "_geometry", NickName = "_geometry", Description = "Rhino/GH geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "SAMGeometries", NickName = "SAMGeos", Description = "SAM Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            IGH_GeometricGoo geometricGoo = null;
            if (index == -1 || !dataAccess.GetData(index, ref geometricGoo) || geometricGoo == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = geometricGoo.ToSAM(true);

            if (@object == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            else if (@object is IEnumerable)
            {
                index = Params.IndexOfOutputParam("SAMGeometries");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, (IEnumerable)@object);
                }
            }
            else
            {
                index = Params.IndexOfOutputParam("SAMGeometries");
                if (index != -1)
                {
                    dataAccess.SetData(index, @object);
                }
            }
        }
    }
}
