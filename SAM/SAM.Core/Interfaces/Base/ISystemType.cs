namespace SAM.Core
{
    public interface ISystemType : IParameterizedSAMObject, ISAMObject
    {
        string Description { get; }
    }
}