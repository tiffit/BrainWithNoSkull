using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net.Pckt.Sb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client;

public class PlayerController {

    private readonly GameClient _client;
    private readonly ClientPlayerEntity _player;
    private readonly InputHandler _inputHandler;
    
    public float Pitch;
    public float Yaw;
    public Vector3D<float> LookDirection;
    
    private bool _hasMouseMoved;
    private Vector3D<float> _velocity;
    
    public PlayerController(GameClient client, ClientPlayerEntity player, InputHandler inputHandler){
        _client = client;
        _player = player;
        _inputHandler = inputHandler;
    }

    public void Update(double delta){
        _inputHandler.Tick(delta, out Vector2D<float> mouseMovement);
        if (!_hasMouseMoved && mouseMovement.Equals(Vector2D<float>.Zero)) {
            _hasMouseMoved = true;
            mouseMovement = Vector2D<float>.Zero;
        }

        Pitch = Math.Clamp(Pitch + mouseMovement.Y / 300f, -MathF.PI / 2 + 0.0001f, MathF.PI / 2 - 0.0001f);
        Yaw = (Yaw - mouseMovement.X / 300f) % MathF.Tau;

        LookDirection = Vector3D.Normalize(
            Vector3D.Transform(new Vector3D<float>(0, 0, 1), Matrix4X4.CreateFromYawPitchRoll(Yaw, Pitch, 0))
        );

        float movementSpeed = 5f * (float)delta;
        Vector3D<float> lookHorizontal = Vector3D.Normalize(new Vector3D<float>(LookDirection.X, 0, LookDirection.Z));
        Vector3D<float> lookCross = Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, lookHorizontal));

        Vector3D<float> moveDelta = new Vector3D<float>();
        
        if (_inputHandler.IsKeyDown(Key.A)) {
            moveDelta += lookCross;
        }

        if (_inputHandler.IsKeyDown(Key.D)) {
            moveDelta -= lookCross;
        }

        if (_inputHandler.IsKeyDown(Key.W)) {
            moveDelta += lookHorizontal;
        }

        if (_inputHandler.IsKeyDown(Key.S)) {
            moveDelta -= lookHorizontal;
        }

        if (moveDelta != Vector3D<float>.Zero) {
            moveDelta = Vector3D.Normalize(moveDelta) * movementSpeed;
        }

        bool onGround = _player.IsOnGround();
        if (onGround) {
            _velocity.Y = MathF.Max(0, _velocity.Y);
        }
        else {
            _velocity.Y = MathF.Max(-40, _velocity.Y + -18*(float)delta);
        }
        
        if (onGround && _inputHandler.IsKeyDown(Key.Space) && _velocity.Y == 0) {
            _velocity = new Vector3D<float>(0, 7, 0);
        }

        moveDelta += _velocity * (float)delta;

        CollisionTest test = new CollisionTest(_player.World!, _player.GetBoundingBox(), moveDelta);
        
        Vector3D<float> newPlayerPos = _player.Position + test.RunTest();

        WorldBase? world = _player.World;
        if (world != null) {
            float worldSize = world.Settings.WorldSize * 16;
            if (newPlayerPos.X < 0) {
                newPlayerPos.X = worldSize + newPlayerPos.X;
            }else if (newPlayerPos.X > worldSize) {
                newPlayerPos.X -= worldSize;
            }
            if (newPlayerPos.Z < 0) {
                newPlayerPos.Z = worldSize + newPlayerPos.Z;
            }else if (newPlayerPos.Z > worldSize) {
                newPlayerPos.Z -= worldSize;
            }   
        }

        if (_inputHandler.IsButtonPressedThisFrame(MouseButton.Left)) {
            if (_client.BlockRayResult != null) {
                Vector3D<int> blockPos = _client.BlockRayResult.Value.Pos;
                _client.TickActions.Enqueue(() => {
                    if (world != null) {
                        world.SetBlock(blockPos, BlockRegistry.Air);
                    }
                });
                _player.ServerConnection.SendPacket(new SbPlayerBreakBlockPacket(blockPos));
            }
        }

        _player.SetPosition(newPlayerPos);
        _inputHandler.Reset();
    }
    
}