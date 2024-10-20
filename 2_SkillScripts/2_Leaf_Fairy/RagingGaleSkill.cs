using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagingGaleSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> areaCols = new List<Collider>(); //돌풍 콜라이더들

    float galeDuration = 2f; //날라가는 시간
    int galeCount = 1; //돌풍 개수

    List<AttackHandler> targets = new List<AttackHandler>(); //상태 효과를 위한 리스트

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        //크기 설정, 돌풍 생성
        InitGale(galeCount);
        Utils.SetSkillRange(allObj, skillData.skillScale);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //에어본 효과 실행
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            target.statusEffMgr.AddBuff(new AirborneDebuff(skillData.statusValue[0], target));
        }
    }

    public override void UseActiveSkill()
    {      
        StartCoroutine(ThrowGaleCoro());
    }

    //IEnumerator StartAoESkill()
    //{
    //    //플레이어 위치에서 시작
    //    transform.position = skillUser.transform.position;

    //    Vector3 dirVec = targetList[0].transform.position - transform.position;
    //    Rigidbody rigid = GetComponent<Rigidbody>();

    //    areaCol.gameObject.SetActive(true);

    //    //돌풍을 가장 가까운 적 방향으로 날림
    //    rigid.AddForce(dirVec.normalized * force, ForceMode.Impulse);

    //    yield return YieldCache.WaitForSeconds(2f);

    //    rigid.velocity = Vector3.zero;
    //    rigid.angularVelocity = Vector3.zero;

    //    areaCol.gameObject.SetActive(false);
    //    StopSkill();
    //}

    //스킬 실행 - 돌풍 날리기
    IEnumerator ThrowGaleCoro()
    {
        for (int i = 0; i < galeCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this, skillData);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            yield return YieldCache.WaitForSeconds(skillData.useTime / galeCount);

            ThrowOneGale(i);
        }
        //마지막 날아가기 위한 시간
        yield return YieldCache.WaitForSeconds(galeDuration);

        StopSkill();
    }

    //돌풍 하나 날리기
    void ThrowOneGale(int order)
    {
        targets.Clear();

        //Vector3 sPos = areaCols[order].transform.localPosition;
        areaCols[order].transform.position = skillUser.transform.position;
        Vector3 dirVec = targetList[0].transform.position - areaCols[order].transform.position;

        areaCols[order].gameObject.SetActive(true);

        areaCols[order].transform.DOMove(dirVec * skillData.atkRange, galeDuration)
            .SetRelative().SetEase(Ease.InQuad).OnComplete(() => areaCols[order].gameObject.SetActive(false));

        targetList.Clear();
    }

    //돌풍 생성
    void InitGale(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider gale = Instantiate(originalCol, allObj.transform);
            areaCols.Add(gale);
        }
    }

    public override void AddFirstUpgrade()
    {      
        //데미지 증가
        skillData.skillDamage += skillData.firstUpgradeValue[0];

    }

    public override void AddSecondUpgrade()
    {     
        //개수 증가
        galeCount += (int)skillData.secondUpgradeValue[0];
        InitGale((int)skillData.secondUpgradeValue[0]);
    }

    public override void AddThirdUpgrade()
    {     
        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
