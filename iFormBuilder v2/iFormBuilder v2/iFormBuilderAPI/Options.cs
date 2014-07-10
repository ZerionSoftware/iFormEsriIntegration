using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace iFormBuilderAPI
{
    public class Option
    {
        public Option()
        {
            this.SORT_ORDER = 1;
        }

        public string OPTION_ID { get; set; }
        public string OPTION_LIST_ID { get; set; }
        public string KEY_VALUE { get; set; }
        public string LABEL { get; set; }
        public int SORT_ORDER { get; set; }
        public string IS_DISABLED { get; set; }
        public string JUMP_OPTION_LIST_ID { get; set; }
        public string JUMP_OPTION_LIST_INDEX { get; set; }
        public string CONDITION_VALUE { get; set; }
    }
}
