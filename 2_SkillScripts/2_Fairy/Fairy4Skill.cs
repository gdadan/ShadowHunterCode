using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fairy4Skill : TargetingSkill
{
    [SerializeField] Collider areaCol;

    float duration = 1.2f;

    public override void LevelUp()
    {
        base.LevelUp();

    }

    public override void UseSkill()
    {
        base.UseSkill();

        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);

        StartAoESkill();
    }

    void StartAoESkill()
    {
        //일정 범위 내 적에게 광역 공격
        areaCol.transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCol, skillData.useTime, duration));
    }
}
