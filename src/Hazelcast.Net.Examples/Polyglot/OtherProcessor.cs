using System;
using Hazelcast.DistributedObjects;

namespace Hazelcast.Examples.Polyglot
{
    // processor is serialized natively by C#
    // is ILoveEntryProcessor so we can identify it (yea naming is hard)

    [Serializable]
    public class OtherProcessor : ILoveEntryProcessor<string, string, string>
    {
        // the processor main method
        public string Process(IEntry<string, string> entry)
        {
            // compute the result
            var result = entry.Key + "--" + entry.Value + "--" + entry.Value.Length + "--" + SomeString;

            // mutate the entry (optional)
            entry.Value += "--changed";

            // return the result
            return result;
        }

        // a parameter for the processor
        public string SomeString { get; set; }
    }
}
