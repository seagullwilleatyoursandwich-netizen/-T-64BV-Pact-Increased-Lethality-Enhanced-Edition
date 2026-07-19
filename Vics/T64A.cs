using GHPC;
using GHPC.Equipment;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using MelonLoader.Utils;
using ModUtil;
using NWH.VehiclePhysics;
using Reticle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class T64A : Module
    {
        static MelonPreferences_Entry<bool> t64_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static MelonPreferences_Entry<string> t64_ammo_type;
        static MelonPreferences_Entry<string> t64av_ammo_type;
        static MelonPreferences_Entry<string> t64av_heat_type;
        static MelonPreferences_Entry<bool> t64_random_ammo;
        static MelonPreferences_Entry<bool> has_drozd;
        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        internal static MelonPreferences_Entry<bool> lead_calculator_t64;
        static MelonPreferences_Entry<bool> du_armour;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> tpn3;
        static MelonPreferences_Entry<bool> kontakt1;
        static MelonPreferences_Entry<int> t64a_1981_av_chance;
        static MelonPreferences_Entry<int> t64a_1983_av_chance;
        static MelonPreferences_Entry<int> t64a_1984_av_chance;
        static MelonPreferences_Entry<List<string>> t64_random_ammo_pool;

        public static void Config(MelonPreferences_Category cfg)
        {
            var random_ammo_pool = new List<string>()
            {
                "3BM26",
                "3BM32",
                "3BM42",
                "3BM46"
            };

            t64_patch = cfg.CreateEntry<bool>("T-64A Patch", true);
            t64_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-64A)", false);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            kontakt1 = cfg.CreateEntry<bool>("Kontakt-1 (T-64AV)", true);
            t64a_1981_av_chance = cfg.CreateEntry<int>("T-64A 1981 -> T-64AV Chance (%)", 40);
            t64a_1983_av_chance = cfg.CreateEntry<int>("T-64A 1983 -> T-64AV Chance (%)", 60);
            t64a_1984_av_chance = cfg.CreateEntry<int>("T-64A 1984 -> T-64AV Chance (%)", 80);

            t64_ammo_type = cfg.CreateEntry<string>("AP Round (T-64A)", "3BM26");
            t64_ammo_type.Description = " ";
            t64_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized)";

            t64av_ammo_type = cfg.CreateEntry<string>("AP Round (T-64AV)", "3BM26");
            t64av_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized), 3BM46";
            t64av_ammo_type.Description = " ";

            t64av_heat_type = cfg.CreateEntry<string>("HEAT Round (T-64AV)", "3BK18M");
            t64av_heat_type.Comment = "3BK18M";
            t64av_heat_type.Description = " ";

            t64_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-64A)", false);
            t64_random_ammo.Comment = "Randomizes ammo selection for T-64As (3BM26, 3BM32, 3BM42), 3BM46";
            t64_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-64A)", random_ammo_pool);
            t64_random_ammo_pool.Comment = "3BM26, 3BM32, 3BM42, 3BM46";

            lead_calculator_t64 = cfg.CreateEntry<bool>("Lead Calculator (T-64A)", false);
            lead_calculator_t64.Comment = "For use with the standard sight; displays a number that corresponds to the horizontal markings on the sight (LRF required)";
            lead_calculator_t64.Description = " ";

            //has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-64A)", false);
            //has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";

            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-64A)", false);
            better_stab.Comment = "Less reticle blur, shake while on the move";

            tpn3 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-64A)", true);
            tpn3.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";

            thermals = cfg.CreateEntry<bool>("Has Thermals (T-64A + T-64AV)", false);
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-64A + T-64AV)", "High");
            thermals_quality.Comment = "Low, High";
        }
        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                if (vic == null) continue;

                GameObject vic_go = vic.gameObject;
                if (!vic.FriendlyName.Contains("T-64A")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                int conversion_chance = 0;

                if (vic.FriendlyName.Contains("T-64A obr.1981"))
                {
                    conversion_chance = t64a_1981_av_chance.Value;
                }
                else if (vic.FriendlyName.Contains("T-64A obr.1983"))
                {
                    conversion_chance = t64a_1983_av_chance.Value;
                }
                else if (vic.FriendlyName.Contains("T-64A obr.1984"))
                {
                    conversion_chance = t64a_1984_av_chance.Value;
                }

                bool converted_to_t64av =
                kontakt1.Value &&
                UnityEngine.Random.Range(0, 100) < conversion_chance;
                if (converted_to_t64av)
                {
                    vic_go.AddComponent<AlreadyConverted>();
                    if (vic.FriendlyName.Contains("T-64A obr.1981"))
                    {
                        vic._friendlyName = "T-64AV obr.1981";
                    }
                    else if (vic.FriendlyName.Contains("T-64A obr.1983"))
                    {
                        vic._friendlyName = "T-64AV obr.1983";
                    }
                    else if (vic.FriendlyName.Contains("T-64A obr.1984"))
                    {
                        vic._friendlyName = "T-64AV obr.1984";
                    }
                }

                if (converted_to_t64av)
                {
                    // ERA CONVERSION (stolen from bv lol)
                    Transform hull = vic.transform.Find("---T64A_MESH---/HULL");
                    Transform turret = vic.transform.Find("---T64A_MESH---/HULL/TURRET");

                    GameObject k1_full = GameObject.Instantiate(T64B.t64bv_full);

                    Transform era_hull = k1_full.transform.Find("HULL K1");
                    Transform era_turret = k1_full.transform.Find("TURRET K1");

                    Transform hull_misc = k1_full.transform.Find("HULL MISC");
                    Transform turret_misc = k1_full.transform.Find("TURRET MISC");

                    // HULL ERA
                    if (era_hull != null)
                    {
                        era_hull.SetParent(hull, false);
                        if (hull_misc != null)
                            hull_misc.SetParent(hull, false);

                        era_hull.localPosition = new Vector3(-0.06f, -0.35f, -0.785f);
                        era_hull.localEulerAngles = new Vector3(0f, 90f, 0f);
                        era_hull.localScale = new Vector3(0.12f, 0.12f, 0.12f);

                        hull_misc.localPosition = new Vector3(-0.06f, 0.39f, -0.785f);
                        hull_misc.localEulerAngles = new Vector3(0f, 90f, 0f);
                        hull_misc.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    }
                    // TURRET ERA
                    if (era_turret != null)
                    {
                        era_turret.SetParent(turret, false);
                        if (turret_misc != null)
                            turret_misc.SetParent(turret, false);

                        era_turret.localPosition = new Vector3(-0.06f, -0.26f, -0.24f);
                        era_turret.localEulerAngles = new Vector3(0f, 90f, 0f);
                        era_turret.localScale = new Vector3(0.12f, 0.12f, 0.12f);

                        turret_misc.localPosition = new Vector3(-0.06f, -0.27f, -0.43f);
                        turret_misc.localEulerAngles = new Vector3(0f, 90f, 0f);
                        turret_misc.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    }

                    VehicleSmokeManager smoke_manager = vic.GetComponentInChildren<VehicleSmokeManager>();

                    if (smoke_manager != null)
                    {
                        Transform smokes = turret_misc.Find("SMOKES");

                        if (smokes != null)
                        {
                            List<Transform> new_smokes = new List<Transform>();

                            // T-64AV has 8 smoke launchers
                            for (int i = 0; i < 8; i++)
                            {
                                Transform cap = smokes.Find("SMOKE CAP " + i);

                                if (cap != null)
                                    new_smokes.Add(cap);
                            }

                            MelonLogger.Msg("T64AV smoke caps found: " + new_smokes.Count);

                            // Only move the existing smoke slots onto the K1 rack
                            for (int i = 0; i < new_smokes.Count; i++)
                            {
                                VehicleSmokeManager.SmokeSlot slot = smoke_manager._smokeSlots[i];

                                Transform smoke_cap = new_smokes[i];

                                slot.DisplayBone = smoke_cap;

                                if (slot.SpawnLocation != null)
                                {
                                    slot.SpawnLocation.transform.SetParent(smoke_cap);
                                    slot.SpawnLocation.transform.position =
                                        smoke_cap.GetComponent<Renderer>().bounds.center;
                                }
                            }


                            // Remove the extra T-64A salvo
                            List<VehicleSmokeManager.SmokePattern> temp =
                                smoke_manager._smokeGroups.ToList();

                            temp.RemoveAt(2);

                            smoke_manager._smokeGroups = temp.ToArray();
                        }
                    }

                    GameObject.Destroy(k1_full);
                    Transform old_smokes = turret.Find("smoke mortars");

                    if (old_smokes != null)
                    {
                        old_smokes.gameObject.SetActive(false);
                    }
                }

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();

                int rand = UnityEngine.Random.Range(0, Ammo_125mm.ap.Count);
                string ammo_str = t64_random_ammo.Value ? t64_random_ammo_pool.Value.ElementAt(rand) : t64_ammo_type.Value;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.05f;
                    day_optic.slot.VibrationShakeMultiplier = 0.1f;
                }

                if (tpn3.Value)
                {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }

                try
                {
                    loadout_manager.LoadedAmmoList.AmmoClips[0] = Ammo_125mm.ap[ammo_str];

                    if (converted_to_t64av && t64av_heat_type.Value == "3BK18M")
                        loadout_manager.LoadedAmmoList.AmmoClips[1] = SharedAssets.clip_codex_3bk18m;

                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Failed, loading default ammo for " + vic.FriendlyName);
                }


                if (super_engine.Value)
                    {
                        VehicleController this_vic_controller = vic_go.GetComponent<VehicleController>();
                        NwhChassis chassis = vic_go.GetComponent<NwhChassis>();

                        Util.ShallowCopy(this_vic_controller.engine, SharedAssets.abrams_vic_controller.engine);
                        Util.ShallowCopy(this_vic_controller.transmission, SharedAssets.abrams_vic_controller.transmission);

                        this_vic_controller.engine.vc = vic_go.GetComponent<VehicleController>();
                        this_vic_controller.transmission.vc = vic_go.GetComponent<VehicleController>();
                        this_vic_controller.engine.Initialize(this_vic_controller);
                        this_vic_controller.engine.Start();
                        this_vic_controller.transmission.Initialize(this_vic_controller);

                        chassis._maxForwardSpeed = 22f;
                        chassis._maxReverseSpeed = 15.176f;
                        chassis._originalEnginePower = 1430.99f;
                    }

                    //if (has_drozd.Value)
                    //{
                    //    List<DrozdLauncher> launchers = new List<DrozdLauncher>();

                    //    Vector3[] launcher_positions = new Vector3[] {
                    //        new Vector3(-1.2953f, -0.1483f, 0.3166f),
                    //        new Vector3(-1.2243f, 0.0691f, 0.2969f),
                    //        new Vector3(1.2953f, -0.1483f, 0.3166f),
                    //        new Vector3(1.2243f, 0.0691f, 0.2969f),
                    //    };

                    //    Vector3[] launcher_rots = new Vector3[] {
                    //        new Vector3(0f, 0f, 0f),
                    //        new Vector3(0f, -17.8007f, 0f),
                    //        new Vector3(0f, 0f, 0f),
                    //        new Vector3(0f, 17.8007f, 0f)
                    //    };

                    //    for (var i = 0; i < launcher_positions.Length; i++)
                    //    {
                    //        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("---T64A_MESH---/HULL/TURRET"));
                    //        launcher.transform.localPosition = launcher_positions[i];
                    //        launcher.transform.localEulerAngles = launcher_rots[i];

                    //        if (i > 1)
                    //        {
                    //            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                    //        }

                    //        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                    //    }

                    //    Drozd.AttachDrozd(
                    //        vic.transform.Find("---T64A_MESH---/HULL/TURRET"), vic, new Vector3(0f, 0f, 9.5f),
                    //        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                    //    );

                    //    vic._friendlyName += "D";
                    //}

                    vic.AimablePlatforms[3].transform.Find("optic cover parent").gameObject.SetActive(false);

                    if (thermals.Value)
                    {
                        PactThermal.Add(day_optic.slot.LinkedNightSight.PairedOptic, thermals_quality.Value.ToLower());
                        vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                    }

                }
                yield return null;
            }
        public override void LoadStaticAssets()
        {
            if (!t64_patch.Value) return;

            string path = Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t64bv_full");

            AssetBundle t64bv_bundle = AssetBundle.LoadFromFile(path);

            if (t64bv_bundle == null)
            {
                MelonLogger.Error("FAILED TO LOAD T64BV ASSETBUNDLE");
                return;
            }

            T64B.t64bv_full = t64bv_bundle.LoadAsset<GameObject>("t64bv_full");

            if (T64B.t64bv_full == null)
            {
                MelonLogger.Error("Could not find t64bv_full inside bundle");
                return;
            }

            T64B.t64bv_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Transform hull_k1 = T64B.t64bv_full.transform.Find("HULL K1/ARMOUR");
            Transform turret_k1 = T64B.t64bv_full.transform.Find("TURRET K1/TURRET K1 ARMOUR");

            Kontakt1.Setup(hull_k1, hull_k1.parent);
            Kontakt1.Setup(turret_k1, turret_k1.parent);

            Util.SetupFLIRShaders(T64B.t64bv_full);

            MelonLogger.Msg("Loaded T64BV ERA asset successfully");
        }

        public static void Init()
        {
            if (!t64_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
