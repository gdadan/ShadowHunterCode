using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashHitSkill : TargetingSkill
{
    [SerializeField] List<Collider> areaCols;

    int hitCount = 3; //공격 횟수

    List<AttackHandler> targets = new List<AttackHandler>(); //타겟 리스트

    bool hasBleed = false; //출혈효과를 가지고 있는 지 => 스킬업그레이드 시 활성

    public override void OnTriggerEnter(Collider other)
    {
        //base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);

            //출혈 효과
            if (hasBleed)
            {
                target.statusEffMgr.AddBuff(new BleedDebuff(skillData.secondUpgradeValue[0], target, 
                    skillData.secondUpgradeValue[1], damage * skillData.secondUpgradeValue[2]));
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        Dash();
    }

    //스킬 실행
    void Dash()
    {
        //플레이어가 가장 가까운 적 방향으로 atkRange만큼 돌진
        Vector3 sPos = skillUser.transform.position; //처음 대쉬 쓸 때 플레이어 위치 시작 지점
        Vector3 secPos = Vector3.zero; //두번째 대쉬 쓸 때 플레이어 위치
        Vector3 targetPos = targetList[0].transform.position;

        Vector3 dirVec = new Vector3(targetPos.x - sPos.x, sPos.y, targetPos.z - sPos.z).normalized;
        Vector3 endValue = dirVec * skillData.atkRange;

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(skillUser.transform.DOMove(endValue, skillData.useTime * 0.5f).SetRelative()
            .OnStart(() => SetEffectPos(sPos, targetPos, 0)).OnComplete(() => { StartCoroutine(CompleteDashCoro(0)); secPos = skillUser.transform.position; }))
            .Append(skillUser.transform.DOMove(sPos, skillData.useTime * 0.5f)
            .OnStart(() => SetEffectPos(secPos, targetPos, 1)).OnComplete(() => StartCoroutine(CompleteDashCoro(1))));
    }

    //이펙트 위치 설정
    void SetEffectPos(Vector3 startPos, Vector3 targetVec, int order)
    {
        areaCols[order].transform.localPosition = startPos;
        areaCols[order].transform.LookAt(targetVec);
        areaCols[order].gameObject.SetActive(true);
    }

    //대쉬 끝난 후
    IEnumerator CompleteDashCoro(int order)
    {
        skillUser.skillUserAtkHandler.Target = null;

        //적에게 hitCount 수만큼 데미지
        for (int i = 0; i < hitCount; i++)
        {
            foreach (AttackHandler target in targets)
            {
                target.OnDamaged(damage, false);
            }
            yield return YieldCache.WaitForSeconds(0.1f);
        }

        yield return YieldCache.WaitForSeconds(0.6f);

        areaCols[order].gameObject.SetActive(false);

        if (order == 0) yield break;

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

        //출혈 효과
        hasBleed = true;
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
