using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;

namespace MantenseiLib
{
    [CreateAssetMenu(menuName = "Mantensei/Create DamageInfo")]
    public class ScriptableDamageInfo : ScriptableObject
    {
        [SerializeField] public DamageObjectSettings damageObjectSettings;

        public DamageObjectSettings GetDamageObjectSettings(Helper owner)
        {
            var setting = GetDamageObjectSettings();
            var info = setting.props.damageInfo;
            info.SetExecuter(owner);
            return setting;
        }

        public DamageObjectSettings GetDamageObjectSettings()
        {
            return new DamageObjectSettings()
            {
                props = damageObjectSettings.props,
                hp = damageObjectSettings.hp,
                lifeTime = damageObjectSettings.lifeTime
            };
        }

        public DamageInfo GetDamageInfo(Helper helper)
            => damageObjectSettings.props.GetNewDamageInfo(helper);
        //public DamageInfo GetDamageInfo(Helper helper)
        //{
        //    var info = new DamageInfo();
        //    var executer = helper.GetOwner();
        //    var director = helper.GetDirector();

        //    info.SetExecuter(executer.gameObject);
        //    info.rb2d = executer.GetComponent<Rigidbody2D>();
        //    info.observer = helper.GetComponent<HelperObserver>();
        //    info.director = director.gameObject;

        //    info.serializableInfo = damageObjectSettings.props.damageInfo.serializableInfo;
        //    return info;
        //}
    }
    
}
