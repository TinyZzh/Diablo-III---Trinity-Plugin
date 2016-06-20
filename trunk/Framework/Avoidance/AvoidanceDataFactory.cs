﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Framework.Avoidance.Handlers;
using Trinity.Framework.Avoidance.Structures;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Framework.Avoidance
{
    /// <summary>
    /// Contains the definition of all types of avoidance.
    /// </summary>
    public class AvoidanceDataFactory
    {
        internal static List<AvoidanceData> AvoidanceData = new List<AvoidanceData>();

        internal static ILookup<SNOAnim, AvoidancePart> LookupPartByAnimation { get; set; }

        public static readonly Dictionary<int, AvoidancePart> AvoidanceDataDictionary = new Dictionary<int, AvoidancePart>();

        public static bool TryCreateAvoidance(List<TrinityCacheObject> actors, TrinityCacheObject actor, out Structures.Avoidance avoidance)
        {
            avoidance = null;

            var data = GetAvoidanceData(actor);
            if (data == null)
                return false;

            avoidance = new Structures.Avoidance
            {
                Data = data,
                CreationTime = DateTime.UtcNow,
                StartPosition = actor.Position,
                Actors = new List<TrinityCacheObject> { actor },
                IsImmune = TrinityPlugin.Player.ElementImmunity.Contains(data.Element)
            };

            return true;
        }

        static AvoidanceDataFactory()
        {
            CreateData();
            CreateUtils();
        }

        private static void CreateData()
        {
            //ClientEffect Name: ZK_tornado_model-468962 ActorSnoId: 186055, Distance: 22.96034

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Zultun Kule Tornado",
                IsEnabledByDefault = true,
                Element = Element.Physical,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Tornado",
                        ActorSnoId = (int)SNOActor.ZK_tornado_model,
                        Type = PartType.Main,
                        Radius = 12f,
                    }
                }
            });

            /*
            [Trinity 2.41.122] -- Dumping Attribtues for X1_LR_Boss_TerrorDemon_A-12271 (Sno=360636 Ann=-2012478880) at <834.7147, 615.7178, 0.2362415> ----
            [Trinity 2.41.122] Attributes (21): 
             867: PowerBuff2VisualEffectB (-3229) [ PowerSnoId: DemonHunter_Multishot: 77649 ] i:0 f:0 Value=0 IsValid=True 
             859: PowerBuff1VisualEffectNone (-3237) [ PowerSnoId: DemonHunter_Passive_ThrillOfTheHunt: 211225 ] i:0 f:0 Value=0 IsValid=True 
             859: PowerBuff1VisualEffectNone (-3237) [ PowerSnoId: ItemPassive_Unique_Gem_002U_x1: 403457 ] i:0 f:0 Value=0 IsValid=True 
             124: HitpointsMaxTotal (-3972) i:0 f:3.80403E+11 Value=380403000000 IsValid=True 
             122: HitpointsMax (-3974) i:0 f:3.80403E+11 Value=380403000000 IsValid=True 
             119: HitpointsTotalFromLevel (-3977) i:0 f:0 Value=0 IsValid=True 
             115: HitpointsCur (-3981) i:0 f:3.401741E+11 Value=340174100000 IsValid=True 
             1386: IsLootRunBoss (-2710) i:1 f:0 Value=1 IsValid=True 
             105: Invulnerable (-3991) i:0 f:0 Value=0 IsValid=True 
             359: ProjectileSpeed (-3737) i:0 f:0 Value=0 IsValid=True 
             1368: BountyObjective (-2728) i:1 f:0 Value=1 IsValid=True 
             585: BuffVisualEffect (-3511) i:1 f:0 Value=1 IsValid=True 
             1091: MinimapActive (-3005) i:1 f:0 Value=1 IsValid=True 
             317: Slow (-3779) i:0 f:0 Value=0 IsValid=True 
             57: Level (-4039) i:70 f:0 Value=70 IsValid=True 
             305: Chilled (-3791) i:0 f:0 Value=0 IsValid=True 
             300: Stunned (-3796) i:0 f:0 Value=0 IsValid=True 
             554: LastDamageACD (-3542) i:-2017590488 f:0 Value=-2017591000 IsValid=True 
             292: Untargetable (-3804) i:0 f:0 Value=0 IsValid=True 
             800: UsingBossbar (-3296) i:1 f:0 Value=1 IsValid=True 
             524: SummonerId (-3572) i:-2012478880 f:0 Value=-2012479000 IsValid=True 

            [Trinity 2.41.122] SelectedItem changed from  to X1_LR_Boss_TerrorDemon_A_BreathMinion, Type=Unit Dist=26.11438 IsBossOrEliteRareUnique=True IsAttackable=True

            [Trinity 2.41.122][Animation] TerrorDemon_attack_FireBreath State=Transform By: X1_LR_Boss_TerrorDemon_A_BreathMinion (429010)

            [Trinity 2.41.122] SelectedItem changed from  to x1_LR_boss_terrorDemon_A_projectile-12931, Type=Unknown Dist=0 IsBossOrEliteRareUnique=False IsAttackable=False

             */

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Orlash (Boss)",
                IsEnabledByDefault = true,
                Element = Element.Lightning,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Breathe Lightning",
                        ActorSnoId = (int)SNOActor.x1_LR_boss_terrorDemon_A_projectile,
                        Type = PartType.Projectile,
                        Radius = 9f,
                    }
                }
            });

            //17:43:34.477 INFO  MainWindow [1A6061A4] Type: Monster Name: X1_LR_Boss_westmarchBrute-75354 ActorSnoId: 353874, Distance: 12.13973
            //17:43:34.477 INFO  MainWindow [1A6058FC] Type: ServerProp Name: p2_westmarchBrute_leap_telegraph-76444 ActorSnoId: 428962, Distance: 12.43738
            //17:43:34.477 INFO  MainWindow [1A5F9A8C] Type: Monster Name: Generic_Proxy-76443 ActorSnoId: 4176, Distance: 12.43738
            //17:43:34.478 INFO  MainWindow [1A614708] Type: ClientEffect Name: x1_westmarchBrute_B_leap_trailActor-76453 ActorSnoId: 332634, Distance: 14.03841
            //17:42:59.069 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_B_attack_06_out State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:00.185 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_B_attack_01 State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:01.539 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_idle_01 State=Idle By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:01.643 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_run_01 State=Running By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:03.256 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_taunt State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:04.820 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_B_attack_02_in State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:05.511 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_attack_02_mid State=Running By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:05.699 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_attack_02_out State=Attacking By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:16.950 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_B_attack_06_in State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //17:43:17.683 INFO  Logger [Trinity 2.41.47][Animation] x1_westmarchBrute_B_attack_06_mid State=Transform By: X1_LR_Boss_westmarchBrute (353874)
            //[1A603208] Type: ServerProp Name: x1_westmarchBrute_bladeFX_model-75675 ActorSnoId: 291331, Distance: 24.00573



            // x1_MonsterAffix_Thunderstorm_Impact

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Thunderstorm",
                IsEnabledByDefault = true,
                Element = Element.Lightning,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Pulse",
                        ActorSnoId = (int)SNOActor.x1_MonsterAffix_Thunderstorm_Impact,
                        Radius = 20f,
                        Delay = TimeSpan.FromSeconds(1),
                        Duration = TimeSpan.FromSeconds(4),
                        Type = PartType.Main,
                    }
                }
            });

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Herald of Pestilence",
                IsEnabledByDefault = true,
                Element = Element.Poison,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Hands",
                        ActorSnoId = (int)SNOActor.creepMobArm,
                        Radius = 12f,
                        Type = PartType.Main,
                    }
                }
            });

            // monsterAffix_Desecrator_damage_AOE
            // monsterAffix_Desecrator_telegraph

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Desecrator",
                IsEnabledByDefault = true,
                Element = Element.Fire,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Desecrator AOE",
                        ActorSnoId = (int)SNOActor.monsterAffix_Desecrator_damage_AOE,
                        Radius = 6f,
                        Type = PartType.Main,
                    }
                }
            });

            //ActorId: 159367, Type: ServerProp, Name: MorluSpellcaster_Meteor_afterBurn
            //ActorId: 159368, Type: ServerProp, Name: MorluSpellcaster_Meteor_Impact
            //ActorId: 159369, Type: ServerProp, Name: MorluSpellcaster_Meteor_Pending

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Morlu Meteor",
                IsEnabledByDefault = true,
                Element = Element.Fire,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Morlu Meteor Pending",
                        ActorSnoId = (int)SNOActor.MorluSpellcaster_Meteor_Pending,
                        Radius = 20f,
                        Type = PartType.Main,
                    },
                }
            });

            //[22017F4C] Type: ClientEffect Name: x1_sniperAngel_shardBolt_orb-3273 ActorSnoId: 333688, Distance: 20.05622
            // 
            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Exarch Lightning Storm",
                IsEnabledByDefault = false,
                Element = Element.Lightning,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Lightning Storm",
                        ActorSnoId = (int)SNOActor.x1_sniperAngel_shardBolt_orb,
                        Radius = 18f,
                        Type = PartType.Main,
                    },
                }
            });

            // Raizeil GR Boss
            //[206427B0] Type: Monster Name: Generic_Proxy-34948 ActorSnoId: 4176, Distance: 0
            //[206725C4] Type: ClientEffect Name: � ActorSnoId: 255720, Distance: 6.180007
            //[2063E270] Type: Monster Name: Generic_Proxy-34955 ActorSnoId: 4176, Distance: 7.551051
            //[2064B684] Type: Monster Name: Generic_Proxy-34967 ActorSnoId: 4176, Distance: 7.551051
            //[20654558] Type: ServerProp Name: g_ChargedBolt_Projectile-34975 ActorSnoId: 4394, Distance: 14.51281
            // Generic proxy probably used for a lot of things, need to find unique id or check only in the presence of this actor. 

            //AvoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Raizeil Thunderstorm",
            //    IsEnabledByDefault = false,
            //    Element = Element.Poison,
            //    Handler = new CircularAvoidanceHandler(),
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "Thunderstorm",
            //            ActorSnoId = (int)SNOActor.Generic_Proxy,
            //            Radius = 20f,
            //            Type = PartType.Main,
            //        },
            //    }
            //});

            //[1FACC478] Type: ServerProp Name: Gluttony_gasCloud_proxy-73081 ActorSnoId: 93837, Distance: 42.44696

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Ghom Gas Cloud",
                IsEnabledByDefault = false,
                Element = Element.Poison,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Gas Cloud",
                        ActorSnoId = (int)SNOActor.Gluttony_gasCloud_proxy,
                        Radius = 20f,
                        Type = PartType.Main,
                    },
                }
            });

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Molten Core",
                IsEnabledByDefault = true,
                Affix = MonsterAffixes.Molten,
                Element = Element.Fire,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Telegraph",
                        InternalName = "monsterAffix_Molten_deathStart_Proxy",
                        ActorSnoId = 4803,
                        Radius = 20f,
                        Duration = TimeSpan.FromSeconds(3),
                        Severity = Severity.Extreme,
                        Type = PartType.Telegraph
                    },
                    new AvoidancePart
                    {
                        Name = "Explosion",
                        InternalName = "monsterAffix_Molten_deathExplosion_Proxy",
                        ActorSnoId = 4804,
                        Radius = 20f,
                        Delay = TimeSpan.FromSeconds(3),
                        Duration = TimeSpan.FromSeconds(1),
                        Severity = Severity.Extreme,
                        Type = PartType.Main,
                    },
                    new AvoidancePart
                    {
                        Name = "Explosion",
                        InternalName = "monsterAffix_molten_fireRing",
                        ActorSnoId = 224225,
                        Radius = 20f,
                        Delay = TimeSpan.FromSeconds(3),
                        Duration = TimeSpan.FromSeconds(1),
                        Severity = Severity.Extreme,
                        Type = PartType.Main,
                    }
                }
            });

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Fire Chains",
                Affix = MonsterAffixes.FireChains,
                Handler = new FireChainsAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Fire Chain Actor",
                        Affix = MonsterAffixes.FireChains,
                        Duration = TimeSpan.FromSeconds(3),
                        Type = PartType.ActorAffix,
                    },
                }
            });

            //Tethrys ActorId: 359688, Type: Monster, Name: X1_LR_Boss_Succubus_A-17034, 
            //X1_Unique_Monster_Generic_Projectile_Fire - 17346
            //ActorId: 315366, Type: ServerProp, Name: x1_Adria_Geyser - 17939, Distance2d: 6.132203, CollisionRadius: 0, MinimapActive: 0, MinimapIconOverride: -1, MinimapDisableArrow: 0
            //ActorId: 4176, Type: Monster, Name: Generic_Proxy - 17932, Distance2d: 4.620997, CollisionRadius: 0, MinimapActive: 0, 
            //ActorId: 315362, Type: ServerProp, Name: x1_Adria_Geyser_Pending - 17931, Distance2d: 48.03704, CollisionRadius: 0, MinimapActive: 0, MinimapIconOverride: -1, MinimapDisableArrow: 0

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Rift Boss: Tethrys",
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Cold,
                Parts = new List<AvoidancePart>
                {
                    //new AvoidancePart
                    //{
                    //    Name = "Projectile",
                    //    ActorSnoId = 4176, // dont use generic proxy
                    //    Radius = 5f,                        
                    //    Severity = Severity.Minor,
                    //    Type = PartType.Projectile,
                    //},
                    new AvoidancePart
                    {
                        Name = "Satanic Symbol 1",
                        ActorSnoId = 315366,
                        InternalName = "x1_Adria_Geyser",
                        Radius = 12f,
                        Type = PartType.Main,
                    },
                    new AvoidancePart
                    {
                        Name = "Satanic Symbol 2",
                        ActorSnoId = 315362,
                        InternalName = "x1_Adria_Geyser_Pending",
                        Type = PartType.Main,
                    },
                }
            });


            //ActorId: 223675, Type: ServerProp, Name: monsterAffix_frozen_iceClusters - 4487, 

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Frozen",
                Affix = MonsterAffixes.Frozen,
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Cold,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Ice Cluster",
                        ActorSnoId = (int) SNOActor.monsterAffix_frozen_iceClusters, //223675,
                        Radius = 16f,
                        Duration = TimeSpan.FromSeconds(4),
                        Type = PartType.Main,
                    },
                }
            });

            // Unit a3_Battlefield_demonic_forge-29378 (174900) has gained BuffVisualEffect (i:1 f:00.00000)
            // Unit a3_Battlefield_demonic_forge-29378 has a3_battlefield_demonic_forge(PowerBuff0VisualEffectNone)
            // ActorId: 185391, Type: Monster, Name: a3_crater_st_demonic_forge-875, 

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Furnace",
                Handler = new FurnaceAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Forge Fire Breath 1",
                        ActorSnoId = (int) SNOActor.a3_Battlefield_demonic_forge, //174900,         
                        Attribute = ActorAttributeType.PowerBuff0VisualEffectNone,
                        Power = SNOPower.a3_battlefield_demonic_forge,
                        Type = PartType.VisualEffect,
                    },
                    new AvoidancePart
                    {
                        Name = "Forge Fire Breath 2",
                        ActorSnoId = (int) SNOActor.a3_crater_st_demonic_forge, //185391,
                        Attribute = ActorAttributeType.PowerBuff0VisualEffectNone,
                        Power = SNOPower.a3_battlefield_demonic_forge,
                        Type = PartType.VisualEffect,
                    },
                }
            });

            //Line 8: [1F490598] Type: ServerProp Name: X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot-211250 ActorSnoId: 359693, Distance: 26.54131
            //Line 259: [1F4A52DC] Type: ClientEffect Name: X1_Unique_Monster_Generic_AOE_Sphere_Distortion-211338 ActorSnoId: 358917, Distance: 2.384186E-07
            //Line 260: [1F472980] Type: ServerProp Name: X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot-211337 ActorSnoId: 359693, Distance: 2.384186E-07
            //Line 271: [1F490598] Type: ServerProp Name: X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot-211250 ActorSnoId: 359693, Distance: 29.45315

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Pentagram",
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Butcher Pentagram",
                        ActorSnoId = (int)SNOActor.X1_Unique_Monster_Generic_AOE_DOT_Fire_10foot, //359693,    
                        Radius = 12f,
                        Type = PartType.Main,
                    },
                    new AvoidancePart
                    {
                        Name = "Butcher Pentagram Telegraph", // ClientEffect
                        ActorSnoId = (int)SNOActor.X1_Unique_Monster_Generic_AOE_Sphere_Distortion, //359693,    
                        Radius = 12f,
                        Type = PartType.Telegraph,
                    },
                }
            });

            //// Fat guys that explode into worms
            //// Stitch_Suicide_Bomb State=Transform By: Corpulent_C (3849)
            //new DoubleInt((int)SNOActor.Corpulent_A, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_A_Unique_01, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_A_Unique_02, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_A_Unique_03, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_B, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_B_Unique_01, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_C, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_D_CultistSurvivor_Unique, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_C_OasisAmbush_Unique, (int)SNOAnim.Stitch_Suicide_Bomb),
            //new DoubleInt((int)SNOActor.Corpulent_D_Unique_Spec_01, (int)SNOAnim.Stitch_Suicide_Bomb),

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Corpulent / Grotesque",
                Handler = new AnimationCircularAvoidanceHandler(),
                Element = Element.Physical,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Death Explosion",
                        Animation = SNOAnim.Stitch_Suicide_Bomb,
                        Type = PartType.ActorAnimation,      
                        Radius = 26f
                    },
                }
            });

            // Anims with charge_ in the name without obvious player anims
            //(int)SNOAnim.AssaultBeast_Land_attack_BullCharge_in, //7287
            //(int)SNOAnim.AssaultBeast_Land_attack_BullCharge_middle, //7288
            //(int)SNOAnim.AssaultBeast_Land_attack_BullCharge_out, //7289
            //(int)SNOAnim.Beast_charge_02, //7755
            //(int)SNOAnim.Beast_charge_04, //7756
            //(int)SNOAnim.Beast_start_charge_02, //7782
            //(int)SNOAnim.DeSoto_Run_Charge_Attack, //8270
            //(int)SNOAnim.Templar_1HT_sheild_charge_attack, //9537
            //(int)SNOAnim.Templar_1HT_sheild_charge_run, //9538
            //(int)SNOAnim.Templar_HTH_sheild_charge_attack, //9562
            //(int)SNOAnim.Templar_HTH_sheild_charge_run, //9563
            //(int)SNOAnim.Butcher_Attack_Charge_01_in, //86159
            //(int)SNOAnim.Templar_1HT_sheild_charge_windup, //87470
            //(int)SNOAnim.Templar_HTH_sheild_charge_windup, //87471
            //(int)SNOAnim.BigRed_charge_01, //134869
            //(int)SNOAnim.Diablo_charge_attack, //194361
            //(int)SNOAnim.Butcher_Attack_Charge_01_in_knockback, //194439
            //(int)SNOAnim.TentacleHorse_Charge_Loop_01, //213335
            //(int)SNOAnim.TentacleHorse_Charge_Loop_Outro_01, //213336
            //(int)SNOAnim.TentacleHorse_Charge_Intro_01, //213337
            //(int)SNOAnim.Templar_1HS_sheild_charge_windup, //216104
            //(int)SNOAnim.Templar_1HS_sheild_charge_attack, //216112
            //(int)SNOAnim.Templar_1HS_sheild_charge_run, //216113
            //(int)SNOAnim.x1_BogFamily_brute_charge_end_01, //238829
            //(int)SNOAnim.x1_BogFamily_brute_charge_intro_01, //238830
            //(int)SNOAnim.x1_BogFamily_brute_charge_loop_01, //238831
            //(int)SNOAnim.x1_scaryEyes_attack_charge_in, //255564
            //(int)SNOAnim.x1_scaryEyes_attack_charge_mid, //255565
            //(int)SNOAnim.x1_scaryEyes_attack_charge_out, //255566
            //(int)SNOAnim.x1_Wraith_attack_05_charge_in, //265860
            //(int)SNOAnim.x1_Wraith_attack_05_charge_mid, //265861
            //(int)SNOAnim.x1_Wraith_attack_05_charge_out, //265862
            //(int)SNOAnim.FloaterDemon_attack_charge_01, //302095
            //(int)SNOAnim.FloaterDemon_attack_charge_out_01, //302097
            //(int)SNOAnim.FloaterDemon_attack_charge_in_01, //302103
            //(int)SNOAnim.x1_Skeleton_Westmarch_run_charge_01, //314784
            //(int)SNOAnim.x1_portalGuardianMinion_attack_charge_01, //321853
            //(int)SNOAnim.x1_portalGuardianMinion_attack_charge_tumble, //327413
            //(int)SNOAnim.x1_portalGuardianMinion_attack_charge_recover, //327416
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Barricade_dead, //351028
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Barricade_death, //351029
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Barricade_idle, //351030
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Skybox_B_Rubble_idle, //351408
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Skybox_B_Rubble_opening, //351409
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Skybox_B_Rubble_open, //351410
            //(int)SNOAnim.x1_TemplarOrder_1HT_sheild_charge_attack, //362401
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Barricade_Client_idle, //362789
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_dead, //363246
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_death, //363247
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_idle, //363248
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_Chain_idle, //364241
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_Chain_dead, //364244
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_Chain_death, //364245
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_Chain_Client_A_idle, //364299
            //(int)SNOAnim.x1_Pand_Ext_ImperiusCharge_Towers_Chain_Client_B_idle, //364360
            //(int)SNOAnim.X1_BigRed_charge_01, //365666
            //(int)SNOAnim.p1_Greed_Attack_Charge_01_in, //382686
            //(int)SNOAnim.p1_Greed_BreakFree_charge_01, //392700
            //(int)SNOAnim.p1_Greed_charge_1Frame, //397118
            //(int)SNOAnim.P4_BigRed_attack_DoublePunch_Charge_in, //431298
            //(int)SNOAnim.P4_GoatWarrior_attack_03_spear_charge_throw, //437659
            //(int)SNOAnim.Barbarian_Female_1HS_Furious_Charge_Loop_Cannibal, //437861
            //(int)SNOAnim.Barbarian_Female_1HS_Furious_Charge_End_Cannibal, //437862

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Charge Animations",
                Handler = new AnimationBeamAvoidanceHandler(),                
                Element = Element.Physical,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Assault Beast Charge",
                        Animation = SNOAnim.AssaultBeast_Land_attack_BullCharge_in,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Assault Beast Charge",
                        Animation = SNOAnim.AssaultBeast_Land_attack_BullCharge_middle,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Assault Beast Charge",
                        Animation = SNOAnim.AssaultBeast_Land_attack_BullCharge_out,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Beast Charge",
                        Animation = SNOAnim.Beast_start_charge_02,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Beast Charge",
                        Animation = SNOAnim.Beast_charge_04,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Beast Charge",
                        Animation = SNOAnim.Beast_charge_02,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Diablo Charge",
                        Animation = SNOAnim.Diablo_charge_attack,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Diablo Charge",
                        Animation = SNOAnim.Beast_start_charge_02,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Diablo Charge",
                        Animation = SNOAnim.Diablo_charge_attack,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },                    
                    new AvoidancePart
                    {
                        Name = "Butcher Charge",
                        Animation = SNOAnim.Butcher_Attack_Charge_01_in,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Butcher Charge",
                        Animation = SNOAnim.Butcher_Attack_Charge_01_in_knockback,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Tentacle Horse",
                        Animation = SNOAnim.TentacleHorse_Charge_Loop_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Tentacle Horse",
                        Animation = SNOAnim.TentacleHorse_Charge_Loop_Outro_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Tentacle Horse",
                        Animation = SNOAnim.TentacleHorse_Charge_Intro_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Wraith",
                        Animation = SNOAnim.x1_Wraith_attack_05_charge_in,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Wraith",
                        Animation = SNOAnim.x1_Wraith_attack_05_charge_mid,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Wraith",
                        Animation = SNOAnim.x1_Wraith_attack_05_charge_out,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Floater Demon",
                        Animation = SNOAnim.FloaterDemon_attack_charge_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Floater Demon",
                        Animation = SNOAnim.FloaterDemon_attack_charge_in_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Floater Demon",
                        Animation = SNOAnim.FloaterDemon_attack_charge_in_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Big Red Charge",
                        Animation = SNOAnim.X1_BigRed_charge_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Big Red Charge",
                        Animation = SNOAnim.BigRed_charge_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },                    
                    new AvoidancePart
                    {
                        Name = "Greed Boss Charge",
                        Animation = SNOAnim.p1_Greed_Attack_Charge_01_in,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                    new AvoidancePart
                    {
                        Name = "Greed Boss Charge",
                        Animation = SNOAnim.p1_Greed_BreakFree_charge_01,
                        Type = PartType.ActorAnimation,
                        Radius = 40f
                    },
                }
            });

            //Line 33558: 15:14:39.491 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_idle_01 State=Idle By: x1_LR_Boss_MalletDemon (343767)
            //Line 33579: 15:14:40.446 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_run_01 State=Running By: x1_LR_Boss_MalletDemon (343767)
            //Line 33624: 15:14:42.916 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_spawn_from_pod_01 State=Transform By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33634: 15:14:43.170 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_generic_cast State=Transform By: x1_LR_Boss_MalletDemon (343767)
            //Line 33653: 15:14:43.752 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_idle_01 State=Idle By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33669: 15:14:44.254 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_Pod_spawn_01 State=Transform By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33813: 15:14:47.874 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_taunt_01 State=Transform By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33885: 15:14:49.397 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_taunt_01 State=Transform By: x1_LR_Boss_MalletDemon (343767)
            //Line 33917: 15:14:50.121 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_run_01 State=Running By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33948: 15:14:51.099 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_attack_01 State=Transform By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 33962: 15:14:51.405 INFO  Logger [Trinity 2.41.51][Animation] coreEliteDemon_walk_01 State=Running By: CoreEliteDemon_B_LR_Boss (369412)
            //Line 34017: 15:14:52.791 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_attack_01 State=Transform By: x1_LR_Boss_MalletDemon (343767)
            //Line 34149: 15:14:56.371 INFO  Logger [Trinity 2.41.51][Animation] malletDemon_stunned State=TakingDamage By: x1_LR_Boss_MalletDemon (343767)
            //x1_LR_Boss_MalletDemon_FallingRocks 368453,
            //todo Parendi / malletdemon avoidance - he hits like a truck.


            // Beast Creatures with a small guy riding on them, stun the player.
            //[Trinity 2.41.50][Weight] Found New Target MastaBlasta_Combined_A dist=6 IsElite=False Radius=11.0 Weight=1099 ActorSnoId=137856 AnimSnoId=MastaBlasta_Combined_taunt TargetedCount=26 Type=Unit 
            //MastaBlasta_Combined_attack_stomp 

            //AvoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Stun/Paralyse Attacks",
            //    Handler = new AnimationConeAvoidanceHandler(),                
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "MastaBlasta Stun",
            //            ActorSnoId = (int)SNOAnim.MastaBlasta_Combined_attack_stomp,
            //            Radius = 10f,
            //            Type = PartType.Main,
            //        },
            //    }
            //});

            //[19147660] Type: ClientEffect Name: p4_ratKing_ratBall_model-47703 ActorSnoId: 427100, Distance: 19.72662
            //p4_RatKing_RatBallMonster, Type=Unit Dist=24.44967 IsBossOrEliteRareUnique=False IsAttackable=True

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Rat King",
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Poison,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Rat Ball",
                        ActorSnoId = (int)SNOActor.p4_RatKing_RatBallMonster,
                        Radius = 10f,
                        Type = PartType.Main,
                    },
                }
            });

            ////x1_MonsterAffix_Thunderstorm_Impact 341512,

            //AvoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Thunderstorm",
            //    Handler = new CircularAvoidanceHandler(),
            //    Element = Element.Fire,
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "Thunderstorm Impact",
            //            ActorSnoId = (int)SNOActor.MorluSpellcaster_Meteor_Pending,
            //            Radius = 14f,
            //            Type = PartType.Main,
            //        },
            //        new AvoidancePart
            //        {
            //            Name = "Thunderstorm Impact",
            //            ActorSnoId = (int)SNOActor.MorluSpellcaster_Meteor_Pending,
            //            Radius = 14f,
            //            Type = PartType.Telegraph,
            //        },
            //    }
            //});

            ////[1A5FBD2C] Type: ServerProp Name: MorluSpellcaster_Meteor_Pending-17716 ActorSnoId: 159369, Distance: 0
            ////[1A61DA30] Type: ServerProp Name: MorluSpellcaster_Meteor_afterBurn-31703 ActorSnoId: 159367

            //AvoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Meteor",
            //    Handler = new CircularAvoidanceHandler(),
            //    Element = Element.Fire,
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "Meteor Telegraph",
            //            ActorSnoId = (int)SNOActor.MorluSpellcaster_Meteor_Pending,
            //            Radius = 18f,
            //            Type = PartType.Telegraph,
            //        },
            //    }
            //});

            // monsterAffix_Plagued_endCloud = 108869,
            // Zombie_Plagued_C_Unique = 111321,
            // monsterAffix_plagued_groundGeo = 223933,
            // X1_Plagued_LacuniMale_A = 349601,
            // X1_Plagued_LacuniMale_Unique_A = 361088,
            // X1_Plagued_LacuniMale_Unique_B = 361099,
            // x1_Lacuni_male_plagued_swipeRight = 365732,
            // x1_Lacuni_male_plagued_swipeLeft = 365733,
            // x1_Lacuni_male_plagued_comboSwipe4 = 365745,
            // X1_Plagued_LacuniMale_Event_RatAlley = 367485,
            // x1_lacuniMale_plagued_summon_castRat = 374347,

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Plagued",
                Affix = MonsterAffixes.Plagued,
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Poison,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Swirly Poison Pool A",
                        ActorSnoId = 108869,
                        Radius = 12f,
                        Type = PartType.Main,
                    },
                    new AvoidancePart
                    {
                        Name = "Swirly Poison Pool B",
                        ActorSnoId = 223933,
                        Radius = 12f,
                        Type = PartType.Main,
                    },
                }
            });

            //ActorId: 349774, Type: Monster, Name: x1_MonsterAffix_frozenPulse_monster-17780, Distance2d: 41.06812
            //Unit x1_MonsterAffix_frozenPulse_monster-5812 has X1_MonsterAffix_LightningStormCast(PowerBuff1VisualEffectNone)
            //Unit x1_MonsterAffix_frozenPulse_monster-5812 has X1_MonsterAffix_LightningStorm_Pulse(PowerBuff0VisualEffectNone)

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "FrozenPulse",
                Affix = MonsterAffixes.FrozenPulse,
                Element = Element.Cold,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Swirly Poison Pool A",
                        ActorSnoId = 349774,
                        Radius = 12f,
                        Type = PartType.Main,
                    },
                }
            });

            //if (monster.MonsterAffixes.Contains(TrinityMonsterAffix.FireChains) && monster.MonsterQualityLevel == MonsterQuality.Minion)
            //    Telegraph.TelegraphFireChains(monster);

            //_avoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Thunderstorm",
            //    AffixGbId = 0,
            //    HandlerProducer = () => new CircularAvoidanceHandler(),
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "Telegraph",
            //            ActorSnoId = 0,
            //            Duration = TimeSpan.FromSeconds(1),
            //            Type = PartType.Telegraph
            //        },
            //        new AvoidancePart
            //        {
            //            Name = "Pulse",
            //            ActorSnoId = 0,
            //            Radius = 20f,
            //            Delay = TimeSpan.FromSeconds(1),
            //            Duration = TimeSpan.FromSeconds(4),
            //            Type = PartType.Main,
            //        }
            //    }
            //});

            //ActorId: 95868, Type: ServerProp, Name: monsterAffix_Molten_trail-21985

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Molten Trail",
                Affix = MonsterAffixes.Molten, //AffixGbId = 95868,
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Trail",
                        ActorSnoId = 95868,
                        Duration = TimeSpan.FromSeconds(1),
                        Type = PartType.Telegraph
                    },
                }
            });

            //ActorId: 257306, Type: ServerProp, Name: arcaneEnchantedDummy_spawn - 15126,
            //ActorId: 221225, Type: Monster, Name: MonsterAffix_ArcaneEnchanted_PetSweep_reverse - 15129,
            //ActorId: 219702, Type: Monster, Name: MonsterAffix_ArcaneEnchanted_PetSweep - 15166,

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Arcane Sentry",
                IsEnabledByDefault = true,
                Affix = MonsterAffixes.ArcaneEnchanted,
                Element = Element.Arcane,
                Handler = new ArcaneAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Telegraph",
                        ActorSnoId = 257306,
                        Duration = TimeSpan.FromSeconds(2),
                        Type = PartType.Telegraph
                    },
                    new AvoidancePart
                    {
                        Name = "Beam",
                        ActorSnoId = 219702,
                        //Delay = TimeSpan.FromSeconds(2),
                        Duration = TimeSpan.FromSeconds(10),
                        MovementType = MovementType.Rotation,
                        Type = PartType.Main,
                    },
                    new AvoidancePart
                    {
                        Name = "Beam Reverse Beam",
                        ActorSnoId = 221225,
                        //Delay = TimeSpan.FromSeconds(2),
                        Duration = TimeSpan.FromSeconds(10),
                        MovementType = MovementType.Rotation,
                        Type = PartType.Main,
                    }
                }
            });

            //Type: Projectile Name: UberMaghda_Punish_projectile-1409 ActorSnoId: 278340, Distance: 5.54583

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Maghda Projectile",
                Element = Element.Physical,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Projectile",
                        ActorSnoId = (int) SNOActor.UberMaghda_Punish_projectile, //278340,
                        InternalName = "UberMaghda_Punish_projectile",
                        Duration = TimeSpan.FromSeconds(10),
                        Type = PartType.Projectile,
                        MovementType = MovementType.Follow,
                    },
                }
            });

            //Projectile Name: X1_MonsterAffix_Orbiter_Projectile - 3943 ActorSnoId: 343539, Distance: 2.998138
            //Type: ClientEffect Name: x1_MonsterAffix_orbiter_projectile_orb-3944 ActorSnoId: 346805, Distance: 5.775279
            //ClientEffect Name: x1_MonsterAffix_orbiter_projectile_orb - 3948 ActorSnoId: 346805, Distance: 14.95078
            //Unit x1_MonsterAffix_orbiter_projectile_orb-10831 has X1_MonsterAffix_OrbiterChampionCast(PowerBuff0VisualEffectNone)
            //Unit x1_MonsterAffix_orbiter_projectile_orb-10831 has None(ProjectileDetonateTime) Value=44339

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Orbiter",
                IsEnabledByDefault = true,
                Element = Element.Lightning,
                Handler = new CircularAvoidanceHandler(),
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Orb1",
                        Radius = 10f,
                        ActorSnoId = (int) SNOActor.X1_MonsterAffix_Orbiter_Projectile, //343539,
                        InternalName = "X1_MonsterAffix_Orbiter_Projectile",
                        Duration = TimeSpan.FromSeconds(10),
                        Type = PartType.Projectile,
                    },
                    new AvoidancePart
                    {
                        Name = "Orb2",
                        Radius = 6f,
                        InternalName = "x1_MonsterAffix_orbiter_projectile_orb",
                        ActorSnoId = (int) SNOActor.x1_MonsterAffix_orbiter_projectile_orb, //346805,
                        Duration = TimeSpan.FromSeconds(10),
                        Type = PartType.Projectile,
                    },
                }
            });

            //ActorId: 432, Type: ServerProp, Name: skeletonMage_fire_groundPool-1029, Distance2d: 0.240312, 
            //ActorId: 5374, Type: Projectile, Name: skeletonMage_Fire_projectile-1435, 

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Mage Fire",
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Projectile",
                        Radius = 4f,
                        ActorSnoId = (int) SNOActor.skeletonMage_Fire_projectile, //5374,
                        InternalName = "skeletonMage_Fire_projectile",
                        Type = PartType.Projectile,
                    },
                    new AvoidancePart
                    {
                        Name = "Ground Effect",
                        Radius = 8f,
                        ActorSnoId = (int) SNOActor.skeletonMage_fire_groundPool, //432,
                        InternalName = "skeletonMage_fire_groundPool",
                        Duration = TimeSpan.FromSeconds(3),
                        Type = PartType.Main,
                    },
                }
            });

            //AvoidanceData.Add(new AvoidanceData
            //{
            //    Name = "Skeleton Archer Test",
            //    AffixGbId = 0,
            //    Handler = new CircularAvoidanceHandler(),
            //    Parts = new List<AvoidancePart>
            //    {
            //        new AvoidancePart
            //        {
            //            Name = "Actor",
            //            Radius = 14f,
            //            ActorSnoId = 5347,
            //            InternalName = "SkeletonArcher_B",
            //            Duration = TimeSpan.FromSeconds(2),
            //            Type = PartType.ActorAnimation,
            //        },
            //    }
            //});


            //Line 10390:     x1_MonsterAffix_CorpseBomber_projectile = 316389,
            //Line 10634:     X1_MonsterAffix_corpseBomber_bomb = 325761,
            //Line 11233:     x1_MonsterAffix_CorpseBomber_bomb_start = 340319,
            //Line 13788:     x1_MonsterAffix_Avenger_CorpseBomber_bomb_start = 384614,
            //Line 13789:     x1_MonsterAffix_Avenger_CorpseBomber_projectile = 384617,
            //Line 13844:     x1_MonsterAffix_Avenger_corpseBomber_slime = 389483,

            //Unit x1_MonsterAffix_CorpseBomber_projectile-15784 has X1_MonsterAffix_CorpseBomberCast(PowerBuff2VisualEffectNone)
            //Unit x1_MonsterAffix_CorpseBomber_projectile-15784 has X1_MonsterAffix_CorpseBomberCast(PowerBuff4VisualEffectNone)
            //Unit x1_MonsterAffix_CorpseBomber_projectile-12172 (316389) has gained ProjectileDetonateTime (i:46557 f:00.00000)
            //Unit x1_MonsterAffix_CorpseBomber_bomb_start-11590 (340319) has gained BuffVisualEffect (i:1 f:00.00000)
            //Unit x1_MonsterAffix_CorpseBomber_projectile-9797 (316389) has gained Hidden (i:1 f:00.00000)
            //Unit x1_MonsterAffix_CorpseBomber_projectile-9797 (316389) has gained BuffVisualEffect (i:1 f:00.00000)

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Poison Enchanted",
                Affix = MonsterAffixes.PoisonEnchanted,
                Handler = new PoisonEnchantedAvoidanceHandler(),
                Element = Element.Poison,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Slime Bomb Telegraph",
                        InternalName = "x1_MonsterAffix_CorpseBomber_bomb_start",
                        ActorSnoId = (int) SNOActor.x1_MonsterAffix_CorpseBomber_bomb_start, //340319,
                        Duration = TimeSpan.FromSeconds(2),
                        Type = PartType.Telegraph
                    },
                    new AvoidancePart
                    {
                        Name = "Slime Bomb",
                        ActorSnoId = (int) SNOActor.X1_MonsterAffix_corpseBomber_bomb, //325761,
                        InternalName = "X1_MonsterAffix_corpseBomber_bomb",
                        Type = PartType.Main
                    },
                    new AvoidancePart
                    {
                        Name = "Slime Bomb Projectile",
                        ActorSnoId = (int) SNOActor.x1_MonsterAffix_CorpseBomber_projectile, //316389,
                        InternalName = "x1_MonsterAffix_CorpseBomber_projectile",
                        Type = PartType.Projectile
                    },
                }
            });

            //[TrinityPlugin 2.14.34] Unit Grenadier_Proj_mortar_inpact-17615 has None (ProjectileDetonateTime) Value=134387

            AvoidanceData.Add(new AvoidanceData
            {
                Name = "Mortar",
                Affix = MonsterAffixes.Mortar,
                Handler = new CircularAvoidanceHandler(),
                Element = Element.Fire,
                Parts = new List<AvoidancePart>
                {
                    new AvoidancePart
                    {
                        Name = "Mortar Impact",
                        InternalName = "x1_MonsterAffix_CorpseBomber_bomb_start",
                        ActorSnoId = (int) SNOActor.Grenadier_Proj_mortar_inpact,
                        Type = PartType.Main
                    },
                }

            });
        }

        private static void CreateUtils()
        {
            var allParts = new List<AvoidancePart>();
                                
            foreach (var avoidanceDatum in AvoidanceData)
            {
                foreach (var part in avoidanceDatum.Parts)
                {
                    part.Parent = avoidanceDatum;
                    allParts.Add(part);
                    
                    try
                    {
                        if (part.ActorSnoId > 0)
                        {
                            AvoidanceDataDictionary.Add(part.ActorSnoId, part);
                        }
                    }
                    catch(Exception ex)                   
                    {
                        Logger.LogError("Failed to add AvoidanceData for {0} > {1}. Probably a duplicate ActorSnoId ({2})", avoidanceDatum.Name, part.Name, part.ActorSnoId);
                    }
                }
            }

            LookupPartByAnimation = allParts.Where(o => o.Animation != default(SNOAnim)).ToLookup(k => k.Animation, v => v);
        }

        public static AvoidanceData GetAvoidanceData(TrinityCacheObject actor)
        {
            if (actor == null)
                return null;

            AvoidanceData data;

            if (TryFindPartByActorId(actor, out data))
                return data;

            if (TryFindPartByAffix(actor, out data))
                return data;

            if (TryFindPartByAnimation(actor, out data))
                return data;

            return null;
        }

        private static bool TryFindPartByActorId(TrinityCacheObject actor, out AvoidanceData data)
        {
            data = null;

            if (actor == null || actor.ActorSNO <= 0)
                return false;

            var part = GetAvoidancePart(actor.ActorSNO);
            if (part != null)
            {            
                data = part.Parent;
                return true;                
            }
            return false;
        }

        private static bool TryFindPartByAffix(TrinityCacheObject actor, out AvoidanceData data)
        {
            data = null;

            if (actor.MonsterAffixes.HasFlag(MonsterAffixes.None))
                return false;

            var part = GetAvoidancePart(actor.MonsterAffixes);
            if (part != null)
            {
                data = part.Parent;
                return true;
            }        
            return false;
        }

        private static bool TryFindPartByAnimation(TrinityCacheObject actor, out AvoidanceData data)
        {
            data = null;

            if (actor.Animation == default(SNOAnim))
                return false;

            var part = GetAvoidancePart(actor.Animation);
            if (part != null)
            {
                data = part.Parent;
                return true;
            }
            return false;
        }

        public static AvoidancePart GetAvoidancePart(int actorId)
        {
            return AvoidanceDataDictionary.ContainsKey(actorId) ? AvoidanceDataDictionary[actorId] : null;
        }

        public static AvoidancePart GetAvoidancePart(MonsterAffixes affixes)
        {
            return AvoidanceDataDictionary.Values.FirstOrDefault(a => affixes.HasAny(a.Affix));
        }

        public static AvoidancePart GetAvoidancePart(SNOAnim actorAnimation)
        {
            return LookupPartByAnimation.Contains(actorAnimation) ? LookupPartByAnimation[actorAnimation].FirstOrDefault() : null;
        }

    }
}

