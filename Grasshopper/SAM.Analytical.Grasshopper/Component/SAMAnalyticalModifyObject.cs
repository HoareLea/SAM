using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyObject : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("069a51b6-9db4-4bbd-8461-6709d12b162f");

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
        public SAMAnalyticalModifyObject()
          : base("SAMAnalytical.ModifyObject", "SAMAnalytical.ModifyObject",
              "Modify Object to AdjacencyCluster or AnalyticalModel",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "objects_", NickName = "objects_", Description = "Object to be added", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            
            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("objects_");
            if (index != -1)
                dataAccess.GetDataList(index, sAMObjects);

            if(sAMObjects != null && sAMObjects.Count != 0)
            {
                AdjacencyCluster adjacencyCluster = null;
                
                if (sAMObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                    sAMObjects.FindAll(x => x is IMaterial).ForEach(x => analyticalModel.AddMaterial((IMaterial)x));
                    sAMObjects.FindAll(x => x is Profile).ForEach(x => analyticalModel.AddProfile((Profile)x));

                    sAMObject = analyticalModel;
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                if(adjacencyCluster != null)
                {
                    foreach (SAMObject sAMObject_Temp in sAMObjects)
                    {
                        if (sAMObject_Temp is Aperture)
                        {
                            Aperture aperture = (Aperture)sAMObject_Temp;

                            Panel panel = adjacencyCluster.GetPanel(aperture);
                            if (panel != null)
                            {
                                panel.RemoveAperture(aperture.Guid);
                                panel.AddAperture(aperture);
                                adjacencyCluster.AddObject(panel);
                            }

                            if (adjacencyCluster.Contains<Aperture>(aperture.Guid))
                            {
                                adjacencyCluster.AddObject(aperture);
                            }
                        }
                        else if (sAMObject_Temp is ApertureConstruction)
                        {
                            ApertureConstruction apertureConstruction = (ApertureConstruction)sAMObject_Temp;

                            List<Panel> panels = adjacencyCluster.GetPanels(apertureConstruction);
                            if (panels != null)
                            {
                                foreach (Panel panel in panels)
                                {
                                    List<Aperture> apertures = panel.GetApertures(apertureConstruction);
                                    if (apertures != null)
                                    {
                                        foreach (Aperture aperture in apertures)
                                        {
                                            panel.RemoveAperture(aperture.Guid);
                                            panel.AddAperture(new Aperture(aperture, apertureConstruction));
                                        }
                                        adjacencyCluster.AddObject(panel);
                                    }
                                }
                            }

                            if (adjacencyCluster.Contains<ApertureConstruction>(apertureConstruction.Guid))
                            {
                                adjacencyCluster.AddObject(apertureConstruction);
                            }
                        }
                        else if (sAMObject_Temp is Construction)
                        {
                            Construction construction = (Construction)sAMObject_Temp;

                            List<Panel> panels = adjacencyCluster.GetPanels(construction);
                            if (panels != null)
                            {
                                foreach (Panel panel in panels)
                                {
                                    adjacencyCluster.AddObject(Create.Panel(panel, construction));
                                }
                            }

                            if (adjacencyCluster.Contains<Construction>(construction.Guid))
                            {
                                adjacencyCluster.AddObject(construction);
                            }
                        }
                        else if (sAMObject_Temp is InternalCondition)
                        {
                            InternalCondition internalCondition = (InternalCondition)sAMObject_Temp;

                            List<Space> spaces = adjacencyCluster.GetSpaces();
                            if (spaces != null)
                            {
                                foreach (Space space in spaces)
                                {
                                    InternalCondition internalCodintion_Space = space.InternalCondition;
                                    if(internalCodintion_Space != null && internalCodintion_Space.Guid == internalCondition.Guid)
                                    {
                                        space.InternalCondition = internalCondition;
                                        adjacencyCluster.AddObject(space);
                                    }
                                }
                            }

                            if (adjacencyCluster.Contains<InternalCondition>(internalCondition.Guid))
                            {
                                adjacencyCluster.AddObject(internalCondition);
                            }
                        }
                        else if (adjacencyCluster.IsValid(sAMObject_Temp))
                        {
                            adjacencyCluster.AddObject(sAMObject_Temp);
                        }
                    }
                    
                    
                    if (sAMObject is AnalyticalModel)
                    {
                        sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                    }
                    else if (sAMObject is AdjacencyCluster)
                    {
                        sAMObject = adjacencyCluster;
                    }
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}