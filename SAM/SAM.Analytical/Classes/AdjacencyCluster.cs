using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class AdjacencyCluster : Core.RelationCluster
    {        
        public AdjacencyCluster()
            : base()
        {

        }
        
        public AdjacencyCluster(AdjacencyCluster adjacencyCluster)
            :base(adjacencyCluster)
        {

        }
        
        public AdjacencyCluster(JObject jObject)
            : base(jObject)
        {

        }
        
        public override bool IsValid(Type type)
        {
            if (!base.IsValid(type))
                return false;

            return typeof(Panel).IsAssignableFrom(type) || typeof(Space).IsAssignableFrom(type);
        }

        public override Core.RelationCluster Clone()
        {
            return new AdjacencyCluster(this);
        }

        public bool Internal(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count > 1;
        }

        public bool External(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count == 1;
        }

        public bool Shade(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces == null || spaces.Count == 0;
        }

        public List<Panel> GetInternalPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Internal(x));
        }

        public List<Panel> GetExternalPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => External(x));
        }

        public List<Panel> GetShadingPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Shade(x));
        }

        public List<Panel> GetPanels()
        {
            return GetObjects<Panel>();
        }

        public List<Panel> GetPanels(Space space)
        {
            return GetRelatedObjects<Panel>(space);
        }

        public List<Space> GetSpaces()
        {
            return GetObjects<Space>();
        }

        public List<Space> GetSpaces(IEnumerable<Geometry.Spatial.Point3D> point3Ds, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            List<Tuple<Space, Geometry.Spatial.Shell>> tuples = new List<Tuple<Space, Geometry.Spatial.Shell>>();
            spaces.ForEach(x => tuples.Add(new Tuple<Space, Geometry.Spatial.Shell>(x, this.Shell(x))));
            tuples.RemoveAll(x => x.Item2 == null);

            List<Space> result = new List<Space>();
            foreach(Geometry.Spatial.Point3D point3D in point3Ds)
            {
                Tuple<Space, Geometry.Spatial.Shell> tuple = tuples.Find(x => x.Item2.Inside(point3D, silverSpacing, tolerance));
                result.Add(tuple?.Item1);
            }

            return result;
        }

        public List<Space> GetSpaces(Panel panel)
        {
            return GetRelatedObjects<Space>(panel);
        }

        public Space GetSpace(Geometry.Spatial.Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            return GetSpaces(new Geometry.Spatial.Point3D[] { point3D }, silverSpacing, tolerance)?.FirstOrDefault();
        }

        public AdjacencyCluster Filter(IEnumerable<Space> spaces)
        {
            if (spaces == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster();
            foreach(Space space in spaces)
            {
                if (GetGuid(space) == Guid.Empty)
                    continue;

                AddObject(space);

                List<object> relatedObjects = GetRelatedObjects(space);
                if (relatedObjects == null)
                    continue;
                
            }

            return result;
        }
    }
}
