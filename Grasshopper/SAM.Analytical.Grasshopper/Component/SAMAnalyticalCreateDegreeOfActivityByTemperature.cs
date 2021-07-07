using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateDegreeOfActivityByTemperature : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ab1ac9cd-e8d4-4f11-83c2-818581632543");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

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
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;
            
            index = inputParamManager.AddTextParameter("_name_", "_name_", "Name ,default = Activity level", GH_ParamAccess.item, string.Empty);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddIntegerParameter("_activityLevel_", "_activityLevel_", "Activity level [1 - 4], I=100 W/p, II=125 W/p, III=170 W/p, IV=210 W/p ", GH_ParamAccess.item, 2);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddNumberParameter("_temperature_", "_temperature_", "Temperature [degC], will range between 16-28 default = 24 degC", GH_ParamAccess.item, 24);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooDegreeOfActivityParam(), "DegreeOfActivity", "DegreeOfActivity", "SAM Analytical DegreeOfActivity", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            dataAccess.GetData(0, ref name);

            int activityLevel = 0;
            dataAccess.GetData(1, ref activityLevel);

            if (activityLevel < 1 || activityLevel > 4)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double temperature = 0;
            dataAccess.GetData(2, ref temperature);

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

            dataAccess.SetData(0, new GooDegreeOfActivity(Create.DegreeOfActivity((ActivityLevel)activityLevel, name, temperature)));
        }
    }
}