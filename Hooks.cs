using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using static CounterStrikeSharp.API.Core.Listeners;
using System.Runtime.Intrinsics.Arm;
using static System.Runtime.InteropServices.JavaScript.JSType;

using Nexd.MySQL;
using System.Runtime.ExceptionServices;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Memory;
using System.Security.Cryptography;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;


namespace VIP
{
    
    public partial class VIP
    {
        private readonly nint Handle;
        public CHandle<CBaseEntity> EndEntity => Schema.GetDeclaredClass<CHandle<CBaseEntity>>(this.Handle, "CBeam", "m_hEndEntity");
        private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
        {
            if (!Config.EnableVIPPrefix)
            {
                return HookResult.Continue;
            }
            var client = player.Index;
                var message = info.GetArg(1);
                string message_first = info.GetArg(1);

                if (player == null || !player.IsValid || player.IsBot || message == null || message == "")
                    return HookResult.Continue;
                if (message_first.Substring(0, 1) == "/" || message_first.Substring(0, 1) == "!" || message_first.Substring(0, 1) == "." || message_first.Substring(0, 1) == "rtv")
                    return HookResult.Continue;
                var GetTag = "";
                if (IsVIP[client] == 1)
                {
                    GetTag = $" {ChatColors.Lime}VIP {ChatColors.Default}»";
                    var isAlive = player.PawnIsAlive ? "" : "-DEAD-";

                    Server.PrintToChatAll(ReplaceTags($"{isAlive} {GetTag} {ChatColors.Red}{player.PlayerName} {ChatColors.Default}: {ChatColors.Lime}{message}"));
                }
                else
                {
                return HookResult.Continue;
                }
                return HookResult.Handled;
        }
        private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
        {
            if (!Config.EnableVIPPrefix)
            {
                return HookResult.Continue;
            }
            var client = player.Index;
                var message = info.GetArg(1);
                string message_first = info.GetArg(1);

                if (player == null || !player.IsValid || player.IsBot || message == null || message == "")
                    return HookResult.Continue;
                if (message_first.Substring(0, 1) == "/" || message_first.Substring(0, 1) == "!" || message_first.Substring(0, 1) == "." || message_first.Substring(0, 1) == "rtv")
                    return HookResult.Continue;
                var GetTag = "";
                if (IsVIP[client] == 1)
                {
                    GetTag = $" {ChatColors.Lime}VIP {ChatColors.Default}»";
                    var isAlive = player.PawnIsAlive ? "" : "-DEAD-";
                    for (int i = 1; i <= Server.MaxPlayers; i++)
                    {
                        CCSPlayerController? pc = Utilities.GetPlayerFromIndex(i);
                        if (pc == null || !pc.IsValid || pc.IsBot || pc.TeamNum != player.TeamNum) continue;
                        pc.PrintToChat(ReplaceTags($"{isAlive}(TEAM) {GetTag} {ChatColors.Red}{player.PlayerName} {ChatColors.Default}: {ChatColors.Lime}{message}"));
                    }
                }
                else
                {
                    return HookResult.Continue;
                }
            return HookResult.Handled;
        }
        [GameEventHandler]
        public HookResult OnClientConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;
            if (ConnectedPlayers == 0)
            {
                ConnectedPlayers = 1;
            }
            else
            {
                ConnectedPlayers++;
            }

            var client = player.Index;
            Used[client] = 0;
            LastUsed[client] = 0;
            IsVIP[client] = 0;
            HaveGroup[client] = 0;
            LoadPlayerData(player);

            player.PrintToChat($" {ChatColors.Green}www.ClutchArena.com.br");
            player.PrintToChat($" {ChatColors.White}Olá {ChatColors.LightYellow}{player.PlayerName} {ChatColors.White} acesse nosso site e concorra a prêmios em SKIN todo mês");
            player.PrintToChat($" {ChatColors.Green}Torne-se um membro VIP {ChatColors.LightYellow}🜲 {ChatColors.Green} e aproveite várias vantagens, além de participar de sorteios exclusivos!");
            player.PrintToChat($" {ChatColors.White}Faça parte da nossa comunidade: {ChatColors.Green}!discord !whatsapp");

            if (Config.WelcomeMessageEnable)
            {
                player.PrintToChat($" {Localizer["welcome"]}");
            }
            return HookResult.Continue;
        }
        [GameEventHandler]
        public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid;

            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;
            int connected = 0;
            foreach (var player_l in Utilities.GetPlayers().Where(player => player is { IsBot: false, IsValid: true }))
            {
                connected++;
            }
            ConnectedPlayers = connected;
            return HookResult.Continue;
        }
    }
}
