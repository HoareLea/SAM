using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public interface IBuildingElement : IAnalyticalObject, IParameterizedSAMObject, IFace3DObject, ISAMObject
    {
        void Transform(Transform3D transform3D);
        
        void Move(Vector3D vector3D);

        BoundingBox3D GetBoundingBox(double offset = 0);

    }
}
