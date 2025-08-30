using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BossTime
{

    // System Variables
    // These variables are used throughout the system and can be adjusted here.
    // For example, you can change the username, password and server which the system will try to connect to.
    // Note: Changing these variables will affect the entire system.
    // Be sure to update any related configurations or documentation accordingly.
    public static class DBCredentials
    {

        public static string dbID = "YOUR SQL USER ID";
        public static string dbPass = "YOUR SQL PASSWORD";
        public static string server = "SERVER\\SQLEXPRESS";

    }

    // Registration Variables
    // These variables are used during the registration process and can be adjusted here.
    // For example, you can change the minimum and maximum lengths for usernames, passwords, and emails.
    // You can also change the allowed characters for usernames and passwords.
    // Note: Changing these variables will not affect existing accounts, only new registrations.
    // Be sure to keep the allowed characters in sync with any client-side validation you may have.
    // Also, be sure to keep the minimum and maximum lengths in sync with any client-side validation you may have.
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

    }

}