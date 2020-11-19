namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Write(this IJSAMObject jSAMObject, string path)
        {
            string json = jSAMObject?.ToJObject()?.ToString();
            if (json == null)
                return false;

            System.IO.File.WriteAllText(path, json);
            return true;
        }
    }
}