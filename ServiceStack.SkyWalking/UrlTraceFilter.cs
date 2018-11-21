using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace ServiceStack.SkyWalking
{
    public class UrlTraceFilter
    {
        private List<string> ls=new List<string>()
        {
            "/api/json/reply/heartbeat"
        };
        public bool IsTrace(string url)

        {
            foreach (var filter in ls)
            {
                if (url.Contains(filter))
                {
                    return false;
                }
                
            }

            return true;

        }
    }
}