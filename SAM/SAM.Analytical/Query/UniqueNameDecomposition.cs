using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// UniqueName Format (prefix)(name)[(guid)][(id)] where: prefix - optional, name - obligatory, guid - optional, id - optional
        /// </summary>
        /// <param name="uniqueName">UniqueName</param>
        /// <param name="prefix">Prefix based on PanelType or ApertureType</param>
        /// <param name="name">Name</param>
        /// <param name="guid">Guid</param>
        /// <param name="id">Id value grater than 0</param>
        /// <returns>Bool</returns>
        public static bool UniqueNameDecomposition(string uniqueName, out string prefix, out string name, out Guid? guid, out int id)
        {
            prefix = null;
            name = null;
            guid = null;
            id = -1;

            if(uniqueName == null)
            {
                return false;
            }

            name = uniqueName.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            foreach(PanelType panelType in Enum.GetValues(typeof(PanelType)))
            {
                if(name.StartsWith(UniqueNamePrefix(panelType) + ":"))
                {
                    prefix = UniqueNamePrefix(panelType);
                    break;
                }
            }

            if(prefix == null)
            {
                foreach (ApertureType apertureType in Enum.GetValues(typeof(ApertureType)))
                {
                    if (name.StartsWith(UniqueNamePrefix(apertureType) + ":"))
                    {
                        prefix = UniqueNamePrefix(apertureType);
                        break;
                    }
                }
            }

            if(prefix != null)
            {
                name = name.Substring(prefix.Length + 1).Trim();
            }

            int index_End = name.LastIndexOf("]");
            if (index_End == name.Length - 1)
            {
                int count_Max = int.MaxValue.ToString().Length;

                int index_Start = name.LastIndexOf("[");
                if(index_Start >= 0)
                {
                    int count = index_End - index_Start - 1;
                    if (count > 0)
                    {
                        string value = name.Substring(index_Start + 1, count);
                        if (count > count_Max)
                        {
                            if(Guid.TryParse(value, out Guid guid_Temp))
                            {
                                guid = guid_Temp;
                                name = name.Substring(0, index_Start).Trim();
                            }
                        }
                        else
                        {
                            if (int.TryParse(value, out int @int))
                            {
                                id = @int;
                                name = name.Substring(0, index_Start).Trim();
                            }
                        }
                    }
                }

                index_End = name.LastIndexOf("]");
                if (index_End == name.Length - 1)
                {
                    count_Max = int.MaxValue.ToString().Length;

                    index_Start = name.LastIndexOf("[");
                    if (index_Start >= 0)
                    {
                        int count = index_End - index_Start - 1;
                        if (count > 0)
                        {
                            string value = name.Substring(index_Start + 1, count);
                            if (count > count_Max)
                            {
                                if(guid == null)
                                {
                                    if (Guid.TryParse(value, out Guid guid_Temp))
                                    {
                                        guid = guid_Temp;
                                        name = name.Substring(0, index_Start).Trim();
                                    }
                                }
                            }
                            else
                            {
                                if(id == -1)
                                {
                                    if (int.TryParse(value, out int @int))
                                    {
                                        id = @int;
                                        name = name.Substring(0, index_Start).Trim();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}