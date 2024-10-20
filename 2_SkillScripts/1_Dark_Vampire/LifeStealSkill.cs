using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LifeStealSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject[] enemyEffects; //적에 있는 이펙트
    [SerializeField] GameObject[] lineTargets; // 라인렌더러 타겟 = 플레이어

    float healingAmount = 0.05f; //스킬 시전자 회복량

    List<AttackHandler> targets = new List<AttackHandler>();

    public List<Transform> targetList { get; set; }

    public override void UseActiveSkill()
    {        
        StartCoroutine(StartLifeStealSkill());
    }

    IEnumerator LifeStealCoro(AttackHandler target)
    {
        float checkTime = 0;

        //타겟에게 데미지 주고, 시전자는 체력 회복
        while (checkTime <= skillData.duration)
        {
            target.OnDamaged(damage, false);
            skillUser.skillUserAtkHandler.RecoverHp(damage * healingAmount);

            yield return YieldCache.WaitForSeconds(skillData.intervalTime);

            CheckTargetLive();

            checkTime += skillData.intervalTime;
        }
    }

    //스킬 실행
    IEnumerator StartLifeStealSkill()
    {
        SetTarget();

        for (int i = 0; i < targets.Count; i++)
            enemyEffects[i].transform.parent.gameObject.SetActive(true);

        //적 움직임 x
        foreach (AttackHandler target in targets)
        {
            if (target.isLive)
            {
                target.StopCharacter(skillData.useTime);
                StartCoroutine(LifeStealCoro(target));
            }
        }

        yield return YieldCache.WaitForSeconds(skillData.useTime);

        for (int i = 0; i < targets.Count; i++)
            enemyEffects[i].transform.parent.gameObject.SetActive(false);

        StopSkill();
    }

    //타겟 설정
    void SetTarget()
    {
        targets.Clear();

        //이펙트 설정
        for (int i = 0; i < targetList.Count; i++)
        {
            targets.Add(targetList[i].GetComponent<AttackHandler>());
            enemyEffects[i].transform.position = targetList[i].position;
            lineTargets[i].transform.SetParent(skillUser.transform);
            lineTargets[i].transform.position = skillUser.transform.position + Vector3.up;
        }
    }

    //타겟 죽은 상태 체크
    void CheckTargetLive()
    {
        int offCount = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].isLive)
            {
                enemyEffects[i].transform.parent.gameObject.SetActive(false);
                offCount++;
            }
        }

        if (offCount == targets.Count)
        {
            StopAllCoroutines();

            skillUser.skillUserAtkHandler.ChangeState(AttackHandler.AttackState.Find);
            
            StopSkill();
        }

    }

    public override void AddFirstUpgrade()
    {
         //흡혈량 증가
        healingAmount += skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
         //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
