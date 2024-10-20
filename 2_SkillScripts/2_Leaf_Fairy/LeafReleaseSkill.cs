using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafReleaseSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] Collider areaCol;

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과를 위한 리스트

    public List<Transform> targetList { get; set; }

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

    public override void UseActiveSkill()
    {
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
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
