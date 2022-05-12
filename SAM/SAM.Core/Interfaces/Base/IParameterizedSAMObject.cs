using System;

namespace SAM.Core
{
    public interface IParameterizedSAMObject : IJSAMObject
    {
        object GetValue(Enum @enum);
        T GetValue<T>(Enum @enum);
        bool HasValue(Enum @enum);
        bool RemoveValue(Enum @enum);
        bool SetValue(Enum @enum, object value);
        bool TryGetValue<T>(Enum @enum, out T value, bool tryConvert = true);
        bool TryGetValue(Enum @enum, out object value);
    }
}