using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSwordSkill : TargetingSkill
{
    [SerializeField] Collider areaCol;

    float duration = 0.8f; //오브젝트 꺼질 때까지 지연시간

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과를 위한 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //기절 효과 실행
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new StunDebuff(skillData.statusValue[0], target));
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        SummonSword();
    }

    //스킬 실행
    void SummonSword()
    {
        //일정 범위 내 적에게 광역 공격
        areaCol.transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCol, skillData.useTime, duration));
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //스킬 데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
