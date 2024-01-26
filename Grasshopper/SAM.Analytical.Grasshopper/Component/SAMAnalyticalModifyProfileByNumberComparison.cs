using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyProfileByNumberComparison : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("120b495d-a8b8-453c-9ab6-321cff363c81");

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
        public SAMAnalyticalModifyProfileByNumberComparison()
          : base("SAMAnalytical.ModifyProfileByNumberComparison", "SAMAnalytical.ModifyProfileByNumberComparison",
              "Modify Profile By Number Comparison",
              "SAM", "Analytical1")
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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "_profile", NickName = "_profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "name_", NickName = "name_", Description = "Profile Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_values", NickName = "_values", Description = "Values or Profile", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_numberComparisonType", NickName = "_numberComparisonType", Description = "NumberComparisonType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_valuesComparison", NickName = "_valuesComparison", Description = "Comparison Values or Profile", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "valueTrue_", NickName = "valueTrue_", Description = "True Value", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "valueFalse_", NickName = "valueFalse_", Description = "False Value", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "indexesTrue", NickName = "indexesTrue", Description = "True Indexes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "indexesFalse", NickName = "indexesFalse", Description = "False Indexes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_profile");
            Profile profile = null;
            if (index == -1 || !dataAccess.GetData(index, ref profile) || profile == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("name_");
            string name = null;
            if (index != -1)
            {
                if(!dataAccess.GetData(index, ref name))
                {
                    name = null;
                }
            }
            
            if(name == null)
            {
                name = profile.Name;
            }

            index = Params.IndexOfInputParam("_numberComparisonType");
            string @string = null;
            if (index == -1 || !dataAccess.GetData(index, ref @string) || @string == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!Core.Query.TryGetEnum(@string, out NumberComparisonType numberComparisonType))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double valueTrue = double.NaN;
            index = Params.IndexOfInputParam("valueTrue_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref valueTrue))
                {
                    valueTrue = double.NaN;
                }
            }

            double valueFalse = double.NaN;
            index = Params.IndexOfInputParam("valueFalse_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref valueFalse))
                {
                    valueFalse = double.NaN;
                }
            }


            List<GH_ObjectWrapper> objectWrappers = null;
            
            index = Params.IndexOfInputParam("_values");
            objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IndexedDoubles indexedDoubles_1 = new IndexedDoubles();
            for(int i =0; i < objectWrappers.Count; i++)
            {
                object @object = objectWrappers[i]?.Value;
                if(@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if(@object == null)
                {
                    continue;
                }

                if(Core.Query.IsNumeric(@object))
                {
                    if(Core.Query.TryConvert(@object, out double value))
                    {
                        indexedDoubles_1.Add(i, value);
                    }
                }
                else if(@object is Profile)
                {
                    IndexedDoubles indexDoubles_Temp = ((Profile)@object).GetIndexedDoubles();

                    List<int> keys = indexDoubles_Temp?.Keys?.ToList();
                    if (keys != null)
                    {
                        foreach (int key in keys)
                        {
                            indexedDoubles_1[key] = indexDoubles_Temp[key];
                        }
                    }
                }
                else if (@object is IndexedDoubles)
                {
                    IndexedDoubles indexDoubles_Temp = (IndexedDoubles)@object;

                    List<int> keys = indexDoubles_Temp?.Keys?.ToList();
                    if (keys != null)
                    {
                        foreach (int key in keys)
                        {
                            indexedDoubles_1[key] = indexDoubles_Temp[key];
                        }
                    }
                }
            }

            index = Params.IndexOfInputParam("_valuesComparison");
            objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IndexedDoubles indexedDoubles_2 = new IndexedDoubles();
            for (int i = 0; i < objectWrappers.Count; i++)
            {
                object @object = objectWrappers[i]?.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object == null)
                {
                    continue;
                }

                if (Core.Query.IsNumeric(@object))
                {
                    if (Core.Query.TryConvert(@object, out double value))
                    {
                        indexedDoubles_2.Add(i, value);
                    }
                }
                else if (@object is Profile)
                {
                    IndexedDoubles indexDoubles_Temp = ((Profile)@object).GetIndexedDoubles();

                    List<int> keys = indexDoubles_Temp?.Keys?.ToList();
                    if (keys != null)
                    {
                        foreach (int key in keys)
                        {
                            indexedDoubles_2[key] = indexDoubles_Temp[key];
                        }
                    }
                }
                else if (@object is IndexedDoubles)
                {
                    IndexedDoubles indexDoubles_Temp = (IndexedDoubles)@object;

                    List<int> keys = indexDoubles_Temp?.Keys?.ToList();
                    if (keys != null)
                    {
                        foreach (int key in keys)
                        {
                            indexedDoubles_2[key] = indexDoubles_Temp[key];
                        }
                    }
                }
            }

            IndexedDoubles indexedDoubles = !double.IsNaN(valueTrue) || !double.IsNaN(valueFalse) ? new IndexedDoubles() : null;
            List<int> indexesTrue = new List<int>();
            List<int> indexesFalse = new List<int>();
            for (int i = profile.Min; i <= profile.Max; i++)
            {
                if(indexedDoubles != null)
                {
                    indexedDoubles[i] = profile[i];
                }

                double value_1 = indexedDoubles_1[i];
                if(double.IsNaN(value_1))
                {
                    continue;
                }

                double value_2 = indexedDoubles_2[i];
                if (double.IsNaN(value_2))
                {
                    continue;
                }

                if(Core.Query.Compare(value_1, value_2, numberComparisonType))
                {
                    indexesTrue.Add(i);
                    if(!double.IsNaN(valueTrue))
                    {
                        indexedDoubles[i] = valueTrue;
                    }

                }
                else
                {
                    indexesFalse.Add(i);
                    if (!double.IsNaN(valueFalse))
                    {
                        indexedDoubles[i] = valueFalse;
                    }
                }
            }

            profile = new Profile(profile);
            if(name != null)
            {
                profile = new Profile(profile.Guid, profile, name, profile.Category);
            }

            if(indexedDoubles != null)
            {
                profile = new Profile(profile, indexedDoubles);
            }


            index = Params.IndexOfOutputParam("profile");
            if (index != -1)
            {
                dataAccess.SetData(index, profile);
            }

            index = Params.IndexOfOutputParam("indexesTrue");
            if (index != -1)
            {
                dataAccess.SetDataList(index, indexesTrue);
            }

            index = Params.IndexOfOutputParam("indexesFalse");
            if (index != -1)
            {
                dataAccess.SetDataList(index, indexesFalse);
            }
        }
    }
}