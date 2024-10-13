using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGround : MonoBehaviour
{
    [SerializeField] FireExplosionSkill fireExplosionSkill;

    public virtual void OnTriggerEnter(Collider other)
    {
        float damage = fireExplosionSkill.damage * fireExplosionSkill.skillData.secondUpgradeValue[2];
        other.GetComponent<IDamageable>().OnDamaged(damage, false);
    }
}
