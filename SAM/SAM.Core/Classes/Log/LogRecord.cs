using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class LogRecord : IJSAMObject
    {
        private DateTime dateTime;
        private string text;
        private LogRecordType logRecordType;

        public LogRecord(LogRecord logRecord)
        {
            dateTime = logRecord.dateTime;
            logRecordType = logRecord.logRecordType;
            text = logRecord.text;
        }

        public LogRecord(string text)
        {
            dateTime = DateTime.UtcNow;
            logRecordType = LogRecordType.Undefined;
            this.text = text;
        }

        public LogRecord(string format, params object[] values)
        {
            dateTime = DateTime.UtcNow;
            logRecordType = LogRecordType.Undefined;

            if (format != null)
                text = string.Format(format, values);
            else
                text = string.Empty;
        }

        public LogRecord(string format, LogRecordType logRecordType, params object[] values)
        {
            dateTime = DateTime.UtcNow;
            this.logRecordType = logRecordType;

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
            logRecordType = LogRecordType.Undefined;

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

        public LogRecordType LogRecordType
        {
            get
            {
                return logRecordType;
            }
        }

        public override string ToString()
        {
            string text_Temp = text;
            if (string.IsNullOrWhiteSpace(text_Temp))
                text_Temp = string.Empty;

            if (logRecordType == LogRecordType.Undefined)
                return string.Format("[{0}]\t{1}", dateTime.ToString("yyyy-MM-dd HH:mm:ss.f"), text_Temp);
            else
                return string.Format("[{0}\t{1}]\t{2}", dateTime.ToString("yyyy-MM-dd HH:mm:ss.f"), logRecordType.ToString(), text_Temp);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            dateTime = jObject.Value<DateTime>("DateTime");
            text = jObject.Value<string>("Text");

            logRecordType = LogRecordType.Undefined;
            if (jObject.ContainsKey("LogRecordType"))
                Enum.TryParse(jObject.Value<string>("LogRecordType"), out logRecordType);

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("DateTime", dateTime);

            if (logRecordType != LogRecordType.Undefined)
                jObject.Add("LogRecordType", logRecordType.ToString());

            if (text != null)
                jObject.Add("Text", text);

            return jObject;
        }
    }
}