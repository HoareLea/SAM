namespace SAM.Core
{
    public interface ISAMRelation
    {
        T GetObject<T>();

        T GetRelatedObject<T>();
    }
}