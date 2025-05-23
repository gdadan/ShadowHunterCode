using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] Collider areaCol;

    float duration = 1f; //오브젝트 꺼질 때까지 지연시간

    public List<Transform> targetList { get; set; }

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
        //데미지 증가
        skillData.skillDamage += skillData.firstUpgradeValue[0];
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
