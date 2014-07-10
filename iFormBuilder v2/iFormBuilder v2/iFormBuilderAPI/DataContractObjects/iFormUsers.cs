using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace iFormBuilderAPI.DataContractObjects
{
    [DataContract]
    internal class iFormUsers
    {
        [DataMember]
        internal List<User> USERS { get; set; }
    }
}
