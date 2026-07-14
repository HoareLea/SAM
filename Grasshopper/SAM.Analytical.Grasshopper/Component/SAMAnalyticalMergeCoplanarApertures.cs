// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeCoplanarApertures : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b09ab5e5-eb79-4ef9-93e6-22aa1504fecf");

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
        public SAMAnalyticalMergeCoplanarApertures()
          : base("SAMAnalytical.MergeCoplanarApertures", "SAMAnalytical.MergeCoplanarApertures",
              "Merge Coplanar SAM Analytical Apertures",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance", NickName = "_tolerance", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "mergedApertures", NickName = "mergedApertures", Description = "mergedApertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "redundantApertures", NickName = "redundantApertures", Description = "redundantApertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            index = Params.IndexOfInputParam("_analyticalObject");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_tolerance");
            double tolerance = Tolerance.MacroDistance;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref tolerance))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            List<Aperture> redundantApertures = null;
            List<Aperture> mergedApertures = null;

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            panels = Analytical.Query.MergeCoplanarApertures(panels, out redundantApertures, out mergedApertures, true, tolerance);

            for (int i = 0; i < adjacencyClusters.Count; i++)
            {
                List<Aperture> redundantApertures_Temp = null;
                List<Aperture> mergedApertures_Temp = null;

                adjacencyClusters[i] = Analytical.Query.MergeCoplanarApertures(adjacencyClusters[i], out mergedApertures_Temp, out redundantApertures_Temp, true, tolerance);
                if (redundantApertures_Temp != null)
                {
                    if (redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(redundantApertures_Temp);
                }

                if (mergedApertures_Temp != null)
                {
                    if (mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.AddRange(mergedApertures_Temp);
                }
            }

            for (int i = 0; i < analyticalModels.Count; i++)
            {
                List<Aperture> redundantApertures_Temp = null;
                List<Aperture> mergedApertures_Temp = null;

                analyticalModels[i] = Analytical.Query.MergeCoplanarApertures(analyticalModels[i], out mergedApertures_Temp, out redundantApertures_Temp, true, tolerance);
                if (redundantApertures_Temp != null)
                {
                    if (redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(redundantApertures_Temp);
                }

                if (mergedApertures_Temp != null)
                {
                    if (mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.AddRange(mergedApertures_Temp);
                }
            }

            List<SAMObject> result = new List<SAMObject>();
            if (panels != null)
                result.AddRange(panels.Cast<SAMObject>());

            if (adjacencyClusters != null)
                result.AddRange(adjacencyClusters.Cast<SAMObject>());

            if (analyticalModels != null)
                result.AddRange(analyticalModels.Cast<SAMObject>());

            index = Params.IndexOfOutputParam("analyticalObject");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result);
            }

            index = Params.IndexOfOutputParam("mergedApertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, mergedApertures?.ConvertAll(x => new GooAperture(x)));
            }

            index = Params.IndexOfOutputParam("redundantApertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, redundantApertures?.ConvertAll(x => new GooAperture(x)));
            }
        }
    }
}
