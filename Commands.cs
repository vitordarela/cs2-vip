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
using System.Threading;

namespace VIP
{
    public partial class VIP
    {
        [ConsoleCommand("css_settings", "User settings")]
        public void opensettings(CCSPlayerController? player, CommandInfo info)
        {
            if (IsVIP[player.Index] != 1) { return; }

            settings_menu_open(player);
        }
        [ConsoleCommand("css_addfakeplayer", "Add fake client for testing reservation slots")]
        public void addfakeplayer(CCSPlayerController? player, CommandInfo info)
        {
            if (AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                ConnectedPlayers++;
                Server.PrintToConsole($"You successful add new fake player on server/debugmode. Now are connected players {ConnectedPlayers}");
            }
        }
        [ConsoleCommand("css_addfakeround", "Add fake round for testing weapons")]
        public void addfakeround(CCSPlayerController? player, CommandInfo info)
        {
            if (AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                Round = 5;
                Server.PrintToConsole($"You successful add new round on server/debugmode. Now are round {Round}");
            }
        }
        [ConsoleCommand("css_testvip", "Test VIP")]
        public void CommandTESTVIP(CCSPlayerController? player, CommandInfo info)
        {
            if (Config.TestVIP.EnableTestVIP)
            {
                MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

                MySqlQueryResult result = MySql!.Table($"{Config.DBPrefix}_users_test_vip").Where(MySqlQueryCondition.New("steam_id", "=", player.SteamID.ToString())).Select();

                if (result.Rows == 0)
                {
                    int TimeSec = Config.TestVIP.TimeOfVIP;
                    var TimeToUTC = DateTime.UtcNow.AddSeconds(Convert.ToInt32(TimeSec)).GetUnixEpoch();
                    var timeofvip = DateTime.UtcNow.AddSeconds(Convert.ToInt32(TimeSec)).GetUnixEpoch();
                    MySqlQueryValue _Tvalues = new MySqlQueryValue()
                    .Add("steam_id", $"{player.SteamID}")
                    .Add("used", $"{timeofvip}")
                    .Add("`group`", $"0"); ;
                    MySql.Table($"{Config.DBPrefix}_users_test_vip").Insert(_Tvalues);

                    var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(TimeToUTC) - DateTimeOffset.UtcNow;
                    var timeRemainingFormatted =
                    $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

                    MySqlQueryValue values = new MySqlQueryValue()
                    .Add("steam_id", $"{player.SteamID}")
                    .Add("end", $"{timeofvip}")
                    .Add("`group`", $"0");
                    MySql.Table($"{Config.DBPrefix}_users").Insert(values);
                    var client = player.Index;
                    LoadPlayerData(player);


                    player.PrintToChat($" {ChatColors.Lime}=========================================");
                    player.PrintToChat($" {Config.Prefix} You use a TestVIP.");
                    player.PrintToChat($" {Config.Prefix} Ending time is {ChatColors.Lime}{timeRemainingFormatted}{ChatColors.Default}.");
                    player.PrintToChat($" {ChatColors.Lime}=========================================");
                    Server.PrintToConsole($"VIP Plugin - Player {player.PlayerName} add new TEST VIP with steamid {player.SteamID}, end time is {timeRemainingFormatted}");
                }
                else
                {

                    var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(result.Get<int>(0, "used")) - DateTimeOffset.UtcNow;
                    var timeRemainingFormatted =
                    $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

                    player.PrintToChat($" {ChatColors.Lime}=========================================");
                    player.PrintToChat($" {Config.Prefix} You cannot use anymore TestVIP.");
                    player.PrintToChat($" {Config.Prefix} Ending time is {ChatColors.Lime}{timeRemainingFormatted}{ChatColors.Default}.");
                    player.PrintToChat($" {ChatColors.Lime}=========================================");
                }
            }
            else
            {
                player.PrintToChat($" {Config.Prefix} On this server is not {ChatColors.Red}allowed{ChatColors.Lime} /testvip{ChatColors.Default}, contact the owner.");
            }
        }
        [ConsoleCommand("css_activator", "Activate VIP from Tokens")]
        public void CommandActivator(CCSPlayerController? player, CommandInfo info)
        {
            var token = info.ArgByIndex(1);
            if (token == null || token == "" || IsInt(token))
                return;
            if (is_vip(player))
            {
                player.PrintToChat($" {Config.Prefix} You are {ChatColors.Lime}VIP{ChatColors.Default}, you {ChatColors.Red}cannot activate{ChatColors.Default} this VIP!");
                return;
            }

            MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

            MySqlQueryResult result = MySql!.Table($"{Config.DBPrefix}_users_key_vip").Where(MySqlQueryCondition.New("token", "=", $"{token}")).Select();

            if (result.Rows == 1)
            {

                var timeofvip = result.Get<int>(0, "end");
                var group_int = result.Get<int>(0, "group");
                if (result.Get<int>(0, "end") == 0)
                {
                    timeofvip = 0;
                }
                else
                {
                    timeofvip = result.Get<int>(0, "end");
                }
                var client = player.Index;

                MySqlQueryValue _Tvalues = new MySqlQueryValue()
                .Add("steam_id", $"{player.SteamID}")
                .Add("end", $"{timeofvip}")
                .Add("`group`", $"{group_int}");
                MySql.Table($"{Config.DBPrefix}_users").Insert(_Tvalues);
                player.PrintToChat($" {ChatColors.Lime}=========================================");
                player.PrintToChat($" {Config.Prefix} {Localizer["Activator"]}");
                if (result.Get<int>(0, "end") == 0)
                {
                    player.PrintToChat($" {Config.Prefix} {Localizer["ForeverVIP"]}");
                }
                player.PrintToChat($" {ChatColors.Lime}=========================================");
                LoadPlayerData(player);
                MySql.Table($"{Config.DBPrefix}_users_key_vip").Where($"token = '{token}'").Delete();
            }
            else
            {
                player.PrintToChat($" {ChatColors.Lime}=========================================");
                player.PrintToChat($" {Config.Prefix} {Localizer["TokenNotExist"]}");
                player.PrintToChat($" {Config.Prefix} {Localizer["TokenTyped", token]}");
                player.PrintToChat($" {ChatColors.Lime}=========================================");
            }
        }
        [ConsoleCommand("css_generatevip", "Generate new VIP token")]
        public void CommandGenerateVIP(CCSPlayerController? player, CommandInfo info)
        {
            if (player != null) return;
            var TimeSec = info.ArgByIndex(1);
            var Group = info.ArgByIndex(2);
            if (TimeSec == null || TimeSec == "" || !IsInt(TimeSec) || Group == null || Group == "" || !IsInt(Group))
            {
                Server.PrintToConsole($"==========================================");
                Server.PrintToConsole($" {Config.Prefix} You must add days: css_generatevip <DAYS> <GROUP>, must be added in int.");
                Server.PrintToConsole($" {Config.Prefix} If you wanna give forever VIP: css_generatevip 0 0");
                Server.PrintToConsole($" {Config.Prefix} <------------> List's of Groups <------------>");
                Server.PrintToConsole($" {Config.Prefix} < Group '0' > {Config.GroupsNames.Group1} < Group '0' >");
                Server.PrintToConsole($" {Config.Prefix} < Group '1' > {Config.GroupsNames.Group2} < Group '1' >");
                Server.PrintToConsole($" {Config.Prefix} < Group '2' > {Config.GroupsNames.Group3} < Group '2' >");
                Server.PrintToConsole($" {Config.Prefix} <------------> List's of Groups <------------>");
                Server.PrintToConsole($"==========================================");

                return;
            }
            var TimeToUTC = DateTime.UtcNow.AddDays(Convert.ToInt32(TimeSec)).GetUnixEpoch();
            var timeofvip = 0;
            if (TimeSec == "0")
            {
                timeofvip = 0;
            }
            else
            {
                timeofvip = DateTime.UtcNow.AddDays(Convert.ToInt32(TimeSec)).GetUnixEpoch();
            }

            var token = CreatePassword(20);
            var group_int = Group;

            var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(TimeToUTC) - DateTimeOffset.UtcNow;
                var timeRemainingFormatted =
                $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

            MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);
            MySqlQueryValue values = new MySqlQueryValue()
            .Add("token", token)
            .Add("end", $"{timeofvip}")
            .Add("`group`", group_int);
            MySql.Table($"{Config.DBPrefix}_users_key_vip").Insert(values);

