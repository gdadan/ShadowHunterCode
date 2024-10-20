using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandStormSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] Collider areaCol;

    public List<Transform> targetList { get; set ; }

    private void Start()
    {
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void UseActiveSkill()
    {      
        StartAoESkill();
    }

    //스킬 실행
    void StartAoESkill()
    {
        //일정 범위 내 적에게 광역 지속 공격
        areaCol.transform.position = targetList[0].position;

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
       
        //피해 주기 감소
        skillData.intervalTime *= 1 - skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
       
        //지속 시간 증가
        skillData.duration *= 1 + skillData.thirdUpgradeValue[0];
    }
}
