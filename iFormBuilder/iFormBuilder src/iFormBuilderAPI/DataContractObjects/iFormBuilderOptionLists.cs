using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI.DataContractObjects
{
    /// <summary>
    /// Return a list of pages in the specified profile
    ///Version: 4.5
    ///Request method: GET
    ///https:// SERVER_NAME.iformbuilder.com/exzact/api/profiles/PROFILE_ID/pages
    ///parameter(s):
    ///FROM_MODIFIED_DATE  return pages that are last modified after this date and time
    ///TO_MODIFIED_DATE    return pages that are last modified before this date and time
    ///MODIFIED_BY	        return pages that are last modified by this user
    ///
    ///return(s):
    ///PAGES   (array) list of pages
    ///	ID	    ID of the page
    ///	NAME    name of the page
    ///
    /// {
    ///  "PAGES":[
    ///	  {
    ///		 "ID":6727,
    ///		 "NAME":"media_file_test"
    ///	  },
    ///	  {
    ///		 "ID":6772,
    ///		 "NAME":"testformbuilder"
    ///	  }
    ///   ],
    ///   "STATUS":true
    ///}
    /// </summary>
    [DataContract]
    internal class iFormBuilderOptionLists
    {
        [DataMember]
        internal List<OptionList> OPTIONLISTS;
        [DataMember]
        internal String STATUS;
    }
}
