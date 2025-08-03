using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib.Obsolete
{
    public interface IDamagable
    {
        DamageResult ApplyDamage(DamageInfo damageInfo);
    }

    public interface IGetCharacterProperty
    {
        float HP { get; }
    }

    public enum DamageResultType
    {
        alive,
        death,
        hit,
        noDamage,
        miss,
        guard,
        counter,
    }

    public class DamageResult
    {
        public DamageResult() { }
        public DamageResult(GameObject gameObject)
        {
            hitObj = gameObject;
        }
        public DamageResult(Helper helper)
        {
            hitHelper = helper;
        }
        public DamageResult(IGetCharacterProperty characterProperty)
        {
            hp = characterProperty.HP;
        }
        public DamageResult(Helper helper, params DamageResultType[] resultTypes)
        {
            hitHelper = helper;
            ResultTypes = resultTypes.ToList();
        }

        public DamageResult(params DamageResultType[] resultTypes)
        {
            ResultTypes = resultTypes.ToList();
        }

        public static DamageResult miss
        {
            get
            {
                var result = new DamageResult();
                result.AddResult(DamageResultType.miss, DamageResultType.alive);
                return result;
            }
        }

        public static DamageResult Miss(Helper helper)
        {
            var result = miss;
            result.hitHelper = helper;
            return result;
        }

        public static DamageResult noDamage
        {
            get
            {
                var result = new DamageResult();
                result.AddResult(DamageResultType.noDamage, DamageResultType.alive);
                return result;
            }
        }
        
        public static DamageResult NoDamage(Helper helper)
        {
            var result = noDamage;
            result.hitHelper = helper;
            return result;
        }

        public static DamageResult Dead(Helper helper)
        {
            var result = new DamageResult();
            result.AddResult(DamageResultType.death);
            result.hitHelper = helper;
            return result;
        }

        public bool Contains(DamageResultType resultType) => ResultTypes.Contains(resultType);
        public void AddResult(params DamageResultType[] result) { ResultTypes.AddRange(result); }
        public static DamageResult GetResult(IGetCharacterProperty chara) => new DamageResult() { hp = chara.HP, };
        public List<DamageResultType> ResultTypes { get; private set; } = new List<DamageResultType>();
        public float? hp;
        public float? damage;
        public GameObject hitObj;
        public GameObject HitObj => hitObj ?? hitHelper?.gameObject;
        public Helper hitHelper;
        public Color? color;
    }

    public enum DamageAttributeType
    {
        Normal,
        InvinciblePierce,
        ArmorBypass,
        InstantKill,
        Lethal,
        Healing,
        StaticDamage,
        Flame,

        EventAttack,
        System,
    }

    public partial class DamageInfo
    {
        [SerializeField] public SerializableDamageInfo serializableInfo = new SerializableDamageInfo();
        public float Damage { get => serializableInfo.damage; set => serializableInfo.damage = value; }
        public string HitParticle { get => serializableInfo.HitParticle; set => serializableInfo.HitParticle = value; }
        public string DestroyParticle { get => serializableInfo.DestroyParticle; set => serializableInfo.DestroyParticle = value; }
        public List<string> Options { get; private set; } = new();
        public bool OptionContains(string option) => Options.Contains(option);
        public bool TypeContains(DamageAttributeType damageType) => damageAttributes.Contains(damageType);

        public List<DamageAttributeType> damageAttributes { get; private set; } = new List<DamageAttributeType>();
        public DamageInfo AddDamageProp(params DamageAttributeType[] damageAttributes)
        {
            this.damageAttributes.AddRange(damageAttributes);
            return this;
        }

        public void RemoveDamageProp(DamageAttributeType damageAttribute) => this.damageAttributes.Remove(damageAttribute);
        [NonSerialized] public Helper executor;
        [NonSerialized] public Helper director;
        [NonSerialized] public Rigidbody2D rb2d;
        public float? speed;
        public float GetSpeed() => speed ?? Damage;

        public Vector3? pos;
        public Vector3 GetPos() => pos ?? executor.transform.position;


        public Vector2? velocity;
        public Quaternion? rotation;

        [NonSerialized] public Collider2D ownerCol;
        //[NonSerialized] public HelperObserver observer;

        public DamageInfo SetDamage(float damage)
        {
            Damage = damage;
            return this;
        }

        public DamageInfo() : this(1) { }
        public DamageInfo(params DamageAttributeType[] damageTypes) : this(1) { AddDamageProp(damageTypes); }
        public DamageInfo(float damage) { this.Damage = damage; }
        public DamageInfo(Helper helper, float damage = 1)
        {
            this.Damage = damage;
            SetExecuter(helper);
        }

        public void SetExecuter(Helper executor)
        {
            if (executor == null) return;

            this.executor = executor;
            director = executor.GetDirector() ?? executor;
            pos = executor.transform.position;
            rotation = executor.transform.rotation;
            ownerCol = executor.GetComponent<Collider2D>();
            rb2d = executor.GetComponent<Rigidbody2D>();
            //observer = executor.GetComponent<HelperObserver>();
        }

        public static DamageInfo zero => new DamageInfo(0f);
        public static DamageInfo one => new DamageInfo(1);

        public static DamageInfo operator *(DamageInfo damage, float multiplier)
        {
            damage.Damage *= multiplier;
            return damage;
        }

        public static DamageInfo operator /(DamageInfo damage, float divider)
        {
            damage.Damage /= divider;
            return damage;
        }

        public static DamageInfo operator +(DamageInfo damage1, DamageInfo damage2)
        {
            return new DamageInfo { Damage = damage1.Damage + damage2.Damage };
        }

        public static DamageInfo operator -(DamageInfo damage1, DamageInfo damage2)
        {
            return new DamageInfo { Damage = damage1.Damage - damage2.Damage };
        }

        public override string ToString()
        {
            return Damage.ToString();
        }
    }

    [Serializable]
    public partial class SerializableDamageInfo
    {
        public float damage;
        public DamageAttributeType[] damageTypes = new DamageAttributeType[0];
    }
}