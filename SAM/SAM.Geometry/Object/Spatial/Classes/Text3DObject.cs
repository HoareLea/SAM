using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public class Text3DObject : IText3DObject, ITaggable
    {
        public Plane Plane { get; private set; }
        public string Text { get; private set; }

        public TextAppearance TextAppearance { get; set; }

        public Tag Tag { get; set; }

        public Text3DObject(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Text3DObject(Text3DObject text3DObject)
        {
            if(text3DObject != null)
            {
                if (text3DObject.TextAppearance != null)
                {
                    TextAppearance = new TextAppearance(text3DObject.TextAppearance);
                }

                Tag = text3DObject.Tag;

                Plane = text3DObject.Plane;

                Text = text3DObject.Text;
            }
        }

        public Text3DObject(string text)
        {
            Text = text;
            Plane = Plane.WorldXY;
        }

        public Text3DObject(string text, Plane plane, TextAppearance textAppearance)
        {
            Text = text;

            if(textAppearance != null)
            {
                TextAppearance = new TextAppearance(textAppearance);
            }

            if(plane != null)
            {
                Plane = new Plane(plane);
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Text"))
            {
                Text = jObject.Value<string>("Text");
            }

            if (jObject.ContainsKey("TextAppearance"))
            {
                TextAppearance = new TextAppearance(jObject.Value<JObject>("TextAppearance"));
            }

            if (jObject.ContainsKey("Plane"))
            {
                Plane = new Plane(jObject.Value<JObject>("Plane"));
            }

            Tag = Core.Query.Tag(jObject);

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (TextAppearance != null)
            {
                jObject.Add("TextAppearance", TextAppearance.ToJObject());
            }

            if(Plane != null)
            {
                jObject.Add("Plane", Plane.ToJObject());
            }

            if(Text != null)
            {
                jObject.Add("Text", Text);
            }

            Core.Modify.Add(jObject, Tag);

            return jObject;
        }
    }
}
