namespace SAM.Geometry.Spatial
{
    public class Segment3DObject : Segment3D, ISegment3DObject, Core.ITaggable
    {
        public object Tag { get; set; }
        
        public Segment3D Segment3D
        {
            get
            {
                return new Segment3D(this);
            }
        }

        public Segment3DObject(Segment3D segment3D)
            : base(segment3D)
        {

        }
    }
}
