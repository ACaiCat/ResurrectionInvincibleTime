using Microsoft.Xna.Framework;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria.GameContent.Creative;

namespace GodTime
{
    namespace TaskSystem
    {
        [ApiVersion(2, 1)]
        public class MainPlugin : TerrariaPlugin
        {
            /// <summary>
            /// Gets the author(s) of this plugin
            /// </summary>
            public override string Author => "Cai";

            /// <summary>
            /// Gets the description of this plugin.
            /// A short, one lined description that tells people what your plugin does.
            /// </summary>
            public override string Description => "复活为玩家添加一段无敌时间";

            /// <summary>
            /// Gets the name of this plugin.
            /// </summary>
            public override string Name => "复活无敌时间";

            /// <summary>
            /// Gets the version of this plugin.
            /// </summary>
            public override Version Version => new Version(1, 0, 0, 0);
            public static ThreadStart threadStart = new ThreadStart(closeGod);
            Thread thread = new Thread(threadStart);
            public string ConfigPath { get { return Path.Combine(TShock.SavePath, "复活无敌时间.json"); } }
            public Config config = new Config();
            public static List<TSPlayer> godTimePlayer = new();
            /// <summary>
            /// Initializes a new instance of the TestPlugin class.
            /// This is where you set the plugin's order and perfrom other constructor logic
            /// </summary>
            public MainPlugin(Main game) : base(game)
            {

            }

            /// <summary>
            /// Handles plugin initialization. 
            /// Fired when the server is started and the plugin is being loaded.
            /// You may register hooks, perform loading procedures etc here.
            /// </summary>
            public override void Initialize()
            {
                ReadConfig();
                ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
                GeneralHooks.ReloadEvent += Reload;
                GetDataHandlers.KillMe += new EventHandler<GetDataHandlers.KillMeEventArgs>(OnPlayerKilled);
                GetDataHandlers.PlayerSpawn += new EventHandler<GetDataHandlers.SpawnEventArgs>(OnPlayerSpawn);
                ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
            }


            private void OnInitialize(EventArgs args)
            {
                thread.IsBackground = true;
                thread.Start();
            }

            private void OnPlayerLeave(LeaveEventArgs args)
            {
                var plr = TShock.Players[args.Who];
                godTimePlayer.RemoveAll(x => plr.Index == plr.Index);
            }

            public static void closeGod()
            {

                while (true)
                {
                    try
                    {
                        foreach (var plr in TShock.Players)
                        {
                            if (plr != null && !plr.GodMode && !godTimePlayer.Exists(x => x.Index == plr.Index))
                            {
                                //TShock.Utils.Broadcast("关闭玩家:" + plr.Name, 0, 0, 0);
                                var godPower = CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>();
                                godPower.SetEnabledState(plr.Index, false);
                            }
                        }
                        
                    }
                    catch (Exception e)
                    {
                        TShock.Log.Error(e.Message);
                    }
                    Thread.Sleep(5 * 1000);
                }
               
            }

            private async void OnPlayerSpawn(object? sender, GetDataHandlers.SpawnEventArgs args)
            {
                if (args.Player.GodMode) { return; }
                if (godTimePlayer.Exists(x => x.Index == args.Player.Index))
                {
                    var godPower = CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>();
                    godPower.SetEnabledState(args.Player.Index, true);
                    args.Player.SendInfoMessage($"[i:29]你已复活, 获得[c/32FF82:{config.time}]秒无敌时间!");
                    await Task.Delay(config.time * 1000);
                    if ((godTimePlayer.Exists(x => x.Index == args.Player.Index)))
                    {
                        godPower.SetEnabledState(args.Player.Index, false);
                        args.Player.SendInfoMessage($"[i:29]无敌时间已结束!");
                        godTimePlayer.RemoveAll(x=>x.Index==args.Player.Index);
                    }
                }

            }

            public void OnPlayerKilled(object? sender,GetDataHandlers.KillMeEventArgs args)
            {
                godTimePlayer.Add(args.Player);
            }

            public void Reload (ReloadEventArgs args)
            {
                ReadConfig();
            }


      
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                    GeneralHooks.ReloadEvent -= Reload;
                }
                base.Dispose(disposing);
            }
            public void ReadConfig()
            {
                try
                {
                    config = Config.Read(ConfigPath).Write(ConfigPath);
                }
                catch (Exception ex)
                {
                    config = new Config();
                    TShock.Log.ConsoleError("[玩家复活无敌] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
                }


            }
        }
    }
}
