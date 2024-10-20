using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class CONSTANTS
{
    public const int MaxSkillLevel = 30; // 스킬 최대레벨

    public static readonly int[] SkillUpgradeLevel = new int[] { 10, 20, 30 }; // 스킬 업그레이드 기준 레벨

}

// 액티브 스킬데이터
[Serializable]
public class ActiveSkillData
{

    public int id; //ID 일반스킬(1~) 변신스킬(101~)
    public string name; //이름

    public int characType; // 스킬 사용 캐릭터 타입 - 0: 일반 1: 뱀파이어 2: 요정

    public int openLevel; // 스킬 사용 레벨

    public int grade;

    // 스킬 생성 타입 -> 0: 외부 / 1: 플레이어 중심 ->타입에 따라 초기생성 장소다르게
    public int insType;

    public float coolTime; // 스킬 쿨타임
    public float skillDamage; // 스킬 데미지 계수 공격력 N%

    public float atkRange; // 스킬 공격범위 -> 원:반경 / 직선: 거리

    public float skillScale; // 스킬 충돌체 크기 -> 조정이 필요한 경우 사용

    public int atkCnt; // 공격 횟수

    public float duration; // 스킬 지속시간 -> 지속스킬 일 경우
    public float intervalTime; // 지속스킬일 경우 간격

    public int statusType;
    public float[] statusValue; //상태효과 수치

    public float levelUpValue; // 레벨업시 증가하는 능력치 값
    public float[] firstUpgradeValue; // 첫번째 스킬 업글 변수
    public float[] secondUpgradeValue; // 두번째 스킬 업글 변수
    public float[] thirdUpgradeValue; // 세번째 스킬 업글 변수

    public string animName; // 사용 애니메이션 이름
    public float animTimeScale; // 사용 애니메이션 재생속도

    // --- 스킬 타겟 관련 ---
    public int tDectectionType; // 스킬 타겟탐지 타입 - 0: 논타겟 1: 가장 가까운적
    public float tDetectionRange; // 스킬 타겟 탐지 거리 값 -> EX) 타입이 1번일 경우 사거리값
    public int targetCount; // 타게팅 스킬일 경우 찾아야하는 타겟 수

    public float useTime; //스킬 발동 시간

    public string explain;  // 스킬 설명

    // 스킬 강화 설명 1~3번
    public string firstUpgradeExplain;
    public string secondUpgradeExplain;
    public string thirdUpgradeExplain;
}

public interface IDamageable
{
    public void OnDamaged(float damage, bool isCri);
}


/// <summary>
/// 스킬 관련 클래스
/// </summary>

[Serializable]
public abstract class Skill : MonoBehaviour
{
    public int level; // 스킬 레벨

    public SkillHandler skillUser; // 스킬 사용자

    public abstract void UseSkill(); // 스킬 사용 효과                                  
    public abstract void CheckSkillUpgrade();  // 스킬 레벨업 시 업그레이드 체크

    public virtual void InitSkill(SkillHandler _skillUser, int skilLevel)
    {
        skillUser = _skillUser;
        level = skilLevel;
    }

    // 레벨 업 시
    public virtual void LevelUp()
    {
        //스킬 레벨 증가
        level++;
        CheckSkillUpgrade();
    }

    public virtual void StopSkill()  // 스킬 종료시
    {
        gameObject.SetActive(false);
    }

}

public class SkillHandler: MonoBehaviour { }

/// <summary>
/// 액티브 스킬
/// </summary>
[Serializable]
public abstract class ActiveSkill : Skill
{
    // 스킬 사운드
    public AudioClip skillSound; 

    // 액티브 스킬 데이터 -> 시트에서 받아오는 데이터
    public ActiveSkillData skillData; 

    // 스킬 콜라이더 리스트 -> 레이어변경이 필요한 콜라이더들
    public List<Collider> skillCollList = new List<Collider>(); 

    public bool isActive = false; // 스킬 활성 여부 

    public float damage; // 데미지

    // 액티브 스킬 사용 기능 -> 실제 스킬 사용효과
    public abstract void UseActiveSkill();

    //스킬 레벨에 따른 강화효과 추가
    public abstract void AddFirstUpgrade(); // 10레벨 강화효과
    public abstract void AddSecondUpgrade(); // 20레벨 강화효과
    public abstract void AddThirdUpgrade(); // 30레벨 강화효과

