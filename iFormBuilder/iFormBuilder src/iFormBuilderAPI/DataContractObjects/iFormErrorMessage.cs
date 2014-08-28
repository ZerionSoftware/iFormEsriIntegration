using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    [DataContract]
    internal class iFormErrorMessage
    {
        ///{"MESSAGE":"Require name to create page","CODE":400,"STATUS":false}
        ///
        [DataMember]
        internal string MESSAGE;

        [DataMember]
        internal int CODE;

        [DataMember]
        internal bool STATUS;
    }
}
