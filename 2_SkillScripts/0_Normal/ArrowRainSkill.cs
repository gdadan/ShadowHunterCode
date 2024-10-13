using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRainSkill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider arrowCol; //생성할 콜라이더
    List<Collider> arrowCols = new List<Collider>(); //각각 콜라이더

    float duration = 1.2f; //오브젝트 꺼질 때까지 지연시간
    int arrowCount = 3; //화살 개수

    private void Start()
    {
        //크기 설정, 화살 수 설정
        InitArrow(arrowCount);
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(DropArrowCoro());
    }

    IEnumerator DropArrowCoro()
    {
        //일정 범위 내 적 랜덤한 적에게 최대 count번 화살 떨어뜨림
        for (int i = 0; i < arrowCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            DropOneArrow(i);

            //타겟 초기화
            //targetList.Clear();

            yield return YieldCache.WaitForSeconds(skillData.useTime / arrowCount);
        }

        //이펙트 재생을 위한 시간
        yield return YieldCache.WaitForSeconds(skillData.useTime + duration);

        StopSkill();
    }

    //화살 하나 낙하
    void DropOneArrow(int order)
    {
        //if (targetList[0] == null)
        //    return;

        arrowCols[order].transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(arrowCols[order], skillData.useTime, duration));

        targetList.Clear();
    }

    //화살 생성
    void InitArrow(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider arrow = Instantiate(arrowCol, allObj.transform);
            arrowCols.Add(arrow);
        }
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //화살 수 증가
        arrowCount += (int)skillData.firstUpgradeValue[0];
        InitArrow((int)skillData.firstUpgradeValue[0]);
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //화살 범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //화살 수 증가
        arrowCount += (int)skillData.thirdUpgradeValue[0];
        InitArrow((int)skillData.thirdUpgradeValue[0]);
    }
}