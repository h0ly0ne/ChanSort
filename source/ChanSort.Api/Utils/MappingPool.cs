using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChanSort.Api
{
    public class MappingPool<T> where T : DataMapping
    {
        private const string ERR_unknownACTChannelDataLength = "Configuration doesn't contain a {0} data mapping for length {1}";
        private readonly Dictionary<string, T> mappings = new Dictionary<string, T>();
        private readonly string Caption;
        public Encoding DefaultEncoding { get; set; }

        public MappingPool(string caption)
        {
            Caption = caption;
        }

        public void AddMapping(int dataLength, T mapping)
        {
            AddMapping(dataLength.ToString(), mapping);
        }

        public void AddMapping(string id, T mapping)
        {
            mappings.Add(id, mapping);
        }

        public T GetMapping(int dataLength, bool throwException = true)
        {
            return GetMapping(dataLength.ToString(), throwException);
        }

        public T GetMapping(string id, bool throwException = true)
        {
            if (id == "0" || string.IsNullOrEmpty(id))
                return null;

            if (!mappings.TryGetValue(id, out var mapping) && throwException)
                throw new FileLoadException(string.Format(ERR_unknownACTChannelDataLength, Caption, id));

            if (mapping != null && DefaultEncoding != null)
                mapping.DefaultEncoding = DefaultEncoding;

            return mapping;
        }
    }
}