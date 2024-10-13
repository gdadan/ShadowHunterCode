using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderSlashSkill : TargetingSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider slashCol;
    [SerializeField] Collider areaCol;

    private void Start()
    {
        //크기 설정
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        StartCoroutine(StartRangeSkill());
    }

    //스킬 실행 - 1.슬래쉬 후 2.부채꼴 장판 스킬
    IEnumerator StartRangeSkill()
    {
        slashCol.transform.LookAt(targetList[0].position);
        slashCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime / 2);

        slashCol.gameObject.SetActive(false);

        //2.부채꼴장판 애니메이션 실행
        skillUser.ControlAnimTimeScale(1.6f);
        skillUser.PlayAnimation("attack2-1");

        areaCol.transform.LookAt(targetList[0].position);
        areaCol.gameObject.SetActive(true);

        yield return YieldCache.WaitForSeconds(skillData.useTime / 2);

        areaCol.gameObject.SetActive(false);

        StopSkill();
    }

    public override void AddFirstUpgrade()
    {
        base.AddFirstUpgrade();

        //재사용 대기시간 감소
        skillData.coolTime *= 1 - skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        base.AddSecondUpgrade();

        //범위 증가
        skillData.atkRange *= 1 + skillData.secondUpgradeValue[0];
        Utils.SetSkillRange(allObj, skillData.atkRange);
    }

    public override void AddThirdUpgrade()
    {
        base.AddThirdUpgrade();

        //데미지 증가
        skillData.skillDamage += skillData.thirdUpgradeValue[0];
    }
}
