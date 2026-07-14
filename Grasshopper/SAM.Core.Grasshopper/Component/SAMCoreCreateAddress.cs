// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCreateAddress : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9acfb75a-f2b3-490e-bd20-9e6b960351b1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                Address address = Core.Query.DefaultAddress();

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_street_", NickName = "_street_", Description = "Street", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData(address.Street);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_city_", NickName = "_city_", Description = "City", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData(address.City);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_postalCode_", NickName = "_postalCode_", Description = "Postal Code", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData(address.PostalCode);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_countryCode_", NickName = "_countryCode_", Description = "Country Code", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData(address.CountryCode.ToString());
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAddressParam() { Name = "address", NickName = "address", Description = "SAM Core Address", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_street_");
            string street = string.Empty;
            if (index == -1 || !dataAccess.GetData(index, ref street))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_city_");
            string city = string.Empty;
            if (index == -1 || !dataAccess.GetData(index, ref city))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_postalCode_");
            string postalCode = string.Empty;
            if (index == -1 || !dataAccess.GetData(index, ref postalCode))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            CountryCode countryCode = CountryCode.Undefined;

            index = Params.IndexOfInputParam("_countryCode_");
            GH_ObjectWrapper objectWrapper = null;
            dataAccess.GetData(index, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    countryCode = Core.Query.Enum<CountryCode>(((GH_String)objectWrapper.Value).Value);
                else
                    countryCode = Core.Query.Enum<CountryCode>(objectWrapper.Value.ToString());
            }

            index = Params.IndexOfOutputParam("address");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAddress(new Address(street, city, postalCode, countryCode)));
            }
        }
    }
}
