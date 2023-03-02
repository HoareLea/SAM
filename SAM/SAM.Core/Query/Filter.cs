using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Log Filter(this Log log, IEnumerable<LogRecordType> logRecordTypes)
        {
            if (log == null || logRecordTypes == null)
                return null;

            Log result = new Log(log.Name);
            foreach(LogRecord logRecord in log)
            {
                if (logRecord == null)
                    continue;

                if (logRecordTypes.Contains(logRecord.LogRecordType))
                    result.Add(logRecord);
            }

            return result;
        }

        public static Log Filter(this Log log, LogRecordType logRecordType)
        {
            return Filter(log, new LogRecordType[] { logRecordType });
        }

        public static void Filter<T>(this IEnumerable<T> ts, out List<T> @in, out List<T> @out, params Func<T, bool>[] functions)
        {
            @in = null;
            @out = null;

            if(ts == null || functions == null)
            {
                return;
            }

            @in = new List<T>(ts);
            @out = new List<T>();

            for (int i = @in.Count - 1; i >= 0; i--)
            {
                bool remove = false;
                foreach (Func<T, bool> function in functions)
                {
                    if (!function(@in[i]))
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                {
                    @out.Add(@in[i]);
                    @in.RemoveAt(i);
                }
            }
        }

        public static List<object> Filter(this RelationCluster relationCluster_In, RelationCluster relationCluster_Out, IEnumerable<object> objects)
        {
            if (relationCluster_In == null || objects == null)
                return null;

            if (relationCluster_Out == null)
                relationCluster_Out = new RelationCluster();

            HashSet<object> objects_Out = new HashSet<object>();

            Filter(relationCluster_In, relationCluster_Out, objects, objects_Out);

            return objects_Out.ToList();
        }

        public static List<T> Filter<T>(this IEnumerable<IJSAMObject> jSAMObjects, IFilter filter) where T : IJSAMObject
        {
            if (jSAMObjects == null || filter == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (IJSAMObject jSAMObject in jSAMObjects)
            {
                if (!(jSAMObject is T))
                {
                    continue;
                }

                if (!filter.IsValid(jSAMObject))
                {
                    continue;
                }

                result.Add((T)jSAMObject);
            }

            return result;
        }


        private static void Filter(this RelationCluster relationCluster_In, RelationCluster relationCluster_Out, IEnumerable<object> objects_In, HashSet<object> objects_Out)
        {
            if (objects_In == null || relationCluster_In == null || relationCluster_Out == null || objects_Out == null)
                return;

            HashSet<object> objects_In_New = new HashSet<object>();
            foreach (object object_In in objects_In)
            {
                if (objects_Out.Contains(objects_In))
                    continue;

                relationCluster_Out.AddObject(object_In);
                
                List<object> relatedObjects = relationCluster_In.GetRelatedObjects(object_In);
                if (relatedObjects == null || relatedObjects.Count == 0)
                    continue;

                foreach(object relatedObject in relatedObjects)
                {
                    if (objects_Out.Contains(relatedObject))
                        continue;

                    relationCluster_Out.AddObject(relatedObject);
                    relationCluster_Out.AddRelation(object_In, relatedObject);

                    objects_In_New.Add(relatedObject);
                }

                objects_Out.Add(object_In);
            }

            if (objects_In_New != null && objects_In_New.Count != 0)
                Filter(relationCluster_In, relationCluster_Out, objects_In_New, objects_Out);
        }
    }
}