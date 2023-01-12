namespace SAM.Core
{
    public static partial class Create
    {
        public static TextMap TextMap(string name)
        {
            return new TextMap(name);
        }

        public static TextMap TextMap(TextMap textMap)
        {
            if(textMap == null)
            {
                return null;
            }

            return new TextMap(textMap);
        }

        public static TextMap TextMap(string name, TextMap textMap)
        {
            if (textMap == null)
            {
                return null;
            }

            return new TextMap(name, textMap);
        }
    }
}