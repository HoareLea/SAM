using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Window : BuildingElement<WindowType>, IOpening
    {
        public Window(Window window)
            : base(window)
        {

        }

        public Window(JObject jObject)
            : base(jObject)
        {

        }

        public Window(WindowType windowType, Face3D face3D)
            : base(windowType, face3D)
        {

        }

        public Window(System.Guid guid, WindowType windowType, Face3D face3D)
            : base(guid, windowType, face3D)
        {

        }

        public Window(System.Guid guid, Window window, Face3D face3D)
            : base(guid, window, face3D)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
