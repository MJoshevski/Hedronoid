using System;
using InControl;

namespace Hedronoid
{
    public class PlayerActionSet : InControl.PlayerActionSet
    {
        public PlayerAction Weapon1;
        public PlayerAction Weapon2;
        public PlayerAction MoveLeft;
        public PlayerAction MoveRight;

        public PlayerAction MoveForward;
        public PlayerAction MoveBackward;
        public PlayerTwoAxisAction Move;
        public PlayerAction LookLeft;
        public PlayerAction LookRight;
        public PlayerAction LookUp;
        public PlayerAction LookDown;
        public PlayerTwoAxisAction Look;
        public PlayerAction Jump;
        public PlayerAction Dash;

        public PlayerActionSet()
        {
            Weapon1 = CreatePlayerAction("Weapon1");
            Weapon2 = CreatePlayerAction("Weapon2");
            MoveLeft = CreatePlayerAction("Move Left");
            MoveRight = CreatePlayerAction("Move Right");
            MoveForward = CreatePlayerAction("Move Forward");
            MoveBackward = CreatePlayerAction("Move Backward");
            Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveBackward, MoveForward);
            LookLeft = CreatePlayerAction("Look Left");
            LookRight = CreatePlayerAction("Look Right");
            LookUp = CreatePlayerAction("Look Up");
            LookDown = CreatePlayerAction("Look Down");
            Look = CreateTwoAxisPlayerAction(LookLeft, LookRight, LookDown, LookUp);
            Jump = CreatePlayerAction("Jump");
            Dash = CreatePlayerAction("Dash");
        }

        public static PlayerActionSet CreateWithDefaultBindings()
        {
            var playerActions = new PlayerActionSet();

            playerActions.Weapon1.AddDefaultBinding(InputControlType.RightBumper);
            playerActions.Weapon1.AddDefaultBinding(Mouse.LeftButton);

            playerActions.Weapon2.AddDefaultBinding(InputControlType.RightTrigger);
            playerActions.Weapon2.AddDefaultBinding(Mouse.RightButton);

            playerActions.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
            playerActions.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
            playerActions.MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
            playerActions.MoveBackward.AddDefaultBinding(InputControlType.LeftStickDown);
            playerActions.MoveLeft.AddDefaultBinding(Key.A);
            playerActions.MoveRight.AddDefaultBinding(Key.D);
            playerActions.MoveForward.AddDefaultBinding(Key.W);
            playerActions.MoveBackward.AddDefaultBinding(Key.S);

            playerActions.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
            playerActions.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
            playerActions.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
            playerActions.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
            playerActions.LookLeft.AddDefaultBinding(Mouse.NegativeX);
            playerActions.LookRight.AddDefaultBinding(Mouse.PositiveX);
            playerActions.LookUp.AddDefaultBinding(Mouse.PositiveY);
            playerActions.LookDown.AddDefaultBinding(Mouse.NegativeY);

            playerActions.Jump.AddDefaultBinding(InputControlType.Action1);
            playerActions.Jump.AddDefaultBinding(Key.Space);

            playerActions.Dash.AddDefaultBinding(InputControlType.Action2);
            playerActions.Dash.AddDefaultBinding(Key.Shift);

            playerActions.ListenOptions.MaxAllowedBindings = 2;
            playerActions.ListenOptions.UnsetDuplicateBindingsOnSet = true;
            playerActions.ListenOptions.IncludeMouseButtons = true;
            playerActions.ListenOptions.IncludeModifiersAsFirstClassKeys = true;
            // playerActions.ListenOptions.MaxAllowedBindingsPerType = 1;

            playerActions.ListenOptions.OnBindingFound = (action, binding) =>
            {
                if (binding == new KeyBindingSource(Key.Escape))
                {
                    action.StopListeningForBinding();
                    return false;
                }
                return true;
            };

            playerActions.ListenOptions.OnBindingAdded += (action, binding) =>
            {
                D.CoreLog("Binding added... " + binding.DeviceName + ": " + binding.Name);
            };

            playerActions.ListenOptions.OnBindingRejected += (action, binding, reason) =>
            {
                D.CoreLog("Binding rejected... " + reason);
            };

            return playerActions;
        }
    }
}