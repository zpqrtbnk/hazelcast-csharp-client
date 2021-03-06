﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples
{
    // ReSharper disable once UnusedMember.Global
    public class LoggingExample
    {
        // run this example with
        // ./hz.ps1 run-example Logging 
        // ./hz.ps1 run-example Logging --% --Logging:LogLevel:Hazelcast.Examples.LoggingExample.A=Debug

        public static void Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast.Examples.LoggingExample.B", "Information")
                .Build();

            var loggerFactory = options.LoggerFactory.Service;

            var loggerA = loggerFactory.CreateLogger<A>();

            // default level is None - nothing shows
            loggerA.LogDebug("debug.a");
            loggerA.LogInformation("info.a");
            loggerA.LogWarning("warning.a");

            var loggerB = loggerFactory.CreateLogger<B>();

            // level is info - first line is skipped
            loggerB.LogDebug("debug.b");
            loggerB.LogInformation("info.b");
            loggerB.LogWarning("warning.b");

            // flush logs!
            loggerFactory.Dispose();
        }

        public class A
        { }

        public class B
        { }
    }
}
