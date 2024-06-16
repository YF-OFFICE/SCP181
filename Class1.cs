using CommandSystem.Commands.RemoteAdmin.ServerEvent;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingPlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        [Description("打开门幸运值")]
        public int Luck { get; set; } = 50;
        [Description("躲避伤害幸运值")]
        public int Luck1 { get; set; } = 30;
        [Description("最大血量")]
        public int Health { get; set; } = 150;
        [Description("多少人时会刷新")]
        public int People { get; set; } = 1;
        [Description("181开局默认给的物品")]
        public List<ItemType> itemTypes { get; set; } = new List<ItemType>() { ItemType.KeycardJanitor, ItemType.Medkit, ItemType.Coin };

    }
    public class Plugin : Plugin<Config>
    {
        public override string Author => "YF-OFFICE";
        public override Version Version => new Version(1, 0, 0);
        public override string Name => "SCP181";
        public Plugin plugin;
        public string SCP181ID = "";
        public override void OnEnabled()
        {
            plugin = this;
            Exiled.Events.Handlers.Server.RestartingRound += this.RoundEnding;
            Exiled.Events.Handlers.Server.RoundStarted += this.RoundStarted;
            Exiled.Events.Handlers.Player.InteractingDoor += this.Indoor;
            Exiled.Events.Handlers.Player.Hurting += this.Hurt;
            Exiled.Events.Handlers.Player
                .Died += this.Died;

            Log.Info("加载插件中");
            base.OnEnabled();
        }
        public override void OnDisabled()
        {

            Exiled.Events.Handlers.Server.RestartingRound -= this.RoundEnding;
            Exiled.Events.Handlers.Server.RoundStarted -= this.RoundStarted;
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Indoor;
            Exiled.Events.Handlers.Player.Hurting -= this.Hurt;
            Exiled.Events.Handlers.Player.Died -= this.Died;
            plugin = null;
            Log.Info("插件关闭了");
            base.OnDisabled();
        }
        public static List<ItemType> itemTypes = new List<ItemType>();
        public void RoundStarted()
        {
            if (Player.List.Count() >= this.Config.People)
            {
                Timing.CallDelayed(3f, () =>
                {

                    SCP181ID = Player.Get(PlayerRoles.RoleTypeId.ClassD).GetRandomValue().UserId;
                    var player = Player.Get(SCP181ID);
                    player.MaxHealth = Config.Health;
                    player.Health = player.MaxHealth;
                    player.RankName = "SCP181";
                    player.CustomInfo = "SCP-181";
                    player.RankColor = "yellow";
                    player.ClearInventory();
                    player.AddItem(Config.itemTypes);
                    player.ClearBroadcasts();
                    player.Broadcast(5, $"你是SCP181\n具有{Config.Luck}%概率打开门 {Config.Luck1}%免伤 背包里还有好东西");

                });
            }
        }
        public void Indoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                if (ev.Door.IsKeycardDoor&&!ev.Door.IsLocked&&!ev.Door.IsNonInteractable)
                {
                    int luck = new Random().Next(0, 100);
                    if (luck >= Config.Luck)
                    {
                        ev.IsAllowed = true;
                        ev.Player.ShowHint("D:你很幸运打开了门");
                    }
                }

            }

        }
        public void Hurt(HurtingEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                if (ev.Attacker != null && ev.Player != null)
                {
                    int luck = new Random().Next(0, 100);
                    if (luck >= Config.Luck1)
                    {
                        ev.IsAllowed = false;
                        ev.Player.ShowHint("你幸运地躲避了一次伤害");
                        ev.Attacker.ShowHint("你很倒霉 没有伤到181");

                    }
                }
            }

        }
        public void Died(DiedEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                var player = ev.Player;
                if (ev.Attacker == null)
                {
                    player.RankName = "";
                    player.CustomInfo = "";
                    player.RankColor = ""; 
                    SCP181ID = ""; Map.Broadcast(7, $"[设施消息]\nSCP181已被重新收容 \n 收容者:Null"); }
                else
                {
                    SCP181ID = "";
                    player.RankName = "";
                    player.CustomInfo = "";
                    player.RankColor = "";
                    Map.Broadcast(7, $"[设施消息]\nSCP181已被重新收容 \n 收容者:{ev.Attacker.Nickname}");
                }
               
            }


        }
        public void RoundEnding()
        {
            SCP181ID = "";
            Log.Info("181数据已重置");
        }

    }
}
