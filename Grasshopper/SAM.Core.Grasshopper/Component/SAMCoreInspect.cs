using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreInspect : GH_Component, IGH_VariableParameterComponent, IGH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fe77c7c9-e24b-44d3-9e0b-d40a061cecbe");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public string LatestComponentVersion => "1.0.0";

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Inspect;

        public override bool Obsolete
        {
            get
            {
                return Query.Obsolete(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreInspect()
          : base("Inspect", "Inspect",
              "Inspect Object",
              "SAM", "Core")
        {
            SetValue("SAM_SAMVersion", Core.Query.CurrentVersion());
            SetValue("SAM_ComponentVersion", LatestComponentVersion);
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

            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);

            Modify.AppendSourceCodeAdditionalMenuItem(this, menu);
            Modify.AppendNewComponentAdditionalMenuItem(this, menu);
        }



        void PopulateOutputParameters(IEnumerable<GooObjectParam> gooParameterParams)
        {
            Dictionary<string, IList<IGH_Param>> dictionary = new Dictionary<string, IList<IGH_Param>>();
            foreach (IGH_Param param in Params.Output)
            {
                if (param.Recipients == null && param.Recipients.Count == 0)
                    continue;

                GooObjectParam gooParameterParam = param as GooObjectParam;
                if (gooParameterParam == null)
                    continue;

                dictionary.Add(gooParameterParam.Name, new List<IGH_Param>(gooParameterParam.Recipients));
            }

            while(Params.Output != null && Params.Output.Count() > 0)
                Params.UnregisterOutputParameter(Params.Output[0]);

            if (gooParameterParams != null)
            {
                foreach (GooObjectParam gooParameterParam in gooParameterParams)
                {
                    if (gooParameterParam == null)
                        continue;
                    
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

            PopulateOutputParameters(names_Temp.ConvertAll(x => new GooObjectParam(x)));
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
                    PopulateOutputParameters(new List<GooObjectParam>());
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

            PopulateOutputParameters(names_Result.ToList().ConvertAll(x => new GooObjectParam(x)));
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
                GooObjectParam gooParameterParam = Params.Output[i] as GooObjectParam;
                if (gooParameterParam == null)
                    gooParameterParam = new GooObjectParam(Params.Output[i].Name);

                if (gooParameterParam == null || string.IsNullOrWhiteSpace(gooParameterParam.Name))
                    continue;

                if (@object is Rhino.Geometry.GeometryBase)
                {
                    if (((Rhino.Geometry.GeometryBase)@object).Disposed)
                        break;
                }

                object result = null;

                @object.TryGetValue(gooParameterParam.Name, out result, true);

                if(result is IJSAMObject)
                {
                    dataAccess.SetData(i, new GooJSAMObject<IJSAMObject>((IJSAMObject)result));
                }
                if (result is JToken)
                {
                    dataAccess.SetData(i, new GooObject(result.ToString()));
                }
                else if (result is IEnumerable && !result.GetType().Namespace.StartsWith("SAM.") && !(result is string))
                {
                    List<GooObject> objects = new List<GooObject>();
                    foreach (object object_Result in (IEnumerable)result)
                    {
                        objects.Add(new GooObject(object_Result));
                    }

                    if(gooParameterParam.Access == GH_ParamAccess.item)
                    {
                        if(objects.Count == 0)
                        {
                            dataAccess.SetData(i, null);
                        }
                        else
                        {
                            dataAccess.SetDataList(i, objects);
                        }
                    }
                    else if (gooParameterParam.Access == GH_ParamAccess.list)
                    {
                        dataAccess.SetDataList(i, objects);
                    }

                    
                    //List<GooObject> gooObjects = new List<GooObject>();
                    //foreach (object object_Result in (IEnumerable)result)
                    //{
                    //    gooObjects.Add(new GooObject(object_Result));
                    //}

                    //dataAccess.SetDataList(i, gooObjects);

                    //bool isDataTree = false;
                    //foreach (object @object_Result in (IEnumerable)result)
                    //{
                    //    if (object_Result is IEnumerable && !object_Result.GetType().Namespace.StartsWith("SAM.") && !(object_Result is string))
                    //    {
                    //        isDataTree = true;
                    //        break;
                    //    }
                    //}

                    //if (isDataTree)
                    //{
                    //    DataTree<GooObject> dataTree = new DataTree<GooObject>();
                    //    int index = 0;
                    //    foreach (object @object_Result in (IEnumerable)result)
                    //    {
                    //        if (object_Result is IEnumerable && !object_Result.GetType().Namespace.StartsWith("SAM.") && !(object_Result is string))
                    //        {
                    //            int index_Temp = 0;
                    //            foreach (object @object_Result_Temp in (IEnumerable)object_Result)
                    //            {
                    //                dataTree.Add(new GooObject(@object_Result_Temp), new GH_Path(index));
                    //                index_Temp++;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            dataTree.Add(new GooObject(object_Result), new GH_Path(index));
                    //        }
                    //        index++;
                    //    }

                    //    dataAccess.SetDataTree(i, dataTree);

                    //}
                    //else
                    //{
                    //    List<GooObject> gooObjects = new List<GooObject>();
                    //    foreach (object object_Result in (IEnumerable)result)
                    //    {
                    //        gooObjects.Add(new GooObject(object_Result));
                    //    }

                    //    dataAccess.SetDataList(i, gooObjects);
                    //}




                }
                else
                {
                    if (result is string)
                    {
                        dataAccess.SetData(i, new GH_String((string)result));
                    }
                    else if (result is Enum)
                    {
                        dataAccess.SetData(i, new GooObject(result.ToString()));
                    }
                    else if(result is int || result is long)
                    {
                        int value;
                        if (Core.Query.TryConvert(result, out value))
                            dataAccess.SetData(i, new GH_Number(value));
                        else
                            dataAccess.SetData(i, new GooObject(result));
                    }
                    else if (Core.Query.IsNumeric(result))
                    {
                        double value;
                        if (Core.Query.TryConvert(result, out value))
                            dataAccess.SetData(i, new GH_Number(value));
                        else
                            dataAccess.SetData(i, new GooObject(result));
                    }
                    else if (result is bool)
                    {
                        dataAccess.SetData(i, new GH_Boolean((bool)result));
                    }
                    else if (result is DateTime)
                    {
                        dataAccess.SetData(i, new GH_Time((DateTime)result));
                    }
                    else
                    {
                        dataAccess.SetData(i, new GooObject(result));
                    }
                }

            }
        }

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index) => false;
        
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index) => side == GH_ParameterSide.Output;
        
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index) => null;
        
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index) => true;
        
        void IGH_VariableParameterComponent.VariableParameterMaintenance() { }


        public virtual void OnSourceCodeClick(object sender = null, object e = null)
        {
            Process.Start("https://github.com/HoareLea/SAM");
        }

        public string ComponentVersion
        {
            get
            {
                return GetValue("SAM_ComponentVersion", null);
            }
        }

        public string SAMVersion
        {
            get
            {
                return GetValue("SAM_SAMVersion", null);
            }
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Message = ComponentVersion;
        }
    }
}