using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDashSkill : TargetingSkill
{
    [SerializeField] Collider dashCol; //캐릭터 주위 콜라이더
    [SerializeField] GameObject dashEffect; //대쉬 하는 중에 보이는 이펙트

    int dashCount = 3; //돌진 횟수

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과 적용을 위한 리스트

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //출혈효과 적용
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new BleedDebuff(skillData.statusValue[0], target, skillData.statusValue[1], damage * skillData.statusValue[2]));
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartCoroutine(StartDashCoro());
    }

    //스킬 실행
    IEnumerator StartDashCoro()
    {
        dashCol.gameObject.SetActive(true);

        for (int i = 0; i < dashCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            Dash();

            //targetList.Clear();

            yield return YieldCache.WaitForSeconds(skillData.useTime / dashCount);
        }

        dashCol.gameObject.SetActive(false);
        StopSkill();
    }

    //대쉬 한번
    void Dash()
    {
        //플레이어가 가장 먼 적 방향으로 돌진
        Transform skillUserTrf = skillUser.transform;

        Vector3 dirVec = new Vector3(targetList[0].position.x - skillUserTrf.position.x,
           skillUserTrf.position.y, targetList[0].position.z - skillUserTrf.position.z);

        float distance = skillData.atkRange;
        Vector3 endValue = dirVec.normalized * distance;

        //대쉬 이펙트 시전자 밖으로
        dashEffect.transform.SetParent(null);
        dashEffect.transform.position = transform.position;
        dashEffect.transform.LookAt(targetList[0]);
        dashEffect.SetActive(true);

        //플립 설정 
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        skillUserTrf.DOMove(endValue, skillData.useTime / dashCount).SetRelative()
             .OnComplete(() => { skillUser.skillUserAtkHandler.Target = null; dashEffect.SetActive(false); });

        targetList.Clear();
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //횟수 증가
        dashCount += (int)skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //출혈 데미지 증가
        skillData.statusValue[2] += skillData.thirdUpgradeValue[0];
    }
}
