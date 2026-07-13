using System;
using System.Collections.Generic;

public static class WeaponEffectivenessCatalog
{
	private static readonly IReadOnlyDictionary<(WeaponClass WeaponClass, ArmorClass ArmorClass), float> Modifiers =
		new Dictionary<(WeaponClass, ArmorClass), float>
		{
			[(WeaponClass.SMALL_ARMS, ArmorClass.LIGHT)] = 1.25f,
			[(WeaponClass.SMALL_ARMS, ArmorClass.MEDIUM)] = 0.75f,
			[(WeaponClass.SMALL_ARMS, ArmorClass.HEAVY)] = 0.40f,
			[(WeaponClass.SMALL_ARMS, ArmorClass.STRUCTURE)] = 0.30f,
			[(WeaponClass.SMALL_ARMS, ArmorClass.AIR)] = 0.00f,
			
			[(WeaponClass.CANNON, ArmorClass.LIGHT)] = 1.00f,
			[(WeaponClass.CANNON, ArmorClass.MEDIUM)] = 1.00f,
			[(WeaponClass.CANNON, ArmorClass.HEAVY)] = 0.85f,
			[(WeaponClass.CANNON, ArmorClass.STRUCTURE)] = 0.75f,
			[(WeaponClass.CANNON, ArmorClass.AIR)] = 0.00f,

			[(WeaponClass.ANTI_ARMOR, ArmorClass.LIGHT)] = 0.75f,
			[(WeaponClass.ANTI_ARMOR, ArmorClass.MEDIUM)] = 1.25f,
			[(WeaponClass.ANTI_ARMOR, ArmorClass.HEAVY)] = 1.50f,
			[(WeaponClass.ANTI_ARMOR, ArmorClass.STRUCTURE)] = 1.00f,
			[(WeaponClass.ANTI_ARMOR, ArmorClass.AIR)] = 0.00f,

			[(WeaponClass.NON_COMBAT, ArmorClass.LIGHT)] = 0.00f,
			[(WeaponClass.NON_COMBAT, ArmorClass.MEDIUM)] = 0.00f,
			[(WeaponClass.NON_COMBAT, ArmorClass.HEAVY)] = 0.00f,
			[(WeaponClass.NON_COMBAT, ArmorClass.STRUCTURE)] = 0.00f,
			[(WeaponClass.NON_COMBAT, ArmorClass.AIR)] = 0.00f,

			[(WeaponClass.ANTI_AIR, ArmorClass.LIGHT)] = 0.25f,
			[(WeaponClass.ANTI_AIR, ArmorClass.MEDIUM)] = 0.25f,
			[(WeaponClass.ANTI_AIR, ArmorClass.HEAVY)] = 0.25f,
			[(WeaponClass.ANTI_AIR, ArmorClass.STRUCTURE)] = 0.10f,
			[(WeaponClass.ANTI_AIR, ArmorClass.AIR)] = 1.50f,
		};

	public static float GetModifier(WeaponClass weaponClass, ArmorClass armorClass)
	{
		if (Modifiers.TryGetValue((weaponClass, armorClass), out var modifier))
			return modifier;

		throw new ArgumentOutOfRangeException(
			nameof(weaponClass),
			$"No weapon effectiveness is defined for {weaponClass} against {armorClass}."
		);
	}
}
