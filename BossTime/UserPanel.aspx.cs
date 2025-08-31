using Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class UserPanel : System.Web.UI.Page
    {

        string usern = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string auth = Request.Cookies["Auth"].Value;
                if (!string.IsNullOrEmpty(auth))
                {
                    APIHandler api = new APIHandler();
                    APIResponse resp = api.ValidateToken(auth);
                    if (resp.Success)
                    {
                        lblWelcome.Text = $"Welcome, {resp.Data}";
                        usern = resp.Data.ToString();
                    }
                    else
                    {
                        Response.Cookies.Remove("Auth");
                        Response.Redirect("Login.aspx");
                    }
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
            catch (Exception)
            {
                Response.Redirect("login.aspx");
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            if (Request.Cookies["Auth"] != null)
            {
                Response.Cookies["Auth"].Expires = DateTime.Now.AddDays(-1);
            }
            Response.Redirect("Login.aspx");
        }

        protected void btnDonate10_Click(object sender, EventArgs e)
        {
            StripeConfiguration.ApiKey = StripeData.Stripe_API_Key;



            PaymentLinkCreateOptions plco = new PaymentLinkCreateOptions()
            {
                AfterCompletion = new PaymentLinkAfterCompletionOptions()
                {
                    Type = "redirect",
                    Redirect = new PaymentLinkAfterCompletionRedirectOptions { Url = "https://www.google.co.uk" }
                },
                Metadata = new Dictionary<string, string>()
                {
                    { "Username", usern },
                    { "Amount", "10" },
                    { "Currency", StripeData.Stripe_Currency },
                    { "Item", "Donation" },
                },
                SubmitType = "donate",
                PaymentMethodTypes = new List<string>
                {
                    "card","link"
                },
                Currency = StripeData.Stripe_Currency,

                PaymentIntentData = new PaymentLinkPaymentIntentDataOptions
                {
                    CaptureMethod = "automatic",

                },

                LineItems = new List<PaymentLinkLineItemOptions>
                {
                    new PaymentLinkLineItemOptions
                    {
                        Price = "price_1S1w1vC7XrBW2LT3ekILY8Og",
                         Quantity = 1,

                    }
                }




            };

            PaymentLinkService pls = new PaymentLinkService();
            PaymentLink pl =  pls.Create(plco);


            Response.Redirect(pl.Url);


            Debug.WriteLine(pl.ToJson());

        }
    }
}