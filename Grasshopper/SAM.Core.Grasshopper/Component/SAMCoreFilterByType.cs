using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
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
    public class SAMCoreFilterByType : GH_Component, IGH_VariableParameterComponent, IGH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5a03ff4c-e38d-4fa9-b95c-1bb50ca853be");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public string LatestComponentVersion => "1.0.0";

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

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
        public SAMCoreFilterByType()
          : base("SAMCore.FilterByType", "SAMCore.FilterByType",
              "Filter By Type",
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
            Menu_AppendItem(menu, "Get types", Menu_PopulateOutputParameters, hasInputData, false);
            Menu_AppendItem(menu, "Remove unconnected types", Menu_RemoveUnconnectedParameters, hasOutputParameters, false);

            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);

            Modify.AppendSourceCodeAdditionalMenuItem(this, menu);
            Modify.AppendNewComponentAdditionalMenuItem(this, menu);
        }

        private void PopulateOutputParameters(IEnumerable<GooObjectParam> gooParameterParams)
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

            while (Params.Output != null && Params.Output.Count() > 0)
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

        private void AddOutputParameter(IGH_Param param)
        {
            if (param.Attributes is null)
                param.Attributes = new GH_LinkedParamAttributes(param, Attributes);

            param.Access = GH_ParamAccess.list;
            Params.RegisterOutputParam(param);
        }

        private void Menu_PopulateOutputParameters(object sender, EventArgs e)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (object @object in Params.Input[0].VolatileData.AllData(true).OfType<object>())
            {
                object value = @object;
                if (@object is IGH_Goo)
                {
                    value = (@object as dynamic).Value;
                }

                string name = value?.GetType()?.Name;
                if(string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                names.Add(name);
            }

            RecordUndoEvent("Get Output Parameters");

            List<string> names_Temp = names.ToList();
            names_Temp.Sort();

            PopulateOutputParameters(names_Temp.ConvertAll(x => new GooObjectParam(x)));
        }

        private void Menu_RemoveUnconnectedParameters(object sender, EventArgs e)
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
            inputParamManager.AddGenericParameter("_objects", "_objects", "Objects", GH_ParamAccess.list);
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
            List<object> objects = new List<object>();
            if (!dataAccess.GetDataList(0, objects) || objects == null)
            {
                return;
            }

            List<Tuple<int, string, List<GooObject>>> tuples = new List<Tuple<int, string, List<GooObject>>>();
            for (int i = 0; i < Params.Output.Count; ++i)
            {
                GooObjectParam gooParameterParam = Params.Output[i] as GooObjectParam;
                if (gooParameterParam == null || string.IsNullOrWhiteSpace(gooParameterParam.Name))
                {
                    continue;
                }

                tuples.Add(new Tuple<int, string, List<GooObject>>(i, gooParameterParam.Name, new List<GooObject>()));
            }

            for (int i = 0; i < objects.Count; i++)
            {
                object @object = objects[i];

                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object is Rhino.Geometry.GeometryBase)
                {
                    if (((Rhino.Geometry.GeometryBase)@object).Disposed)
                    {
                        return;
                    }

                    @object = ((Rhino.Geometry.GeometryBase)@object).Duplicate();
                }

                if (@object is IJSAMObject)
                {
                    IJSAMObject jSAMObject = Core.Query.Clone((IJSAMObject)@object);
                    if (jSAMObject != null)
                        @object = jSAMObject;
                }

                string name = @object?.GetType()?.Name;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                List<GooObject> gooObjects = tuples.Find(x => name.Equals(x.Item2))?.Item3;
                if (gooObjects == null)
                {
                    continue;
                }

                gooObjects.Add(new GooObject(@object));

            }

            for (int i = 0; i < Params.Output.Count; ++i)
            {
                List<GooObject> gooObjects = tuples.Find(x => x.Item1 == i)?.Item3;
                dataAccess.SetDataList(i, gooObjects);
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