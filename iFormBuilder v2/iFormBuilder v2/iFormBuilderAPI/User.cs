using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iFormBuilderAPI
{
    public class Users : iFormBuilderAPI.IUsers
    {
        public Users()
        {
        }
        public bool DETAIL { get; set; }
        public int OFFSET { get; set; }
        public int LIMIT { get; set; }
        public FilterKey SORT { get; set; }
        public List<UserFilter> FILTER { get; set; }

        public enum FilterKey { ID, USERNAME, FIRSTNAME, LASTNAME, EMAIL };
        public enum SORTORDERTYPE { ACS, DESC };
    }

    public class UserFilter : iFormBuilderAPI.IUserFilter
    {
        public Users.FilterKey KEY { get; set; }
        public string CONDITION { get; set; }
        public string VALUE { get; set; }
    }

    public class User
    {
        public int ID { get; set; }
        public string USERNAME { get; set; }
        public string EMAIL { get; set; }
        public string FIRSTNAME { get; set; }
        public int IS_ACTIVE { get; set; }
        public string LASTNAME  { get; set; }
        public long ROLE { get; set; }
        public string DisplayName
        {
            get
            {
                return string.Format("{0}, {1} <{2}>", LASTNAME, FIRSTNAME, EMAIL);
            }
        }
    }
}