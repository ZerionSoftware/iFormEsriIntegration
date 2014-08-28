using System;
using System.Collections.Generic;
namespace iFormBuilderAPI
{
    interface IUsers
    {
        bool DETAIL { get; set; }
        List<UserFilter> FILTER { get; set; }
        int LIMIT { get; set; }
        int OFFSET { get; set; }
        Users.FilterKey SORT { get; set; }
    }
}
