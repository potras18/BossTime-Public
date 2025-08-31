<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserPanel.aspx.cs" Inherits="BossTime.UserPanel" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="style/Main.css?1=1" />
    <link rel="icon" type="image/x-icon" href="/images/favicon.ico?1=1"/>
    <script type="text/javascript" src="Scripts/Register.js?1=13422"></script>
    <title>Priston Tale - Account Registration</title>
</head>
<body>
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel runat="server" ID="upMain">
        <ContentTemplate>
    <form id="form1" runat="server">
        <asp:Image runat="server" CssClass="ptLogo" ID="imgLogo" draggable="false" OnClick="imgLogo_Click" ImageUrl="~/images/logo_pt.png" />
        <div class="RegBox">
            <asp:Label runat="server" ID="lblWelcome"></asp:Label>
            <asp:LinkButton Text="Logout" runat="server" ID="lbLogout" OnClick="lbLogout_Click"  />
            <br />

            <div runat="server" id="dvPackages">

            </div>

        </div>

        <div class="UIShroud Hidden" id="divShroud">
            <div class="RegBox SuperFront">
                <h1 runat="server" id="hdStatus">Account Login</h1>
                <asp:Label ID="lblStatus" Text="" runat="server" Width="100%" />
                <br />
                <br />
                <button type="button" class="btnReg" onclick="HideParent(this.parentElement);">OK</button>

            </div>
        </div>
    </form>
            </ContentTemplate>
</asp:UpdatePanel>
</body>
</html>
