using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDefaultColorByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f233a26a-2eff-44be-bd5d-aefd0cbba6c8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalDefaultColorByPanelType()
          : base("SAMAnalytical.DefaultColor", "SAMAnalytical.DefaultColor",
              "Gets Default Color for given Panel (PanelType) or Aperture (ApertureType)",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "color", NickName = "color", Description = "Color", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_analytical");
            if(index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = objectWrapper.Value;
            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value;
            }

            System.Drawing.Color color = System.Drawing.Color.Transparent;

            if (@object is Panel)
            {
                Panel panel = @object as Panel;
                color = Analytical.Query.Color(panel);
            }
            else if (@object is Aperture)
            {
                Aperture aperture = @object as Aperture;
                ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                if(apertureConstruction != null)
                {
                    switch(apertureConstruction.ApertureType)
                    {
                        case ApertureType.Window:
                            color = Analytical.Query.Color(apertureConstruction.ApertureType, AperturePart.Pane);
                            break;

                        case ApertureType.Door:
                            color = Analytical.Query.Color(apertureConstruction.ApertureType, AperturePart.Frame);
                            break;
                    }

                    
                }
            }
            else if(@object is string)
            {
                PanelType panelType = Analytical.Query.PanelType((string)@object);
                if (panelType != PanelType.Undefined)
                {
                    color = Analytical.Query.Color(panelType);
                }
                else
                {
                    ApertureType apertureType = Analytical.Query.ApertureType((string)@object);
                    switch (apertureType)
                    {
                        case ApertureType.Window:
                            color = Analytical.Query.Color(apertureType, AperturePart.Pane);
                            break;

                        case ApertureType.Door:
                            color = Analytical.Query.Color(apertureType, AperturePart.Frame);
                            break;
                    }
                }
            }
            else
            {
                PanelType panelType = Analytical.Query.PanelType(@object);
                color = Analytical.Query.Color(panelType);
            }

            index = Params.IndexOfOutputParam("color");
            if (index != -1)
                dataAccess.SetData(index, color);
        }
    }
}