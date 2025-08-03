using MantenseiLib.Develop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dmg_Test_02 : MonoBehaviour, IDamageable
{
    public void TakeDamage(DamageInfo damageInfo)
    {
        Debug.Log($"Damage taken: {damageInfo?.Damage} from {damageInfo?.Attacker?.name}");
    }
}
