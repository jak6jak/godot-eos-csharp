using EOSPluign.Demo;
using Godot;
using Riptide; // For RiptideExtensions

namespace Riptide.Demos.Steam.PlayerHosted
{
    public partial class PlayerMovement : Node
    {
        [Export] private ServerPlayer player;
        [Export] private float gravity = -9.81f;
        [Export] private float moveSpeed = 5.0f;
        [Export] private float jumpSpeed = 5.0f;

        public bool[] Inputs { get; set; }
        private float yVelocity;
        private CharacterBody3D characterBody;

        public override void _Ready()
        {
            // Get references to required components
            if (player == null)
                player = GetParent<ServerPlayer>();
                
            characterBody = GetParent<CharacterBody3D>();
            
            // Scale values for Godot's frame-rate independent physics
            gravity *= (float)(Engine.PhysicsTicksPerSecond * Engine.PhysicsTicksPerSecond) / 3600.0f;
            moveSpeed *= (float)Engine.PhysicsTicksPerSecond / 60.0f;
            jumpSpeed *= (float)Engine.PhysicsTicksPerSecond / 60.0f;

            Inputs = new bool[5];
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 inputDirection = Vector2.Zero;
            if (Inputs[0]) // Forward
                inputDirection.Y += 1;

            if (Inputs[1]) // Backward
                inputDirection.Y -= 1;

            if (Inputs[2]) // Left
                inputDirection.X -= 1;

            if (Inputs[3]) // Right
                inputDirection.X += 1;

            Move(inputDirection, (float)delta);
        }

        private void Move(Vector2 inputDirection, float delta)
        {
            Transform3D transform = characterBody.GlobalTransform;
            Vector3 velocity = characterBody.Velocity;
            
            // Calculate movement direction based on player orientation
            Vector3 forward = -transform.Basis.Z; // Forward direction in Godot
            Vector3 right = transform.Basis.X;    // Right direction in Godot
            
            Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
            moveDirection = moveDirection.Normalized() * moveSpeed;

            // Handle gravity and jumping
            if (characterBody.IsOnFloor())
            {
                yVelocity = 0f;
                if (Inputs[4]) // Jump
                    yVelocity = jumpSpeed;
            }
            
            yVelocity += gravity * delta;

            // Set velocity for CharacterBody3D
            velocity.X = moveDirection.X;
            velocity.Z = moveDirection.Z;
            velocity.Y = yVelocity;
            
            characterBody.Velocity = velocity;
            characterBody.MoveAndSlide();

            SendMovement();
        }

        #region Messages
        private void SendMovement()
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.PlayerMovement);
            message.AddUShort(player.Id);
            message.AddVector3(characterBody.GlobalPosition);
            
            // Get forward direction
            Vector3 forward = -characterBody.GlobalTransform.Basis.Z;
            message.AddVector3(forward);
            
            NetworkManager.Singleton.Server.SendToAll(message);
        }
        #endregion
    }
}