using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCalculateOccupancy : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("33ea74fc-6836-4712-9de5-7b3aa9092cf1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCalculateOccupancy()
          : base("SAMAnalytical.CalculateOccupancy", "SAMAnalytical.CalculateOccupancy",
              "Calculates Occupancy Base on InternalCondition",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_spaces");
            List<Space> spaces = new List<Space>();
            if (!dataAccess.GetDataList(index, spaces))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

           
            if(sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = null;
                if(sAMObject is AnalyticalModel)
                    adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                else if(sAMObject is AdjacencyCluster)
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

                if(adjacencyCluster != null)
                {
                    if (spaces != null && spaces.Count > 0)
                    {
                        List<Space> spaces_Temp = new List<Space>();
                        foreach(Space space in adjacencyCluster.GetSpaces())
                        {
                            Space space_Temp = spaces.Find(x => x.Guid == space.Guid);
                            if (space_Temp != null)
                                spaces_Temp.Add(space_Temp);
                        }
                        spaces = spaces_Temp;
                    }
                    else
                    {
                        spaces = adjacencyCluster.GetSpaces();
                    }
                        

                    foreach(Space space in spaces)
                    {
                        if(!space.TryGetValue(SpaceParameter.Area, out double area) || double.IsNaN(area) || area == 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Space {0} (Guid: {1}) has no area", string.IsNullOrWhiteSpace(space.Name) ? "???" : space.Name, space.Guid));
                            continue;
                        }

                        InternalCondition internalCondition = space.InternalCondition;
                        if(internalCondition == null)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Space {0} (Guid: {1}) has no InternalCondition assigned", string.IsNullOrWhiteSpace(space.Name) ? "???" : space.Name, space.Guid));
                            continue;
                        }

                        if (!internalCondition.TryGetValue(InternalConditionParameter.AreaPerPerson, out double areaPerPerson) || double.IsNaN(areaPerPerson))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("InternalCondition {0} (Guid: {1}) for Space {2} (Guid: {3}) has invalid value for Area Per Person parameter", string.IsNullOrWhiteSpace(internalCondition.Name) ? "???" : internalCondition.Name, space.Guid, string.IsNullOrWhiteSpace(space.Name) ? "???" : space.Name, space.Guid));
                            continue;
                        }

                        Space space_Temp = new Space(space);
                        space_Temp.SetValue(SpaceParameter.Occupancy, area / areaPerPerson);
                        adjacencyCluster.AddObject(space_Temp);
                    }

                    if (sAMObject is AnalyticalModel)
                        sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                    else if (sAMObject is AdjacencyCluster)
                        sAMObject = adjacencyCluster;
                }
            }
            else if(sAMObject is Space)
            {

            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}