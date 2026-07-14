// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAirflow : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b73301da-72d3-4ab6-a625-17a1f59d128c");

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
        public SAMAnalyticalAirflow()
          : base("SAMAnalytical.Airflow", "SAMAnalytical.Airflow",
              "Calculates Exhaust and Supply Airflow from Internal Condition ONLY" +
              "Calculated Exhaust/Supply Air Flow [m3/s] for IC inside Space.as Sum of ExhaustAirFlowPerPerson, ExhaustAirFlowPerArea, ExhaustAirFlow and ExhaustAirChangesPerHour",
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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Supply", NickName = "Supply", Description = "Supply Airflow [m3/s]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Exhaust", NickName = "Exhasut", Description = "Exhaust Airflow [m3/s]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Supply_LpS", NickName = "Supply_LpS", Description = "Supply Airflow [l/s]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Exhaust_LpS", NickName = "Exhasut_LpS", Description = "Exhaust Airflow [l/s]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_space");
            Space space = null;
            if (index == -1 || !dataAccess.GetData(index, ref space) || space == null)
            {

            }

            double exhaustAirflow = Analytical.Query.CalculatedExhaustAirFlow(space);
            double supplyAirflow = Analytical.Query.CalculatedSupplyAirFlow(space);

            index = Params.IndexOfOutputParam("Supply");
            if (index != -1)
            {
                dataAccess.SetData(index, supplyAirflow);
            }

            index = Params.IndexOfOutputParam("Exhaust");
            if (index != -1)
            {
                dataAccess.SetData(index, exhaustAirflow);
            }

            index = Params.IndexOfOutputParam("Supply_LpS");
            if (index != -1)
            {
                dataAccess.SetData(index, supplyAirflow * 1000);
            }

            index = Params.IndexOfOutputParam("Exhaust_LpS");
            if (index != -1)
            {
                dataAccess.SetData(index, exhaustAirflow * 1000);
            }
        }
    }
}
