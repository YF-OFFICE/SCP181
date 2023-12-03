using CommandSystem.Commands.RemoteAdmin.ServerEvent;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Exiled.Events;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingPlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
    public class Plugin:Plugin<Config>
    {
        public override string Author => "YFOFFICE";
        public override Version Version => new Version(1, 0, 1);
        public override string Name => "ROLE-SCP181";
        public Plugin plugin;
        public string SCP181ID = "";
        public override void OnEnabled()
        {
            plugin = this;
            Exiled.Events.Handlers.Server.RestartingRound+= this.RoundEnding;
            Exiled.Events.Handlers.Server.RoundStarted += this.RoundStarted;
            Exiled.Events.Handlers.Player.InteractingDoor += this.Indoor;
            Exiled.Events.Handlers.Player.Hurting += this.Hurt;
            Exiled.Events.Handlers.Player.Died+= this.Died;
            Log.Info("加载插件中");
            base.OnEnabled();
        }
        public void RoundStarted()
        {
            Timing.CallDelayed(2f, () =>
            {
                SCP181ID = Player.Get(PlayerRoles.RoleTypeId.ClassD).ToList().RandomItem().UserId;
            });
              Timing.CallDelayed(2f, () => {
                var player = Player.Get(SCP181ID);
                player.MaxHealth = 150;
                player.Health = 150;
                player.RankName = "SCP181";
                player.CustomInfo = "SCP-181";
                player.RankColor = "yellow";
                player.ClearInventory();
                player.AddItem(Item.List.ToList().FindAll(x =>x.Type.IsKeycard()).RandomItem());
                player.AddItem(ItemType.SCP207);
                player.AddItem(ItemType.Medkit);
                player.ClearBroadcasts();
                player.Broadcast(5,"你是SCP181\n具有50%概率打开门 30%免伤 背包里还有好东西");
                
                });
        }
        public void Indoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                int luck = new Random().Next(0, 100);
                if (luck >= 50)
                { 
                    ev.IsAllowed = true;
                    ev.Player.ShowHint("D:你很幸运打开了门");
                }
            
            }
        
        }
        public void Hurt(HurtingEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                int luck = new Random().Next(0,100);
                if (luck >= 70)
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint("你幸运地躲避了一次伤害");
                    ev.Attacker.ShowHint("你很倒霉 没有伤到181");
                
                }
            
            }
        
        }
        public void Died(DiedEventArgs ev)
        {
            if (ev.Player.UserId == SCP181ID)
            {
                Map.Broadcast(7,$"[设施消息]\nSCP181已被重新收容 \n 收容者:{ev.Attacker.Nickname}");
                SCP181ID = "";
            }
        
        
        }
        public void RoundEnding()
        {
            SCP181ID = "";
            Log.Info("181数据已重置");
        }

    }
}
