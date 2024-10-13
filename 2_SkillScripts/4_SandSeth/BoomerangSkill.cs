using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BoomerangSkill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> boomerangCols = new List<Collider>(); //부메랑 콜라이더들

    float duration = 0.8f; //부메랑 날아가는 시간
    int boomerangCount = 3; //부메랑 개수

    private void Start()
    {
        //크기 설정, 부메랑 생성
        Utils.SetSkillRange(allObj, skillData.skillScale);
        InitBoomerang(boomerangCount);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(ThrowBoomerangCoro());
    }

    //스킬 실행 - 부메랑 개수만큼 전체 날리기
    IEnumerator ThrowBoomerangCoro()
    {
        for (int i = 0; i < boomerangCount; i++)
        {
            ThrowOneBoomerang(i);
        }

        //부메랑 날아가는 시간
        yield return YieldCache.WaitForSeconds(duration * 2);

        StopSkill();
    }

    // 부메랑 하나 날리기
    void ThrowOneBoomerang(int order)
    {
        Vector3 directionToTarget = (targetList[0].position - transform.position).normalized;
        float angleStep = 180 / (boomerangCount - 1); //각 부메랑 간의 각도 차이 계산
        float angleOffset = -180 / 2f; //각도 범위의 중심을 기준으로 각도 조정

        Vector3 sPos = boomerangCols[order].transform.localPosition;

        float angle = angleOffset + order * angleStep;
        Vector3 direction = Quaternion.Euler(0, angle, 0) * directionToTarget;
        Vector3 endValue = transform.position + direction * skillData.atkRange;

        boomerangCols[order].gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(boomerangCols[order].transform.DOLocalMove(endValue, duration).SetEase(Ease.InQuart))
            .Append(boomerangCols[order].transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InQuad))
            .OnComplete(() => boomerangCols[order].gameObject.SetActive(false));
    }

    //부메랑 생성
    void InitBoomerang(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider boomerang = Instantiate(originalCol, allObj.transform);
            boomerangCols.Add(boomerang);
        }
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //개수 증가
        boomerangCount += (int)skillData.secondUpgradeValue[0];
        InitBoomerang((int)skillData.secondUpgradeValue[0]);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
