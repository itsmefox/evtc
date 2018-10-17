using ScratchEVTCParser.Model.Agents;

namespace ScratchEVTCParser.Model
{
	public static class SkillIds
	{
		// Boons and Conditions
		public const int Protection = 717;
		public const int Regeneration = 718;
		public const int Swiftness = 719;
		public const int Blinded = 720;
		public const int Crippled = 721;
		public const int Chilled = 722;
		public const int Poisoned = 723;
		public const int Fury = 725;
		public const int Vigor = 726;
		public const int Immobile = 727;
		public const int Bleeding = 736;
		public const int Burning = 737;
		public const int Vulnerability = 738;
		public const int Might = 740;
		public const int Weakness = 742;
		public const int Aegis = 743;
		public const int Fear = 791;
		public const int Confusion = 861;
		public const int Retaliation = 873;
		public const int Stability = 1122;
		public const int Quickness = 1187;
		public const int Torment = 19426;
		public const int Alacrity = 30328;

		// Buffs
		public const int Superspeed = 5974;

		public const int Invulnerability = 757;
		public const int Determined = 762;
		public const int GorsevalInvulnerability = 31790;
		public const int QadimFlameArmor = 52568;
	}

	public class SkillDefinition
	{
		public int Id { get; }
		public string InternalName { get; }
		public Profession Profession { get; }
		public EliteSpecialization EliteSpecialization { get; }
		public SkillSlot SkillSlot { get; }

		public SkillDefinition(int id, string internalName, SkillSlot skillSlot,
			Profession profession = Profession.None, EliteSpecialization eliteSpecialization = EliteSpecialization.None)
		{
			Id = id;
			InternalName = internalName;
			SkillSlot = skillSlot;
			Profession = profession;
			EliteSpecialization = eliteSpecialization;
		}
	}
}