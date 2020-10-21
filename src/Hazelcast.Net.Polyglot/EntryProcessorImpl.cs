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

using System;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;

namespace Hazelcast.Net.Polyglot
{
    internal class EntryProcessorImpl : Processor.ProcessorBase /*EntryProcessor.EntryProcessorBase*/
    {
        private readonly ISerializationService _serializationService;

        public EntryProcessorImpl(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        // server side handler of the EntryProcessor RPC
        public override Task<ProcessReply> process(ProcessRequest request, ServerCallContext context)
        {
            //var processorData = DataFactory.CreateData(request.ProcessorData.ToByteArray());
            var processorData = new HeapData(request.ProcessorData.ToByteArray());
            var keyData = new HeapData(request.KeyData.ToByteArray());
            IData valueData = new HeapData(request.ValueData.ToByteArray());

            var processor = _serializationService.ToObject(processorData);
            var processorType = processor.GetType();

            var key = _serializationService.ToObject(keyData);
            var value = _serializationService.ToObject(valueData);

            // TODO: cache + dynamic method
            var processMethod = processorType.GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
            if (processMethod == null) throw new InvalidOperationException("No Process method.");
            var processParameters = processMethod.GetParameters();
            //if (processParameters.Length != 2) throw new InvalidOperationException("Bad Process method.");
            if (processParameters.Length != 1) throw new InvalidOperationException("Bad Process method.");
            var entryType = processParameters[0].ParameterType;
            if (!entryType.IsGenericType) throw new InvalidOperationException("Bad parameter type.");
            var genericArguments = entryType.GetGenericArguments();
            if (genericArguments.Length != 2) throw new InvalidOperationException("Bad parameter type.");

            //var keyType = processParameters[0].ParameterType;
            //var valueType = processParameters[1].ParameterType;

            var keyType = genericArguments[0];
            var valueType = genericArguments[1];

            if (!keyType.IsInstanceOfType(key)) throw new InvalidOperationException("Bad key type.");
            if (!valueType.IsInstanceOfType(value)) throw new InvalidOperationException("Bad value type.");

            // entryType is IEntry<,> not Entry<,>
            var actualEntryType = typeof (Entry<,>).MakeGenericType(keyType, valueType);
            var entryCtor = actualEntryType.GetConstructor(new[] { keyType, valueType });
            var entry = (IEntry) entryCtor.Invoke(new object[] { key, value });

            var result = processMethod.Invoke(processor, new[] { entry });

            var resultData = _serializationService.ToData(result);
            var resultBytes = resultData.ToByteArray();

            valueData = _serializationService.ToData(entry.Value);
            var valueBytes = valueData.ToByteArray();

            return Task.FromResult(new ProcessReply
            {
                ResultData = ByteString.CopyFrom(resultBytes),
                NewValueData = ByteString.CopyFrom(valueBytes),
                Mutate = entry.Mutated,
            });
        }
    }
}