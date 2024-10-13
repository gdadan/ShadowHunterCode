using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GroundStrikeSkill : TargetingSkill
{
    [SerializeField] Collider groundCol;

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과를 위한 리스트

    bool hasBonusDamage = false; //출혈 중인 적에게 추가피해 발생 여부 => 스킬 업그레이드 시 활성

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(groundCol.gameObject, skillData.skillScale);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //에어본 효과 실행
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new AirborneDebuff(skillData.statusValue[0], target));

            //출혈 중인 적이면 추가피해
            if (hasBonusDamage)
            {
                StatusEffect bleed = target.statusEffMgr.activeBuffs.Find(buff => buff.target == target && buff.statusEffectType == StatusEffect.StatusEffectType.Bleed);

                if (bleed != null)
                {
                    target.GetComponent<IDamageable>().OnDamaged(damage * skillData.firstUpgradeValue[0], false);
                }
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartCoroutine(GroundSlashCoro());
    }

    //스킬 실행
    IEnumerator GroundSlashCoro()
    {
        //애니메이션 모션을 위한 시간
        yield return YieldCache.WaitForSeconds(0.2f);

        //가장 가까운 적 방향으로 공격
        Vector3 dirVec = targetList[0].position - skillUser.transform.position;
        
        groundCol.transform.LookAt(targetList[0].position);
        groundCol.transform.localPosition = dirVec.normalized;

        groundCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime - 0.2f);

        groundCol.gameObject.SetActive(false);

        StopSkill();
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //출혈 효과 적에게 추가 피해
        hasBonusDamage = true;
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}