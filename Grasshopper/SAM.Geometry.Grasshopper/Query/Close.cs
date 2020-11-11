using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Query
    {
        public static bool TryClose(this Mesh mesh_Input, out Mesh mesh_Output, MeshType meshType = MeshType.Undefined)
        {
            mesh_Output = null;

            if (mesh_Input == null)
                return false;

            mesh_Output = new Mesh();
            mesh_Output.CopyFrom(mesh_Input);

            if (mesh_Input.IsClosed)
                return true;

            mesh_Output.Normals.ComputeNormals();
            mesh_Output.FaceNormals.ComputeFaceNormals();
            mesh_Output.Vertices.CombineIdentical(true, true);
            mesh_Output.UnifyNormals();
            mesh_Output.Normals.ComputeNormals();
            mesh_Output.FaceNormals.ComputeFaceNormals();

            Polyline[] polylines = mesh_Output.GetNakedEdges();
            if (polylines == null)
                return false;

            bool result = true;
            foreach(Polyline polyline in polylines)
            {
                Mesh mesh_Temp = Mesh.CreateFromClosedPolyline(polyline);
                if (mesh_Temp == null)
                {
                    result = false;
                    continue;
                }

                mesh_Temp.Convert(meshType);

                mesh_Output.Append(mesh_Temp);
            }

            mesh_Output.Normals.ComputeNormals();
            mesh_Output.FaceNormals.ComputeFaceNormals();
            mesh_Output.Vertices.CombineIdentical(true, true);
            mesh_Output.UnifyNormals();
            mesh_Output.Normals.ComputeNormals();
            mesh_Output.FaceNormals.ComputeFaceNormals();
            mesh_Output.Compact();

            return result;
        }
    }
}