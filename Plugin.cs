using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.SpookyHash;

namespace FriendlyFired
{
    [BepInPlugin("com.maxgamertyper1.friendlyfired", "Friendly Fired", "1.0.0")]
    public class FriendlyFirePlugin : BaseUnityPlugin
    {
        internal static ConfigFile config;

        internal static ConfigEntry<bool> MissilePatch;

        internal static ConfigEntry<bool> ArrowPatch;

        internal static ConfigEntry<bool> TeslaPatch;

        internal static ConfigEntry<bool> BlachHolePatch;

        internal static ConfigEntry<bool> SpikePatch;

        internal static ConfigEntry<bool> ExplosionPatch;

        private void Log(string message)
        {
            Logger.LogInfo(message);
        }

        private void Awake()
        {
            // Plugin startup logic
            Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            DoPatching();
        }

        private void DoPatching()
        {
            config = ((BaseUnityPlugin)this).Config;
            MissilePatch = config.Bind<bool>("Explosion Patches", "Missile Patch", true, "Make it so your Missile explosions wont kill you");
            ExplosionPatch = config.Bind<bool>("Explosion Patches", "Explosion Patch", true, "Make it so your: [Grenade, Smoke Grenade, Mine] explosions wont kill you");
            TeslaPatch = config.Bind<bool>("Physical Patches", "Tesla Coil Patch", true, "Make it so your Tesla Coil wont kill you");
            SpikePatch = config.Bind<bool>("Physical Patches", "Spike Patch", true, "Make it so your Spike wont kill you");
            ArrowPatch = config.Bind<bool>("Other Patches", "Arrow Patch", true, "Make it so your Arrows wont kill you");
            BlachHolePatch = config.Bind<bool>("Other Patches", "Black Hole Patch", true, "Make it so your Black Holes wont kill you or apply gravity to you");

            var harmony = new Harmony("com.maxgamertyper1.tethered");

            if (MissilePatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(Missile), "OnCollide"), null, new HarmonyMethod(typeof(Patches), "MissileKillPatch")); }
            if (ExplosionPatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(Explosion), "Awake"), new HarmonyMethod(typeof(Patches), "OwnerExplosionPatch")); }
            if (TeslaPatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(StretchableLightning), "InitLightning"), new HarmonyMethod(typeof(Patches), "TeslaCoilPatch")); harmony.Patch((MethodBase)AccessTools.Method(typeof(PlaceSparkNode), "Place"), null, new HarmonyMethod(typeof(Patches), "TeslaHeadPatch"));  }
            if (SpikePatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(Spike), "CastSpike"),null, new HarmonyMethod(typeof(Patches), "SpikeKillPatch")); }
            if (ArrowPatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(BowTransform), "Awake"), new HarmonyMethod(typeof(Patches), "ArrowFirePatch")); }
            if (BlachHolePatch.Value) { harmony.Patch((MethodBase)AccessTools.Method(typeof(BlackHole), "OnCollide"), new HarmonyMethod(typeof(Patches), "BlackHoleKillPatch")); harmony.Patch((MethodBase)AccessTools.Method(typeof(BlackHole), "GravityForce"), null, new HarmonyMethod(typeof(Patches), "BlackHoleGravityPatch"));  } // 2
        }
        private void OnDestroy()
        {
            Log($"Bye Bye From {PluginInfo.PLUGIN_GUID}");
        }
    }
    public class Patches
    {
        public static bool ArrowFirePatch(ref BowTransform __instance) // arrow
        {
            Traverse.Create(__instance).Field("TimeBeforeArrowsHurtOwner").SetValue((Fix)10000);
            return true;
        }
        public static bool OwnerExplosionPatch(ref Explosion __instance) // fixes grenade, smoke explosion, and mine
        {
            __instance.canHurtOwner = false;
            return true;
        }
        public static void TeslaCoilPatch(ref StretchableLightning __instance, ref int id) // prevents death by own tesla coil
        {
            Transform LinkedLightning = __instance.transform.parent;
            if (LinkedLightning == null) return;
            Explosion HurtBox = LinkedLightning.GetComponent<Explosion>();
            HurtBox.canHurtOwner = false;
            HurtBox.PlayerOwnerId = id;
        }
        public static void TeslaHeadPatch(ref PlaceSparkNode __instance, ref GameObject __result, ref int playerId)
        {
            Explosion hurtbox = __result.GetComponent<Explosion>();
            hurtbox.PlayerOwnerId = playerId;
            hurtbox.canHurtOwner = false;
        }
        public static void BlackHoleGravityPatch(ref BlackHole __instance, ref Fix __result, ref FixTransform fixTrans) // prevents the player from getting sucked by their own blackhole
        {
            if (fixTrans == null) return;

            Transform Object = fixTrans.transform.parent;
            if (Object == null) return;

            int OwnerId = (int)Traverse.Create(__instance).Field("ownerId").GetValue();

            Player plr = PlayerHandler.Get().GetPlayer(OwnerId);

            if (fixTrans.name == "Player(Clone)")
            {
                PlayerPhysics plrphysics = fixTrans.GetComponent<PlayerPhysics>();
                IPlayerIdHolder plridholder = Traverse.Create(plrphysics).Field("playerIdHolder").GetValue() as IPlayerIdHolder;
                int plrid = plridholder.GetPlayerId();
                if (plrid == OwnerId)
                {
                    __result = (Fix)0;
                }
            }

            if (Object.name == "Player")
            {
                PlayerPhysics plrphysics = Object.GetComponent<PlayerPhysics>();
                IPlayerIdHolder plridholder = Traverse.Create(plrphysics).Field("playerIdHolder").GetValue() as IPlayerIdHolder;
                int plrid = plridholder.GetPlayerId();
                if (plrid == OwnerId)
                {
                    __result = (Fix)0;
                }
            }
        }
        public static bool BlackHoleKillPatch(ref BlackHole __instance, ref CollisionInformation collision) // prevents the player dying to their own blackhole
        {
            int playerLayer = (int)Traverse.Create(__instance).Field("playerLayer").GetValue();

            if (collision.layer == playerLayer)
            {
                PlayerCollision component2 = collision.colliderPP.fixTrans.gameObject.GetComponent<PlayerCollision>();
                if (component2 != null)
                {
                    IPlayerIdHolder PlayeridHolder = (IPlayerIdHolder)Traverse.Create(component2).Field("playerIdHolder").GetValue();
                    if (PlayeridHolder == null) return true;
                    int Playerid = PlayeridHolder.GetPlayerId();

                    if (Playerid == __instance.OwnerId)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static void SpikeKillPatch(ref Spike __instance)
        {
            PlayerInfo playerdata = (PlayerInfo)Traverse.Create(__instance).Field("playerInfo").GetValue();
            int OwnerId = playerdata.playerId;

            SpikeAttack PhysicalSpike = (SpikeAttack)Traverse.Create(__instance).Field("currentSpike").GetValue();

            Explosion hurtbox = PhysicalSpike.GetComponent<Explosion>();
            hurtbox.PlayerOwnerId = OwnerId;
            hurtbox.canHurtOwner = false;
        }
        public static void MissileKillPatch(ref Missile __instance)
        {
            Item playeridhandler = (Item)Traverse.Create(__instance).Field("item").GetValue();
            int OwnerId = playeridhandler.OwnerId;
            Explosion[] AllExplosions = GameObject.FindObjectsOfType<Explosion>(); ;
            if (AllExplosions == null) return;
            BoplBody body = (BoplBody)Traverse.Create(__instance).Field("body").GetValue();
            Vec2 pos = body.position;
            foreach (Explosion obj in AllExplosions)
            {
                if (obj == null) continue;
                if (obj.GetComponent<FixTransform>().position == pos)
                {
                    Explosion hurtbox = obj.GetComponent<Explosion>();
                    hurtbox.PlayerOwnerId = OwnerId;
                    hurtbox.canHurtOwner = false;
                }
            }
        }
    }
} // friendly fired