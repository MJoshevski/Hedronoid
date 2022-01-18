using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Objects;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class FlockSubscribeEvent : HNDBaseEvent { public string FlockName; public GameObject FlockMember; }
    public class FlockAttractSubscribeEvent : HNDBaseEvent { public List<NPC.NPCType>  FlockType; public FlockAttract Attract; }
    public class FlockRepeltSubscribeEvent : HNDBaseEvent { public List<NPC.NPCType> FlockType; public FlockRepel Repel; }
    public class FlockAttractUnsubscribeEvent : HNDBaseEvent { public List<NPC.NPCType> FlockType; public FlockAttract Attract; }
    public class FlockRepeltUnsubscribeEvent : HNDBaseEvent { public List<NPC.NPCType> FlockType; public FlockRepel Repel; }
    public class FlockAttractUnsubscribeTypeEvent : HNDBaseEvent { public NPC.NPCType FlockType; public FlockAttract Attract; }
    public class FlockRepeltUnsubscribeTypeEvent : HNDBaseEvent { public NPC.NPCType FlockType; public FlockRepel Repel; }
    public class FlockInteractSubscribeEvent : HNDBaseEvent { public List<NPC.NPCType> FlockType; public BaseInteractable Interact; }
    public class FlockInteractUnsubscribeEvent : HNDBaseEvent { public List<NPC.NPCType> FlockType; public BaseInteractable Interact; }
    public class FlockDisperseEvent : HNDBaseEvent { public NPC.NPCType FlockType; public Transform Target;
                                                    public float Range; public float Panic; public float Time; }
    public class FlockAttractRequest : HNDBaseEvent { };
    public class FlockRepelRequest : HNDBaseEvent { };
}