﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationFramework
{
    public interface IKernel
    {
        string Version { get; }
        string Name { get; }
        void Run(RunInfo runInfo, Func<object> getMetaData = null);
    }
}
