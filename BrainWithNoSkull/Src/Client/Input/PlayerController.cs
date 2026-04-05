using BrainWithNoSkull.Client.Guis;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net.Pckt.Sb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Input;

public class PlayerController {
    private readonly GameClient _client;
    private readonly ClientPlayerEntity _player;
    private readonly InputHandler _inputHandler;

    public float Pitch;
    public float Yaw;
    public Vector3D<float> LookDirection;

    private bool _hasMouseMoved;
    private Vector3D<float> _velocity;

    private readonly Keybinding _keybindMoveLeft = new Keybinding("Left", new BoundInput(Key.A), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindMoveRight = new Keybinding("Right", new BoundInput(Key.D), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindMoveForward = new Keybinding("Forward", new BoundInput(Key.W), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindMoveBackwards = new Keybinding("Backwards", new BoundInput(Key.S), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindJump = new Keybinding("Jump", new BoundInput(Key.Space), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindBreakBlock = new Keybinding("Break Block", new BoundInput(MouseButton.Left), KeyContext.FirstPerson, true);
    private readonly Keybinding _keybindInteract = new Keybinding("Interact", new BoundInput(MouseButton.Right), KeyContext.FirstPerson, false);
    
    private readonly Keybinding _keybindHotbar1 = new Keybinding("Hotbar 1", new BoundInput(Key.Number1), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar2 = new Keybinding("Hotbar 2", new BoundInput(Key.Number2), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar3 = new Keybinding("Hotbar 3", new BoundInput(Key.Number3), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar4 = new Keybinding("Hotbar 4", new BoundInput(Key.Number4), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar5 = new Keybinding("Hotbar 5", new BoundInput(Key.Number5), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar6 = new Keybinding("Hotbar 6", new BoundInput(Key.Number6), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar7 = new Keybinding("Hotbar 7", new BoundInput(Key.Number7), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar8 = new Keybinding("Hotbar 8", new BoundInput(Key.Number8), KeyContext.FirstPerson, false);
    private readonly Keybinding _keybindHotbar9 = new Keybinding("Hotbar 9", new BoundInput(Key.Number9), KeyContext.FirstPerson, false);
    
    private readonly Keybinding _keybindInventory = new Keybinding("Inventory", new BoundInput(Key.Tab), KeyContext.All, false);
    private readonly Keybinding _keybindPause = new Keybinding("Pause", new BoundInput(Key.Escape), KeyContext.All, false);

    public PlayerController(GameClient client, ClientPlayerEntity player, InputHandler inputHandler){
        _client = client;
        _player = player;
        _inputHandler = inputHandler;
    }

    public void Update(double delta){
        _inputHandler.Tick(delta, out Vector2D<float> mouseMovement);
        KeyContext context = _client.Gui == null ? KeyContext.FirstPerson : KeyContext.Gui;
        if (context == KeyContext.Gui) {
            mouseMovement = new Vector2D<float>();
            Gui gui = _client.Gui!;
            foreach (MouseButton btn in _inputHandler.MouseDownThisFrame) {
                gui.HandleMouseButtonInput(btn, true);
            }
        }

        if (!_hasMouseMoved && mouseMovement.Equals(Vector2D<float>.Zero)) {
            _hasMouseMoved = true;
            mouseMovement = Vector2D<float>.Zero;
        }

        float pitchPadding = float.DegreesToRadians(0.5f);

        Pitch = Math.Clamp(Pitch + mouseMovement.Y / 300f, -MathF.PI / 2 + pitchPadding, MathF.PI / 2 - pitchPadding);
        Yaw = (Yaw - mouseMovement.X / 300f) % MathF.Tau;

        LookDirection = Vector3D.Normalize(
            Vector3D.Transform(new Vector3D<float>(0, 0, 1), Matrix4X4.CreateFromYawPitchRoll(Yaw, Pitch, 0))
        );

        float movementSpeed = 5f * (float)delta;
        Vector3D<float> lookHorizontal = Vector3D.Normalize(new Vector3D<float>(LookDirection.X, 0, LookDirection.Z));
        Vector3D<float> lookCross = Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, lookHorizontal));

        Vector3D<float> moveDelta = new Vector3D<float>();

        if (_keybindMoveLeft.IsPressed(context, _inputHandler)) {
            moveDelta += lookCross;
        }

        if (_keybindMoveRight.IsPressed(context, _inputHandler)) {
            moveDelta -= lookCross;
        }

        if (_keybindMoveForward.IsPressed(context, _inputHandler)) {
            moveDelta += lookHorizontal;
        }

        if (_keybindMoveBackwards.IsPressed(context, _inputHandler)) {
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
            _velocity.Y = MathF.Max(-40, _velocity.Y + -18 * (float)delta);
        }

        if (onGround && _keybindJump.IsPressed(context, _inputHandler) && _velocity.Y == 0) {
            _velocity = new Vector3D<float>(0, 7, 0);
        }

        moveDelta += _velocity * (float)delta;

        AABB playerBox = _player.GetBoundingBox();
        CollisionTest test = new CollisionTest(_player.World!, playerBox, moveDelta);
        Vector3D<float> newMove = moveDelta;
        if (moveDelta.LengthSquared > 0) {
            newMove = test.RunTest(out List<AABB> collided);
            if (collided.Count > 0 && moveDelta.Y == 0) {
                float maxY = collided.Max(aabb => aabb.MaxY);
                float stepAmount = maxY - playerBox.MinY;
                if (stepAmount < 0.6f) {
                    test = new CollisionTest(_player.World!, playerBox + new Vector3D<float>(0, stepAmount, 0), moveDelta);
                    newMove = test.RunTest(out collided) + new Vector3D<float>(0, stepAmount, 0);
                }
            }
        }

        Vector3D<float> newPlayerPos = _player.Position + newMove;

        if (newMove.Y == 0) _velocity.Y = 0;
        if (newMove.X == 0) _velocity.X = 0;
        if (newMove.Z == 0) _velocity.Z = 0;

        WorldBase? world = _player.World;
        if (world != null) {
            float worldSize = world.Settings.WorldSize * 16;
            if (newPlayerPos.X < 0) {
                newPlayerPos.X = worldSize + newPlayerPos.X;
            }
            else if (newPlayerPos.X > worldSize) {
                newPlayerPos.X -= worldSize;
            }

            if (newPlayerPos.Z < 0) {
                newPlayerPos.Z = worldSize + newPlayerPos.Z;
            }
            else if (newPlayerPos.Z > worldSize) {
                newPlayerPos.Z -= worldSize;
            }
        }

        if (_keybindBreakBlock.IsPressed(context, _inputHandler)) {
            if (_client.BlockRayResult != null && !_player.BlockBreakPause) {
                Vector3D<int> blockPos = _client.BlockRayResult.Value.Pos;
                if (_player.BlockBreakPosition == null || _player.BlockBreakPosition != blockPos) {
                    _player.BlockBreakPosition = blockPos;
                    _player.BlockBreakTime = 0;
                }

                float breakMult = _player.Inventory.GetHeldStack()?.Item.BreakSpeed ?? 1;
                _player.BlockBreakTime += (float)delta * breakMult;
                if (_player.BlockBreakTime >= world!.GetBlock(blockPos).BreakTime) {
                    _client.TickActions.Enqueue(() => {
                        _player.World?.SetBlock(blockPos, BlockRegistry.Air);
                        _player.BlockBreakPause = false;
                    });
                    _player.ServerConnection.SendPacket(new SbPlayerBreakBlockPacket(blockPos));
                    _player.BlockBreakPosition = null;
                    _player.BlockBreakTime = 0;
                    _player.BlockBreakPause = true;
                }
            }
        }
        else {
            _player.BlockBreakPosition = null;
            _player.BlockBreakTime = 0;
            if (_keybindInteract.IsPressed(context, _inputHandler)) {
                WorldRayResult? result = _client.BlockRayResult;
                if (result != null) {
                    ItemStack? stack = _player.Inventory.GetHeldStack();
                    if (!ItemStack.IsNull(stack)) {
                        _client.TickActions.Enqueue(() => {
                            if (world != null) {
                                ItemStack? updatedStack = _player.Inventory.GetHeldStack();
                                if (!ItemStack.IsNull(updatedStack)) {
                                    stack.Item.Interact(result.Value, world, _player, updatedStack);
                                }
                            }
                        });
                        _player.ServerConnection.SendPacket(new SbItemInteractPacket(result.Value));
                    }
                }
            }
        }

        int oldHotbarSlot = _player.Inventory.HotbarSlot;
        if (_keybindHotbar1.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 0;
        else if (_keybindHotbar2.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 1;
        else if (_keybindHotbar3.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 2;
        else if (_keybindHotbar4.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 3;
        else if (_keybindHotbar5.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 4;
        else if (_keybindHotbar6.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 5;
        else if (_keybindHotbar7.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 6;
        else if (_keybindHotbar8.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 7;
        else if (_keybindHotbar9.IsPressed(context, _inputHandler)) _player.Inventory.HotbarSlot = 8;

        if (_player.Inventory.HotbarSlot != oldHotbarSlot) {
            _player.ServerConnection.SendPacket(new SbChangeHotbarSlotPacket(_player.Inventory.HotbarSlot));
        }

        _player.SetPosition(newPlayerPos);

        if (_keybindInventory.IsPressed(context, _inputHandler)) {
            _client.SetGui(_client.Gui == null ? new InventoryGui() : null);
        }
        else if (_keybindPause.IsPressed(context, _inputHandler)) {
            _client.SetGui(_client.Gui == null ? new PauseGui() : null);
        }

        _inputHandler.Reset();
    }
}