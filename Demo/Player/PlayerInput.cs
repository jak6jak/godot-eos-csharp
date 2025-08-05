using Godot;
using Riptide; // For RiptideExtensions

namespace Riptide.Demos.Steam.PlayerHosted
{
    public partial class PlayerInput : Node
    {
        [Export] private Camera3D cameraTransform;
        private bool[] inputs;

        public override void _Ready()
        {
            inputs = new bool[5];
            
            // Get camera reference if not assigned
            if (cameraTransform == null)
                cameraTransform = GetViewport().GetCamera3D();
        }

        public override void _Process(double delta)
        {
            // Sample inputs every frame and store them until they're sent
            // This ensures no inputs are missed between physics frames
            if (Input.IsActionPressed("move_forward"))
                inputs[0] = true;

            if (Input.IsActionPressed("move_backward"))
                inputs[1] = true;

            if (Input.IsActionPressed("move_left"))
                inputs[2] = true;

            if (Input.IsActionPressed("move_right"))
                inputs[3] = true;

            if (Input.IsActionPressed("jump"))
                inputs[4] = true;
        }

        public override void _PhysicsProcess(double delta)
        {
            SendInput();

            // Reset input booleans after sending
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = false;
        }

        #region Messages
        private void SendInput()
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.PlayerInput);
            message.AddBools(inputs, false);
            
            // Get camera forward direction
            Vector3 forward = -cameraTransform.GlobalTransform.Basis.Z;
            message.AddVector3(forward);
            
            NetworkManager.Singleton.Client.Send(message);
        }
        #endregion
    }
}