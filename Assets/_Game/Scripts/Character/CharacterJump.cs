using UnityEngine;

public class CharacterJump : CharacterDash
{
    [Header("Refs")]
    [SerializeField] CharacterWallRun CharacterWallRun;

    protected override void FixedUpdate()
    {
        if (CharacterWallRun.WallRunning)
        {
            return;
        }

        base.FixedUpdate();
    }
}