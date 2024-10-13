using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashSkill : TargetingSkill
{
    [SerializeField] Collider dashCol;
    [SerializeField] GameObject shieldEffect; //쉴드 이펙트

    bool hasShield = false; //쉴드 활성화 가능 여부 => 업그레이드 시 활성

    float shieldAmount; //쉴드량
    float shieldDuration; //쉴드 지속시간

    ShieldData shieldData;

    private void Start()
    {
        shieldAmount = skillData.firstUpgradeValue[0];
        shieldDuration = skillData.firstUpgradeValue[1];
    }

    public override void UseSkill()
    {
        base.UseSkill();

        if (hasShield)
        {
            shieldData = new ShieldData(skillUser.SkillUserHp * shieldAmount, skillUser.SkillUserHp * shieldAmount, BrokenShield);
            StartCoroutine(ActiveShield());
        }

        Dash();
    }

    void Dash()
    {
        Transform skillUserTrf = skillUser.transform;

        //플레이어가 가장 가까운 적 방향으로 atkRange만큼 돌진
        Vector3 dirVec = new Vector3(targetList[0].position.x - skillUserTrf.position.x,
            skillUserTrf.position.y,
            targetList[0].position.z - skillUserTrf.position.z);

        float distance = skillData.atkRange;
        Vector3 endValue = dirVec.normalized * distance;
        
        dashCol.gameObject.SetActive(true);

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        skillUserTrf.DOMove(endValue, skillData.useTime).SetRelative()
        .OnComplete(() => { skillUser.skillUserAtkHandler.Target = null; dashCol.gameObject.SetActive(false); StopSkill(); });

        //Sequence sequence = DOTween.Sequence();

        //float lotate = IdleGameManager.Instance.player.dirX > 0 ? 360 : -360;

        //sequence.Append(IdleGameManager.Instance.player.characTrf.DORotate(new Vector3(0, 0, lotate), skillData.skillData.sUseAnimDelay, RotateMode.FastBeyond360))
        //    .Join(IdleGameManager.Instance.player.transform.DOMove(endValue, skillData.skillData.sUseAnimDelay).SetRelative())
        //    .OnComplete(() => { IdleGameManager.Instance.player.scanner.nearestTarget = null; dashColl.SetActive(false); StopSkill(); });
    }

    void BrokenShield()
    {
        //중간에 쉴드가 깨질 시 이펙트 비활성화, 쉴드데이터 제거
        shieldEffect.SetActive(false);
        skillUser.skillUserAtkHandler.RemoveShield(shieldData);

        StopCoroutine(ActiveShield());
    }

    //쉴드 활성화
    IEnumerator ActiveShield()
    {
        skillUser.skillUserAtkHandler.SetShield(shieldData);

        shieldEffect.SetActive(true);

        yield return YieldCache.WaitForSeconds(shieldDuration);

        //일정 시간 후에 아직 쉴드가 남아있다면
        if (shieldEffect.activeSelf)
        {
            shieldEffect.SetActive(false);
            skillUser.skillUserAtkHandler.RemoveShield(shieldData);
        }
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //쉴드 활성화 가능
        shieldEffect.transform.parent = skillUser.transform;
        shieldEffect.transform.localPosition = Vector3.zero;
        hasShield = true;
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //쉴드 지속시간 증가
        shieldDuration *= 1 + skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //쉴드량 증가
        shieldAmount += skillData.thirdUpgradeValue[0];
    }
}
