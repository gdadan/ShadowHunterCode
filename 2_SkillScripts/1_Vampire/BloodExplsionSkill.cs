using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodExplsionSkill : TargetingSkill
{
    [SerializeField] Collider areaCol;

    float duration = 0.6f; //오브젝트 꺼질 때까지 지연시간
    
    bool hasBleed = false; //출혈 활성화 여부 => 스킬 업그레이드 시 활성
    
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

        //스턴 효과
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new StunDebuff(skillData.statusValue[0], target));

            //출혈 효과
            if (hasBleed)
            {
                target.statusEffMgr.AddBuff(new BleedDebuff(skillData.firstUpgradeValue[0], target, 
                    skillData.firstUpgradeValue[1], damage * skillData.firstUpgradeValue[2]));
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartAoESkill();
    }

    //스킬 실행
    void StartAoESkill()
    {
        //일정 범위 내 적에게 광역 공격
        areaCol.transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCol, skillData.useTime, duration));
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //출혈 효과 생성
        hasBleed = true;
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

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