    // 스킬 초기 설정
    public override void InitSkill(SkillHandler _skillUser, int skilLevel)
    {
        base.InitSkill(_skillUser, skilLevel);
        // 스킬 레벨에 따른 업그레이드 여부 체크
        if (level >= CONSTANTS.SkillUpgradeLevel[0]) AddFirstUpgrade();
        if (level >= CONSTANTS.SkillUpgradeLevel[1]) AddSecondUpgrade();
        if (level >= CONSTANTS.SkillUpgradeLevel[2]) AddThirdUpgrade();
    }

    // 스킬 레벨업 시 업그레이드 체크
    public override void CheckSkillUpgrade()
    {
        if (level == CONSTANTS.SkillUpgradeLevel[0]) AddFirstUpgrade();
        if (level == CONSTANTS.SkillUpgradeLevel[1]) AddSecondUpgrade();
        if (level == CONSTANTS.SkillUpgradeLevel[2]) AddThirdUpgrade();
    }

    // 액티브 스킬 생성
    public virtual void InitActiveSkill(SkillHandler _skillUser, ActiveSkillData _skillData, int skillLevel)
    {
        skillData = _skillData;

        InitSkill(_skillUser ,skillLevel);

        SetSkillLayer(skillUser.gameObject.layer);

        skillData.skillDamage += (skillData.levelUpValue * (level - 1));
    }


    public virtual void OnTriggerEnter(Collider other)
    {  
        other.GetComponent<IDamageable>().OnDamaged(damage, false);

    }

    // 액티브 스킬은 사용 시 데미지 계산
    public override void UseSkill()
    {
        isActive = true;

        if(skillSound != null)
        {   
            // 스킬 효과음 재생
           // SoundManager.instance.PlaySfx(skillSound, 1.0f);
        }

        UseActiveSkill();
    }

    public override void LevelUp()
    {
        //스킬 데미지 증가
        skillData.skillDamage += skillData.levelUpValue;

        base.LevelUp();
    }

    // 스킬 종료시
    public override void StopSkill()
    {
        isActive = false;
        base.StopSkill();
    }

    //범위 공격 시 
    protected IEnumerator CastDoTSkillCoro(Collider collider, float delay)
    {
        //sDuration + delay = 파티클 재생 시간
        collider.enabled = false;
        collider.gameObject.SetActive(true);

        //콜라이더 켜질 때까지의 딜레이
        yield return  new WaitForSeconds(delay);

        StartCoroutine(Utils.AreaDoTAttack(collider, skillData.duration, skillData.intervalTime));

        yield return new WaitForSeconds(skillData.duration);

        collider.gameObject.SetActive(false);
        StopSkill();
    }

    
    protected IEnumerator CastSkillCoro(Collider collider, float delay, float duration)
    {
        collider.enabled = false;
        collider.gameObject.SetActive(true);

        yield return new WaitForSeconds(delay);

        collider.enabled = true;

        yield return   new WaitForFixedUpdate();

        collider.enabled = false;

        //오브젝트 꺼지는시간
        yield return new WaitForSeconds(duration);

        collider.gameObject.SetActive(false);
        StopSkill();
    }

    // 상대 타겟 레이어 가져오기
    protected LayerMask GetTargetLayer(int myLayer)
    {
        if (myLayer == 6) return LayerMask.GetMask("Enemy");
        if (myLayer == 7) return LayerMask.GetMask("Player");

        return 0;
    }

    // 스킬 콜라이더들 찾아서 레이어 변경 적군 레이어로변경
    protected void SetSkillLayer(int myLayer)
    {
        LayerMask skillLayerMask = 0;

        if (myLayer == 6) skillLayerMask = LayerMask.NameToLayer("PlayerAttack");
        if (myLayer == 7) skillLayerMask = LayerMask.NameToLayer("EnemyAttack");

        gameObject.layer = skillLayerMask;

        if (skillCollList.Count == 0) return;

        foreach (Collider col in skillCollList)
        {
            col.gameObject.layer = skillLayerMask;
        }

    
    }

}

// 타겟팅 스킬에는 해당 인터페이스 추가 -> 타겟들의 트랜스폼 리스트
public interface ITargetingSkill
{
    public List<Transform> targetList { get; set; }

}




