using System;

namespace IATClient
{
    class CServerURL
    {
        private static String sDefaultIATServer = Properties.Resources.sDefaultIATServerDomain;
        private String URL;
        private String Servlet;
        private bool bURLRepaired;

        public CServerURL(String url, String servlet)
        {
            URL = url;
            Servlet = servlet;
            bURLRepaired = false;
        }

        public bool IsValidServerURL
        {
            get
            {
                if (!RepairURL())
                    return false;
                return true;
            }
        }

        public String FullURL
        {
            get
            {
                if (!bURLRepaired)
                    if (!RepairURL())
                        return String.Empty;
                return URL + Servlet;
            }
        }


        public bool RepairURL()
        {
            if (URL.Substring(0, 7).ToLower() != "http://")
                URL = "http://" + URL;
            else if (!URL.StartsWith("http://"))
                URL = "http://" + URL.Substring(7, URL.Length - 7);
            bool dotFound = false;
            int ctr;
            for (ctr = 7; ctr < URL.Length; ctr++)
                if (URL[ctr] == '.')
                {
                    dotFound = true;
                    break;
                }
            if ((!dotFound) || (ctr == URL.Length))
                return false;
            bool slashFound = false;
            while (ctr < URL.Length)
                if (URL[ctr++] == '/')
                {
                    slashFound = true;
                    break;
                }
            if (!slashFound)
            {
                URL = URL + "/" + sDefaultIATServer + "/";
                return true;
            }
            else if (ctr == URL.Length)
            {
                URL += sDefaultIATServer + "/";
                return true;
            }
            else if (URL.Substring(ctr, URL.Length - ctr).Contains("."))
                return false;
            else if (URL.Substring(ctr, URL.Length - ctr).EndsWith("/"))
                return true;
            URL = URL + "/";
            return true;
        }
    }
}
