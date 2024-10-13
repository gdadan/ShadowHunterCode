using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderDashSkill : TargetingSkill
{
    [SerializeField] Collider dashCol; //돌진할 때 콜라이더
    [SerializeField] Collider slashCol; //슬래쉬 콜라이더

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(slashCol.gameObject, skillData.skillScale);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        Dash();
    }

    //스킬 실행 - 대쉬 -> 슬래쉬
    void Dash()
    {
        Transform skillUserTrf = skillUser.transform;

        //플레이어가 가장 가까운 적의 방향으로 atkRange만큼 돌진
        Vector3 dirVec = new Vector3(targetList[0].position.x - skillUserTrf.position.x,
            skillUserTrf.position.y,
            targetList[0].position.z - skillUserTrf.position.z);

        float distance = skillData.atkRange;
        Vector3 endValue = dirVec.normalized * distance;

        dashCol.gameObject.SetActive(true);

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        skillUserTrf.DOMove(endValue, skillData.useTime / 2).SetRelative().OnComplete(() => StartCoroutine(SlashCoro()));
    }

    IEnumerator SlashCoro()
    {
        skillUser.skillUserAtkHandler.Target = null;
        dashCol.gameObject.SetActive(false);

        // 애니메이션 실행
        skillUser.ControlAnimTimeScale(1f);
        skillUser.PlayAnimation("attack2");

        //플립 설정
        skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

        slashCol.transform.LookAt(targetList[0].position);
        slashCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime / 2);

        slashCol.gameObject.SetActive(false);
        StopSkill();
    }

    ////슬래쉬 스킬
    //void Slash()
    //{
    //    skillUser.skillUserAtkHandler.Target = null;
    //    targetList.Clear();

    //    dashCol.gameObject.SetActive(false);

    //    // 타겟 체크
    //    if (targetList.Count == 0)
    //    {
    //        skillUser.SetTarget(this);
    //    }

    //    //타겟이 없다면 종료
    //    if (targetList[0] == null) return;

    //    // 애니메이션 실행
    //    skillUser.ControlAnimTimeScale(1f);
    //    skillUser.PlayAnimation("attack2");

    //    slashCol.transform.LookAt(targetList[0].position);
    //    slashCol.gameObject.SetActive(true);

    //    //플레이어가 가장 가까운 적을 향해 돌진
    //    Vector3 sPos = skillUser.transform.position;
    //    Vector3 targetPos = targetList[0].transform.position;

    //    Vector3 targetVec = new Vector3(targetPos.x, sPos.y, targetPos.z);
    //    Vector3 dirVec = new Vector3(targetPos.x - sPos.x, sPos.y, targetPos.z - sPos.z).normalized;
    //    Vector3 endValue = targetVec - dirVec;

    //    //플립 설정
    //    skillUser.skillUserAtkHandler.SetFlip(targetList[0]);

    //    skillUser.transform.DOMove(endValue, skillData.useTime / 2).OnComplete(() => { slashCol.gameObject.SetActive(false); StopSkill(); });
    //}

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(slashCol.gameObject, skillData.skillScale);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
