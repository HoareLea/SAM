namespace SAM.Geometry.Spatial
{
    public class Face3DObject : Face3D, IFace3DObject, Core.ITaggable
    {
        public object Tag { get; set; }
        
        public Face3D Face3D
        {
            get
            {
                return new Face3D(this);
            }
        }

        public Face3DObject(Face3D face3D)
            : base(face3D)
        {

        }
    }
}
