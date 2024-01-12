namespace SAM.Geometry.Planar
{
    public interface IMovable2D : ISAMGeometry2D
    {
        bool Move(Vector2D vector2D);
    }

    public interface IMovable2D<T> : IMovable2D where T : ISAMGeometry2D
    {
        T GetMoved(Vector2D vector2D);

        bool Move(Vector2D vector2D);
    }
}