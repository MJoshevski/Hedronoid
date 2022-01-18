using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public interface IAIPlanner
    {
        /*** Basic method to check rules **/
        void CheckRules();

        /*** Navigation can be used for all kinds of movements
             that are unrelated to characters/players.
             
             Ex: Patrol through spline, return to area, wander
             randomly in area, etc. ***/

        void SetNavigationTarget(Transform target = null); // Target can be area, spline, object in scene, etc.

        void OnNavigationUpdate(); 
        void PauseNavigation();
        void ResumeNavigation();
        void CancelNavigation();

        /*** Attracion is related to movement towards a  
             character or object of interest.

             Can be used for following characters or 
             aggressively chase.  ***/

        void OnAttractionUpdate(); 
        void PauseAttraction();
        void ResumeAttraction();
        void CancelAttraction();

        /*** Repel is related to movement away from a 
             character or object of interest
             .
             Can be used for fleeing from situations or 
             simulate fright.  ***/

        void OnRepelUpdate();
        void PauseRepel();
        void ResumeRepel();
        void CancelRepel();

        /*** Interact is related to interaction with 
             world objects, such as levers, buttons and 
             others.

             Can be used for building things, etc.***/

        void OnInteractUpdate(); 
        void PauseInteract();
        void ResumeInteract();
        void CancelInteract();

        /*** Attack is related to agressive actions 
             towards a character or object

             Can be used for bashing, dashing into
             objects, throwing things, etc.***/

        void OnAttackUpdate();
        void PauseAttack();
        void ResumeAttack();
        void CancelAttack();
    }
}