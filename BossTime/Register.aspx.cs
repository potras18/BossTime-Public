using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            APIHandler api = new APIHandler();

            APIResponse resp = api.RegisterAccount(tbUsername.Text, tbPassword.Text, tbEmail.Text);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);

            lblStatus.Text = $"{resp.Message}\r\n\r\n{resp.Data}";

            if (resp.Success)
            {
                hdStatus.InnerText = "Success!";
            }
            else
            {
                hdStatus.InnerText = "Error!";
            }

        }

        protected void imgLogo_Click(object sender, ImageClickEventArgs e)
        {

        }
    }
}