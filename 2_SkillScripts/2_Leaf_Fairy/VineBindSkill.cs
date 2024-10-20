using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VineBindSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject[] enemyEffects; //적에 있는 이펙트
    [SerializeField] GameObject[] lineTargets; // 라인렌더러 타겟

    int bindCount = 5; //속박 횟수

    List<AttackHandler> targets = new List<AttackHandler>();

    public LayerMask targetLayer;

    IEnumerator enumerator;

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        enumerator = StartRootSkillCoro();
    }

    public override void UseActiveSkill()
    {       
        targets.Clear();
        StartCoroutine(StartRootSkillCoro());
    }

    IEnumerator StartRootSkillCoro()
    {
        SetTarget();

        //타겟 위치에 따라 이펙트 위치 설정
        for (int i = 0; i < targets.Count; i++)
        {
            if (i == targets.Count - 1)
            {
                enemyEffects[i].transform.position = targets[i].transform.position + new Vector3(0, 0.2f, 0);
                lineTargets[i].transform.position = targets[i].transform.position + new Vector3(0.01f, 0.2f, 0.01f);
                enemyEffects[i].SetActive(true);
                break;
            }

            enemyEffects[i].transform.position = targets[i].transform.position + new Vector3(0, 0.2f, 0);
            lineTargets[i].transform.position = targets[i + 1].transform.position + new Vector3(0, 0.2f, 0);
            enemyEffects[i].SetActive(true);
        }

        //적 움직임 x
        foreach (AttackHandler target in targets)
        {
            if (target.isLive)
            {
                target.ChangeState(AttackHandler.AttackState.Stop);
                target.OnDamaged(damage, false);
            }
        }

        yield return YieldCache.WaitForSeconds(3f);

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].ChangeState(AttackHandler.AttackState.Find);
            enemyEffects[i].SetActive(false);
        }

        StopSkill();
    }

    //타겟 죽은 상태 체크
    IEnumerator CheckTargetLiveCoro()
    {
        yield return YieldCache.WaitForSeconds(1f);

        int offCount = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].isLive)
            {
                enemyEffects[i].SetActive(false);
                offCount++;
            }
        }

        if (offCount == targets.Count)
        {
            StopCoroutine(enumerator);

            skillUser.skillUserAtkHandler.ChangeState(AttackHandler.AttackState.Find);

            StopSkill();
        }
    }

    void SetTarget()
    {
        Collider[] targetCols = Physics.OverlapSphere(targetList[0].position, 5, targetLayer);

        // 적의 거리를 저장할 배열
        float[] distances = new float[targetCols.Length];
        Transform[] targetsTr = new Transform[targetCols.Length];

        // 각 적까지의 거리 계산
        for (int i = 0; i < targetCols.Length; i++)
        {
            distances[i] = Vector3.Distance(targetList[0].position, targetCols[i].transform.position);
            targetsTr[i] = targetCols[i].transform;
        }

        // 거리에 따라 적 정렬
        System.Array.Sort(distances, targetsTr);

        // 가장 가까운 적 선택
        int count = Mathf.Min(bindCount, targetsTr.Length);
        Transform[] result = new Transform[count];
        System.Array.Copy(targetsTr, result, count);

        for (int i = 0; i < result.Length; i++)
            targets.Add(result[i].GetComponent<AttackHandler>());
    }

    public override void AddFirstUpgrade()
    {       
        //개수 증가
        bindCount += (int)skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
