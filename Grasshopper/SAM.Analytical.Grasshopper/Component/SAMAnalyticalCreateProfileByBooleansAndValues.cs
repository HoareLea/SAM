using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByBooleansAndValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1fb99ccf-ec46-458b-934e-d165b7cef068");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateProfileByBooleansAndValues()
          : base("SAMAnalytical.CreateProfileByBooleansAndValues", "SAMAnalytical.CreateProfileByBooleansAndValues",
              "Create Profile By Bools and Values",
              "SAM", "Analytical01")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_name", NickName = "_name", Description = "Profile Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "category_", NickName = "category_", Description = "Profile Category", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_bools", NickName = "_bools", Description = "Bools as value or list", Access = GH_ParamAccess.list }, ParamVisibility.Binding));


                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "_valueTrue", NickName = "_valueTrue", Description = "Value True", Access = GH_ParamAccess.item };
                number.SetPersistentData(1);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "_valueFalse", NickName = "_valueFalse", Description = "Value False", Access = GH_ParamAccess.item };
                number.SetPersistentData(0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profile", NickName = "profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_name");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("category_");
            string category = null;
            if (index != -1)
            {
                if(!dataAccess.GetData(index, ref category))
                {
                    category = null;
                }
            }

            index = Params.IndexOfInputParam("_bools");
            List<bool> bools = new List<bool>();
            if (index == -1 || !dataAccess.GetDataList(index, bools) || bools == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_valueTrue");
            double valueTrue = 1;
            if (index == -1 || !dataAccess.GetData(index, ref valueTrue))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_valueFalse");
            double valueFalse = 0;
            if (index == -1 || !dataAccess.GetData(index, ref valueFalse))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<double> values = bools.ConvertAll(x => x ? valueTrue : valueFalse);

            Profile profile = new Profile(name, category, values);

            index = Params.IndexOfOutputParam("profile");
            if (index != -1)
            {
                dataAccess.SetData(index, profile);
            }
        }
    }
}