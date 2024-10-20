using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandTornadoSkill : ActiveSkill, ITargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> galeCols = new List<Collider>(); //돌풍 콜라이더

    float galeDuration = 6; //날라가는 기간
    int galeCount = 1; //돌풍 개수

    public List<Transform> targetList { get; set; }

    private void Start()
    {
        //돌풍 생성, 크기 설정
        InitGale(galeCount);
        Utils.SetSkillRange(allObj, skillData.skillScale);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        AttackHandler target = other.GetComponent<AttackHandler>();

        //에어본 효과 실행
        if (target != null )
        {
            target.statusEffMgr.AddBuff(new AirborneDebuff(skillData.statusValue[0], target));
        }
    }

    public override void UseActiveSkill()
    {
        StartCoroutine(ThrowGaleCoro());
    }

    //스킬 실행 - 돌풍 날리기
    IEnumerator ThrowGaleCoro()
    {
        for (int i = 0; i < galeCount; i++)
        {
            // 타겟 체크
            if (targetList.Count == 0)
            {
                skillUser.SetTarget(this,skillData);
            }

            //타겟이 없다면 종료
            if (targetList[0] == null) break;

            yield return YieldCache.WaitForSeconds(skillData.useTime / galeCount);

            ThrowOneGale(i);
        }
        yield return YieldCache.WaitForSeconds(galeDuration);

        StopSkill();
    }

    //돌풍 하나 날리기
    void ThrowOneGale(int order)
    {
        //targets.Clear();

        galeCols[order].transform.position = skillUser.transform.position;

        galeCols[order].gameObject.SetActive(true);

        //위치 설정
        Vector3[] path = new Vector3[6];
        path[0] = targetList[0].transform.position;
        path[1] = path[0] + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6));
        path[2] = path[0] + new Vector3(Random.Range(0, 6), 0, -Random.Range(0, 6));
        path[3] = path[0] + new Vector3(-Random.Range(0, 6), 0, -Random.Range(0, 6));
        path[4] = path[0] + new Vector3(-Random.Range(0, 6), 0, Random.Range(0, 6));
        path[5] = targetList[0].transform.position;

        galeCols[order].transform.DOPath(path, galeDuration, PathType.CatmullRom).OnComplete(() => galeCols[order].gameObject.SetActive(false));

        targetList.Clear();
    }

    //돌풍 생성
    void InitGale(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider gale = Instantiate(originalCol, allObj.transform);
            galeCols.Add(gale);
        }
    }

    public override void AddFirstUpgrade()
    {
        skillData.skillDamage += skillData.firstUpgradeValue[0];

    }

    public override void AddSecondUpgrade()
    {
        galeCount += (int)skillData.secondUpgradeValue[0];
        InitGale((int)skillData.secondUpgradeValue[0]);
    }

    public override void AddThirdUpgrade()
    {
        skillData.atkRange *= 1 + skillData.thirdUpgradeValue[0];
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }
}
