using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public partial class BossTime : System.Web.UI.Page
    {
        int bosstime = 0;
        int utcoffset = 0;
        List<Boss> StartBoss = new List<Boss>();
        List<Boss> Bosses = new List<Boss>();
        ServerData apidata = new ServerData();
        
        public TimeSlot[] bossTimes = new TimeSlot[24];
        int minutes = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //utcoffset = GetOffset();


            APIHandler apih = new APIHandler(Server.MapPath("Resources/api.json"));
            apidata = apih.ApiData();
         
            DoLevelSelection();

            minutes = apih.GetBossMinute();
            hfMins.Value = minutes.ToString();
            DateTime dtutc = DateTime.UtcNow.AddHours(utcoffset);
            TimeSpan dtnext;
            TimeSpan dtuntil = TimeSpan.Zero;
            if(dtutc.Minute > minutes && dtutc.Hour < 23)
            {
                dtnext = new TimeSpan(dtutc.Hour + 1, minutes, 0).Subtract(dtutc.TimeOfDay);

            }else if(dtutc.Minute < minutes && dtutc.Hour < 23)
            {
                dtnext = new TimeSpan(dtutc.Hour, minutes, 0).Subtract(dtutc.TimeOfDay);
            }
            else
            {
                dtnext = TimeSpan.Zero;
            }

            string status = "<br>(" + ((int)Math.Ceiling(dtnext.TotalMinutes)).ToString() + " Min Until Next)";
            if (dtnext == TimeSpan.Zero)
            {
                status = "";
            }

            lblsTime.Text = $"Current Server Time - {dtutc.ToString("HH:mm:ss")}";
            
            lblbTime.Text = $"Current Boss Time - XX:{minutes.ToString().PadLeft(2, '0')}{status}";
            lblApiTime.Text = $"Last Request - {apidata.RequestTime.ToString("HH:mm:ss")}";
            PopulateBosses(LowBoss,HighBoss);

            ScriptManager.RegisterStartupScript(upMain, typeof(string), "JumpToTime", $"ShowCurrent({utcoffset});", true);
            ScriptManager.RegisterStartupScript(upMain, typeof(string), "StartServerTime", $"StartTimer({utcoffset});", true);
            try
            {

               
                string boss = Request.QueryString["boss"];
                if (!string.IsNullOrEmpty(boss))
                {
                    GetBossTimeAPI(boss);
                }

                string bosst = Request.QueryString["until"];
                if (!string.IsNullOrEmpty(bosst))
                {
                    GetBossTimeAPI(bosst, 1);
                }

                string bossexact = Request.QueryString["bosstime"];
                if (!string.IsNullOrEmpty(bossexact))
                {
                    GetBossTimeAPI(bossexact, 2);
                }

                string bossnow = Request.QueryString["bosshour"];
                if (!string.IsNullOrEmpty(bossnow))
                {
                    if (bossnow == "1")
                    {
                        CheckHour();
                    }
                    else if (bossnow == "2")
                    {
                        CheckHour(false);
                    }else if (bossnow == "3")
                    {

                        CheckHour(true, true);
                    
                    }else if(bossnow == "4")
                    {
                        CheckHour(false, true);
                    }else if(bossnow == "5")
                    {
                        Response.Write(minutes.ToString());
                        Response.End();
                    }
                }

                string arena = Request.QueryString["arena"];
                if (!string.IsNullOrEmpty(arena))
                {
                    string strArena = GetArena().Replace("...&", " & ").Replace("...", ", ");
                    Response.Write(strArena);
                    Response.End();
                }

                string normal = Request.QueryString["normal"];
                if (!string.IsNullOrEmpty(normal))
                {
                    string strArena = GetNormal().Replace("...&"," & ").Replace("...",", ");
                    Response.Write(strArena);
                    Response.End();
                }


                string prompt = Request.QueryString["prompt"];
                if (!string.IsNullOrEmpty(prompt))
                {
                    if (prompt == "refresh")
                    {
                        apidata = apih.ApiData(true);
                        
                    }else if (prompt == "reloadboss")
                    {
                        string path = Server.MapPath("Resources/bosses.json");
                        GenerateDefault(path);
                    }
                }

                

            }catch(Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }
        }

        int LowBoss = 0;
        int HighBoss = 120;



        private void DoLevelSelection()
        {
            try
            {

                int low = 70;
                if (Request.Cookies.AllKeys.Contains("low"))
                {
                    string cooklow = Request.Cookies["low"].Value;
                    if (!string.IsNullOrEmpty(cooklow))
                    {
                        bool lowparse = int.TryParse(cooklow, out low);
                    }
                    else
                    {
                        Response.Cookies.Set(new HttpCookie("low", "70") { Expires = DateTime.Now.AddDays(100) });
                    }
                }
                else
                {
                    Response.Cookies.Add(new HttpCookie("low", "70") { Expires = DateTime.Now.AddDays(100) });
                }
                int high = 120;
                if (Request.Cookies.AllKeys.Contains("high"))
                {
                    string cookhigh = Request.Cookies["high"].Value;
                    if (!string.IsNullOrEmpty(cookhigh))
                    {
                        bool highparse = int.TryParse(Request.Cookies["high"].Value, out high);
                    }
                    else
                    {
                        Response.Cookies.Set(new HttpCookie("high", "120") { Expires = DateTime.Now.AddDays(100) });
                    }
                }
                else
                {
                    Response.Cookies.Add(new HttpCookie("high", "120") { Expires = DateTime.Now.AddDays(100) });
                }

                int finallow = low;
                int finalhigh = high;

                if(low > high)
                {
                    finallow = high;
                    finalhigh = low;
                }

                LowBoss = finallow;
                HighBoss = finalhigh;

                ScriptManager.RegisterStartupScript(smMain, typeof(string), "GetMinMax", "GetMinMax();", true);

            }
            catch (Exception ex)
            {
               
            }
        }


        private void GetBossTimeAPI(string boss, int realtime = 0)
        {
            List<TimeSlot> timeslots = bossTimes.ToList().Where((x) => new TimeSpan(x.Hour, x.Minute, 0) > DateTime.UtcNow.AddHours(utcoffset).TimeOfDay).ToList();
            timeslots.Sort((x, y) => x.Hour - y.Hour);
           
            string[] bossnames = new string[] { };
          
            foreach (TimeSlot ts in timeslots)
            {
               
                
                foreach (Boss b in ts.bosses)
                {
                    
                   
              
                    if (!string.IsNullOrEmpty(b.Name))
                    {
                        if (b.Name.ToLower().Replace(" ", "") == boss.ToLower())
                        {

                            TimeSpan utcts = new TimeSpan(ts.Hour, ts.Minute, 0);
                            if ((boss.ToLower().Contains("fury") || boss.ToLower().Contains("deius")) && DateTime.UtcNow.AddHours(utcoffset).TimeOfDay < new TimeSpan(ts.Hour, 0, 0))
                            {
                                utcts = new TimeSpan(ts.Hour, 0, 0);
                            }
                            else if ((boss.ToLower().Contains("fury") || boss.ToLower().Contains("deius")) && DateTime.UtcNow.AddHours(utcoffset).TimeOfDay >= new TimeSpan(ts.Hour, 0, 0))
                            {
                                break;
                            }


                            TimeSpan utcnow = DateTime.UtcNow.AddHours(utcoffset).TimeOfDay;
                            TimeSpan diff = utcts.Subtract(utcnow);
                            string resp = $"{(int)diff.TotalSeconds}";
                            if (realtime == 1)
                            {
                                resp = diff.ToString(@"hh\:mm\:ss");
                            }
                            else if (realtime == 2)
                            {
                                resp = utcts.ToString(@"hh\:mm\:ss");
                            }
                            Response.Write(resp);
                            Response.End();
                        }
                    }
                }

                

            }

            Response.Write($"-1");
            Response.End();
        }

        private int GetOffset()
        {
            string filepath = Server.MapPath("Resources/utcoffset.dat");
            if (File.Exists(filepath))
            {
                string offset = File.ReadAllText(filepath);
                int offsetutc = 0;
                bool parsed = int.TryParse(offset, out offsetutc);

                return offsetutc;
            }
            else
            {
                return 0;
            }
        }

        private string CheckHour(bool special = true,bool extime = false)
        {
                
                string bosslistresp = "";
                TimeSlot ts = bossTimes[DateTime.UtcNow.AddHours(utcoffset).Hour];
            if (ts != null)
            {
                //Debug.WriteLine(ts.Minute);
                if (ts.Minute > DateTime.UtcNow.AddHours(utcoffset).Minute)
                {
                    bosslistresp = GetBossList(ts.Hour,special,extime);
                    //Debug.WriteLine("MINUTES LESS");
                }
                else
                {
                    //Debug.WriteLine("MINUTES MORE");
                    if (ts.Hour < 23)
                    {
                        //Debug.WriteLine("HOUR LESS");
                        bosslistresp = GetBossList(ts.Hour + 1,special,extime);
                    }
                    else
                    {
                        //Debug.WriteLine("HOUR MORE");
                        bosslistresp = GetBossList(0, special, extime);
                    }
                }
                Response.Write (bosslistresp);
                Response.End();
            }

            return "";
                
        }



        private string GetBossList(int hour, bool specialb = true, bool extime = false, bool specialonly = false)
        {
            TimeSlot ts = bossTimes[hour];
            List<string> bosses = new List<string>();
            List<string> special = new List<string>();
            if(ts != null)
            {
                foreach (Boss b in ts.bosses) {
                    if (b.Name.ToLower().Contains("fury") || b.Name.ToLower().Contains("deius"))
                    {
                        if (specialb)
                        {
                            if (ts.Minute < DateTime.UtcNow.AddHours(utcoffset).Minute || ts.Hour > DateTime.UtcNow.AddHours(utcoffset).Hour)
                            {
                                special.Add(b.Name);
                            }
                        }
                    }
                    else
                    {
                        bosses.Add(b.Name);
                        //Debug.WriteLine("Adding Boss " + b.Name);
                    }
                }
            }
            string specialheader = "";
            string timeheader = "";
            if (!extime)
            {
                specialheader += $"{ts.Hour.ToString().PadLeft(2, '0')}:00 - ";
                timeheader = $"{ts.Hour.ToString().PadLeft(2, '0')}:{ts.Minute.ToString().PadLeft(2, '0')} - ";
            }
          
            
            string bossstring = string.Join(", ", bosses);
            string specialstring = string.Join(", ", special);

            string bossret = timeheader + bossstring;
            string ret = "";
            if(special.Count > 0)
            {
                string spacer = "";
                string separator = " - ";
                if (extime)
                {
                    spacer = " ";
                    separator = ", ";
                }
                ret = specialheader + spacer + specialstring + separator;
            }

            ret += bossret;
            

            return ret;
        }


        private void GenerateDefault(string path)
        {
            //StartBoss = new List<Boss>() {
            //new Boss() { hourStart = 0, Map = "Galubia Valley", MapLevel = 89, Name = "Chaos Cara Queen", phonic = "CHAOS CARA QUEEN", respawnTimeout = 2 },
            //new Boss() { hourStart = 1, Map = "Frozen Sanctuary", MapLevel = 92, Name = "Valento", phonic = "VAL EN TOW", respawnTimeout = 2 },
            //new Boss() { hourStart = 0, Map = "Kelvezu's Cave", MapLevel = 92, Name = "Kelvezu",phonic = "KEL VE ZOO", respawnTimeout = 3 },
            //new Boss() { hourStart = 1, Map = "Land of Chaos", MapLevel = 95, Name = "Bloody Prince",phonic="Bloody Prince", respawnTimeout = 3 },
            //new Boss() { hourStart = 2, Map = "Lost Temple", MapLevel = 98, Name = "Mokova",phonic="MOH KO VAH", respawnTimeout = 3 },
            //new Boss() { hourStart = 0, Map = "Endless Tower 1F", MapLevel = 100, Name = "Gorgoniac",phonic = "Gorgoniac", respawnTimeout = 3 },
            //new Boss() { hourStart = 0, Map = "Endless Tower 2F", MapLevel = 102, Name = "Devil Shy", phonic="Devil Shy", respawnTimeout = 4 },
            //new Boss() { hourStart = 1, Map = "Ice Mine 1", MapLevel = 105, Name = "Tulla",phonic="TOO LAH", respawnTimeout = 4 },
            //new Boss() { hourStart = 3, Map = "Secret Lab", MapLevel = 108, Name = "Draxos",phonic="Draxos", respawnTimeout = 4 },
            //new Boss() { hourStart = 0, Map = "Ancient Weapon", MapLevel = 110, Name = "Greedy",phonic="Greedy", respawnTimeout = 6 },
            //new Boss() { hourStart = 1, Map = "Abyss of The Sea", MapLevel = 113, Name = "Yagditha",phonic="Yag di tha", respawnTimeout = 6 },
            //new Boss() { hourStart = 4, Map = "Forge of The Ancients", MapLevel = 115, Name = "Golem",phonic="Go Lem", respawnTimeout = 12 },
            //new Boss() { hourStart = 10, Map = "Forge of The Ancients (H)", MapLevel = 115, Name = "Golem (H)",phonic="Go Lem Hard", respawnTimeout = 12 },
            //new Boss() { hourStart = 0, Map = "Aragonian's Lair", MapLevel = 118, Name = "Aragonian",phonic="Argonian", respawnTimeout = 6 },
            //new Boss() {hourStart = 2, Map = "Heart of Fire", MapLevel = 113, Name = "Ignis",phonic="Ig nis",respawnTimeout = 6 },
            //new Boss() {hourStart = 3, Map = "Fury Arena", MapLevel = 70, Name = "Fury (Rage)",minStart=0,Arena = true,respawnTimeout = 3 },
            //new Boss() {hourStart = 1, Map = "Fury Arena", MapLevel = 89, Name = "Fury",minStart=0,Arena = true,respawnTimeout = 3 },
            //new Boss() {hourStart = 2, Map = "Fury Arena", MapLevel = 104, Name = "Fury (Wrath)",minStart=0,Arena = true,respawnTimeout = 3 },
            //new Boss() {hourStart = 2, Map = "Deius Arena", MapLevel = 120, Name = "Deius",phonic="Day us",minStart=0,Arena = true,respawnTimeout = 2 },
            //new Boss() {hourStart = 0, Map = "Ricarten Vault 3", MapLevel = 100, Name = "Skillmaster (PVP)",phonic="Skillmaster Eadric",respawnTimeout = 3 }

            //};
            //try
            //{
            //    string js = JsonConvert.SerializeObject(StartBoss,Formatting.Indented);
            //    File.WriteAllText(path, js);
            //}
            //catch (Exception)
            //{
            //    //Debug.WriteLine("Failed to write default boss chunk");
            //}
        }


        public void PopulateBosses(int min=0, int max=120)
        {


      

           

            GenerateList(min, max);
            

            dlMain.DataSource = bossTimes;
            DataListItem dli = new DataListItem(1, ListItemType.Item);
       
            dlMain.DataBind();



        }

        private string GetArena()
        {

            string ret = "";
            DateTime dtutc = DateTime.UtcNow.AddHours(utcoffset);
            List<Boss> spboss = new List<Boss>();
            
            if(dtutc.Minute >= 0 && dtutc.Hour < 23)
            {
                spboss.AddRange(bossTimes[dtutc.Hour + 1].bosses.FindAll((x) => x.Arena==true));
            }
            else{
                spboss.AddRange(bossTimes[0].bosses.FindAll((x)=> x.Arena==true));

            }

            List<string> BossNames = new List<string>();
            int index = 0;
            foreach (Boss b in spboss)
            {
                if (index < spboss.Count - 1)
                {
                    if (index > 0)
                    {
                        BossNames.Add("...");
                    }
                    BossNames.Add(b.Name);
                }
                else
                {
                    if (spboss.Count > 1)
                    {
                        BossNames.Add("...&" + b.Name);
                    }
                    else
                    {
                        BossNames.Add(b.Name);
                    }
                }
                index++;
            }

            ret = string.Join("",BossNames);
           

            return ret;
        }

        private string GetNormal()
        {

            string ret = "";
            DateTime dtutc = DateTime.UtcNow.AddHours(utcoffset);
            List<Boss> spboss = new List<Boss>();

            if (dtutc.Minute >= minutes && dtutc.Hour < 23)
            {
                spboss.AddRange(bossTimes[dtutc.Hour + 1].bosses.FindAll((x) => x.Arena==false && x.MapLevel >= LowBoss && x.MapLevel <= HighBoss));
            }
            else if(dtutc.Minute < minutes && dtutc.Hour < 23) {
            
                spboss.AddRange(bossTimes[dtutc.Hour].bosses.FindAll((x) => x.Arena==false && x.MapLevel >= LowBoss && x.MapLevel <= HighBoss));

            }else{

                spboss.AddRange(bossTimes[0].bosses.FindAll((x)=> x.Arena==false && x.MapLevel >= LowBoss && x.MapLevel <= HighBoss));

            }

            List<string> BossNames = new List<string>();
            int index = 0;
            foreach (Boss b in spboss)
            {
                if (index < spboss.Count - 1) {
                    if(index > 0)
                    {
                        BossNames.Add("...");
                    }
                    BossNames.Add(b.Name);
                }
                else
                {
                    if (spboss.Count > 1)
                    {
                        BossNames.Add("...&" + b.Name);
                    }
                    else
                    {
                        BossNames.Add(b.Name);
                    }
                }
                index++;
            }

           
            ret = string.Join("", BossNames);
            Debug.WriteLine(ret);




            return ret;
        }

        private void GenerateList(int min=0, int max=120)
        {
            
            int lowest = 120, highest = 0;
            foreach (Map b in apidata.Maps)
            {
                int bcount = 0;
                foreach (int i in b.Spawns)
                {
                    bool bvis = true;

                    if(b.MapLevel < min || b.MapLevel > max)
                    {
                        bvis = false;
                    }
                    else
                    {
                        bcount++;
                    }

                    if(b.MapLevel < lowest)
                    {
                        lowest = b.MapLevel;
                    }

                    if (b.MapLevel > highest)
                    {
                        highest = b.MapLevel;
                    }

                    if(DateTime.UtcNow.AddHours(utcoffset).Hour == i  && b.Arena && DateTime.UtcNow.AddHours(utcoffset).Minute > 0)
                    {
                        bvis = false;
                    }

                    if(DateTime.UtcNow.AddHours(utcoffset).Hour == i && DateTime.UtcNow.AddHours(utcoffset).Minute > minutes)
                    {
                        bvis = false;
                    }


                    Boss nbos = new Boss() { Name = b.BossName, Map = b.MapName, MapLevel = b.MapLevel, hourStart = i,minStart=minutes, ImgUrl = $"images/{b.BossName.Replace(" ","")}.png", isVisible=bvis,Arena=b.Arena };
                   
                    if (bossTimes[i] == null)
                    {
                                             
                        bossTimes[i] = new TimeSlot() { Hour = i,Minute=minutes };
                    }

                    if (bvis && DateTime.UtcNow.AddHours(utcoffset).Hour <= i)
                    {
                        bossTimes[i].isVisible = true;
                    }

                    
                        bossTimes[i].bosses.Add(nbos);
                        bossTimes[i].bosses.Sort((x, y) => x.MapLevel.CompareTo(y.MapLevel));
                    

                }
            }
            
            tbBHigh.Attributes.Add("max", highest.ToString());
            tbBHigh.Attributes.Add("min", lowest.ToString());
            tbBLow.Attributes.Add("max", highest.ToString());
            tbBLow.Attributes.Add("min", lowest.ToString());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            GenerateList(Convert.ToInt32(tbBHigh.Text), Convert.ToInt32(tbBHigh.Text));
        }

        protected void dlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label lbl =  dlMain.SelectedItem.FindControl("lblHour") as Label;
            lbl.Focus();
        }

        protected void imgLogo_Click(object sender, ImageClickEventArgs e)
        {
            
        }
        static int GetInterval()
        {
            DateTime now = DateTime.Now;
            return ((60 - now.Second) * 1000 - now.Millisecond);
        }
        protected void tmrMain_Tick(object sender, EventArgs e)
        {
            tmrMain.Interval = GetInterval();
            TimeSpan ts = DateTime.UtcNow.AddHours(utcoffset).TimeOfDay;

            if(ts.Minutes == 55) 
            {
                //playAudio(\"resources/fury_5.mp3\");
                string arenaboss = GetArena();
                string arenastring = $"{arenaboss.Replace("(","").Replace(")","")} will spawn in 5 Minutes";
                ScriptManager.RegisterStartupScript(upMain, typeof(string), "PlayAudio", $"var speechText='{arenastring}';SynthSpeak();", true);
            }

            if(ts.Minutes == minutes - 5)
            {
                //playAudio(\"resources/boss_5.mp3\");
                string normalboss = GetNormal();
                string normalstring = $"{normalboss.Replace("(", "").Replace(")", "")} will spawn in 5 Minutes";
                ScriptManager.RegisterStartupScript(upMain, typeof(string), "PlayAudio", $"var speechText='{normalstring}';SynthSpeak();", true);
            }

        }
    }

    public class TimeSlot
    {
        public int Hour { get; set; }

        public int Minute { get; set; }

        public List<Boss> bosses = new List<Boss>();

        public bool isVisible = false;
    }

    public class Boss
    {
        public string ImgUrl { get; set; }

        public string Name { get; set; }

        public int hourStart { get; set; }

        public int minStart { get; set; } = 1;

        public bool Arena { get; set; } = false;

        public int respawnTimeout { get; set; }

        public string Map { get; set; }

        public int MapLevel { get; set; }

        public bool isVisible { get; set; } = true;

        public string phonic { get; set; } = "";

        public override string ToString()
        {
            return $"{Name}";
        }

    }
}