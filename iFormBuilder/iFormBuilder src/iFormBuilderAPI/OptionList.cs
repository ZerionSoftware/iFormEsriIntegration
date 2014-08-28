using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI
{
    public class OptionList
    {
        public int ID { get; set; }
        public int OPTION_LIST_ID { get; set; }
        public string NAME { get; set; }
        public string VERSION { get; set; }
        public string CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public string MODIFIED_DATE { get; set; }
        public string MODIFIED_BY { get; set; }
        public List<Option> OPTIONS { get; set; }

        public string GetParameters
        {
            get
            {
                string.Format("NAME={0}", this.NAME);
                return string.Format("NAME={0}&{1}", this.NAME,this.ConvertToJson);
            }

        }

        public bool AddOption(Option option)
        {
            bool allowadd = true;
            foreach (Option opt in OPTIONS)
            {
                if (opt.KEY_VALUE == option.KEY_VALUE && opt.LABEL == option.LABEL)
                {
                    allowadd = false;
                    break;
                }
            }

            if (allowadd)
                OPTIONS.Add(option);

            return allowadd;
        }

        public Dictionary<string, string> OptionsDictionary
        {
            get
            {
                Dictionary<string, string> optlist = new Dictionary<string, string>();
                foreach (Option opt in this.OPTIONS)
                {
                    optlist.Add(opt.KEY_VALUE, opt.LABEL);
                }

                return optlist;
            }
        }
        private string ConvertToJson
        {
            get
            {
                string optionlist = "";
                int optioncount = 0;
                foreach (Option opt in OPTIONS)
                {
                    if (optioncount > 0)
                        optionlist += "&";

                    optionlist += string.Format("OPTIONS[{0}][KEY_VALUE]={1}&", optioncount, opt.KEY_VALUE);
                    optionlist += string.Format("OPTIONS[{0}][LABEL]={1}&", optioncount, opt.LABEL);
                    optionlist += string.Format("OPTIONS[{0}][SORT_ORDER]={1}", optioncount, optioncount + 1);
                    if(opt.CONDITION_VALUE != null)
                        optionlist += string.Format("&OPTIONS[{0}][CONDITION_VALUE]={1}", optioncount, opt.CONDITION_VALUE);
                    optioncount++;
                }
                return optionlist;
            }
        }
    }
}
