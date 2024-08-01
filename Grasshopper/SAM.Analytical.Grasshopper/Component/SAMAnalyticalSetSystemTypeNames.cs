using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetSystemTypeNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("98f6dda3-da1c-4102-8192-9bf9136cb552");

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
        public SAMAnalyticalSetSystemTypeNames()
          : base("SAMAnalytical.SetSystemTypeNames", "SAMAnalytical.SetSystemTypeNames",
              "Align Panels",
              "SAM WIP", "Analytical")
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

                GooAnalyticalModelParam analyticalModelParam = new GooAnalyticalModelParam() { Name = "analyticalModel_", NickName = "analyticalModel_", Description = "SAM Analytical Model", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject paramGenericObject;

                paramGenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_spacesOrZones_", NickName = "_spacesOrZones_", Description = "SAM Analytical Spaces or Zones", Access = GH_ParamAccess.list, Optional = true };
                paramGenericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(paramGenericObject, ParamVisibility.Binding));

                paramGenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "ventilationSystemTypeNames_", NickName = "ventilationSystemTypeNames_", Description = "VentilationSystemType or Ventilation System Type Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(paramGenericObject, ParamVisibility.Binding));

                paramGenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "heatingSystemTypeNames_", NickName = "heatingSystemTypeNames_", Description = "HeatingSystemType or Heating System Type Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(paramGenericObject, ParamVisibility.Binding));

                paramGenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "coolingSystemTypeNames_", NickName = "coolingSystemTypeNames_", Description = "CoolingSystemType or Cooling System Type Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(paramGenericObject, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticals", NickName = "analyticals", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("analyticalModel_");
            AnalyticalModel analyticalModel = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref analyticalModel);
            }

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            List<GH_ObjectWrapper> objectWrappers = null;

            List<List<Space>> spacesList = new List<List<Space>>();

            index = Params.IndexOfInputParam("_spacesOrZones_");
            if (index != -1)
            {
                objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(index, objectWrappers);
                foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper?.Value;
                    if (value is IGH_Goo)
                    {
                        value = (value as dynamic).Value;
                    }

                    List<Space> spaces = null;
                    if(value is Zone)
                    {
                        spaces = analyticalModel.GetSpaces((Zone)value);
                    }
                    else if (value is Space)
                    {
                        spaces = new List<Space>() { (Space)value };
                    }
                    else if(value is string)
                    {
                        spaces = new List<Space>() { analyticalModel?.GetSpaces()?.Find(x => x?.Name == (string)value) };
                    }

                    spacesList.Add(spaces);
                }
            }

            List<string> ventilationSystemTypeNames = new List<string>();
            index = Params.IndexOfInputParam("ventilationSystemTypeNames_");
            if (index != -1)
            {
                objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(index, objectWrappers);
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper?.Value;
                    if (value is IGH_Goo)
                        value = (value as dynamic).Value;

                    string name = null;
                    if (value is SAMObject)
                    {
                        name = (value as SAMObject).Name;
                    }
                    else if (value is string)
                    {
                        name = (string)value;
                    }

                    ventilationSystemTypeNames.Add(name);
                }
            }

            List<string> heatingSystemTypeNames = new List<string>();
            index = Params.IndexOfInputParam("heatingSystemTypeNames_");
            if (index != -1)
            {
                objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(index, objectWrappers);
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper?.Value;
                    if (value is IGH_Goo)
                        value = (value as dynamic).Value;

                    string name = null;
                    if (value is SAMObject)
                    {
                        name = (value as SAMObject).Name;
                    }
                    else if (value is string)
                    {
                        name = (string)value;
                    }

                    heatingSystemTypeNames.Add(name);
                }
            }

            List<string> coolingSystemTypeNames = new List<string>();
            index = Params.IndexOfInputParam("coolingSystemTypeNames_");
            if (index != -1)
            {
                objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(index, objectWrappers);
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper?.Value;
                    if (value is IGH_Goo)
                        value = (value as dynamic).Value;

                    string name = null;
                    if (value is SAMObject)
                    {
                        name = (value as SAMObject).Name;
                    }
                    else if (value is string)
                    {
                        name = (string)value;
                    }

                    coolingSystemTypeNames.Add(name);
                }
            }



            for (int i = 0; i < spacesList.Count; i++)
            {
                List<Space> spaces = spacesList[i];
                if(spaces == null || spaces.Count == 0)
                {
                    continue;
                }

                string ventilationSystemTypeName = ventilationSystemTypeNames.Count > i ? ventilationSystemTypeNames[i] : null;              
                string heatingSystemTypeName = heatingSystemTypeNames.Count > i ? heatingSystemTypeNames[i] : null;
                string coolingSystemTypeName = coolingSystemTypeNames.Count > i ? coolingSystemTypeNames[i] : null;

                for (int j = 0; j < spaces.Count; j++)
                {
                    Space space = spaces[j];
                    if(space != null)
                    {
                        space = adjacencyCluster.GetObject<Space>(space.Guid);
                    }

                    if (space == null)
                    {
                        continue;
                    }

                    InternalCondition internalCondition = space.InternalCondition;
                    if (internalCondition == null)
                    {
                        internalCondition = new InternalCondition(space.Name);
                    }

                    if (!string.IsNullOrWhiteSpace(ventilationSystemTypeName))
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);
                    }
                    else
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationSystemTypeName);
                    }

                    if (!string.IsNullOrWhiteSpace(heatingSystemTypeName))
                    {
                        internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, heatingSystemTypeName);
                    }
                    else
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.HeatingSystemTypeName);
                    }

                    if (!string.IsNullOrWhiteSpace(coolingSystemTypeName))
                    {
                        internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, coolingSystemTypeName);
                    }
                    else
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.CoolingSystemTypeName);
                    }

                }
            }

            if(adjacencyCluster != null)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, new GooAnalyticalModel[] { new GooAnalyticalModel(analyticalModel) });
            }
        }
    }
}