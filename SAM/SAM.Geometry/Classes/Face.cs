using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry
{
    public abstract class Face : SAMGeometry
    {
        protected IClosed2D externalEdge;
        protected List<IClosed2D> internalEdges;

        public Face(Face face)
        {
            externalEdge = (IClosed2D)face.externalEdge.Clone();
            if (face.internalEdges != null)
                internalEdges = face.internalEdges.ConvertAll(x => (IClosed2D)x.Clone());
        }

        public Face(IClosed2D externalEdge)
        {
            this.externalEdge = externalEdge;
        }

        public Face(JObject jObject)
        {
            FromJObject(jObject);
        }

        public override bool FromJObject(JObject jObject)
        {
            externalEdge = Planar.Create.IClosed2D(jObject.Value<JObject>("ExternalEdge"));

            if (jObject.ContainsKey("InternalEdges"))
                internalEdges = Core.Create.IJSAMObjects<IClosed2D>(jObject.Value<JArray>("InternalEdges"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("ExternalEdge", externalEdge.ToJObject());
            if (internalEdges != null)
                jObject.Add("InternalEdges", Core.Create.JArray(internalEdges));

            return jObject;
        }

        public Planar.IClosed2D ExternalEdge
        {
            get
            {
                return externalEdge.Clone() as IClosed2D;
            }
        }

        public List<Planar.IClosed2D> InternalEdges
        {
            get
            {
                if (internalEdges == null)
                    return null;

                return internalEdges.ConvertAll(x => x.Clone() as IClosed2D);
            }
        }

        public List<IClosed2D> Edges
        {
            get
            {
                List<IClosed2D> result = new List<IClosed2D>() { externalEdge };
                if (internalEdges != null && internalEdges.Count > 0)
                    result.AddRange(internalEdges);
                return result;
            }
        }

        public double GetArea()
        {
            double area = externalEdge.GetArea();
            if (internalEdges != null && internalEdges.Count > 0)
                foreach (IClosed2D closed2D in internalEdges)
                    area -= closed2D.GetArea();

            return area;
        }
    }
}
