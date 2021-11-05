namespace SAM.Math
{
    public static partial class Query
    {
        public static double Remap(this double value, double from_1, double to_1, double from_2, double to_2)
        {
            if ((to_1 - from_1) * (to_2 - from_2) == 0) 
            { return from_2; }
            else {
                return ((value - from_1) / (to_1 - from_1) * (to_2 - from_2) + from_2);
                    }
            
        }
    }
}