using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font; 
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
        private static bool _isPermaDissabled=false;
        private static bool _isDissabled=false;
        private static bool _bye = false;
        #endregion
        static List <string> _badWords = new List<string> { "kys", "scrub", "pleb", "smd", "anus", "arse", "banger", "clown", "cock", "cracker", "nig", "nigger", "ass", "jacker", "fap", "jerk off", "jerking off", "munch", "pirate", "hole", "wad", "axwound", "fck", "cancer", "fak", "fcuk", "bastard", "braindead", "l2p", "fk", "cunt", "dick", "fuck", "cancer", "trash", "garbage", "asshole", "kill yourself", "kill urself", "hang yourself", "hang urself", "hope u die", "homo", "fucking die", "beaner", "spick", "jew", "bitch", "blow job", "blowjob", "bollocks", "bj", "bollox", "bumble", "butt", "fucka", "fuk", "camel toe", "carpetmuncher", "carpet muncher", "chesticle", "chinc", "chink", "choad", "chode", "clit", "clusterfuck", "cock", "c0ck", "cok", "coochie", "coon", "cum", "dumpster", "slut", "cunnie", "tit", "tits", "damn", "dam", "dick", "beater", "face", "head", "slap", "dip", "dildo", "douche", "dumb", "dyke", "i hope", "faggit", "tard", "fat", "fellatio", "feltch", "felch", "bag", "fuckin", "fukin", "shoot yourself", "shot in the", "killed in the", "nut", "nutt", "fudgepacker", "wasted", "garbage", "gay", "god", "lord", "dam", "dammit", "damnit", "gooch", "gook", "gringo", "fob", "guido", "hj", "handjob", "hard on", "hardon", "heeb", "hell", "hoe", "ho", "honkey", "humping", "jack", "jagoff", "jerk", "jigaboo", "jizz", "jungle bunny", "junglebunny", "kike", "kooch", "kootch", "kraut", "kunt", "kyke", "lame", "lard", "lesbian", "lesbo", "lezzie", "fagget", "mick", "minge", "motha", "fucka", "fuckin", "muff", "munging", "negro", "nigaboo", "niga", "nigga", "niqqa", "ni99er", "ni99a", "nigger", "niglet", "nut sack", "nutsack", "nutsac", "nut sac", "paki", "panooch", "pecker", "penis", "puffer", "piss", "flaps", "pole", "pollock", "pollack", "poon", "poonani", "poonany", "poontang", "pootang", "porch monkey", "porchmonkey", "prick", "punanny", "punta", "pussy", "lick", "puto", "queer", "queef", "renob", "rimjob", "rim job", "ruski", "sand", "shlong", "scrote", "bag", "canned", "stain", "breath", "hole", "spitter", "shiz", "skank", "skeet", "skull", "slut", "bag", "smeg", "snatch", "spic", "spick", "splooge", "spook", "testicle", "twat", "semen", "seamen", "vaj", "vag", "vj", "wank", "wetback", "whore", "wop", "brain", "faggot", "fag", "slice your", "slice ur", "slit ur", "slice ur", "wrists", "bleed to death", "off yourself", "off urself", "stupid", "kurwa", "shit", "suck", "mom", "kid", "noob", "retard", "report", "feeder", "bronzie", "nab", "tard", "idiot", "moron", "your family", "ur family", "downs", "down syndrome", "downy", "syndrome", "autist", "autistic", "piece of" };
        static List<string> _whiteList = new List<string> { "cass", "afk", "faker", "Faker", "/msg", "/w", "dragon", "baron", "lag" };
        static List<Obj_AI_Hero> AllPlayers;
        static List<string> BannedPlayers = new List<string>();

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        static void Game_OnGameLoad(EventArgs args)
        {
            AllPlayers = ObjectManager.Get<Obj_AI_Hero>().ToList();
            if (AllPlayers.Contains(ObjectManager.Player))
                AllPlayers.Remove(ObjectManager.Player);
            (_main = new Menu("RageControl", "RageControl", true)).AddToMainMenu();
            
            var enableChatMenu = _main.AddSubMenu(new Menu("Disable your chat", "dyc"));
            enableChatMenu.AddItem(new MenuItem("disable", "Disable?").SetValue(false).DontSave());
            enableChatMenu.AddItem(new MenuItem("perma", "!PERMA! Disable").SetValue(false).DontSave());
            
            var bannedPlayers = _main.AddSubMenu(new Menu("Ban Player?", "bannedPlayer"));
            Notifications.AddNotification("Rage Control Loaded!", 1000);
            Notifications.AddNotification("Reading files....", 1000);
            ReadFiles(SandboxConfig.DataDirectory);
            _main.Item("disable").ValueChanged+=Program_ValueChanged;
            _main.Item("perma").ValueChanged+=PermaDissable;

            if (AllPlayers != null)
            {
                foreach (var player in AllPlayers)
                {
                    bannedPlayers.AddItem(new MenuItem(player.Name, "Ban " + player.Name + "?").SetValue(false));
                }
                foreach (var item in bannedPlayers.Items)
                {
                    item.ValueChanged += item_ValueChanged;
                }
            }
            bannedPlayers.AddItem(new MenuItem("allyban", "Ban all allies? :S").SetValue(false).DontSave());
            bannedPlayers.AddItem(new MenuItem("enemiesban", "Ban all enemies?").SetValue(false).DontSave());
            bannedPlayers.AddItem(new MenuItem("allban", "Ban all players? :S").SetValue(false).DontSave());
            Game.OnInput += Game_OnInput;
            Game.OnChat += Game_OnChat;
        }

        private static void PermaDissable(object sender, OnValueChangeEventArgs e)
        {
            if (sender == null)
                return;
            var Sender = sender as MenuItem;
            if (e.GetNewValue<bool>() && _bye==false)
            {
                _isPermaDissabled = true;
                _bye = true;
                Notifications.AddNotification(new Notification("Chat Perma Dissabled!", 3000).SetBorderColor(Color.Red).SetBoxColor(Color.Black).SetTextColor(Color.Red));
            }
            else if (_bye)
            {
                Notifications.AddNotification(new Notification("Chat Perma Dissabled!", 3000).SetBorderColor(Color.Red).SetBoxColor(Color.Black).SetTextColor(Color.Red));
                Notifications.AddNotification(new Notification("Pssssst remember? :) Try unloading the assembly", 4000).SetBorderColor(Color.Yellow).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
            }
            
        }
        //going to switch totaly to OnChat sooon....
        //nah.... It works so it's fine :P
        static void Game_OnChat(GameChatEventArgs args)
        {
            if (BannedPlayers.Contains(args.Sender.Name))
            {
                Notifications.AddNotification(new Notification(args.Sender.Name + "'s message is blocked", 1500).SetTextColor(Color.Orange).SetBoxColor(Color.Black));
                args.Process = false;
                return;
            }
        }
        static void item_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (sender == null)
            {
                Notifications.AddNotification(new Notification("sender is null! Report to Foxy...", 3000));
                return;
            }
            var newSender = sender as MenuItem;
            if (newSender == null)
                return;
            if (e.GetNewValue<bool>())
            {
                switch (newSender.Name)
                {
                    case "allyban":
                        foreach (var player in AllPlayers.Where(player => (player.IsAlly && BannedPlayers.Contains(player.Name))))
                        {
                            BannedPlayers.Add(player.Name);
                        }
                        Notifications.AddNotification(new Notification("Ally team chat banned!", 2000).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
                        break;
                    case "enemyban":
                        foreach (var player in AllPlayers.Where(player => (player.IsEnemy && !BannedPlayers.Contains(player.Name))))
                        {
                            BannedPlayers.Add(player.Name);
                        }
                        Notifications.AddNotification(new Notification("Enemy chat banned!", 2000).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
                        break;
                    case "allban":
                        foreach (var player in AllPlayers.Where(player => !BannedPlayers.Contains(player.Name)))
                        {
                            BannedPlayers.Add(player.Name);
                        }
                        Notifications.AddNotification(new Notification("All chat banned :S This is usually bad idea :(", 2000).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
                        break;
                }
                if (BannedPlayers.Contains(newSender.Name))
                {
                    Notifications.AddNotification(new Notification("He is banned already, you used teambans", 2000).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
                    return;
                }
                BannedPlayers.Add(newSender.Name);
                Notifications.AddNotification(new Notification(newSender.Name + " banned!", 2000).SetBoxColor(Color.Black).SetTextColor(Color.Orange));
            }
            else
            {
                switch (newSender.Name)
                {
                    case "allyban":
                        foreach (var player in AllPlayers.Where(player => (player.IsAlly && BannedPlayers.Contains(player.Name))))
                        {
                            BannedPlayers.Remove(player.Name);
                        }
                        Notifications.AddNotification(new Notification("Ally team chat unbaned! :)", 2000).SetBoxColor(Color.Black).SetTextColor(Color.GreenYellow));
                        break;
                    case "enemyban":
                        foreach (var player in AllPlayers.Where(player => (player.IsEnemy && BannedPlayers.Contains(player.Name))))
                        {
                            BannedPlayers.Remove(player.Name);
                        }
                        Notifications.AddNotification(new Notification("Enemy chat unbanned! :)", 2000).SetBoxColor(Color.Black).SetTextColor(Color.GreenYellow));
                        break;
                    case "allban":
                        foreach (var player in AllPlayers.Where(player => BannedPlayers.Contains(player.Name)))
                        {
                            BannedPlayers.Remove(player.Name);
                            Notifications.AddNotification(new Notification("All players unbanned :)", 2000).SetBoxColor(Color.Black).SetTextColor(Color.GreenYellow));
                        }
                        break;
                }
                if (!BannedPlayers.Contains(newSender.Name))
                {
                    Notifications.AddNotification(new Notification("He is not banned. You used teamunban probs :S", 2000).SetBoxColor(Color.Black).SetTextColor(Color.GreenYellow));
                    return;
                }
                BannedPlayers.Remove(newSender.Name);
                Notifications.AddNotification(new Notification(newSender.Name + " unbaned! :)", 2000).SetBoxColor(Color.Black).SetTextColor(Color.GreenYellow));
            }
        }
        private static void ReadFiles(string path)
        {
            string line;
            try
            {
                var file = new StreamReader(path + "blackListRC.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;
                    _badWords.Add(line);
                }
                line = string.Empty;
                Notifications.AddNotification(new Notification("BlackList loaded",1000).SetBoxColor(Color.WhiteSmoke).SetTextColor(Color.Green));
            }
            catch (Exception e)
            {
                Notifications.AddNotification(new Notification("Not using text files. You can add words to block (check thread for info)", 2000, true).SetBoxColor(Color.Black).SetTextColor(Color.Red));
            }
            try
            {
                var file = new StreamReader(path + "whiteListRC.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;
                    _whiteList.Add(line);
                }
                line = string.Empty;
                Notifications.AddNotification(new Notification("WhiteList loaded",1000).SetBoxColor(Color.WhiteSmoke).SetTextColor(Color.Green));
            }
            catch (Exception e)
            {
                Notifications.AddNotification(new Notification("You can add words to bypass blocking too! (check thread for info)", 1000).SetBoxColor(Color.Black).SetTextColor(Color.Red));
            }
        }


        static void Game_OnInput(GameInputEventArgs args)
        {
            if (_isDissabled==true || _isPermaDissabled==true)
            {
                foreach (var whiteword in _whiteList)
                {
                    if (args.Input.Contains(whiteword))
                    {
                        args.Process=true;
                        return;
                    }
                }
                Notifications.AddNotification("You disabled chat :S",1500).Border(true).SetBoxColor(Color.Black).SetTextColor(Color.Orange).SetBorderColor(Color.Red);
                args.Process = false;
                return;
            }
            if (_isPunished)
            {
                args.Process = false;
                Notifications.AddNotification(new Notification("Pssst...This is for your own good", 1500, true).SetTextColor(Color.DarkRed).SetBoxColor(Color.AntiqueWhite));
                Notifications.AddNotification(new Notification(TimeLeft(_startTime),2000).SetBoxColor(Color.Black).SetTextColor(Color.Red));
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
                    new Notification(CurseWarn + _curseWord, 2000, true).SetTextColor(Color.FromArgb(255,100,0)).SetBoxColor(Color.Gray));
                if (_curseCount >= 2) //reducing this BiK, at flamer's request :P
                {
                    Notifications.AddNotification(
                        new Notification(CurseWarnBig, 2000, true).SetTextColor(Color.Red).SetTextColor(Color.Gray));
                }
                else if (_curseCount >= 3 && _curseCount <= 4)
                {
                    Notifications.AddNotification(
                        new Notification(CurseWarnBIK, 200, true).SetTextColor(Color.Crimson)
                            .SetBoxColor(Color.FromArgb(105, 105, 105)));
                }
                else if (_curseCount == 5)
                    Notifications.AddNotification(
                        new Notification(CurseWarnFinal, 2000, true).SetTextColor(Color.FromArgb(255, 30, 30)));

                else if (_curseCount > 5)
                {
                    Notifications.AddNotification(new Notification(CurseWarnPunish, 1000 * (_curseCount * 15)).SetBoxColor(Color.Black).SetTextColor(Color.Red));
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
            var timeLeft = start.AddSeconds(1000 * (_curseCount * 10)) - DateTime.Now;
            return timeLeft.ToString();
        }

        private static void stfu_Elapsed(object sender, ElapsedEventArgs e)
        {
            _isPunished = false;
        }
        #region MenuItemProblem
        static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!_main.Item("disable").GetValue<bool>())
            {
                Notifications.AddNotification(new Notification("Your chat dissabled", 2000).SetTextColor(Color.Red).SetBoxColor(Color.Black));
                _isDissabled = true;
            }
            else if (_main.Item("disable").GetValue<bool>())
            {
                var enableTimer = new System.Timers.Timer(30000);
                Notifications.AddNotification(new Notification("Your chat will be enabled in 20 sec", 2000).SetTextColor(Color.Green).SetBoxColor(Color.Black));
                enableTimer.Enabled = true;
                enableTimer.Start();
                enableTimer.Elapsed += enableTimer_Elapsed;
            }
            else
            {
                Notifications.AddNotification(new Notification("Ayee!!error detected! Tell Foxy ASAP! ERROR CODE: -2").SetTextColor(Color.White).SetBoxColor(Color.Blue));
                Console.WriteLine("Value: " + _main.Item("disable").GetValue<bool>());
            }
        }

        static void enableTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Notifications.AddNotification(new Notification("Enabled :)", 2000).SetBorderColor(Color.Green).SetBoxColor(Color.Black).SetTextColor(Color.Green));
            _isDissabled = false;
        }
        #endregion

    }
}
