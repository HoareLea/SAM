using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCalculateFloorArea : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c66cffd2-29e7-484a-b09c-e8f1cc8b60b0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCalculateFloorArea()
          : base("SAMAnalytical.CalculateFloorArea", "SAMAnalytical.CalculateFloorArea",
              "Calculates Floor Area from Space",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxTiltDifference_", NickName = "_maxTiltDifference_", Description = "Maximal Allowed Tilt Difference", Access = GH_ParamAccess.item };
                number.SetPersistentData(20);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Areas", NickName = "Areas", Description = "Calculated Areas", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
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

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if(index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            else if (sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = null;
            index = Params.IndexOfInputParam("spaces_");
            if (index != -1)
            {
                spaces = new List<Space>(); 
                dataAccess.GetDataList(index, spaces);
            }

            if (spaces == null || spaces.Count == 0)
                spaces = adjacencyCluster.GetSpaces();

            if(spaces != null && spaces.Count != 0)
            {
                double maxTiltDifference = 20;
                index = Params.IndexOfInputParam("_maxTiltDifference_");
                if (index != -1)
                {
                    double maxTiltDifference_Temp = 20;
                    if (dataAccess.GetData(index, ref maxTiltDifference_Temp))
                        maxTiltDifference = maxTiltDifference_Temp;
                }

                Dictionary<Space, double> dictionary_Area = new Dictionary<Space, double>();
                Dictionary<Space, List<Panel>> dictionary_Panel = new Dictionary<Space, List<Panel>>();
                foreach(Space space in spaces)
                {
                    if(space == null)
                    {
                        continue;
                    }

                    Space space_Temp = adjacencyCluster.GetObject<Space>(space.Guid);
                    if (space_Temp == null)
                    {
                        return;
                    }

                    dictionary_Area[space_Temp] = double.NaN;
                    dictionary_Panel[space_Temp] = new List<Panel>();
                }

                Parallel.For(0, dictionary_Area.Count, (int i) => {

                    Space space = dictionary_Area.Keys.ElementAt(i);
                    if(space == null)
                    {
                        return;
                    }

                    List<Panel> panels = Analytical.Query.GeomericalFloorPanels(adjacencyCluster, space, maxTiltDifference);
                    if (panels == null || panels.Count == 0)
                    {
                        return;
                    }

                    dictionary_Panel[space] = panels;
                    dictionary_Area[space] = panels.ConvertAll(x => x.GetArea()).Sum();
                });

                spaces = null;

                DataTree<GooPanel> dataTree_Panel = new DataTree<GooPanel>();
                if (dictionary_Panel != null && dictionary_Panel.Count > 0)
                {
                    spaces = new List<Space>();

                    int count = 0;
                    foreach(KeyValuePair<Space, List<Panel>> keyValuePair in dictionary_Panel)
                    {
                        spaces.Add(keyValuePair.Key);

                        GH_Path path = new GH_Path(count);
                        keyValuePair.Value.ForEach(x => dataTree_Panel.Add(new GooPanel(x), path));
                        count++;
                    }
                }

                index = Params.IndexOfOutputParam("Panels");
                if (index != -1)
                    dataAccess.SetDataTree(index, dataTree_Panel);

                index = Params.IndexOfOutputParam("Area");
                if (index != -1)
                    dataAccess.SetDataList(index, dictionary_Area?.Values);

                index = Params.IndexOfOutputParam("Analytical");
                if (index != -1)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                    foreach(KeyValuePair<Space, double> keyValuePair in dictionary_Area)
                    {
                        Space space = new Space(keyValuePair.Key);
                        space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                        adjacencyCluster.AddObject(space);

                        int index_Space = spaces.IndexOf(keyValuePair.Key);
                        if(index_Space != -1)
                        {
                            spaces[index_Space] = space;
                        }
                    }

                    if (sAMObject is AnalyticalModel)
                        sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                    else if (sAMObject is AdjacencyCluster)
                        sAMObject = adjacencyCluster;

                    dataAccess.SetData(index, sAMObject);
                }

                index = Params.IndexOfOutputParam("Spaces");
                if (index != -1)
                    dataAccess.SetDataList(index, spaces?.ConvertAll(x => new GooSpace(x)));


            }


        }
    }
}