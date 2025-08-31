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

            if (tbPassword.Text == tbConfPass.Text)
            {

                APIHandler api = new APIHandler();

                APIResponse resp = api.RegisterAccount(tbUsername.Text, tbPassword.Text, tbEmail.Text);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);

                lblStatus.Text = $"{resp.Message}\r\n\r\n{resp.Data}";

                if (resp.Success)
                {
                    hdStatus.InnerText = "Success!";
                    lblStatus.Text = $"{resp.Message}\r\n\r\nYou will be redirected to the login page in 5 seconds.\r\n\r\n{resp.Data}";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "WaitRedirect", "function Redirect(){\r\n    setTimeout(() => {\r\n  console.log(\"Redirecting to Login.\");\r\n        window.location.href=\"Login.aspx\";\r\n}, \"5000\");\r\n} Redirect();", true);
                }
                else
                {
                    hdStatus.InnerText = "Error!";
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);
                hdStatus.InnerText = "Error!";
                lblStatus.Text = "The passwords do not match.";
            }

        }

        protected void imgLogo_Click(object sender, ImageClickEventArgs e)
        {

        }



        protected void lbLoginAccount_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }
    }
}