namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Construction Duplicate(this Construction construction, string name)
        {
            if (construction == null || string.IsNullOrEmpty(name))
                return null;

            return new Construction(construction, name);
        }
    }
}
