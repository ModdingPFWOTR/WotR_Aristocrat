using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace Aristocrat
{
    static class RightToRuleRules
    {
        public static bool Active(UnitEntityData unit, int minLevel)
        {
            if (unit?.Progression == null || Content.Inquisitor == null)
            {
                return false;
            }

            if (unit.Progression.GetClassLevel(Content.Inquisitor) < minLevel)
            {
                return false;
            }

            if (unit.Buffs == null || Content.JudgmentBuffs == null)
            {
                return false;
            }

            foreach (var buff in Content.JudgmentBuffs)
            {
                if (buff != null && unit.Buffs.GetBuff(buff) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Chaotic(UnitEntityData unit)
        {
            return unit?.Descriptor != null
                && unit.Descriptor.Alignment.ValueRaw.HasComponent(AlignmentComponent.Chaotic);
        }
    }

    public class RightToRuleDefense : UnitFactComponentDelegate<RightToRuleDefense.Memory>,
        ITargetRulebookHandler<RuleAttackWithWeapon>,
        IRulebookHandler<RuleAttackWithWeapon>,
        ISubscriber,
        ITargetRulebookSubscriber,
        ITargetRulebookHandler<RuleCalculateAC>,
        IRulebookHandler<RuleCalculateAC>,
        IInitiatorRulebookHandler<RuleSavingThrow>,
        IInitiatorRulebookSubscriber
    {
        public class Memory
        {
            public int ACModifier;
            public UnitEntityData Initiator;

            public void Clear()
            {
                ACModifier = 0;
                Initiator = null;
            }
        }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.Target != Owner)
            {
                return;
            }

            if (RightToRuleRules.Active(Owner, 4) && RightToRuleRules.Chaotic(evt.Initiator))
            {
                Data.ACModifier = 2;
                Data.Initiator = evt.Initiator;
            }
            else
            {
                Data.Clear();
            }
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.Target == Owner)
            {
                Data.Clear();
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            if (Data.Initiator != null && evt.Initiator == Data.Initiator)
            {
                evt.AddModifier(Data.ACModifier, Fact, ModifierDescriptor.Insight);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAC evt)
        {
        }

        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (!RightToRuleRules.Active(Owner, 8))
            {
                return;
            }

            var chaotic = AlignmentComponent.Chaotic;
            var caster = evt.Reason.Caster;
            var fromCreature = caster?.Descriptor != null && caster.Descriptor.Alignment.ValueRaw.HasComponent(chaotic);
            var context = evt.Reason.Context;
            var fromEffect = context != null && context.SpellDescriptor.HasAnyFlag(chaotic.GetAlignmentDescriptor());
            if (!fromCreature && !fromEffect)
            {
                return;
            }

            evt.AddTemporaryModifier(evt.Initiator.Stats.SaveWill.AddModifier(2, Runtime, ModifierDescriptor.Insight));
            evt.AddTemporaryModifier(evt.Initiator.Stats.SaveReflex.AddModifier(2, Runtime, ModifierDescriptor.Insight));
            evt.AddTemporaryModifier(evt.Initiator.Stats.SaveFortitude.AddModifier(2, Runtime, ModifierDescriptor.Insight));
        }

        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
        }
    }

    public class RightToRuleOffense : UnitFactComponentDelegate<RightToRuleOffense.Memory>,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>,
        IRulebookHandler<RuleAttackWithWeapon>,
        ISubscriber,
        IInitiatorRulebookSubscriber,
        IInitiatorRulebookHandler<RuleCalculateAttackBonus>,
        IRulebookHandler<RuleCalculateAttackBonus>
    {
        public class Memory
        {
            public int AttackBonus;
            public UnitEntityData Target;

            public void Clear()
            {
                AttackBonus = 0;
                Target = null;
            }
        }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.Initiator != Owner || evt.Weapon == null)
            {
                return;
            }

            if (!RightToRuleRules.Active(Owner, 12) || !RightToRuleRules.Chaotic(evt.Target))
            {
                Data.Clear();
                return;
            }

            Data.AttackBonus = 2;
            Data.Target = evt.Target;

            if (!RightToRuleRules.Active(Owner, 16))
            {
                return;
            }

            var wisdom = Owner.Stats.Wisdom.Bonus;
            if (wisdom == 0)
            {
                return;
            }

            evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalDamage.AddModifier(wisdom, Runtime, ModifierDescriptor.Insight));
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.Initiator == Owner)
            {
                Data.Clear();
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        {
            if (Data.Target != null && Data.Target == evt.Target)
            {
                evt.AddModifier(Data.AttackBonus, Fact, ModifierDescriptor.Insight);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt)
        {
        }
    }
}
