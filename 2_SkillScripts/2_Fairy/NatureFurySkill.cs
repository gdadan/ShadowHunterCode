using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureFurykill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> areaCols = new List<Collider>(); //각각 콜라이더들

    float duration = 1.2f; //오브젝트 꺼질 때까지 지연시간

    int explosionCount = 4; //폭발 횟수

    private void Start()
    {
        //초기 생성, 크기 설정
        InitExplosion(explosionCount);
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(ExplodeMultipleCoro());
    }

    //스킬 실행
    IEnumerator ExplodeMultipleCoro()
    {
        //일정 범위 내 적 랜덤한 적에게 최대 수만큼 폭발
        for (int i = 0; i < explosionCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            ExplodeSingle(i);

            //타겟 초기화
            //targetList.Clear();

            yield return YieldCache.WaitForSeconds(skillData.useTime / explosionCount);
        }

        yield return YieldCache.WaitForSeconds(skillData.useTime + duration);

        StopSkill();
    }

    //하나 폭발
    void ExplodeSingle(int order)
    {
        areaCols[order].transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCols[order], skillData.useTime, duration));

        targetList.Clear();
    }

    //생성
    void InitExplosion(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider explosion = Instantiate(originalCol, allObj.transform);
            areaCols.Add(explosion);
        }
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //횟수 증가
        explosionCount += (int)skillData.secondUpgradeValue[0];
        InitExplosion((int)skillData.secondUpgradeValue[0]);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.thirdUpgradeValue[0];
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }
}