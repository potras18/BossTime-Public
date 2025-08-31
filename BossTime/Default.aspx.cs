using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnBossTimer_Click(object sender, EventArgs e)
        {
            Response.Redirect("BossTimer.aspx");
        }

        protected void btnUserPanel_Click(object sender, EventArgs e)
        {
            Response.Redirect("UserPanel.aspx");
        }
    }
}