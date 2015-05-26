using System;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using System.Timers;
using LeagueSharp;
using LeagueSharp.Common;

namespace RageControl
{
    internal class Program
    {
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

        static string[] _badWords = { "ass","fak", "bastard", "braindead", "l2p", "fk", "cunt", "dick", "fuck", "kurwa", "shit", "suck", "mom", "kid", "noob", "retard", "report", "feeder", "bronzie", "nab", "tard", "idiot", "moron", "mother" };

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            (_main = new Menu("RageControl", "RageControl", true)).AddToMainMenu();
            Notifications.AddNotification("Rage Control Loaded!", 1000);
            Game.OnInput += Game_OnInput;
        }

        static void Game_OnInput(GameInputEventArgs args)
        {
            //AddNotification("Punished is " + _isPunished); hehe
            if (_isPunished)
            {
                args.Process = false;
                Notifications.AddNotification(new Notification("Pssst...This is for your own good", 1000, true).SetTextColor(Color.DarkRed).SetBoxColor(Color.AntiqueWhite));
            }

            foreach (var word in _badWords)
            {
                if (!args.Input.Contains(word))
                    continue;
                _curseWord = word;
                args.Process = false;
                _curseCount++;
                break;
            }
            if (args.Process == false)
            {
                Notifications.AddNotification(new Notification(CurseWarn + _curseWord, 2000, true).SetTextColor(Color.OrangeRed).SetBoxColor(Color.Gray));
                if (_curseCount >= 4 && _curseCount < 7)
                {
                    Notifications.AddNotification(new Notification(CurseWarnBig, 2000, true).SetTextColor(Color.Red).SetTextColor(Color.Gray));
                }
                else if (_curseCount >= 7 && _curseCount < 9)
                {
                    Notifications.AddNotification(new Notification(CurseWarnBIK, 200, true).SetTextColor(Color.DarkRed).SetBoxColor(Color.FromArgb(105, 105, 105)));
                }
                else if (_curseCount == 9)
                    Notifications.AddNotification(new Notification(CurseWarnFinal, 2000, true).SetTextColor(Color.FromArgb(255, 30, 30)));

                else if (_curseCount >= 10)
                {
                    AddNotification(CurseWarnPunish);
                    _isPunished = true;
                    var stfu = new Timer { Interval = 1000 * (_curseCount * 10), Enabled = true, AutoReset = false };
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
        public static void AddNotification(string text)
        {
            var notification = new Notification(text, 1000);
            Notifications.AddNotification(notification);
        }
        public static void AddNotification(Notification notification)
        {
            Notifications.AddNotification(notification);
        }
    }
}
