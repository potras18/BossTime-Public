<%@ Page Language="C#" AutoEventWireup="true" Title="Priston Tale - Boss Timer" CodeBehind="BossTimer.aspx.cs" Inherits="BossTime.BossTime" MaintainScrollPositionOnPostback="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Priston Tale - Boss Timer</title>
    <script src="Scripts/Main.js?1=13422" type="text/javascript"></script>
    <script src="Scripts/Synth.js?1=1" type="text/javascript"></script>
    <link rel="stylesheet" href="style/main.css?1=2022" />
    <link rel="icon" type="image/x-icon" href="/images/favicon.ico?1=1"/>
    </head>
<body oncontextmenu="return false">
  
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" ID="smMain"></asp:ScriptManager>
        <asp:UpdatePanel runat="server" ID="upMain"><ContentTemplate>
            
            <asp:Timer Enabled="true" Interval="1000" runat="server" ID="tmrMain" OnTick="tmrMain_Tick"></asp:Timer>
            <asp:HiddenField runat="server" ID="hfMins" Value="0" />
        <div class="sTime">
            <asp:Label runat="server" id="lblsTime" Text="Current Server Time - 00:00:00"></asp:Label><br />
            <asp:Label runat="server" id="lblbTime" Text="Current Boss Time - XX:00"></asp:Label><br />
            <asp:Label runat="server" id="lblApiTime" Visible="false" Text="Data Time - 00:00:00"></asp:Label><br />
            <label>Level Range Selection</label><br />
            <label>From</label><asp:TextBox CssClass="tbLSel" min="70" Text="70" max="120" onchange="SetMinMax(1,this.value)" runat="server" ID="lvlFrom" TextMode="Number"></asp:TextBox><br />
            <label>To</label><asp:TextBox CssClass="tbLSel" min="70" Text="120" max="120" onchange="SetMinMax(2,this.value)" runat="server" ID="lvlTo" TextMode="Number"></asp:TextBox>
            </div>
        <asp:ImageButton runat="server" CssClass="ptLogo" ID="imgLogo" draggable="false" OnClick="imgLogo_Click" ImageUrl="~/images/logo_pt.png" />
        <div class="ctls">
            Min Level:<asp:TextBox ID="tbBLow" runat="server" AutoCompleteType="Disabled" max="120" min="0" TextMode="Number">0</asp:TextBox>
&nbsp;Max Level:<asp:TextBox ID="tbBHigh" runat="server" AutoCompleteType="Disabled" TextMode="Number">120</asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
        </div>

        <asp:DataList SelectedItemStyle-BackColor="Green" ID="dlMain" runat="server" OnSelectedIndexChanged="dlMain_SelectedIndexChanged" CssClass="bossMain">
            <ItemStyle Wrap="False" />
            <ItemTemplate>
                <div class="bossR" runat="server" id="bossR" visible="<%# ((BossTime.TimeSlot)Container.DataItem).isVisible %>">
                    <table>
                        <tr>
                            <td class="TimeHeader">
                                <asp:Label ID="lblFuryHour" runat="server" CssClass="rowTime" Text='<%# "Fury:" + TimeSpan.FromHours((int)((BossTime.TimeSlot)Container.DataItem).Hour) %>'></asp:Label><br /><br />
                            
                                <asp:Label ID="lblHour" runat="server" CssClass="rowTime" Text='<%# "Boss:" + new TimeSpan((int)((BossTime.TimeSlot)Container.DataItem).Hour,(int)((BossTime.TimeSlot)Container.DataItem).Minute,0) %>'></asp:Label>
                            </td>
                            <td>
                                <asp:DataList ID="dlSub" runat="server" DataSource="<%# ((BossTime.TimeSlot)Container.DataItem).bosses %>" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                    <ItemTemplate>
                                        <div class="bossCard" runat="server" id="bCardMain" visible="<%# ((BossTime.Boss)Container.DataItem).isVisible %>">
                                            <table class="tblBoss">
                                                <tr>
                                                    <td class="bossHead">
                                                        <asp:Label ID="lblName" runat="server" Text="<%# ((BossTime.Boss)Container.DataItem).Name.ToUpper() %>"></asp:Label>
                                                        <hr class="hrc" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div class="bossP">
                                                        <asp:Image CssClass="bImg" draggable="false" ID="imgBoss" runat="server" ImageUrl="<%# ((BossTime.Boss)Container.DataItem).ImgUrl %>" onerror="this.onerror=null;this.src='images/placeholder.png'" />
                                                            </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="lblMap" runat="server" Text="<%# ((BossTime.Boss)Container.DataItem).Map %>"></asp:Label>
                                                        <br />
                                                        <asp:Label ID="lblMLevel" runat="server" Text="<%# '(' + ((BossTime.Boss)Container.DataItem).MapLevel.ToString().Trim() + ')' %>"></asp:Label>
                                                        </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                            </td>
                        </tr>
                    </table>
                    </div>
            </ItemTemplate>

<SelectedItemStyle BackColor="Green"></SelectedItemStyle>
        </asp:DataList>
            <button type="button" style="display:none" id="btnFury" onclick="FuryClick()"></button>
            <button type="button" style="display:none" id="btnBoss" onclick="BossClick()"></button>
            <button type="button" style="display:none" id="btnScrl" onclick="ScrlClick()"></button>


            <div id="pnlAudio" class="bossCard wide front" style="display:none;">
                <table style="width:100%;vertical-align:top;margin:0px;margin-top:-20px;">
                    <tr>
                        <td>&nbsp;</td>
                        <td><h3>Enable Audio Notifications</h3></td>
                        <td>
                            <input id="btnClosePop" class="btnClose" onclick="HideSpeakerPanel();" type="button" value="X" />
                        </td>
                    </tr>
                </table>
                To hear audio notfications for boss spawns, please enable speaker permissions for this website in your browser settings.</div>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="tmrMain" EventName="Tick" />
            </Triggers>
        </asp:UpdatePanel>
    </form>
</body>
</html>
