using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace Aristocrat
{
    static class Content
    {
        public static BlueprintCharacterClass Inquisitor;
        public static BlueprintBuff[] JudgmentBuffs;

        const string ArchetypeGuid = "c3a17a1100014e6a9b2c000000000001";
        const string JudgmentGuid = "c3a17a1100014e6a9b2c000000000002";
        const string WeaponFocusGuid = "c3a17a1100014e6a9b2c000000000003";
        const string GreaterWeaponFocusGuid = "c3a17a1100014e6a9b2c000000000004";
        const string RightToRuleGuid = "c3a17a1100014e6a9b2c000000000005";

        const string InquisitorGuid = "f1a70d9e1b0b41e49874e1fa9052a1ce";
        const string FaithHunterGuid = "ac87f40932cfd324c90b441f93857c40";
        const string FaithHunterJudgmentGuid = "19a36bd554ff4fd492d7458d4f922be4";
        const string BaseJudgmentGuid = "981def910b98200499c0c8f85a78bde8";
        const string TeamworkGuid = "d87e2f6a9278ac04caeb0f93eff95fcb";
        const string MartialGuid = "203992ef5b35c864390b4e4a1e200629";
        const string WeaponFocusFeatGuid = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
        const string GreaterWeaponFocusFeatGuid = "09c9e82965fb4334b984a1e9df3bd088";
        const string ProtectionAbilityGuid = "1bb08308c9f6a5e4697887cd438b7221";
        const string PurityAbilityGuid = "15d3b21354c105b498a0b1b293280ded";
        const string SmitingMagicFeatureGuid = "6b96f8db83d8e754cb0c1993c71c39d0";
        const string SmitingAlignmentFeatureGuid = "5d7ba4fad7dc44c4280c78f1fee0099d";
        const string TrueJudgmentGuid = "f069b6557a2013544ac3636219186632";
        const string EverlastingJudgmentGuid = "4a6dc772c9a7fe742a65820007107f03";

        static readonly string[] JudgmentBuffGuids =
        {
            "99345ce7c0f8001438fee23db660d039",
            "fbc7fe386bceb354494b05736381c238",
            "4594286e4a565cd41ad9475f6c903fda",
            "481b03bc6cbc5af448b1f6cb70d88859",
            "2e3f01df36b508b4e9186bab7a337dfa"
        };

        const string ArchetypeText =
            "Aristocrats belong to an order of historians and scholars who believe civilization advances through knowledge, order, and truth. "
            + "The right to rule is earned by wisdom, and they abhor the stupidity of the crowd.\n"
            + "An aristocrat follows the Faith Hunter, but judgment is limited to Purity, Protection, and Smiting. "
            + "Smiting begins as Smiting (Magic) and becomes Smiting (Alignment) at 6th level. "
            + "The aristocrat does not gain the teamwork feat selections at 15th and 18th level. "
            + "An aristocrat is proficient with martial weapons, chooses Weapon Focus at 1st level, and Greater Weapon Focus at 10th level. "
            + "At 4th level the aristocrat gains Right to Rule.";

        const string JudgmentText =
            "You can pronounce judgment on your foes. This aristocrat may use only three judgments: Purity, Protection, and Smiting. "
            + "Smiting is Smiting (Magic) until 6th level, when it becomes Smiting (Alignment).";

        const string WeaponFocusText =
            "Choose one weapon. You gain Weapon Focus with that weapon. You do not need to meet the usual prerequisites.";

        const string GreaterWeaponFocusText =
            "Choose one weapon. You gain Greater Weapon Focus with that weapon. You do not need the fighter level requirement.";

        const string RightToRuleText =
            "While one of your judgments is active, your claim to rule stands against the chaos of the crowd.\n"
            + "At 4th level, you gain a +2 insight bonus to AC against attacks from chaotic creatures.\n"
            + "At 8th level, you gain a +2 insight bonus on saving throws against chaotic creatures and chaotic effects.\n"
            + "At 12th level, you gain a +2 insight bonus on weapon attack rolls against chaotic creatures.\n"
            + "At 16th level, you add your Wisdom modifier to weapon damage rolls against chaotic creatures.";

        public static void Create()
        {
            Inquisitor = BlueprintUtil.Load<BlueprintCharacterClass>(InquisitorGuid);
            var faithHunter = BlueprintUtil.Load<BlueprintArchetype>(FaithHunterGuid);
            var faithJudgment = BlueprintUtil.Load<BlueprintFeature>(FaithHunterJudgmentGuid);
            if (Inquisitor == null || faithHunter == null || faithJudgment == null)
            {
                Main.Log("Aristocrat needs the Inquisitor and Faith Hunter.");
                return;
            }

            JudgmentBuffs = JudgmentBuffGuids
                .Select(guid => BlueprintUtil.Load<BlueprintBuff>(guid))
                .Where(buff => buff != null)
                .ToArray();

            if (HasArchetype(Inquisitor, ArchetypeGuid))
            {
                return;
            }

            var judgment = CreateJudgment(faithJudgment);
            var weaponFocus = CreateWeaponSelection(
                WeaponFocusGuid,
                "AristocratWeaponFocusSelection",
                "Aristocrat.WeaponFocus",
                "Weapon Focus",
                WeaponFocusText,
                WeaponFocusFeatGuid);
            var greaterFocus = CreateWeaponSelection(
                GreaterWeaponFocusGuid,
                "AristocratGreaterWeaponFocusSelection",
                "Aristocrat.GreaterWeaponFocus",
                "Greater Weapon Focus",
                GreaterWeaponFocusText,
                GreaterWeaponFocusFeatGuid);
            var rightToRule = CreateRightToRule(faithJudgment);
            var martial = BlueprintUtil.Load<BlueprintFeature>(MartialGuid);
            var teamwork = BlueprintUtil.Load<BlueprintFeatureSelection>(TeamworkGuid);

            var archetype = BuildArchetype(faithHunter);
            var added = BlueprintUtil.CloneEntries(faithHunter.AddFeatures);
            var removed = BlueprintUtil.CloneEntries(faithHunter.RemoveFeatures);
            Swap(added, FaithHunterJudgmentGuid, judgment);
            BlueprintUtil.AddAtLevel(added, 1, martial);
            BlueprintUtil.AddAtLevel(added, 1, weaponFocus);
            BlueprintUtil.AddAtLevel(added, 4, rightToRule);
            BlueprintUtil.AddAtLevel(added, 10, greaterFocus);
            BlueprintUtil.AddAtLevel(removed, 15, teamwork);
            BlueprintUtil.AddAtLevel(removed, 18, teamwork);
            archetype.AddFeatures = added.OrderBy(entry => entry.Level).ToArray();
            archetype.RemoveFeatures = removed.OrderBy(entry => entry.Level).ToArray();
            BlueprintUtil.Register(archetype);
            AddArchetype(Inquisitor, archetype);
            AcceptJudgment(BlueprintUtil.Load<BlueprintFeature>(TrueJudgmentGuid), judgment);
            AcceptJudgment(BlueprintUtil.Load<BlueprintFeature>(EverlastingJudgmentGuid), judgment);
            Main.Log("Aristocrat is ready.");
        }

        static BlueprintFeature CreateJudgment(BlueprintFeature faithJudgment)
        {
            var feature = BlueprintUtil.Create<BlueprintFeature>();
            feature.name = "AristocratJudgmentFeature";
            feature.AssetGuid = BlueprintGuid.Parse(JudgmentGuid);
            BlueprintUtil.SetName(feature, "Aristocrat.Judgment", "Judgment", JudgmentText);
            BlueprintUtil.CopyIcon(faithJudgment, feature);
            feature.IsClassFeature = true;
            feature.Ranks = 1;

            var components = new List<BlueprintComponent>();
            foreach (var component in faithJudgment.ComponentsArray ?? new BlueprintComponent[0])
            {
                if (component is AddFacts facts)
                {
                    var clone = (AddFacts)BlueprintUtil.CloneComponent(facts, feature);
                    var protection = BlueprintUtil.Load<BlueprintUnitFact>(ProtectionAbilityGuid);
                    var purity = BlueprintUtil.Load<BlueprintUnitFact>(PurityAbilityGuid);
                    BlueprintUtil.Set(clone, "m_Facts", new[]
                    {
                        protection.ToReference<BlueprintUnitFactReference>(),
                        purity.ToReference<BlueprintUnitFactReference>()
                    });
                    components.Add(clone);
                    continue;
                }

                if (component is AddAbilityResources)
                {
                    components.Add(BlueprintUtil.CloneComponent(component, feature));
                    continue;
                }

                if (Same(FeatureGuid(component), SmitingMagicFeatureGuid) || Same(FeatureGuid(component), SmitingAlignmentFeatureGuid))
                {
                    components.Add(BlueprintUtil.CloneComponent(component, feature));
                }
            }

            BlueprintUtil.Set(feature, "Components", components.ToArray());
            BlueprintUtil.Register(feature);
            Main.Log("Aristocrat judgment keeps " + components.Count + " components.");
            return feature;
        }

        static BlueprintFeatureSelection CreateWeaponSelection(string guid, string assetName, string key, string title, string description, string featGuid)
        {
            var feat = BlueprintUtil.Load<BlueprintFeature>(featGuid);
            var selection = BlueprintUtil.Create<BlueprintFeatureSelection>();
            selection.name = assetName;
            selection.AssetGuid = BlueprintGuid.Parse(guid);
            BlueprintUtil.SetName(selection, key, title, description);
            BlueprintUtil.CopyIcon(feat, selection);
            selection.IsClassFeature = true;
            selection.Ranks = 1;
            selection.IgnorePrerequisites = true;
            selection.Mode = SelectionMode.Default;
            selection.Group = FeatureGroup.None;
            selection.Group2 = FeatureGroup.None;
            var reference = feat.ToReference<BlueprintFeatureReference>();
            BlueprintUtil.Set(selection, "m_Features", new[] { reference });
            BlueprintUtil.Set(selection, "m_AllFeatures", new[] { reference });
            BlueprintUtil.Set(selection, "ShowThisSelection", true);
            BlueprintUtil.Set(selection, "Components", new BlueprintComponent[0]);
            BlueprintUtil.Register(selection);
            return selection;
        }

        static BlueprintFeature CreateRightToRule(BlueprintFeature iconSource)
        {
            var feature = BlueprintUtil.Create<BlueprintFeature>();
            feature.name = "AristocratRightToRuleFeature";
            feature.AssetGuid = BlueprintGuid.Parse(RightToRuleGuid);
            BlueprintUtil.SetName(feature, "Aristocrat.RightToRule", "Right to Rule", RightToRuleText);
            BlueprintUtil.CopyIcon(iconSource, feature);
            feature.IsClassFeature = true;
            feature.Ranks = 1;

            var defense = new RightToRuleDefense { name = "AristocratRightToRuleDefense" };
            var offense = new RightToRuleOffense { name = "AristocratRightToRuleOffense" };
            BlueprintUtil.Own(defense, feature);
            BlueprintUtil.Own(offense, feature);
            BlueprintUtil.Set(feature, "Components", new BlueprintComponent[] { defense, offense });
            BlueprintUtil.Register(feature);
            return feature;
        }

        static void AcceptJudgment(BlueprintFeature gate, BlueprintFeature judgment)
        {
            if (gate?.ComponentsArray == null)
            {
                return;
            }

            var prerequisites = gate.ComponentsArray.OfType<PrerequisiteFeature>().ToList();
            var related = prerequisites.Where(item => Same(FeatureGuid(item), BaseJudgmentGuid) || Same(FeatureGuid(item), FaithHunterJudgmentGuid)).ToList();
            if (related.Count == 0 || prerequisites.Any(item => Same(FeatureGuid(item), judgment.AssetGuid.ToString())))
            {
                return;
            }

            var copy = (PrerequisiteFeature)BlueprintUtil.CloneComponent(related[0], gate);
            copy.name = "AristocratJudgmentPrerequisite";
            BlueprintUtil.Set(copy, "m_Feature", judgment.ToReference<BlueprintFeatureReference>());
            var components = gate.ComponentsArray.ToList();
            components.Add(copy);
            BlueprintUtil.Set(gate, "Components", components.ToArray());
            Main.Log(gate.name + " accepts Aristocrat judgment.");
        }

        static BlueprintArchetype BuildArchetype(BlueprintArchetype faithHunter)
        {
            var archetype = BlueprintUtil.Create<BlueprintArchetype>();
            BlueprintUtil.CopyMergedFields(faithHunter, archetype);
            archetype.name = "AristocratArchetype";
            archetype.AssetGuid = BlueprintGuid.Parse(ArchetypeGuid);
            BlueprintUtil.Set(archetype, "m_ParentClass", Inquisitor);
            BlueprintUtil.Set(archetype, "m_HiddenInUI", false);
            archetype.LocalizedName = BlueprintUtil.MakeString("Aristocrat.Archetype.Name", "Aristocrat");
            archetype.LocalizedDescription = BlueprintUtil.MakeString("Aristocrat.Archetype.Desc", ArchetypeText);
            archetype.LocalizedDescriptionShort = BlueprintUtil.MakeString(
                "Aristocrat.Archetype.Short",
                "A faith hunter of scholars whose right to rule is proven by wisdom.");
            return archetype;
        }

        static void Swap(List<LevelEntry> entries, string oldGuid, BlueprintFeatureBase replacement)
        {
            foreach (var entry in entries)
            {
                if (entry?.Features == null)
                {
                    continue;
                }

                var features = new List<BlueprintFeatureBase>();
                var changed = false;
                foreach (var feature in entry.Features)
                {
                    if (feature != null && Same(feature.AssetGuid.ToString(), oldGuid))
                    {
                        features.Add(replacement);
                        changed = true;
                    }
                    else if (feature != null)
                    {
                        features.Add(feature);
                    }
                }

                if (changed)
                {
                    entry.SetFeatures(features);
                }
            }
        }

        static string FeatureGuid(BlueprintComponent component)
        {
            var reference = component == null ? null : AccessTools.Field(component.GetType(), "m_Feature")?.GetValue(component);
            if (reference == null)
            {
                return "";
            }

            var guid = AccessTools.Property(reference.GetType(), "Guid")?.GetValue(reference, null);
            return guid == null ? "" : guid.ToString();
        }

        static bool HasArchetype(BlueprintCharacterClass characterClass, string guid)
        {
            var field = AccessTools.Field(typeof(BlueprintCharacterClass), "m_Archetypes");
            var current = (BlueprintArchetypeReference[])field.GetValue(characterClass) ?? new BlueprintArchetypeReference[0];
            return current.Any(reference => reference != null && Same(reference.Guid.ToString(), guid));
        }

        static void AddArchetype(BlueprintCharacterClass characterClass, BlueprintArchetype archetype)
        {
            var field = AccessTools.Field(typeof(BlueprintCharacterClass), "m_Archetypes");
            var current = (BlueprintArchetypeReference[])field.GetValue(characterClass) ?? new BlueprintArchetypeReference[0];
            var list = current.ToList();
            list.Add(archetype.ToReference<BlueprintArchetypeReference>());
            field.SetValue(characterClass, list.ToArray());
            Main.Log("Aristocrat is on the Inquisitor archetype list (" + list.Count + ").");
        }

        static bool Same(string left, string right)
        {
            string Clean(string value)
            {
                return value == null ? "" : value.Replace("-", "");
            }

            return string.Equals(Clean(left), Clean(right), StringComparison.OrdinalIgnoreCase);
        }
    }
}
