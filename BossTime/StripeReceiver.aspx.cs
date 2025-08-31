using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class StripeReceiver : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.ContentType.Contains("application/json"))
                {
                    CheckStripeID(GetRequestBody());
                }
            }
            catch (Exception)
            {

            }
        }

        private void CheckStripeID(string data)
        {
            Stripe.Event evnt = Stripe.Event.FromJson(data);
            string evntname = evnt.Data.Object.Object;
            Debug.WriteLine(evntname);
            
            switch (evntname)
            {
                case "payment_link":
                    HandlePaymentLink(evnt);
                    break;
                case "Charge":
                 

                case "PaymentIntent":
                    

                case "checkout.session":
                    HandleSessionCompleted(evnt);
                    break;

            }
        }

        private void HandleSessionCompleted(Stripe.Event sEvent)
        {
            try
            {
                string evntname = sEvent.Data.Object.GetType().Name;
                
                EventService evs = new EventService();
                Event CurrentEvent = evs.Get(sEvent.Id);
                if(CurrentEvent != null)
                {



                    SessionService sessionService = new SessionService();
                    Session session = (Session)CurrentEvent.Data.Object;
                    session = sessionService.Get(session.Id);
                    
                    if(session != null)
                    {


                        string paymentStatus = session.PaymentStatus;

                        if (paymentStatus != null && paymentStatus.ToLower() == "paid")
                        {

                            int amounttotal = (int)session.AmountTotal;
                            string username = session.Metadata["Username"];
                            string userhash = session.Metadata["UserHash"];
                            int coins = Convert.ToInt32(session.Metadata["Coins"]);
                            string packname = session.Metadata["PackageName"];
                            string packid = session.Metadata["PackageID"];
                            string eventid = CurrentEvent.Id;
                            string uniquekey = session.Metadata["UniqueKey"];
                            int price = Convert.ToInt32(session.Metadata["Price"]);
                            string paymentLinkID = session.PaymentLinkId;

                            Debug.WriteLine("Sending Update");
                            ptPaymentIntent ptPaymentIntent = new ptPaymentIntent(username, userhash, packid, packname, price, coins, paymentLinkID, eventid, uniquekey, session.Id);

                            APIHandler aPI = new APIHandler();
                            aPI.UpdatePaymentCompleted(ptPaymentIntent);
                        }
                        else
                        {
                            Debug.WriteLine("Payment not completed yet");
                        }

                    }
                    else
                    {
                        Debug.WriteLine("Session Null");
                    }
                }
                else
                {
                    Debug.WriteLine("Current Event Null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void HandlePaymentLink(Stripe.Event sEvent){

            try
            {
                PaymentLinkService pls = new PaymentLinkService();
                EventService evs = new EventService();
                Event curev = evs.Get(sEvent.Id);
                PaymentLink pLink = (PaymentLink)curev.Data.Object;

                PaymentLinkService LinkService = new PaymentLinkService();

                pLink = LinkService.Get(pLink.Id);

           

                string username = pLink.Metadata["Username"];
                string userhash = pLink.Metadata["UserHash"];
                int coins = Convert.ToInt32(pLink.Metadata["Coins"]);
                string packname = pLink.Metadata["PackageName"];
                string packid = pLink.Metadata["PackageID"];
                string eventid = curev.Id;
                int price = Convert.ToInt32(pLink.Metadata["Price"]);
                string reqID = curev.Request.Id;
                string UniqueKey = pLink.Metadata["UniqueKey"];
                string paymentLinkID = pLink.Id;


                ptPaymentIntent payintent = new ptPaymentIntent(username, userhash, packid, packname, price, coins, paymentLinkID, eventid, UniqueKey,reqID);

                APIHandler api = new APIHandler();

                api.RegisterPaymentIntent(payintent);
                


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public string GetRequestBody()
        {
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }

    }
}