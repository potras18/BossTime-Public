<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomError.aspx.cs" Inherits="BossTime.CustomError" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="style/Main.css?1=1" />
    <link rel="icon" type="image/x-icon" href="/images/favicon.ico?1=1"/>
    <script type="text/javascript" src="Scripts/Register.js?1=13422"></script>
    <title>Priston Tale - Account Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Image runat="server" CssClass="ptLogo" ID="imgLogo" draggable="false" OnClick="imgLogo_Click" ImageUrl="~/images/logo_pt.png" />
        <div class="RegBox">
            <asp:Button runat="server" ID="btnReturn" CssClass="btnReg" Text="Previous Page" OnClick="btnReturn_Click" /><br /><br />
            <asp:Button runat="server" ID="btnHome" CssClass="btnReg" Text="Home Page" OnClick="btnHome_Click" />
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
</body>
</html>
