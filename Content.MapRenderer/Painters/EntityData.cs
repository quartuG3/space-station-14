using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Content.MapRenderer.Painters;

public readonly record struct EntityData(EntityUid Owner, SpriteComponent Sprite, float X, float Y, Angle Rotation)
{
    public readonly EntityUid Owner = Owner;

    public readonly SpriteComponent Sprite = Sprite;

    public readonly float X = X;

    public readonly float Y = Y;

    public readonly Angle Rotation = Rotation;
}
