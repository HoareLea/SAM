namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string RiserName(this MechanicalSystemCategory mechanicalSystemCategory, int index = 1)
        {
            string prefix = RiserNamePrefix(mechanicalSystemCategory);
            if(prefix == null)
            {
                return null;
            }

            return string.Format("{0}{1}", prefix, index).Trim();
        }
    }
}