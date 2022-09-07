using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Payloads
{
    public class CreatePagePL
    {
        public string Title { get; set; }
        public string Route { get; set; }
        public string Body { get; set; }
        public Guid LangId { get; set; }
    }
}
