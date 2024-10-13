using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningField : ActiveSkill
{
    [SerializeField] Collider areaCol;

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void UseSkill()
    {
        base.UseSkill();

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
        base.AddFirstUpgrade();

        //지속시간 증가
        skillData.duration *= 1 + skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //피해 주기 감소
        skillData.intervalTime *= 1 - skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.thirdUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }
}
