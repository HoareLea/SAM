using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public class LogRecord : IJSAMObject
    {
        private DateTime dateTime;
        private string text;

        public LogRecord(string text)
        {
            dateTime = DateTime.UtcNow;
            this.text = text;
        }

        public LogRecord(string format, params object[] values)
        {
            dateTime = DateTime.UtcNow;

            if (format != null)
                text = string.Format(format, values);
            else
                text = string.Empty;
        }

        public LogRecord(JObject jObject)
        {
            FromJObject(jObject);
        }

        public LogRecord(DateTime dateTime, string text)
        {
            this.dateTime = dateTime;

            if (text == null)
                this.text = string.Empty;
            else
                this.text = text;
        }

        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public override string ToString()
        {
            string text_Temp = text;
            if (string.IsNullOrWhiteSpace(text_Temp))
                text_Temp = string.Empty;

            return string.Format("[{0}]\t{1}", dateTime.ToString("yyyy-MM-dd HH:mm:ss.f"), text_Temp);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            dateTime = jObject.Value<DateTime>("DateTime");
            text = jObject.Value<string>("Text");
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("DateTime", dateTime);

            if (text != null)
                jObject.Add("Text", text);

            return jObject;
        }
    }
}