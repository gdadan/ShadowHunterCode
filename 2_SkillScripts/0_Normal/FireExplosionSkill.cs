using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExplosionSkill : TargetingSkill
{
    [SerializeField] Collider areaCol; //폭발 콜라이더
    [SerializeField] Collider fireGround; //화염지대 콜라이더

    float duration = 1.3f; //오브젝트 꺼질 때까지 지연시간
    
    bool hasFireGround = false; //화염지대 활성화 여부 => 업그레이드 시 활성

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과 적용을 위한 리스트

    private void Start()
    {
        //폭발 크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //화상효과 적용
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new BurnDebuff(skillData.statusValue[0], target,
               skillData.statusValue[1], damage * skillData.statusValue[2])); ;
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartAoESkill();

        if (hasFireGround)
        {
            StartCoroutine(ActiveFireGroundCoro());
        }
    }

    //스킬 실행
    void StartAoESkill()
    {
        //일정 범위 내 적에게 광역 공격
        areaCol.transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCol, skillData.useTime, duration));
    }

    //화염지대 활성화
    IEnumerator ActiveFireGroundCoro()
    {
        fireGround.transform.position = targetList[0].position;

        yield return YieldCache.WaitForSeconds(skillData.useTime + 0.3f);

        fireGround.enabled = false;
        fireGround.gameObject.SetActive(true);

        StartCoroutine(Utils.AreaDoTAttack(fireGround, skillData.secondUpgradeValue[0], skillData.secondUpgradeValue[1]));

        yield return YieldCache.WaitForSeconds(skillData.secondUpgradeValue[0]);

        fireGround.gameObject.SetActive(false);
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //폭발 범위 증가
        skillData.atkRange *= 1 + skillData.firstUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //화염지대 생성
        fireGround.transform.parent = null;
        Utils.SetSkillRange(fireGround.gameObject, skillData.atkRange);

        hasFireGround = true;
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //화상 데미지 증가
        skillData.statusValue[2] += skillData.thirdUpgradeValue[0];
    }

}
