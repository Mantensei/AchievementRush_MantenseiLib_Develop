using System.Collections;
using System.Collections.Generic;
using MantenseiLib.UnityAction.Action2D;
using UnityEngine;

namespace MantenseiLib
{
    public sealed class GeneralPlayerHub : BaseMonoBehaviour, IPlayerHub
    {
        //[GetComponent(HierarchyRelation.Children)]
        //public PlayerStateManager PlayerStateManager { get; private set; }

        [GetComponent(HierarchyRelation.Children)]
        public MoverBase Mover {get; private set;}

        [GetComponent(HierarchyRelation.Children)]
        public Jumper Jumper {get; private set;}

        [field:SerializeField]
        public GroundChecker GroundChecker {get; private set;}

        [field:SerializeField]
        public GroundChecker CeillingChecker {get; private set;}

        [GetComponents(HierarchyRelation.Self | HierarchyRelation.Children)]
        public Collider2D[] Colliders {get; private set;}

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Children)]
        public Rigidbody2D rb2d {get; private set;}
    }

    public abstract class ProjectSpecificPlayerHub : BaseMonoBehaviour, IPlayerHub
    {
        [GetComponent]
        public GeneralPlayerHub PlayerHub { get; protected set; }

        //public PlayerStateManager PlayerStateManager => PlayerHub.PlayerStateManager;

        public MoverBase Mover => PlayerHub.Mover;

        public Jumper Jumper => PlayerHub.Jumper;

        public GroundChecker GroundChecker => PlayerHub.GroundChecker;

        public GroundChecker CeillingChecker => PlayerHub.CeillingChecker;

        public Collider2D[] Colliders => PlayerHub.Colliders;

        public Rigidbody2D rb2d => PlayerHub.rb2d;
    }
}
