using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreInspect : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fe77c7c9-e24b-44d3-9e0b-d40a061cecbe");

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Explode;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreInspect()
          : base("Inspect", "Inspect",
              "Inspect Object",
              "SAM", "Core")
        {
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            bool hasInputData = !Params.Input[0].VolatileData.IsEmpty;
            bool hasOutputParameters = Params.Output.Count > 0;

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Get common parameters", Menu_PopulateOutputsWithCommonParameters, hasInputData, false);
            Menu_AppendItem(menu, "Get all parameters", Menu_PopulateOutputsWithAllParameters, hasInputData, false);
            Menu_AppendItem(menu, "Remove unconnected parameters", Menu_RemoveUnconnectedParameters, hasOutputParameters, false);
        }

        void PopulateOutputParameters(IEnumerable<GooParameterParam> gooParameterParams)
        {
            var connected = new Dictionary<GooParameterParam, IList<IGH_Param>>();
            foreach (var output in Params.Output.ToArray())
            {
                if(output.Recipients.Count > 0 && output is GooParameterParam gooParameterParam)
                    connected.Add(gooParameterParam, gooParameterParam.Recipients.ToArray());

                Params.UnregisterOutputParameter(output);
            }

            if (gooParameterParams != null)
            {
                foreach (GooParameterParam gooParameterParam in gooParameterParams)
                {
                    AddOutputParameter(gooParameterParam);

                    if (connected.TryGetValue(gooParameterParam, out var recipients))
                    {
                        foreach (var recipient in recipients)
                            recipient.AddSource(gooParameterParam);
                    }
                }
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        void AddOutputParameter(IGH_Param param)
        {
            if (param.Attributes is null)
                param.Attributes = new GH_LinkedParamAttributes(param, Attributes);

            param.Access = GH_ParamAccess.item;
            Params.RegisterOutputParam(param);
        }

        void Menu_PopulateOutputsWithAllParameters(object sender, EventArgs e)
        {
            var all = new HashSet<GooParameterParam>();
            foreach (var goo in Params.Input[0].VolatileData.AllData(true).OfType<object>())
            {
                var element = goo;
                if (element is null)
                    continue;

                List<string> names = element.UserFriendlyNames();
                if (names == null && names.Count == 0)
                    continue;

                foreach (string name in names)
                    all.Add(new GooParameterParam(name));
            }

            RecordUndoEvent("Get All Parameters");

            PopulateOutputParameters(all);
        }

        void Menu_PopulateOutputsWithCommonParameters(object sender, EventArgs e)
        {
            var common = default(HashSet<GooParameterParam>);
            foreach (object @object in Params.Input[0].VolatileData.AllData(true).OfType<object>())
            {
                object value = @object;
                if(@object is IGH_Goo)
                    value = (@object as dynamic).Value;

                if (value is null)
                    continue;

                var current = new HashSet<GooParameterParam>();

                List<string> names = value.UserFriendlyNames();
                foreach (var name in names)
                    current.Add(new GooParameterParam(name));

                if (common is null)
                    common = current;
                else
                    common.IntersectWith(current);
            }

            RecordUndoEvent("Get Common Parameters");

            PopulateOutputParameters(common);
        }

        void Menu_RemoveUnconnectedParameters(object sender, EventArgs e)
        {
            RecordUndoEvent("Remove Unconnected Outputs");

            foreach (var output in Params.Output.ToArray())
            {
                if (output.Recipients.Count > 0)
                    continue;

                Params.UnregisterOutputParameter(output);
            }

            Params.OnParametersChanged();
            OnDisplayExpired(false);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_object", "_object", "Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            object @object = null;
            if (!dataAccess.GetData(0, ref @object))
                return;

            if(@object is IGH_Goo)
                @object = (@object as dynamic).Value;

            for (int i = 0; i < Params.Output.Count; ++i)
            {
                GooParameterParam gooParameterParam = Params.Output[i] as GooParameterParam;
                if (gooParameterParam == null)
                    continue;

                object result = null;
                
                @object.TryGetValue(gooParameterParam.Name, out result, true);

                if(result is IEnumerable && !(result is string))
                {
                    List<GooParameter> gooParameters = new List<GooParameter>();
                    foreach (object @object_Result in (IEnumerable)result)
                        gooParameters.Add(new GooParameter(@object_Result));

                    dataAccess.SetDataList(i, gooParameters);
                }
                else
                {
                    dataAccess.SetData(i, new GooParameter(result));
                }
                
                

            }
        }

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index) => false;
        
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => side == GH_ParameterSide.Output;
        
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index) => null;
        
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index) => true;
        
        void IGH_VariableParameterComponent.VariableParameterMaintenance() { }
    }
}