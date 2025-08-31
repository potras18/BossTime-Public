using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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

            StripeConfiguration.ApiKey = StripeData.Stripe_API_Key;

            PopulatePackages();

        }

        private void PopulatePackages()
        {
            try
            {
                Stripe.ProductService ps = new Stripe.ProductService();
                List<Product> pdList = ps.List().ToList<Product>();



                foreach (Product cp in pdList)
                {
                    string packtype;
                    bool hasType = cp.Metadata.TryGetValue("Type", out packtype);




                    if (!hasType)
                    {
                        continue;
                    }

                    if (packtype == "CoinPack")
                    {
                        Stripe.PriceService prs = new Stripe.PriceService();
                        Price pp = prs.Get(cp.DefaultPriceId);

                        Button btn = new Button();
                        btn.Text = $"{cp.Description} - {pp.UnitAmount / 100.0m} {pp.Currency}";
                        btn.CssClass = "btnReg";
                        btn.CommandArgument = cp.Id;

                        btn.Click += btnDonate_Click;
                        dvPackages.Controls.Add(btn);


                    }
                }
            }
            catch (Exception)
            {
                lblStatus.Text = "Stripe API Error";
                hdStatus.InnerText = "Error!";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);
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

        protected void btnDonate_Click(object sender, EventArgs e)
        {

            Button btn = (Button)sender;

            //CoinPackage cp = StripeData.CoinPackages.Find((x)=>x.ID == btn.CommandArgument);

            string id = btn.CommandArgument;

            if (id != null)
            {
                CreatePaymentLink(id);
            }
            else
            {
                lblStatus.Text = "That coin package doesnt exist";
                hdStatus.InnerText = "Error!";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);
            }



        }

        private void CreatePaymentLink(string id)
        {
            try
            {


                ProductService productService = new ProductService();

                Product thisProduct = productService.Get(id);

                PriceService priceService = new PriceService();

                thisProduct.DefaultPrice = priceService.Get(thisProduct.DefaultPriceId);



                if (thisProduct != null)
                {

                    Debug.WriteLine(thisProduct.ToJson());

                    PaymentLinkCreateOptions plco = new PaymentLinkCreateOptions()
                    {
                        AfterCompletion = new PaymentLinkAfterCompletionOptions()
                        {
                            Type = "redirect",
                            Redirect = new PaymentLinkAfterCompletionRedirectOptions { Url = StripeData.Stripe_RedirectURL }
                        },
                        Metadata = new Dictionary<string, string>()
                        {
                            { "Username", usern },
                            { "Price", thisProduct.DefaultPrice.UnitAmount.ToString() },
                            { "Coins", thisProduct.Metadata["Coins"].ToString() },
                            { "PackageID", thisProduct.Id },
                            { "PackageName", thisProduct.Name },
                            { "Currency", thisProduct.DefaultPrice.Currency },
                            { "Item", "Donation" },
                        },
                        SubmitType = "donate",
                        PaymentMethodTypes = new List<string>
                        {
                            "card","link"
                        },
                        Currency = thisProduct.DefaultPrice.Currency,

                        PaymentIntentData = new PaymentLinkPaymentIntentDataOptions
                        {
                            CaptureMethod = "automatic",

                        },

                        LineItems = new List<PaymentLinkLineItemOptions>
                {
                    new PaymentLinkLineItemOptions
                    {
                        Price = thisProduct.DefaultPriceId,
                         Quantity = 1,

                    }
                }




                    };

                    PaymentLinkService pls = new PaymentLinkService();
                    PaymentLink pl = pls.Create(plco);


                    Response.Redirect(pl.Url);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "There was a problem processing this coin purchase " + ex.ToString();
                hdStatus.InnerText = "Error!";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Shroud", "ShowShroud();", true);
            }


        }
    }
}