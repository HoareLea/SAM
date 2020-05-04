using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace SAM.Geometry.Grasshopper
{
    public class AssemblyInfo : GH_AssemblyInfo
    {
        private static MeshingParameters meshingParameters;

        public override string Name
        {
            get
            {
                return "SAM";
            }
        }

        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.SAM_Geometry; ;
            }
        }

        public override Bitmap AssemblyIcon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.SAM_Geometry; ;
            }
        }

        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "SAM.Geometry.Grasshopper Toolkit, please explore";
            }
        }

        public override Guid Id
        {
            get
            {
                return new Guid("8e0b764e-e3d4-4b15-9279-e42403dccc36");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Michal Dengusiak & Jakub Ziolkowski at Hoare Lea";
            }
        }

        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "Michal Dengusiak -> michaldengusiak@hoarelea.com and Jakub Ziolkowski -> jakubziolkowski@hoarelea.com";
            }
        }

        public static MeshingParameters GetMeshingParameters()
        {
            if (meshingParameters == null)
            {
                meshingParameters = new MeshingParameters
                {
                    SimplePlanes = false,
                    RefineGrid = true,
                    GridMinCount = 16,
                    GridMaxCount = 400,
                    GridAspectRatio = 20,
                    MaximumEdgeLength = 0,
                    RefineAngle = 20,
                    MinimumEdgeLength = 0,
                    //SimplePlanes = true,
                    //MinimumEdgeLength = 0.6,
                    //Tolerance = 0.2
                };
            }

            return meshingParameters;
        }
    }
}