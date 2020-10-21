using System;
using System.Threading.Tasks;
using Grpc.Core;
using Hazelcast.Aggregating;
using Hazelcast.Examples.Polyglot;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Net.Polyglot
{
    public class Program
    {
        //private const int Port = 30001;
        private const int Port = 50051;

        public static async Task Main(string[] args)
        {
            // TODO not pretty!
            var options = new SerializationOptions();
            options.AddDataSerializableFactory(SomeFactory.FactoryId, new SomeFactory());

            var loggerFactory = new NullLoggerFactory();

            var serializationService = new SerializationServiceBuilder(loggerFactory)
                .SetConfig(options)
                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy()) // TODO: should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                .Build();

            var server = new Server
            {
                Services = { /*Entry*/Processor.BindService(new EntryProcessorImpl(serializationService)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine($"Server listening on port {Port}.");
            Console.WriteLine("Pres any key to stop...");
            Console.ReadKey();

            await server.ShutdownAsync();
        }
    }
}
