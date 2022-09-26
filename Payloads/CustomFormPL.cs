using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Payloads
{
    public class CustomFormPL
    {
        public string FormName { get; set; }
        public InputPL[] Inputs { get; set; }
    }
}
