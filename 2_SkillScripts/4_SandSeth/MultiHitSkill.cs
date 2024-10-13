using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiHitSkill : TargetingSkill
{
    [SerializeField] Collider areaCol;
    [SerializeField] List<ParticleSystem> slashEffects = new List<ParticleSystem>(); //스킬 이펙트

    int hitCount = 6; //공격 횟수

    List<AttackHandler> targets = new List<AttackHandler>(); //타겟 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        //base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartCoroutine(StartRangeSkill());
    }

    //스킬 실행
    IEnumerator StartRangeSkill()
    {
        //플레이어에 가장 가까운 적을 향해 범위공격
        Vector3 dirVec = targetList[0].position - skillUser.transform.position;

        //콜라이더 위치 조정
        areaCol.transform.rotation = Quaternion.LookRotation(dirVec) * Quaternion.Euler(0, 90, 0);
        areaCol.transform.localPosition = dirVec.normalized * 2;

        areaCol.gameObject.SetActive(true);

        //hitCount 만큼 연속 데미지
        for (int i = 0; i < hitCount; i++)
        {
            slashEffects[i].gameObject.SetActive(true);

            yield return YieldCache.WaitForSeconds(0.1f);

            foreach (AttackHandler target in targets)
            {
                target.OnDamaged(damage, false);
            }
        }

        yield return YieldCache.WaitForSeconds(1f);

        for (int i = 0; i < hitCount; i++)
        {
            slashEffects[i].gameObject.SetActive(false);
        }

        //areaCol.transform.localPosition = Vector3.zero;
        //areaCol.transform.rotation = Quaternion.Euler(0,0,0);
        areaCol.gameObject.SetActive(false);

        StopSkill();
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //횟수 증가
        hitCount += (int)skillData.firstUpgradeValue[0];
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
