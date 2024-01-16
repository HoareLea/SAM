
namespace SAM.Core
{
    public static partial class Create
    {
        public static IndexedDoubles IndexedDoubles(this IndexedDoubles indexedDoubles, int start, int end)
        {
            if(indexedDoubles == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();
            for (int i = start; i < end; i++)
            {
                if(!indexedDoubles.ContainsIndex(i))
                {
                    continue;
                }

                result.Add(i, indexedDoubles[i]);
            }

            return result;
        }
    }
}