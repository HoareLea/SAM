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
    public class SAMAnalyticalAddAperturesByAperture : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a2392741-bec2-4646-8205-f9eb36cccd46");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.8";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByAperture()
          : base("SAMAnalytical.AddAperturesByAperture", "SAMAnalytical.AddAperturesByAperture",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model. Component does not copy instance parameters of Aperture!",
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

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "_apertures_", NickName = "_apertures_", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list, Optional = true, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance_", NickName = "_maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "trimGeometry_", NickName = "trimGeometry_", Description = "trimGeometry (bool, default = true)\r\n\r\nDetermines how apertures are handled when their geometry extends beyond the boundaries of panels in the AdjacencyCluster.\r\n\r\ntrue (default)\r\n\r\nIf an aperture overlaps multiple panels, the geometry will be split so that each portion is placed as a separate aperture on the corresponding panels.\r\n\r\nIf an aperture extends beyond a single panel and no other adjacent panel exists, the aperture will be trimmed to fit within that panel.\r\n\r\nfalse\r\n\r\nApertures will be added without splitting or trimming, even if they extend beyond panel boundaries. ", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "Apertures", NickName = "Apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;

            index = Params.IndexOfInputParam("_analyticalObject");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures = new List<Aperture>();
            index = Params.IndexOfInputParam("_apertures_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            double maxDistance = 0.1;
            index = Params.IndexOfInputParam("_maxDistance_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref maxDistance) || double.IsNaN(maxDistance))
                {
                    maxDistance = 0.1;
                }
            }

            bool trimApertures = true;
            index = Params.IndexOfInputParam("trimGeometry_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref trimApertures);
            }

            if (analyticalObject is Panel)
            {
                Panel panel = Create.Panel((Panel)analyticalObject);

                List<Aperture> apertures_Result = null;
                if (apertures != null && apertures.Count != 0)
                {
                    apertures_Result = new List<Aperture>();
                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null)
                        {
                            continue;
                        }

                        List<Aperture> apertures_New = panel.AddApertures([aperture], trimApertures, Tolerance.MacroDistance, maxDistance);
                        if (apertures_New != null)
                        {
                            apertures_Result.AddRange(apertures_New);
                        }
                    }
                }

                index = Params.IndexOfOutputParam("AnalyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, panel);
                }

                index = Params.IndexOfOutputParam("Apertures");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, apertures_Result?.ConvertAll(x => new GooAperture(x)));
                }

                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, apertures_Result != null && apertures_Result.Count != 0);
                }

                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)analyticalObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures_Result_Cluster = Analytical.Modify.AddAperturesByApertures(adjacencyCluster, apertures, trimApertures, Tolerance.MacroDistance, maxDistance);

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                if (analyticalModel != null)
                {
                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(index, analyticalModel_Result);
                }
                else
                {
                    dataAccess.SetData(index, adjacencyCluster);
                }
            }

            index = Params.IndexOfOutputParam("Apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertures_Result_Cluster?.ConvertAll(x => new GooAperture(x)));
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, apertures_Result_Cluster != null && apertures_Result_Cluster.Count != 0);
            }
        }
    }
}
