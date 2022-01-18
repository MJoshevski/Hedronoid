using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public interface IAIMotor 
    {
        void Attack(Transform target);
        void Move(Vector3 Direction);
        void Interact(Transform target);
    }
}