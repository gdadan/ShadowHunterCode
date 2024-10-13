using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSwing : ActiveSkill
{
    [SerializeField] Collider areaCol;

    float bonusDamage = 0.3f;

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과를 위한 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //넉백 효과 실행
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new KnockBack(skillData.statusValue[0], target, skillData.statusValue[1], skillUser));

            //출혈 효과 적에게 추가피해
            StatusEffect bleed = target.statusEffMgr.activeBuffs.Find(buff => buff.target == target && buff.statusEffectType == StatusEffect.StatusEffectType.Bleed);

            if (bleed != null)
            {
                target.GetComponent<IDamageable>().OnDamaged(damage * bonusDamage, false);
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartCoroutine(StartRangeSkill());
    }

    //스킬 실행
    IEnumerator StartRangeSkill()
    {
        //플레이어 주위 범위 공격
        areaCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime);

        areaCol.gameObject.SetActive(false);

        StopSkill();
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.firstUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //출혈 효과 적에게 추가피해 증가
        bonusDamage += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
