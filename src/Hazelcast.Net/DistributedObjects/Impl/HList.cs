﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    /// <summary>
    /// Implements <see cref="IHList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the list items.</typeparam>
    internal partial class HList<T> : HCollectionBase<T>, IHList<T>
    {
        public HList(string name, DistributedObjectFactory factory, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HList.ServiceName, name, factory, cluster, serializationService, loggerFactory)
        { }

        public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            var items = await IterateAllAsync().CAF();
            foreach (var item in items)
                yield return item;
        }

        private async Task<IReadOnlyList<T>> IterateAllAsync()
        {
            var requestMessage = ListIteratorCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CAF();
            var response = ListIteratorCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<T>(response, SerializationService);
        }
    }
}