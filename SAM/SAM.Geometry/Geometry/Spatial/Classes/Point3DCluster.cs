using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class Point3DCluster
    {
        private List<Point3D> point3Ds;
        private double tolerance;
        private int[] parent;

        public Point3DCluster(IEnumerable<Point3D> point3Ds, double tolerance)
        {
            this.point3Ds = point3Ds == null ? null : new List<Point3D>(point3Ds);
            this.tolerance = tolerance;
            parent = this.point3Ds == null ? null : new int[this.point3Ds.Count];

            // Initialize each point as its own cluster
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
            }
        }

        // Find the root of a cluster
        private int Find(int i)
        {
            if (parent[i] != i)
            {
                parent[i] = Find(parent[i]); // Path compression
            }

            return parent[i];
        }

        // Union operation to merge two clusters
        private void Union(int i, int j)
        {
            int rootI = Find(i);
            int rootJ = Find(j);
            if (rootI != rootJ)
            {
                parent[rootI] = rootJ;
            }
        }

        // Clustering logic
        public List<List<Point3D>> Combine()
        {
            // Merge clusters if points are within tolerance
            for (int i = 0; i < point3Ds.Count; i++)
            {
                for (int j = i + 1; j < point3Ds.Count; j++)
                {
                    if (point3Ds[i].Distance(point3Ds[j]) <= tolerance)
                    {
                        Union(i, j);
                    }
                }
            }

            // Organize points into clusters
            Dictionary<int, List<Point3D>> clusters = new Dictionary<int, List<Point3D>>();
            for (int i = 0; i < point3Ds.Count; i++)
            {
                int root = Find(i);
                if (!clusters.ContainsKey(root))
                {
                    clusters[root] = new List<Point3D>();
                }
                clusters[root].Add(point3Ds[i]);
            }

            return new List<List<Point3D>>(clusters.Values);
        }
    }
}
