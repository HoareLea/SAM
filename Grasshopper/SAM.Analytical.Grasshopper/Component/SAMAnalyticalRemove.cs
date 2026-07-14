// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemove : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2ae3cd9b-e0bb-4eb6-86f9-02c6a3e0d171");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRemove()
          : base("SAMAnalytical.Remove", "SAMAnalytical.Remove",
              "Remove from SAM Analytical Object",
              "SAM", "Analytical03")
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

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_objects", NickName = "_objects", Description = "SAM Objects", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "guids", NickName = "guids", Description = "Guids of Removed Elements", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalObject");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_objects");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                Panel result = Create.Panel((Panel)sAMObject);
                List<Guid> guids = new List<Guid>();

                List<Aperture> apertures = sAMObjects.FindAll(x => x is Aperture).ConvertAll(x => (Aperture)x);
                if (apertures != null)
                {
                    foreach (Aperture aperture in apertures)
                        if (result.RemoveAperture(aperture.Guid))
                            guids.Add(aperture.Guid);
                }

                index = Params.IndexOfOutputParam("analyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, result);
                }

                index = Params.IndexOfOutputParam("guids");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, guids);
                }
                return;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);

                List<Guid> guids = analyticalModel.Remove(sAMObjects);

                index = Params.IndexOfOutputParam("analyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, analyticalModel);
                }

                index = Params.IndexOfOutputParam("guids");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, guids);
                }
                return;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

                List<Guid> guids = adjacencyCluster.Remove(sAMObjects);

                index = Params.IndexOfOutputParam("analyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, adjacencyCluster);
                }

                index = Params.IndexOfOutputParam("guids");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, guids);
                }
                return;
            }
        }
    }
}
