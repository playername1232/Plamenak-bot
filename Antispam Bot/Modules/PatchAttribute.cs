using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plamenak_Bot.Modules
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    internal class PatchAttribute : Attribute
    {
        public string Patch { get; set; }

        public PatchAttribute(string patch)
        {
            if (patch == string.Empty)
                return;

            Patch = patch;
        }
        public override string ToString() => Patch;
    }
}
