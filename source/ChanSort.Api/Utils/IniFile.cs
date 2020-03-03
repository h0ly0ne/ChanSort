using System;
using System.Collections.Generic;
using System.IO;

namespace ChanSort.Api
{
    public class IniFile
    {
        #region class Section
        public class Section
        {
            private readonly Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            public Section(string name)
            {
                Name = name;
            }

            #region Name
            public string Name { get; }
            #endregion

            #region Set()
            internal void Set(string key, string value)
            {
                data[key] = value;
            }
            #endregion

            #region Keys
            public IEnumerable<string> Keys => data.Keys;
            #endregion

            #region GetString()
            public string GetString(string key)
            {
                return !data.TryGetValue(key, out string value) ? null : value;
            }
            #endregion

            #region GetInt()
            public int GetInt(string key, int defaultValue = 0)
            {
                return !data.TryGetValue(key, out string value) ? defaultValue : ParseNumber(value);
            }
            #endregion

            #region GetBytes()
            public byte[] GetBytes(string key)
            {
                if (!data.TryGetValue(key, out string value))
                    return null;

                if (string.IsNullOrEmpty(value))
                    return new byte[0];

                string[] parts = value.Split(',');
                byte[] bytes = new byte[parts.Length];
                int i = 0;

                foreach (string part in parts)
                {
                    bytes[i++] = (byte)ParseNumber(part);
                }

                return bytes;
            }
            #endregion

            #region GetIntList()
            public int[] GetIntList(string key)
            {
                string value = GetString(key);

                if (string.IsNullOrEmpty(value))
                    return new int[0];

                string[] numbers = value.Split(',');
                int[] ret = new int[numbers.Length];

                for (int i = 0; i < numbers.Length; i++)
                {
                    ret[i] = ParseNumber(numbers[i]);
                }
                
                return ret;
            }
            #endregion

            #region ParseNumber()
            private int ParseNumber(string value)
            {
                int sig = value.StartsWith("-") ? -1 : 1;

                if (sig < 0)
                    value = value.Substring(1).Trim();

                if (value.ToLower().StartsWith("0x"))
                {
                    try
                    {
                        return Convert.ToInt32(value, 16) * sig;
                    }
                    catch
                    {
                        return 0;
                    }
                }

                int.TryParse(value, out int intValue);
                
                return intValue;
            }
            #endregion
        }
        #endregion

        private readonly Dictionary<string, Section> sectionDict;
        private readonly List<Section> sectionList;

        public IniFile(string fileName)
        {
            sectionDict = new Dictionary<string, Section>();
            sectionList = new List<Section>();
            ReadIniFile(fileName);
        }

        public IEnumerable<Section> Sections => sectionList;

        public Section GetSection(string sectionName)
        {
            return sectionDict.TryGet(sectionName);
        }

        #region ReadIniFile()
        private void ReadIniFile(string fileName)
        {
            using (StreamReader rdr = new StreamReader(fileName))
            {
                Section currentSection = null;
                string line;
                string key = null;
                string val = null;

                while ((line = rdr.ReadLine()) != null)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith(";"))
                        continue;

                    if (trimmedLine.StartsWith("["))
                    {
                        string sectionName = trimmedLine.EndsWith("]") ? trimmedLine.Substring(1, trimmedLine.Length - 2) : trimmedLine.Substring(1);
                        
                        currentSection = new Section(sectionName);
                        sectionList.Add(currentSection);
                        sectionDict[sectionName] = currentSection;

                        continue;
                    }

                    if (currentSection == null)
                        continue;

                    if (val == null)
                    {
                        int idx = trimmedLine.IndexOf("=", StringComparison.Ordinal);

                        if (idx < 0)
                            continue;

                        key = trimmedLine.Substring(0, idx).Trim();
                        val = trimmedLine.Substring(idx + 1).Trim();
                    }
                    else
                        val += line;

                    if (val.EndsWith("\\"))
                        val = val.Substring(val.Length - 1).Trim();
                    else
                    {
                        currentSection.Set(key, val);
                        val = null;
                    }
                }
            }
        }
        #endregion
    }
}