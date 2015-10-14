using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapEntrySetCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapEntrySet;
        public const int ResponseType = 117;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;

            public static int CalculateDataSize(string name)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IList<KeyValuePair<IData, IData>> entrySet;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            IList<KeyValuePair<IData, IData>> entrySet = null;
            int entrySet_size = clientMessage.GetInt();
            entrySet = new List<KeyValuePair<IData, IData>>();
            for (int entrySet_index = 0; entrySet_index < entrySet_size; entrySet_index++)
            {
                KeyValuePair<IData, IData> entrySet_item;
                entrySet_item = clientMessage.GetMapEntry();
                entrySet.Add(entrySet_item);
            }
            parameters.entrySet = entrySet;
            return parameters;
        }

    }
}
