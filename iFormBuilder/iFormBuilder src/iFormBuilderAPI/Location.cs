﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iFormBuilderAPI
{

    public class Location
    {
        public float X { get { return this.Longitude; } }
        public float Y { get { return this.Latitude; } }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public float Speed { get; set; }
        public float Accuracy { get; set; }
        public float Provider { get; set; }
        public float Time { get; set; }

        public void SetLocationFromField(string recordinformation)
        {
            //Replace any commas in the recordinformation with .
            recordinformation = recordinformation.Replace(',', '.');
            char[] splitter = ":".ToCharArray();
            string[] splitvalues = recordinformation.Split(splitter);
            if (splitvalues.Length == 8)
            {
                //47.048837:-122.903474:20.359947:93.174871:10.384083:-1.000000:-1.000000:1363968593.979734
                this.Latitude = float.Parse(splitvalues[0]);
                this.Longitude = float.Parse(splitvalues[1]);
                this.Altitude = float.Parse(splitvalues[2]);
                this.Speed = float.Parse(splitvalues[3]);
                this.Accuracy = float.Parse(splitvalues[4]);
                this.Provider = float.Parse(splitvalues[5]);
                this.Time = float.Parse(splitvalues[7]);
            }

            //Newer Format:  "Latitude:-18.149009.\nLongitude:49.401572.\nAltitude:17.594269.\nVitesse:0.340000.\nPrécision horizontale:5.000000.\nPrécision verticale:8.000000.\nTemps:19:04:17 UTC+3"
            if (splitvalues.Length == 16)
            {
                this.Latitude = float.Parse(splitvalues[1]);
                this.Longitude = float.Parse(splitvalues[3]);
                this.Altitude = float.Parse(splitvalues[5]);
                this.Speed = float.Parse(splitvalues[7]);
                this.Accuracy = float.Parse(splitvalues[9]);
                this.Time = float.Parse(splitvalues[13]);
            }
        }

        public void UpdateValuesFromField(string recordinformation)
        {
            recordinformation = recordinformation.Replace(',', '.');
            char[] splitter = ":".ToCharArray();
            string[] splitvalues = recordinformation.Split(splitter);
            if (splitvalues.Length == 8)
            {
                //47.048837:-122.903474:20.359947:93.174871:10.384083:-1.000000:-1.000000:1363968593.979734
                this.Altitude = float.Parse(splitvalues[2]);
                this.Speed = float.Parse(splitvalues[3]);
                this.Accuracy = float.Parse(splitvalues[4]);
                this.Provider = float.Parse(splitvalues[5]);
                this.Time = float.Parse(splitvalues[7]);
            }

            //Newer Format:  "Latitude:-18.149009.\nLongitude:49.401572.\nAltitude:17.594269.\nVitesse:0.340000.\nPrécision horizontale:5.000000.\nPrécision verticale:8.000000.\nTemps:19:04:17 UTC+3"
            if(splitvalues.Length == 16)
            {
                this.Latitude = float.Parse(splitvalues[1]);
                this.Longitude = float.Parse(splitvalues[3]);
                this.Altitude = float.Parse(splitvalues[5]);
                this.Speed = float.Parse(splitvalues[7]);
                this.Accuracy = float.Parse(splitvalues[9]);
                this.Time = float.Parse(splitvalues[13]);
            }
        }

        public void SetDefaultValues()
        {
            this.Latitude = 0;
            this.Longitude = 0;
            this.Altitude = 0;
            this.Speed = 0;
            this.Accuracy = 0;
            this.Provider = 0;
            this.Time = 0;
        }

        public object GetLocationValue(string fieldname)
        {
            switch(fieldname)
            {
                case "Latitude":
                    return this.Latitude;
                case "Longitude":
                    return this.Longitude;
                case "Altitude":
                    return this.Altitude;
                case "Speed":
                    return this.Speed;
                case "Accuracy":
                    return this.Accuracy;
                case "Provider":
                    return this.Provider;
                case "Time":
                    return this.Time;
            }
            return 0.0f;
        }
    }
}
