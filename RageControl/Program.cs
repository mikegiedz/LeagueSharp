using System;
using System.Collections.Generic;
using System.IO;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font; //will be used soon^tm
using System.Timers;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Sandbox;


namespace RageControl
{
    internal class Program
    {
        #region VarDeclarations
        static Menu _main;
        private static DateTime _startTime;
        private static int _curseCount;
        private static string _curseWord;
        private const string CurseWarn = "Flame Warning! You said "; // 2000
        private const string CurseWarnBig = "Flaming won't fix your gameplay... Say smtg usefull instead!"; // 2000;
        private const string CurseWarnBIK = "This is really bad... pls stahp :(";
        private const string CurseWarnFinal = "This is your final warning!"; // 2000
        private const string CurseWarnPunish = "Time for you to STFU for a while don't you think?"; // 2000
        private static bool _isPunished;
        #endregion
        static List <string> _badWords = new List<string> { "ass",  "cancer", "fak", "bastard", "braindead", "l2p", "fk", "cunt", "dick", "fuck", "kurwa", "shit", "suck", "mom", "kid", "noob", "retard", "report", "feeder", "bronzie", "nab", "tard", "idiot", "moron", "mother" };
        static List<string>_whiteList = new List<string> { "cass", "afk", "faker", "Faker" };

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            (_main = new Menu("RageControl", "RageControl", true)).AddToMainMenu();
          //  _main.AddItem(new MenuItem("Disable your chat", "playerCD").SetValue(false));
            Notifications.AddNotification("Rage Control Loaded!", 1000);
            Notifications.AddNotification("Reading files....", 1000);
            ReadFiles(SandboxConfig.DataDirectory);
            Game.OnInput += Game_OnInput;
        }

        private static void ReadFiles(string path)
        {
            string line;
            try
            {
                var file = new StreamReader(path + "blackListRC.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("#"))
                        continue;
                    _badWords.Add(line);
                }
                line = string.Empty;
                Notifications.AddNotification(new Notification("BlackList loaded",1000).SetBoxColor(Color.WhiteSmoke).SetTextColor(Color.Green));
            }
            catch (Exception e)
            {
                Notifications.AddNotification(new Notification("Error with blacklist: " + e.Message, 2000, true).SetBoxColor(Color.Black).SetTextColor(Color.Red));
            }
            try
            {
                var file = new StreamReader(path + "whiteListRC.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("#"))
                        continue;
                    _whiteList.Add(line);
                }
                line = string.Empty;
                Notifications.AddNotification(new Notification("WhiteList loaded",1000).SetBoxColor(Color.WhiteSmoke).SetTextColor(Color.Green));
            }
            catch (Exception e)
            {
                Notifications.AddNotification(new Notification("Error with whitelist" + e.Message).SetBoxColor(Color.Black).SetTextColor(Color.Red));
            }
        }


        static void Game_OnInput(GameInputEventArgs args)
        {
            if (_isPunished)
            {
                args.Process = false;
                Notifications.AddNotification(new Notification("Pssst...This is for your own good", 1000, true).SetTextColor(Color.DarkRed).SetBoxColor(Color.AntiqueWhite));
                return;
            }

            foreach (var word in _badWords)
            {
                if (!args.Input.Contains(word))
                    continue;
                    _curseWord = word;
                    args.Process = false;
                    _curseCount++;
            }
            foreach (var white in _whiteList)
            {
                if (!args.Input.Contains(white))
                    continue;
                args.Process = true;
                break;
            }
            if (args.Process == false)
            {
                Notifications.AddNotification(
                    new Notification(CurseWarn + _curseWord, 2000, true).SetTextColor(Color.OrangeRed)
                        .SetBoxColor(Color.Gray));
                if (_curseCount >= 4 && _curseCount < 7)
                {
                    Notifications.AddNotification(
                        new Notification(CurseWarnBig, 2000, true).SetTextColor(Color.Red).SetTextColor(Color.Gray));
                }
                else if (_curseCount >= 7 && _curseCount < 9)
                {
                    Notifications.AddNotification(
                        new Notification(CurseWarnBIK, 200, true).SetTextColor(Color.DarkRed)
                            .SetBoxColor(Color.FromArgb(105, 105, 105)));
                }
                else if (_curseCount == 9)
                    Notifications.AddNotification(
                        new Notification(CurseWarnFinal, 2000, true).SetTextColor(Color.FromArgb(255, 30, 30)));

                else if (_curseCount >= 10)
                {
                    Notifications.AddNotification(new Notification (CurseWarnPunish).SetBoxColor(Color.Black).SetTextColor(Color.Red));
                    _isPunished = true;
                    var stfu = new Timer {Interval = 1000*(_curseCount*10), Enabled = true, AutoReset = false};
                    _startTime = DateTime.Now;
                    stfu.Start();
                    stfu.Elapsed += stfu_Elapsed;
                }
            }
        }
        private static string TimeLeft(DateTime start)
        {
            var timeLeft = start - DateTime.Now;
            return timeLeft.ToString();
        }

        private static void stfu_Elapsed(object sender, ElapsedEventArgs e)
        {
            _isPunished = false;
        }
      //  public static void AddNotification(Notification notification)
      //  {
      //      Notifications.AddNotification(notification);
      //  }
        #region MenuItemProblem
        //static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        //{
        //    if (_main.Item("playerCD").GetValue<bool>())
        //        AddNotification(new Notification("Your chat dissabled").SetTextColor(Color.Red).SetBoxColor(Color.Black));
        //    else if (_main.Item("playerCD").GetValue<bool>())
        //        AddNotification(new Notification("Your chat enabled").SetTextColor(Color.Green).SetBoxColor(Color.Gray));
        //    else
        //        AddNotification(new Notification("An error detected! Tell Foxy ASAP! ERROR CODE: -12 ").SetTextColor(Color.White).SetBoxColor(Color.Blue));
        //}
        #endregion

    }
}
