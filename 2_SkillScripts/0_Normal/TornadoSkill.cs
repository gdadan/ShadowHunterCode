using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoSkill : ActiveSkill
{
    [SerializeField] Collider areaCol;

    bool hasBleed = false; //출혈 활성화 여부 => 업그레이드 시 활성

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과 적용을 위한 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (hasBleed)
        {
            AttackHandler target = other.GetComponent<AttackHandler>();

            //출혈효과 적용
            if (target != null && !targets.Contains(target))
            {
                targets.Add(target);
                target.statusEffMgr.AddBuff(new BleedDebuff(skillData.secondUpgradeValue[0], target, skillData.secondUpgradeValue[1], damage * skillData.secondUpgradeValue[2]));
            }
        }
    }

    public override void UseActiveSkill()
    {       
        targets.Clear();
        
        StartAoESkill();
    }

    //스킬 실행
    void StartAoESkill()
    {
        //플레이어 주위 범위 공격
        StartCoroutine(CastDoTSkillCoro(areaCol, skillData.useTime));
    }

    public override void AddFirstUpgrade()
    {
        //범위 증가
        skillData.atkRange *= 1 + skillData.firstUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddSecondUpgrade()
    {
        //출혈 효과 생성
        hasBleed = true;
    }

    public override void AddThirdUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
