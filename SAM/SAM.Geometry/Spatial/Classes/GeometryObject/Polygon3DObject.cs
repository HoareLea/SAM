namespace SAM.Geometry.Spatial
{
    public class Polygon3DObject : Polygon3D, IPolygon3DObject, Core.ITaggable
    {
        public object Tag { get; set; }
        
        public Polygon3D Polygon3D
        {
            get
            {
                return new Polygon3D(this);
            }
        }

        public Polygon3DObject(Polygon3D polygon3D)
            : base(polygon3D)
        {

        }
    }
}
