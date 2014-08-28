using System;
namespace iFormBuilderAPI
{
    public interface IConfiguration
    {
        string arcgispassword { get; set; }
        string arcgisurl { get; set; }
        string arcgisusername { get; set; }
        string clientid { get; set; }
        string iformpassword { get; set; }
        string iformserverurl { get; set; }
        string iformusername { get; set; }
        //string pageid { get; set; }
        int profileid { get; set; }
        string refreshcode { get; set; }
    }
}
