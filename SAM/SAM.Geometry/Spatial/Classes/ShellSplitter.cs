using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class ShellSplitter
    {
        private class ShellData
        {
            private Shell shell;
            private List<Face3DData> face3DDatas;

            public ShellData(Shell shell)
            {
                this.shell = shell;

                face3DDatas = shell?.Face3Ds.ConvertAll(x => new Face3DData(x));
            }

            public List<Face3DData> Face3DDatas
            {
                get
                {
                    return face3DDatas == null ? null : new List<Face3DData>(face3DDatas);
                }
            }

            public static implicit operator ShellData(Shell shell)
            {
                if (shell == null)
                {
                    return null;
                }

                return new ShellData(shell);
            }
        }
        
        private class Face3DData
        {
            private Face3D face3D;
            private List<Segment3DData> segment3DDatas;

            public Face3DData(Face3D face3D)
            {
                this.face3D = face3D;
                segment3DDatas = GetSegment3Ds(face3D).ConvertAll(x => new Segment3DData(x));
            }

            public bool HasOverlay(Segment3DData segment3DData, double tolerance = Core.Tolerance.Distance)
            {
                if(segment3DData == null)
                {
                    return false;
                }

                return segment3DDatas?.FindAll(x => x != null && Core.Query.Round(x.Segment3D.GetLength(), tolerance) > tolerance).Find(x => segment3DData.Overlay(x, tolerance)) != null;
            }
            
            public List<Segment3DData> Segment3DDatas
            {
                get
                {
                    return segment3DDatas == null ? null : new List<Segment3DData>(segment3DDatas);
                }
            }

            public Face3D Face3D
            {
                get
                {
                    return face3D;
                }
            }

            private static List<Segment3D> GetSegment3Ds(Face3D face3D)
            {
                if (face3D == null)
                {
                    return null;
                }

                List<Segment3D> result = new List<Segment3D>();

                ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                {
                    throw new NotImplementedException();
                }

                result.AddRange(segmentable3D.GetSegments());

                List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
                if (internalEdges == null || internalEdges.Count == 0)
                {
                    return result;
                }

                foreach (IClosedPlanar3D internalEdge in internalEdges)
                {
                    if (internalEdge == null)
                    {
                        continue;
                    }

                    segmentable3D = internalEdge as ISegmentable3D;
                    if (segmentable3D == null)
                    {
                        throw new NotImplementedException();
                    }

                    result.AddRange(segmentable3D.GetSegments());
                }

                return result;
            }

            public static implicit operator Face3DData(Face3D face3D)
            {
                if (face3D == null)
                {
                    return null;
                }

                return new Face3D(face3D);
            }
        }

        private class Segment3DData
        {
            private Segment3D segment3D;
            private Point3D mid;

            public Segment3DData(Segment3D segment3D)
            {
                this.segment3D = segment3D;

                mid = segment3D?.Mid();
            }

            public Segment3D Segment3D
            {
                get
                {
                    return segment3D;
                }
            }

            public bool Overlay(Segment3DData segment3DData, double tolerance = Core.Tolerance.Distance)
            {
                if (segment3DData == null)
                {
                    return false;
                }

                if (segment3DData.mid == null || mid == null)
                {
                    return false;
                }

                return segment3D.On(segment3DData.mid, tolerance) || segment3DData.segment3D.On(mid, tolerance);
            }

            public List<Face3DData> GetFace3DDatas(IEnumerable<Face3DData> face3DDatas, double tolerance = Core.Tolerance.Distance)
            {
                if (face3DDatas == null)
                {
                    return null;
                }

                List<Face3DData> result = new List<Face3DData>();
                foreach (Face3DData face3DData in face3DDatas)
                {
                    if (face3DData == null)
                    {
                        continue;
                    }

                    if (face3DData.HasOverlay(this, tolerance))
                    {
                        result.Add(face3DData);
                    }

                }

                return result;
            }

            public static implicit operator Segment3DData(Segment3D segment3D)
            {
                if(segment3D == null)
                {
                    return null;
                }

                return new Segment3DData(segment3D);
            }

        }

        public double Tolerance_Distance { get; set; } = Core.Tolerance.Distance;
        public double Tolerance_Angle { get; set; } = Core.Tolerance.Angle;
        public double Tolerance_Snap { get; set; } = Core.Tolerance.MacroDistance;
        
        private List<Shell> shells;
        private List<Face3D> face3Ds;

        public ShellSplitter()
        {
        }
        
        public ShellSplitter(IEnumerable<Shell> shells, IEnumerable<Face3D> face3Ds)
        {
            this.shells = shells?.ToList().FindAll(x => x != null).ConvertAll(x => new Shell(x));
            this.face3Ds = face3Ds?.ToList().FindAll(x => x != null).ConvertAll(x => new Face3D(x));
        }

        public bool Add(Shell shell)
        {
            if(shell == null)
            {
                return false;
            }

            if(shells == null)
            {
                shells = new List<Shell>();
            }

            shells.Add(new Shell(shell));
            return true;
        }

        public bool Add(Face3D face3D)
        {
            if (face3D == null)
            {
                return false;
            }

            if (face3Ds == null)
            {
                face3Ds = new List<Face3D>();
            }

            face3Ds.Add(new Face3D(face3D));
            return true;
        }

        public List<Shell> Split()
        {
            if(shells == null || face3Ds == null)
            {
                return null;
            }

            List<Face3D> face3D_Temp = Query.Union(face3Ds, Tolerance_Snap);
            face3D_Temp.SplitEdges(Tolerance_Snap);

            List<Face3DData> face3DDatas = face3D_Temp.ConvertAll(x => new Face3DData(x));

            List<List<Shell>> shells_Result = Enumerable.Repeat<List<Shell>>(null, shells.Count).ToList();

            Parallel.For(0, shells.Count, (int i) => 
            {
                Shell shell = shells[i];

                shells_Result[i] = Split(shell, face3DDatas);
            });

            List<Shell> result = new List<Shell>();
            foreach(List<Shell> shells_Temp in shells_Result)
            {
                if(shells_Temp == null)
                {
                    continue;
                }

                result.AddRange(shells_Temp);
            }

            return result;
        }

        private List<Shell> Split(Shell shell, IEnumerable<Face3DData> face3DDatas)
        {
            if(shell == null || face3DDatas == null)
            {
                return null;
            }

            if(!shell.IsValid())
            {
                return new List<Shell>() { new Shell(shell)};
            }

            double snapFactor = 10;

            Shell shell_Temp = new Shell(shell);

            ShellData shellData_Before = new ShellData(shell_Temp);
            List<Face3DData> face3DDatas_Before = new List<Face3DData>(face3DDatas);

            List<Face3D> face3Ds_Shell_Before = shell_Temp.Face3Ds;
            if(face3Ds_Shell_Before == null || face3Ds_Shell_Before.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds = face3DDatas_Before.ConvertAll(x => x.Face3D);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return new List<Shell>() { new Shell(shell_Temp) };
            }

            for(int i=0; i < face3Ds.Count; i++)
            {
                Face3D face3D = face3Ds[i].Snap(new Shell[] { shell }, Tolerance_Snap * snapFactor, Tolerance_Distance);
                if(face3D != null)
                {
                    face3Ds[i] = face3D;
                }
            }

            List<Face3D> face3Ds_Shell_After = new List<Face3D>();
            foreach (Face3D face3D_Shell in face3Ds_Shell_Before)
            {
                List<Face3D> face3Ds_Temp = face3D_Shell.Split(face3Ds, Tolerance_Snap, Tolerance_Angle, Tolerance_Snap);
                if (face3Ds_Temp == null || face3Ds_Temp.Count == 0)
                {
                    face3Ds_Shell_After.Add(face3D_Shell);
                }
                else
                {
                    face3Ds_Shell_After.AddRange(face3Ds_Temp);
                }

            }

            Shell shell_After = new Shell(face3Ds_Shell_After);
            shell_After = shell_After.Snap(shell, Tolerance_Snap * snapFactor, Tolerance_Distance);
            for (int i = 0; i < face3Ds.Count; i++)
            {
                Face3D face3D = face3Ds[i].Snap(new Shell[] { shell_After }, Tolerance_Snap * snapFactor, Tolerance_Distance);
                if (face3D != null)
                {
                    face3Ds[i] = face3D;
                }
            }

            shell_After.SplitEdges(Tolerance_Snap);

            ShellData shellData_After = new ShellData(shell_After);


            List<Face3D> face3Ds_After = new List<Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                List<Face3D> face3Ds_Temp = face3D.Split(face3Ds_Shell_Before, Tolerance_Snap, Tolerance_Angle, Tolerance_Distance);
                if (face3Ds_Temp == null || face3Ds_Temp.Count == 0)
                {
                    face3Ds_After.Add(face3D);
                }
                else
                {
                    face3Ds_After.AddRange(face3Ds_Temp);
                }
            }

            for(int i = face3Ds_After.Count - 1; i >= 0; i--)
            {
                Point3D centroid = face3Ds_After[i].GetInternalPoint3D();

                if (!shell_Temp.Inside(centroid, Tolerance_Snap, Tolerance_Distance) || shell_Temp.On(centroid, Tolerance_Snap))
                {
                    face3Ds_After.RemoveAt(i);
                }
            }

            List<Face3DData> face3DDatas_After = face3Ds_After.ConvertAll(x => new Face3DData(x));

            //List<Segment3DData> segment3DDatas_Shell_Difference = GetDifference(shellData_Before, shellData_After);
            List<Segment3DData> segment3DDatas_After = GetSegment3DDatas(face3DDatas_After, Tolerance_Snap);

            List<Face3DData> face3DDatas_Shell_After = shellData_After.Face3DDatas;

            List<Shell> result = new List<Shell>();
            while(face3DDatas_Shell_After.Count > 0)
            {
                Face3DData face3DData = face3DDatas_Shell_After[0];
                face3DDatas_Shell_After.RemoveAt(0);

                List<Face3DData> face3DDatas_Temp = GetFace3DDatas(face3DData, face3DDatas_Shell_After, segment3DDatas_After, Tolerance_Snap);
                if(face3DDatas_Temp == null || face3DDatas_Temp.Count == 0)
                {
                    continue;
                }

                face3DDatas_Temp.Add(face3DData);

                List<Face3D> face3Ds_Temp = face3DDatas_Temp.ConvertAll(x => x.Face3D);
                if(face3Ds_After != null)
                {
                    face3Ds_Temp.AddRange(face3Ds_After);
                }

                for (int i = 0; i < face3Ds_Temp.Count; i++)
                {
                    Face3D face3D = null;

                    face3D = face3Ds_Temp[i].Snap(new Shell[] { shell_After }, Tolerance_Snap * snapFactor, Tolerance_Distance);
                    if (face3D != null)
                    {
                        face3Ds_Temp[i] = face3D;
                    }

                    List<Face3D> face3Ds_Snapping = new List<Face3D>(face3Ds_Temp);
                    face3Ds_Snapping.RemoveAt(i);
                    face3D = face3Ds_Temp[i].Snap(face3Ds_Snapping, Tolerance_Snap, Tolerance_Distance);
                    if (face3D != null)
                    {
                        face3Ds_Temp[i] = face3D;
                    }
                }

                face3Ds_Temp = face3Ds_Temp.Split(Tolerance_Snap, Tolerance_Angle, Tolerance_Distance);
                face3Ds_Temp.SplitEdges(Tolerance_Snap);

                Shell shell_New = new Shell(face3Ds_Temp);
                shell_New = shell_New.RemoveInvalidFace3Ds(Tolerance_Snap);
                if(shell_New == null || !shell_New.IsValid())
                {
                    continue;
                }

                if(!shell_New.IsClosed(Tolerance_Snap))
                {
                    continue;
                }

                result.Add(shell_New);
            }

            if(result == null || result.Count == 0)
            {
                return new List<Shell>() { shell };
            }

            double volume_Before = Core.Query.Round(shell_Temp.Volume(Tolerance_Snap, Tolerance_Distance));
            double volume_After = result.ConvertAll(x => Core.Query.Round(x.Volume(Tolerance_Snap, Tolerance_Distance), Tolerance_Snap)).Sum();

            if(System.Math.Abs(volume_Before - volume_After) < Tolerance_Snap)
            {
                return result;
            }

            List<Shell> shells_Difference = shell_Temp.Difference(result);
            if(shells_Difference == null || shells_Difference.Count == 0)
            {
                return result;
            }

            foreach(Shell shell_Difference in shells_Difference)
            {
                if(shell_Difference == null || !shell_Difference.IsValid())
                {
                    continue;
                }

                double volume = shell_Difference.Volume(Tolerance_Snap, Tolerance_Distance);
                if (double.IsNaN(volume) || volume < Tolerance_Snap)
                {
                    continue;
                }

                List<Shell> shells_Split = Split(shell_Difference, face3DDatas);
                if(shells_Split == null || shells_Split.Count == 0)
                {
                    continue;
                }

                shells_Split.ForEach(x => result.Add(x));
            }

            return result;
        }

        private static List<Face3DData> GetFace3DDatas(Face3DData face3DData, List<Face3DData> face3DDatas, List<Segment3DData> segment3DDatas_ToBeExcluded, double tolerance = Core.Tolerance.Distance)
        {
            if(face3DData == null || face3DDatas == null)
            {
                return null;
            }

            if(face3DDatas.Contains(face3DData))
            {
                face3DDatas.Remove(face3DData);
            }

            List<Face3DData> result = new List<Face3DData>();

            List<Segment3DData> segment3DDatas = face3DData.Segment3DDatas;
            if(segment3DDatas == null)
            {
                return result;
            }

            foreach(Segment3DData segment3DData in segment3DDatas)
            {
                if(segment3DDatas_ToBeExcluded != null && segment3DDatas_ToBeExcluded.Find(x => segment3DData.Overlay(x, tolerance)) != null)
                {
                    continue;
                }

                List<Face3DData> face3DDatas_Adjacent = segment3DData.GetFace3DDatas(face3DDatas, tolerance);
                if(face3DDatas_Adjacent == null || face3DDatas_Adjacent.Count == 0)
                {
                    continue;
                }

                foreach(Face3DData face3DData_Adjacent in face3DDatas_Adjacent)
                {
                    if (!result.Contains(face3DData_Adjacent))
                    {
                        result.Add(face3DData_Adjacent);
                    }

                    if (face3DDatas.Contains(face3DData_Adjacent))
                    {
                        face3DDatas.Remove(face3DData_Adjacent);
                    }
                }

                foreach(Face3DData face3DData_Adjacent in face3DDatas_Adjacent)
                {
                    List<Face3DData> face3DDatas_Temp = GetFace3DDatas(face3DData_Adjacent, face3DDatas, segment3DDatas_ToBeExcluded, tolerance);
                    if (face3DDatas_Temp == null || face3DDatas_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Face3DData face3DData_Temp in face3DDatas_Temp)
                    {
                        if (!result.Contains(face3DData_Temp))
                        {
                            result.Add(face3DData_Temp);
                        }

                        if (face3DDatas.Contains(face3DData_Temp))
                        {
                            face3DDatas.Remove(face3DData_Temp);
                        }
                    }
                }
            }

            return result;
        }

        private static List<Segment3DData> GetSegment3DDatas(IEnumerable<Face3DData> face3DDatas, double tolerance = Core.Tolerance.Distance)
        {
            if(face3DDatas == null)
            {
                return null;
            }

            List<Segment3DData> result = new List<Segment3DData>();
            if (face3DDatas.Count() == 0)
            {
                return result;
            }

            foreach (Face3DData face3DData in face3DDatas)
            {
                if (face3DData == null)
                {
                    continue;
                }

                foreach (Segment3DData segment3DData in face3DData.Segment3DDatas)
                {
                    if (segment3DData == null)
                    {
                        continue;
                    }

                    if (result.Find(x => segment3DData.Overlay(x, tolerance)) == null)
                    {
                        result.Add(segment3DData);
                    }
                }
            }

            return result;
        }
    }
}
