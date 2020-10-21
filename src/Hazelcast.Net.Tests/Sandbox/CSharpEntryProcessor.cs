// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Examples.Polyglot;
using NUnit.Framework;

namespace Hazelcast.Tests.Sandbox
{
    // TODO
    // - bubble exceptions back to the user
    //    proto + reply.exception
    //    and rethrow from java
    // - get this to work as a normal test (via RC)

    [TestFixture]
    [Explicit("Requires ...?")]
    public class CSharpEntryProcessorTests //: SingleMemberClientRemoteTestBase
    {
        //protected override HazelcastOptions CreateHazelcastOptions()
        //{
        //    var options = base.CreateHazelcastOptions();
        //    options.Serialization.AddDataSerializableFactory(SomeFactory.FactoryId, new SomeFactory());
        //    return options;
        //}

        [Test]
        public async Task ExecuteCSharpProcessor()
        {
            using var _ = HConsole.Capture();

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options =>
            {
                options.Serialization.AddDataSerializableFactory(SomeFactory.FactoryId, new SomeFactory());
            });

            //var name = CreateUniqueName();
            var name = "a-random-name";
            var dict = await client.GetDictionaryAsync<string, string>(name);
            //await using var _ = DestroyAndDispose(dict);

            try
            {
                await dict.SetAsync("key", "value");

                var processor = new SomeProcessor();

                var result = await dict.ExecuteAsync(processor, "key");
                Assert.That(result, Is.EqualTo("key--value--5"));

                var value = await dict.GetAsync("key");
                Assert.That(value, Is.EqualTo("value--changed"));
            }
            finally
            {
                await dict.DestroyAsync();
            }
        }

        [Test]
        public async Task ExecuteCSharpProcessor2()
        {
            // start a new client
            await using var client = await HazelcastClientFactory.StartNewClientAsync();

            // obtain a dictionary (aka a "map" for you Java guys)
            var dict = await client.GetDictionaryAsync<string, string>("a-random-name");

            try
            {
                // add an entry
                await dict.SetAsync("key", "value");

                // instantiate the processor with a parameter
                var processor = new OtherProcessor { SomeString = "meh" };

                // execute and assert the result
                var result = await dict.ExecuteAsync(processor, "key");
                Assert.That(result, Is.EqualTo("key--value--5--meh"));

                // get then entry and assert it has been updated
                var value = await dict.GetAsync("key");
                Assert.That(value, Is.EqualTo("value--changed"));
            }
            finally
            {
                // cleanup behind us
                await dict.DestroyAsync();
            }
        }
    }
}
