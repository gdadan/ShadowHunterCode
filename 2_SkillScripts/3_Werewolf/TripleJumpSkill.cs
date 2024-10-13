using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleJumpSkill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> floorCols = new List<Collider>(); //착지 후 바닥 콜라이더

    int jumpCount = 3; //점프 횟수

    List<AttackHandler> targets = new List<AttackHandler>(); //상태효과를 위한 리스트

    bool hasFreeze = false; //동상효과 가지고 있는지 여부 => 스킬 업그레이드 시 활성

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(allObj, skillData.atkRange);
        InitFloorEffect(jumpCount);
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

            //동상 효과
            if (hasFreeze)
            {
                target.statusEffMgr.AddBuff(new FreezeDebuff(skillData.secondUpgradeValue[0], target,
                    skillData.secondUpgradeValue[1], skillData.secondUpgradeValue[1]));
            }
        }
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(MultiJumpCoro());
    }

    //스킬 실행
    IEnumerator MultiJumpCoro()
    {
        for (int i = 0; i < jumpCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            JumpAtaack(i);

            //애니메이션 실행
            skillUser.ControlAnimTimeScale(skillData.animTimeScale);
            skillUser.PlayAnimation(skillData.animName);

            yield return YieldCache.WaitForSeconds(skillData.useTime);
        }

        yield return YieldCache.WaitForSeconds(1f);

        StopSkill();
    }

    void JumpAtaack(int order)
    {
        //플레이어가 범위 내 가장 먼 적 방향으로 점프
        Vector3 endPos = new Vector3(targetList[0].position.x, skillUser.transform.position.y, targetList[0].position.z);

        //useTime 중 2분의 1는 점프, 2분의 1은 도착 후
        skillUser.transform.DOMove(endPos, skillData.useTime / 2).SetEase(Ease.InQuart)
            .OnComplete(() => StartCoroutine(ReachGround(order, endPos)));

        targetList.Clear();
    }

    //바닥에 착지할 때
    IEnumerator ReachGround(int order, Vector3 endPos)
    {
        floorCols[order].transform.position = endPos;
        floorCols[order].gameObject.SetActive(true);

        skillUser.skillUserAtkHandler.Target = null;

        yield return YieldCache.WaitForSeconds(skillData.useTime / 2);

        floorCols[order].gameObject.SetActive(false);
        targets.Clear();
    }

    //바닥 이펙트 생성
    void InitFloorEffect(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider floorEffect = Instantiate(originalCol, allObj.transform);
            floorCols.Add(floorEffect);
        }
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

        //상태이상 추가
        hasFreeze = true;
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.thirdUpgradeValue[0];
        Utils.SetSkillRange(allObj.gameObject, skillData.atkRange);
    }
}
