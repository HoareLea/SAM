// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTransform : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("41a52d4f-159e-4e30-bb4c-167b259eeb7d");

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
        public SAMAnalyticalTransform()
          : base("SAMAnalytical.Transform", "SAMAnalytical.Transform",
              "WIP!!! Currently Transform only Panel - known issue when Pushing Revit - DO NOT USE WITH REVIT",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooTransform3DParam() { Name = "_transform", NickName = "_transform", Description = "Transform can be SAM or GH", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object such as Panel, Aperture etc.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_transform");
            Transform3D transform3D = null;
            if (index == -1 || !dataAccess.GetData(index, ref transform3D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);
                panel.Transform(transform3D);
                sAMObject = panel;
            }
            else if (sAMObject is Aperture)
            {
                Aperture aperture = new Aperture((Aperture)sAMObject);
                aperture.Transform(transform3D);
                sAMObject = aperture;
            }
            else if (sAMObject is Space)
            {
                Space space = new Space((Space)sAMObject);
                space.Transform(transform3D);
                sAMObject = space;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                adjacencyCluster.Transform(transform3D);
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                analyticalModel.Transform(transform3D);
                sAMObject = analyticalModel;
            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, sAMObject);
            }
        }
    }
}
