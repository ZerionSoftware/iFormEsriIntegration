using System;
namespace iFormBuilderAPI
{
    public interface IElement
    {
        int ID { get; set; }
        string ATTACHMENT_LINK { get; set; }
        string CLIENT_VALIDATION { get; set; }
        string CONDITION_VALUE { get; set; }
        int DATA_SIZE { get; set; }
        int DATA_TYPE { get; set; }
        string DESCRIPTION { get; set; }
        string DYNAMIC_VALUE { get; set; }
        string HIGH_VALUE { get; set; }
        bool IS_ACTION { get; set; }
        bool IS_DISABLED { get; set; }
        bool IS_ENCRYPT { get; set; }
        bool IS_HIDE_TYPING { get; set; }
        bool IS_READONLY { get; set; }
        bool IS_REQUIRED { get; set; }
        string LABEL { get; set; }
        string LOW_VALUE { get; set; }
        string NAME { get; set; }
        int OPTION_LIST_ID { get; set; }
        string REFERENCE_ID_1 { get; set; }
        string REFERENCE_ID_2 { get; set; }
        string REFERENCE_ID_3 { get; set; }
        string REFERENCE_ID_4 { get; set; }
        string REFERENCE_ID_5 { get; set; }
        string SMART_TBL_SEARCH { get; set; }
        string SMART_TBL_SEARCH_COL { get; set; }
        string SORT_ORDER { get; set; }
        string VALIDATION_MESSAGE { get; set; }
        string WIDGET_TYPE { get; set; }
        string Alias { get; }
    }
}
