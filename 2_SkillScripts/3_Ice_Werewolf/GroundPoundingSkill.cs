using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPoundingSkill : ActiveSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider lastGroundCol; //스킬 마지막 콜라이더
    [SerializeField] Collider groundCol; //생성할 원본 콜라이더
    List<Collider> groundCols = new List<Collider>(); //바닥 콜라이더

    int poundingCount = 3; //공격 횟수 => 마지막 공격 제외

    bool hasBonusDamage = false; //동상 중인 적에게 추가 데미지를 주는 지 여부 => 스킬 업그레이드 시 활성

    private void Start()
    {
        //크기 설정
        InitGroundCol(poundingCount);
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //동상 중인 적이면 추가피해
        if (target != null && hasBonusDamage)
        {
            StatusEffect freeze = target.statusEffMgr.activeBuffs.Find(buff => buff.target == target && buff.statusEffectType == StatusEffect.StatusEffectType.Freeze);

            if (freeze != null)
            {
                target.GetComponent<IDamageable>().OnDamaged(damage * skillData.secondUpgradeValue[0], false);
            }
        }
    }

    public override void UseActiveSkill()
    {
        StartCoroutine(GroundHit());
    }

    //마지막 바닥 공격 => 이펙트, 애니메이션 다름
    IEnumerator LastGroundHit()
    {
        //애니메이션 실행
        skillUser.ControlAnimTimeScale(1f);
        skillUser.PlayAnimation("attack4");

        //이펙트 연출 위한 오브젝트 비활성, 딜레이
        lastGroundCol.enabled = false;
        lastGroundCol.gameObject.SetActive(true);

        foreach (Collider col in groundCols)
        {
            col.gameObject.SetActive(false);
        }

        yield return YieldCache.WaitForSeconds(0.5f);

        lastGroundCol.enabled = true;

        yield return YieldCache.WaitForSeconds(0.5f);

        lastGroundCol.gameObject.SetActive(false);

        StopSkill();
    }

    //스킬 실행 - 바닥 공격 횟수 만큼 공격 후 마지막 공격
    IEnumerator GroundHit()
    {
        for (int i = 0; i < poundingCount; i++)
        {
            //애니메이션 실행
            skillUser.ControlAnimTimeScale(skillData.animTimeScale);
            skillUser.PlayAnimation(skillData.animName);

            groundCols[i].gameObject.SetActive(true);

            yield return YieldCache.WaitForSeconds((skillData.useTime - 1) / poundingCount);

            //groundCols[i].gameObject.SetActive(false);
        }

        StartCoroutine(LastGroundHit());
    }

    //바닥 콜라이더 생성
    void InitGroundCol(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider ground = Instantiate(groundCol, allObj.transform);
            groundCols.Add(ground);
        }
    }
    public override void AddFirstUpgrade()
    {

        //데미지 증가
        skillData.skillDamage += skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {

        hasBonusDamage = true;
    }

    public override void AddThirdUpgrade()
    {
 
        //개수 증가
        poundingCount += (int)skillData.thirdUpgradeValue[0];
        InitGroundCol((int)skillData.thirdUpgradeValue[0]);
    }
}
