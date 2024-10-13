using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공통적으로 쓰이는 메소드 들
public class Utils
{

    //스킬 크기 변경
    public static void SetSkillRange(GameObject gameObject, float skillAtkRange)
    {
        gameObject.transform.localScale = Vector3.one * skillAtkRange;
    }

    // 근처 타겟 찾기
    public static Collider[] GetNearTargets(Vector3 position, float radius, int targetLayer)
    {
        Collider[] targetCols = Physics.OverlapSphere(position, radius, targetLayer);

        return targetCols;
    }

    //가장 가까운 적 구하기
    public static Transform GetNearest(Vector3 position, float radius, int targetLayer)
    {
        Collider[] targetCols = GetNearTargets(position, radius, targetLayer);

        Transform result = null;
        float diff = 100;

        foreach (Collider target in targetCols)
        {
            Vector3 myPos = position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }
        return result;
    }

    //가장 가까운 적 구하기
    public static Transform GetNearest(Vector3 position, Collider[] _targetCols)
    {
        Collider[] targetCols = _targetCols;

        Transform result = null;
        float diff = 100;

        foreach (Collider target in targetCols)
        {
            Vector3 myPos = position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }
        return result;
    }


    //가장 먼 적 구하기
    public static Transform GetFarthest(Vector3 position, float radius, int targetLayer)
    {
        Collider[] targetCols = GetNearTargets(position, radius, targetLayer);

        Transform result = null;
        float diff = 0;

        foreach (Collider target in targetCols)
        {
            Vector3 myPos = position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff > diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }
        return result;
    }

    //가장 먼 적 구하기
    public static Transform GetFarthest(Vector3 position, Collider[] _targeCols)
    {
        Collider[] targetCols = _targeCols;

        Transform result = null;
        float diff = 0;

        foreach (Collider target in targetCols)
        {
            Vector3 myPos = position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff > diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }
        return result;
    }

    //가장 먼 적을 겹치지 않게 여럿 구하기
    public static Transform[] GetMultiFarthest(Vector3 position, float radius, int targetLayer, int targetCount)
    {
        Collider[] targetCols = GetNearTargets(position, radius, targetLayer);

        // 적의 거리를 저장할 배열
        float[] distances = new float[targetCols.Length];
        Transform[] targets = new Transform[targetCols.Length];

        // 각 적까지의 거리 계산
        for (int i = 0; i < targetCols.Length; i++)
        {
            distances[i] = Vector3.Distance(position, targetCols[i].transform.position);
            targets[i] = targetCols[i].transform;
        }

        // 거리에 따라 적 정렬
        System.Array.Sort(distances, targets);
        
        // 가장 먼 적 선택
        int count = Mathf.Min(targetCount, targets.Length);
        Transform[] result = new Transform[count];
        System.Array.Copy(targets, targets.Length - count, result, 0, count);

        return result;
    }

    //가장 먼 적을 겹치지 않게 여럿 구하기
    public static Transform[] GetMultiFarthest(Vector3 position, int targetCount , Collider[] _targetCols)
    {
        Collider[] targetCols = _targetCols;

        // 적의 거리를 저장할 배열
        float[] distances = new float[targetCols.Length];
        Transform[] targets = new Transform[targetCols.Length];

        // 각 적까지의 거리 계산
        for (int i = 0; i < targetCols.Length; i++)
        {
            distances[i] = Vector3.Distance(position, targetCols[i].transform.position);
            targets[i] = targetCols[i].transform;
        }

        // 거리에 따라 적 정렬
        System.Array.Sort(distances, targets);

        // 가장 먼 적 선택
        int count = Mathf.Min(targetCount, targets.Length);
        Transform[] result = new Transform[count];
        System.Array.Copy(targets, targets.Length - count, result, 0, count);

        return result;
    }

    //랜덤 적 구하기
    public static Transform GetRandom(Collider[] _targetCols)
    {
        Collider[] targetCols = _targetCols;
        Transform result = targetCols[Random.Range(0, targetCols.Length)].transform;

        return result;
    }

    //랜덤 적 구하기
    public static Transform GetRandom(Vector3 position, float radius, int targetLayer)
    {
        Collider[] targetCols = GetNearTargets(position, radius, targetLayer);

        if (targetCols.Length == 0) return null;

        Transform result = targetCols[Random.Range(0, targetCols.Length)].transform;

        return result;
    }

    //범위 지속 공격 -> 지속시간 동안 공격 간격마다 콜라이더 활성
    public static IEnumerator AreaDoTAttack(Collider collider, float duration, float interval)
    {
        float checkTime = 0;

        while (checkTime <= duration)
        {
            collider.enabled = true;

            yield return YieldCache.WaitForFixedUpdate;

            collider.enabled = false;

            if (duration <= interval)
                break;

            yield return YieldCache.WaitForSeconds(interval);

            checkTime += interval;
        }
    }

    public static IEnumerator DoTAttack(AttackHandler target, float damage, float duration, float interval)
    {
        float checkTime = 0f;

        while (checkTime <= duration)
        {
            target.OnDamaged(damage, false);

            yield return YieldCache.WaitForSeconds(interval); // burnIntevalTime마다 화상 데미지

            checkTime += interval;
        }
    }
}


