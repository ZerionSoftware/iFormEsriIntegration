using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    /// <summary>
    /// </summary>
    [DataContract]
    internal class iFormBuilderPages
    {
        [DataMember]
        internal List<Page> PAGES;
        [DataMember]
        internal String STATUS;
    }
}
