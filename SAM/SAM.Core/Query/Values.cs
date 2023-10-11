using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<object> Values(this IComplexReference complexReference, RelationCluster relationCluster, object parent = null)
        {
            if (complexReference == null || relationCluster == null)
            {
                return null;
            }

            ObjectReference objectReference = complexReference is PathReference ? ((PathReference)complexReference).FirstOrDefault() : complexReference as ObjectReference;
            if (objectReference == null)
            {
                return null;
            }

            List<object> objects = null;
            if (objectReference is PropertyReference)
            {
                if (parent == null)
                {
                    objects = relationCluster.GetObjects(objectReference);
                }
                else if (string.IsNullOrEmpty(objectReference.TypeName) && (objectReference.Reference == null || !objectReference.Reference.HasValue))
                {
                    objects = new List<object>() { parent };
                }
                else if(objectReference.Type == parent.GetType())
                {
                    objects = new List<object>() { parent };
                }
                else
                {
                    objects = relationCluster.GetObjects(objectReference, parent);
                }

                if(objects == null || objects.Count == 0)
                {
                    return objects;
                }

                string propertyName = ((PropertyReference)objectReference).PropertyName;
                if(!string.IsNullOrEmpty(propertyName))
                {
                    List<object> objects_Temp = new List<object>();
                    foreach(object @object in objects)
                    {
                        if(!TryGetValue(@object, propertyName, out object value))
                        {
                            continue;
                        }

                        objects_Temp.Add(value);
                    }

                    objects = objects_Temp;
                }
            }
            else
            {
                objects = relationCluster.GetObjects(objectReference, parent);
            }

            if(objects == null || objects.Count == 0)
            {
                return objects;
            }

            if(complexReference is PathReference)
            {
                List<ObjectReference> objectReferences = new List<ObjectReference>((PathReference)complexReference);
                objectReferences?.Remove(objectReference);

                if(objectReferences != null && objectReferences.Count != 0)
                {
                    PathReference pathReference = new PathReference(objectReferences);

                    List<object> objects_Temp = new List<object>();
                    foreach (object @object in objects)
                    {
                        List<object> objects_Temp_Temp = Values(pathReference, relationCluster, @object);
                        if(objects_Temp_Temp != null)
                        {
                            objects_Temp.AddRange(objects_Temp_Temp);
                        }
                    }

                    objects = objects_Temp;
                }
            }

            return objects;
        }
    }
}