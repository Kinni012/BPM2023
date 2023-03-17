using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore
{
    public class NamedStream
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public NamedStream() { }

        public NamedStream(string name, Stream stream)
        {
            Stream = stream;
            Name = name;
        }
    }
}
