using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public List<StatusEffect> activeBuffs = new List<StatusEffect>();

    //상태효과 추가
    public void AddBuff(StatusEffect statusEffect)
    {
        //1.버프가 이미 적용되고 있을 경우 2. 중첩가능한 경우 -> 시간 추가
        StatusEffect existingBuff = activeBuffs.Find(buff => buff.target == statusEffect.target && buff.statusEffectType == statusEffect.statusEffectType);
        
        if (existingBuff != null)
        {
            if (existingBuff.isDuplicated)
            {
                existingBuff.duration += statusEffect.duration;
            }
            return;
        }

        activeBuffs.Add(statusEffect);

        //파티클 가지고 있는 경우
        if (statusEffect.hasParticle)
        {
            statusEffect.particle = EffectManager.Instance.GetParticle((int)statusEffect.statusEffectType);
            //파티클 위치 변경
            statusEffect.particle.transform.SetParent(transform);

            if(gameObject.CompareTag("Enemy"))
            statusEffect.particle.transform.localPosition = transform.GetComponent<Enemy>().hitEffect.transform.localPosition;
        }

        StartCoroutine(EffectCoroutine(statusEffect));
    }

    //상태효과 제거
    public void RemoveBuff(StatusEffect statusEffect)
    {
        activeBuffs.Remove(statusEffect);
    }

    //상태효과 전체 제거
    public void RemoveAllBuff()
    {
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            //코루틴 정지
            StopCoroutine(EffectCoroutine(activeBuffs[i]));

            //파티클 있을 시 제거
            if (activeBuffs[i].particle != null)
            {
                EffectManager.Instance.ReturnParticle((int)activeBuffs[i].statusEffectType, activeBuffs[i].particle);
            }

            activeBuffs[i].Remove();
        }
        activeBuffs.Clear();
    }

    //상태효과 실행
    public IEnumerator EffectCoroutine(StatusEffect statusEffect)
    {
        statusEffect.Active();

        //파티클 있을 경우 실행
        if (statusEffect.particle != null)
        {
            statusEffect.particle.Play();
        }

        float elapsedTime = 0;

        while (elapsedTime < statusEffect.duration)
        {
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        //파티클 있을 경우 중지, 풀에 반환
        if (statusEffect.particle != null)
        {
            statusEffect.particle.Stop();
            EffectManager.Instance.ReturnParticle((int)statusEffect.statusEffectType, statusEffect.particle);
        }

        statusEffect.Remove();
        RemoveBuff(statusEffect);
    }
}