using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackHandler;
using DG.Tweening;

public abstract class StatusEffect
{
    public enum StatusEffectType { Stun, Airborne, KnockBack, Freeze, Burn, Bleed, AtkBoost, CriBoost, Haste }

    public StatusEffectType statusEffectType;

    public bool isDuplicated; //효과 중첩 가능 여부
    public bool hasParticle = false; //파티클 여부

    public float duration; //효과 시간
    public AttackHandler target; //적용할 타겟

    public ParticleSystem particle;

    public StatusEffect(float duration, AttackHandler target)
    {
        this.duration = duration;
        this.target = target;
    }

    public abstract void Active();
    public abstract void Remove();
}

//기절 효과
public class StunDebuff : StatusEffect
{
    public StunDebuff(float duration, AttackHandler target) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Stun;
        isDuplicated = true;
        hasParticle = true;
    }

    public override void Active()
    {
        //target 움직임 x
        target.ChangeState(AttackState.Stop);
    }

    public override void Remove()
    {
        if (!target.isLive)
            return;

        //target 다시 움직임
        target.ChangeState(AttackState.Find);
    }
}

//공중에 뜸 효과
public class AirborneDebuff : StatusEffect
{
    Vector3 oriPos; //원래 위치

    Sequence sequence = DOTween.Sequence();

    public AirborneDebuff(float duration, AttackHandler target) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Airborne;
        isDuplicated = false;
    }

    public override void Active()
    {
        //움직임 x, 공중에 뜨고 제자리로
        target.ChangeState(AttackState.Stop);

        oriPos = target.transform.position;
        sequence.Append(target.transform.DOMoveY(oriPos.y + 1, duration * 0.5f)).OnComplete(() => target.transform.DOMoveY(oriPos.y, duration * 0.5f));
    }

    public override void Remove()
    {
        if (sequence != null)
        {
            sequence.Kill();
            target.transform.DOMoveY(oriPos.y, 0.2f);
        }

        if (!target.isLive)
            return;

        //다시 움직임
        target.ChangeState(AttackState.Find);
    }
}

//넉백 효과
public class KnockBack : StatusEffect
{
    float knockbackForce; //넉백 힘

    SkillHandler skillUser; //사용자

    public KnockBack(float duration, AttackHandler target, float knockbackForce, SkillHandler skillUser) : base(duration, target)
    {
        statusEffectType = StatusEffectType.KnockBack;
        isDuplicated = false;

        this.knockbackForce = knockbackForce;
        this.skillUser = skillUser;
    }

    public override void Active()
    {
        //넉백
        //target.ChangeState(AttackState.Stop);
        StartKnockBack();
    }

    public override void Remove()
    {
        //넉백 제거
        target.rigid.velocity = Vector3.zero;
        target.rigid.angularVelocity = Vector3.zero;
        //target.ChangeState(AttackState.Find);
    }

    // 넉백
    void StartKnockBack()
    {
        Vector3 userPos = skillUser.transform.position;
        Vector3 dirVec = target.transform.position - userPos;

        target.rigid.AddForce(dirVec.normalized * knockbackForce, ForceMode.Impulse);
    }
}

//빙결 효과 (공속, 이속 감소)
public class FreezeDebuff : StatusEffect
{
    float atkSpeedDecrese; //공속 감소량
    float moveSpeedDecrese; //이속 감소량

    public FreezeDebuff(float duration, AttackHandler target, float atkSpeedDecrese, float moveSpeedDecrese) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Freeze;
        isDuplicated = true;
        hasParticle = true;

        this.atkSpeedDecrese = atkSpeedDecrese;
        this.moveSpeedDecrese = moveSpeedDecrese;
    }

    public override void Active()
    {
        // target 공속, 이속 감소
        target.atkStatus.atkSpeed *= (1 - atkSpeedDecrese);
        target.atkStatus.moveSpeed *= (1 - moveSpeedDecrese);
    }

    public override void Remove()
    {
        // 빙결 효과 제거
        target.atkStatus.atkSpeed /= (1 - atkSpeedDecrese);
        target.atkStatus.moveSpeed /= (1 - moveSpeedDecrese);
    }
}

