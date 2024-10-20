using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafStromSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] Collider areaCol;

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과를 위한 리스트

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //타겟이 상태 효과가 적용되고 있지 않을 때 실행, 에어본 효과
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            StartAirborne(target);
        }
    }

    //스킬 종료 시 적들도 에어본 종료
    private void OnDisable()
    {
        foreach (var handler in targets)
        {
            StatusEffect airborne = handler.statusEffMgr.activeBuffs.Find(buff => buff.statusEffectType == StatusEffect.StatusEffectType.Airborne);

            if (airborne != null)
            {
                airborne.Remove();
                handler.statusEffMgr.RemoveBuff(airborne);
            }
        }
    }

    public override void UseActiveSkill()
    {    
        targets.Clear();

        StartAoESkill();
    }

    //스킬 실행
    void StartAoESkill()
    {
        areaCol.transform.position = targetList[0].position;

        StartCoroutine(CastDoTSkillCoro(areaCol, skillData.useTime));
    }

    //에어본 효과
    void StartAirborne(AttackHandler target)
    {
        AirborneDebuff airborne = new AirborneDebuff(skillData.duration, target);
        target.statusEffMgr.AddBuff(airborne);
    }

    //에어본 효과 실행
    //IEnumerator StartAirborneCoro(AttackHandler target)
    //{
    //    float checkTime = 0;

    //    AirborneDebuff airborne = new AirborneDebuff(0.3f, target);
    //    target.statusEffMgr.AddBuff(airborne);

    //    //intervalTime 마다 띄우는 시간 추가
    //    while (checkTime < skillData.duration)
    //    {
    //        yield return YieldCache.WaitForSeconds(skillData.intervalTime);

    //        checkTime += skillData.intervalTime;
    //        airborne.duration += skillData.intervalTime;
    //    }
    //}

    public override void AddFirstUpgrade()
    {
      
        //범위 증가
        skillData.atkRange *= 1 + skillData.firstUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddSecondUpgrade()
    {
      
        //지속 시간 증가
        skillData.duration *= 1 + skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
       
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
