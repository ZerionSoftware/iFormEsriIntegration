using System;
namespace iFormBuilderAPI
{
    interface IUserFilter
    {
        string CONDITION { get; set; }
        Users.FilterKey KEY { get; set; }
        string VALUE { get; set; }
    }
}
