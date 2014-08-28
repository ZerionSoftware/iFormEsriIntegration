using System;
using System.Collections.Generic;
namespace iFormBuilderAPI
{
    /// <summary>
    /// Interface for the Page
    /// </summary>
    public interface IPage
    {
        string _S_ICON_LINK { get; set; }
        string CREATED_BY { get; set; }
        string CREATED_DATE { get; set; }
        string DESCRIPTION { get; set; }
        int ID { get; set; }
        string LABEL { get; set; }
        string MODIFIED_BY { get; set; }
        string MODIFIED_DATE { get; set; }
        string NAME { get; set; }
        string PAGE_TYPE { get; set; }
        string STATUS { get; set; }
        string VERSION { get; set; }
    }
}
