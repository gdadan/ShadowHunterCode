using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleThunderSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject allObj; // 전체 오브젝트
    [SerializeField] GameObject thunderEffect; // 번개 원본 이펙트
    List<GameObject> thunderEffects = new List<GameObject>(); //번개 이펙트들 

    int thunderCount = 7; //번개 개수
    float duration = 1f; //이펙트 꺼지는 시간

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        InitThunder(thunderCount);
    }

    public override void UseActiveSkill()
    {
        StartCoroutine(StartThunderSkill());
    }

    //스킬 실행
    IEnumerator StartThunderSkill()
    {
        for (int i = 0; i < thunderCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this , skillData);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            StartCoroutine(SetEffect(i));

            yield return YieldCache.WaitForSeconds(skillData.useTime / thunderCount);

            //타겟 초기화
            targetList.Clear();
        }
       
        yield return YieldCache.WaitForSeconds(duration);

        StopSkill();
    }

    //이펙트 위치 설정
    IEnumerator SetEffect(int order)
    {
        thunderEffects[order].transform.position = targetList[0].position;
        thunderEffects[order].SetActive(true);

        //이펙트 타이밍 맞게 데미지
        yield return YieldCache.WaitForSeconds(0.1f);

        targetList[0].GetComponent<AttackHandler>().OnDamaged(damage, false);

        //오브젝트 꺼지는시간
        yield return YieldCache.WaitForSeconds(duration);

        thunderEffects[order].SetActive(false);
        
    }

    //번개 생성
    void InitThunder(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject thunder = Instantiate(thunderEffect, allObj.transform);
            thunderEffects.Add(thunder);
        }
    }
    public override void AddFirstUpgrade()
    {
      
        //횟수 증가
        thunderCount += (int)skillData.firstUpgradeValue[0];
        InitThunder((int)skillData.firstUpgradeValue[0]);
    }

    public override void AddSecondUpgrade()
    {
      
        //데미지 증가
        skillData.skillDamage += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
       
        //횟수 증가
        thunderCount += (int)skillData.thirdUpgradeValue[0];
        InitThunder((int)skillData.thirdUpgradeValue[0]);
    }
}
