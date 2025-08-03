using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MantenseiLib.Develop
{
    /// <summary>
    /// �_���[�W���󂯂邱�Ƃ��ł���I�u�W�F�N�g�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
    }

    /// <summary>
    /// �_���[�W�����̌��ʏ��
    /// </summary>
    public class DamageResult
    {
        public float HP { get; set; }
        public float ActualDamage { get; set; }

        public bool Hit { get; set; }
        public bool Miss => !Hit;
        public bool Dead { get; private set; }
        public MonoBehaviour Target { get; set; }

        public bool IsHealing => ActualDamage < 0;
        public bool IsDamage => ActualDamage > 0;

        public static DamageResult Missed => new DamageResult(0, 0, false, false);

        /// <summary> hit��dead�͎����Őݒ� </summary>
        public DamageResult(float hp, float actualDamage, MonoBehaviour target = null) :this(hp, actualDamage, actualDamage != 0, hp <= 0, target) { }

        /// <summary> hit��dead���蓮�Őݒ� </summary>
        public DamageResult(float hp, float actualDamage, bool hit, bool dead, MonoBehaviour target = null)
        {
            HP = hp;
            ActualDamage = actualDamage;

            Target = target;
        }
    }

    /// <summary>
    /// �_���[�W���N���X�i�g���̂Đ݌v�j
    /// </summary>
    public partial class DamageInfo
    {
        // ===== ���͏�� =====
        public float Damage { get; set; }
        public MonoBehaviour Attacker { get; set; }
        public HashSet<string> Tags { get; private set; } = new HashSet<string>();

        // ===== ���ʏ�� =====
        public DamageResult Result { get; set; } = null;

        // ===== �R���X�g���N�^ =====
        public DamageInfo(float damage, MonoBehaviour attacker = null)
        {
            Damage = damage;
            Attacker = attacker;
        }

        public DamageInfo AddTag(params string[] tags)
        {
            foreach (var tag in tags)
                Tags.Add(tag);

            return this;
        }

        public bool HasTag(string tag) => Tags.Contains(tag);
    }

    public partial class DamageInfo
    {
        // ===== �֗����\�b�h =====
        public bool IsHealing => Damage < 0;
        public bool IsDamage => Damage > 0;

        // ===== �ÓI�t�@�N�g�� =====
        public static DamageInfo zero => new DamageInfo(0);
        public static DamageInfo one => new DamageInfo(1);

        // ===== ���Z�q�I�[�o�[���[�h =====
        public static DamageInfo operator *(DamageInfo info, float multiplier)
        {
            return new DamageInfo(info.Damage * multiplier, info.Attacker)
                .AddTag(info.Tags.ToArray());
        }

        public static DamageInfo operator +(DamageInfo a, DamageInfo b)
        {
            return new DamageInfo(a.Damage + b.Damage, a.Attacker ?? b.Attacker)
                .AddTag(a.Tags.ToArray())
                .AddTag(b.Tags.ToArray());
        }

        public override string ToString()
        {
            return $"Damage: {Damage}, Tags: [{string.Join(", ", Tags)}]";
        }
    }
}