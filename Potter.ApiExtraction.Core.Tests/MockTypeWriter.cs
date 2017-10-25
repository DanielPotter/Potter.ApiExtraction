using System;
using System.Collections.Generic;
using System.IO;
using Potter.ApiExtraction.Core.Generation;

namespace Potter.ApiExtraction.Core.Tests
{
    public class MockTypeWriter : ITypeWriter
    {
        public MockTypeWriter(IEnumerable<KeyValuePair<string, string>> initialContent = null)
        {
            if (initialContent != null)
            {
                foreach (var entry in initialContent)
                {
                    _content[entry.Key] = entry.Value;
                }
            }
        }

        public string GetContent(string typeName)
        {
            _content.TryGetValue(typeName, out string content);

            return content;
        }

        private readonly Dictionary<string, string> _content = new Dictionary<string, string>();

        public TextWriter WriteType(IApiType type)
        {
            return new CallbackStringWriter
            {
                DisposedCallback = (string content) =>
                {
                    _content[type.FullName] = content;
                },
            };
        }

        public bool TryReadType(IApiType type, out TextReader reader)
        {
            if (_content.TryGetValue(type.FullName, out string content))
            {
                reader = new StringReader(content);
                return true;
            }

            reader = null;
            return false;
        }

        public class CallbackStringWriter : StringWriter
        {
            public Action<string> DisposedCallback { get; set; }

            protected override void Dispose(bool disposing)
            {
                string content = GetStringBuilder()?.ToString();

                base.Dispose(disposing);

                if (disposing && content != null)
                {
                    DisposedCallback?.Invoke(content);
                }
            }
        }
    }
}
