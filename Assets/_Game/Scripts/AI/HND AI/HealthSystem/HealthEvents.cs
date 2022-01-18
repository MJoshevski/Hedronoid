using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Health
{
    public class RecieveHealthEvent : HNDBaseEvent { public float amount; public bool canGoAboveMax; public int GOID; }
    public class KillEvent : HNDBaseEvent { public int GOID; }
    public class OnKillEvent : HNDBaseEvent { public string ID; }
    public class ReviveEvent : HNDBaseEvent { public int GOID; }

    public class RespawnEvent : HNDBaseEvent { public int GOID; }
}