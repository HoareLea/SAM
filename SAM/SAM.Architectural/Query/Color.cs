namespace SAM.Architectural
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this IPartition partition)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if(partition == null)
            {
                return result;
            }

            if(partition is Wall)
            {
                return System.Drawing.ColorTranslator.FromHtml("#FFB400");
            }

            if (partition is Floor)
            {
                return System.Drawing.ColorTranslator.FromHtml("#804000");
            }

            if (partition is Roof)
            {
                return System.Drawing.ColorTranslator.FromHtml("#800000");
            }

            if (partition is AirPartition)
            {
                return System.Drawing.ColorTranslator.FromHtml("#FFFF00");
            }

            return result;
        }

        public static System.Drawing.Color Color(this IPartition partition, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (partition == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Gray;
            }

            return Color(partition);
        }

        public static System.Drawing.Color Color(this IOpening opening)
        {
            return Color(OpeningAnalyticalType(opening));
        }

        public static System.Drawing.Color Color(this IOpening opening, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (opening == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Violet;
            }

            return Color(OpeningAnalyticalType(opening));
        }

        public static System.Drawing.Color Color(this OpeningType openingType, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (openingType == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Violet;
            }

            return Color(OpeningAnalyticalType(openingType));
        }

        public static System.Drawing.Color Color(this PartitionAnalyticalType partitionAnalyticalType)
        {
            switch (partitionAnalyticalType)
            {
                case Architectural.PartitionAnalyticalType.CurtainWall:
                    return System.Drawing.Color.BlueViolet;

                case Architectural.PartitionAnalyticalType.ExternalFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#40B4FF");

                case Architectural.PartitionAnalyticalType.InternalFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#80FFFF");

                case Architectural.PartitionAnalyticalType.Roof:
                    return System.Drawing.ColorTranslator.FromHtml("#800000");

                case Architectural.PartitionAnalyticalType.Shade:
                    return System.Drawing.ColorTranslator.FromHtml("#FFCE9D");

                case Architectural.PartitionAnalyticalType.OnGradeFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#804000");

                case Architectural.PartitionAnalyticalType.Undefined:
                    return System.Drawing.ColorTranslator.FromHtml("#FFB400");

                case Architectural.PartitionAnalyticalType.UndergroundCeiling:
                    return System.Drawing.ColorTranslator.FromHtml("#408080");

                case Architectural.PartitionAnalyticalType.UndergroundFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#804000");

                case Architectural.PartitionAnalyticalType.UndergroundWall:
                    return System.Drawing.ColorTranslator.FromHtml("#A55200");

                case Architectural.PartitionAnalyticalType.ExternalWall:
                    return System.Drawing.ColorTranslator.FromHtml("#FFB400");

                case Architectural.PartitionAnalyticalType.InternalWall:
                    return System.Drawing.ColorTranslator.FromHtml("#008000");

                case Architectural.PartitionAnalyticalType.Air:
                    return System.Drawing.ColorTranslator.FromHtml("#FFFF00");
            }

            return System.Drawing.Color.Empty;
        }

        public static System.Drawing.Color Color(this OpeningAnalyticalType openingAnalyticalType)
        {
            switch(openingAnalyticalType)
            {
                case Architectural.OpeningAnalyticalType.Door:
                    return System.Drawing.Color.Brown;

                case Architectural.OpeningAnalyticalType.Window:
                    return System.Drawing.Color.Blue;

                case Architectural.OpeningAnalyticalType.Undefined:
                    return System.Drawing.Color.Empty;
            }

            return System.Drawing.Color.Empty;
        }
    }
}