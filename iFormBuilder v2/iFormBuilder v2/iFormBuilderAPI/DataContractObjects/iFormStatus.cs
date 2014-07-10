using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    [DataContract]
    internal class iFormBuilderStatus
    {
        [DataMember]
        internal bool STATUS;
    }

    [DataContract]
    internal class RecordsStatus
    {

        [DataMember]
        internal bool STATUS;

        [DataMember]
        internal List<RecordStatus> RECORDS;
    }

    [DataContract]
    internal class RecordStatus
    {
        [DataMember]
        internal long RECORD_ID;
    }
}
