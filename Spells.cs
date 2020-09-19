﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurePlease
{
	public static class Spells
	{
		private static SpellInfo[] spells = new[]
		{
			new SpellInfo("Cure", 2f, 8),
			new SpellInfo("Cure II", 2.25f, 24),
			new SpellInfo("Cure III", 2.5f, 46),
			new SpellInfo("Cure IV", 2.5f, 88),
			new SpellInfo("Cure V", 2.5f, 135),
			new SpellInfo("Cure VI", 2f, 227),
			new SpellInfo("Curaga", 4.5f, 60),
			new SpellInfo("Curaga II", 4.75f, 120),
			new SpellInfo("Curaga III", 5f, 180),
			new SpellInfo("Curaga IV", 5.25f, 260),
			new SpellInfo("Curaga V", 5.5f, 380),
			new SpellInfo("Raise", 15f, 150),
			new SpellInfo("Raise II", 14f, 150),
			new SpellInfo("Poisona", 1f, 8),
			new SpellInfo("Paralyna", 1f, 12),
			new SpellInfo("Blindna", 1f, 16),
			new SpellInfo("Silena", 1f, 24),
			new SpellInfo("Stona", 1f, 40),
			new SpellInfo("Viruna", 1f, 48),
			new SpellInfo("Cursna", 1f, 30),
			new SpellInfo("Holy", 0.75f, 100),
			new SpellInfo("Holy II", 1f, 150),
			new SpellInfo("Dia", 1f, 7),
			new SpellInfo("Dia II", 1.5f, 30),
			new SpellInfo("Dia III", 2f, 45),
			new SpellInfo("Dia IV", 2.5f, 164),
			new SpellInfo("Dia V", 3f, 217),
			new SpellInfo("Banish", 2f, 15),
			new SpellInfo("Banish II", 2.5f, 57),
			new SpellInfo("Banish III", 3f, 96),
			new SpellInfo("Banish IV", 5.75f, 108),
			new SpellInfo("Banish V", 7.5f, 159),
			new SpellInfo("Diaga", 1.5f, 12),
			new SpellInfo("Diaga II", 1.75f, 60),
			new SpellInfo("Diaga III", 2f, 120),
			new SpellInfo("Diaga IV", 2.25f, 180),
			new SpellInfo("Diaga V", 2.5f, 240),
			new SpellInfo("Banishga", 2.75f, 41),
			new SpellInfo("Banishga II", 4.5f, 120),
			new SpellInfo("Banishga III", 6.5f, 233),
			new SpellInfo("Banishga IV", 5.75f, 380),
			new SpellInfo("Banishga V", 6f, 563),
			new SpellInfo("Protect", 1f, 9),
			new SpellInfo("Protect II", 1.25f, 28),
			new SpellInfo("Protect III", 1.5f, 46),
			new SpellInfo("Protect IV", 1.75f, 65),
			new SpellInfo("Protect V", 2f, 84),
			new SpellInfo("Shell", 1f, 18),
			new SpellInfo("Shell II", 1.25f, 37),
			new SpellInfo("Shell III", 1.5f, 56),
			new SpellInfo("Shell IV", 1.75f, 75),
			new SpellInfo("Shell V", 2f, 93),
			new SpellInfo("Blink", 6f, 20),
			new SpellInfo("Stoneskin", 7f, 29),
			new SpellInfo("Aquaveil", 5f, 12),
			new SpellInfo("Slow", 2f, 15),
			new SpellInfo("Haste", 3f, 40),
			new SpellInfo("Paralyze", 3f, 6),
			new SpellInfo("Silence", 3f, 16),
			new SpellInfo("Barfire", 0.5f, 6),
			new SpellInfo("Barblizzard", 0.5f, 6),
			new SpellInfo("Baraero", 0.5f, 6),
			new SpellInfo("Barstone", 0.5f, 6),
			new SpellInfo("Barthunder", 0.5f, 6),
			new SpellInfo("Barwater", 0.5f, 6),
			new SpellInfo("Barfira", 0.5f, 12),
			new SpellInfo("Barblizzara", 0.5f, 12),
			new SpellInfo("Baraera", 0.5f, 12),
			new SpellInfo("Barstonra", 0.5f, 12),
			new SpellInfo("Barthundra", 0.5f, 12),
			new SpellInfo("Barwatera", 0.5f, 12),
			new SpellInfo("Barsleep", 2.5f, 7),
			new SpellInfo("Barpoison", 2.5f, 9),
			new SpellInfo("Barparalyze", 2.5f, 11),
			new SpellInfo("Barblind", 2.5f, 13),
			new SpellInfo("Barsilence", 2.5f, 15),
			new SpellInfo("Barpetrify", 2.5f, 20),
			new SpellInfo("Barvirus", 2.5f, 25),
			new SpellInfo("Slow II", 3f, 45),
			new SpellInfo("Paralyze II", 3f, 36),
			new SpellInfo("Recall-Jugner", 20f, 125),
			new SpellInfo("Recall-Pashh", 20f, 125),
			new SpellInfo("Recall-Meriph", 20f, 125),
			new SpellInfo("Baramnesia", 2.5f, 30),
			new SpellInfo("Baramnesra", 5f, 60),
			new SpellInfo("Barsleepra", 5f, 14),
			new SpellInfo("Barpoisonra", 5f, 18),
			new SpellInfo("Barparalyzra", 5f, 22),
			new SpellInfo("Barblindra", 5f, 26),
			new SpellInfo("Barsilencera", 5f, 30),
			new SpellInfo("Barpetra", 5f, 40),
			new SpellInfo("Barvira", 5f, 50),
			new SpellInfo("Cura", 3f, 30),
			new SpellInfo("Sacrifice", 1f, 18),
			new SpellInfo("Esuna", 1f, 24),
			new SpellInfo("Auspice", 3f, 48),
			new SpellInfo("Reprisal", 1f, 24),
			new SpellInfo("Repose", 3f, 26),
			new SpellInfo("Sandstorm", 2f, 30),
			new SpellInfo("Enfire", 3f, 12),
			new SpellInfo("Enblizzard", 3f, 12),
			new SpellInfo("Enaero", 3f, 12),
			new SpellInfo("Enstone", 3f, 12),
			new SpellInfo("Enthunder", 3f, 12),
			new SpellInfo("Enwater", 3f, 12),
			new SpellInfo("Phalanx", 3f, 21),
			new SpellInfo("Phalanx II", 3f, 42),
			new SpellInfo("Regen", 1.5f, 15),
			new SpellInfo("Refresh", 5f, 40),
			new SpellInfo("Regen II", 1.75f, 36),
			new SpellInfo("Regen III", 2f, 64),
			new SpellInfo("Flash", 0.5f, 25),
			new SpellInfo("Rainstorm", 2f, 30),
			new SpellInfo("Windstorm", 2f, 30),
			new SpellInfo("Firestorm", 2f, 30),
			new SpellInfo("Hailstorm", 2f, 30),
			new SpellInfo("Thunderstorm", 2f, 30),
			new SpellInfo("Voidstorm", 2f, 30),
			new SpellInfo("Aurorastorm", 2f, 30),
			new SpellInfo("Teleport-Yhoat", 20f, 100),
			new SpellInfo("Teleport-Altep", 20f, 100),
			new SpellInfo("Teleport-Holla", 20f, 75),
			new SpellInfo("Teleport-Dem", 20f, 75),
			new SpellInfo("Teleport-Mea", 20f, 75),
			new SpellInfo("Protectra", 1f, 9),
			new SpellInfo("Protectra II", 1.25f, 28),
			new SpellInfo("Protectra III", 1.5f, 46),
			new SpellInfo("Protectra IV", 1.75f, 65),
			new SpellInfo("Protectra V", 2f, 84),
			new SpellInfo("Shellra", 1f, 18),
			new SpellInfo("Shellra II", 1.25f, 37),
			new SpellInfo("Shellra III", 1.5f, 56),
			new SpellInfo("Shellra IV", 1.75f, 75),
			new SpellInfo("Shellra V", 2f, 93),
			new SpellInfo("Reraise", 8f, 150),
			new SpellInfo("Invisible", 3f, 15),
			new SpellInfo("Sneak", 3f, 12),
			new SpellInfo("Deodorize", 2f, 10),
			new SpellInfo("Teleport-Vahzl", 20f, 100),
			new SpellInfo("Raise III", 13f, 150),
			new SpellInfo("Reraise II", 7.5f, 150),
			new SpellInfo("Reraise III", 7f, 150),
			new SpellInfo("Erase", 2.5f, 18),
			new SpellInfo("Fire", 0.5f, 7),
			new SpellInfo("Fire II", 1.5f, 26),
			new SpellInfo("Fire III", 3f, 63),
			new SpellInfo("Fire IV", 5f, 135),
			new SpellInfo("Fire V", 7.5f, 228),
			new SpellInfo("Blizzard", 0.5f, 8),
			new SpellInfo("Blizzard II", 1.5f, 31),
			new SpellInfo("Blizzard III", 3f, 75),
			new SpellInfo("Blizzard IV", 5f, 162),
			new SpellInfo("Blizzard V", 7.5f, 267),
			new SpellInfo("Aero", 0.5f, 6),
			new SpellInfo("Aero II", 1.5f, 22),
			new SpellInfo("Aero III", 3f, 54),
			new SpellInfo("Aero IV", 5f, 115),
			new SpellInfo("Aero V", 7.5f, 198),
			new SpellInfo("Stone", 0.5f, 4),
			new SpellInfo("Stone II", 1.5f, 16),
			new SpellInfo("Stone III", 3f, 40),
			new SpellInfo("Stone IV", 5f, 88),
			new SpellInfo("Stone V", 7.5f, 156),
			new SpellInfo("Thunder", 0.5f, 9),
			new SpellInfo("Thunder II", 1.5f, 37),
			new SpellInfo("Thunder III", 3f, 91),
			new SpellInfo("Thunder IV", 5f, 195),
			new SpellInfo("Thunder V", 7.5f, 306),
			new SpellInfo("Water", 0.5f, 5),
			new SpellInfo("Water II", 1.5f, 19),
			new SpellInfo("Water III", 3f, 46),
			new SpellInfo("Water IV", 5f, 99),
			new SpellInfo("Water V", 7.5f, 175),
			new SpellInfo("Firaga", 2f, 57),
			new SpellInfo("Firaga II", 4f, 153),
			new SpellInfo("Firaga III", 7f, 263),
			new SpellInfo("Firaga IV", 12f, 360),
			new SpellInfo("Firaga V", 16f, 450),
			new SpellInfo("Blizzaga", 2f, 80),
			new SpellInfo("Blizzaga II", 4f, 175),
			new SpellInfo("Blizzaga III", 7f, 297),
			new SpellInfo("Blizzaga IV", 12f, 403),
			new SpellInfo("Blizzaga V", 16f, 500),
			new SpellInfo("Aeroga", 2f, 45),
			new SpellInfo("Aeroga II", 4f, 131),
			new SpellInfo("Aeroga III", 7f, 232),
			new SpellInfo("Aeroga IV", 12f, 318),
			new SpellInfo("Aeroga V", 16f, 402),
			new SpellInfo("Stonega", 2f, 24),
			new SpellInfo("Stonega II", 4f, 93),
			new SpellInfo("Stonega III", 7f, 175),
			new SpellInfo("Stonega IV", 12f, 243),
			new SpellInfo("Stonega V", 16f, 315),
			new SpellInfo("Thundaga", 2f, 105),
			new SpellInfo("Thundaga II", 4f, 200),
			new SpellInfo("Thundaga III", 7f, 332),
			new SpellInfo("Thundaga IV", 12f, 450),
			new SpellInfo("Thundaga V", 16f, 520),
			new SpellInfo("Waterga", 2f, 34),
			new SpellInfo("Waterga II", 4f, 112),
			new SpellInfo("Waterga III", 7f, 202),
			new SpellInfo("Waterga IV", 12f, 280),
			new SpellInfo("Waterga V", 16f, 357),
			new SpellInfo("Flare", 8.5f, 315),
			new SpellInfo("Flare II", 7f, 280),
			new SpellInfo("Freeze", 8.5f, 315),
			new SpellInfo("Freeze II", 7f, 280),
			new SpellInfo("Tornado", 8.5f, 315),
			new SpellInfo("Tornado II", 7f, 280),
			new SpellInfo("Quake", 8.5f, 315),
			new SpellInfo("Quake II", 7f, 280),
			new SpellInfo("Burst", 8.5f, 315),
			new SpellInfo("Burst II", 7f, 280),
			new SpellInfo("Flood", 8.5f, 315),
			new SpellInfo("Flood II", 7f, 280),
			new SpellInfo("Gravity", 1.5f, 24),
			new SpellInfo("Gravity II", 1.5f, 36),
			new SpellInfo("Meteor", 8f, 418),
			new SpellInfo("Comet", 10f, 350),
			new SpellInfo("Poison", 1f, 5),
			new SpellInfo("Poison II", 1f, 38),
			new SpellInfo("Poison III", 1f, 72),
			new SpellInfo("Poison IV", 1f, 106),
			new SpellInfo("Poison V", 1f, 140),
			new SpellInfo("Poisonga", 2f, 44),
			new SpellInfo("Poisonga II", 2f, 112),
			new SpellInfo("Poisonga III", 2f, 180),
			new SpellInfo("Poisonga IV", 2f, 248),
			new SpellInfo("Poisonga V", 2f, 314),
			new SpellInfo("Bio", 1.5f, 15),
			new SpellInfo("Bio II", 1.5f, 36),
			new SpellInfo("Bio III", 1.5f, 54),
			new SpellInfo("Bio IV", 1.5f, 154),
			new SpellInfo("Bio V", 1.5f, 197),
			new SpellInfo("Burn", 2.5f, 25),
			new SpellInfo("Frost", 2.5f, 25),
			new SpellInfo("Choke", 2.5f, 25),
			new SpellInfo("Rasp", 2.5f, 25),
			new SpellInfo("Shock", 2.5f, 25),
			new SpellInfo("Drown", 2.5f, 25),
			new SpellInfo("Retrace", 5f, 150),
			new SpellInfo("Absorb-ACC", 0.5f, 33),
			new SpellInfo("Absorb-Attri", 0.5f, 33),
			new SpellInfo("Meteor II", 18f, 150),
			new SpellInfo("Drain", 3f, 21),
			new SpellInfo("Drain II", 3f, 37),
			new SpellInfo("Aspir", 3f, 10),
			new SpellInfo("Aspir II", 3f, 5),
			new SpellInfo("Blaze Spikes", 3f, 8),
			new SpellInfo("Ice Spikes", 3f, 16),
			new SpellInfo("Shock Spikes", 3f, 24),
			new SpellInfo("Stun", 0.5f, 25),
			new SpellInfo("Sleep", 2.5f, 19),
			new SpellInfo("Blind", 2f, 5),
			new SpellInfo("Break", 3f, 39),
			new SpellInfo("Virus", 1.5f, 10),
			new SpellInfo("Curse", 3f, 10),
			new SpellInfo("Bind", 2f, 8),
			new SpellInfo("Sleep II", 3f, 29),
			new SpellInfo("Dispel", 3f, 25),
			new SpellInfo("Warp", 4f, 100),
			new SpellInfo("Warp II", 5f, 150),
			new SpellInfo("Escape", 15f, 125),
			new SpellInfo("Tractor", 3f, 26),
			new SpellInfo("Tractor II", 3f, 50),
			new SpellInfo("Absorb-STR", 0.5f, 33),
			new SpellInfo("Absorb-DEX", 0.5f, 33),
			new SpellInfo("Absorb-VIT", 0.5f, 33),
			new SpellInfo("Absorb-AGI", 0.5f, 33),
			new SpellInfo("Absorb-INT", 0.5f, 33),
			new SpellInfo("Absorb-MND", 0.5f, 33),
			new SpellInfo("Absorb-CHR", 0.5f, 33),
			new SpellInfo("Sleepga", 3f, 38),
			new SpellInfo("Sleepga II", 3.5f, 58),
			new SpellInfo("Absorb-TP", 0.5f, 33),
			new SpellInfo("Blind II", 3f, 31),
			new SpellInfo("Dread Spikes", 3f, 78),
			new SpellInfo("Geohelix", 5f, 26),
			new SpellInfo("Hydrohelix", 5f, 26),
			new SpellInfo("Anemohelix", 5f, 26),
			new SpellInfo("Pyrohelix", 5f, 26),
			new SpellInfo("Cryohelix", 5f, 26),
			new SpellInfo("Ionohelix", 5f, 26),
			new SpellInfo("Noctohelix", 5f, 26),
			new SpellInfo("Luminohelix", 5f, 26),
			new SpellInfo("Addle", 2f, 36),
			new SpellInfo("Klimaform", 3f, 30),
			new SpellInfo("Fire Spirit", 1f, 10),
			new SpellInfo("Ice Spirit", 1f, 10),
			new SpellInfo("Air Spirit", 1f, 10),
			new SpellInfo("Earth Spirit", 1f, 10),
			new SpellInfo("Thunder Spirit", 1f, 10),
			new SpellInfo("Water Spirit", 1f, 10),
			new SpellInfo("Light Spirit", 1f, 10),
			new SpellInfo("Dark Spirit", 1f, 10),
			new SpellInfo("Carbuncle", 5f, 5),
			new SpellInfo("Fenrir", 5f, 15),
			new SpellInfo("Ifrit", 5f, 7),
			new SpellInfo("Titan", 5f, 7),
			new SpellInfo("Leviathan", 5f, 7),
			new SpellInfo("Garuda", 5f, 7),
			new SpellInfo("Shiva", 5f, 7),
			new SpellInfo("Ramuh", 5f, 7),
			new SpellInfo("Diabolos", 5f, 15),
			new SpellInfo("Odin", 1f, 0),
			new SpellInfo("Alexander", 1f, 0),
			new SpellInfo("Cait Sith", 5f, 5),
			new SpellInfo("Animus Augeo", 3f, 21),
			new SpellInfo("Animus Minuo", 3f, 21),
			new SpellInfo("Enlight", 3f, 24),
			new SpellInfo("Endark", 3f, 24),
			new SpellInfo("Enfire II", 3f, 24),
			new SpellInfo("Enblizzard II", 3f, 24),
			new SpellInfo("Enaero II", 3f, 24),
			new SpellInfo("Enstone II", 3f, 24),
			new SpellInfo("Enthunder II", 3f, 24),
			new SpellInfo("Enwater II", 3f, 24),
			new SpellInfo("Monomi: Ichi", 1.5f, 0),
			new SpellInfo("Aisha: Ichi", 4f, 0),
			new SpellInfo("Katon: Ichi", 4f, 0),
			new SpellInfo("Katon: Ni", 1.5f, 0),
			new SpellInfo("Katon: San", 2.75f, 0),
			new SpellInfo("Hyoton: Ichi", 4f, 0),
			new SpellInfo("Hyoton: Ni", 1.5f, 0),
			new SpellInfo("Hyoton: San", 2.75f, 0),
			new SpellInfo("Huton: Ichi", 4f, 0),
			new SpellInfo("Huton: Ni", 1.5f, 0),
			new SpellInfo("Huton: San", 2.75f, 0),
			new SpellInfo("Doton: Ichi", 4f, 0),
			new SpellInfo("Doton: Ni", 1.5f, 0),
			new SpellInfo("Doton: San", 2.75f, 0),
			new SpellInfo("Raiton: Ichi", 4f, 0),
			new SpellInfo("Raiton: Ni", 1.5f, 0),
			new SpellInfo("Raiton: San", 2.75f, 0),
			new SpellInfo("Suiton: Ichi", 4f, 0),
			new SpellInfo("Suiton: Ni", 1.5f, 0),
			new SpellInfo("Suiton: San", 2.75f, 0),
			new SpellInfo("Utsusemi: Ichi", 4f, 0),
			new SpellInfo("Utsusemi: Ni", 1.5f, 0),
			new SpellInfo("Utsusemi: San", 0.5f, 0),
			new SpellInfo("Jubaku: Ichi", 4f, 0),
			new SpellInfo("Jubaku: Ni", 1.5f, 0),
			new SpellInfo("Jubaku: San", 12f, 0),
			new SpellInfo("Hojo: Ichi", 4f, 0),
			new SpellInfo("Hojo: Ni", 1.5f, 0),
			new SpellInfo("Hojo: San", 12f, 0),
			new SpellInfo("Kurayami: Ichi", 4f, 0),
			new SpellInfo("Kurayami: Ni", 1.5f, 0),
			new SpellInfo("Kurayami: San", 12f, 0),
			new SpellInfo("Dokumori: Ichi", 4f, 0),
			new SpellInfo("Dokumori: Ni", 1.5f, 0),
			new SpellInfo("Dokumori: San", 12f, 0),
			new SpellInfo("Tonko: Ichi", 1.5f, 0),
			new SpellInfo("Tonko: Ni", 1.5f, 0),
			new SpellInfo("Siren", 5f, 7),
			new SpellInfo("Paralyga", 4f, 12),
			new SpellInfo("Slowga", 2.5f, 30),
			new SpellInfo("Hastega", 4f, 80),
			new SpellInfo("Silencega", 4f, 32),
			new SpellInfo("Dispelga", 3f, 200),
			new SpellInfo("Blindga", 2.5f, 10),
			new SpellInfo("Bindga", 2.5f, 16),
			new SpellInfo("Sleepga", 3f, 38),
			new SpellInfo("Sleepga II", 4f, 58),
			new SpellInfo("Breakga", 3.5f, 78),
			new SpellInfo("Graviga", 2f, 48),
			new SpellInfo("Death", 3f, 1),
			new SpellInfo("Foe Requiem", 2f, 0),
			new SpellInfo("Foe Requiem II", 2f, 0),
			new SpellInfo("Foe Requiem III", 2f, 0),
			new SpellInfo("Foe Requiem IV", 2f, 0),
			new SpellInfo("Foe Requiem V", 2f, 0),
			new SpellInfo("Foe Requiem VI", 2f, 0),
			new SpellInfo("Foe Requiem VII", 2f, 0),
			new SpellInfo("Foe Requiem VIII", 2f, 0),
			new SpellInfo("Horde Lullaby", 2f, 0),
			new SpellInfo("Horde Lullaby II", 2f, 0),
			new SpellInfo("Army's Paeon", 8f, 0),
			new SpellInfo("Army's Paeon II", 8f, 0),
			new SpellInfo("Army's Paeon III", 8f, 0),
			new SpellInfo("Army's Paeon IV", 8f, 0),
			new SpellInfo("Army's Paeon V", 8f, 0),
			new SpellInfo("Army's Paeon VI", 8f, 0),
			new SpellInfo("Army's Paeon VII", 8f, 0),
			new SpellInfo("Army's Paeon VIII", 8f, 0),
			new SpellInfo("Mage's Ballad", 8f, 0),
			new SpellInfo("Mage's Ballad II", 8f, 0),
			new SpellInfo("Mage's Ballad III", 8f, 0),
			new SpellInfo("Knight's Minne", 8f, 0),
			new SpellInfo("Knight's Minne II", 8f, 0),
			new SpellInfo("Knight's Minne III", 8f, 0),
			new SpellInfo("Knight's Minne IV", 8f, 0),
			new SpellInfo("Knight's Minne V", 8f, 0),
			new SpellInfo("Valor Minuet", 8f, 0),
			new SpellInfo("Valor Minuet II", 8f, 0),
			new SpellInfo("Valor Minuet III", 8f, 0),
			new SpellInfo("Valor Minuet IV", 8f, 0),
			new SpellInfo("Valor Minuet V", 8f, 0),
			new SpellInfo("Sword Madrigal", 8f, 0),
			new SpellInfo("Blade Madrigal", 8f, 0),
			new SpellInfo("Hunter's Prelude", 8f, 0),
			new SpellInfo("Archer's Prelude", 8f, 0),
			new SpellInfo("Sheepfoe Mambo", 8f, 0),
			new SpellInfo("Dragonfoe Mambo", 8f, 0),
			new SpellInfo("Fowl Aubade", 8f, 0),
			new SpellInfo("Herb Pastoral", 8f, 0),
			new SpellInfo("Chocobo Hum", 8f, 0),
			new SpellInfo("Shining Fantasia", 8f, 0),
			new SpellInfo("Scop's Operetta", 8f, 0),
			new SpellInfo("Puppet's Operetta", 8f, 0),
			new SpellInfo("Jester's Operetta", 8f, 0),
			new SpellInfo("Gold Capriccio", 8f, 0),
			new SpellInfo("Devotee Serenade", 8f, 0),
			new SpellInfo("Warding Round", 8f, 0),
			new SpellInfo("Goblin Gavotte", 8f, 0),
			new SpellInfo("Cactuar Fugue", 8f, 0),
			new SpellInfo("Honor March", 8f, 0),
			new SpellInfo("Protected Aria", 8f, 0),
			new SpellInfo("Advancing March", 8f, 0),
			new SpellInfo("Victory March", 8f, 0),
			new SpellInfo("Battlefield Elegy", 2f, 0),
			new SpellInfo("Carnage Elegy", 2f, 0),
			new SpellInfo("Massacre Elegy", 2f, 0),
			new SpellInfo("Sinewy Etude", 8f, 0),
			new SpellInfo("Dextrous Etude", 8f, 0),
			new SpellInfo("Vivacious Etude", 8f, 0),
			new SpellInfo("Quick Etude", 8f, 0),
			new SpellInfo("Learned Etude", 8f, 0),
			new SpellInfo("Spirited Etude", 8f, 0),
			new SpellInfo("Enchanting Etude", 8f, 0),
			new SpellInfo("Herculean Etude", 8f, 0),
			new SpellInfo("Uncanny Etude", 8f, 0),
			new SpellInfo("Vital Etude", 8f, 0),
			new SpellInfo("Swift Etude", 8f, 0),
			new SpellInfo("Sage Etude", 8f, 0),
			new SpellInfo("Logical Etude", 8f, 0),
			new SpellInfo("Bewitching Etude", 8f, 0),
			new SpellInfo("Fire Carol", 8f, 0),
			new SpellInfo("Ice Carol", 8f, 0),
			new SpellInfo("Wind Carol", 8f, 0),
			new SpellInfo("Earth Carol", 8f, 0),
			new SpellInfo("Lightning Carol", 8f, 0),
			new SpellInfo("Water Carol", 8f, 0),
			new SpellInfo("Light Carol", 8f, 0),
			new SpellInfo("Dark Carol", 8f, 0),
			new SpellInfo("Fire Carol II", 8f, 0),
			new SpellInfo("Ice Carol II", 8f, 0),
			new SpellInfo("Wind Carol II", 8f, 0),
			new SpellInfo("Earth Carol II", 8f, 0),
			new SpellInfo("Lightning Carol II", 8f, 0),
			new SpellInfo("Water Carol II", 8f, 0),
			new SpellInfo("Light Carol II", 8f, 0),
			new SpellInfo("Dark Carol II", 8f, 0),
			new SpellInfo("Fire Threnody", 2f, 0),
			new SpellInfo("Ice Threnody", 2f, 0),
			new SpellInfo("Wind Threnody", 2f, 0),
			new SpellInfo("Earth Threnody", 2f, 0),
			new SpellInfo("Ltng. Threnody", 2f, 0),
			new SpellInfo("Water Threnody", 2f, 0),
			new SpellInfo("Light Threnody", 2f, 0),
			new SpellInfo("Dark Threnody", 2f, 0),
			new SpellInfo("Magic Finale", 2f, 0),
			new SpellInfo("Foe Lullaby", 2f, 0),
			new SpellInfo("Goddess's Hymnus", 4f, 0),
			new SpellInfo("Chocobo Mazurka", 4f, 0),
			new SpellInfo("Maiden's Virelai", 4f, 0),
			new SpellInfo("Raptor Mazurka", 4f, 0),
			new SpellInfo("Foe Sirvente", 8f, 0),
			new SpellInfo("Adventurer's Dirge", 8f, 0),
			new SpellInfo("Sentinel's Scherzo", 8f, 0),
			new SpellInfo("Foe Lullaby II", 2f, 0),
			new SpellInfo("Pining Nocturne", 2f, 0),
			new SpellInfo("Refresh II", 5f, 60),
			new SpellInfo("Cura II", 3f, 45),
			new SpellInfo("Cura III", 3f, 60),
			new SpellInfo("Crusade", 3f, 18),
			new SpellInfo("Regen IV", 2.25f, 82),
			new SpellInfo("Embrava", 3f, 1),
			new SpellInfo("Boost-STR", 5f, 36),
			new SpellInfo("Boost-DEX", 5f, 36),
			new SpellInfo("Boost-VIT", 5f, 36),
			new SpellInfo("Boost-AGI", 5f, 36),
			new SpellInfo("Boost-INT", 5f, 36),
			new SpellInfo("Boost-MND", 5f, 36),
			new SpellInfo("Boost-CHR", 5f, 36),
			new SpellInfo("Gain-STR", 5f, 36),
			new SpellInfo("Gain-DEX", 5f, 36),
			new SpellInfo("Gain-VIT", 5f, 36),
			new SpellInfo("Gain-AGI", 5f, 36),
			new SpellInfo("Gain-INT", 5f, 36),
			new SpellInfo("Gain-MND", 5f, 36),
			new SpellInfo("Gain-CHR", 5f, 36),
			new SpellInfo("Temper", 3f, 36),
			new SpellInfo("Arise", 12f, 300),
			new SpellInfo("Adloquium", 5f, 50),
			new SpellInfo("Firaja", 7f, 358),
			new SpellInfo("Blizzaja", 7f, 378),
			new SpellInfo("Aeroja", 7f, 338),
			new SpellInfo("Stoneja", 7f, 298),
			new SpellInfo("Thundaja", 7f, 398),
			new SpellInfo("Waterja", 7f, 318),
			new SpellInfo("Kaustra", 5f, 1),
			new SpellInfo("Impact", 12f, 666),
			new SpellInfo("Regen V", 2.5f, 100),
			new SpellInfo("Gekka: Ichi", 3f, 0),
			new SpellInfo("Yain: Ichi", 3f, 0),
			new SpellInfo("Myoshu: Ichi", 3f, 0),
			new SpellInfo("Yurin: Ichi", 4f, 0),
			new SpellInfo("Kakka: Ichi", 3f, 0),
			new SpellInfo("Migawari: Ichi", 1.5f, 0),
			new SpellInfo("Haste II", 3f, 80),
			new SpellInfo("Venom Shell", 3f, 86),
			new SpellInfo("Maelstrom", 6f, 162),
			new SpellInfo("Metallic Body", 2.5f, 19),
			new SpellInfo("Screwdriver", 0.5f, 21),
			new SpellInfo("MP Drainkiss", 4f, 20),
			new SpellInfo("Death Ray", 4.5f, 49),
			new SpellInfo("Sandspin", 1.5f, 10),
			new SpellInfo("Smite of Rage", 0.5f, 28),
			new SpellInfo("Bludgeon", 0.5f, 16),
			new SpellInfo("Refueling", 1.5f, 29),
			new SpellInfo("Ice Break", 5.25f, 142),
			new SpellInfo("Blitzstrahl", 4.5f, 70),
			new SpellInfo("Self-Destruct", 3.25f, 100),
			new SpellInfo("Mysterious Light", 3.75f, 73),
			new SpellInfo("Cold Wave", 4f, 37),
			new SpellInfo("Poison Breath", 3f, 22),
			new SpellInfo("Stinking Gas", 4f, 37),
			new SpellInfo("Memento Mori", 3.5f, 46),
			new SpellInfo("Terror Touch", 3.25f, 62),
			new SpellInfo("Spinal Cleave", 0.5f, 61),
			new SpellInfo("Blood Saber", 4f, 25),
			new SpellInfo("Digest", 4f, 20),
			new SpellInfo("Mandibular Bite", 0.5f, 38),
			new SpellInfo("Cursed Sphere", 3f, 36),
			new SpellInfo("Sickle Slash", 0.5f, 41),
			new SpellInfo("Cocoon", 1.75f, 10),
			new SpellInfo("Filamented Hold", 2f, 38),
			new SpellInfo("Pollen", 2f, 8),
			new SpellInfo("Power Attack", 0.5f, 5),
			new SpellInfo("Death Scissors", 0.5f, 51),
			new SpellInfo("Magnetite Cloud", 4.5f, 86),
			new SpellInfo("Eyes On Me", 4.5f, 112),
			new SpellInfo("Frenetic Rip", 0.5f, 61),
			new SpellInfo("Frightful Roar", 2f, 32),
			new SpellInfo("Hecatomb Wave", 5.25f, 116),
			new SpellInfo("Body Slam", 1f, 74),
			new SpellInfo("Radiant Breath", 5.25f, 116),
			new SpellInfo("Helldive", 0.5f, 16),
			new SpellInfo("Jet Stream", 0.5f, 47),
			new SpellInfo("Blood Drain", 4f, 10),
			new SpellInfo("Sound Blast", 4f, 25),
			new SpellInfo("Feather Tickle", 4f, 48),
			new SpellInfo("Feather Barrier", 2f, 29),
			new SpellInfo("Jettatura", 0.5f, 37),
			new SpellInfo("Yawn", 3f, 55),
			new SpellInfo("Foot Kick", 0.5f, 5),
			new SpellInfo("Wild Carrot", 2.5f, 37),
			new SpellInfo("Voracious Trunk", 10f, 72),
			new SpellInfo("Healing Breeze", 4.5f, 55),
			new SpellInfo("Chaotic Eye", 3f, 13),
			new SpellInfo("Sheep Song", 3f, 22),
			new SpellInfo("Ram Charge", 0.5f, 79),
			new SpellInfo("Claw Cyclone", 1f, 24),
			new SpellInfo("Lowing", 7f, 66),
			new SpellInfo("Dimensional Death", 0.5f, 48),
			new SpellInfo("Heat Breath", 7.5f, 169),
			new SpellInfo("Blank Gaze", 3f, 25),
			new SpellInfo("Magic Fruit", 2.5f, 72),
			new SpellInfo("Uppercut", 0.5f, 31),
			new SpellInfo("1000 Needles", 12f, 350),
			new SpellInfo("Pinecone Bomb", 2.5f, 48),
			new SpellInfo("Sprout Smack", 0.5f, 6),
			new SpellInfo("Soporific", 3f, 38),
			new SpellInfo("Queasyshroom", 2f, 20),
			new SpellInfo("Wild Oats", 0.5f, 9),
			new SpellInfo("Bad Breath", 8.75f, 212),
			new SpellInfo("Geist Wall", 3f, 35),
			new SpellInfo("Awful Eye", 2.5f, 32),
			new SpellInfo("Frost Breath", 6.5f, 136),
			new SpellInfo("Infrasonics", 3f, 42),
			new SpellInfo("Disseverment", 0.5f, 74),
			new SpellInfo("Actinic Burst", 0.5f, 24),
			new SpellInfo("Reactor Cool", 3f, 28),
			new SpellInfo("Saline Coat", 3f, 66),
			new SpellInfo("Plasma Charge", 3f, 24),
			new SpellInfo("Temporal Shift", 0.5f, 48),
			new SpellInfo("Vertical Cleave", 0.5f, 86),
			new SpellInfo("Blastbomb", 2.25f, 36),
			new SpellInfo("Battle Dance", 1f, 12),
			new SpellInfo("Sandspray", 3f, 43),
			new SpellInfo("Grand Slam", 1f, 24),
			new SpellInfo("Head Butt", 0.5f, 12),
			new SpellInfo("Bomb Toss", 3.75f, 42),
			new SpellInfo("Frypan", 1f, 65),
			new SpellInfo("Flying Hip Press", 5.75f, 125),
			new SpellInfo("Hydro Shot", 0.5f, 55),
			new SpellInfo("Diamondhide", 7f, 99),
			new SpellInfo("Enervation", 3.5f, 48),
			new SpellInfo("Light of Penance", 3f, 53),
			new SpellInfo("Warm-Up", 2.5f, 59),
			new SpellInfo("Firespit", 6.5f, 121),
			new SpellInfo("Feather Storm", 0.5f, 12),
			new SpellInfo("Tail Slap", 1f, 77),
			new SpellInfo("Hysteric Barrage", 0.5f, 61),
			new SpellInfo("Amplification", 3.5f, 48),
			new SpellInfo("Cannonball", 0.5f, 66),
			new SpellInfo("Mind Blast", 3f, 82),
			new SpellInfo("Exuviation", 3f, 40),
			new SpellInfo("Magic Hammer", 4f, 40),
			new SpellInfo("Zephyr Mantle", 3.5f, 31),
			new SpellInfo("Regurgitation", 3f, 69),
			new SpellInfo("Seedspray", 2.5f, 61),
			new SpellInfo("Corrosive Ooze", 3.5f, 55),
			new SpellInfo("Spiral Spin", 2.5f, 39),
			new SpellInfo("Asuran Claws", 2f, 81),
			new SpellInfo("Sub-zero Smash", 1f, 44),
			new SpellInfo("Triumphant Roar", 0.5f, 24),
			new SpellInfo("Acrid Stream", 3f, 89),
			new SpellInfo("Blazing Bound", 4f, 113),
			new SpellInfo("Plenilune Embrace", 2.75f, 106),
			new SpellInfo("Demoralizing Roar", 2.75f, 46),
			new SpellInfo("Cimicine Discharge", 1.5f, 32),
			new SpellInfo("Animating Wail", 2f, 53),
			new SpellInfo("Battery Charge", 3.5f, 50),
			new SpellInfo("Leafstorm", 6f, 132),
			new SpellInfo("Regeneration", 1.5f, 36),
			new SpellInfo("Final Sting", 5f, 88),
			new SpellInfo("Goblin Rush", 0.5f, 76),
			new SpellInfo("Vanity Dive", 0.5f, 58),
			new SpellInfo("Magic Barrier", 5f, 29),
			new SpellInfo("Whirl of Rage", 1f, 73),
			new SpellInfo("Benthic Typhoon", 0.5f, 43),
			new SpellInfo("Auroral Drape", 4f, 51),
			new SpellInfo("Osmosis", 6f, 47),
			new SpellInfo("Quad. Continuum", 1f, 91),
			new SpellInfo("Fantod", 0.5f, 12),
			new SpellInfo("Thermal Pulse", 5.5f, 151),
			new SpellInfo("Empty Thrash", 0.5f, 33),
			new SpellInfo("Dream Flower", 2f, 68),
			new SpellInfo("Occultation", 1.5f, 138),
			new SpellInfo("Charged Whisker", 5f, 183),
			new SpellInfo("Winds of Promy.", 2.5f, 36),
			new SpellInfo("Delta Thrust", 0.5f, 28),
			new SpellInfo("Evryone. Grudge", 6f, 185),
			new SpellInfo("Reaving Wind", 4f, 84),
			new SpellInfo("Barrier Tusk", 5f, 41),
			new SpellInfo("Mortal Ray", 8.5f, 267),
			new SpellInfo("Water Bomb", 2.5f, 67),
			new SpellInfo("Heavy Strike", 0.5f, 32),
			new SpellInfo("Dark Orb", 7f, 124),
			new SpellInfo("White Wind", 4.5f, 145),
			new SpellInfo("Sudden Lunge", 0.5f, 18),
			new SpellInfo("Quadrastrike", 0.5f, 98),
			new SpellInfo("Vapor Spray", 3f, 172),
			new SpellInfo("Thunder Breath", 7f, 193),
			new SpellInfo("O. Counterstance", 4f, 18),
			new SpellInfo("Amorphic Spikes", 0.5f, 79),
			new SpellInfo("Wind Breath", 1f, 26),
			new SpellInfo("Barbed Crescent", 0.5f, 52),
			new SpellInfo("Nat. Meditation", 1f, 38),
			new SpellInfo("Tem. Upheaval", 0.5f, 133),
			new SpellInfo("Rending Deluge", 2f, 118),
			new SpellInfo("Embalming Earth", 3f, 57),
			new SpellInfo("Paralyzing Triad", 0.5f, 33),
			new SpellInfo("Foul Waters", 3.5f, 76),
			new SpellInfo("Glutinous Dart", 0.5f, 16),
			new SpellInfo("Retinal Glare", 0.5f, 26),
			new SpellInfo("Subduction", 1f, 27),
			new SpellInfo("Thrashing Assault", 0.5f, 119),
			new SpellInfo("Erratic Flutter", 2f, 92),
			new SpellInfo("Restoral", 2f, 127),
			new SpellInfo("Rail Cannon", 2.5f, 200),
			new SpellInfo("Diffusion Ray", 4f, 238),
			new SpellInfo("Sinker Drill", 0.5f, 91),
			new SpellInfo("Molting Plumage", 1f, 146),
			new SpellInfo("Nectarous Deluge", 3f, 97),
			new SpellInfo("Sweeping Gouge", 0.5f, 29),
			new SpellInfo("Atra. Libations", 4f, 164),
			new SpellInfo("Searing Tempest", 6f, 116),
			new SpellInfo("Spectral Floe", 6f, 116),
			new SpellInfo("Anvil Lightning", 6f, 116),
			new SpellInfo("Entomb", 6f, 116),
			new SpellInfo("Saurian Slide", 0.5f, 109),
			new SpellInfo("Palling Salvo", 3f, 175),
			new SpellInfo("Blinding Fulgor", 6f, 116),
			new SpellInfo("Scouring Spate", 6f, 116),
			new SpellInfo("Silent Storm", 6f, 116),
			new SpellInfo("Tenebral Crush", 6f, 116),
			new SpellInfo("Thunderbolt", 3.5f, 138),
			new SpellInfo("Harden Shell", 1.5f, 20),
			new SpellInfo("Absolute Terror", 0.5f, 29),
			new SpellInfo("Gates of Hades", 3.5f, 156),
			new SpellInfo("Tourbillion", 1f, 108),
			new SpellInfo("Pyric Bulwark", 1.5f, 50),
			new SpellInfo("Bilgestorm", 1f, 122),
			new SpellInfo("Bloodrake", 0.5f, 99),
			new SpellInfo("Droning Whirlwind", 1.5f, 224),
			new SpellInfo("Carcharian Verve", 1f, 65),
			new SpellInfo("Blistering Roar", 0.5f, 43),
			new SpellInfo("Uproot", 1.5f, 88),
			new SpellInfo("Crashing Thunder", 1f, 172),
			new SpellInfo("Polar Roar", 3f, 126),
			new SpellInfo("Mighty Guard", 3f, 299),
			new SpellInfo("Cruel Joke", 3f, 187),
			new SpellInfo("Cesspool", 3f, 166),
			new SpellInfo("Tearing Gust", 3f, 202),
			new SpellInfo("Indi-Regen", 2f, 37),
			new SpellInfo("Indi-Poison", 2f, 12),
			new SpellInfo("Indi-Refresh", 2f, 63),
			new SpellInfo("Indi-Haste", 2f, 100),
			new SpellInfo("Indi-STR", 2f, 63),
			new SpellInfo("Indi-DEX", 2f, 63),
			new SpellInfo("Indi-VIT", 2f, 63),
			new SpellInfo("Indi-AGI", 2f, 63),
			new SpellInfo("Indi-INT", 2f, 63),
			new SpellInfo("Indi-MND", 2f, 63),
			new SpellInfo("Indi-CHR", 2f, 63),
			new SpellInfo("Indi-Fury", 2f, 70),
			new SpellInfo("Indi-Barrier", 2f, 59),
			new SpellInfo("Indi-Acumen", 2f, 91),
			new SpellInfo("Indi-Fend", 2f, 80),
			new SpellInfo("Indi-Precision", 2f, 25),
			new SpellInfo("Indi-Voidance", 2f, 17),
			new SpellInfo("Indi-Focus", 2f, 49),
			new SpellInfo("Indi-Attunement", 2f, 38),
			new SpellInfo("Indi-Wilt", 2f, 161),
			new SpellInfo("Indi-Frailty", 2f, 147),
			new SpellInfo("Indi-Fade", 2f, 186),
			new SpellInfo("Indi-Malaise", 2f, 174),
			new SpellInfo("Indi-Slip", 2f, 112),
			new SpellInfo("Indi-Torpor", 2f, 101),
			new SpellInfo("Indi-Vex", 2f, 136),
			new SpellInfo("Indi-Languor", 2f, 124),
			new SpellInfo("Indi-Slow", 2f, 94),
			new SpellInfo("Indi-Paralysis", 2f, 107),
			new SpellInfo("Indi-Gravity", 2f, 174),
			new SpellInfo("Geo-Regen", 2f, 74),
			new SpellInfo("Geo-Poison", 2f, 25),
			new SpellInfo("Geo-Refresh", 2f, 126),
			new SpellInfo("Geo-Haste", 2f, 200),
			new SpellInfo("Geo-STR", 2f, 126),
			new SpellInfo("Geo-DEX", 2f, 126),
			new SpellInfo("Geo-VIT", 2f, 126),
			new SpellInfo("Geo-AGI", 2f, 126),
			new SpellInfo("Geo-INT", 2f, 126),
			new SpellInfo("Geo-MND", 2f, 126),
			new SpellInfo("Geo-CHR", 2f, 126),
			new SpellInfo("Geo-Fury", 2f, 140),
			new SpellInfo("Geo-Barrier", 2f, 119),
			new SpellInfo("Geo-Acumen", 2f, 182),
			new SpellInfo("Geo-Fend", 2f, 161),
			new SpellInfo("Geo-Precision", 2f, 50),
			new SpellInfo("Geo-Voidance", 2f, 35),
			new SpellInfo("Geo-Focus", 2f, 98),
			new SpellInfo("Geo-Attunement", 2f, 77),
			new SpellInfo("Geo-Wilt", 2f, 322),
			new SpellInfo("Geo-Frailty", 2f, 294),
			new SpellInfo("Geo-Fade", 2f, 372),
			new SpellInfo("Geo-Malaise", 2f, 348),
			new SpellInfo("Geo-Slip", 2f, 225),
			new SpellInfo("Geo-Torpor", 2f, 203),
			new SpellInfo("Geo-Vex", 2f, 273),
			new SpellInfo("Geo-Languor", 2f, 249),
			new SpellInfo("Geo-Slow", 2f, 189),
			new SpellInfo("Geo-Paralysis", 2f, 215),
			new SpellInfo("Geo-Gravity", 2f, 348),
			new SpellInfo("Fira", 1.5f, 93),
			new SpellInfo("Fira II", 3f, 206),
			new SpellInfo("Blizzara", 1.5f, 108),
			new SpellInfo("Blizzara II", 3f, 229),
			new SpellInfo("Aera", 1.5f, 79),
			new SpellInfo("Aera II", 3f, 184),
			new SpellInfo("Stonera", 1.5f, 54),
			new SpellInfo("Stonera II", 3f, 143),
			new SpellInfo("Thundara", 1.5f, 123),
			new SpellInfo("Thundara II", 3f, 253),
			new SpellInfo("Watera", 1.5f, 66),
			new SpellInfo("Watera II", 3f, 163),
			new SpellInfo("Foil", 1f, 48),
			new SpellInfo("Distract", 3f, 32),
			new SpellInfo("Distract II", 3f, 58),
			new SpellInfo("Frazzle", 3f, 38),
			new SpellInfo("Frazzle II", 3f, 64),
			new SpellInfo("Flurry", 3f, 40),
			new SpellInfo("Flurry II", 3f, 80),
			new SpellInfo("Atomos", 5f, 50),
			new SpellInfo("Reraise IV", 6.5f, 150),
			new SpellInfo("Fire VI", 10.5f, 339),
			new SpellInfo("Blizzard VI", 10.5f, 386),
			new SpellInfo("Aero VI", 10.5f, 299),
			new SpellInfo("Stone VI", 10.5f, 237),
			new SpellInfo("Thunder VI", 10.5f, 437),
			new SpellInfo("Water VI", 10.5f, 266),
			new SpellInfo("Enlight II", 3f, 36),
			new SpellInfo("Endark II", 3f, 36),
			new SpellInfo("Sandstorm II", 2f, 60),
			new SpellInfo("Rainstorm II", 2f, 60),
			new SpellInfo("Windstorm II", 2f, 60),
			new SpellInfo("Firestorm II", 2f, 60),
			new SpellInfo("Hailstorm II", 2f, 60),
			new SpellInfo("Thunderstorm II", 2f, 60),
			new SpellInfo("Voidstorm II", 2f, 60),
			new SpellInfo("Aurorastorm II", 2f, 60),
			new SpellInfo("Fira III", 4.5f, 390),
			new SpellInfo("Blizzara III", 4.5f, 432),
			new SpellInfo("Aera III", 4.5f, 350),
			new SpellInfo("Stonera III", 4.5f, 276),
			new SpellInfo("Thundara III", 4.5f, 476),
			new SpellInfo("Watera III", 4.5f, 312),
			new SpellInfo("Fire Threnody II", 2f, 0),
			new SpellInfo("Ice Threnody II", 2f, 0),
			new SpellInfo("Wind Threnody II", 2f, 0),
			new SpellInfo("Earth Threnody II", 2f, 0),
			new SpellInfo("Ltng. Threnody II", 2f, 0),
			new SpellInfo("Water Threnody II", 2f, 0),
			new SpellInfo("Light Threnody II", 2f, 0),
			new SpellInfo("Dark Threnody II", 2f, 0),
			new SpellInfo("Inundation", 3f, 48),
			new SpellInfo("Drain III", 3f, 53),
			new SpellInfo("Aspir III", 3f, 2),
			new SpellInfo("Distract III", 3f, 84),
			new SpellInfo("Frazzle III", 3f, 90),
			new SpellInfo("Addle II", 2f, 63),
			new SpellInfo("Geohelix II", 7.5f, 78),
			new SpellInfo("Hydrohelix II", 7.5f, 78),
			new SpellInfo("Anemohelix II", 7.5f, 78),
			new SpellInfo("Pyrohelix II", 7.5f, 78),
			new SpellInfo("Cryohelix II", 7.5f, 78),
			new SpellInfo("Ionohelix II", 7.5f, 78),
			new SpellInfo("Noctohelix II", 7.5f, 78),
			new SpellInfo("Luminohelix II", 7.5f, 78),
			new SpellInfo("Full Cure", 1.5f, 1),
			new SpellInfo("Refresh III", 5f, 80),
			new SpellInfo("Temper II", 3f, 72),
			new SpellInfo("Shantotto", 2f, 0),
			new SpellInfo("Naji", 2f, 0),
			new SpellInfo("Kupipi", 2f, 0),
			new SpellInfo("Excenmille", 2f, 0),
			new SpellInfo("Ayame", 2f, 0),
			new SpellInfo("Nanaa Mihgo", 2f, 0),
			new SpellInfo("Curilla", 2f, 0),
			new SpellInfo("Volker", 2f, 0),
			new SpellInfo("Ajido-Marujido", 2f, 0),
			new SpellInfo("Trion", 2f, 0),
			new SpellInfo("Zeid", 2f, 0),
			new SpellInfo("Lion", 2f, 0),
			new SpellInfo("Tenzen", 2f, 0),
			new SpellInfo("Mihli Aliapoh", 2f, 0),
			new SpellInfo("Valaineral", 2f, 0),
			new SpellInfo("Joachim", 2f, 0),
			new SpellInfo("Naja Salaheem", 2f, 0),
			new SpellInfo("Prishe", 2f, 0),
			new SpellInfo("Ulmia", 2f, 0),
			new SpellInfo("Shikaree Z", 2f, 0),
			new SpellInfo("Cherukiki", 2f, 0),
			new SpellInfo("Iron Eater", 2f, 0),
			new SpellInfo("Gessho", 2f, 0),
			new SpellInfo("Gadalar", 2f, 0),
			new SpellInfo("Rainemard", 2f, 0),
			new SpellInfo("Ingrid", 2f, 0),
			new SpellInfo("Lehko Habhoka", 2f, 0),
			new SpellInfo("Nashmeira", 2f, 0),
			new SpellInfo("Zazarg", 2f, 0),
			new SpellInfo("Ovjang", 2f, 0),
			new SpellInfo("Mnejing", 2f, 0),
			new SpellInfo("Sakura", 2f, 0),
			new SpellInfo("Luzaf", 2f, 0),
			new SpellInfo("Najelith", 2f, 0),
			new SpellInfo("Aldo", 2f, 0),
			new SpellInfo("Moogle", 2f, 0),
			new SpellInfo("Fablinix", 2f, 0),
			new SpellInfo("Maat", 2f, 0),
			new SpellInfo("D. Shantotto", 2f, 0),
			new SpellInfo("Star Sibyl", 2f, 0),
			new SpellInfo("Karaha-Baruha", 2f, 0),
			new SpellInfo("Cid", 2f, 0),
			new SpellInfo("Gilgamesh", 2f, 0),
			new SpellInfo("Areuhat", 2f, 0),
			new SpellInfo("Semih Lafihna", 2f, 0),
			new SpellInfo("Elivira", 2f, 0),
			new SpellInfo("Noillurie", 2f, 0),
			new SpellInfo("Lhu Mhakaracca", 2f, 0),
			new SpellInfo("Ferreous Coffin", 2f, 0),
			new SpellInfo("Lilisette", 2f, 0),
			new SpellInfo("Mumor", 2f, 0),
			new SpellInfo("Uka Totlihn", 2f, 0),
			new SpellInfo("Klara", 2f, 0),
			new SpellInfo("Romaa Mihgo", 2f, 0),
			new SpellInfo("Kuyin Hathdenna", 2f, 0),
			new SpellInfo("Rahal", 2f, 0),
			new SpellInfo("Koru-Moru", 2f, 0),
			new SpellInfo("Pieuje (UC)", 2f, 0),
			new SpellInfo("I. Shield (UC)", 2f, 0),
			new SpellInfo("Apururu (UC)", 2f, 0),
			new SpellInfo("Jakoh (UC)", 2f, 0),
			new SpellInfo("Flaviria (UC)", 2f, 0),
			new SpellInfo("Babban", 2f, 0),
			new SpellInfo("Abenzio", 2f, 0),
			new SpellInfo("Rughadjeen", 2f, 0),
			new SpellInfo("Kukki-Chebukki", 2f, 0),
			new SpellInfo("Margret", 2f, 0),
			new SpellInfo("Chacharoon", 2f, 0),
			new SpellInfo("Lhe Lhangavo", 2f, 0),
			new SpellInfo("Arciela", 2f, 0),
			new SpellInfo("Mayakov", 2f, 0),
			new SpellInfo("Qultada", 2f, 0),
			new SpellInfo("Adelheid", 2f, 0),
			new SpellInfo("Amchuchu", 2f, 0),
			new SpellInfo("Brygid", 2f, 0),
			new SpellInfo("Mildaurion", 2f, 0),
			new SpellInfo("Halver", 2f, 0),
			new SpellInfo("Rongelouts", 2f, 0),
			new SpellInfo("Leonoyne", 2f, 0),
			new SpellInfo("Maximilian", 2f, 0),
			new SpellInfo("Kayeel-Payeel", 2f, 0),
			new SpellInfo("Robel-Akbel", 2f, 0),
			new SpellInfo("Kupofried", 2f, 0),
			new SpellInfo("Selh'teus", 2f, 0),
			new SpellInfo("Yoran-Oran (UC)", 2f, 0),
			new SpellInfo("Sylvie (UC)", 2f, 0),
			new SpellInfo("Abquhbah", 2f, 0),
			new SpellInfo("Balamor", 2f, 0),
			new SpellInfo("August", 2f, 0),
			new SpellInfo("Rosulatia", 2f, 0),
			new SpellInfo("Teodor", 2f, 0),
			new SpellInfo("Ullegore", 2f, 0),
			new SpellInfo("Makki-Chebukki", 2f, 0),
			new SpellInfo("King of Hearts", 2f, 0),
			new SpellInfo("Morimar", 2f, 0),
			new SpellInfo("Darrcuiln", 2f, 0),
			new SpellInfo("AAHM", 2f, 0),
			new SpellInfo("AAEV", 2f, 0),
			new SpellInfo("AAMR", 2f, 0),
			new SpellInfo("AATT", 2f, 0),
			new SpellInfo("AAGK", 2f, 0),
			new SpellInfo("Iroha", 2f, 0),
			new SpellInfo("Ygnas", 2f, 0),
			new SpellInfo("Monberaux", 2f, 0),
			new SpellInfo("Excenmille [S]", 2f, 0),
			new SpellInfo("Ayame (UC)", 2f, 0),
			new SpellInfo("Maat (UC)", 2f, 0),
			new SpellInfo("Aldo (UC)", 2f, 0),
			new SpellInfo("Naja (UC)", 2f, 0),
			new SpellInfo("Lion II", 2f, 0),
			new SpellInfo("Zeid II", 2f, 0),
			new SpellInfo("Prishe II", 2f, 0),
			new SpellInfo("Nashmeira II", 2f, 0),
			new SpellInfo("Lilisette II", 2f, 0),
			new SpellInfo("Tenzen II", 2f, 0),
			new SpellInfo("Mumor II", 2f, 0),
			new SpellInfo("Ingrid II", 2f, 0),
			new SpellInfo("Arciela II", 2f, 0),
			new SpellInfo("Iroha II", 2f, 0),
			new SpellInfo("Shantotto II", 2f, 0),
		};

		public static float GetCastTimeSeconds(string name)
    {
			foreach (var spell in spells)
      {
				if (spell.Name.ToLower() == name.ToLower())
        {
					return spell.CastTime;
        }
      }

			return 3f;
    }
	}

	public class SpellInfo
	{
		public string Name { get; set; }
		public float CastTime { get; set; }
		public int MpCost { get; set; }
		public SpellInfo(string name, float castTime, int mpCost)
		{
			Name = name;
			CastTime = castTime;
			MpCost = MpCost;
		}
	}
}
