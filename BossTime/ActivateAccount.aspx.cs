using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class ActivateAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string requestCode = (Request.QueryString["AuthCode"] != null) ? Request.QueryString["AuthCode"] : "";

            if (string.IsNullOrEmpty(requestCode) || !SystemVariables.RequireEmailAccountActivation)
            {
                Response.Redirect("Login.aspx");
            }
            else
            {
                try
                {
                    string decodedauth = APIHandler.Decrypt(requestCode);
                    EmailAuthentication auth = JsonConvert.DeserializeObject<EmailAuthentication>(decodedauth);
                    APIHandler api = new APIHandler();
                    APIResponse resp = api.ActivateAccount(auth.Username,auth.Email);

                    if (resp.Success)
                    {
                        Response.Write($"Activated Account for {auth.Username}");
                        Response.Redirect($"Login.aspx?username={auth.Username}&act=1");
                    }
                    else
                    {
                        Response.Write($"Failed to Activate Account for {auth.Username}: {resp.Message}: {resp.Data}");
                        Response.Redirect("CustomError.aspx");
                    }
                }
                catch (Exception)
                {
                    Response.Write("Error Activating Account.");
                    Response.Redirect("CustomError.aspx");
                }
            }

        }
    }
}
