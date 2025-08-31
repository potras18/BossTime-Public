using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }




        protected void btnLogin_Click(object sender, EventArgs e)
        {
            APIHandler api = new APIHandler();

            APIResponse resp = api.LoginAccount(tbUsername.Text, tbPassword.Text);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);

            lblStatus.Text = $"{resp.Message}";

            if (resp.Success)
            {
                hdStatus.InnerText = "Success!";
                Response.Cookies.Add(new HttpCookie("Auth", resp.Data.ToString()));
                Response.Redirect("UserPanel.aspx");
            }
            else
            {
                hdStatus.InnerText = "Error!";

                Response.Cookies.Remove("Auth");
            }
        }

        protected void lbCreateAcc_Click(object sender, EventArgs e)
        {
            Response.Redirect("Register.aspx");
        }
    }
}