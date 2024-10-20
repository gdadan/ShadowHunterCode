using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackHandler;


public class BloodChainSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] GameObject[] lineTarget; //이펙트 라인렌더러 움직이는 타겟
    [SerializeField] Collider[] lineCol; //콜라이더

    List<AttackHandler> targets = new List<AttackHandler>();

    IEnumerator enumerator;

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        enumerator = PullSkillCoro();
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

    public override void InitActiveSkill(SkillHandler _skillUser, ActiveSkillData _skillData, int skillLevel)
    {
        skillCollList.AddRange(lineCol);
        base.InitActiveSkill(_skillUser, _skillData, skillLevel);
    }

    public override void UseActiveSkill()
    {      
        targets.Clear();

        StartCoroutine(PullSkillCoro());
    }

    //스킬 실행
    IEnumerator PullSkillCoro()
    {
        SetLine();

        for (int i = 0; i < targetList.Count; i++)
            lineCol[i].transform.parent.parent.gameObject.SetActive(true);

        //적 멈추는 기능
        foreach (Transform target in targetList)
        {
            AttackHandler targetAtkHandler = target.GetComponent<AttackHandler>();

            if (targetAtkHandler.isLive)
                targetAtkHandler.StopCharacter(skillData.useTime);
        }

        StartCoroutine(CheckTargetLiveCoro());

        yield return YieldCache.WaitForSeconds(skillData.useTime);

        //닿은 적 모두 끌어당기기
        foreach (AttackHandler target in targets)
        {
            if (target.isLive)
            {
                StartCoroutine(PullEnemy(target, 10f));

                //출혈 효과 적용
                target.statusEffMgr.AddBuff(new BleedDebuff(skillData.statusValue[0], target, skillData.statusValue[1], damage * skillData.statusValue[2]));
            }
        }

        for (int i = 0; i < targetList.Count; i++)
            lineCol[i].transform.parent.parent.gameObject.SetActive(false);

        //적 속도 초기화를 위한 시간
        yield return YieldCache.WaitForSeconds(0.2f);

        StopSkill();
    }

    //적 당기기
    IEnumerator PullEnemy(AttackHandler target, float force)
    {
        //플레이어를 향해 적 끌어당기기
        Vector3 dirVec = transform.position - target.transform.position;
        Rigidbody rigid = target.rigid;

        float distance = Vector3.Magnitude(dirVec);

        //플레이어와 너무 가까우면 당기는 힘 감소
        if (distance < 1f)
        {
            force = force / 10;
        }

        rigid.AddForce(dirVec.normalized * force * distance, ForceMode.Impulse);

        for (int i = 0; i < targetList.Count; i++)
        {
            lineTarget[i].transform.position = transform.position;
        }

        target.OnDamaged(damage, false);

        yield return YieldCache.WaitForSeconds(0.1f);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    //타겟 죽은 상태 체크
    IEnumerator CheckTargetLiveCoro()
    {
        yield return YieldCache.WaitForSeconds(skillData.useTime * 0.5f);

        int offCount = 0;

        for (int i = 0; i < targetList.Count; i++)
        {
            AttackHandler targetAtkHandler = targetList[i].GetComponent<AttackHandler>();

            if (!targetAtkHandler.isLive)
            {
                lineCol[i].transform.parent.parent.gameObject.SetActive(false);
                offCount++;
            }
        }

        if (offCount == targetList.Count)
        {
            StopCoroutine(enumerator);

            skillUser.skillUserAtkHandler.ChangeState(AttackState.Find);

            StopSkill();
        }

    }

    void SetLine()
    {
        //Collider[] targetCols = Physics.OverlapSphere(transform.position, skillData.skillData.sUseCondValue, targetLayer);
        //Transform targetCol = Utils.GetFarthest(attackHandler.rigid.position, skillData.skillData.sUseCondValue, targetLayer);
        //Collider[] targetArray = new Collider[lineCol.Length]; //타겟을 모두 다르게 하기 위한 저장

        ////가장 먼 적 최대 5명 설정
        //for (int i = 0; i < lineCol.Length; i++)
        //{
        //    if (targetCol == null)
        //        break;

        //    targetList.Add(targetCol.GetComponent<Enemy>());
        //    targetArray[i] = targetCol.GetComponent<Collider>();

        //    Collider[] newTargetCols = targetCols.Except(targetArray).ToArray();
        //    //targetCol = Utils.GetFarthest(newTargetCols);
        //}

        //   Transform[] targets = Utils.GetMultiFarthest(attackHandler.rigid.position, skillData.skillData.sUseCondValue, targetLayer, lineCol.Length);

        //라인렌더러 타겟 위치, 콜라이더 설정
        for (int i = 0; i < targetList.Count; i++)
        {
            lineTarget[i].transform.position = targetList[i].transform.position + Vector3.up * 0.2f;

            lineCol[i].GetComponent<BoxCollider>().size = new Vector3(1 + Vector3.Distance(allObj.transform.position, lineTarget[i].transform.position), 1, 1);
            lineCol[i].transform.position = (allObj.transform.position + lineTarget[i].transform.position) / 2;
            lineCol[i].transform.LookAt(lineTarget[i].transform.position);
            lineCol[i].transform.Rotate(new Vector3(0, 90, 0));
        }
    }

    public override void AddFirstUpgrade()
    {     
        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {     
        //출혈데미지 증가
        skillData.statusValue[2] += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {      
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
