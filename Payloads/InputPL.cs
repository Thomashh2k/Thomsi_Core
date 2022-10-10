using Headless.DB.DataObj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Payloads
{
    public class InputPL
    {
        public string InputName { get; set; }
        public long? InputLength { get; set; }
        public bool NotNullable { get; set; }
        public InputType InputType { get; set; }
        public bool Delete { get; set; }
        public bool New { get; set; }

        public Input ToInput()
        {
            Input input = new Input 
            { 
                InputName = InputName,
                InputLength = InputLength,
                InputType = InputType,
                NotNullable = NotNullable
            };

            return input;
        }
    }
}
