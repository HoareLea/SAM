// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySAMGeometry2D : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("50eb9b79-0a5f-4fb7-938d-451ff9432eee");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySAMGeometry2D()
          : base("SAMGeometry.SAMGeometry2D", "SAMGeometry.SAMGeometry2D",
              "Convert SAM geometry 3D to SAM geometry 2D",
              "SAM", "Geometry")
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
                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObjectParameter;

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_SAMGeometry3D", NickName = "_SAMGeometry3D", Description = "SAM Geometry 3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_ownPlane", NickName = "_ownPlane", Description = "Projection on own plane if possible", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                genericObjectParameter = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Plane", NickName = "Plane", Description = "SAM Plane", Access = GH_ParamAccess.item, Optional = true };
                genericObjectParameter.SetPersistentData(new GH_Plane(new global::Rhino.Geometry.Plane(new global::Rhino.Geometry.Point3d(0, 0, 0), new global::Rhino.Geometry.Vector3d(0, 0, 1))));
                result.Add(new GH_SAMParam(genericObjectParameter, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "sAMGeometry2D", NickName = "sAMgeo2D", Description = "SAM Geometry 2D", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("_SAMGeometry3D");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<ISAMGeometry3D> sAMGeometry3Ds) || sAMGeometry3Ds == null || sAMGeometry3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IPlanar3D> planars = new List<IPlanar3D>();
            List<ISAMGeometry3D> notPlanars = new List<ISAMGeometry3D>();

            foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
            {
                if (sAMGeometry3D == null)
                {
                    continue;
                }

                if (sAMGeometry3D is Shell)
                {
                    ((Shell)sAMGeometry3D).Face3Ds?.ForEach(x => planars.Add(x));
                }
                else if (sAMGeometry3D is Extrusion)
                {
                    ((Extrusion)sAMGeometry3D).Face3Ds()?.ForEach(x => planars.Add(x));
                }
                else if (sAMGeometry3D is IPlanar3D)
                {
                    if (sAMGeometry3D is IBoundable3D)
                    {
                        planars.Add((IPlanar3D)sAMGeometry3D);
                    }
                }
                else
                {
                    notPlanars.Add(sAMGeometry3D);
                }
            }

            if (planars.Count == 0 && notPlanars.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool ownPlane = true;
            index = Params.IndexOfInputParam("_ownPlane");
            if (index == -1 || !dataAccess.GetData(index, ref ownPlane))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = null;

            if (!ownPlane)
            {
                index = Params.IndexOfInputParam("Plane");
                if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                GH_Plane gHPlane = objectWrapper.Value as GH_Plane;
                if (gHPlane == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                plane = Convert.ToSAM(gHPlane);
            }

            if (plane == null)
            {
                plane = Plane.WorldXY;
            }

            List<ISAMGeometry2D> sAMGeometry2Ds = new List<ISAMGeometry2D>();
            foreach (ISAMGeometry3D notPlanar in notPlanars)
            {
                if (notPlanar is Point3D)
                {
                    sAMGeometry2Ds.Add(plane.Convert((Point3D)notPlanar));
                }
                else if (notPlanar is Segment3D)
                {
                    sAMGeometry2Ds.Add(plane.Convert((Segment3D)notPlanar));
                }
                else if (notPlanar != null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Could not convert {0} geometry.", notPlanar.GetType().FullName));
                }
            }

            foreach (IBoundable3D boundable3D in planars)
            {
                Plane plane_Boundable3D = plane;
                if (ownPlane && boundable3D is IPlanar3D)
                {
                    plane_Boundable3D = (boundable3D as IPlanar3D).GetPlane();
                }

                sAMGeometry2Ds.Add(plane_Boundable3D.Convert(boundable3D));
            }

            index = Params.IndexOfOutputParam("sAMGeometry2D");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sAMGeometry2Ds);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.SAM_Geometry;
            }
        }
    }
}
