
using System.Collections.Generic;
using System.Linq;

namespace MissingReferenceFinder
{
    public class AssemblyInfoListItem
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsInGAC { get; set; }
        public bool IsMissing { get; set; }
        public bool HasError { get; set; }
        public string ErrorText { get; set; }
        public bool HasMissingChild { get { return ReferredAssemblies.Any(r => r.IsMissing); } }

        public List<AssemblyInfoListItem> ReferredAssemblies { get; set; }
        public List<AssemblyInfoListItem> MissingAssemblies { get { return ReferredAssemblies.Where(r => r.IsMissing).ToList(); } }
    }
}
