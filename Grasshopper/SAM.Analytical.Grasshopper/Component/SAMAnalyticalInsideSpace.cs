// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalInsideSpace : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("09c7dbc3-2383-46bf-a60a-404e82213d50");

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
        public SAMAnalyticalInsideSpace()
          : base("SAMAnalytical.InsideSpace", "SAMAnalytical.InsideSpace",
              "Checks if Point is inside SAM Analytical Space",
              "SAM", "Analytical02")
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
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_point", NickName = "_point", Description = "Point", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Inside", NickName = "Inside", Description = "Point Inside Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            AdjacencyCluster adjacencyCluster = null;
            index = Params.IndexOfInputParam("_adjacencyCluster");
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space = null;
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || !dataAccess.GetData(index, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_point");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Point3D point3D = null;
            if (objectWrapper.Value is GH_Point)
            {
                point3D = ((GH_Point)objectWrapper.Value).ToSAM();
            }
            else if (objectWrapper.Value is Geometry.Spatial.Point3D)
            {
                point3D = ((Geometry.Spatial.Point3D)objectWrapper.Value);
            }

            index = Params.IndexOfOutputParam("Inside");
            if (index != -1)
                dataAccess.SetData(index, Analytical.Query.Inside(adjacencyCluster, space, point3D));
        }
    }
}
