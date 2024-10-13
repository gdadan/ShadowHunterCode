using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDanceSkill : ActiveSkill
{
    [SerializeField] GameObject allObj; //검기 전체 오브젝트
    [SerializeField] GameObject slashEffect; //캐릭터 주위 휘두르는 이펙트
    [SerializeField] Collider slashCol; //생성할 검기 콜라이더
    List<Collider> slashCols = new List<Collider>(); //날아가는 검기 콜라이더들

    [SerializeField] float slashDuration; //검기 날아가는 시간
    [SerializeField] float divisor; 

    int slashCount = 15; //검기 개수
    
    bool hasBonusDamage = false; //출혈 시 추가피해 여부 => 스킬 업그레이드 시 활성

    private void Start()
    {
        //검기 생성, 크기 설정
        InitSlash(slashCount);
        Utils.SetSkillRange(allObj, skillData.skillScale);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        //출혈중인 적이면 추가피해
        if (hasBonusDamage)
        {
            AttackHandler target = other.GetComponent<AttackHandler>();
            StatusEffect bleed = target.statusEffMgr.activeBuffs.Find(buff => buff.target == target && buff.statusEffectType == StatusEffect.StatusEffectType.Bleed);

            if (target != null && bleed != null)
            {
                target.GetComponent<IDamageable>().OnDamaged(damage * skillData.firstUpgradeValue[0], false);
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(ThrowSlashCoro());
    }

    //스킬 실행 - 검기 날리기
    IEnumerator ThrowSlashCoro()
    {
        slashEffect.SetActive(true);

        for (int i = 0; i < slashCount; i++)
        {
            ThrowOneSlash(i);

            yield return YieldCache.WaitForSeconds(skillData.useTime / slashCount);
        }
        slashEffect.SetActive(false);

        //마지막 검기 날아가기 위한 시간
        yield return YieldCache.WaitForSeconds(slashDuration);

        StopSkill();
    }

    // 검기 하나 날리기
    void ThrowOneSlash(int order)
    {
        Vector3 sPos = slashCols[order].transform.localPosition;

        float index = order % (slashCount / divisor);
        Vector3 dirVec = new Vector3(Mathf.Cos(Mathf.PI * 2 * index / (slashCount / divisor)), sPos.y, Mathf.Sin(Mathf.PI * 2 * index / (slashCount / divisor))).normalized;

        slashCols[order].transform.rotation = Quaternion.LookRotation(dirVec);
        slashCols[order].gameObject.SetActive(true);

        slashCols[order].transform.DOMove(dirVec * skillData.atkRange, slashDuration)
            .SetRelative().SetEase(Ease.InQuad).OnComplete(() => { slashCols[order].gameObject.SetActive(false); slashCols[order].transform.localPosition = sPos; });
    }

    //검기 생성
    void InitSlash(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider slash = Instantiate(slashCol, allObj.transform);
            slashCols.Add(slash);
        }
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //출혈 효과 적에게 추가 피해
        hasBonusDamage = true;
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //개수 증가
        slashCount += (int)skillData.secondUpgradeValue[0];
        InitSlash((int)skillData.secondUpgradeValue[0]);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //개수 증가
        slashCount += (int)skillData.thirdUpgradeValue[0];
        InitSlash((int)skillData.thirdUpgradeValue[0]);
    }
}
