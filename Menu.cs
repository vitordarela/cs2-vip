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
using CounterStrikeSharp.API.Modules.Memory;
using static CounterStrikeSharp.API.Core.Listeners;
using System.Runtime.Intrinsics.Arm;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nexd.MySQL;
using System.Runtime.ExceptionServices;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using System.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;


namespace VIP
{
    public partial class VIP
    {
        static public void settings_menu_open(CCSPlayerController? player)
        {
            var client = player.Index;
            var smoke = "";
            if (UserSmoke[client] == 1) { smoke = "Disable"; } else { smoke = "Enable"; }
            var sett_menu = new ChatMenu("-- [ VIP SETTINGS ] --");
            sett_menu.AddMenuOption($"[{smoke}] Colored smokes", sett_handle);


            ChatMenus.OpenMenu(player, sett_menu);
        }
        public static void sett_handle(CCSPlayerController player, ChatMenuOption option)
        {
            var client = player.Index;
            
            if (option.Text.Contains("smokes"))
            {
                if (UserSmoke[client] == 1) { UserSmoke[client] = 0; } else { UserSmoke[client] = 1; }
            }
           
        }
    }
}
