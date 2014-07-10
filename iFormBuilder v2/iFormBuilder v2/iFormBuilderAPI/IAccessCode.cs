using System;
namespace iFormBuilderAPI
{
    interface IAccessCode
    {
        string access_token { get; set; }
        int expires_in { get; set; }
        bool isExpired { get; }
        string token_type { get; set; }
    }
}
