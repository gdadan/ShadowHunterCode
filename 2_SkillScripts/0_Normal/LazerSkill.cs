using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerSkill : TargetingSkill
{
    [SerializeField] Collider lazerCol;

    float delayTime = 1f; // 스킬 실행 딜레이 시간 (발사 전 이펙트 시간)

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과 적용을 위한 리스트

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(lazerCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //넉백 효과 적용
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new KnockBack(skillData.statusValue[0], target, skillData.statusValue[1], skillUser));
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        targets.Clear();

        StartLazer();
    }
    
    //스킬 실행
    void StartLazer()
    {
        //가장 가까운 적의 방향으로 레이저 발사
        //delayTime + duration = useTime
        lazerCol.transform.LookAt(targetList[0].position);

        StartCoroutine(CastDoTSkillCoro(lazerCol, delayTime));
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
        Utils.SetSkillRange(lazerCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}