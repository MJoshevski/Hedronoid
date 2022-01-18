using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public interface IAISensor
    {
        Transform ClosestPlayer();
        float DistanceToClosestPlayer();
        bool IsAnyPlayerInReach(float distance);
        bool IsPlayerInReach(int player, float distance);
        Transform GetRandomPlayerInReach(float distance);

        float DistanceFrontalCollision();
        float DistanceRightCollision();
        float DistanceLeftCollision();

        GameObject GetObjectInFront();
    }
}
