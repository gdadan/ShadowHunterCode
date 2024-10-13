using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FairyProjectile : Projectile
{
   
    public ParticleSystem hitEffect; //맞을 때 이펙트

  //  [SerializeField] Collider explodeCol; // 폭발콜라이더

    [SerializeField] ExplosionAttack explodeAttack; // 폭발 공격
   // Vector3 oriPos; //처음 위치

    bool isExplode = false;

    private void Start()
    {
        explodeAttack.InitExplosionAttack(1f, Attacker);
    }

    public override void Init(float speed, Vector3 dir, AttackHandler attacker)
    {
        base.Init(speed, dir, attacker);
        projectileEffect.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            if (isExplode == false)
            {
                StartCoroutine(HitProjectile());
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;

                isExplode = true;

                Debug.Log("멈춤");
            }
          //  if(CompareTag("Skill"))
           // other.GetComponent<AttackHandler>().OnDamaged(damage, false);
        }

       
    }

    //투사체 맞을 때 이펙트
    IEnumerator HitProjectile()
    {
        col.enabled = false;
        projectileEffect.Stop();
        projectileEffect.gameObject.SetActive(false);
        hitEffect.Play();

        explodeAttack.gameObject.SetActive(true);
        explodeAttack.StartExplodeAttack();
        Debug.Log("폭발!");
        yield return new WaitForSeconds(0.5f);
     //   explodeCol.enabled = false;
        hitEffect.Stop();

        explodeAttack.StopExplodeAttack();

        DeactiveProjectile();
    }

    public override void DeactiveProjectile()
    {
        isExplode = false;
        base.DeactiveProjectile();

    }
}
