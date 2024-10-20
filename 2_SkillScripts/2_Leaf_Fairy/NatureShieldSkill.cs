using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureShieldSkill : ActiveSkill
{
    [SerializeField] Collider explosionCol; //쉴드가 남아있을 시 폭발할 때 콜라이더
    [SerializeField] GameObject shieldEffect; //쉴드 이펙트

    float shieldAmount = 0.05f; //쉴드량
    float shieldTime = 5f; //쉴드 시간

    ShieldData shieldData;

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(explosionCol.gameObject, skillData.atkRange);
    }

    public override void UseActiveSkill()
    {      
        //쉴드 데이터 추가
        shieldData = new ShieldData(skillUser.SkillUserHp * shieldAmount, skillUser.SkillUserHp * shieldAmount, BrokenShield);

        StartCoroutine(ActiveShieldCoro());
    }

    //쉴드 시간이 끝나기 전에 쉴드가 깨질 시
    void BrokenShield()
    {
        //중간에 쉴드가 깨질 시 이펙트 비활성화, 쉴드데이터 제거
        shieldEffect.SetActive(false);
        skillUser.skillUserAtkHandler.RemoveShield(shieldData);

        StopCoroutine(ActiveShieldCoro());
    }

    //스킬 실행 - 쉴드 활성화
    IEnumerator ActiveShieldCoro()
    {
        //쉴드 데이터 추가, 이펙트 활성화
        skillUser.skillUserAtkHandler.SetShield(shieldData);

        shieldEffect.SetActive(true);

        yield return YieldCache.WaitForSeconds(shieldTime);

        //일정 시간 후에 아직 쉴드가 남아있다면
        if (shieldEffect.gameObject.activeSelf)
        {
            shieldEffect.SetActive(false);
            skillUser.skillUserAtkHandler.RemoveShield(shieldData);

            explosionCol.gameObject.SetActive(true);

            yield return YieldCache.WaitForSeconds(1f);

            explosionCol.gameObject.SetActive(false);
        }

        StopSkill();
    }

    public override void AddFirstUpgrade()
    {
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        //쉴드량 증가
        shieldAmount += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
