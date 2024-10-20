using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostlyFireSkill : ActiveSkill
{
    [SerializeField] GameObject allObj; //전체 오브젝트
    [SerializeField] Collider originalCol; //생성할 원본 콜라이더
    List<Collider> fireCols = new List<Collider>(); //도깨비불 콜라이더

    int ghostlyFireCount = 3; //도깨비불 개수
    float rotateSpeed = 200f; //회전 속도
    float duration = 5f; //도깨비불 시간

    bool isRotated = false; //도깨비불 돌고있는 지 여부
   
    private void Start()
    {
        //도깨비불 생성, 크기 설정, 위치 설정
        InitGhostlyFire(ghostlyFireCount);
        Utils.SetSkillRange(allObj.gameObject, skillData.skillScale);
        Batch();
    }

    private void Update()
    {
        if (!isRotated)
            return;

        transform.Rotate(Vector3.down * rotateSpeed * Time.deltaTime);
    }

    //ghostlyFire 생성
    void InitGhostlyFire(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider ghostlyFire = Instantiate(originalCol, allObj.transform);
            fireCols.Add(ghostlyFire);
        }
    }

    //도깨비불 배치
    void Batch()
    {
        for (int i = 0; i < fireCols.Count; i++)
        {
            Vector3 rotVec = Vector3.up * 360 * i / ghostlyFireCount;
            fireCols[i].transform.Rotate(rotVec);
            fireCols[i].transform.Translate(fireCols[i].transform.forward * 2f, Space.World);
        }
    }

    public override void UseActiveSkill()
    {
        StartCoroutine(RotateGhostlyFire());
    }

    //도깨비불 스킬 실행
    IEnumerator RotateGhostlyFire()
    {
        isRotated = true;

        for (int i = 0; i < fireCols.Count; i++)
        {
            fireCols[i].gameObject.SetActive(true);
        }

        yield return YieldCache.WaitForSeconds(duration);

        isRotated = false;

        for (int i = 0; i < fireCols.Count; i++)
        {
            fireCols[i].gameObject.SetActive(false);
        }
    }

    public override void AddFirstUpgrade()
    {
        //지속시간 증가
        duration += skillData.firstUpgradeValue[0];
    }

    public override void AddSecondUpgrade()
    {
        //데미지 증가
        skillData.skillDamage += skillData.secondUpgradeValue[0];
    }

    public override void AddThirdUpgrade()
    {
        //개수 증가
        ghostlyFireCount += (int)skillData.thirdUpgradeValue[0];
        InitGhostlyFire((int)skillData.thirdUpgradeValue[0]);
        Batch();
    }
}
