using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

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
              "Create SAM Analytical DegreeOfActivity By RoomTemperature",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;
            
            index = inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item, string.Empty);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddIntegerParameter("_activityLevel", "_activityLevel", "Activity level [1 - 4]", GH_ParamAccess.item, 1);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddNumberParameter("temperature_", "temperature_", "Temperature [C]", GH_ParamAccess.item, 24);
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