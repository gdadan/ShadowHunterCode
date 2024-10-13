using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAttackSkill : TargetingSkill
{
    [SerializeField] Collider arrivalCol; //착지 후 바닥 이펙트
    [SerializeField] GameObject jumpEffect; //점프할 때의 이펙트

    [SerializeField] float jumpPower; //점프 높이

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과를 위한 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(arrivalCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //에어본 효과 적용
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new AirborneDebuff(skillData.statusValue[0], target));
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        JumpAtaack();
    }

    //스킬 실행
    void JumpAtaack()
    {
        //플레이어가 범위 내 가장 먼 적 방향으로 점프
        Vector3 fPos = new Vector3(targetList[0].position.x, skillUser.transform.position.y, targetList[0].position.z);

        jumpEffect.SetActive(true);

        //useTime 중 3분의 2는 점프, 3분의 1은 도착 후
        skillUser.transform.DOMove(fPos,skillData.useTime / 2).SetEase(Ease.InQuart)
            .OnComplete(() => StartCoroutine(ReachGroundCoro()));
    }

    //바닥에 착지할 때
    IEnumerator ReachGroundCoro()
    {
        //점프이펙트 비활성, 바닥이펙트 활성
        jumpEffect.SetActive(false);
        arrivalCol.gameObject.SetActive(true);

        skillUser.skillUserAtkHandler.Target = null;

        yield return YieldCache.WaitForSeconds(skillData.useTime / 2);

        arrivalCol.gameObject.SetActive(false);

        StopSkill();
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

        //크기 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(arrivalCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}