using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using SAM.Core;

namespace SAM.Analytical.Grasshopper
{

    public abstract class A_SAMAnalyticalBase : GH_Component, IGH_VariableParameterComponent
    {
        protected A_SAMAnalyticalBase(string name, string nickname, string description, string category, string subCategory)
        : base(name, nickname, description, category, subCategory)
        { }

        protected override void RegisterInputParams(GH_InputParamManager manager)
        {
            manager.AddParameter(new global::Grasshopper.Kernel.Parameters.Param_String(), "Filter A", "A", string.Empty, GH_ParamAccess.item);
            manager.AddParameter(new global::Grasshopper.Kernel.Parameters.Param_String(), "Filter B", "B", string.Empty, GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager manager)
        {
            manager.AddParameter(new global::Grasshopper.Kernel.Parameters.Param_String(), "Filter", "F", string.Empty, GH_ParamAccess.item);
        }

        static int ToIndex(char value) => value - 'A';
        
        static char ToChar(int value) => (char)('A' + value);

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index <= ToIndex('Z') && index == Params.Input.Count;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index > ToIndex('B') && index == Params.Input.Count - 1;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Output) return default;

            var name = $"Filter {ToChar(index)}";
            var nickName = ToChar(index).ToString();
            return new global::Grasshopper.Kernel.Parameters.Param_String()
            {
                Name = name,
                NickName = global::Grasshopper.CentralSettings.CanvasFullNames ? name : nickName
            };
        }

        public bool DestroyParameter(GH_ParameterSide side, int index) => CanRemoveParameter(side, index);
        
        public void VariableParameterMaintenance() { }

        public override void AddedToDocument(GH_Document document)
        {
            global::Grasshopper.CentralSettings.CanvasFullNamesChanged += CentralSettings_CanvasFullNamesChanged;
            base.AddedToDocument(document);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            global::Grasshopper.CentralSettings.CanvasFullNamesChanged -= CentralSettings_CanvasFullNamesChanged;
            base.RemovedFromDocument(document);
        }

        private void CentralSettings_CanvasFullNamesChanged()
        {
            for (int i = 0; i < Params.Input.Count; ++i)
            {
                var param = Params.Input[i];
                var name = $"Filter {ToChar(i)}";
                var nickName = ToChar(i).ToString();

                if (global::Grasshopper.CentralSettings.CanvasFullNames)
                {
                    if (param.NickName == nickName)
                        param.NickName = name;
                }
                else
                {
                    if (param.NickName == name)
                        param.NickName = nickName;
                }
            }
        }
    }

    public class A_SAMAnalytical : A_SAMAnalyticalBase
    {
        public override Guid ComponentGuid => new Guid("206c04c6-5bfe-4af0-8099-db4a90b4e579");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public A_SAMAnalytical()
          : base("SAM.Test", "SAM.Test",
              "Test",
              "SAM", "SAM.Test")
        {

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            double ratio = double.NaN;

            var filters = new List<string>(Params.Input.Count);
            for (int i = 0; i < Params.Input.Count; ++i)
            {
                string filter = string.Empty;
                if (DA.GetData(i, ref filter) && filter is object)
                {
                    if(Core.Convert.ToSAM<Panel>(filter) is List<Panel> panels && panels.Count != 0)
                    {
                        panel = panels[0];
                    }
                    else if(Core.Query.TryConvert(filter, out double value))
                    {
                        ratio = value;
                    }
                }
            }

            if (panel == null || double.IsNaN(ratio))
            {
                return;
            }

            ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);

            List<Aperture> apertures = Analytical.Modify.AddApertures(panel, apertureConstruction, ratio);


            DA.SetData("Filter", string.Join("_", filters));
        }
    }
}