using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAreaSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] Collider areaCol;

    float duration = 0.8f; //오브젝트 꺼질 때까지 지연시간

    bool hasBurn = false; //화상 효과를 가지고 있는 지 => 스킬업그레이드 시 활성

    List<AttackHandler> targets = new List<AttackHandler>(); //타겟 리스트

    public List<Transform> targetList { get; set; }

    public override void OnTriggerEnter(Collider other)
    {
        //base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);

            //출혈 효과
            if (hasBurn)
            {
                target.statusEffMgr.AddBuff(new BurnDebuff(skillData.firstUpgradeValue[0], target,
                    skillData.firstUpgradeValue[1], damage * skillData.firstUpgradeValue[2]));
            }
        }
    }

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void UseActiveSkill()
    {
        StartSkill();
    }

    //스킬 실행
    void StartSkill()
    {
        //일정 범위 내 적에게 광역 공격
        areaCol.transform.position = targetList[0].position; 

        StartCoroutine(CastSkillCoro(areaCol, skillData.useTime, duration));
    }

    public override void AddFirstUpgrade()
    {
        //화상 효과
        hasBurn = true;
    }

    public override void AddSecondUpgrade()
    {
         //범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
