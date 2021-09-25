var health = target.GetComponent(HealthComponent);
var kickStrength = source.GetComponent(KickAbility).Strength;

health.Value -= kickStrength.Roll();
target.SetComponent(HealthComponent, health);