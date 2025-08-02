using System;

namespace MantenseiLib
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentAttribute : Attribute
    {
        public HierarchyRelation relation { get; set; } = HierarchyRelation.Self;
        public virtual QuantityType quantity { get; } = QuantityType.Single;
        public virtual GetComponentType GetComponentType { get; } = GetComponentType.GetComponent;
        public bool HideErrorHandling { get; set; } = false;

        public GetComponentAttribute() { }
        public GetComponentAttribute(HierarchyRelation relation) { this.relation = relation; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentsAttribute : GetComponentAttribute
    {
        public GetComponentsAttribute() : base() { }
        public GetComponentsAttribute(HierarchyRelation relation) : base(relation){ }

        public override QuantityType quantity => QuantityType.Multiple;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AddComponentAttribute : GetComponentAttribute
    {
        public override GetComponentType GetComponentType { get => GetComponentType.AddComponent; }
    }

    [Flags]
    public enum HierarchyRelation
    {
        Self = 1,    // é©ï™é©êg
        Parent = 2,  // êe
        Children = 4, // éq

        None = 0,
        All = Self | Parent | Children,
    }

    public enum GetComponentType
    {
        GetComponent,
        AddComponent,
    }

    public enum QuantityType
    {
        Single,
        Multiple
    }
}
