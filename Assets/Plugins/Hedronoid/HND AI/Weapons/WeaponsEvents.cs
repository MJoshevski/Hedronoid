using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Weapons
{
    public enum EDamageType
    {
        Sword = 0,
        Fire,
        Frost,
        Frost_Side,
        Impact_Above,
        Impact_Side,
        Any,
        Push
    }

    public enum EAttackType
    {
        Direct = 0,
        AoE,
        PushAoE,
        PushDirect,
        PullAoE,
        FreezeAoE,
        FreezeDash
    }

    public class DoDamageByJumpFromAboveEvent : HNDBaseEvent 
    {
        public int GOID;
        public float Damage;
        public int RecieverGOID;
        public int SenderGOID;
        public Collision Collision; }
    
    public class DoDamageByImpactEvent : HNDBaseEvent 
    {
        public int GOID;
        public float Damage;
        public int RecieverGOID;
        public int SenderGOID;
        public Collision Collision; }
    
    public class DoDamagePlayer : HNDBaseEvent 
    {
        public int GOID;
        public float Damage;
        public int RecieverGOID;
        public int SenderGOID;
        public Collision Collision; }

    public class DoDamage : HNDBaseEvent 
    {
        public int GOID;
        public float Damage;
        public string Skill;
        public GameObject Particle;
        public Vector3 DamageDirection = Vector3.zero;
        public EDamageType Type = EDamageType.Impact_Side;
        public Collision Col = null;
    }

    public class CallAction : HNDBaseEvent 
    {
        public int GOID;
        public int ActionID;
        public bool State;
    }

    public class ClearActions : HNDBaseEvent
    {
    }
}