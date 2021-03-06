﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.UnitTests.TestSetup
{
    public class TestParallelModule : Module<TestModuleResult>
    {
        public TestParallelModule(IStageBuilder builder) : base(builder) { }

        public override string Name { get; init; } = "Test Parallel Stage With Result";
        public int RandomInteger { get; init; }
        public string[] Array { get; init; }

        protected override async Task<TestModuleResult> DoWork(CancellationToken token)
        {
            await Task.Run(() =>
            {
                var task = Task.Delay(RandomInteger, token);
                while (!task.IsCompleted)
                    foreach (var item in Array) Log(LogLevels.Information, item);
            }, token);
            return new TestModuleResult()
            {
                Name = Name,
                StringArray = new string[]
                {
                    "T1",
                    "T2",
                    "T3",
                },
            };
        }
    }
}
