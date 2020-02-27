using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Vector2D> Directions(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Tolerance.Angle)
        {
            if (segmentable2Ds == null)
                return null;

            Dictionary<double, Vector2D> dictionary = new Dictionary<double, Vector2D>();
            if (segmentable2Ds.Count() == 0)
                return new List<Vector2D>();

            Vector2D vector2D_Base = Vector2D.BaseY;

            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds = segmentable2D.GetSegments();
                foreach(Segment2D segment2D in segment2Ds)
                {
                    Vector2D vector2D = segment2D.Vector;
                    double angle = vector2D.Angle(vector2D_Base);

                    Vector2D vector2D_Temp;
                    if (dictionary.TryGetValue(angle, out vector2D_Temp))
                        dictionary[angle] = vector2D + vector2D_Temp;
                    else
                        dictionary[angle] = vector2D;

                }
            }

            if (tolerance == 0)
                dictionary.Values.ToList();

            List<KeyValuePair<double, Vector2D>> keyValuePairs = dictionary.AsEnumerable().ToList();
            keyValuePairs.Sort((x, y) => x.Value.Length.CompareTo(y.Value.Length));

            //TODO: Implement Direction method for tolerance different than 0
            throw new System.NotImplementedException();
        }

    }
}
