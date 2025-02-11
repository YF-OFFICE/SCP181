using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SCP181
{
    public class Config
    {
        [Description("打开门幸运值")]
        public int Luck { get; set; } = 10;
        [Description("躲避伤害幸运值")]
        public int Luck1 { get; set; } = 10;
        [Description("最大血量")]
        public int Health { get; set; } = 150;
        [Description("多少人时会刷新")]
        public int People { get; set; } = 1;
        [Description("181开局默认给的物品")]
        public List<ItemType> itemTypes { get; set; } = new List<ItemType>() { ItemType.KeycardJanitor, ItemType.Medkit, ItemType.Coin };

    }
    public class Plugin1 : Plugin<Config>
    {
        public override string Author => "YF-OFFICE";
        public override Version Version => new Version(1, 0, 0);
        public override string Description => "SCP181角色";

        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);
        public override string Name => "SCP181";
        public Plugin plugin;
        public int SCP181ID = 0;
        public override void Enable()
        {
            plugin = this;
            ServerEvents.RoundRestarted += this.RoundEnding;
            ServerEvents.RoundStarted += this.RoundStarted;
            PlayerEvents.InteractingDoor += this.Indoor;
            PlayerEvents.Hurting += this.Hurt;
            PlayerEvents.Death += this.Died;
            Logger.Info("加载插件完毕");
        }
        public override void Disable()
        {

            ServerEvents.RoundRestarted -= this.RoundEnding;
            ServerEvents.RoundStarted -= this.RoundStarted;
            PlayerEvents.InteractingDoor -= this.Indoor;
            PlayerEvents.Hurting -= this.Hurt;
            PlayerEvents.Death -= this.Died;
            plugin = null;
            Logger.Info("插件关闭了");
        }
        public static List<ItemType> itemTypes = new List<ItemType>();
        public void RoundStarted()
        {
            if (Player.List.Count() >= this.Config.People)
            {
                Timing.CallDelayed(3f, () =>
                {

                    SCP181ID = Player.List.Where(x => x.Role == RoleTypeId.ClassD).ToList().RandomItem().PlayerId;
                    var player = Player.Get(SCP181ID);
                    player.MaxHealth = Config.Health;
                    player.Health = player.MaxHealth;
                    player.GroupName = "SCP181";
                    player.GroupColor = "yellow";
                    player.ClearInventory();
                    if (!Config.itemTypes.IsEmpty())
                    {   Config.itemTypes.ForEach(x=>player.AddItem(x));
                    }
                    player.ClearBroadcasts();
                    player.SendBroadcast($"你是SCP181\n具有{Config.Luck}%概率打开门 {Config.Luck1}%免伤 背包里还有好东西",5);

                });
            }
        }
        public void Indoor(PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Player.PlayerId== SCP181ID)
            {
                
                if (ev.CanOpen == false && !ev.Door.IsLocked)
                {
                    int luck = new Random().Next(0, 100);
                    if (luck <= Config.Luck)
                    {
                        ev.CanOpen = true;
                        ev.Player.SendHint("D:你很幸运打开了门",2);
                    }
                }

            }

        }
        public void Hurt(PlayerHurtingEventArgs ev)
        {
            if (ev.Player.PlayerId == SCP181ID)
            {
                if (ev.Player != null && ev.Target != null)
                {
                    int luck = new Random().Next(0, 100);
                    if (luck <= Config.Luck1)
                    {
                        ev.IsAllowed = false;
                        ev.Target.SendHint("你幸运地躲避了一次伤害");
                        ev.Player.SendHint("你很倒霉 没有伤到181");

                    }
                }
            }

        }
        public void Died(PlayerDeathEventArgs ev)
        {
            if (ev.Player.PlayerId == SCP181ID)
            {
                var player = ev.Player;
                if (ev.Attacker == null)
                {
                    player.GroupName = "";
                    player.GroupColor = "";
                    SCP181ID = 0; 
                    Player.List.ToList().ForEach(x=>x.SendBroadcast($"[设施消息]\nSCP181已被重新收容 \n 收容者:未知",6));
                }
                else
                {
                    SCP181ID = 0;
                    player.GroupName = "";
                    player.GroupColor = "";
                    Player.List.ToList().ForEach(x => x.SendBroadcast($"[设施消息]\nSCP181已被重新收容 \n 收容者:{ev.Attacker.Nickname}",6));
                }

            }


        }
        public void RoundEnding()
        {
            SCP181ID = 0;
            Logger.Debug("181数据已重置");
        }

    }
}
