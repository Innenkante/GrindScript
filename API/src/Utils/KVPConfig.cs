using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// A simple key-value configuration file.
    /// </summary>
    public class KVPConfig : IEnumerable<KeyValuePair<string, object>>
    {
        private static string[] s_separators;

        private Dictionary<string, object> _pairs = new Dictionary<string, object>();

        static KVPConfig()
        {
            s_separators = new string[]
            {
                "\n", "\r\n"
            };
        }

        public void FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new InvalidOperationException("Can't read from the given stream.");

            StreamReader reader = new StreamReader(stream);

            string[] lines = reader.ReadToEnd().Split(s_separators, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, object> readPairs = new Dictionary<string, object>();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("#"))
                {
                    continue;
                }

                try
                {
                    int keyStart = trimmed.IndexOf('[') + 1;
                    int keyEnd = trimmed.IndexOf(']', keyStart + 1);

                    int valueStart = trimmed.IndexOf('[', keyStart + 1) + 1;
                    int valueEnd = trimmed.IndexOf(']', valueStart + 1);

                    int equalIndex = trimmed.IndexOf('=', keyEnd + 1, valueStart - keyEnd);

                    if (keyStart == -1 || keyEnd == -1 || valueStart == -1 || valueEnd == -1 || equalIndex == -1)
                    {
                        continue;
                    }

                    string keyString = trimmed.Substring(keyStart, keyEnd - keyStart).Trim();
                    string valueString = trimmed.Substring(valueStart, valueEnd - valueStart).Trim();

                    object value = null;

                    if (double.TryParse(valueString, out double doubleval))
                    {
                        value = doubleval;
                    }
                    else if (long.TryParse(valueString, out long longval))
                    {
                        value = longval;
                    }
                    else if (bool.TryParse(valueString, out bool boolval))
                    {
                        value = boolval;
                    }
                    else
                    {
                        value = valueString;
                    }

                    readPairs[keyString] = value;
                }
                catch
                {
                    continue;
                }
            }

            _pairs = readPairs;
        }

        public void ToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                throw new InvalidOperationException("Can't read from the given stream.");

            StringBuilder builder = new StringBuilder();

            StreamWriter writer = new StreamWriter(stream);

            foreach (var pair in _pairs)
            {
                builder
                    .Append('[')
                    .Append(pair.Key.ToString())
                    .Append("] = [")
                    .Append(pair.Value.ToString())
                    .Append("]" + Environment.NewLine);
            }

            if (_pairs.Count > 0)
            {
                builder.Length -= 2;
            }

            writer.Write(builder);
        }

        public void Set<T>(string key, T value)
        {
            _pairs[key] = value;
        }

        public bool Clear(string key)
        {
            return _pairs.Remove(key);
        }

        public T Get<T>(string key)
        {
            return (T)_pairs[key];
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_pairs.TryGetValue(key, out object obj) && obj is T val)
            {
                value = val;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var pair in _pairs)
            {
                builder
                    .Append('[')
                    .Append(pair.Key.ToString())
                    .Append("] = [")
                    .Append(pair.Value.ToString())
                    .Append("], ");
            }

            if (_pairs.Count > 0)
            {
                builder.Length -= 2;
            }

            return builder.ToString();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)this._pairs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._pairs).GetEnumerator();
        }
    }
}
