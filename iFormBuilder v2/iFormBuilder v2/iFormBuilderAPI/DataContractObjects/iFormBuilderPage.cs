using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    [DataContract]
    internal class iFormBuilderPage
    {
        [DataMember]
        internal Page PAGE;
        [DataMember]
        internal List<Element> ELEMENTS;
        [DataMember]
        internal String STATUS;
    }
}
