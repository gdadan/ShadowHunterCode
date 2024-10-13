using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashClawSkill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] List<Collider> clawCols; //손톱 콜라이더

    int attackCount = 3; //공격 횟수

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(allObj, skillData.skillScale);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        Dash();
    }

    //스킬 실행 - 적을 향해 돌진 후 공격
    void Dash()
    {
        //플레이어가 가장 먼 적 방향으로 atkRange만큼 돌진
        Vector3 sPos = skillUser.transform.position;
        Vector3 targetPos = targetList[0].transform.position;

        Vector3 targetVec = new Vector3(targetPos.x, sPos.y, targetPos.z);
        Vector3 dirVec = new Vector3(targetPos.x- sPos.x, sPos.y, targetPos.z - sPos.z).normalized;
        Vector3 endValue = targetVec - dirVec;

        allObj.transform.LookAt(targetPos);

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        skillUser.transform.DOMove(endValue, skillData.useTime / (attackCount + 1)).SetEase(Ease.InQuad).OnComplete(() => StartCoroutine(AttackCoro()));
    }

    //대쉬 끝난 후 공격
    IEnumerator AttackCoro()
    {
        skillUser.skillUserAtkHandler.Target = null;

        //애니메이션 실행
        skillUser.ControlAnimTimeScale(0.8f);
        skillUser.PlayAnimation("attack2-1");

        for (int i = 0; i < attackCount; i++)
        {
            clawCols[i].gameObject.SetActive(true);

            yield return YieldCache.WaitForSeconds(skillData.useTime / (attackCount + 1));

            clawCols[i].gameObject.SetActive(false);
        }

        StopSkill();
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

        //횟수 증가
        attackCount += (int)skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
