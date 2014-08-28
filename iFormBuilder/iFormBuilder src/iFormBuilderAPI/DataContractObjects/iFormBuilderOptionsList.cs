using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    /// <summary>
    /// 
    ///OPTIONLIST      (hash) contains the optionlist information
    ///OPTION_LIST_ID	optionlist ID
    ///NAME            name of the optionlist
    ///VERSION         version of the optionlist
    ///CREATED_DATE    created date of the page
    ///CREATED_BY      creator of the page
    ///MODIFIED_DATE   last modified date of the page
    ///MODIFIED_BY     last modifier of the page
    ///IS_DOWNLOADABLE (boolean) whether if the optionlist is downloadable
    ///OPTIONS (array) options of the optionlist
    /// </summary>
    [DataContract]
    internal class iFormBuilderOptionList
    {
        [DataMember]
        internal OptionList OPTIONLIST;
        [DataMember]
        internal String OPTIONLIST_ID;
        [DataMember]
        internal List<Option> OPTIONS;
        [DataMember]
        internal String STATUS;
    }
}
