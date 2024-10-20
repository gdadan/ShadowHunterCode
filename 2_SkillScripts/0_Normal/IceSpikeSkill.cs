using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpikeSkill : ActiveSkill , ITargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> areaCols = new List<Collider>(); //얼음 가시 콜라이더

    float duration = 1.3f; //오브젝트 꺼질 때까지 지연시간
    int iceCount = 1; //얼음가시 개수

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과를 위한 리스트

    public List<Transform> targetList { get ; set ; }

    private void Start()
    {
        InitIceSpike(iceCount);
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //빙결 효과 적용
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new FreezeDebuff(skillData.statusValue[0], target, skillData.statusValue[1], skillData.statusValue[1]));
        }
    }

    public override void UseActiveSkill()
    {      
        targets.Clear();

        StartCoroutine(AoESkillCoro());
    }

    //스킬 실행
    IEnumerator AoESkillCoro()
    {
        for (int i = 0; i < iceCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this , skillData);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            StartAoESkill(i);

            //타겟 초기화
            //targetList.Clear();

            yield return YieldCache.WaitForSeconds(skillData.useTime / iceCount);
        }

        //이펙트 재생을 위한 시간
        yield return YieldCache.WaitForSeconds(skillData.useTime + duration);

        StopSkill();
    }

    void StartAoESkill(int order)
    {
        //일정 범위 내 적에게 광역 공격
        areaCols[order].transform.position = targetList[0].position;

        StartCoroutine(CastSkillCoro(areaCols[order], skillData.useTime, duration));

        targetList.Clear();
    }

    //iceSpike 생성
    void InitIceSpike(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider iceSpike = Instantiate(originalCol, allObj.transform);
            areaCols.Add(iceSpike);
        }
    }

    public override void AddFirstUpgrade()
    {  
        //개수 증가
        iceCount += (int)skillData.firstUpgradeValue[0];
        InitIceSpike((int)skillData.firstUpgradeValue[0]);
    }

    public override void AddSecondUpgrade()
    {     
        //빙결 시간 증가
        skillData.statusValue[0] += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {   
        //개수 증가
        iceCount += (int)skillData.thirdUpgradeValue[0];
        InitIceSpike((int)skillData.thirdUpgradeValue[0]);
    }
}
