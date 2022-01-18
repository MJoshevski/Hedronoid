using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public interface IAINavigation 
    {
        /*** Target for movement **/ 

        void SetTarget(Transform target);

        /*** Set default place to return to if
             there is no target. If set for self
             it will remain static. ***/

        void SetDefaultTarget(Transform target);

        /*** Get direction based on navigation
             system and state ***/

        Vector3 GetDirection();

        /*** Go to target that is currently set ***/

        void OnGoToTargetUpdate();

        /*** Flee to target that is currently set ***/

        void OnFleeFromTargetUpdate();

        /*** Return to default area/target/object ***/

        void OnReturnToDefaultUpdate();

        /*** Execute default movement behaviour ***/

        void OnDefaultMovementUpdate();
    }
}
