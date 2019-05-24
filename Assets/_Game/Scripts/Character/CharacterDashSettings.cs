using System.Linq;
using InControl;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDashSettings : ScriptableObject
{
    public string ActionName;
    public int ExecutionsBeforeReset = 0;
    public bool ContinuousInput = false;

    public PhysicalForceSettings PhysicalForce;

    public InControl.PlayerAction GetPlayerAction()
    {
        var playerAction = MDKShooter.InputManager.Instance
            .PlayerActions
            .Actions
            .FirstOrDefault(a => a.Name == ActionName.Trim());
        if (playerAction == null)
        {
            Debug.LogError("Could not find player action with name " + ActionName);
        }
        return playerAction;
    }
}