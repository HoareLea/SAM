using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    [Obsolete("Obsolete since 2021.11.24")]
    public class SAMAnalyticalCreateOpeningsBy3DGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e1079084-3a9b-408f-a10d-fa10f3519b89");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateOpeningsBy3DGeometry()
          : base("SAMAnalytical.CreateOpeningsBy3DGeometry", "SAMAnalytical.CreateOpeningsBy3DGeometry",
              "Create SAM Analytical Opening by 3D Geometry",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "_geometry", Description = "Elevations", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                GooOpeningTypeParam gooOpeningTypeParam = new GooOpeningTypeParam() { Name = "openingType_", NickName = "openingType_", Description = "SAM Analytical OpeningType", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooOpeningTypeParam, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooOpeningParam() { Name = "openings", NickName = "openings", Description = "SAM Analytical Openings", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("openingType_");
            OpeningType openingType = null;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref openingType))
                {
                    openingType = null;
                }
            }

            index = Params.IndexOfInputParam("_geometry");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IOpening> openings = new List<IOpening>();
            foreach (Face3D face3D in face3Ds)
            {
                OpeningType openingType_Temp = openingType;
                if (openingType_Temp == null)
                {
                    OpeningTypeLibrary openingTypeLibrary = Analytical.Query.DefaultOpeningTypeLibrary();
                    if (openingTypeLibrary == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could Not Get Default OpeningTypes");
                        return;
                    }

                    PartitionAnalyticalType partitionAnalyticalType = PartitionAnalyticalType.Undefined;

                    HostPartitionCategory hostPartitionCategory = Analytical.Query.HostPartitionCategory(face3D.GetPlane().Normal);
                    if(hostPartitionCategory == HostPartitionCategory.Floor)
                    {
                        partitionAnalyticalType = PartitionAnalyticalType.ExternalFloor;
                    }
                    else if(hostPartitionCategory == HostPartitionCategory.Roof)
                    {
                        partitionAnalyticalType = PartitionAnalyticalType.Roof;
                    }
                    else if (hostPartitionCategory == HostPartitionCategory.Wall)
                    {
                        partitionAnalyticalType = PartitionAnalyticalType.ExternalWall;
                    }
                    
                    openingType_Temp = openingTypeLibrary.GetOpeningTypes(OpeningAnalyticalType.Window, partitionAnalyticalType)?.FirstOrDefault();
                }

                if (openingType_Temp == null)
                {
                    continue;
                }

                IOpening opening = Create.Opening(openingType_Temp, face3D);
                if (opening == null)
                {
                    continue;
                }

                openings.Add(opening);
            }

            index = Params.IndexOfOutputParam("openings");
            if(index != -1)
            {
                dataAccess.SetDataList(index, openings.ConvertAll(x => new GooOpening(x)));
            }
        }
    }
}