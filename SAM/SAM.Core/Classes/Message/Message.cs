using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class Message : IJSAMObject
    {
        private MessageType messageType;
        private string text;

        public Message(MessageType messageType, string text)
        {
            this.messageType = messageType;
            this.text = text;
        }

        public Message(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Message(Message message)
        {
            if (message != null)
            {
                messageType = message.messageType;
                text = message.text;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Text"))
            {
                text = jObject.Value<string>("Text");
            }


            messageType = MessageType.Undefined;
            if (jObject.ContainsKey("MessageType"))
            {
                messageType = Query.Enum<MessageType>(jObject.Value<string>("MessageType"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if (messageType != MessageType.Undefined)
                jObject.Add("MessageType", messageType.ToString());

            if (text != null)
                jObject.Add("Text", text);

            return jObject;
        }
    }
}
