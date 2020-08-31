using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
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
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Inspect;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreInspect()
          : base("Inspect", "Inspect",
              "Inspect Object",
              "SAM", "Core")
        {
            Message = Core.Query.CurrentVersion();
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
            Dictionary<string, IList<IGH_Param>> dictionary = new Dictionary<string, IList<IGH_Param>>();
            foreach (IGH_Param param in Params.Output)
            {
                if (param.Recipients == null && param.Recipients.Count == 0)
                    continue;

                GooParameterParam gooParameterParam = param as GooParameterParam;
                if (gooParameterParam == null)
                    continue;

                dictionary.Add(gooParameterParam.Name, new List<IGH_Param>(gooParameterParam.Recipients));
            }

            while(Params.Output != null && Params.Output.Count() > 0)
                Params.UnregisterOutputParameter(Params.Output[0]);

            if (gooParameterParams != null)
            {
                foreach (GooParameterParam gooParameterParam in gooParameterParams)
                {
                    AddOutputParameter(gooParameterParam);

                    IList<IGH_Param> @params = null;

                    if (!dictionary.TryGetValue(gooParameterParam.Name, out @params))
                        continue;

                    foreach (IGH_Param param in @params)
                        param.AddSource(gooParameterParam);
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
            HashSet<string> names = new HashSet<string>();
            foreach (object @object in Params.Input[0].VolatileData.AllData(true).OfType<object>())
            {
                object value = @object;
                if (@object is IGH_Goo)
                    value = (@object as dynamic).Value;

                value?.UserFriendlyNames()?.ForEach(x => names.Add(x));
            }

            RecordUndoEvent("Get Common Parameters");

            List<string> names_Temp = names.ToList();
            names_Temp.Sort();

            PopulateOutputParameters(names_Temp.ConvertAll(x => new GooParameterParam(x)));
        }

        void Menu_PopulateOutputsWithCommonParameters(object sender, EventArgs e)
        {
            HashSet<string> names_Unique = new HashSet<string>();
            List<HashSet<string>> names_Objects = new List<HashSet<string>>();
            foreach (object @object in Params.Input[0].VolatileData.AllData(true).OfType<object>())
            {
                object value = @object;
                if (@object is IGH_Goo)
                    value = (@object as dynamic).Value;

                List<string> names = value?.UserFriendlyNames();
                if (names == null || names.Count == 0)
                {
                    PopulateOutputParameters(new List<GooParameterParam>());
                    return;
                }

                HashSet<string> names_Unique_Temp = new HashSet<string>();
                names.ForEach(x => names_Unique_Temp.Add(x));

                names.ForEach(x => names_Unique.Add(x));
                names_Objects.Add(names_Unique_Temp);
            }

            RecordUndoEvent("Get Common Parameters");

            List<string> names_Temp = names_Unique.ToList();
            names_Temp.Sort();

            HashSet<string> names_Result = new HashSet<string>();
            foreach (string name in names_Temp)
            {
                if (names_Objects.TrueForAll(x => x.Contains(name)))
                    names_Result.Add(name);
            }

            PopulateOutputParameters(names_Result.ToList().ConvertAll(x => new GooParameterParam(x)));
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

            if (@object is IGH_Goo)
                @object = (@object as dynamic).Value;

            if (@object is Rhino.Geometry.GeometryBase)
            {
                if (((Rhino.Geometry.GeometryBase)@object).Disposed)
                    return;

                @object = ((Rhino.Geometry.GeometryBase)@object).Duplicate();
            }

            if (@object is IJSAMObject)
            {
                IJSAMObject jSAMObject = Core.Query.Clone((IJSAMObject)@object);
                if (jSAMObject != null)
                    @object = jSAMObject;
            }

            for (int i = 0; i < Params.Output.Count; ++i)
            {
                GooParameterParam gooParameterParam = Params.Output[i] as GooParameterParam;
                if (gooParameterParam == null)
                    gooParameterParam = new GooParameterParam(Params.Output[i].Name);

                if (gooParameterParam == null || string.IsNullOrWhiteSpace(gooParameterParam.Name))
                    continue;

                if (@object is Rhino.Geometry.GeometryBase)
                {
                    if (((Rhino.Geometry.GeometryBase)@object).Disposed)
                        break;
                }

                object result = null;

                @object.TryGetValue(gooParameterParam.Name, out result, true);

                if (result is JToken)
                {
                    dataAccess.SetData(i, new GooParameter(result.ToString()));
                }
                else if (result is IEnumerable && !result.GetType().Namespace.StartsWith("SAM.") && !(result is string))
                {
                    List<GooParameter> gooParameters = new List<GooParameter>();
                    foreach (object @object_Result in (IEnumerable)result)
                        gooParameters.Add(new GooParameter(@object_Result));

                    dataAccess.SetDataList(i, gooParameters);
                }
                else
                {
                    if (result is string)
                    {
                        dataAccess.SetData(i, new GH_String((string)result));
                    }
                    else if (Core.Query.IsNumeric(result))
                    {
                        double value;
                        if (Core.Query.TryConvert(result, out value))
                            dataAccess.SetData(i, new GH_Number((double)result));
                        else
                            dataAccess.SetData(i, new GooParameter(result));
                    }
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