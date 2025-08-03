using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib
{
    public partial class DamageObject
    {
        partial void PartialDamageAction(DamageResult result)
        {
            VisualizeDamage(result);
        }

        void VisualizeDamage(DamageResult result)
        {
            //if ((result.HitObj?.CompareTag("Character") == true
            //    || result.HitObj?.CompareTag("Item") == true)
            //    && !(result?.Contains(DamageResultType.miss) == true)
            //    && !(result?.Contains(DamageResultType.noDamage) == true))
            //{
            //    var pos = (result.HitObj.transform.position + transform.position) / 2;

            //    if (result.color == null)
            //        EffectTextController.InstantiateSelf(pos, DamageInfo.Damage);
            //    else
            //        EffectTextController.InstantiateSelf(pos, DamageInfo.Damage, result.color.GetValueOrDefault());

            //    if (result.hitHelper is Enemy enemy)
            //    {
            //        var hp = enemy.HP;
            //        var maxHP = enemy.MaxHP;
            //        HPBar.InstantiateSelf(enemy.transform, hp, maxHP);
            //    }
            //}
        }
    }
}
