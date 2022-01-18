using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Enemies
{    
    public class LocatedPlayerEvent : HNDBaseEvent { public int GOID; public Transform target; }
}