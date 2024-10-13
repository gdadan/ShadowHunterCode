using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafReleaseSkill : TargetingSkill
{
    [SerializeField] Collider areaCol;

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

        //넉백 효과 실행
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new KnockBack(skillData.statusValue[0], target, skillData.statusValue[1], skillUser));
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartCoroutine(StartRangeSkillCoro());
    }

    //스킬 실행
    IEnumerator StartRangeSkillCoro()
    {
        //플레이어에 가장 가까운 적을 향해 범위공격
        areaCol.transform.LookAt(targetList[0].position);

        areaCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime);

        areaCol.gameObject.SetActive(false);

        StopSkill();
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

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
