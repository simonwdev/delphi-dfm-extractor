using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Delphi
{
    public class DfmObject
    {
        public DfmObject()
        {
            Children = new List<DfmObject>();
            Properties = new List<DfmProperty>();
        }

        public List<DfmObject> Children { get; set; }
        public List<DfmProperty> Properties { get; set; }
        public string ClassName { get; set; }
        public string ObjectName { get; set; }
    }

    public class DfmProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
