﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BossTime
{

    // System Variables
    /// <summary>
    /// These variables are used throughout the system and can be adjusted here.
    /// For example, you can change the username, password and server which the system will try to connect to.
    /// Note: Changing these variables will affect the entire system.
    /// Be sure to update any related configurations or documentation accordingly.
    /// </summary>
    /// 
    public static class DBCredentials
    {

        public static string dbID = "YOUR SQL USER ID";
        public static string dbPass = "YOUR SQL PASSWORD";
        public static string server = "SERVER\\SQLEXPRESS";

        

    }

    // Encryption Variables
    // These variables are used for encryption and decryption of sensitive data.
    /// <summary>
    /// This class contains system-wide variables used for encryption and decryption of sensitive data.
    /// Be sure to change the EncryptionKey to something unique and secure.
    /// The AES_IV can be modified here, or you can implement a system to use a unique IV for each user for increased security.
    /// The system is designed to include a datetime with login data, which will randomize the data regardless, making the IV slightly less critical.
    /// </summary>
    /// 
    public static class SystemVariables
    {
        //This encryption pass is used for encrypting and decrypting sensitive data for your login data, 
        //Be sure to change this to something unique and secure.
        //Maximum length 32 characters
        public static string EncryptionKey = "BossTimeEncryptionKey2025!";

        public static bool RequireEmailAccountActivation = true;

        //Set your base URL here, used for email links and redirects.
        public static string BaseURL = "https://YOURURL/";

        public static string ServerName = "YOUR PT SERVER NAME eg. AwesomePT";

        // Email Variables
        // These variables are used for sending emails from the system.
        // Be sure to set the EmailAddress, EmailPassword, EmailSMTP, and EmailPort to your actual email settings.
        public static string EmailAddress = "YOUR EMAIL ADDRESS";

        public static string EmailPassword = "YOUR EMAIL PASSWORD";

        // Common SMTP servers are smtp.yoursite.com or mail.yoursite.com. Check with your email provider if unsure.
        public static string EmailSMTP = "smtp.yoursite.com";

        // Common ports are 25, 465, and 587. Check with your email provider if unsure.
        public static int EmailPort = 587;

        // Set to true if your SMTP server requires TLS/SSL.
        public static bool EmailTLS = true;


        //Set your Initialization vector here, or modify your database to contain a specific vector for each user for increased security.
        // The system is designed to include a datetime with login data, with each login, which will randomize the data regardless, making the IV slightly pointless.
        public static byte[] AES_IV = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }


    // Stripe Variables
    /// <summary>
    /// This class contains system-wide variables used for Stripe payment processing.
    /// Be sure to set the Stripe_API_Key to your actual Stripe secret key.
    /// Also, set the Stripe_RedirectURL to the URL you want users to be redirected to after completing a transaction.
    /// </summary>
    /// 
    public static class StripeData
    {
        // Set your Stripe API key here. Be sure to use the secret key, not the publishable key.
        public static string Stripe_API_Key = "YOUR STRIPE API KEY";

        //Set your redirect URL for stripe to use after the transaction is complete
        public static string Stripe_RedirectURL = "https://YOURURL/UserPanel.aspx";


    }

    // Registration Variables
    /// <summary>
    ///
    /// These variables are used during the registration process and can be adjusted here.
    /// For example, you can change the minimum and maximum lengths for usernames, passwords, and emails.
    /// You can also change the allowed characters for usernames and passwords.
    /// Note: Changing these variables will not affect existing accounts, only new registrations.
    /// Be sure to keep the allowed characters in sync with any client-side validation you may have.
    /// Also, be sure to keep the minimum and maximum lengths in sync with any client-side validation you may have.
    /// </summary>
    /// 
    public static class RegisterVariables
    {
        public static int MinUsernameLength = 6;
        public static int MaxUsernameLength = 30;
        public static int MinPasswordLength = 8;
        public static int MaxPasswordLength = 50;
        public static int MinEmailLength = 8;
        public static int MaxEmailLength = 64;
        public static string AllowedUsernameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static string AllowedPasswordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}|;:',.<>?/`~\"\\";
        public static int NewAccountStartingCoins = 0;

    }


}