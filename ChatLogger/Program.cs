using System.IO;
using System;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Sandbox;
using Color = System.Drawing.Color;


namespace ChatLogger
{

    class Program
    {
        static Menu main;
        static string _path = SandboxConfig.DataDirectory + "\\Chatlog.txt";
      //  static string now = DateTime.Today.ToString(CultureInfo.InvariantCulture);
      //  static StreamWriter file = new StreamWriter(path + "\\"+ DateTime.Today + "ChatLog.txt", true);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }
        
        static void Game_OnGameLoad(EventArgs args)
        {
            Notifications.AddNotification(new Notification("ChatLogger loaded!",1000).SetBoxColor(Color.WhiteSmoke).SetTextColor(Color.Green));
            (main = new Menu("Chat Logger","chatlogger",true)).AddToMainMenu();
            var enabler = main.AddItem(new MenuItem("enabled","Enabled?").SetValue(false));
            var nottify = main.AddItem(new MenuItem("notify", "Show Notifications").SetValue(true));
            
            //File.Create(SandboxConfig.DataDirectory + "\\ChatlogTest");

            enabler.ValueChanged += EnablerValueChanged;
            Game.OnChat +=Game_OnChat;
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
        }

        static void Game_OnGameEnd(EventArgs args)
        {
            File.Move(_path,SandboxConfig.DataDirectory+"\\ChatLog"+1+"txt");
        }

        static void EnablerValueChanged(object sender, OnValueChangeEventArgs e)
        {
            Notifications.AddNotification(
                e.GetNewValue<bool>()
                    ? new Notification("Logging enabled", 1000)
                    : new Notification("Logging dissabled", 1000));
        }

        static void Game_OnChat(GameChatEventArgs args)
        {
            if (!main.Item("enabled").GetValue<bool>())
                return;
            try{
                var stream = new StreamWriter(_path, true);
                if (args.Sender.IsAlly)
                {
                    stream.WriteLine("[" + Utils.FormatTime(Game.ClockTime) + "]" + " sender: " + args.Sender.Name + " says: " + args.Message);
                    stream.Close();
                }
                else
                {
                    stream.WriteLine("[" + Utils.FormatTime(Game.ClockTime) + "]" + "[enemy] sender: " + args.Sender.Name + " says: " + args.Message);
                    stream.Close();
                }
                if (main.Item("notify").GetValue<bool>())
                    Notifications.AddNotification(new Notification("Chat loged",500).SetBoxColor(Color.Black).SetTextColor(Color.Green));
            }
            catch (Exception e)
            {
                //Notifications.AddNotification("ChatLog error: " + e.Message,1000);
            }
        }
    }
}
