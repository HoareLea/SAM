using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Range<double> TemperatureRange(this TM52BuildingCategory tM52BuildingCategory)
        {
            if (tM52BuildingCategory == TM52BuildingCategory.Undefined)
            {
                return null;
            }

            switch(tM52BuildingCategory)
            {
                case TM52BuildingCategory.CategoryI:
                    return new Range<double>(-2, 2);

                case TM52BuildingCategory.CategoryII:
                    return new Range<double>(-3, 3);

                case TM52BuildingCategory.CategoryIII:
                    return new Range<double>(-4, 4);

                case TM52BuildingCategory.CategoryIV:
                    return new Range<double>(-5, 5);
            }

            return null;
        }
    }
}