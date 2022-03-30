namespace SAM.Analytical
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
                case Analytical.PartitionAnalyticalType.CurtainWall:
                    return System.Drawing.Color.BlueViolet;

                case Analytical.PartitionAnalyticalType.ExternalFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#40B4FF");

                case Analytical.PartitionAnalyticalType.InternalFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#80FFFF");

                case Analytical.PartitionAnalyticalType.Roof:
                    return System.Drawing.ColorTranslator.FromHtml("#800000");

                case Analytical.PartitionAnalyticalType.Shade:
                    return System.Drawing.ColorTranslator.FromHtml("#FFCE9D");

                case Analytical.PartitionAnalyticalType.OnGradeFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#804000");

                case Analytical.PartitionAnalyticalType.Undefined:
                    return System.Drawing.ColorTranslator.FromHtml("#FFB400");

                case Analytical.PartitionAnalyticalType.UndergroundCeiling:
                    return System.Drawing.ColorTranslator.FromHtml("#408080");

                case Analytical.PartitionAnalyticalType.UndergroundFloor:
                    return System.Drawing.ColorTranslator.FromHtml("#804000");

                case Analytical.PartitionAnalyticalType.UndergroundWall:
                    return System.Drawing.ColorTranslator.FromHtml("#A55200");

                case Analytical.PartitionAnalyticalType.ExternalWall:
                    return System.Drawing.ColorTranslator.FromHtml("#FFB400");

                case Analytical.PartitionAnalyticalType.InternalWall:
                    return System.Drawing.ColorTranslator.FromHtml("#008000");

                case Analytical.PartitionAnalyticalType.Air:
                    return System.Drawing.ColorTranslator.FromHtml("#FFFF00");
            }

            return System.Drawing.Color.Empty;
        }

        public static System.Drawing.Color Color(this OpeningAnalyticalType openingAnalyticalType)
        {
            switch(openingAnalyticalType)
            {
                case Analytical.OpeningAnalyticalType.Door:
                    return System.Drawing.Color.Brown;

                case Analytical.OpeningAnalyticalType.Window:
                    return System.Drawing.Color.Blue;

                case Analytical.OpeningAnalyticalType.Undefined:
                    return System.Drawing.Color.Empty;
            }

            return System.Drawing.Color.Empty;
        }

        public static System.Drawing.Color Color(this BoundaryType boundaryType)
        {
            switch (boundaryType)
            {
                case Analytical.BoundaryType.Adiabatic:
                    return System.Drawing.Color.FromArgb(192, 128, 255);

                case Analytical.BoundaryType.Exposed:
                    return System.Drawing.Color.FromArgb(128, 255, 128);

                case Analytical.BoundaryType.Ground:
                    return System.Drawing.Color.FromArgb(128, 255, 255);

                case Analytical.BoundaryType.Linked:
                    return System.Drawing.Color.FromArgb(255, 128, 128);

                case Analytical.BoundaryType.Shade:
                    return System.Drawing.Color.FromArgb(255, 180, 128);

            }

            return System.Drawing.Color.Empty;
        }
    }
}