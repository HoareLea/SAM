namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetTypeNameAndAssemblyName(this string fullTypeName, out string typeName, out string assemblyName)
        {
            typeName = null;
            assemblyName = null;

            if (string.IsNullOrWhiteSpace(fullTypeName))
                return false;

            if (fullTypeName.Contains(","))
            {
                string[] values = fullTypeName.Split(',');
                if (values.Length != 2)
                    return false;

                typeName = values[0];
                assemblyName = values[1];
                return true;
            }
            else
            {
                typeName = fullTypeName;
                return true;
            }

            return false;
        }
    }
}