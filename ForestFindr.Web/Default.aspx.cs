using System;
using System.IO;
using System.Web;
using System.Xml;

namespace ForestFindr.Web
{
    public partial class Default : System.Web.UI.Page
    {
        public string InitParam;
       
        protected void Page_Load(object sender, EventArgs e)
        {
            InitParam = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
     }
}