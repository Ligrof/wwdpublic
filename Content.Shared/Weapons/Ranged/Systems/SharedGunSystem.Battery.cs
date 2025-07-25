using Content.Shared._White.Guns;
using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    protected virtual void InitializeBattery()
    {
        // Trying to dump comp references hence the below
        // Hitscan
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine);

        // Projectile
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine);

        // WWDP EDIT START
        // Container, Projectile
        SubscribeLocalEvent<ProjectileContainerBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<ProjectileContainerBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<ProjectileContainerBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<ProjectileContainerBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        //SubscribeLocalEvent<ProjectileContainerBatteryAmmoProviderComponent, ExaminedEvent>(OnBatteryExamine); // you'll have trouble examining something inside a container

        // Container, Hitscan
        SubscribeLocalEvent<HitscanContainerBatteryAmmoProviderComponent, ComponentGetState>(OnBatteryGetState);
        SubscribeLocalEvent<HitscanContainerBatteryAmmoProviderComponent, ComponentHandleState>(OnBatteryHandleState);
        SubscribeLocalEvent<HitscanContainerBatteryAmmoProviderComponent, TakeAmmoEvent>(OnBatteryTakeAmmo);
        SubscribeLocalEvent<HitscanContainerBatteryAmmoProviderComponent, GetAmmoCountEvent>(OnBatteryAmmoCount);
        // WWDP EDIT END
    }

    private void OnBatteryHandleState(EntityUid uid, BatteryAmmoProviderComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not BatteryAmmoProviderComponentState state)
            return;

        component.Shots = state.Shots;
        component.Capacity = state.MaxShots;
        component.FireCost = state.FireCost;

        if (component is HitscanBatteryAmmoProviderComponent hitscan && state.Prototype != null) // Shitmed Change
            hitscan.Prototype = state.Prototype;
    }

    private void OnBatteryGetState(EntityUid uid, BatteryAmmoProviderComponent component, ref ComponentGetState args)
    {
        var state = new BatteryAmmoProviderComponentState() // Shitmed Change
        {
            Shots = component.Shots,
            MaxShots = component.Capacity,
            FireCost = component.FireCost,
        };

        if (TryComp<HitscanBatteryAmmoProviderComponent>(uid, out var hitscan)) // Shitmed Change
            state.Prototype = hitscan.Prototype;

        args.State = state; // Shitmed Change
    }

    private void OnBatteryExamine(EntityUid uid, BatteryAmmoProviderComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("gun-battery-examine", ("color", AmmoExamineColor), ("count", component.Shots)));
    }

    private void OnBatteryTakeAmmo(EntityUid uid, BatteryAmmoProviderComponent component, TakeAmmoEvent args)
    {
        var shots = Math.Min(args.Shots, component.Shots);

        // Don't dirty if it's an empty fire.
        if (shots == 0)
            return;

        for (var i = 0; i < shots; i++)
        {
            args.Ammo.Add(GetShootable(component, args.Coordinates));
            component.Shots--;
        }

        TakeCharge(uid, component);
        UpdateBatteryAppearance(uid, component);
        Dirty(uid, component);
    }

    private void OnBatteryAmmoCount(EntityUid uid, BatteryAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        args.Count = component.Shots;
        args.Capacity = component.Capacity;
    }

    /// <summary>
    /// Update the battery (server-only) whenever fired.
    /// </summary>
    protected virtual void TakeCharge(EntityUid uid, BatteryAmmoProviderComponent component) { }

    protected void UpdateBatteryAppearance(EntityUid uid, BatteryAmmoProviderComponent component)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        Appearance.SetData(uid, AmmoVisuals.HasAmmo, component.Shots != 0, appearance);
        Appearance.SetData(uid, AmmoVisuals.AmmoCount, component.Shots, appearance);
        Appearance.SetData(uid, AmmoVisuals.AmmoMax, component.Capacity, appearance);
    }

    private (EntityUid? Entity, IShootable) GetShootable(BatteryAmmoProviderComponent component, EntityCoordinates coordinates)
    {
        switch (component)
        {
            case ProjectileBatteryAmmoProviderComponent proj:
                var ent = Spawn(proj.Prototype, coordinates);
                return (ent, EnsureShootable(ent));
            case HitscanBatteryAmmoProviderComponent hitscan:
                return (null, ProtoManager.Index<HitscanPrototype>(hitscan.Prototype));
            case ProjectileContainerBatteryAmmoProviderComponent proj:
                var entc = Spawn(proj.Prototype, coordinates);
                return (entc, EnsureShootable(entc));
            case HitscanContainerBatteryAmmoProviderComponent hitscan:
                return (null, ProtoManager.Index<HitscanPrototype>(hitscan.Prototype));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Serializable, NetSerializable]
    private sealed class BatteryAmmoProviderComponentState : ComponentState
    {
        public int Shots;
        public int MaxShots;
        public float FireCost;
        public string? Prototype; // Shitmed Change
    }
}
