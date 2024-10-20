using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafBoltSkill : ActiveSkill
{
    [SerializeField] GameObject projectileObj; //투사체 전체 오브젝트
    [SerializeField] FairyProjectile projectile; //투사체 생성할 콜라이더
    List<FairyProjectile> projectiles = new List<FairyProjectile>(); //투사체 콜라이더

    [SerializeField] float force; //투사체 발사 속도
    int proCount = 10;

    private void Start()
    {
        //크기 설정, 투사체 생성
        Utils.SetSkillRange(projectileObj, skillData.skillScale);
        InitProjectile(proCount);
    }

    public override void UseActiveSkill()
    {       
        StartCoroutine(ThrowProjectileCoro());
    }

    // 스킬 실행
    IEnumerator ThrowProjectileCoro()
    {
        yield return YieldCache.WaitForSeconds(skillData.useTime - 0.05f);

        for (int i = 0; i < proCount; i++)
        {
            ThrowOneProjectile(i);
        }

        //투사체 날아가는 시간
        yield return YieldCache.WaitForSeconds(2f);

        for (int i = 0; i < proCount; i++)
            projectiles[i].DeactiveProjectile();

        StopSkill();
    }

    // 투사체 하나 날리기
    void ThrowOneProjectile(int order)
    {
        Vector3 sPos = projectiles[order].transform.localPosition;

        Vector3 dirVec = new Vector3(Mathf.Cos(Mathf.PI * 2 * order / proCount), sPos.y, Mathf.Sin(Mathf.PI * 2 * order / proCount)).normalized;

        projectiles[order].gameObject.SetActive(true);
        projectiles[order].Init(force, dirVec, skillUser.skillUserAtkHandler);
       
       
    }

    // 투사체 하나 날리기
    //void ThrowOneProjectile(int order)
    //{
    //    Vector3 dirVec = targetList[0].transform.position - projectiles[order].transform.position;
    //    dirVec = dirVec.normalized;

    //    //랜덤 방향으로 투사체 하나 발사
    //    float angle = Random.Range(-2f, 2f);

    //    Vector3 normalDir = new Vector3(dirVec.z * angle, dirVec.y, -dirVec.x * angle);
    //    Vector3 finalDir = dirVec - normalDir;
    //    finalDir = finalDir.normalized;

    //    projectiles[order].gameObject.SetActive(true);
    //    projectiles[order].Init(damage, force, finalDir);
    //}

    //투사체 생성
    void InitProjectile(int count)
    {
        for (int i = 0; i < count; i++)
        {
            FairyProjectile pro = Instantiate(projectile, projectileObj.transform);
            projectiles.Add(pro);
        }
    }

    public override void AddFirstUpgrade()
    {
        //투사체 수 증가
        proCount += (int)skillData.firstUpgradeValue[0];
        InitProjectile((int)skillData.firstUpgradeValue[0]);
    }

    public override void AddSecondUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        //투사체 수 증가
        proCount += (int)skillData.thirdUpgradeValue[0];
        InitProjectile((int)skillData.thirdUpgradeValue[0]);
    }
}
