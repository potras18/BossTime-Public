<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="BossTime.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="style/Main.css?1=1" />
    <link rel="icon" type="image/x-icon" href="/images/favicon.ico?1=1"/>
    <script type="text/javascript" src="Scripts/Register.js?1=13422"></script>
    <title>Priston Tale - Account Registration</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Image runat="server" CssClass="ptLogo" ID="imgLogo" draggable="false" ImageUrl="~/images/logo_pt.png" />
        <div class="RegBox">
            <h1>Register Account</h1><br />

            <label>Username</label><br />
            <asp:TextBox  ID="tbUsername" runat="server" CssClass="tbReg" MaxLength="20" AutoCompleteType="Disabled"></asp:TextBox><br /><br />
            <label>Password</label><br />
            <asp:TextBox  ID="tbPassword" runat="server" CssClass="tbReg" TextMode="Password" MaxLength="20" AutoCompleteType="Disabled"></asp:TextBox><br /><br />
            <label>Confirm Password</label><br />
            <asp:TextBox  ID="tbConfPass" runat="server" CssClass="tbReg" TextMode="Password" MaxLength="20" AutoCompleteType="Disabled"></asp:TextBox><br /><br />
            <label>Email</label><br />
            <asp:TextBox  ID="tbEmail" runat="server" CssClass="tbReg" MaxLength="50" AutoCompleteType="Disabled"></asp:TextBox><br /><br />
            <asp:Button ID="btnRegister" runat="server" CssClass="btnReg" Text="Register" OnClick="btnRegister_Click" /><br />
            <br />
            <asp:LinkButton ForeColor="White" runat="server" Text="Login to Existing Account" ID="lbLoginAccount" OnClick="lbLoginAccount_Click"></asp:LinkButton>
        </div>

        <div class="UIShroud Hidden" id="divShroud">
            <div class="RegBox SuperFront">
                <h1 runat="server" id="hdStatus">Account Registration</h1>
                <asp:Label ID="lblStatus" Text="" runat="server" Width="100%" />
                <br />
                <br />
                <button type="button" class="btnReg" onclick="HideParent(this.parentElement);">OK</button>

            </div>
        </div>
    </form>
</body>
</html>
