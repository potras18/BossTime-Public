using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class CustomError : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);
            hdStatus.InnerText = "Error!";
            lblStatus.Text = "There was an error with the page you were trying to view.";

            try
            {
                string sessionerror = Session["ErrorData"].ToString();
                if (!string.IsNullOrEmpty(sessionerror))
                {
                    lblStatus.Text = sessionerror;
                }
            }
            catch (Exception)
            {
                // No session error
            }

        }

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            try
            {
                string errpath = (Request.QueryString["aspxerrorpath"] != null) ? Request.QueryString["aspxerrorpath"] : "";
                if (errpath != "")
                {
                    Response.Redirect(errpath);
                }
                else
                {
                    Response.Redirect("Default.aspx");
                }
            }
            catch (Exception)
            {

            }
        }

        protected void btnHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }
    }
}