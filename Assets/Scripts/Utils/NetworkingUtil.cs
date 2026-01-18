using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class NetworkingUtil
    {
        public static string GetLoginApiUrl()
        {
            return GetBaseUri() + ":" + GetPortHttp() + "/login";
        }

        public static string GetRegisterApiUrl()
        {
            return GetBaseUri() + ":" + GetPortHttp() + "/register";
        }

        public static string GetLoadTemplateApiUrl()
        {
            return GetBaseUri() + ":" + GetPortHttp() + "/load-templates";
        }


        public static string GetBaseUri()
        {
           return "http://localhost";
          // return "http://103.67.199.174";
        }

        public static int GetPortHttp()
        {
            return 7678;
        }

        public static int GetPortTcp()
        {
            return 7679;
        }
    }

}
