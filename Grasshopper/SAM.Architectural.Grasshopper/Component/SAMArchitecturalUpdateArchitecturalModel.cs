using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class SAMArchitecturalUpdateArchitecturalModel : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("45587d1b-9161-4daf-adfc-a1c229493e57");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalUpdateArchitecturalModel()
          : base("SAMArchitectural.UpdateArchitecturalModel", "SAMArchitectural.UpdateArchitecturalModel",
              "Update ArchitecturalModel",
              "SAM", "Architectural")
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
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "_architecturalModel", NickName = "_architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooLocationParam() { Name = "location_", NickName = "location_", Description = "SAM Core Location", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooAddressParam() { Name = "address_", NickName = "address_", Description = "SAM Core Address", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "architecturalModel", NickName = "architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ArchitecturalModel architecturalModel = null;
            index = Params.IndexOfInputParam("_architecturalModel");
            if (index == -1 || !dataAccess.GetData(index, ref architecturalModel) || architecturalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            architecturalModel = new ArchitecturalModel(architecturalModel);

            index = Params.IndexOfInputParam("location_");
            if (index != -1)
            {
                Core.Location location = null;
                if(dataAccess.GetData(index, ref location) && location != null)
                {
                    architecturalModel.Location = location;
                }
            }

            index = Params.IndexOfInputParam("address_");
            if (index != -1)
            {
                Core.Address address = null;
                if (dataAccess.GetData(index, ref address) && address != null)
                {
                    architecturalModel.Address = address;
                }
            }

            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                string description = null;
                if (dataAccess.GetData(index, ref description) && description != null)
                {
                    architecturalModel.Description = description;
                }
            }

            index = Params.IndexOfOutputParam("architecturalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooArchitecturalModel(architecturalModel));
        }
    }
}