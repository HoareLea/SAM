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

        public Log(Log log)
            : base(log)
        {
            if (log.logRecords != null)
                logRecords = log.logRecords.ConvertAll(x => new LogRecord(x));
        }

        public LogRecord Add(string format, params object[] values)
        {
            LogRecord logRecord = new LogRecord(format, values);
            logRecords.Add(logRecord);
            return logRecord;
        }

        public LogRecord Add(LogRecord logRecord)
        {
            if (logRecord == null)
                return null;

            LogRecord result = logRecord.Clone();
            logRecords.Add(result);

            return result;
        }

        public LogRecord Add(string format, LogRecordType logRecordType, params object[] values)
        {
            LogRecord logRecord = new LogRecord(format, logRecordType, values);
            logRecords.Add(logRecord);
            return logRecord;
        }

        public List<LogRecord> AddRange(IEnumerable<LogRecord> logRecords)
        {
            if (logRecords == null)
                return null;

            List<LogRecord> result = new List<LogRecord>();
            foreach (LogRecord logRecord in logRecords)
            {
                LogRecord logRecord_New = logRecord?.Clone();
                if (logRecord_New == null)
                    continue;

                this.logRecords.Add(logRecord_New);
                result.Add(logRecord_New);
            }
            return result;
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
    }
}