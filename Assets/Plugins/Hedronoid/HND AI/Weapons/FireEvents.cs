using UnityEngine;
using System.Collections.Generic;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Fire
{
    public class OnCatchFire : HNDBaseEvent { public int GOID; }
    public class OnPutOutFire : HNDBaseEvent { public int GOID; }
}
