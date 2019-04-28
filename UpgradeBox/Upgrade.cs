using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using InfinityScript;

namespace UpgradeBox
{
    public class Upgrade : BaseScript
    {
        private static Random _rng = new Random();
        private int UpgradeRange = 75;
        private Entity _airdropCollision;
        public Upgrade() : base()
        {
            Entity care_package = Call<Entity>("getent", "care_package", "targetname");
            _airdropCollision = Call<Entity>("getent", care_package.GetField<string>("target"), "targetname");
            Call("setdvar", "allow_cheats", "1");
            Call("setDvar", "g_hardcore", "1");
            Call("setDvar", "cg_drawCrosshair", "1");

            PlayerConnected += new Action<Entity>(ent =>
            {
                UsablesHud(ent);
                ent.SetClientDvar("g_hardcore", "1");
                ent.SetClientDvar("g_compassForceDisplay", "1");
                WallGuns(ent);
                CreateHUD(ent);

                ent.OnInterval(100, player =>
                {
                    UpdateHUDAmmo(player);
                    return true;
                });

                ent.OnNotify("weapon_change", (player, newWeap) =>
                {
                    UpdateHUDAmmo(player);
                });

                ent.OnNotify("weapon_fired", (player, weapon) =>
                {
                    UpdateHUDAmmo(player);
                });

                HandleUpgradeSpecialWeps(ent);

                ent.SpawnedPlayer += new Action(() =>
                {
                    ent.SetClientDvar("g_hardcore", "1");
                    ent.SetClientDvar("g_compassForceDisplay", "1");
                    ent.OnNotify("Rbox", (boxdude) =>
                    {
                        if (boxdude.GetField<int>("cash") == 5000 || (boxdude.GetField<int>("cash") > 5000))
                        {
                            if (boxdude.GetField<int>("canBox") == 1)
                            {
                                string gun = (UpgradeWeapon(boxdude));
                                boxdude.TakeWeapon(boxdude.CurrentWeapon);
                                boxdude.GiveWeapon(gun);
                                boxdude.SwitchToWeaponImmediate(gun);
                                boxdude.SetField("cash", ent.GetField<int>("cash") - 5000);
                                boxdude.Call("givemaxammo", gun);
                                if (gun == "iw5_usp45_mp_akimbo_silencer02")
                                {
                                    boxdude.Call("setweaponammostock", "iw5_usp45_mp_akimbo_silencer02", 24);
                                }
                                boxdude.SetField("canBox", 0);
                                boxdude.Call("playlocalsound", "ui_mp_nukebomb_timer");
                            }
                        }
                    });
                });
            });
        }

        public Entity WallGuns(Entity player)
        {
        
            //RANDOM BOX
            //Entity boxx = Call<Entity>("spawn", "script_model", new Vector3(617.1638f, 831.9703f, -300.722f));  OLD DOME, use for release!
            Entity boxx = Call<Entity>("spawn", "script_model", new Vector3(1998.867f, 1520.875f, -175.5018f)); //used for teleporting map, remove before publication!
            boxx.Call("setmodel", "com_plasticcase_enemy");
            boxx.Call("clonebrushmodeltoscriptmodel", _airdropCollision);
            boxx.SetField("angles", new Vector3(0, 171, 0));
            boxx.SetField("tipo", "randombox");
            usables.Add(boxx);
            return boxx;
            
        }

        public static List<Entity> usables = new List<Entity>();
        public void UsablesHud(Entity player)
        {
            HudElem message = HudElem.CreateFontString(player, "hudbig", 0.7f);
            message.SetPoint("CENTER", "CENTER", 0, 150);

            OnInterval(100, () =>
            {
                bool _changed = false;
                foreach (Entity ent in usables)
                {
                    if (player.Origin.DistanceTo(ent.Origin) < UpgradeRange)
                    {
                        switch (ent.GetField<string>("tipo"))
                        {                    
                            case "randombox":
                                message.SetText("Press ^3[{+activate}] ^7to buy Pack-A-Punch [Cost: 5000]");
                                player.Call("notifyonplayercommand", "Rbox", "+activate");
                                player.SetField("pickgun", 1);
                                player.SetField("canBox", 1);
                                break;
                            default:
                                message.SetText("");
                                break;
                        }
                        _changed = true;
                    }
                }
                if (!_changed)
                {
                    message.SetText("");
                    player.SetField("canBox", 0);
                }
                return true;
            });
        }


        private void CreateHUD(Entity player)
        {

        }


   }
}
