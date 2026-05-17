using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMetadata
    {
        [DataMember]
        public List<string> Headers { get; set; }
    }
}