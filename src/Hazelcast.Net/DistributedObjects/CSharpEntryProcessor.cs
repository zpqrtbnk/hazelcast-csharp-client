using System;
using System.Collections.Generic;
using System.Text;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects
{
    public class CSharpEntryProcessor : IIdentifiedDataSerializable
    {
        private readonly byte[] _bytes;

        public CSharpEntryProcessor(byte[] bytes)
        {
            _bytes = bytes;
        }

        public void ReadData(IObjectDataInput input)
        {
            throw new NotImplementedException();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteArray(_bytes);
        }

        public int FactoryId { get; } = -3;

        public int ClassId { get; } = 6;
    }

    public interface ILoveEntryProcessor
    { }

    public interface ILoveEntryProcessor<TKey, TValue, TResult> : IEntryProcessor<TResult>, ILoveEntryProcessor
    {
        TResult Process(IEntry<TKey, TValue> entry);
    }

    public interface IEntry
    {
        public bool Mutated { get; }
        public object Value { get; }
    }

    public interface IEntry<TKey, TValue> : IEntry
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
    }

    public class Entry<TKey, TValue> : IEntry<TKey, TValue>
    {
        private TValue _value;

        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }

        public TValue Value
        {
            get => _value;
            set
            {
                Mutated = true;
                _value = value;
            }
        }

        object IEntry.Value => _value;

        public bool Mutated { get; private set; }
    }
}
