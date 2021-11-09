using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTriangulateConcavePanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5ca1e422-dc91-4cc0-8134-88a7c11ca95d");

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
        public SAMAnalyticalTriangulateConcavePanels()
          : base("SAMAnalytical.TriangulateConcavePanels", "SAMAnalytical.TriangulateConcavePanels",
              "Triangulate Concave Panels",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Objects such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerace_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analyticals", NickName = "analyticals", Description = "SAM Analytical Objects such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "oldPanels", NickName = "oldPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "newPanels", NickName = "newPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

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

            List<SAMObject> sAMObjects = null;
            index = Params.IndexOfInputParam("_analyticals");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = Tolerance.Distance;
            index = Params.IndexOfInputParam("_tolerace_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref tolerance))
                {
                    tolerance = Tolerance.Distance;
                }
            }

            List<SAMObject> newObjects = new List<SAMObject>();
            List<SAMObject> oldObjects = new List<SAMObject>();
            int count = sAMObjects.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (sAMObjects[i] is ArchitecturalModel)
                {
                    sAMObjects[i] = sAMObjects[i].Clone();

                    List<IPartition> newPartitions = Analytical.Modify.TriangulateConcavePartitions((ArchitecturalModel)sAMObjects[i], out List<IPartition> oldPartitions, tolerance);
                    if (newPartitions != null)
                    {
                        newPartitions.ForEach(x => newObjects.Add(x as SAMObject));
                    }

                    if (oldPartitions != null)
                    {
                        oldPartitions.ForEach(x => oldObjects.Add(x as SAMObject));
                    }
                }
                else if (sAMObjects[i] is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObjects[i]).AdjacencyCluster;

                    List<Panel> newPanels = Analytical.Modify.TriangulateConcavePanels(adjacencyCluster, out List<Panel> oldPanels, tolerance);
                    if (newPanels != null)
                    {
                        newPanels.ForEach(x => newObjects.Add(x));
                    }

                    if (oldPanels != null)
                    {
                        oldPanels.ForEach(x => oldObjects.Add(x));
                    }

                    sAMObjects[i] = new AnalyticalModel((AnalyticalModel)sAMObjects[i], adjacencyCluster);
                }
                else if(sAMObjects[i] is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = ((AdjacencyCluster)sAMObjects[i]).Clone<AdjacencyCluster>();

                    List<Panel> newPanels = Analytical.Modify.TriangulateConcavePanels(adjacencyCluster, out List<Panel> oldPanels, tolerance);
                    if (newPanels != null)
                    {
                        newPanels.ForEach(x => newObjects.Add(x));
                    }

                    if (oldPanels != null)
                    {
                        oldPanels.ForEach(x => oldObjects.Add(x));
                    }

                    sAMObjects[i] = adjacencyCluster;
                }
                else if(sAMObjects[i] is Panel)
                {
                    Panel panel = (Panel)sAMObjects[i];
                    if(Geometry.Spatial.Query.Concave(panel))
                    {
                        List<Panel> panels = panel.Triangulate(tolerance);
                        if(panels != null)
                        {
                            oldObjects.Add(panel);
                            panels.ForEach(x => newObjects.Add(panel));
                            sAMObjects.RemoveAt(i);
                            panels.ForEach(x => sAMObjects.Add(x));
                        }
                    }
                }
                else if (sAMObjects[i] is IPartition)
                {
                    IPartition partition = (IPartition)sAMObjects[i];
                    if (Geometry.Spatial.Query.Concave(partition))
                    {
                        List<IPartition> partitions = partition.Triangulate(tolerance);
                        if (partitions != null)
                        {
                            oldObjects.Add(partition as SAMObject);
                            partitions.ForEach(x => newObjects.Add(partition as SAMObject));
                            sAMObjects.RemoveAt(i);
                            partitions.ForEach(x => sAMObjects.Add(x as SAMObject));
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sAMObjects);
            }

            index = Params.IndexOfOutputParam("oldPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, oldObjects);
            }

            index = Params.IndexOfOutputParam("newPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, newObjects);
            }
        }
    }
}