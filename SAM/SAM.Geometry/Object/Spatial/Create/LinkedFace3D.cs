namespace SAM.Geometry.Object.Spatial
{
    public static partial class Create
    {
        public static LinkedFace3D LinkedFace3D(this IFace3DObject face3DObject)
        {
            if(face3DObject == null)
            {
                return null;
            }

            if(face3DObject is LinkedFace3D)
            {
                return new LinkedFace3D((LinkedFace3D)face3DObject);
            }

            System.Guid guid = System.Guid.Empty;
            if(face3DObject is Core.SAMObject)
            {
                guid = ((Core.SAMObject)face3DObject).Guid;
            }

            return new LinkedFace3D(guid, face3DObject.Face3D);
        }
    }
}