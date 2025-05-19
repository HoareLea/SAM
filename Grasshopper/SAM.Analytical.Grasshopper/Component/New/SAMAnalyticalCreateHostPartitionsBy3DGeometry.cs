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
    public class SAMAnalyticalCreateHostPartitionBy3DGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4b1179d5-66d6-411b-92f6-7137e3cc1e25");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateHostPartitionBy3DGeometry()
          : base("SAMAnalytical.CreateHostPartitionBy3DGeometry", "SAMAnalytical.CreateHostPartitionBy3DGeometry",
              "Create SAM Analytical IHostPartition by 3D Geometry",
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

                GooHostPartitionTypeParam gooHostPartitionTypeParam = new GooHostPartitionTypeParam() { Name = "hostPartitionType_", NickName = "hostPartitionType_", Description = "SAM Analytical HostPartitionType", Optional = true, Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooHostPartitionTypeParam, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooPartitionParam() { Name = "hostPartitions", NickName = "hostPartitions", Description = "SAM Analytical IHostPartition", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("hostPartitionType_");
            HostPartitionType hostPartitionType = null;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref hostPartitionType))
                {
                    hostPartitionType = null;
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

            List<IHostPartition> hostPartitions = new List<IHostPartition>();
            foreach (Face3D face3D in face3Ds)
            {
                HostPartitionType hostPartitionType_Temp = hostPartitionType;
                if (hostPartitionType_Temp == null)
                {
                    HostPartitionTypeLibrary hostPartitionTypeLibrary = Analytical.Query.DefaultHostPartitionTypeLibrary();
                    if (hostPartitionTypeLibrary == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could Not Get Default HostPartitionTypes");
                        return;
                    }

                    HostPartitionCategory hostPartitionCategory = Analytical.Query.HostPartitionCategory(face3D.GetPlane().Normal);
                    if (hostPartitionCategory == HostPartitionCategory.Undefined)
                    {
                        hostPartitionCategory = HostPartitionCategory.Wall;
                    }

                    if (hostPartitionCategory == HostPartitionCategory.Floor)
                    {
                        hostPartitionType_Temp = hostPartitionTypeLibrary.GetHostPartitionType<HostPartitionType>(PartitionAnalyticalType.InternalFloor);
                    }

                    if (hostPartitionCategory == HostPartitionCategory.Wall)
                    {
                        hostPartitionType_Temp = hostPartitionTypeLibrary.GetHostPartitionType<HostPartitionType>(PartitionAnalyticalType.ExternalWall);
                    }

                    if (hostPartitionType_Temp == null)
                    {
                        hostPartitionType_Temp = hostPartitionTypeLibrary.GetHostPartitionTypes(hostPartitionCategory)?.FirstOrDefault();
                    }

                    if (hostPartitionType_Temp == null)
                    {
                        hostPartitionType_Temp = hostPartitionTypeLibrary.GetHostPartitionType<HostPartitionType>(PartitionAnalyticalType.ExternalWall);
                    }
                }

                if (hostPartitionType_Temp == null)
                {
                    continue;
                }

                IHostPartition hostPartition = Create.HostPartition(face3D, hostPartitionType_Temp);
                if (hostPartition == null)
                {
                    continue;
                }

                hostPartitions.Add(hostPartition);
            }

            index = Params.IndexOfOutputParam("hostPartitions");
            if(index != -1)
            {
                dataAccess.SetDataList(index, hostPartitions.ConvertAll(x => new GooPartition(x)));
            }
        }
    }
}