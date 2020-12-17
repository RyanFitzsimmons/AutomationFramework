using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestMetaData : IMetaData
    {
        public string Test { get; set; } = "Test Meta String";
    }
}
