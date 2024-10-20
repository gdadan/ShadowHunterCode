using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawSlashSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider slashCol; //마지막 슬래쉬 콜라이더
    [SerializeField] List<Collider> clawCols; //손톱 콜라이더들

    int dashCount = 2; //돌진 횟수

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(allObj, skillData.skillScale);

        //slashCol.transform.SetParent(null);
    }


    public override void UseActiveSkill()
    {      
        StartCoroutine(AttackCoro());
    }

    //스킬 실행
    IEnumerator AttackCoro()
    {
        for (int i = 0; i < dashCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this , skillData);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            Dash(i);

            targetList.Clear();

            yield return YieldCache.WaitForSeconds(skillData.useTime / (dashCount + 1));
        }

        ThrowSlash();

        yield return YieldCache.WaitForSeconds(skillData.useTime / (dashCount + 1));

        StopSkill();
    }

    //돌진 => 돌진하면서 손톱공격
    void Dash(int order)
    {
        //애니메이션 실행
        skillUser.ControlAnimTimeScale(skillData.animTimeScale);
        skillUser.PlayAnimation(skillData.animName);

        //플레이어가 랜덤 적으로 돌진
        Vector3 sPos = skillUser.transform.position;
        Vector3 targetPos = targetList[0].transform.position;

        Vector3 targetVec = new Vector3(targetPos.x, sPos.y, targetPos.z);
        Vector3 dirVec = new Vector3(targetPos.x - sPos.x, sPos.y, targetPos.z - sPos.z).normalized;
        Vector3 endValue = targetVec - dirVec;

        allObj.transform.LookAt(targetPos);
        clawCols[order].gameObject.SetActive(true);

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        skillUser.transform.DOMove(endValue, skillData.useTime / (dashCount + 1)).OnComplete(() => clawCols[order].gameObject.SetActive(false));
    }

    //IEnumerator ClawAttack()
    //{
    //    Vector3 targetPos = targetList[0].transform.position;

    //    allObj.transform.LookAt(targetPos);

    //    for (int i = 0; i < dashCount; i++)
    //    {
    //        //애니메이션 실행
    //        skillUser.ControlAnimTimeScale(skillData.animTimeScale);
    //        skillUser.PlayAnimation(skillData.animName);

    //        clawCols[i].gameObject.SetActive(true);

    //        yield return YieldCache.WaitForSeconds(skillData.useTime / 3);

    //        clawCols[i].gameObject.SetActive(false);
    //    }

    //    ThrowSlash();
    //}

    //마지막 공격 - 슬래쉬 날리기
    void ThrowSlash()
    {
        // 타겟 체크
        if (targetList.Count == 0)
        {
            skillUser.SetTarget(this, skillData);
        }

        //타겟이 없다면 종료
        if (targetList[0] == null) return;

        //애니메이션 재생
        skillUser.ControlAnimTimeScale(skillData.animTimeScale);
        skillUser.PlayAnimation(skillData.animName);

        Vector3 sPos = skillUser.transform.position;
        Vector3 targetPos = targetList[0].transform.position;
        Vector3 dirVec = new Vector3(targetPos.x - sPos.x, sPos.y, targetPos.z - sPos.z).normalized;

        allObj.transform.LookAt(targetPos);

        slashCol.gameObject.SetActive(true);

        slashCol.transform.DOMove(dirVec * skillData.atkRange, skillData.useTime / (dashCount + 1))
            .SetRelative().SetEase(Ease.InQuad).OnComplete(() => { slashCol.gameObject.SetActive(false); slashCol.transform.localPosition = Vector3.zero; });
    }


    public override void AddFirstUpgrade()
    {      
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {      
        //횟수 증가
        dashCount += (int)skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {      
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
