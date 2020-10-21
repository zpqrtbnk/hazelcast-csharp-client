using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.Polyglot
{
    public class SomeProcessor : ILoveEntryProcessor<string, string, string>, IIdentifiedDataSerializable
    {
        public const int ProcessorClassId = 57;

        public string Process(IEntry<string, string> entry)
        {
            var result = entry.Key + "--" + entry.Value + "--" + entry.Value.Length;
            entry.Value += "--changed";
            return result;
        }

        public void ReadData(IObjectDataInput input)
        {
            // nothing yet
        }

        public void WriteData(IObjectDataOutput output)
        {
            // nothing yet
        }

        public int FactoryId => SomeFactory.FactoryId;

        public int ClassId => ProcessorClassId;
    }
}