            Server.PrintToConsole($"==========================================");
            Server.PrintToConsole($"You generate new VIP Token");
            Server.PrintToConsole($"Token: {token}");
            Server.PrintToConsole($"Ending (days): {TimeSec}");
            Server.PrintToConsole($"Group ID: {Group}");
            Server.PrintToConsole($"==========================================");



        }
        [ConsoleCommand("css_addvip", "Add new VIP")]
        public void CommandAddVIP(CCSPlayerController? player, CommandInfo info)
        {
            var Group = info.ArgByIndex(3);
            var SteamIDC = info.ArgByIndex(2);
            var TimeSec = info.ArgByIndex(1);
            if (!AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                player.PrintToChat($" {Config.Prefix} You are not admin..");
                return;
            }
            else if (SteamIDC == null || SteamIDC == "" || !IsInt(SteamIDC))
            {
                player.PrintToChat($" {Config.Prefix} You must add SteamID. Example {ChatColors.Lime}/addvip <Time in days> 77777777 <GROUP>{ChatColors.Default}, must be added in int.");
                player.PrintToChat($" {Config.Prefix} Or if you wanna add forever VIP type {ChatColors.Lime}/addvip 0 77777777{ChatColors.Default}.");
                return;
            }
            else if (TimeSec == null || TimeSec == "" || !IsInt(TimeSec))
            {
                player.PrintToChat($" {Config.Prefix} You must add Time in days. Example {ChatColors.Lime}/addvip <Time in days> 77777777 <GROUP>{ChatColors.Default}, must be added in int.");
                player.PrintToChat($" {Config.Prefix} Or if you wanna add forever VIP type {ChatColors.Lime}/addvip 0 77777777{ChatColors.Default}.");

                return;
            }
            else if (Group == null || Group == "" || !IsInt(Group))
            {
                player.PrintToChat($" {Config.Prefix} You must add Group (Exist: 0, 1). Example {ChatColors.Lime}/addvip <Time in days> 77777777 <GROUP>{ChatColors.Default}, must be added in int.");
                player.PrintToChat($" {Config.Prefix} Or if you wanna add forever VIP type {ChatColors.Lime}/addvip 0 77777777 0{ChatColors.Default}.");
                player.PrintToChat($" {Config.Prefix} {ChatColors.Lime}<------------>{ChatColors.Default} List's of Groups {ChatColors.Lime}<------------>");
                player.PrintToChat($" {Config.Prefix} {ChatColors.Lime}< Group '0' >{ChatColors.Default} {Config.GroupsNames.Group1} {ChatColors.Lime}< Group '0' >");
                player.PrintToChat($" {Config.Prefix} {ChatColors.Lime}< Group '1' >{ChatColors.Default} {Config.GroupsNames.Group2} {ChatColors.Lime}< Group '1' >");
                player.PrintToChat($" {Config.Prefix} {ChatColors.Lime}< Group '2' >{ChatColors.Default} {Config.GroupsNames.Group3} {ChatColors.Lime}< Group '2' >");
                player.PrintToChat($" {Config.Prefix} {ChatColors.Lime}<------------>{ChatColors.Default} List's of Groups {ChatColors.Lime}<------------>");


                return;
            }
            else
            {
                var TimeToUTC = DateTime.UtcNow.AddDays(Convert.ToInt32(TimeSec)).GetUnixEpoch();
                var timeofvip = 0;
                if(TimeSec == "0")
                {
                    timeofvip = 0;
                }
                else
                {
                    timeofvip = DateTime.UtcNow.AddDays(Convert.ToInt32(TimeSec)).GetUnixEpoch();
                }

                
                var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(TimeToUTC) - DateTimeOffset.UtcNow;
                var timeRemainingFormatted =
                $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
                var group_int = Group;

                MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);
                MySqlQueryValue values = new MySqlQueryValue()
                .Add("steam_id", $"{SteamIDC}")
                .Add("end", $"{timeofvip}")
                .Add("`group`", group_int);
                MySql.Table($"{Config.DBPrefix}_users").Insert(values);
                player.PrintToChat($" {ChatColors.Lime}=========================================");
                player.PrintToChat($" {Config.Prefix} Player with steamid {ChatColors.Lime}{SteamIDC}{ChatColors.Default} has been added.");
                player.PrintToChat($" {Config.Prefix} Ending time is {ChatColors.Lime}{timeRemainingFormatted}{ChatColors.Default}.");
                player.PrintToChat($" {ChatColors.Lime}=========================================");
                Server.PrintToConsole($"VIP Plugin - Admin {player.PlayerName} add new VIP with steamid {SteamIDC}, end time is {timeRemainingFormatted}");

            }
        }
        [ConsoleCommand("css_vips", "Load all VIPs on server")]
        public void CommandVIPList(CCSPlayerController? controller, CommandInfo info)
        {
            int vips = 0;
            controller.PrintToChat($" {ChatColors.Green}===!-!==={ChatColors.Lime} VIP {ChatColors.Default}List {ChatColors.Green}===!-!===");
            foreach (var player in Utilities.GetPlayers().Where(player => player is { IsBot: false, IsValid: true }).Where(player => IsVIP[player.Index] == 1))
            {
                vips++;
                controller.PrintToChat($" [{ChatColors.Green}{player.SteamID}{ChatColors.Default}] {ChatColors.Orange}{player.PlayerName}");
            }
            controller.PrintToChat($" {ChatColors.Green}► Numbers of VIPs{ChatColors.Default} {ChatColors.Purple}{vips}{ChatColors.Default} {ChatColors.Green}◄ Numbers of VIPs");
            controller.PrintToChat($" {ChatColors.Green}===!-!==={ChatColors.Lime} VIP {ChatColors.Default}List {ChatColors.Green}===!-!===");
        }
        [ConsoleCommand("css_vip", "Info about VIP")]
        public void CommandVIPInfo(CCSPlayerController? player, CommandInfo info)
        {
            MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

            MySqlQueryResult result = MySql!.Table($"{Config.DBPrefix}_users").Where(MySqlQueryCondition.New("steam_id", "=", player.SteamID.ToString())).Select();
            var status = "";
            var formating = "";
            int status_i = 0;
            if (result.Rows == 1)
            {
                var client = player.Index;
                var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(result.Get<int>(0, "end")) - DateTimeOffset.UtcNow;
                var nowtimeis = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeRemainingFormatted =
                $"{timeRemaining.Days}d {timeRemaining.Hours}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
                status = $" {ChatColors.Green}Active";
                formating = $" {ChatColors.Green}{timeRemainingFormatted}";
                IsVIP[client] = 1;
                LoadPlayerData(player);
                status_i = 1;
                if (result.Get<int>(0, "end") != 0)
                {
                    status = $" {ChatColors.Green}Active";
                }
                if (result.Get<int>(0, "end") == 0)
                {
                    formating = $" {ChatColors.Green}Never ending";
                }
                status_i = 1;
            }
            else
            {
                status = $" {ChatColors.Red} Inactive";
                status_i = 0;
            }
            player.PrintToChat($" {ChatColors.Green}==!-!=={ChatColors.Lime} VIP {ChatColors.Default}Clutch Arena {ChatColors.Green}==!-!==");
            player.PrintToChat($" {ChatColors.Gold}» {ChatColors.Default}Sua VIP está: {status}{ChatColors.Default}");
            if(status_i == 1)
            {
                player.PrintToChat($" {ChatColors.Gold}» {ChatColors.Default}O Tempo de {ChatColors.Lime}duração da sua VIP é {formating}{ChatColors.Default}.");
                player.PrintToChat($" {ChatColors.Gold}» {ChatColors.Default}Sua {ChatColors.Lime}TAG {ChatColors.Default}de VIP é {ChatColors.Green}{get_name_group(player)}{ChatColors.Default}.");
                //player.PrintToChat($" {ChatColors.Gold}▼ {ChatColors.Lime}Yours command available for you{ChatColors.Gold} ▼");
                //if (get_vip_group(player) >= Config.CommandOnGroup.Weapons)
                //{
                //    player.PrintToChat($" {ChatColors.Gold}► {ChatColors.Default}Selecting weapons : {ChatColors.Lime}/weapon {ChatColors.Default}1,2,3,4,5{ChatColors.Gold} ◄");
                //}
                //if (get_vip_group(player) >= Config.CommandOnGroup.Pack )
                //{
                //    player.PrintToChat($" {ChatColors.Gold}► {ChatColors.Default}Selecting package : {ChatColors.Lime}/pack {ChatColors.Default}1,2{ChatColors.Gold} ◄");
                //}
                //if (get_vip_group(player) >= Config.CommandOnGroup.Respawn || Config.RespawnAllowed)
                //{
                //    player.PrintToChat($" {ChatColors.Gold}► {ChatColors.Default}Respawn on spawn  : {ChatColors.Lime}/respawnvip{ChatColors.Gold} ◄");
                //}
                //player.PrintToChat($" {ChatColors.Gold}► {ChatColors.Default}Turn of auto wep. : {ChatColors.Lime}/guns_off{ChatColors.Gold} ◄");

            }
            player.PrintToChat($" {ChatColors.Green}==!-!=={ChatColors.Lime} VIP {ChatColors.Default}Clutch Arena {ChatColors.Green}==!-!==");


        }
       
        
     
       
    }
}
