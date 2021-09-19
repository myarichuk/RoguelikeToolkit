var health = target.Get<HealthComponent>();
var kickStrength = source.Get<KickAbility>().Strength;

health.Value -= kickStrength.Roll();
target.Set(health);