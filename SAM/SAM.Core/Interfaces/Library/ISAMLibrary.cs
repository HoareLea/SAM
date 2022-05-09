namespace SAM.Core
{
    public interface ISAMLibrary : IParameterizedSAMObject
    {
        System.Type GenericType { get; }
        bool Write(string path);
        bool Append(string path);
    }
}