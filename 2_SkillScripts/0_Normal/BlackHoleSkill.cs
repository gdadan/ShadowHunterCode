using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BlackHoleSkill : ActiveSkill , ITargetingSkill
{
    [SerializeField] Collider areaCol;

    List<AttackHandler> targets = new List<AttackHandler>();

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

        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    public override void UseActiveSkill()
    {
        targets.Clear();

        StartCoroutine(StartBloodHole());

        //SoundManager.instance.PlaySfx(SoundManager.Sfx.V_BlackHole, 1f);
    }

    IEnumerator StartBloodHole()
    {
        float checkTime = 0f;

        areaCol.transform.position = targetList[0].position;
        areaCol.gameObject.SetActive(true);

        while (checkTime <= skillData.duration)
        {
            foreach (AttackHandler target in targets)
            {
                if (target.isLive)
                    StartCoroutine(AttractEnemy(target, 3f));
            }

            yield return YieldCache.WaitForSeconds(skillData.intervalTime);

            checkTime += skillData.intervalTime;
        }

        areaCol.gameObject.SetActive(false);

        yield return YieldCache.WaitForSeconds(0.2f);

        StopSkill();
    }

    // 끌어당기기
    IEnumerator AttractEnemy(AttackHandler target, float force)
    {
        yield return YieldCache.WaitForFixedUpdate; // 다음 하나의 물리 프레임 딜레이

        Vector3 dirVec = areaCol.transform.position - target.transform.position;
        Rigidbody rigid = target.rigid;

        rigid.AddForce(dirVec.normalized * force, ForceMode.Impulse);

        target.OnDamaged(damage, false);

        yield return YieldCache.WaitForSeconds(0.1f);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    public override void AddFirstUpgrade()
    {
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        //범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(areaCol.gameObject, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }

}
