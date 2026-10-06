// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateDegreeOfActivityByTemperature : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ab1ac9cd-e8d4-4f11-83c2-818581632543");

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
        public SAMAnalyticalCreateDegreeOfActivityByTemperature()
          : base("SAMAnalytical.CreateDegreeOfActivityByTemperature", "SAMAnalytical.CreateDegreeOfActivityByTemperature",
              "Create SAM Analytical DegreeOfActivity By RoomTemperature according to VDI 2078, EN 13779; Activity Level I (seating, relaxed), Activity Level II (seating office, school, lab), Activity Level II (standing, light activity, shop, lab, light industry), Activity Level IV (standing, moderate activity, lab assistant, working with machinery); I=100 W/p, II=125 W/p, III=170 W/p, IV=210 W/p",
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

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Name ,default = Activity level", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData(string.Empty);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer;
                param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_activityLevel_", NickName = "_activityLevel_", Description = "Activity level [1 - 4], I=100 W/p, II=125 W/p, III=170 W/p, IV=210 W/p ", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.SetPersistentData(2);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_temperature_", NickName = "_temperature_", Description = "Temperature [degC], will range between 16-28 default = 24 degC", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(24);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooDegreeOfActivityParam() { Name = "DegreeOfActivity", NickName = "DegreeOfActivity", Description = "SAM Analytical DegreeOfActivity", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            index = Params.IndexOfInputParam("_activityLevel_");
            int activityLevel = 0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref activityLevel);
            }

            if (activityLevel < 1 || activityLevel > 4)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_temperature_");
            double temperature = 0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref temperature);
            }

            if (string.IsNullOrEmpty(name))
            {
                string id = null;
                switch (activityLevel)
                {
                    case 1:
                        id = "I";
                        break;
                    case 2:
                        id = "II";
                        break;
                    case 3:
                        id = "III";
                        break;
                    case 4:
                        id = "IV";
                        break;
                }

                name = string.Format("Activity Level {0} ({1}C)", id, System.Math.Round(temperature, 0));
            }

            index = Params.IndexOfOutputParam("DegreeOfActivity");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooDegreeOfActivity(Create.DegreeOfActivity((ActivityLevel)activityLevel, name, temperature)));
            }
        }
    }
}