//화상 효과
public class BurnDebuff : StatusEffect
{
    float burnIntevalTime; //화상 피해 입는 간격
    float burnDamage; //화상 데미지

    IEnumerator burnCoro;

    public BurnDebuff(float duration, AttackHandler target, float burnIntevalTime, float burnDamage) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Burn;
        isDuplicated = true;
        hasParticle = true;

        this.burnIntevalTime = burnIntevalTime;
        this.burnDamage = burnDamage;

        burnCoro = Utils.DoTAttack(target, burnDamage, duration, burnIntevalTime);
    }

    public override void Active()
    {
        //화상 효과
        target.StartCoroutine(burnCoro);
    }

    public override void Remove()
    {
        //화상 효과 제거
        target.StopCoroutine(burnCoro);
    }
}

public class BleedDebuff : StatusEffect
{
    float bleedIntevalTime; //출혈 피해 입는 간격
    float bleedDamage; //출혈 데미지

    IEnumerator bleedCoro;

    public BleedDebuff(float duration, AttackHandler target, float bleedIntevalTime, float bleedDamage) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Bleed;
        isDuplicated = true;
        hasParticle = true;

        this.bleedIntevalTime = bleedIntevalTime;
        this.bleedDamage = bleedDamage;

        bleedCoro = Utils.DoTAttack(target, bleedDamage, duration, bleedIntevalTime);
    }

    public override void Active()
    {
        //출혈 효과
        target.StartCoroutine(bleedCoro);
    }

    public override void Remove()
    {
        //출혈 효과 제거
        target.StopCoroutine(bleedCoro);
    }
}

//공격력 증가 효과
public class AttackBuff : StatusEffect
{
    float atkIncrease; //공격력 증가량

    public AttackBuff(float duration, AttackHandler target, float atkIncrease) : base(duration, target)
    {
        statusEffectType = StatusEffectType.AtkBoost;
        isDuplicated = true;

        this.atkIncrease = atkIncrease;
    }

    public override void Active()
    {
        // target 공격력 증가
        target.atkStatus.atk *= (1 + atkIncrease);
    }

    public override void Remove()
    {
        target.atkStatus.atk /= (1 + atkIncrease);
    }
}

//크리티컬 데미지, 크리티컬 확률 증가 효과
public class CriticalBuff : StatusEffect
{
    float criProbIncrease; //크확 증가량
    float criDamageIncrease; //크뎀 증가량

    public CriticalBuff(float duration, AttackHandler target, float criProbIncrease, float criDamageIncrease) : base(duration, target)
    {
        statusEffectType = StatusEffectType.CriBoost;
        isDuplicated = true;

        this.criProbIncrease = criProbIncrease;
        this.criDamageIncrease = criDamageIncrease;
    }

    public override void Active()
    {
        // target 크리티컬 확률, 크리티컬 데미지 증가
        target.atkStatus.criProb *= (1 + criProbIncrease);
        target.atkStatus.criDamage *= (1 + criDamageIncrease);
    }

    public override void Remove()
    {
        target.atkStatus.criProb /= (1 + criProbIncrease);
        target.atkStatus.criDamage /= (1 + criDamageIncrease);
    }
}

//공속, 이속 증가 효과
public class HasteBuff : StatusEffect
{
    float atkSpeedIncrese; //공속 증가량 (%)
    float moveSpeedIncrese; //이속 증가량 (%)

    public HasteBuff(float duration, AttackHandler target, float atkSpeedIncrese, float moveSpeedIncrese) : base(duration, target)
    {
        statusEffectType = StatusEffectType.Haste;
        isDuplicated = true;

        this.atkSpeedIncrese = atkSpeedIncrese;
        this.moveSpeedIncrese = moveSpeedIncrese;
    }

    public override void Active()
    {
        // target 공속, 이속 증가
        target.atkStatus.atkSpeed *= (1 + atkSpeedIncrese);
        target.atkStatus.moveSpeed *= (1 + moveSpeedIncrese);
    }

    public override void Remove()
    {
        target.atkStatus.atkSpeed /= (1 + atkSpeedIncrese);
        target.atkStatus.moveSpeed /= (1 + moveSpeedIncrese);
    }
}
