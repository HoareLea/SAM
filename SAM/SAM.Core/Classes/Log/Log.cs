using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class Log : SAMObject, IEnumerable<LogRecord>, IJSAMObject
    {
        private List<LogRecord> logRecords = new List<LogRecord>();

        public Log(string name)
            : base(name)
        {
        }

        public Log()
            : base()
        {

        }

        public LogRecord Add(string format, params object[] values)
        {
            LogRecord logRecord = new LogRecord(format, values);
            logRecords.Add(logRecord);
            return logRecord;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, logRecords.ConvertAll(x => x.ToString()));
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            logRecords = new List<LogRecord>();

            JArray jArray = jObject.Value<JArray>("LogRecords");
            if(jArray != null)
            {
                foreach (JObject jObject_Temp in jArray)
                    logRecords.Add(new LogRecord(jObject_Temp));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray = new JArray();
            logRecords.ForEach(x => jArray.Add(x.ToJObject()));
            jObject.Add("LogRecords", jArray);

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<LogRecord> GetEnumerator()
        {
            return logRecords.GetEnumerator();
        }

        public void Clear()
        {
            logRecords.Clear();
        }

        public bool Write(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                System.IO.File.AppendAllText(path, ToString() + Environment.NewLine);
            }
            catch(Exception exception)
            {
                return false;
            }

            return true;
        }


        public static LogRecord Add(Log log, string format, params object[] values)
        {
            if (log == null)
                return null;

            return log.Add(format, values);
        }
    }
}