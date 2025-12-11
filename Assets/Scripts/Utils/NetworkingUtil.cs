using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class NetworkingUtil
    {
        private const string base_url = "http://localhost:7678";
        public static string GetLoginApiUrl()
        {
            return base_url + "/login";
        }

        public static string GetLoadTemplateApiUrl()
        {
            return base_url + "/load-templates";
        }
    }

}
