namespace SAM.Math
{
    public static partial class Query
    {
        /// <summary>
        /// Returns neigbour indices in values array for given value. Values have to be sorted.
        /// </summary>
        /// <param name="values">Values array</param>
        /// <param name="value">Value</param>
        /// <param name="lowerIndex">Lower Index</param>
        /// <param name="upperIndex">Upper Index</param>
        public static void NeigbourIndices(double[] values, double value, out int lowerIndex, out int upperIndex)
        {
            lowerIndex = -1;
            upperIndex = -1;

            if(values == null || values.Length == 0 || double.IsNaN(value))
            {
                return;
            }

            if(value <= values[0] )
            {
                lowerIndex = 0;
                upperIndex = 0;
                return;
            }

            if(value >= values[values.Length - 1])
            {
                lowerIndex = values.Length - 1;
                upperIndex = values.Length - 1;
                return;
            }

            for (int i = 1; i < values.Length; i++)
            {
                if(value < values[i])
                {
                    lowerIndex = i - 1;
                    upperIndex = i;
                    return;
                }
                else if(value == values[i])
                {
                    lowerIndex = i;
                    upperIndex = i;
                }
            }
        }
    }
}