using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
//using System.Threading.Tasks;

namespace iFormBuilderAPI
{
    public class Element : IElement
    {
        /// <summary>
        /// ID of the element
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// name of the element
        /// </summary>
        public string NAME { get; set; }

        ///label of the element
        public string LABEL { get; set; }

        //description of the element
        public string DESCRIPTION { get; set; }

        //data type of the element
        public int DATA_TYPE { get; set; }

        //data size of the element
        public int DATA_SIZE { get; set; }

        //widget type of the element
        public string WIDGET_TYPE { get; set; }

        //sort order of the element on page
        public string SORT_ORDER { get; set; }

        //option list ID for pick list data type
        public int OPTION_LIST_ID { get; set; }

        //low value of the element
        public string LOW_VALUE { get; set; }

        //high value of the element
        public string HIGH_VALUE { get; set; }

        //dynamic value of the element
        public string DYNAMIC_VALUE { get; set; }

        //condition value of the element
        public string CONDITION_VALUE { get; set; }

        //(boolean) whether the element is required for submission
        public bool IS_REQUIRED { get; set; }

        //client validation of the element
        public string CLIENT_VALIDATION { get; set; }

        //(boolean) whether the element is disabled on page
        public bool IS_DISABLED { get; set; }

        //reference ID for functionality
        public string REFERENCE_ID_1 { get; set; }

        //reference ID for functionality
        public string REFERENCE_ID_2 { get; set; }

        //reference ID for functionality
        public string REFERENCE_ID_3 { get; set; }

        //reference ID for functionality
        public string REFERENCE_ID_4 { get; set; }

        //reference ID for functionality
        public string REFERENCE_ID_5 { get; set; }

        //attachment link for attachment data type
        public string ATTACHMENT_LINK { get; set; }

        //(boolean) whether the element is readonly
        public bool IS_READONLY { get; set; }

        //validation message of the element
        public string VALIDATION_MESSAGE { get; set; }

        //(boolean) whether the user list needs to be downloaded for email to and assign to data type
        public bool IS_ACTION { get; set; }

        //name of the page for smart table lookup
        public string SMART_TBL_SEARCH { get; set; }

        //column name of the page for smart table lookup
        public string SMART_TBL_SEARCH_COL { get; set; }

        //(boolean) whether the data is encrypted
        public bool IS_ENCRYPT { get; set; }

        //(boolean) when put on true, data entered in the text field will not be displayed
        public bool IS_HIDE_TYPING { get; set; }

        public bool IgnoreElement
        {
            get
            {
                return (DATA_TYPE == 16 || DATA_TYPE == 17 || DATA_TYPE == 18);
            }
        }
        public string Alias
        {
            get
            {
                if (DESCRIPTION == "My description")
                    return this.NAME;
                else
                    return this.LABEL;
            }
        }

        public enum Widget
        {
            Text = 1,
            Number = 2,
            Date = 3,
            Time = 4,
            DateTime = 5,
            Toggle = 6,
            Select = 7,
            PickList = 8,
            MultiSelect = 9,
            Range = 10,
            Image = 11,
            Signature = 12,
            Sound = 13,
            QRCode = 15,
            Label = 16,
            Divider = 17,
            Subform = 18,
            TextArea = 19,
            Phone = 20,
            SSN = 21,
            Email = 22,
            ZipCode = 23,
            AssignTo = 24,
            UniqueID = 25,
            Drawing = 28,
            RFID = 31,
            Attachment = 32,
            ReadOnly = 33,
            ImageLabel = 35,
            Location = 37,
            SocketScanner = 38,
            LineaPro = 39
        };

        public Widget WidgetType
        {
            get{
                return (Widget) Enum.Parse(typeof(Widget),DATA_TYPE.ToString());
            }
        }
    }
}
