using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCreateAddress : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9acfb75a-f2b3-490e-bd20-9e6b960351b1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCreateAddress()
          : base("SAMCore.CreateAddress", "SAMCore.CreateAddress",
              "Create Address",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            Address address = Core.Query.DefaultAddress();

            inputParamManager.AddTextParameter("_street_", "_street_", "Street", GH_ParamAccess.item, address.Street);
            inputParamManager.AddTextParameter("_city_", "_city_", "City", GH_ParamAccess.item, address.City);
            inputParamManager.AddTextParameter("_postalCode_", "_postalCode_", "Postal Code", GH_ParamAccess.item, address.PostalCode);
            inputParamManager.AddTextParameter("_countryCode_", "_countryCode_", "Country Code", GH_ParamAccess.item, address.CountryCode.ToString());
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAddressParam(), "address", "address", "SAM Core Address", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string street = string.Empty;
            if (!dataAccess.GetData(0, ref street))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string city = string.Empty;
            if (!dataAccess.GetData(1, ref city))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string postalCode = string.Empty;
            if (!dataAccess.GetData(2, ref postalCode))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            CountryCode countryCode = CountryCode.Undefined;

            GH_ObjectWrapper objectWrapper = null;
            dataAccess.GetData(3, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    countryCode = Core.Query.Enum<CountryCode>(((GH_String)objectWrapper.Value).Value);
                else
                    countryCode = Core.Query.Enum<CountryCode>(objectWrapper.Value.ToString());
            }

            dataAccess.SetData(0, new GooAddress(new Address(street, city, postalCode, countryCode)));
        }
    }
}