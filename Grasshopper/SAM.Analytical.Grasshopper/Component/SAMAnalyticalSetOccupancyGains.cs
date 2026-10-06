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
    public class SAMAnalyticalSetOccupancyGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("301c9fa9-8d7e-457b-b643-86b696d3e229");

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
        public SAMAnalyticalSetOccupancyGains()
          : base("SAMAnalytical.SetOccupancyGains", "SAMAnalytical.SetOccupancyGains",
              "Set Occupancy Gains",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Space or InternalCondition", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooDegreeOfActivityParam() { Name = "_degreeOfActivity", NickName = "_degreeOfActivity", Description = "SAM Analytical DegreeOfActivity", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "Analyticals", NickName = "Analyticals", Description = "SAM Analytical Model, Panels or Adjacency Cluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticals");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_degreeOfActivity");
            DegreeOfActivity degreeOfActivity = null;
            if (index == -1 || !dataAccess.GetData(index, ref degreeOfActivity))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for (int i = 0; i < sAMObjects.Count; i++)
            {
                if (sAMObjects[i] is InternalCondition)
                {
                    InternalCondition internalCondition = new InternalCondition((InternalCondition)sAMObjects[i]);
                    if (internalCondition.SetOccupancyGains(degreeOfActivity))
                        sAMObjects[i] = internalCondition;
                }
                else if (sAMObjects[i] is Space)
                {
                    Space space = new Space((Space)sAMObjects[i]);
                    if (space.SetOccupancyGains(degreeOfActivity))
                        sAMObjects[i] = space;
                }
            }

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sAMObjects.ConvertAll(x => new GooJSAMObject<SAMObject>(x)));
            }
        }
    }
}
