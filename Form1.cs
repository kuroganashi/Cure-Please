namespace CurePlease
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Serialization;
	using CurePlease.Properties;
	using EliteMMO.API;

	public partial class Form1 : Form
	{
		public enum LoginStatus
		{
			CharacterLoginScreen = 0,
			Loading = 1,
			LoggedIn = 2
		}

		public enum Status : byte
		{
			Standing = 0,
			Fighting = 1,
			Dead1 = 2,
			Dead2 = 3,
			Event = 4,
			Chocobo = 5,
			Healing = 33,
			Synthing = 44,
			Sitting = 47,
			Fishing = 56,
			FishBite = 57,
			Obtained = 58,
			RodBreak = 59,
			LineBreak = 60,
			CatchMonster = 61,
			LostCatch = 62,
			Unknown
		}

		public class BuffStorage : List<BuffStorage>
		{
			public string CharacterName { get; set; }
			public string CharacterBuffs { get; set; }
		}

		public class CharacterData : List<CharacterData>
		{
			public int TargetIndex { get; set; }
			public int MemberNumber { get; set; }
		}

		public class SongData : List<SongData>
		{
			public string SongType { get; set; }
			public int SongPosition { get; set; }
			public string SongName { get; set; }
			public short BuffId { get; set; }
		}

		public class SpellsData : List<SpellsData>
		{
			public string Name { get; set; }
			public int Position { get; set; }
			public int Type { get; set; }
			public short BuffId { get; set; }
			public bool AoeVersion { get; set; }
		}

		public class GeoData : List<GeoData>
		{
			public int Position { get; set; }
			public string IndiSpell { get; set; }
			public string GeoSpell { get; set; }
		}

		public class JobTitles : List<JobTitles>
		{
			public int Number { get; set; }
			public string Name { get; set; }
		}

		private static readonly DateTime defaultDate = new DateTime(1970, 1, 1);

		private IPEndPoint endpoint;
		private readonly UdpClient listener;
		private Form settings;
		private Form2 form2 = new CurePlease.Form2();
		private string spellCommand = "";
		private float spellCastTime = 0f;
		private int currentSCHCharges = 0;
		private string debug_MSG_show = string.Empty;
		private int lastCommand = 0;
		private int lastKnownEstablisherTarget = 0;
		private int song_casting = 0;
		private int plBardCount = 0;
		private bool forceSongRecast = false;
		private string lastSongCastedName = string.Empty;
		public bool targetEngaged = false;
		public bool eclipticActive = false;
		public bool castingLocked = false;
		public bool abilityLocked = false;
		public string abilityCommand = string.Empty;
		private DateTime defaultTime = new DateTime(1970, 1, 1);
		private bool curePlease_autofollow = false;
		private List<string> characterNames_naRemoval = new List<string>();
		public string hookMode = "Windower";
		public List<SpellsData> barSpells = new List<SpellsData>();
		public List<SpellsData> enSpells = new List<SpellsData>();
		public List<SpellsData> stormSpells = new List<SpellsData>();
		public static EliteAPI instancePrimary;
		public static EliteAPI instanceMonitored;
		public ListBox processIds = new ListBox();
		public ListBox activeProcessIds = new ListBox();
		public int max_count = 10;
		public int spell_delay_count = 0;
		public int geo_step = 0;
		public int followWarning = 0;
		public bool stuckWarning = false;
		public int stuckCount = 0;
		public int idFound = 0;
		public float lastPlZ;
		public float lastPlX;
		public float lastPlY;
		private DateTime currentTime = DateTime.UtcNow;
		private DateTime lastTimePrimaryMoved = DateTime.Now;
		public List<BuffStorage> activeBuffs = new List<BuffStorage>();
		public List<SongData> bardSongs = new List<SongData>();
		public List<GeoData> geoSpells = new List<GeoData>();
		public string wakeSleepSpellName = "Cure";
		public string plSilenceitemName = "Echo Drops";
		public string plDoomItemName = "Holy Water";
		private float plX;
		private float plY;
		private float plZ;
		private byte playerOptionsSelected;
		private byte autoOptionsSelected;
		private bool pauseActions;
		private bool waitingForMp;
		private bool healingForMp;
		public int isAddonLoaded = 0;
		public int firstTimePause = 0;
		private bool enableActions = false;
		private readonly SemaphoreSlim casting = new SemaphoreSlim(1, 1);

		public List<string> tempItemZones = new List<string>
		{
			"Escha Ru'Aun", "Escha Zi'Tah", "Reisenjima", "Abyssea - La Theine", "Abyssea - Konschtat",
			"Abyssea - Tahrongi", "Abyssea - Attohwa", "Abyssea - Misareaux", "Abyssea - Vunkerl",
			"Abyssea - Altepa", "Abyssea - Uleguerand", "Abyssea - Grauberg", "Walk of Echoes"
		};

		private bool[] autoHasteEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoHaste_IIEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoFlurryEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoFlurry_IIEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoPhalanx_IIEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoRegen_Enabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoShell_Enabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoProtect_Enabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoSandstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoRainstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoWindstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoFirestormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoHailstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoThunderstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoVoidstormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoAurorastormEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoRefreshEnabled = Helpers.CreateAndFill<bool>(18, () => false);
		private bool[] autoAdloquium_Enabled = Helpers.CreateAndFill<bool>(18, () => false);

		private DateTime[] playerHaste = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerHaste_II = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerFlurry = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerFlurry_II = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerShell = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerProtect = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerPhalanx_II = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerRegen = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerRefresh = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerStormspell = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerAdloquium = Helpers.CreateAndFill<DateTime>(18, () => defaultDate);
		private DateTime[] playerSong1 = Helpers.CreateAndFill<DateTime>(1, () => defaultDate);
		private DateTime[] playerSong2 = Helpers.CreateAndFill<DateTime>(1, () => defaultDate);
		private DateTime[] playerSong3 = Helpers.CreateAndFill<DateTime>(1, () => defaultDate);
		private DateTime[] playerSong4 = Helpers.CreateAndFill<DateTime>(1, () => defaultDate);
		private DateTime[] lastSongCast = Helpers.CreateAndFill<DateTime>(1, () => defaultDate);
		private DateTime[] playerPianissimo1_1 = Helpers.CreateAndFill<DateTime>(6, () => defaultDate);
		private DateTime[] playerPianissimo2_1 = Helpers.CreateAndFill<DateTime>(6, () => defaultDate);
		private DateTime[] playerPianissimo1_2 = Helpers.CreateAndFill<DateTime>(6, () => defaultDate);
		private DateTime[] playerPianissimo2_2 = Helpers.CreateAndFill<DateTime>(6, () => defaultDate);

		private TimeSpan[] playerHasteSpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerHaste_IISpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerFlurrySpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerFlurry_IISpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerShell_Span = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerProtect_Span = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerPhalanx_IISpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerRegen_Span = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerRefresh_Span = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerStormspellSpan = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerAdloquium_Span = Helpers.CreateAndFill<TimeSpan>(18, () => new TimeSpan());
		private TimeSpan[] playerSong1_Span = Helpers.CreateAndFill<TimeSpan>(1, () => new TimeSpan());
		private TimeSpan[] playerSong2_Span = Helpers.CreateAndFill<TimeSpan>(1, () => new TimeSpan());
		private TimeSpan[] playerSong3_Span = Helpers.CreateAndFill<TimeSpan>(1, () => new TimeSpan());
		private TimeSpan[] playerSong4_Span = Helpers.CreateAndFill<TimeSpan>(1, () => new TimeSpan());
		private TimeSpan[] lastSongCast_Span = Helpers.CreateAndFill<TimeSpan>(1, () => new TimeSpan());
		private TimeSpan[] pianissimo1_1_Span = Helpers.CreateAndFill<TimeSpan>(6, () => new TimeSpan());
		private TimeSpan[] pianissimo2_1_Span = Helpers.CreateAndFill<TimeSpan>(6, () => new TimeSpan());
		private TimeSpan[] pianissimo1_2_Span = Helpers.CreateAndFill<TimeSpan>(6, () => new TimeSpan());
		private TimeSpan[] pianissimo2_2_Span = Helpers.CreateAndFill<TimeSpan>(6, () => new TimeSpan());

		public Form1()
		{
			endpoint = GetDynamicEndpoint();
			listener = new UdpClient(endpoint);

			StartPosition = FormStartPosition.CenterScreen;
			InitializeComponent();

			currentAction.Text = string.Empty;
			var position = 0;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minne",
				SongName = "Knight's Minne",
				SongPosition = position,
				BuffId = 197
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minne",
				SongName = "Knight's Minne II",
				SongPosition = position,
				BuffId = 197
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minne",
				SongName = "Knight's Minne III",
				SongPosition = position,
				BuffId = 197
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minne",
				SongName = "Knight's Minne IV",
				SongPosition = position,
				BuffId = 197
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minne",
				SongName = "Knight's Minne V",
				SongPosition = position,
				BuffId = 197
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minuet",
				SongName = "Valor Minuet",
				SongPosition = position,
				BuffId = 198
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minuet",
				SongName = "Valor Minuet II",
				SongPosition = position,
				BuffId = 198
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minuet",
				SongName = "Valor Minuet III",
				SongPosition = position,
				BuffId = 198
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minuet",
				SongName = "Valor Minuet IV",
				SongPosition = position,
				BuffId = 198
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Minuet",
				SongName = "Valor Minuet V",
				SongPosition = position,
				BuffId = 198
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon II",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon III",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon IV",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon V",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Paeon",
				SongName = "Army's Paeon VI",
				SongPosition = position,
				BuffId = 195
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Madrigal",
				SongName = "Sword Madrigal",
				SongPosition = position,
				BuffId = 199
			});
			position++;
			bardSongs.Add(new SongData
			{
				SongType = "Madrigal",
				SongName = "Blade Madrigal",
				SongPosition = position,
				BuffId = 199
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Prelude",
				SongName = "Hunter's Prelude",
				SongPosition = position,
				BuffId = 200
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Prelude",
				SongName = "Archer's Prelude",
				SongPosition = position,
				BuffId = 200
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Sinewy Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Dextrous Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Vivacious Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Quick Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Learned Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Spirited Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Enchanting Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Herculean Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Uncanny Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Vital Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Swift Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Sage Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Logical Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Etude",
				SongName = "Bewitching Etude",
				SongPosition = position,
				BuffId = 215
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Mambo",
				SongName = "Sheepfoe Mambo",
				SongPosition = position,
				BuffId = 201
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Mambo",
				SongName = "Dragonfoe Mambo",
				SongPosition = position,
				BuffId = 201
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Ballad",
				SongName = "Mage's Ballad",
				SongPosition = position,
				BuffId = 196
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Ballad",
				SongName = "Mage's Ballad II",
				SongPosition = position,
				BuffId = 196
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Ballad",
				SongName = "Mage's Ballad III",
				SongPosition = position,
				BuffId = 196
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "March",
				SongName = "Advancing March",
				SongPosition = position,
				BuffId = 214
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "March",
				SongName = "Victory March",
				SongPosition = position,
				BuffId = 214
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "March",
				SongName = "Honor March",
				SongPosition = position,
				BuffId = 214
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Fire Carol",
				SongPosition = position
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Fire Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Ice Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Ice Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = " Wind Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Wind Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Earth Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Earth Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Lightning Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Lightning Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Water Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Water Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Light Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Light Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Dark Carol",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Carol",
				SongName = "Dark Carol II",
				SongPosition = position,
				BuffId = 216
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Hymnus",
				SongName = "Godess's Hymnus",
				SongPosition = position,
				BuffId = 218
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Blank",
				SongName = "Blank",
				SongPosition = position,
				BuffId = 0
			});
			position++;

			bardSongs.Add(new SongData
			{
				SongType = "Scherzo",
				SongName = "Sentinel's Scherzo",
				SongPosition = position,
				BuffId = 222
			});
			position++;

			var geo_position = 0;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Voidance",
				GeoSpell = "Geo-Voidance",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Precision",
				GeoSpell = "Geo-Precision",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Regen",
				GeoSpell = "Geo-Regen",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Haste",
				GeoSpell = "Geo-Haste",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Attunement",
				GeoSpell = "Geo-Attunement",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Focus",
				GeoSpell = "Geo-Focus",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Barrier",
				GeoSpell = "Geo-Barrier",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Refresh",
				GeoSpell = "Geo-Refresh",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-CHR",
				GeoSpell = "Geo-CHR",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-MND",
				GeoSpell = "Geo-MND",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Fury",
				GeoSpell = "Geo-Fury",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-INT",
				GeoSpell = "Geo-INT",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-AGI",
				GeoSpell = "Geo-AGI",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Fend",
				GeoSpell = "Geo-Fend",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-VIT",
				GeoSpell = "Geo-VIT",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-DEX",
				GeoSpell = "Geo-DEX",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Acumen",
				GeoSpell = "Geo-Acumen",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-STR",
				GeoSpell = "Geo-STR",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Poison",
				GeoSpell = "Geo-Poison",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Slow",
				GeoSpell = "Geo-Slow",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Torpor",
				GeoSpell = "Geo-Torpor",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Slip",
				GeoSpell = "Geo-Slip",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Languor",
				GeoSpell = "Geo-Languor",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Paralysis",
				GeoSpell = "Geo-Paralysis",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Vex",
				GeoSpell = "Geo-Vex",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Frailty",
				GeoSpell = "Geo-Frailty",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Wilt",
				GeoSpell = "Geo-Wilt",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Malaise",
				GeoSpell = "Geo-Malaise",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Gravity",
				GeoSpell = "Geo-Gravity",
				Position = geo_position,
			});
			geo_position++;

			geoSpells.Add(new GeoData
			{
				IndiSpell = "Indi-Fade",
				GeoSpell = "Geo-Fade",
				Position = geo_position,
			});
			geo_position++;

			barSpells.Add(new SpellsData
			{
				Name = "Barfire",
				Type = 1,
				Position = 0,
				BuffId = Buffs.Barfire,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barfira",
				Type = 1,
				Position = 0,
				BuffId = Buffs.Barfire,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barstone",
				Type = 1,
				Position = 1,
				BuffId = Buffs.Barstone,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barstonra",
				Type = 1,
				Position = 1,
				BuffId = Buffs.Barstone,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barwater",
				Type = 1,
				Position = 2,
				BuffId = Buffs.Barwater,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barwatera",
				Type = 1,
				Position = 2,
				BuffId = Buffs.Barwater,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Baraero",
				Type = 1,
				Position = 3,
				BuffId = Buffs.Baraero
			});
			barSpells.Add(new SpellsData
			{
				Name = "Baraera",
				Type = 1,
				Position = 3,
				BuffId = Buffs.Baraero,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barblizzard",
				Type = 1,
				Position = 4,
				BuffId = Buffs.Barblind
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barblizzara",
				Type = 1,
				Position = 4,
				BuffId = Buffs.Barblizzard,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barthunder",
				Type = 1,
				Position = 5,
				BuffId = Buffs.Barthunder
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barthundra",
				Type = 1,
				Position = 5,
				BuffId = Buffs.Barthunder,
				AoeVersion = true,
			});

			barSpells.Add(new SpellsData
			{
				Name = "Baramnesia",
				Type = 2,
				Position = 0,
				BuffId = Buffs.Baramnesia,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Baramnesra",
				Type = 2,
				Position = 0,
				BuffId = Buffs.Baramnesia,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barvirus",
				Type = 2,
				Position = 1,
				BuffId = Buffs.Barvirus
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barvira",
				Type = 2,
				Position = 1,
				BuffId = Buffs.Barvirus,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barparalyze",
				Type = 2,
				Position = 2,
				BuffId = Buffs.Barparalyze
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barparalyzra",
				Type = 2,
				Position = 2,
				BuffId = Buffs.Barparalyze,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barsilence",
				Type = 2,
				Position = 3,
				BuffId = Buffs.Barsilence
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barsilencera",
				Type = 2,
				Position = 3,
				BuffId = Buffs.Barsilence,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barpetrify",
				Type = 2,
				Position = 4,
				BuffId = Buffs.Barpetrify
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barpetra",
				Type = 2,
				Position = 4,
				BuffId = Buffs.Barpetrify,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barpoison",
				Type = 2,
				Position = 5,
				BuffId = Buffs.Barpoison
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barpoisonra",
				Type = 2,
				Position = 5,
				BuffId = Buffs.Barpoison,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barblind",
				Type = 2,
				Position = 6,
				BuffId = Buffs.Barblind
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barblindra",
				Type = 2,
				Position = 6,
				BuffId = Buffs.Barblind,
				AoeVersion = true,
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barsleep",
				Type = 2,
				Position = 7,
				BuffId = Buffs.Barsleep
			});
			barSpells.Add(new SpellsData
			{
				Name = "Barsleepra",
				Type = 2,
				Position = 7,
				BuffId = Buffs.Barsleep,
				AoeVersion = true,
			});

			enSpells.Add(new SpellsData
			{
				Name = "Enfire",
				Type = 1,
				Position = 0,
				BuffId = Buffs.Enfire
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enstone",
				Type = 1,
				Position = 1,
				BuffId = Buffs.Enstone
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enwater",
				Type = 1,
				Position = 2,
				BuffId = Buffs.Enwater
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enaero",
				Type = 1,
				Position = 3,
				BuffId = Buffs.Enaero
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enblizzard",
				Type = 1,
				Position = 4,
				BuffId = Buffs.Enblizzard
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enthunder",
				Type = 1,
				Position = 5,
				BuffId = Buffs.Enthunder
			});

			enSpells.Add(new SpellsData
			{
				Name = "Enfire II",
				Type = 1,
				Position = 6,
				BuffId = Buffs.EnfireII
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enstone II",
				Type = 1,
				Position = 7,
				BuffId = Buffs.EnfireII
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enwater II",
				Type = 1,
				Position = 8,
				BuffId = Buffs.EnwaterII
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enaero II",
				Type = 1,
				Position = 9,
				BuffId = Buffs.EnaeroII
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enblizzard II",
				Type = 1,
				Position = 10,
				BuffId = Buffs.EnblizzardII
			});
			enSpells.Add(new SpellsData
			{
				Name = "Enthunder II",
				Type = 1,
				Position = 11,
				BuffId = Buffs.EnthunderII
			});

			stormSpells.Add(new SpellsData
			{
				Name = "Firestorm",
				Type = 1,
				Position = 0,
				BuffId = Buffs.Firestorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Sandstorm",
				Type = 1,
				Position = 1,
				BuffId = Buffs.Sandstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Rainstorm",
				Type = 1,
				Position = 2,
				BuffId = Buffs.Rainstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Windstorm",
				Type = 1,
				Position = 3,
				BuffId = Buffs.Windstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Hailstorm",
				Type = 1,
				Position = 4,
				BuffId = Buffs.Hailstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Thunderstorm",
				Type = 1,
				Position = 5,
				BuffId = Buffs.Thunderstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Voidstorm",
				Type = 1,
				Position = 6,
				BuffId = Buffs.Voidstorm
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Aurorastorm",
				Type = 1,
				Position = 7,
				BuffId = Buffs.Aurorastorm
			});

			stormSpells.Add(new SpellsData
			{
				Name = "Firestorm II",
				Type = 1,
				Position = 8,
				BuffId = Buffs.Firestorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Sandstorm II",
				Type = 1,
				Position = 9,
				BuffId = Buffs.Sandstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Rainstorm II",
				Type = 1,
				Position = 10,
				BuffId = Buffs.Rainstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Windstorm II",
				Type = 1,
				Position = 11,
				BuffId = Buffs.Windstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Hailstorm II",
				Type = 1,
				Position = 12,
				BuffId = Buffs.Hailstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Thunderstorm II",
				Type = 1,
				Position = 13,
				BuffId = Buffs.Thunderstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Voidstorm II",
				Type = 1,
				Position = 14,
				BuffId = Buffs.Voidstorm2
			});
			stormSpells.Add(new SpellsData
			{
				Name = "Aurorastorm II",
				Type = 1,
				Position = 15,
				BuffId = Buffs.Aurorastorm2
			});

			var pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

			if (pol.Count() < 1)
			{
				MessageBox.Show("FFXI not found");
			}
			else
			{
				for (var i = 0; i < pol.Count(); i++)
				{
					POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
					POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
					processIds.Items.Add(pol.ElementAt(i).Id);
					activeProcessIds.Items.Add(pol.ElementAt(i).Id);
				}
				POLID.SelectedIndex = 0;
				POLID2.SelectedIndex = 0;
				processIds.SelectedIndex = 0;
				activeProcessIds.SelectedIndex = 0;
			}
			// Show the current version number..
			Text = notifyIcon1.Text = "Cure Please v" + Application.ProductVersion;
			notifyIcon1.BalloonTipTitle = "Cure Please v" + Application.ProductVersion;
			notifyIcon1.BalloonTipText = "CurePlease has been minimized.";
			notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
			StartActionLoop();
		}

		private void PaintBorderlessGroupBox(object sender, PaintEventArgs e)
		{
			var box = sender as GroupBox;
			DrawGroupBox(box, e.Graphics, Color.Black, Color.Gray);
		}

		private void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor)
		{
			if (box != null)
			{
				Brush textBrush = new SolidBrush(textColor);
				Brush borderBrush = new SolidBrush(borderColor);
				var borderPen = new Pen(borderBrush);
				var strSize = g.MeasureString(box.Text, box.Font);
				var rect = new Rectangle(box.ClientRectangle.X,
																	 box.ClientRectangle.Y + (int)(strSize.Height / 2),
																	 box.ClientRectangle.Width - 1,
																	 box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

				// Clear text and border
				g.Clear(BackColor);

				// Draw text
				g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

				// Drawing Border
				//Left
				g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
				//Right
				g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
				//Bottom
				g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
				//Top1
				g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
				//Top2
				g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));
			}
		}

		private void PaintButton(object sender, PaintEventArgs e)
		{
			var button = sender as Button;

			button.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
		}

		private void setinstance_Click(object sender, EventArgs e)
		{
			if (!CheckForDLLFiles())
			{
				MessageBox.Show(
						"Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
						"Error");
				return;
			}

			processIds.SelectedIndex = POLID.SelectedIndex;
			activeProcessIds.SelectedIndex = POLID.SelectedIndex;
			instancePrimary = new EliteAPI((int)processIds.SelectedItem);
			plLabel.Text = "Selected PL: " + instancePrimary.Player.Name;
			Text = notifyIcon1.Text = instancePrimary.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;

			plLabel.ForeColor = Color.Green;
			POLID.BackColor = Color.White;
			plPosition.Enabled = true;
			setinstance2.Enabled = true;
			Form2.config.autoFollowName = string.Empty;

			forceSongRecast = true;

			foreach (var dats in Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi")).Where(dats => POLID.Text == dats.MainWindowTitle))
			{
				for (var i = 0; i < dats.Modules.Count; i++)
				{
					if (dats.Modules[i].FileName.Contains("Ashita.dll"))
					{
						hookMode = "Ashita";
					}
					else if (dats.Modules[i].FileName.Contains("Hook.dll"))
					{
						hookMode = "Windower";
					}
				}
			}

			if (firstTimePause == 0)
			{
				Follow_BGW.RunWorkerAsync();
				AddonReader.RunWorkerAsync();
				firstTimePause = 1;
			}

			// LOAD AUTOMATIC SETTINGS
			var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings");
			if (true || System.IO.File.Exists(path + "/loadSettings"))
			{
				if (instancePrimary.Player.MainJob != 0)
				{
					if (instancePrimary.Player.SubJob != 0)
					{
						var mainJob = Jobs.Instance.Single(c => c.Number == instancePrimary.Player.MainJob);
						var subJob = Jobs.Instance.Single(c => c.Number == instancePrimary.Player.SubJob);

						var player = instancePrimary.Player.Name;
						var main = mainJob.Name;
						var sub = subJob.Name;

						var f1 = Path.Combine(path, $"{player}_{main}_{sub}.xml");
						var f2 = Path.Combine(path, $"{player}_{main}.xml");
						var f3 = Path.Combine(path, $"{main}_{sub}.xml");
						var f4 = Path.Combine(path, $"{main}.xml");
						var target = "default.xml";

						if (File.Exists(f1))
						{
							target = f1;
						}
						else if (File.Exists(f2))
						{
							target = f2;
						}
						else if (File.Exists(f3))
						{
							target = f3;
						}
						else if (File.Exists(f4))
						{
							target = f4;
						}

						if (File.Exists(target))
						{
							var config = new Form2.MySettings();
							var mySerializer = new XmlSerializer(typeof(Form2.MySettings));

							using (var reader = new StreamReader(target))
							{
								config = (Form2.MySettings)mySerializer.Deserialize(reader);
							}

							form2.updateForm(config);
							form2.button4_Click(sender, e);
						}
					}
				}
			}

			if (isAddonLoaded == 0 && !Form2.config.pauseOnStartBox && instanceMonitored != null)
			{
				// Wait a milisecond and then load and set the config.
				Thread.Sleep(500);

				if (hookMode == "Windower")
				{
					LoadAddonWindower();
				}
				else if (hookMode == "Ashita")
				{
					LoadAddonAshita();
				}

				AddOnStatus_Click(sender, e);


				currentAction.Text = "LUA Addon loaded. ( " + endpoint.Address + " - " + endpoint.Port + " )";

				isAddonLoaded = 1;
			}
		}

		private void setinstance2_Click(object sender, EventArgs e)
		{
			if (!CheckForDLLFiles())
			{
				MessageBox.Show(
						"Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
						"Error");
				return;
			}
			processIds.SelectedIndex = POLID2.SelectedIndex;
			instanceMonitored = new EliteAPI((int)processIds.SelectedItem);
			monitoredLabel.Text = "Monitoring: " + instanceMonitored.Player.Name;
			monitoredLabel.ForeColor = Color.Green;
			POLID2.BackColor = Color.White;
			partyMembersUpdate.Enabled = true;
			pauseButton.Enabled = true;
			hpUpdates.Enabled = true;
			enableActions = true;

			if (Form2.config.pauseOnStartBox)
			{
				pauseActions = true;
				pauseButton.Text = "Loaded, Paused!";
				pauseButton.ForeColor = Color.Red;
				enableActions = false;
			}
			else
			{
				if (Form2.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
				{
					WindowState = FormWindowState.Minimized;
				}
			}

			if (isAddonLoaded == 0 && !Form2.config.pauseOnStartBox && instancePrimary != null)
			{
				// Wait a milisecond and then load and set the config.
				Thread.Sleep(500);
				if (hookMode == "Windower")
				{
					LoadAddonWindower();
				}
				else if (hookMode == "Ashita")
				{
					LoadAddonAshita();
				}

				currentAction.Text = "LUA Addon loaded. ( " + endpoint.Address + " - " + endpoint.Port + " )";

				isAddonLoaded = 1;

				AddOnStatus_Click(sender, e);

				lastCommand = instanceMonitored.ThirdParty.ConsoleIsNewCommand();
			}
		}

		private int GetInventoryItemCount(EliteAPI api, ushort itemid)
		{
			var count = 0;
			for (var x = 0; x <= 80; x++)
			{
				var item = api.Inventory.GetContainerItem(0, x);
				if (item != null && item.Id == itemid)
				{
					count += (int)item.Count;
				}
			}

			return count;
		}

		private int GetTempItemCount(EliteAPI api, ushort itemid)
		{
			var count = 0;
			for (var x = 0; x <= 80; x++)
			{
				var item = api.Inventory.GetContainerItem(3, x);
				if (item != null && item.Id == itemid)
				{
					count += (int)item.Count;
				}
			}

			return count;
		}

		private ushort GetItemId(string name)
		{
			var item = instancePrimary.Resources.GetItem(name, 0);
			return item != null ? (ushort)item.ItemID : (ushort)0;
		}

		private int GetAbilityRecastBySpellId(int id)
		{
			var abilityIds = instancePrimary.Recast.GetAbilityIds();
			for (var x = 0; x < abilityIds.Count; x++)
			{
				if (abilityIds[x] == id)
				{
					return instancePrimary.Recast.GetAbilityRecast(x);
				}
			}

			return -1;
		}

		public static bool HasAbility(string name)
		{
			if (instancePrimary.Player.GetPlayerInfo().Buffs.Any(b => b == 261) || instancePrimary.Player.GetPlayerInfo().Buffs.Any(b => b == 16)) // IF YOU HAVE INPAIRMENT/AMNESIA THEN BLOCK JOB ABILITY CASTING
			{
				return false;
			}
			else if (instancePrimary.Player.HasAbility(instancePrimary.Resources.GetAbility(name, 0).ID))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool HasAcquiredSpell(string checked_spellName)
		{

			checked_spellName = checked_spellName.Trim().ToLower();

			if (checked_spellName == "honor march")
			{
				return true;
			}

			var magic = instancePrimary.Resources.GetSpell(checked_spellName, 0);

			if (instancePrimary.Player.GetPlayerInfo().Buffs.Any(b => b == 262)) // IF YOU HAVE OMERTA THEN BLOCK MAGIC CASTING
			{
				return false;
			}
			else if (instancePrimary.Player.HasSpell(magic.Index) && HasRequiredJobLevel(checked_spellName) == true)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool HasRequiredJobLevel(string spellName)
		{

			var checked_spellName = spellName.Trim().ToLower();

			var magic = instancePrimary.Resources.GetSpell(checked_spellName, 0); // GRAB THE REQUESTED SPELL DATA

			int mainjobLevelRequired = magic.LevelRequired[(instancePrimary.Player.MainJob)]; // GRAB SPELL LEVEL FOR THE MAIN JOB
			int subjobLevelRequired = magic.LevelRequired[(instancePrimary.Player.SubJob)]; // GRAB SPELL LEVEL FOR THE SUB JOB

			if (checked_spellName == "honor march")
			{
				return true;
			}

			if (mainjobLevelRequired <= instancePrimary.Player.MainJobLevel && mainjobLevelRequired != -1)
			{ // IF THE MAIN JOB DOES NOT EQUAl -1 (Meaning the JOB can't use the spell) AND YOUR LEVEL IS EQUAL TO OR LOVER THAN THE REQUIRED LEVEL RETURN true
				return true;
			}
			else if (subjobLevelRequired <= instancePrimary.Player.SubJobLevel && subjobLevelRequired != -1)
			{ // IF THE SUB JOB DOES NOT EQUAl -1 (Meaning the JOB can't use the spell) AND YOUR LEVEL IS EQUAL TO OR LOVER THAN THE REQUIRED LEVEL RETURN true
				return true;
			}
			else if (mainjobLevelRequired > 99 && mainjobLevelRequired != -1)
			{ // IF THE MAIN JOB LEVEL IS GREATER THAN 99 BUT DOES NOT EQUAL -1 THEN IT IS A JOB POINT REQUIRED SPELL AND SO FURTHER CHECKS MUST BE MADE SO GRAB CURRENT JOB POINT TABLE
				var JobPoints = instancePrimary.Player.GetJobPoints(instancePrimary.Player.MainJob);

				// Spell is a JP spell so check this works correctly and that you possess the spell
				if (checked_spellName == "refresh iii" || checked_spellName == "temper ii")
				{
					if (instancePrimary.Player.MainJob == 5 && instancePrimary.Player.MainJobLevel == 99 && JobPoints.SpentJobPoints >= 1200) // IF MAIN JOB IS RDM, AND JOB LEVEL IS AT MAX WITH REQUIRED JOB POINTS
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else if (checked_spellName == "distract iii" || checked_spellName == "frazzle iii")
				{
					if (instancePrimary.Player.MainJob == 5 && instancePrimary.Player.MainJobLevel == 99 && JobPoints.SpentJobPoints >= 550) // IF MAIN JOB IS RDM, AND JOB LEVEL IS AT MAX WITH REQUIRED JOB POINTS
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else if (checked_spellName.Contains("storm ii"))
				{
					if (instancePrimary.Player.MainJob == 20 && instancePrimary.Player.MainJobLevel == 99 && JobPoints.SpentJobPoints >= 100) // IF MAIN JOB IS SCH, AND JOB LEVEL IS AT MAX WITH REQUIRED JOB POINTS
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else if (checked_spellName == "reraise iv")
				{
					if (instancePrimary.Player.MainJob == 3 && instancePrimary.Player.MainJobLevel == 99 && JobPoints.SpentJobPoints >= 100) // IF MAIN JOB IS WHM, AND JOB LEVEL IS AT MAX WITH REQUIRED JOB POINTS
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else if (checked_spellName == "full cure")
				{
					if (instancePrimary.Player.MainJob == 3 && instancePrimary.Player.MainJobLevel == 99 && JobPoints.SpentJobPoints >= 1200) // IF MAIN JOB IS WHM, AND JOB LEVEL IS AT MAX WITH REQUIRED JOB POINTS
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool SpellReadyToCast(string spellName)
		{
			var trimmed = spellName.ToLower().Trim();
			if (string.IsNullOrWhiteSpace(trimmed)) return false;
			if (trimmed == "honor march") return true;
			if (trimmed == "blank") return false;

			try
			{
				var magic = instancePrimary.Resources.GetSpell(spellName, 0);
				var recast = instancePrimary.Recast.GetSpellRecast(magic.Index);
				return recast == 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool AbilityReadyToUse(string name)
		{
			int id = instancePrimary.Resources.GetAbility(name, 0).TimerID;
			var IDs = instancePrimary.Recast.GetAbilityIds();
			for (var x = 0; x < IDs.Count; x++)
			{
				if (IDs[x] == id)
				{
					return instancePrimary.Recast.GetAbilityRecast(x) == 0;
				}
			}

			return false;
		}

		public bool IsStandingStill()
		{
			var elapsed = DateTime.Now.Subtract(lastTimePrimaryMoved);
			return elapsed.TotalMilliseconds > 1000;
		}

		public bool CanCastSpell(string spellName)
		{
			return
				IsStandingStill() &&
				HasRequiredJobLevel(spellName) &&
				HasAcquiredSpell(spellName) &&
				SpellReadyToCast(spellName);
		}

		public bool CanUseJobAbility(string name)
		{
			return
				IsStandingStill() &&
				HasAbility(name) &&
				AbilityReadyToUse(name);
		}

		private bool CheckForDLLFiles()
		{
			if (!File.Exists("eliteapi.dll") || !File.Exists("elitemmo.api.dll"))
			{
				try
				{
					var wc = new WebClient();
					wc.DownloadFile("http://ext.elitemmonetwork.com/downloads/eliteapi/EliteAPI.dll", "EliteAPI.dll");
					wc.DownloadFile("http://ext.elitemmonetwork.com/downloads/elitemmo_api/EliteMMO.API.dll", "EliteMMO.API.dll");
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
		}

		private string CureTiers(string cureSpell, bool priority)
		{
			if (cureSpell.ToLower() == "cure vi")
			{
				if (CanCastSpell("Cure VI"))
				{
					return "Cure VI";
				}
				else if (CanCastSpell("Cure V") && Form2.config.Undercure)
				{
					return "Cure V";
				}
				else if (CanCastSpell("Cure IV") && Form2.config.Undercure)
				{
					return "Cure IV";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "cure v")
			{
				if (CanCastSpell("Cure V"))
				{
					return "Cure V";
				}
				else if (CanCastSpell("Cure IV") && Form2.config.Undercure)
				{
					return "Cure IV";
				}
				else if (CanCastSpell("Cure VI") && (Form2.config.Overcure && !Form2.config.OvercureOnHighPriority || Form2.config.OvercureOnHighPriority && priority))
				{
					return "Cure VI";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "cure iv")
			{
				if (CanCastSpell("Cure IV"))
				{
					return "Cure IV";
				}
				else if (CanCastSpell("Cure III") && Form2.config.Undercure)
				{
					return "Cure III";
				}
				else if (CanCastSpell("Cure V") && (Form2.config.Overcure && Form2.config.OvercureOnHighPriority != true || Form2.config.OvercureOnHighPriority && priority == true))
				{
					return "Cure V";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "cure iii")
			{
				if (CanCastSpell("Cure III"))
				{
					return "Cure III";
				}
				else if (CanCastSpell("Cure IV") && (Form2.config.Overcure && Form2.config.OvercureOnHighPriority != true || Form2.config.OvercureOnHighPriority && priority == true))
				{
					return "Cure IV";
				}
				else if (CanCastSpell("Cure II") && Form2.config.Undercure)
				{
					return "Cure II";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "cure ii")
			{
				if (CanCastSpell("Cure II"))
				{
					return "Cure II";
				}
				else if (CanCastSpell("Cure") && Form2.config.Undercure)
				{
					return "Cure";
				}
				else if (CanCastSpell("Cure III") && (Form2.config.Overcure && Form2.config.OvercureOnHighPriority != true || Form2.config.OvercureOnHighPriority && priority == true))
				{
					return "Cure III";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "cure")
			{
				if (CanCastSpell("Cure"))
				{
					return "Cure";
				}
				else if (CanCastSpell("Cure II") && (Form2.config.Overcure && Form2.config.OvercureOnHighPriority != true || Form2.config.OvercureOnHighPriority && priority == true))
				{
					return "Cure II";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "curaga v")
			{
				if (CanCastSpell("Curaga V"))
				{
					return "Curaga V";
				}
				else if (CanCastSpell("Curaga IV") && Form2.config.Undercure)
				{
					return "Curaga IV";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "curaga iv")
			{
				if (CanCastSpell("Curaga IV"))
				{
					return "Curaga IV";
				}
				else if (CanCastSpell("Curaga V") && Form2.config.Overcure)
				{
					return "Curaga V";
				}
				else if (CanCastSpell("Curaga III") && Form2.config.Undercure)
				{
					return "Curaga III";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "curaga iii")
			{
				if (CanCastSpell("Curaga III"))
				{
					return "Curaga III";
				}
				else if (CanCastSpell("Curaga IV") && Form2.config.Overcure)
				{
					return "Curaga IV";
				}
				else if (CanCastSpell("Curaga II") && Form2.config.Undercure)
				{
					return "Curaga II";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "curaga ii")
			{
				if (CanCastSpell("Curaga II"))
				{
					return "Curaga II";
				}
				else if (CanCastSpell("Curaga") && Form2.config.Undercure)
				{
					return "Curaga";
				}
				else if (CanCastSpell("Curaga III") && Form2.config.Overcure)
				{
					return "Curaga III";
				}
				else
				{
					return "false";
				}
			}
			else if (cureSpell.ToLower() == "curaga")
			{
				if (CanCastSpell("Curaga"))
				{
					return "Curaga";
				}
				else if (CanCastSpell("Curaga II") && Form2.config.Overcure)
				{
					return "Curaga II";
				}
				else
				{
					return "false";
				}
			}
			return "false";
		}

		private bool partyMemberUpdateMethod(byte partyMemberId)
		{
			var member = instanceMonitored.Party.GetPartyMembers()[partyMemberId];
			var inSameZone = instancePrimary.Player.ZoneId == member.Zone;

			if (member.Active >= 1 && inSameZone)
			{
				var entity = instancePrimary.Entity.GetEntity((int)member.TargetIndex);
				return entity.Distance >= 0 && entity.Distance < 21;
			}

			return false;
		}

		private async void partyMembersUpdate_TickAsync(object sender, EventArgs e)
		{
			if (instancePrimary == null || instanceMonitored == null)
			{
				return;
			}

			if (instancePrimary.Player.LoginStatus == (int)LoginStatus.Loading || instanceMonitored.Player.LoginStatus == (int)LoginStatus.Loading)
			{
				if (Form2.config.pauseOnZoneBox == true)
				{
					song_casting = 0;
					forceSongRecast = true;
					if (pauseActions != true)
					{
						pauseButton.Text = "Zoned, paused.";
						pauseButton.ForeColor = Color.Red;
						pauseActions = true;
						enableActions = false;
					}
				}
				else
				{
					song_casting = 0;
					forceSongRecast = true;

					if (pauseActions != true)
					{
						pauseButton.Text = "Zoned, waiting.";
						pauseButton.ForeColor = Color.Red;
						await Task.Delay(100);
						Thread.Sleep(17000);
						pauseButton.Text = "Pause";
						pauseButton.ForeColor = Color.Black;
					}
				}
				activeBuffs.Clear();
			}

			if (instancePrimary.Player.LoginStatus != (int)LoginStatus.LoggedIn || instanceMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
			{
				return;
			}
			if (partyMemberUpdateMethod(0))
			{
				player0.Text = instanceMonitored.Party.GetPartyMember(0).Name;
				player0.Enabled = true;
				player0optionsButton.Enabled = true;
				player0buffsButton.Enabled = true;
			}
			else
			{
				player0.Text = "Inactive or out of zone";
				player0.Enabled = false;
				player0HP.Value = 0;
				player0optionsButton.Enabled = false;
				player0buffsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(1))
			{
				player1.Text = instanceMonitored.Party.GetPartyMember(1).Name;
				player1.Enabled = true;
				player1optionsButton.Enabled = true;
				player1buffsButton.Enabled = true;
			}
			else
			{
				player1.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player1.Enabled = false;
				player1HP.Value = 0;
				player1optionsButton.Enabled = false;
				player1buffsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(2))
			{
				player2.Text = instanceMonitored.Party.GetPartyMember(2).Name;
				player2.Enabled = true;
				player2optionsButton.Enabled = true;
				player2buffsButton.Enabled = true;
			}
			else
			{
				player2.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player2.Enabled = false;
				player2HP.Value = 0;
				player2optionsButton.Enabled = false;
				player2buffsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(3))
			{
				player3.Text = instanceMonitored.Party.GetPartyMember(3).Name;
				player3.Enabled = true;
				player3optionsButton.Enabled = true;
				player3buffsButton.Enabled = true;
			}
			else
			{
				player3.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player3.Enabled = false;
				player3HP.Value = 0;
				player3optionsButton.Enabled = false;
				player3buffsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(4))
			{
				player4.Text = instanceMonitored.Party.GetPartyMember(4).Name;
				player4.Enabled = true;
				player4optionsButton.Enabled = true;
				player4buffsButton.Enabled = true;
			}
			else
			{
				player4.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player4.Enabled = false;
				player4HP.Value = 0;
				player4optionsButton.Enabled = false;
				player4buffsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(5))
			{
				player5.Text = instanceMonitored.Party.GetPartyMember(5).Name;
				player5.Enabled = true;
				player5optionsButton.Enabled = true;
				player5buffsButton.Enabled = true;
			}
			else
			{
				player5.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player5.Enabled = false;
				player5HP.Value = 0;
				player5optionsButton.Enabled = false;
				player5buffsButton.Enabled = false;
			}
			if (partyMemberUpdateMethod(6))
			{
				player6.Text = instanceMonitored.Party.GetPartyMember(6).Name;
				player6.Enabled = true;
				player6optionsButton.Enabled = true;
			}
			else
			{
				player6.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player6.Enabled = false;
				player6HP.Value = 0;
				player6optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(7))
			{
				player7.Text = instanceMonitored.Party.GetPartyMember(7).Name;
				player7.Enabled = true;
				player7optionsButton.Enabled = true;
			}
			else
			{
				player7.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player7.Enabled = false;
				player7HP.Value = 0;
				player7optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(8))
			{
				player8.Text = instanceMonitored.Party.GetPartyMember(8).Name;
				player8.Enabled = true;
				player8optionsButton.Enabled = true;
			}
			else
			{
				player8.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player8.Enabled = false;
				player8HP.Value = 0;
				player8optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(9))
			{
				player9.Text = instanceMonitored.Party.GetPartyMember(9).Name;
				player9.Enabled = true;
				player9optionsButton.Enabled = true;
			}
			else
			{
				player9.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player9.Enabled = false;
				player9HP.Value = 0;
				player9optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(10))
			{
				player10.Text = instanceMonitored.Party.GetPartyMember(10).Name;
				player10.Enabled = true;
				player10optionsButton.Enabled = true;
			}
			else
			{
				player10.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player10.Enabled = false;
				player10HP.Value = 0;
				player10optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(11))
			{
				player11.Text = instanceMonitored.Party.GetPartyMember(11).Name;
				player11.Enabled = true;
				player11optionsButton.Enabled = true;
			}
			else
			{
				player11.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player11.Enabled = false;
				player11HP.Value = 0;
				player11optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(12))
			{
				player12.Text = instanceMonitored.Party.GetPartyMember(12).Name;
				player12.Enabled = true;
				player12optionsButton.Enabled = true;
			}
			else
			{
				player12.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player12.Enabled = false;
				player12HP.Value = 0;
				player12optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(13))
			{
				player13.Text = instanceMonitored.Party.GetPartyMember(13).Name;
				player13.Enabled = true;
				player13optionsButton.Enabled = true;
			}
			else
			{
				player13.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player13.Enabled = false;
				player13HP.Value = 0;
				player13optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(14))
			{
				player14.Text = instanceMonitored.Party.GetPartyMember(14).Name;
				player14.Enabled = true;
				player14optionsButton.Enabled = true;
			}
			else
			{
				player14.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player14.Enabled = false;
				player14HP.Value = 0;
				player14optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(15))
			{
				player15.Text = instanceMonitored.Party.GetPartyMember(15).Name;
				player15.Enabled = true;
				player15optionsButton.Enabled = true;
			}
			else
			{
				player15.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player15.Enabled = false;
				player15HP.Value = 0;
				player15optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(16))
			{
				player16.Text = instanceMonitored.Party.GetPartyMember(16).Name;
				player16.Enabled = true;
				player16optionsButton.Enabled = true;
			}
			else
			{
				player16.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player16.Enabled = false;
				player16HP.Value = 0;
				player16optionsButton.Enabled = false;
			}

			if (partyMemberUpdateMethod(17))
			{
				player17.Text = instanceMonitored.Party.GetPartyMember(17).Name;
				player17.Enabled = true;
				player17optionsButton.Enabled = true;
			}
			else
			{
				player17.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
				player17.Enabled = false;
				player17HP.Value = 0;
				player17optionsButton.Enabled = false;
			}
		}

		private void hpUpdates_Tick(object sender, EventArgs e)
		{
			if (instancePrimary == null || instanceMonitored == null)
			{
				return;
			}

			if (instancePrimary.Player.LoginStatus != (int)LoginStatus.LoggedIn || instanceMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
			{
				return;
			}

			if (player0.Enabled)
			{
				UpdateHPProgressBar(player0HP, instanceMonitored.Party.GetPartyMember(0).CurrentHPP);
			}

			if (player0.Enabled)
			{
				UpdateHPProgressBar(player0HP, instanceMonitored.Party.GetPartyMember(0).CurrentHPP);
			}

			if (player1.Enabled)
			{
				UpdateHPProgressBar(player1HP, instanceMonitored.Party.GetPartyMember(1).CurrentHPP);
			}

			if (player2.Enabled)
			{
				UpdateHPProgressBar(player2HP, instanceMonitored.Party.GetPartyMember(2).CurrentHPP);
			}

			if (player3.Enabled)
			{
				UpdateHPProgressBar(player3HP, instanceMonitored.Party.GetPartyMember(3).CurrentHPP);
			}

			if (player4.Enabled)
			{
				UpdateHPProgressBar(player4HP, instanceMonitored.Party.GetPartyMember(4).CurrentHPP);
			}

			if (player5.Enabled)
			{
				UpdateHPProgressBar(player5HP, instanceMonitored.Party.GetPartyMember(5).CurrentHPP);
			}

			if (player6.Enabled)
			{
				UpdateHPProgressBar(player6HP, instanceMonitored.Party.GetPartyMember(6).CurrentHPP);
			}

			if (player7.Enabled)
			{
				UpdateHPProgressBar(player7HP, instanceMonitored.Party.GetPartyMember(7).CurrentHPP);
			}

			if (player8.Enabled)
			{
				UpdateHPProgressBar(player8HP, instanceMonitored.Party.GetPartyMember(8).CurrentHPP);
			}

			if (player9.Enabled)
			{
				UpdateHPProgressBar(player9HP, instanceMonitored.Party.GetPartyMember(9).CurrentHPP);
			}

			if (player10.Enabled)
			{
				UpdateHPProgressBar(player10HP, instanceMonitored.Party.GetPartyMember(10).CurrentHPP);
			}

			if (player11.Enabled)
			{
				UpdateHPProgressBar(player11HP, instanceMonitored.Party.GetPartyMember(11).CurrentHPP);
			}

			if (player12.Enabled)
			{
				UpdateHPProgressBar(player12HP, instanceMonitored.Party.GetPartyMember(12).CurrentHPP);
			}

			if (player13.Enabled)
			{
				UpdateHPProgressBar(player13HP, instanceMonitored.Party.GetPartyMember(13).CurrentHPP);
			}

			if (player14.Enabled)
			{
				UpdateHPProgressBar(player14HP, instanceMonitored.Party.GetPartyMember(14).CurrentHPP);
			}

			if (player15.Enabled)
			{
				UpdateHPProgressBar(player15HP, instanceMonitored.Party.GetPartyMember(15).CurrentHPP);
			}

			if (player16.Enabled)
			{
				UpdateHPProgressBar(player16HP, instanceMonitored.Party.GetPartyMember(16).CurrentHPP);
			}

			if (player17.Enabled)
			{
				UpdateHPProgressBar(player17HP, instanceMonitored.Party.GetPartyMember(17).CurrentHPP);
			}
		}

		private void UpdateHPProgressBar(ProgressBar hpProgressBar, int currentHpPercent)
		{
			hpProgressBar.Value = currentHpPercent;
			if (currentHpPercent >= 75)
			{
				hpProgressBar.ForeColor = Color.DarkGreen;
			}
			else if (currentHpPercent > 50 && currentHpPercent < 75)
			{
				hpProgressBar.ForeColor = Color.Yellow;
			}
			else if (currentHpPercent > 25 && currentHpPercent < 50)
			{
				hpProgressBar.ForeColor = Color.Orange;
			}
			else if (currentHpPercent < 25)
			{
				hpProgressBar.ForeColor = Color.Red;
			}
		}

		private void plPosition_Tick(object sender, EventArgs e)
		{
			if (instancePrimary == null || instanceMonitored == null)
			{
				return;
			}

			if (instancePrimary.Player.LoginStatus != (int)LoginStatus.LoggedIn || instanceMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
			{
				return;
			}

			var x = instancePrimary.Player.X;
			var y = instancePrimary.Player.Y;
			var z = instancePrimary.Player.Z;

			if (plX != x || plY != y || plZ != z)
			{
				plX = x; plY = y; plZ = z;
				lastTimePrimaryMoved = DateTime.Now;
			}
		}

		private void ClearDebuff(string characterName, int debuffID)
		{
			lock (activeBuffs)
			{
				foreach (var ailment in activeBuffs)
				{
					if (ailment.CharacterName.ToLower() == characterName.ToLower())
					{
						//MessageBox.Show("Found Match: " + ailment.CharacterName.ToLower()+" => "+characterName.ToLower());

						// Build a new list, find cast debuff and remove it.
						var named_Debuffs = ailment.CharacterBuffs.Split(',').ToList();
						named_Debuffs.Remove(debuffID.ToString());

						// Now rebuild the list and replace previous one
						var stringList = string.Join(",", named_Debuffs);

						var i = activeBuffs.FindIndex(x => x.CharacterName.ToLower() == characterName.ToLower());
						activeBuffs[i].CharacterBuffs = stringList;
					}
				}
			}
		}

		private async Task CureCalculator_PL(bool HP)
		{
			// FIRST GET HOW MUCH HP IS MISSING FROM THE CURRENT PARTY MEMBER
			if (instancePrimary.Player.HP > 0)
			{
				var HP_Loss = (instancePrimary.Player.HP * 100) / (instancePrimary.Player.HPP) - (instancePrimary.Player.HP);

				if (Form2.config.cure6enabled && HP_Loss >= Form2.config.cure6amount && instancePrimary.Player.MP > 227 && HasAcquiredSpell("Cure VI") && HasRequiredJobLevel("Cure VI") == true)
				{
					var cureSpell = CureTiers("Cure VI", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
				else if (Form2.config.cure5enabled && HP_Loss >= Form2.config.cure5amount && instancePrimary.Player.MP > 125 && HasAcquiredSpell("Cure V") && HasRequiredJobLevel("Cure V") == true)
				{
					var cureSpell = CureTiers("Cure V", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
				else if (Form2.config.cure4enabled && HP_Loss >= Form2.config.cure4amount && instancePrimary.Player.MP > 88 && HasAcquiredSpell("Cure IV") && HasRequiredJobLevel("Cure IV") == true)
				{
					var cureSpell = CureTiers("Cure IV", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
				else if (Form2.config.cure3enabled && HP_Loss >= Form2.config.cure3amount && instancePrimary.Player.MP > 46 && HasAcquiredSpell("Cure III") && HasRequiredJobLevel("Cure III") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure III", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
				else if (Form2.config.cure2enabled && HP_Loss >= Form2.config.cure2amount && instancePrimary.Player.MP > 24 && HasAcquiredSpell("Cure II") && HasRequiredJobLevel("Cure II") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure II", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
				else if (Form2.config.cure1enabled && HP_Loss >= Form2.config.cure1amount && instancePrimary.Player.MP > 8 && HasAcquiredSpell("Cure") && HasRequiredJobLevel("Cure") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instancePrimary.Player.Name, cureSpell);
					}
				}
			}
		}

		private async Task CureCalculator(byte partyMemberId, bool HP)
		{
			// FIRST GET HOW MUCH HP IS MISSING FROM THE CURRENT PARTY MEMBER
			if (instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP > 0)
			{
				var HP_Loss = (instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / (instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - (instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP);

				if (Form2.config.cure6enabled && HP_Loss >= Form2.config.cure6amount && instancePrimary.Player.MP > 227 && HasAcquiredSpell("Cure VI") && HasRequiredJobLevel("Cure VI") == true)
				{
					var cureSpell = CureTiers("Cure VI", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
				else if (Form2.config.cure5enabled && HP_Loss >= Form2.config.cure5amount && instancePrimary.Player.MP > 125 && HasAcquiredSpell("Cure V") && HasRequiredJobLevel("Cure V") == true)
				{
					var cureSpell = CureTiers("Cure V", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
				else if (Form2.config.cure4enabled && HP_Loss >= Form2.config.cure4amount && instancePrimary.Player.MP > 88 && HasAcquiredSpell("Cure IV") && HasRequiredJobLevel("Cure IV") == true)
				{
					var cureSpell = CureTiers("Cure IV", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
				else if (Form2.config.cure3enabled && HP_Loss >= Form2.config.cure3amount && instancePrimary.Player.MP > 46 && HasAcquiredSpell("Cure III") && HasRequiredJobLevel("Cure III") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure III", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
				else if (Form2.config.cure2enabled && HP_Loss >= Form2.config.cure2amount && instancePrimary.Player.MP > 24 && HasAcquiredSpell("Cure II") && HasRequiredJobLevel("Cure II") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure II", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
				else if (Form2.config.cure1enabled && HP_Loss >= Form2.config.cure1amount && instancePrimary.Player.MP > 8 && HasAcquiredSpell("Cure") && HasRequiredJobLevel("Cure") == true)
				{
					if (Form2.config.PrioritiseOverLowerTier == true) { await RunDebuffChecker(); }
					var cureSpell = CureTiers("Cure", HP);
					if (cureSpell != "false")
					{
						await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, cureSpell);
					}
				}
			}
		}

		private async Task RunDebuffChecker()
		{
			// PL and Monitored Player Debuff Removal Starting with PL
			if (instancePrimary.Player.Status != 33)
			{
				if (Form2.config.plSilenceItem == 0)
				{
					plSilenceitemName = "Catholicon";
				}
				else if (Form2.config.plSilenceItem == 1)
				{
					plSilenceitemName = "Echo Drops";
				}
				else if (Form2.config.plSilenceItem == 2)
				{
					plSilenceitemName = "Remedy";
				}
				else if (Form2.config.plSilenceItem == 3)
				{
					plSilenceitemName = "Remedy Ointment";
				}
				else if (Form2.config.plSilenceItem == 4)
				{
					plSilenceitemName = "Vicar's Drink";
				}

				if (Form2.config.plDoomitem == 0)
				{
					plDoomItemName = "Holy Water";
				}
				else if (Form2.config.plDoomitem == 1)
				{
					plDoomItemName = "Hallowed Water";
				}

				if (Form2.config.wakeSleepSpell == 0)
				{
					wakeSleepSpellName = "Cure";
				}
				else if (Form2.config.wakeSleepSpell == 1)
				{
					wakeSleepSpellName = "Cura";
				}
				else if (Form2.config.wakeSleepSpell == 2)
				{
					wakeSleepSpellName = "Curaga";
				}

				foreach (StatusEffect plEffect in instancePrimary.Player.Buffs)
				{
					if ((plEffect == StatusEffect.Doom) && (Form2.config.plDoom) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Cursna");
					}
					else if ((plEffect == StatusEffect.Paralysis) && (Form2.config.plParalysis) && CanCastSpell("Paralyna") && HasRequiredJobLevel("Paralyna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Paralyna");
					}
					else if ((plEffect == StatusEffect.Amnesia) && (Form2.config.plAmnesia) && CanCastSpell("Esuna") && HasRequiredJobLevel("Esuna") == true && hasBuff(0, 418))
					{
						await CastSpell(instancePrimary.Player.Name, "Esuna");
					}
					else if ((plEffect == StatusEffect.Poison) && (Form2.config.plPoison) && CanCastSpell("Poisona") && HasRequiredJobLevel("Poisona") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Poisona");
					}
					else if ((plEffect == StatusEffect.Attack_Down) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Blindness) && (Form2.config.plBlindness) && CanCastSpell("Blindna") && HasRequiredJobLevel("Blindna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Blindna");
					}
					else if ((plEffect == StatusEffect.Bind) && (Form2.config.plBind) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Weight) && (Form2.config.plWeight))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Slow) && (Form2.config.plSlow))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Curse) && (Form2.config.plCurse) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Cursna");
					}
					else if ((plEffect == StatusEffect.Curse2) && (Form2.config.plCurse2) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Cursna");
					}
					else if ((plEffect == StatusEffect.Addle) && (Form2.config.plAddle) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Bane) && (Form2.config.plBane) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Cursna");
					}
					else if ((plEffect == StatusEffect.Plague) && (Form2.config.plPlague) && CanCastSpell("Viruna") && HasRequiredJobLevel("Viruna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Viruna");
					}
					else if ((plEffect == StatusEffect.Disease) && (Form2.config.plDisease) && CanCastSpell("Viruna") && HasRequiredJobLevel("Viruna") == true)
					{
						await CastSpell(instancePrimary.Player.Name, "Viruna");
					}
					else if ((plEffect == StatusEffect.Burn) && (Form2.config.plBurn) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Frost) && (Form2.config.plFrost) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Choke) && (Form2.config.plChoke) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Rasp) && (Form2.config.plRasp) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Shock) && (Form2.config.plShock) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Drown) && (Form2.config.plDrown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Dia) && (Form2.config.plDia) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Bio) && (Form2.config.plBio) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.STR_Down) && (Form2.config.plStrDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.DEX_Down) && (Form2.config.plDexDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.VIT_Down) && (Form2.config.plVitDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.AGI_Down) && (Form2.config.plAgiDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.INT_Down) && (Form2.config.plIntDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.MND_Down) && (Form2.config.plMndDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.CHR_Down) && (Form2.config.plChrDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Max_HP_Down) && (Form2.config.plMaxHpDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Max_MP_Down) && (Form2.config.plMaxMpDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Accuracy_Down) && (Form2.config.plAccuracyDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Evasion_Down) && (Form2.config.plEvasionDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Defense_Down) && (Form2.config.plDefenseDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Flash) && (Form2.config.plFlash) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Magic_Acc_Down) && (Form2.config.plMagicAccDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Magic_Atk_Down) && (Form2.config.plMagicAtkDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Helix) && (Form2.config.plHelix) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Max_TP_Down) && (Form2.config.plMaxTpDown) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Requiem) && (Form2.config.plRequiem) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Elegy) && (Form2.config.plElegy) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
					else if ((plEffect == StatusEffect.Threnody) && (Form2.config.plThrenody) && (Form2.config.plAttackDown))
					{
						await CastSpell(instancePrimary.Player.Name, "Erase");
					}
				}
			}

			// Next, we check monitored player
			if ((instancePrimary.Entity.GetEntity((int)instanceMonitored.Party.GetPartyMember(0).TargetIndex).Distance < 21) && (instancePrimary.Entity.GetEntity((int)instanceMonitored.Party.GetPartyMember(0).TargetIndex).Distance > 0) && (instanceMonitored.Player.HP > 0) && instancePrimary.Player.Status != 33)
			{
				foreach (StatusEffect monitoredEffect in instanceMonitored.Player.Buffs)
				{
					if ((monitoredEffect == StatusEffect.Doom) && (Form2.config.monitoredDoom) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Cursna");
					}
					else if ((monitoredEffect == StatusEffect.Sleep) && (Form2.config.monitoredSleep) && (Form2.config.wakeSleepEnabled))
					{
						await CastSpell(instanceMonitored.Player.Name, wakeSleepSpellName);
					}
					else if ((monitoredEffect == StatusEffect.Sleep2) && (Form2.config.monitoredSleep2) && (Form2.config.wakeSleepEnabled))
					{
						await CastSpell(instanceMonitored.Player.Name, wakeSleepSpellName);
					}
					else if ((monitoredEffect == StatusEffect.Silence) && (Form2.config.monitoredSilence) && CanCastSpell("Silena") && HasRequiredJobLevel("Silena") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Silena");
					}
					else if ((monitoredEffect == StatusEffect.Petrification) && (Form2.config.monitoredPetrification) && CanCastSpell("Stona") && HasRequiredJobLevel("Stona") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Stona");
					}
					else if ((monitoredEffect == StatusEffect.Paralysis) && (Form2.config.monitoredParalysis) && CanCastSpell("Paralyna") && HasRequiredJobLevel("Paralyna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Paralyna");
					}
					else if ((monitoredEffect == StatusEffect.Amnesia) && (Form2.config.monitoredAmnesia) && CanCastSpell("Esuna") && HasRequiredJobLevel("Esuna") == true && hasBuff(0, 418))
					{
						await CastSpell(instanceMonitored.Player.Name, "Esuna");
					}
					else if ((monitoredEffect == StatusEffect.Poison) && (Form2.config.monitoredPoison) && CanCastSpell("Poisona") && HasRequiredJobLevel("Erase") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Poisona");
					}
					else if ((monitoredEffect == StatusEffect.Attack_Down) && (Form2.config.monitoredAttackDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Blindness) && (Form2.config.monitoredBlindness) && CanCastSpell("Blindna") && HasRequiredJobLevel("Blindna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Blindna");
					}
					else if ((monitoredEffect == StatusEffect.Bind) && (Form2.config.monitoredBind) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Weight) && (Form2.config.monitoredWeight) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Slow) && (Form2.config.monitoredSlow) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Curse) && (Form2.config.monitoredCurse) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Cursna");
					}
					else if ((monitoredEffect == StatusEffect.Curse2) && (Form2.config.monitoredCurse2) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Cursna");
					}
					else if ((monitoredEffect == StatusEffect.Addle) && (Form2.config.monitoredAddle) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Bane) && (Form2.config.monitoredBane) && CanCastSpell("Cursna") && HasRequiredJobLevel("Cursna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Cursna");
					}
					else if ((monitoredEffect == StatusEffect.Plague) && (Form2.config.monitoredPlague) && CanCastSpell("Viruna") && HasRequiredJobLevel("Viruna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Viruna");
					}
					else if ((monitoredEffect == StatusEffect.Disease) && (Form2.config.monitoredDisease) && CanCastSpell("Viruna") && HasRequiredJobLevel("Viruna") == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Viruna");
					}
					else if ((monitoredEffect == StatusEffect.Burn) && (Form2.config.monitoredBurn) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Frost) && (Form2.config.monitoredFrost) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Choke) && (Form2.config.monitoredChoke) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Rasp) && (Form2.config.monitoredRasp) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Shock) && (Form2.config.monitoredShock) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Drown) && (Form2.config.monitoredDrown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Dia) && (Form2.config.monitoredDia) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Bio) && (Form2.config.monitoredBio) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.STR_Down) && (Form2.config.monitoredStrDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.DEX_Down) && (Form2.config.monitoredDexDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.VIT_Down) && (Form2.config.monitoredVitDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.AGI_Down) && (Form2.config.monitoredAgiDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.INT_Down) && (Form2.config.monitoredIntDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.MND_Down) && (Form2.config.monitoredMndDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.CHR_Down) && (Form2.config.monitoredChrDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Max_HP_Down) && (Form2.config.monitoredMaxHpDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Max_MP_Down) && (Form2.config.monitoredMaxMpDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Accuracy_Down) && (Form2.config.monitoredAccuracyDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Evasion_Down) && (Form2.config.monitoredEvasionDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Defense_Down) && (Form2.config.monitoredDefenseDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Flash) && (Form2.config.monitoredFlash) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Magic_Acc_Down) && (Form2.config.monitoredMagicAccDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Magic_Atk_Down) && (Form2.config.monitoredMagicAtkDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Helix) && (Form2.config.monitoredHelix) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Max_TP_Down) && (Form2.config.monitoredMaxTpDown) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Requiem) && (Form2.config.monitoredRequiem) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Elegy) && (Form2.config.monitoredElegy) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
					else if ((monitoredEffect == StatusEffect.Threnody) && (Form2.config.monitoredThrenody) && plMonitoredSameParty() == true)
					{
						await CastSpell(instanceMonitored.Player.Name, "Erase");
					}
				}
			}

			if (Form2.config.EnableAddOn)
			{
				var partyMembers = instancePrimary.Party.GetPartyMembers();
				var activeBuffList = activeBuffs.ToList();

				foreach (var buff in activeBuffList)
				{
					foreach (var member in partyMembers)
					{
						var lowerMemberName = member?.Name?.ToLower();
						var lowerBuffCharName = buff?.CharacterName?.ToLower();
						if (lowerBuffCharName != lowerMemberName)
						{
							continue;
						}

						var memberBuffs = buff?.CharacterBuffs?
							.Split(',').Select(x => x.Trim()).ToList();

						if (memberBuffs == null)
						{
							continue;
						}

						Debug.WriteLine($"Resetting missing debuff timers for {lowerMemberName}");

						if (!HasDebuff(memberBuffs, Buffs.slow) &&
								!HasDebuff(memberBuffs, Buffs.slow2) &&
								!HasDebuff(memberBuffs, Buffs.Haste) &&
								!HasDebuff(memberBuffs, Buffs.Haste2) &&
								!HasDebuff(memberBuffs, Buffs.Flurry) &&
								!HasDebuff(memberBuffs, Buffs.Flurry2))
						{
							playerHaste[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
							playerHaste_II[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
							playerFlurry[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
							playerFlurry_II[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.SublimationActivated) &&
								!HasDebuff(memberBuffs, Buffs.SublimationComplete) &&
								!HasDebuff(memberBuffs, Buffs.Refresh) &&
								!HasDebuff(memberBuffs, Buffs.Refresh2))
						{
							playerRefresh[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.Regen) &&
								!HasDebuff(memberBuffs, Buffs.Regen2))
						{
							playerRegen[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.Protect))
						{
							playerProtect[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.Shell))
						{
							playerShell[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.Phalanx))
						{
							playerPhalanx_II[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						if (!HasDebuff(memberBuffs, Buffs.Firestorm) &&
								!HasDebuff(memberBuffs, Buffs.Sandstorm) &&
								!HasDebuff(memberBuffs, Buffs.Rainstorm) &&
								!HasDebuff(memberBuffs, Buffs.Windstorm) &&
								!HasDebuff(memberBuffs, Buffs.Hailstorm) &&
								!HasDebuff(memberBuffs, Buffs.Thunderstorm) &&
								!HasDebuff(memberBuffs, Buffs.Aurorastorm) &&
								!HasDebuff(memberBuffs, Buffs.Voidstorm) &&
								!HasDebuff(memberBuffs, Buffs.Firestorm2) &&
								!HasDebuff(memberBuffs, Buffs.Sandstorm2) &&
								!HasDebuff(memberBuffs, Buffs.Rainstorm2) &&
								!HasDebuff(memberBuffs, Buffs.Windstorm2) &&
								!HasDebuff(memberBuffs, Buffs.Hailstorm2) &&
								!HasDebuff(memberBuffs, Buffs.Thunderstorm2) &&
								!HasDebuff(memberBuffs, Buffs.Aurorastorm2) &&
								!HasDebuff(memberBuffs, Buffs.Voidstorm2))
						{
							playerStormspell[member.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
						}

						Debug.WriteLine($"Removing debuffs from {lowerMemberName}");

						if (Form2.config.enablePartyDebuffRemoval &&
						(
							!Form2.config.SpecifiednaSpellsenable ||
							characterNames_naRemoval.Contains(lowerMemberName)
						))
						{
							if (
								CanCastSpell("Cursna") &&
								Form2.config.naCurse &&
								HasDebuff(memberBuffs, Buffs.Doom))
							{
								await CastSpell(member.Name, "Cursna");
								break;
							}
							else if (
								CanCastSpell("Cursna") &&
								Form2.config.naCurse &&
								HasDebuff(memberBuffs, Buffs.curse))
							{
								await CastSpell(member.Name, "Cursna");
								ClearDebuff(member.Name, Buffs.curse);
								break;
							}
							else if (
								CanCastSpell(wakeSleepSpellName) &&
								HasDebuff(memberBuffs, Buffs.Sleep))
							{
								await CastSpell(member.Name, wakeSleepSpellName);
								ClearDebuff(member.Name, Buffs.Sleep);
								break;
							}
							else if (
								CanCastSpell("Stona") &&
								Form2.config.naPetrification &&
								HasDebuff(memberBuffs, Buffs.Petrification))
							{
								await CastSpell(member.Name, "Stona");
								ClearDebuff(member.Name, Buffs.Petrification);
								break;
							}
							else if (
								CanCastSpell("Silena") &&
								Form2.config.naSilence &&
								HasDebuff(memberBuffs, Buffs.Silence))
							{
								await CastSpell(member.Name, "Silena");
								ClearDebuff(member.Name, Buffs.Silence);
								break;
							}
							else if (
								CanCastSpell("Paralyna") &&
								Form2.config.naParalysis &&
								HasDebuff(memberBuffs, Buffs.Paralysis))
							{
								await CastSpell(member.Name, "Paralyna");
								ClearDebuff(member.Name, Buffs.Paralysis);
								break;
							}
							else if (
								CanCastSpell("Viruna") &&
								Form2.config.naDisease &&
								HasDebuff(memberBuffs, Buffs.Plague))
							{
								await CastSpell(member.Name, "Viruna");
								ClearDebuff(member.Name, Buffs.Plague);
								break;
							}
							else if (
								CanCastSpell("Viruna") &&
								Form2.config.naDisease &&
								HasDebuff(memberBuffs, Buffs.Disease))
							{
								await CastSpell(member.Name, "Viruna");
								ClearDebuff(member.Name, Buffs.Disease);
								break;
							}
							else if (
								CanCastSpell("Esuna") &&
								Form2.config.Esuna &&
								hasBuff(1, Buffs.AfflatusMisery) &&
								HasDebuff(memberBuffs, Buffs.amnesia))
							{
								await CastSpell(member.Name, "Esuna");
								ClearDebuff(member.Name, Buffs.amnesia);
								break;
							}
							else if (
								CanCastSpell("Blindna") &&
								Form2.config.naBlindness &&
								HasDebuff(memberBuffs, Buffs.blindness))
							{
								await CastSpell(member.Name, "Blindna");
								ClearDebuff(member.Name, Buffs.blindness);
								break;
							}
							else if (
								CanCastSpell("Poisona") &&
								Form2.config.naPoison &&
								HasDebuff(memberBuffs, Buffs.poison))
							{
								await CastSpell(member.Name, "Poisona");
								ClearDebuff(member.Name, Buffs.poison);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Slow &&
								HasDebuff(memberBuffs, Buffs.slow))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.slow);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Bio &&
								HasDebuff(memberBuffs, Buffs.Bio))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Bio);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Bind &&
								HasDebuff(memberBuffs, Buffs.bind))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.bind);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Weight &&
								HasDebuff(memberBuffs, Buffs.weight))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.weight);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_AccuracyDown &&
								HasDebuff(memberBuffs, Buffs.AccuracyDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.AccuracyDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_DefenseDown &&
								HasDebuff(memberBuffs, Buffs.DefenseDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.DefenseDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MagicDefenseDown &&
								HasDebuff(memberBuffs, Buffs.MagicDefDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MagicDefDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_AttackDown &&
								HasDebuff(memberBuffs, Buffs.AttackDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.AttackDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MaxHpDown &&
								HasDebuff(memberBuffs, Buffs.MaxHPDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MaxHPDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_VitDown &&
								HasDebuff(memberBuffs, Buffs.VITDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.VITDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Threnody &&
								HasDebuff(memberBuffs, Buffs.Threnody))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Threnody);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Shock &&
								HasDebuff(memberBuffs, Buffs.Shock))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, 132);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_StrDown &&
								HasDebuff(memberBuffs, Buffs.STRDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.STRDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Requiem &&
								HasDebuff(memberBuffs, Buffs.Requiem))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Requiem);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Rasp &&
								HasDebuff(memberBuffs, Buffs.Rasp))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Rasp);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MaxTpDown &&
								HasDebuff(memberBuffs, Buffs.MaxTPDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MaxTPDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MaxMpDown &&
								HasDebuff(memberBuffs, Buffs.MaxMPDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MaxMPDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Addle &&
								HasDebuff(memberBuffs, Buffs.addle))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.addle);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MagicAttackDown &&
								HasDebuff(memberBuffs, Buffs.MagicAtkDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MagicAtkDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MagicAccDown &&
								HasDebuff(memberBuffs, Buffs.MagicAccDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MagicAccDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_MndDown &&
								HasDebuff(memberBuffs, Buffs.MNDDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.MNDDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_IntDown &&
								HasDebuff(memberBuffs, Buffs.INTDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.INTDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Helix &&
								HasDebuff(memberBuffs, Buffs.Helix))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Helix);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Frost &&
								HasDebuff(memberBuffs, Buffs.Frost))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Frost);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_EvasionDown &&
								HasDebuff(memberBuffs, Buffs.EvasionDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.EvasionDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Elegy &&
								HasDebuff(memberBuffs, Buffs.Elegy))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Elegy);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Drown &&
								HasDebuff(memberBuffs, Buffs.Drown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Drown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Dia &&
								HasDebuff(memberBuffs, Buffs.Dia))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Dia);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_DexDown &&
								HasDebuff(memberBuffs, Buffs.DEXDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.DEXDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Choke &&
								HasDebuff(memberBuffs, Buffs.Choke))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Choke);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_ChrDown &&
								HasDebuff(memberBuffs, Buffs.CHRDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.CHRDown);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_Burn &&
								HasDebuff(memberBuffs, Buffs.Burn))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.Burn);
								break;
							}
							else if (
								CanCastSpell("Erase") &&
								Form2.config.naErase &&
								Form2.config.na_AgiDown &&
								HasDebuff(memberBuffs, Buffs.AGIDown))
							{
								await CastSpell(member.Name, "Erase");
								ClearDebuff(member.Name, Buffs.AGIDown);
								break;
							}
						}
					}
				}
			}
		}

		private bool HasDebuff(List<string> buffs, short buffId)
		{
			return HasDebuff(buffs, buffId.ToString());
		}

		private bool HasDebuff(List<string> buffs, string buffId)
		{
			return buffs?.Any(x => x == buffId) ?? false;
		}

		private async Task CuragaCalculatorAsync(int partyMemberId)
		{
			var lowestHP_Name = instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name;

			if (instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP > 0)
			{
				if ((Form2.config.curaga5enabled) && ((((instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP) >= Form2.config.curaga5Amount) && (instancePrimary.Player.MP > 380) && HasAcquiredSpell("Curaga V") && HasRequiredJobLevel("Curaga V") == true)
				{
					var cureSpell = CureTiers("Curaga V", false);
					if (cureSpell != "false")
					{
						if (Form2.config.curagaTargetType == 0)
						{
							await CastSpell(lowestHP_Name, cureSpell);
						}
						else
						{
							await CastSpell(Form2.config.curagaTargetName, cureSpell);
						}
					}
				}
				else if (((Form2.config.curaga4enabled && HasAcquiredSpell("Curaga IV") && HasRequiredJobLevel("Curaga IV") == true) || (Form2.config.Accession && Form2.config.accessionCure && HasAcquiredSpell("Cure IV") && HasRequiredJobLevel("Cure IV") == true)) && ((((instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP) >= Form2.config.curaga4Amount) && (instancePrimary.Player.MP > 260))
				{
					var cureSpell = string.Empty;
					if (HasAcquiredSpell("Curaga IV"))
					{
						cureSpell = CureTiers("Curaga IV", false);
					}
					else if (Form2.config.Accession && Form2.config.accessionCure && CanUseJobAbility("Accession") && currentSCHCharges >= 1 && (instancePrimary.Player.MainJob == 20 || instancePrimary.Player.SubJob == 20))
					{
						cureSpell = CureTiers("Cure IV", false);
					}

					if (cureSpell != "false" && cureSpell != string.Empty)
					{
						if (cureSpell.StartsWith("Cure") && (plHasBuff(StatusEffect.Light_Arts) || plHasBuff(StatusEffect.Addendum_White)))
						{
							if (!plHasBuff(StatusEffect.Accession))
							{

								await UseJobAbility("Accession");
								return;
							}
						}

						if (Form2.config.curagaTargetType == 0)
						{
							await CastSpell(lowestHP_Name, cureSpell);
						}
						else
						{
							await CastSpell(Form2.config.curagaTargetName, cureSpell);
						}
					}
				}
				else if (((Form2.config.curaga3enabled && HasAcquiredSpell("Curaga III") && HasRequiredJobLevel("Curaga III") == true) || (Form2.config.Accession && Form2.config.accessionCure && HasAcquiredSpell("Cure III") && HasRequiredJobLevel("Cure III") == true)) && ((((instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP) >= Form2.config.curaga3Amount) && (instancePrimary.Player.MP > 180))
				{
					var cureSpell = string.Empty;
					if (HasAcquiredSpell("Curaga III"))
					{
						cureSpell = CureTiers("Curaga III", false);
					}
					else if (Form2.config.Accession && Form2.config.accessionCure && CanUseJobAbility("Accession") && currentSCHCharges >= 1 && (instancePrimary.Player.MainJob == 20 || instancePrimary.Player.SubJob == 20))
					{
						cureSpell = CureTiers("Cure III", false);
					}

					if (cureSpell != "false" && cureSpell != string.Empty)
					{
						if (cureSpell.StartsWith("Cure") && (plHasBuff(StatusEffect.Light_Arts) || plHasBuff(StatusEffect.Addendum_White)))
						{
							if (!plHasBuff(StatusEffect.Accession))
							{
								await UseJobAbility("Accession");
								return;
							}
						}

						if (Form2.config.curagaTargetType == 0)
						{
							await CastSpell(lowestHP_Name, cureSpell);
						}
						else
						{
							await CastSpell(Form2.config.curagaTargetName, cureSpell);
						}
					}
				}
				else if (((Form2.config.curaga2enabled && HasAcquiredSpell("Curaga II") && HasRequiredJobLevel("Curaga II") == true) || (Form2.config.Accession && Form2.config.accessionCure && HasAcquiredSpell("Cure II") && HasRequiredJobLevel("Cure II") == true)) && ((((instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP) >= Form2.config.curaga2Amount) && (instancePrimary.Player.MP > 120))
				{
					var cureSpell = string.Empty;
					if (HasAcquiredSpell("Curaga II"))
					{
						cureSpell = CureTiers("Curaga II", false);
					}
					else if (Form2.config.Accession && Form2.config.accessionCure && CanUseJobAbility("Accession") && currentSCHCharges >= 1 && (instancePrimary.Player.MainJob == 20 || instancePrimary.Player.SubJob == 20))
					{
						cureSpell = CureTiers("Cure II", false);
					}
					if (cureSpell != "false" && cureSpell != string.Empty)
					{
						if (cureSpell.StartsWith("Cure") && (plHasBuff(StatusEffect.Light_Arts) || plHasBuff(StatusEffect.Addendum_White)))
						{
							if (!plHasBuff(StatusEffect.Accession))
							{
								await UseJobAbility("Accession");
								return;
							}
						}

						if (Form2.config.curagaTargetType == 0)
						{
							await CastSpell(lowestHP_Name, cureSpell);
						}
						else
						{
							await CastSpell(Form2.config.curagaTargetName, cureSpell);
						}
					}
				}
				else if (((Form2.config.curagaEnabled && HasAcquiredSpell("Curaga") && HasRequiredJobLevel("Curaga") == true) || (Form2.config.Accession && Form2.config.accessionCure && HasAcquiredSpell("Cure") && HasRequiredJobLevel("Cure") == true)) && ((((instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP * 100) / instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHPP) - instanceMonitored.Party.GetPartyMembers()[partyMemberId].CurrentHP) >= Form2.config.curagaAmount) && (instancePrimary.Player.MP > 60))
				{
					var cureSpell = string.Empty;
					if (HasAcquiredSpell("Curaga"))
					{
						cureSpell = CureTiers("Curaga", false);
					}
					else if (Form2.config.Accession && Form2.config.accessionCure && CanUseJobAbility("Accession") && currentSCHCharges >= 1 && (instancePrimary.Player.MainJob == 20 || instancePrimary.Player.SubJob == 20))
					{
						cureSpell = CureTiers("Cure", false);
					}

					if (cureSpell != "false" && cureSpell != string.Empty)
					{
						if (cureSpell.StartsWith("Cure") && (plHasBuff(StatusEffect.Light_Arts) || plHasBuff(StatusEffect.Addendum_White)))
						{
							if (!plHasBuff(StatusEffect.Accession))
							{
								await UseJobAbility("Accession");
								return;
							}
						}

						if (Form2.config.curagaTargetType == 0)
						{
							await CastSpell(lowestHP_Name, cureSpell);
						}
						else
						{
							await CastSpell(Form2.config.curagaTargetName, cureSpell);
						}
					}
				}
			}
		}

		private bool castingPossible(byte partyMemberId)
		{
			var member = instanceMonitored.Party.GetPartyMembers()[partyMemberId];
			var entity = instancePrimary.Entity.GetEntity((int)member.TargetIndex);

			if (instancePrimary.Party.GetPartyMember(0).ID == member.ID)
			{
				return true;
			}

			if (entity.Distance >= 0 && entity.Distance < 21 && member.CurrentHP > 0)
			{
				return true;
			}

			return false;
		}

		private bool plHasBuff(StatusEffect requestedStatus)
		{
			return plHasBuff((short)requestedStatus);
		}

		private bool plHasBuff(short buffId)
		{
			return instancePrimary.Player.Buffs.Contains(buffId);
		}

		private bool monitoredStatusCheck(StatusEffect requestedStatus)
		{
			var statusFound = false;
			foreach (var status in instanceMonitored.Player.Buffs.Cast<StatusEffect>().Where(status => requestedStatus == status))
			{
				statusFound = true;
			}
			return statusFound;
		}

		public bool hasBuff(short buffID, int checkedPlayer)
		{
			if (checkedPlayer == 0) return plHasBuff(buffID);
			else return instanceMonitored.Player.GetPlayerInfo().Buffs.Contains(buffID);
		}

		private async Task<bool> CastSpell(string partyMemberName, string spellName)
		{
			var spellInfo = instancePrimary.Resources.GetSpell(spellName.Trim(), 0);
			var actualSpellName = spellInfo?.Name[0];

			if (string.IsNullOrWhiteSpace(actualSpellName))
			{
				Invoke((MethodInvoker)(() =>
				{
					currentAction.Text = $"Spell {spellName} not found.";
				}));

				return false;
			}

			try
			{
				// The cancellation token is used by the addon integration to cancel 
				// the spell casting early if in-game fastcast or quick magic procs. 
				if (castTokenSource == null)
				{
					castTokenSource = new CancellationTokenSource();
					spellCastTime = Spells.GetCastTimeSeconds(spellName) * 1000;
					spellCommand = $"/ma \"{actualSpellName}\" {partyMemberName}";
					await CastSpellInternal(castTokenSource.Token);

					// Once casting has completed, we must dispose of and nullify the
					// cancellation token source so that the next spell cast can continue.
					castTokenSource.Dispose();
					castTokenSource = null;
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error casting spell.");
				Debug.WriteLine(ex.ToString());
				return false;
			}
		}

		private async Task hastePlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Haste"))
			{
				playerHaste[partyMemberId] = DateTime.Now;
			}
		}

		private async Task haste_IIPlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Haste II"))
			{
				playerHaste_II[partyMemberId] = DateTime.Now;
			}
		}

		private async Task AdloquiumPlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Adloquium"))
			{
				playerAdloquium[partyMemberId] = DateTime.Now;
			}
		}

		private async Task FlurryPlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Flurry"))
			{
				playerFlurry[partyMemberId] = DateTime.Now;
			}
		}

		private async Task Flurry_IIPlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Flurry II"))
			{
				playerFlurry_II[partyMemberId] = DateTime.Now;
			}
		}

		private async Task Phalanx_IIPlayer(byte partyMemberId)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Phalanx II"))
			{
				playerPhalanx_II[partyMemberId] = DateTime.Now;
			}
		}

		private async Task StormSpellPlayer(byte partyMemberId, string Spell)
		{
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, Spell))
			{
				playerStormspell[partyMemberId] = DateTime.Now;
			}
		}

		private async Task Regen_Player(byte partyMemberId)
		{
			string[] regen_spells = { "Regen", "Regen II", "Regen III", "Regen IV", "Regen V" };
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, regen_spells[Form2.config.autoRegen_Spell]))
			{
				playerRegen[partyMemberId] = DateTime.Now;
			}
		}

		private async Task Refresh_Player(byte partyMemberId)
		{
			string[] refresh_spells = { "Refresh", "Refresh II", "Refresh III" };
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, refresh_spells[Form2.config.autoRefresh_Spell]))
			{
				playerRefresh[partyMemberId] = DateTime.Now;
			}
		}

		private async Task protectPlayer(byte partyMemberId)
		{
			string[] protect_spells = { "Protect", "Protect II", "Protect III", "Protect IV", "Protect V" };
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, protect_spells[Form2.config.autoProtect_Spell]))
			{
				playerProtect[partyMemberId] = DateTime.Now;
			}
		}

		private async Task shellPlayer(byte partyMemberId)
		{
			string[] shell_spells = { "Shell", "Shell II", "Shell III", "Shell IV", "Shell V" };
			if (await CastSpell(instanceMonitored.Party.GetPartyMembers()[partyMemberId].Name, shell_spells[Form2.config.autoShell_Spell]))
			{
				playerShell[partyMemberId] = DateTime.Now;
			}
		}

		private bool ActiveSpikes()
		{
			if ((Form2.config.plSpikes_Spell == 0) && plHasBuff(StatusEffect.Blaze_Spikes))
			{
				return true;
			}
			else if ((Form2.config.plSpikes_Spell == 1) && plHasBuff(StatusEffect.Ice_Spikes))
			{
				return true;
			}
			else if ((Form2.config.plSpikes_Spell == 2) && plHasBuff(StatusEffect.Shock_Spikes))
			{
				return true;
			}
			return false;
		}

		private bool PLInParty()
		{
			// FALSE IS WANTED WHEN NOT IN PARTY

			if (instancePrimary.Player.Name == instanceMonitored.Player.Name) // MONITORED AND POL ARE BOTH THE SAME THEREFORE IN THE PARTY
			{
				return true;
			}

			var PARTYD = instancePrimary.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == instancePrimary.Player.ZoneId);

			var gen = new List<string>();
			foreach (var pData in PARTYD)
			{
				if (pData != null && pData.Name != "")
				{
					gen.Add(pData.Name);
				}
			}

			if (gen.Contains(instancePrimary.Player.Name) && gen.Contains(instanceMonitored.Player.Name))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool CheckIfAutoStormspellEnabled(byte id)
		{

			if (Form2.config.autoStorm_Spell == 0)
			{
				if (autoSandstormEnabled[id])
				{
					return true;
				}
				else if (autoWindstormEnabled[id])
				{
					return true;
				}
				else if (autoFirestormEnabled[id])
				{
					return true;
				}
				else if (autoRainstormEnabled[id])
				{
					return true;
				}
				else if (autoHailstormEnabled[id])
				{
					return true;
				}
				else if (autoThunderstormEnabled[id])
				{
					return true;
				}
				else if (autoVoidstormEnabled[id])
				{
					return true;
				}
				else if (autoAurorastormEnabled[id])
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (Form2.config.autoStorm_Spell == 1)
			{
				if (autoSandstormEnabled[id])
				{
					return true;
				}
				else if (autoWindstormEnabled[id])
				{
					return true;
				}
				else if (autoFirestormEnabled[id])
				{
					return true;
				}
				else if (autoRainstormEnabled[id])
				{
					return true;
				}
				else if (autoHailstormEnabled[id])
				{
					return true;
				}
				else if (autoThunderstormEnabled[id])
				{
					return true;
				}

				else if (autoVoidstormEnabled[id])
				{
					return true;
				}
				else if (autoAurorastormEnabled[id])
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private string CheckStormspell(byte id)
		{
			if (Form2.config.autoStorm_Spell == 0)
			{
				if (autoSandstormEnabled[id])
				{
					return "Sandstorm";
				}
				else if (autoWindstormEnabled[id])
				{
					return "Windstorm";
				}
				else if (autoFirestormEnabled[id])
				{
					return "Firestorm";
				}
				else if (autoRainstormEnabled[id])
				{
					return "Rainstorm";
				}
				else if (autoHailstormEnabled[id])
				{
					return "Hailstorm";
				}
				else if (autoThunderstormEnabled[id])
				{
					return "Thunderstorm";
				}
				else if (autoVoidstormEnabled[id])
				{
					return "Voidstorm";
				}
				else if (autoAurorastormEnabled[id])
				{
					return "Aurorastorm";
				}
				else
				{
					return "false";
				}
			}
			else if (Form2.config.autoStorm_Spell == 1)
			{
				if (autoSandstormEnabled[id])
				{
					return "Sandstorm II";
				}
				else if (autoWindstormEnabled[id])
				{
					return "Windstorm II";
				}
				else if (autoFirestormEnabled[id])
				{
					return "Firestorm II";
				}
				else if (autoRainstormEnabled[id])
				{
					return "Rainstorm II";
				}
				else if (autoHailstormEnabled[id])
				{
					return "Hailstorm II";
				}
				else if (autoThunderstormEnabled[id])
				{
					return "Thunderstorm II";
				}

				else if (autoVoidstormEnabled[id])
				{
					return "Voidstorm II";
				}
				else if (autoAurorastormEnabled[id])
				{
					return "Aurorastorm II";
				}
				else
				{
					return "false";
				}
			}
			else
			{
				return "false";
			}
		}

		private string GetShellraLevel(decimal p)
		{
			switch ((int)p)
			{
				case 1:
					return "Shellra";

				case 2:
					return "Shellra II";

				case 3:
					return "Shellra III";

				case 4:
					return "Shellra IV";

				case 5:
					return "Shellra V";

				default:
					return "Shellra";
			}
		}

		private string GetProtectraLevel(decimal p)
		{
			switch ((int)p)
			{
				case 1:
					return "Protectra";

				case 2:
					return "Protectra II";

				case 3:
					return "Protectra III";

				case 4:
					return "Protectra IV";

				case 5:
					return "Protectra V";

				default:
					return "Protectra";
			}
		}

		private string ReturnGeoSpell(int GEOSpell_ID, int GeoSpell_Type)
		{
			// GRAB THE SPELL FROM THE CUSTOM LIST
			var GeoSpell = geoSpells.Where(c => c.Position == GEOSpell_ID).FirstOrDefault();

			if (GeoSpell_Type == 1)
			{
				if (HasAcquiredSpell(GeoSpell.IndiSpell) && HasRequiredJobLevel(GeoSpell.IndiSpell) == true)
				{
					if (SpellReadyToCast(GeoSpell.IndiSpell))
					{
						return GeoSpell.IndiSpell;
					}
					else
					{
						return "SpellRecast";
					}
				}
				else
				{
					return "SpellNA";
				}
			}
			else if (GeoSpell_Type == 2)
			{
				if (HasAcquiredSpell(GeoSpell.GeoSpell) && HasRequiredJobLevel(GeoSpell.GeoSpell) == true)
				{
					if (SpellReadyToCast(GeoSpell.GeoSpell))
					{
						return GeoSpell.GeoSpell;
					}
					else
					{
						return "SpellRecast";
					}
				}
				else
				{
					return "SpellNA";
				}
			}
			else
			{
				return "SpellError_Cancel";
			}
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var settings = new Form2();
			settings.Show();
		}

		private void player0optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 0;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[0];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[0];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[0];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[0];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[0];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[0];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[0];

			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player1optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 1;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[1];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[1];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[1];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[1];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[1];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[1];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[1];
			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player2optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 2;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[2];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[2];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[2];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[2];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[2];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[2];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[2];
			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player3optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 3;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[3];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[3];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[3];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[3];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[3];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[3];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[3];
			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player4optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 4;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[4];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[4];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[4];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[4];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[4];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[4];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[4];
			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player5optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 5;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[5];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[5];
			autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[5];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[5];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[5];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[5];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[5];
			playerOptions.Show(party0, new Point(0, 0));
		}

		private void player6optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 6;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[6];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[6];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[6];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[6];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[6];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[6];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player7optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 7;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[7];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[7];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[7];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[7];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[7];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[7];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player8optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 8;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[8];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[8];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[8];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[8];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[8];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[8];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player9optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 9;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[9];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[9];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[9];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[9];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[9];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[9];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player10optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 10;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[10];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[10];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[10];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[10];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[10];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[10];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player11optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 11;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[11];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[11];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[11];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[11];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[11];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[11];
			playerOptions.Show(party1, new Point(0, 0));
		}

		private void player12optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 12;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[12];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[12];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[12];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[12];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[12];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[12];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player13optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 13;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[13];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[13];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[13];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[13];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[13];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[13];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player14optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 14;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[14];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[14];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[14];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[14];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[14];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[14];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player15optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 15;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[15];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[15];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[15];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[15];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[15];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[15];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player16optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 16;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[16];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[16];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[16];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[16];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[16];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[16];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player17optionsButton_Click(object sender, EventArgs e)
		{
			playerOptionsSelected = 17;
			autoHasteToolStripMenuItem.Checked = autoHasteEnabled[17];
			autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[17];
			autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[17];
			autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[17];
			autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[17];
			autoShellToolStripMenuItem.Checked = autoShell_Enabled[17];
			playerOptions.Show(party2, new Point(0, 0));
		}

		private void player0buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 0;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[0];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[0];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[0];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[0];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[0];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[0];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[0];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[0];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[0];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[0];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[0];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player1buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 1;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[1];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[1];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[1];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[1];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[1];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[1];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[1];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[1];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[1];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[1];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[1];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player2buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 2;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[2];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[2];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[2];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[2];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[2];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[2];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[2];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[2];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[2];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[2];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[2];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player3buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 3;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[3];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[3];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[3];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[3];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[3];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[3];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[3];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[3];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[3];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[3];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[3];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player4buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 4;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[4];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[4];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[4];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[4];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[4];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[4];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[4];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[4];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[4];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[4];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[4];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player5buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 5;
			autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[5];
			autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[5];
			autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[5];
			SandstormToolStripMenuItem.Checked = autoSandstormEnabled[5];
			RainstormToolStripMenuItem.Checked = autoRainstormEnabled[5];
			WindstormToolStripMenuItem.Checked = autoWindstormEnabled[5];
			FirestormToolStripMenuItem.Checked = autoFirestormEnabled[5];
			HailstormToolStripMenuItem.Checked = autoHailstormEnabled[5];
			ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[5];
			VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[5];
			AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[5];
			autoOptions.Show(party0, new Point(0, 0));
		}

		private void player6buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 6;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player7buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 7;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player8buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 8;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player9buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 9;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player10buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 10;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player11buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 11;
			autoOptions.Show(party1, new Point(0, 0));
		}

		private void player12buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 12;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private void player13buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 13;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private void player14buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 14;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private void player15buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 15;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private void player16buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 16;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private void player17buffsButton_Click(object sender, EventArgs e)
		{
			autoOptionsSelected = 17;
			autoOptions.Show(party2, new Point(0, 0));
		}

		private static async Task SendPrimaryCommand(string command, int delay = 200)
		{
			instancePrimary.ThirdParty.SendString(command);
			await Task.Delay(delay);
		}

		private static async Task WaitForCastingToFinish()
		{
			var percent = instancePrimary.CastBar.Percent;
			var timer = Stopwatch.StartNew();

			while (timer.ElapsedMilliseconds < 5000)
			{
				if (percent == 1) break;
				percent = instancePrimary.CastBar.Percent;
				await Task.Delay(100);
			}
		}

		private void autoHasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoHasteEnabled[playerOptionsSelected] = !autoHasteEnabled[playerOptionsSelected];
			autoHaste_IIEnabled[playerOptionsSelected] = false;
			autoFlurryEnabled[playerOptionsSelected] = false;
			autoFlurry_IIEnabled[playerOptionsSelected] = false;
		}

		private void autoHasteIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoHaste_IIEnabled[playerOptionsSelected] = !autoHaste_IIEnabled[playerOptionsSelected];
			autoHasteEnabled[playerOptionsSelected] = false;
			autoFlurryEnabled[playerOptionsSelected] = false;
			autoFlurry_IIEnabled[playerOptionsSelected] = false;
		}

		private void autoAdloquiumToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoAdloquium_Enabled[playerOptionsSelected] = !autoAdloquium_Enabled[playerOptionsSelected];
		}

		private void autoFlurryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoFlurryEnabled[playerOptionsSelected] = !autoFlurryEnabled[playerOptionsSelected];
			autoHasteEnabled[playerOptionsSelected] = false;
			autoHaste_IIEnabled[playerOptionsSelected] = false;
			autoFlurry_IIEnabled[playerOptionsSelected] = false;
		}

		private void autoFlurryIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoFlurry_IIEnabled[playerOptionsSelected] = !autoFlurry_IIEnabled[playerOptionsSelected];
			autoHasteEnabled[playerOptionsSelected] = false;
			autoFlurryEnabled[playerOptionsSelected] = false;
			autoHaste_IIEnabled[playerOptionsSelected] = false;
		}

		private void autoProtectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoProtect_Enabled[playerOptionsSelected] = !autoProtect_Enabled[playerOptionsSelected];
		}

		private void enableDebuffRemovalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var generated_name = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name.ToLower();
			characterNames_naRemoval.Add(generated_name);
		}

		private void autoShellToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoShell_Enabled[playerOptionsSelected] = !autoShell_Enabled[playerOptionsSelected];
		}

		private void autoHasteToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			autoHasteEnabled[autoOptionsSelected] = !autoHasteEnabled[autoOptionsSelected];
			autoHaste_IIEnabled[playerOptionsSelected] = false;
			autoFlurryEnabled[playerOptionsSelected] = false;
			autoFlurry_IIEnabled[playerOptionsSelected] = false;
		}

		private void autoPhalanxIIToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			autoPhalanx_IIEnabled[autoOptionsSelected] = !autoPhalanx_IIEnabled[autoOptionsSelected];
		}

		private void autoRegenVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoRegen_Enabled[autoOptionsSelected] = !autoRegen_Enabled[autoOptionsSelected];
		}

		private void autoRefreshIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			autoRefreshEnabled[autoOptionsSelected] = !autoRefreshEnabled[autoOptionsSelected];
		}

		private async Task hasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await hastePlayer(playerOptionsSelected);
		}

		private void followToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.autoFollowName = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
		}

		private void stopfollowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.autoFollowName = string.Empty;
		}

		private void EntrustTargetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.EntrustedSpell_Target = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
		}

		private void GeoTargetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.LuopanSpell_Target = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
		}

		private void DevotionTargetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.DevotionTargetName = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
		}

		private void HateEstablisherToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2.config.autoTarget_Target = instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
		}

		private async Task phalanxIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Phalanx II");
		}

		private async Task invisibleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Invisible");
		}

		private async Task refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Refresh");
		}

		private async Task refreshIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Refresh II");
		}

		private async Task refreshIIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Refresh III");
		}

		private async Task sneakToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Sneak");
		}

		private async Task regenIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Regen II");
		}

		private async Task regenIIIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Regen III");
		}

		private async Task regenIVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Regen IV");
		}

		private async Task eraseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Erase");
		}

		private async Task sacrificeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Sacrifice");
		}

		private async Task blindnaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Blindna");
		}

		private async Task cursnaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Cursna");
		}

		private async Task paralynaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Paralyna");
		}

		private async Task poisonaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Poisona");
		}

		private async Task stonaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Stona");
		}

		private async Task silenaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Silena");
		}

		private async Task virunaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Viruna");
		}

		private void setAllStormsFalse(byte autoOptionsSelected)
		{
			// MessageBox.Show("SONG DATA: " + activeStorm + " " + autoOptionsSelected);

			autoSandstormEnabled[autoOptionsSelected] = false;
			autoRainstormEnabled[autoOptionsSelected] = false;
			autoFirestormEnabled[autoOptionsSelected] = false;
			autoWindstormEnabled[autoOptionsSelected] = false;
			autoHailstormEnabled[autoOptionsSelected] = false;
			autoThunderstormEnabled[autoOptionsSelected] = false;
			autoVoidstormEnabled[autoOptionsSelected] = false;
			autoAurorastormEnabled[autoOptionsSelected] = false;
		}

		private void SandstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoSandstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoSandstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void RainstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoRainstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoRainstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void WindstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoWindstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoWindstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void FirestormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoFirestormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoFirestormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void HailstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoHailstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoHailstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void ThunderstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoThunderstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoThunderstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void VoidstormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoVoidstormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoVoidstormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private void AurorastormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var currentStatus = autoAurorastormEnabled[autoOptionsSelected];
			setAllStormsFalse(autoOptionsSelected);
			autoAurorastormEnabled[autoOptionsSelected] = !currentStatus;
		}

		private async Task protectIVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Protect IV");
		}

		private async Task protectVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Protect V");
		}

		private async Task shellIVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Shell IV");
		}

		private async Task shellVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await CastSpell(instanceMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name, "Shell V");
		}

		private void button3_Click(object sender, EventArgs e)
		{
			song_casting = 0;
			forceSongRecast = true;

			if (pauseActions == false)
			{
				pauseButton.Text = "Paused!";
				pauseButton.ForeColor = Color.Red;
				enableActions = false;
				activeBuffs.Clear();
				pauseActions = true;
				if (Form2.config.FFXIDefaultAutoFollow == false)
				{
					instancePrimary.AutoFollow.IsAutoFollowing = false;
				}
			}
			else
			{
				pauseButton.Text = "Pause";
				pauseButton.ForeColor = Color.Black;
				enableActions = true;
				pauseActions = false;

				if (Form2.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
				{
					WindowState = FormWindowState.Minimized;
				}

				if (Form2.config.EnableAddOn && isAddonLoaded == 0)
				{
					if (hookMode == "Windower")
					{
						instancePrimary.ThirdParty.SendString("//lua load CurePlease_addon");
						Thread.Sleep(1500);
						instancePrimary.ThirdParty.SendString("//cpaddon settings " + endpoint.Address + " " + endpoint.Port);
						Thread.Sleep(100);
						if (Form2.config.enableHotKeys)
						{
							instancePrimary.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
							instancePrimary.ThirdParty.SendString("//bind ^!F2 cureplease start");
							instancePrimary.ThirdParty.SendString("//bind ^!F3 cureplease pause");
						}
					}
					else if (hookMode == "Ashita")
					{
						instancePrimary.ThirdParty.SendString("/addon load CurePlease_addon");
						Thread.Sleep(1500);
						instancePrimary.ThirdParty.SendString("/cpaddon settings " + endpoint.Address + " " + endpoint.Port);
						Thread.Sleep(100);
						if (Form2.config.enableHotKeys)
						{
							instancePrimary.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
							instancePrimary.ThirdParty.SendString("/bind ^!F2 /cureplease start");
							instancePrimary.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
						}
					}

					AddOnStatus_Click(sender, e);


					isAddonLoaded = 1;


				}
			}
		}

		private void Debug_Click(object sender, EventArgs e)
		{
			if (instanceMonitored == null)
			{

				MessageBox.Show("Attach to process before pressing this button", "Error");
				return;
			}

			MessageBox.Show(debug_MSG_show);
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (TopMost)
			{
				TopMost = false;
			}
			else
			{
				TopMost = true;
			}
		}

		private void MouseClickTray(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (WindowState == FormWindowState.Minimized && Visible == false)
			{
				Show();
				WindowState = FormWindowState.Normal;
			}
			else
			{
				Hide();
				WindowState = FormWindowState.Minimized;
			}
		}

		private bool CheckShellraLevelPossession()
		{
			switch ((int)Form2.config.plShellra_Level)
			{
				case 1:
					if (HasRequiredJobLevel("Shellra") == true && SpellReadyToCast("Shellra"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 2:
					if (HasRequiredJobLevel("Shellra II") == true && SpellReadyToCast("Shellra II"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 3:
					if (HasRequiredJobLevel("Shellra III") == true && SpellReadyToCast("Shellra III"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 4:
					if (HasRequiredJobLevel("Shellra IV") == true && SpellReadyToCast("Shellra IV"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 5:
					if (HasRequiredJobLevel("Shellra V") == true && SpellReadyToCast("Shellra V"))
					{
						return true;
					}
					else
					{
						return false;
					}

				default:
					return false;
			}
		}

		private bool CheckProtectraLevelPossession()
		{
			switch ((int)Form2.config.plProtectra_Level)
			{
				case 1:
					if (HasRequiredJobLevel("Protectra") == true && SpellReadyToCast("Protectra"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 2:
					if (HasRequiredJobLevel("Protectra II") == true && SpellReadyToCast("Protectra II"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 3:
					if (HasRequiredJobLevel("Protectra III") == true && SpellReadyToCast("Protectra III"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 4:
					if (HasRequiredJobLevel("Protectra IV") == true && SpellReadyToCast("Protectra IV"))
					{
						return true;
					}
					else
					{
						return false;
					}

				case 5:
					if (HasRequiredJobLevel("Protectra V") == true && SpellReadyToCast("Protectra V"))
					{
						return true;
					}
					else
					{
						return false;
					}

				default:
					return false;
			}
		}

		private bool CheckReraiseLevelPossession()
		{
			switch (Form2.config.plReraise_Level)
			{
				case 1:
					if (HasRequiredJobLevel("Reraise") == true && SpellReadyToCast("Reraise"))
					{
						// Check SCH possiblity
						if (instancePrimary.Player.MainJob == 20 && instancePrimary.Player.SubJob != 3 && !hasBuff(401, 0))
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}

				case 2:

					if (HasRequiredJobLevel("Reraise II") == true && SpellReadyToCast("Reraise II"))
					{
						if (instancePrimary.Player.MainJob == 20 && !hasBuff(401, 0))
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}

				case 3:

					if (HasRequiredJobLevel("Reraise III") == true && SpellReadyToCast("Reraise III"))
					{
						if (instancePrimary.Player.MainJob == 20 && !hasBuff(401, 0))
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}

				case 4:
					if (HasRequiredJobLevel("Reraise IV") == true && SpellReadyToCast("Reraise IV"))
					{
						if (instancePrimary.Player.MainJob == 20 && !hasBuff(401, 0))
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}

				default:
					return false;
			}
		}

		private bool CheckRefreshLevelPossession()
		{
			switch (Form2.config.plRefresh_Level)
			{
				case 1:
					return HasAcquiredSpell("Refresh");

				case 2:
					return HasAcquiredSpell("Refresh II");

				case 3:
					return HasAcquiredSpell("Refresh III");

				default:
					return false;
			}
		}

		private bool CheckRegenLevelPossession()
		{
			switch (Form2.config.plRegen_Level)
			{
				case 1:
					return HasAcquiredSpell("Regen");

				case 2:
					return HasAcquiredSpell("Regen II");

				case 3:
					return HasAcquiredSpell("Regen III");

				case 4:
					return HasAcquiredSpell("Regen IV");

				case 5:
					return HasAcquiredSpell("Regen V");

				default:
					return false;
			}
		}

		private void chatLogToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form4 = new Form4(this);
			form4.Show();
		}

		private void partyBuffsdebugToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var PartyBuffs = new PartyBuffs(this);
			PartyBuffs.Show();
		}

		private void refreshCharactersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

			if (instancePrimary.Player.LoginStatus == (int)LoginStatus.Loading || instanceMonitored.Player.LoginStatus == (int)LoginStatus.Loading)
			{
			}
			else
			{
				if (pol.Count() < 1)
				{
					MessageBox.Show("FFXI not found");
				}
				else
				{
					POLID.Items.Clear();
					POLID2.Items.Clear();
					processIds.Items.Clear();

					for (var i = 0; i < pol.Count(); i++)
					{
						POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
						POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
						processIds.Items.Add(pol.ElementAt(i).Id);
					}

					POLID.SelectedIndex = 0;
					POLID2.SelectedIndex = 0;
				}
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			notifyIcon1.Dispose();

			if (instancePrimary != null)
			{
				if (hookMode == "Ashita")
				{
					instancePrimary.ThirdParty.SendString("/addon unload CurePlease_addon");
					if (Form2.config.enableHotKeys)
					{
						instancePrimary.ThirdParty.SendString("/unbind ^!F1");
						instancePrimary.ThirdParty.SendString("/unbind ^!F2");
						instancePrimary.ThirdParty.SendString("/unbind ^!F3");
					}
				}
				else if (hookMode == "Windower")
				{
					instancePrimary.ThirdParty.SendString("//lua unload CurePlease_addon");

					if (Form2.config.enableHotKeys)
					{
						instancePrimary.ThirdParty.SendString("//unbind ^!F1");
						instancePrimary.ThirdParty.SendString("//unbind ^!F2");
						instancePrimary.ThirdParty.SendString("//unbind ^!F3");
					}

				}
			}

		}

		private int followID()
		{
			if ((setinstance2.Enabled == true) && !string.IsNullOrEmpty(Form2.config.autoFollowName) && !pauseActions)
			{
				for (var x = 0; x < 2048; x++)
				{
					var entity = instancePrimary.Entity.GetEntity(x);

					if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.autoFollowName.ToLower()))
					{
						return Convert.ToInt32(entity.TargetID);
					}
				}
				return -1;
			}
			else
			{
				return -1;
			}
		}

		public bool plMonitoredSameParty()
		{
			var PT_Structutre_NO = GeneratePT_structure();

			// Now generate the party
			var cParty = instanceMonitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == instancePrimary.Player.ZoneId);

			// Make sure member number is not 0 (null) or 4 (void)
			if (PT_Structutre_NO != 0 && PT_Structutre_NO != 4)
			{
				// Run through Each party member as we're looking for either a specific name or if set
				// otherwise anyone with the MP criteria in the current party.
				foreach (var pData in cParty)
				{
					if (PT_Structutre_NO == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5 && pData.Name == instanceMonitored.Player.Name)
					{
						return true;
					}
					else if (PT_Structutre_NO == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11 && pData.Name == instanceMonitored.Player.Name)
					{
						return true;
					}
					else if (PT_Structutre_NO == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17 && pData.Name == instanceMonitored.Player.Name)
					{
						return true;
					}
				}
			}

			return false;
		}

		public int GeneratePT_structure()
		{
			// FIRST CHECK THAT BOTH THE PL AND MONITORED PLAYER ARE IN THE SAME PT/ALLIANCE
			var currentPT = instanceMonitored.Party.GetPartyMembers();

			var partyChecker = 0;

			foreach (var PTMember in currentPT)
			{
				if (PTMember.Name == instancePrimary.Player.Name)
				{
					partyChecker++;
				}
				if (PTMember.Name == instanceMonitored.Player.Name)
				{
					partyChecker++;
				}
			}

			if (partyChecker >= 2)
			{
				int plParty = instanceMonitored.Party.GetPartyMembers().Where(p => p.Name == instancePrimary.Player.Name).Select(p => p.MemberNumber).FirstOrDefault();

				if (plParty <= 5)
				{
					return 1;
				}
				else if (plParty <= 11 && plParty >= 6)
				{
					return 2;
				}
				else if (plParty <= 17 && plParty >= 12)
				{
					return 3;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 4;
			}
		}

		private void resetSongTimer_Tick(object sender, EventArgs e)
		{
			song_casting = 0;
		}

		private void checkSCHCharges_Tick(object sender, EventArgs e)
		{
			if (instancePrimary != null && instanceMonitored != null)
			{
				int MainJob = instancePrimary.Player.MainJob;
				int SubJob = instancePrimary.Player.SubJob;

				if (MainJob == 20 || SubJob == 20)
				{
					if (plHasBuff(StatusEffect.Light_Arts) || plHasBuff(StatusEffect.Addendum_White))
					{
						var currentRecastTimer = GetAbilityRecastBySpellId(231);

						int SpentPoints = instancePrimary.Player.GetJobPoints(20).SpentJobPoints;

						int MainLevel = instancePrimary.Player.MainJobLevel;
						int SubLevel = instancePrimary.Player.SubJobLevel;

						var baseTimer = 240;
						var baseCharges = 1;

						// Generate the correct timer between charges depending on level / Job Points
						if (MainLevel == 99 && SpentPoints > 550 && MainJob == 20)
						{
							baseTimer = 33;
							baseCharges = 5;
						}
						else if (MainLevel >= 90 && SpentPoints < 550 && MainJob == 20)
						{
							baseTimer = 48;
							baseCharges = 5;
						}
						else if (MainLevel >= 70 && MainLevel < 90 && MainJob == 20)
						{
							baseTimer = 60;
							baseCharges = 4;
						}
						else if (MainLevel >= 50 && MainLevel < 70 && MainJob == 20)
						{
							baseTimer = 80;
							baseCharges = 3;
						}
						else if ((MainLevel >= 30 && MainLevel < 50 && MainJob == 20) || (SubLevel >= 30 && SubLevel < 50 && SubJob == 20))
						{
							baseTimer = 120;
							baseCharges = 2;
						}
						else if ((MainLevel >= 10 && MainLevel < 30 && MainJob == 20) || (SubLevel >= 10 && SubLevel < 30 && SubJob == 20))
						{
							baseTimer = 240;
							baseCharges = 1;
						}

						// Now knowing what the time between charges is lets calculate how many
						// charges are available

						if (currentRecastTimer == 0)
						{
							currentSCHCharges = baseCharges;
						}
						else
						{
							var t = currentRecastTimer / 60;

							var stratsUsed = t / baseTimer;

							currentSCHCharges = (int)Math.Ceiling((decimal)baseCharges - stratsUsed);

							if (baseTimer == 120)
							{
								currentSCHCharges -= 1;
							}
						}
					}
				}
			}
		}

		private bool CheckEngagedStatus()
		{
			if (instanceMonitored == null || instancePrimary == null) { return false; }


			if (Form2.config.GeoWhenEngaged == false)
			{
				return true;
			}
			else if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
			{
				for (var x = 0; x < 2048; x++)
				{
					var z = instancePrimary.Entity.GetEntity(x);
					if (z.Name != string.Empty && z.Name != null)
					{
						if (z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
						{
							if (z.Status == 1)
							{
								return true;
							}
							else
							{
								return false;
							}
						}
					}
				}
				return false;
			}
			else
			{
				if (instanceMonitored.Player.Status == 1)
				{
					return true;


				}
				else
				{
					return false;
				}
			}
		}

		private void EclipticTimer_Tick(object sender, EventArgs e)
		{
			if (instanceMonitored == null || instancePrimary == null) { return; }

			if (instancePrimary.Player.Pet.HealthPercent >= 1)
			{
				eclipticActive = true;
			}
			else
			{
				eclipticActive = false;
			}
		}

		private bool GEO_EnemyCheck()
		{
			if (instanceMonitored == null || instancePrimary == null) { return false; }

			// Grab GEO spell name
			var SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);

			if (SpellCheckedResult == "SpellError_Cancel" || SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
			{
				// Do nothing and continue on with the program
				return true;
			}
			else
			{
				if (instancePrimary.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
				{
					return true; // SPELL TARGET IS PLAYER THEREFORE ONLY THE DEFAULT CHECK IS REQUIRED SO JUST RETURN TRUE TO VOID THIS CHECK
				}
				else
				{
					if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
					{
						for (var x = 0; x < 2048; x++)
						{
							var z = instancePrimary.Entity.GetEntity(x);
							if (z.Name != string.Empty && z.Name != null)
							{
								if (z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
								{
									if (z.Status == 1)
									{
										return true;
									}
									else
									{
										return false;
									}
								}
							}
						}
						return false;
					}
					else
					{
						if (instanceMonitored.Player.Status == 1)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
				}
			}
		}

		private int CheckEngagedStatus_Hate()
		{
			if (Form2.config.AssistSpecifiedTarget == true && Form2.config.autoTarget_Target != string.Empty)
			{
				idFound = 0;

				for (var x = 0; x < 2048; x++)
				{
					var z = instancePrimary.Entity.GetEntity(x);

					if (z.Name != null && z.Name.ToLower() == Form2.config.autoTarget_Target.ToLower())
					{
						if (z.Status == 1)
						{
							return z.TargetingIndex;
						}
						else
						{
							return 0;
						}
					}
				}
				return 0;
			}
			else
			{
				if (instanceMonitored.Player.Status == 1)
				{
					var target = instanceMonitored.Target.GetTargetInfo();
					var entity = instanceMonitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
					return Convert.ToInt32(entity.TargetID);

				}
				else
				{
					return 0;
				}
			}
		}

		private int GrabGEOTargetID()
		{
			if (Form2.config.specifiedEngageTarget == true && Form2.config.LuopanSpell_Target != string.Empty)
			{
				idFound = 0;

				for (var x = 0; x < 2048; x++)
				{
					var z = instancePrimary.Entity.GetEntity(x);

					if (z.Name != null && z.Name.ToLower() == Form2.config.LuopanSpell_Target.ToLower())
					{
						if (z.Status == 1)
						{
							return z.TargetingIndex;
						}
						else
						{
							return 0;
						}
					}
				}
				return 0;
			}
			else
			{
				if (instanceMonitored.Player.Status == 1)
				{
					var target = instanceMonitored.Target.GetTargetInfo();
					var entity = instanceMonitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
					return Convert.ToInt32(entity.TargetID);

				}
				else
				{
					return 0;
				}
			}
		}

		private int GrabDistance_GEO()
		{
			var checkedName = string.Empty;
			var name1 = string.Empty;

			if (Form2.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(Form2.config.LuopanSpell_Target))
			{
				checkedName = Form2.config.LuopanSpell_Target;
			}
			else
			{
				checkedName = instanceMonitored.Player.Name;
			}

			for (var x = 0; x < 2048; x++)
			{
				var entityGEO = instancePrimary.Entity.GetEntity(x);

				if (!string.IsNullOrEmpty(checkedName) && !string.IsNullOrEmpty(entityGEO.Name))
				{
					name1 = entityGEO.Name;

					if (name1 == checkedName)
					{
						return (int)entityGEO.Distance;
					}
				}
			}

			return 0;
		}

		private void updateInstances_Tick(object sender, EventArgs e)
		{
			if ((instancePrimary != null && instancePrimary.Player.LoginStatus == (int)LoginStatus.Loading) || (instanceMonitored != null && instanceMonitored.Player.LoginStatus == (int)LoginStatus.Loading))
			{
				return;
			}

			var pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

			if (pol.Count() < 1)
			{
			}
			else
			{
				POLID.Items.Clear();
				POLID2.Items.Clear();
				processIds.Items.Clear();

				var selectedPOLID = 0;
				var selectedPOLID2 = 0;

				for (var i = 0; i < pol.Count(); i++)
				{
					POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
					POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
					processIds.Items.Add(pol.ElementAt(i).Id);

					if (instancePrimary != null && instancePrimary.Player.Name != null)
					{
						if (pol.ElementAt(i).MainWindowTitle.ToLower() == instancePrimary.Player.Name.ToLower())
						{
							selectedPOLID = i;
							plLabel.Text = "Selected PL: " + instancePrimary.Player.Name;
							Text = notifyIcon1.Text = instancePrimary.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;
						}
					}

					if (instanceMonitored != null && instanceMonitored.Player.Name != null)
					{
						if (pol.ElementAt(i).MainWindowTitle == instanceMonitored.Player.Name)
						{
							selectedPOLID2 = i;
							monitoredLabel.Text = "Monitored Player: " + instanceMonitored.Player.Name;
						}
					}
				}
				POLID.SelectedIndex = selectedPOLID;
				POLID2.SelectedIndex = selectedPOLID2;
			}
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (FormWindowState.Minimized == WindowState)
			{
				notifyIcon1.Visible = true;
				notifyIcon1.ShowBalloonTip(500);
				Hide();
			}
			else if (FormWindowState.Normal == WindowState)
			{
			}
		}

		private void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Show();
			WindowState = FormWindowState.Normal;
		}

		private void CheckCustomActions_TickAsync(object sender, EventArgs e)
		{
			if (instancePrimary != null && instanceMonitored != null)
			{

				var cmdTime = instanceMonitored.ThirdParty.ConsoleIsNewCommand();

				if (lastCommand != cmdTime)
				{
					lastCommand = cmdTime;

					if (instanceMonitored.ThirdParty.ConsoleGetArg(0) == "cureplease")
					{
						var argCount = instanceMonitored.ThirdParty.ConsoleGetArgCount();

						// 0 = cureplease or cp so ignore
						// 1 = command to run
						// 2 = (if set) PL's name

						if (argCount >= 3)
						{
							if ((instanceMonitored.ThirdParty.ConsoleGetArg(1) == "stop" || instanceMonitored.ThirdParty.ConsoleGetArg(1) == "pause") && instancePrimary.Player.Name == instanceMonitored.ThirdParty.ConsoleGetArg(2))
							{
								pauseButton.Text = "Paused!";
								pauseButton.ForeColor = Color.Red;
								enableActions = false;
								activeBuffs.Clear();
								pauseActions = true;
								song_casting = 0;
								forceSongRecast = true;
								if (Form2.config.FFXIDefaultAutoFollow == false)
								{
									instancePrimary.AutoFollow.IsAutoFollowing = false;
								}
							}
							else if ((instanceMonitored.ThirdParty.ConsoleGetArg(1) == "unpause" || instanceMonitored.ThirdParty.ConsoleGetArg(1) == "start") && instancePrimary.Player.Name.ToLower() == instanceMonitored.ThirdParty.ConsoleGetArg(2).ToLower())
							{
								pauseButton.Text = "Pause";
								pauseButton.ForeColor = Color.Black;
								enableActions = true;
								pauseActions = false;
								song_casting = 0;
								forceSongRecast = true;
							}
							else if ((instanceMonitored.ThirdParty.ConsoleGetArg(1) == "toggle") && instancePrimary.Player.Name.ToLower() == instanceMonitored.ThirdParty.ConsoleGetArg(2).ToLower())
							{
								pauseButton.PerformClick();
							}
							else
							{

							}
						}
						else if (argCount < 3)
						{
							if (instanceMonitored.ThirdParty.ConsoleGetArg(1) == "stop" || instanceMonitored.ThirdParty.ConsoleGetArg(1) == "pause")
							{
								pauseButton.Text = "Paused!";
								pauseButton.ForeColor = Color.Red;
								enableActions = false;
								activeBuffs.Clear();
								pauseActions = true;
								song_casting = 0;
								forceSongRecast = true;
								if (Form2.config.FFXIDefaultAutoFollow == false)
								{
									instancePrimary.AutoFollow.IsAutoFollowing = false;
								}
							}
							else if (instanceMonitored.ThirdParty.ConsoleGetArg(1) == "unpause" || instanceMonitored.ThirdParty.ConsoleGetArg(1) == "start")
							{
								pauseButton.Text = "Pause";
								pauseButton.ForeColor = Color.Black;
								enableActions = true;
								pauseActions = false;
								song_casting = 0;
								forceSongRecast = true;
							}
							else if (instanceMonitored.ThirdParty.ConsoleGetArg(1) == "toggle")
							{
								pauseButton.PerformClick();
							}
							else
							{
							}
						}
						else
						{
							// DO NOTHING
						}
					}
				}
			}
		}

		public async Task Run_BardSongs()
		{
			plBardCount = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b =>
				b == 195 || b == 196 || b == 197 || b == 198 || b == 199 || b == 200 ||
				b == 201 || b == 214 || b == 215 || b == 216 || b == 218 || b == 219 || b == 222).Count();

			if ((Form2.config.enableSinging) && instancePrimary.Player.Status != 33)
			{

				debug_MSG_show = "ORDER: " + song_casting;

				var song_1 = bardSongs.Where(c => c.SongPosition == Form2.config.song1).FirstOrDefault();
				var song_2 = bardSongs.Where(c => c.SongPosition == Form2.config.song2).FirstOrDefault();
				var song_3 = bardSongs.Where(c => c.SongPosition == Form2.config.song3).FirstOrDefault();
				var song_4 = bardSongs.Where(c => c.SongPosition == Form2.config.song4).FirstOrDefault();

				var dummy1_song = bardSongs.Where(c => c.SongPosition == Form2.config.dummy1).FirstOrDefault();
				var dummy2_song = bardSongs.Where(c => c.SongPosition == Form2.config.dummy2).FirstOrDefault();

				// Check the distance of the Monitored player
				var Monitoreddistance = 50;


				var monitoredTarget = instancePrimary.Entity.GetEntity((int)instanceMonitored.Player.TargetID);
				Monitoreddistance = (int)monitoredTarget.Distance;

				var Songs_Possible = 0;

				if (song_1.SongName.ToLower() != "blank")
				{
					Songs_Possible++;
				}
				if (song_2.SongName.ToLower() != "blank")
				{
					Songs_Possible++;
				}
				if (dummy1_song != null && dummy1_song.SongName.ToLower() != "blank")
				{
					Songs_Possible++;
				}
				if (dummy2_song != null && dummy2_song.SongName.ToLower() != "blank")
				{
					Songs_Possible++;
				}

				// List to make it easy to check how many of each buff is needed.
				var SongDataMax = new List<int> { song_1.BuffId, song_2.BuffId, song_3.BuffId, song_4.BuffId };

				// Check Whether e have the songs Currently Up
				var count1_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.BuffId).Count();
				var count2_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.BuffId).Count();
				var count3_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.BuffId).Count();
				var count4_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.BuffId).Count();
				var count5_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.BuffId).Count();
				var count6_type = instancePrimary.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.BuffId).Count();

				var MON_count1_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.BuffId).Count();
				var MON_count2_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.BuffId).Count();
				var MON_count3_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.BuffId).Count();
				var MON_count4_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.BuffId).Count();
				var MON_count5_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.BuffId).Count();
				var MON_count6_type = instanceMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.BuffId).Count();


				if (forceSongRecast == true) { song_casting = 0; forceSongRecast = false; }


				// SONG NUMBER #4
				if (song_casting == 3 && plBardCount >= 3 && song_4.SongName.ToLower() != "blank" && count6_type < SongDataMax.Where(c => c == song_4.BuffId).Count() && lastSongCastedName != song_4.SongName)
				{
					if (plBardCount == 3)
					{
						if (SpellReadyToCast(dummy2_song.SongName) && (HasAcquiredSpell(dummy2_song.SongName)) && HasRequiredJobLevel(dummy2_song.SongName) == true)
						{
							await CastSpell("<me>", dummy2_song.SongName);
						}
					}
					else
					{
						if (SpellReadyToCast(song_4.SongName) && (HasAcquiredSpell(song_4.SongName)) && HasRequiredJobLevel(song_4.SongName) == true)
						{
							await CastSpell("<me>", song_4.SongName);
							lastSongCastedName = song_4.SongName;
							lastSongCast[0] = DateTime.Now;
							playerSong4[0] = DateTime.Now;
							song_casting = 0;
						}
					}

				}
				else if (song_casting == 3 && song_4.SongName.ToLower() != "blank" && count6_type >= SongDataMax.Where(c => c == song_4.BuffId).Count())
				{
					song_casting = 0;
				}


				// SONG NUMBER #3
				else if (song_casting == 2 && plBardCount >= 2 && song_3.SongName.ToLower() != "blank" && count4_type < SongDataMax.Where(c => c == song_3.BuffId).Count() && lastSongCastedName != song_3.SongName)
				{
					if (plBardCount == 2)
					{
						if (SpellReadyToCast(dummy1_song.SongName) && (HasAcquiredSpell(dummy1_song.SongName)) && HasRequiredJobLevel(dummy1_song.SongName) == true)
						{
							await CastSpell("<me>", dummy1_song.SongName);
						}
					}
					else
					{
						if (SpellReadyToCast(song_3.SongName) && (HasAcquiredSpell(song_3.SongName)) && HasRequiredJobLevel(song_3.SongName) == true)
						{
							await CastSpell("<me>", song_3.SongName);
							lastSongCastedName = song_3.SongName;
							lastSongCast[0] = DateTime.Now;
							playerSong3[0] = DateTime.Now;
							song_casting = 3;
						}
					}
				}
				else if (song_casting == 2 && song_3.SongName.ToLower() != "blank" && count4_type >= SongDataMax.Where(c => c == song_3.BuffId).Count())
				{
					song_casting = 3;
				}


				// SONG NUMBER #2
				else if (song_casting == 1 && song_2.SongName.ToLower() != "blank" && count2_type < SongDataMax.Where(c => c == song_2.BuffId).Count() && lastSongCastedName != song_4.SongName)
				{
					if (SpellReadyToCast(song_2.SongName) && (HasAcquiredSpell(song_2.SongName)) && HasRequiredJobLevel(song_2.SongName) == true)
					{
						await CastSpell("<me>", song_2.SongName);
						lastSongCastedName = song_2.SongName;
						lastSongCast[0] = DateTime.Now;
						playerSong2[0] = DateTime.Now;
						song_casting = 2;
					}
				}
				else if (song_casting == 1 && song_2.SongName.ToLower() != "blank" && count2_type >= SongDataMax.Where(c => c == song_2.BuffId).Count())
				{
					song_casting = 2;
				}

				// SONG NUMBER #1
				else if ((song_casting == 0) && song_1.SongName.ToLower() != "blank" && count1_type < SongDataMax.Where(c => c == song_1.BuffId).Count() && lastSongCastedName != song_4.SongName)
				{
					if (SpellReadyToCast(song_1.SongName) && (HasAcquiredSpell(song_1.SongName)) && HasRequiredJobLevel(song_1.SongName) == true)
					{
						await CastSpell("<me>", song_1.SongName);
						lastSongCastedName = song_1.SongName;
						lastSongCast[0] = DateTime.Now;
						playerSong1[0] = DateTime.Now;
						song_casting = 1;
					}

				}
				else if (song_casting == 0 && song_2.SongName.ToLower() != "blank" && count1_type >= SongDataMax.Where(c => c == song_1.BuffId).Count())
				{
					song_casting = 1;
				}


				// ONCE ALL SONGS HAVE BEEN CAST ONLY RECAST THEM WHEN THEY MEET THE THRESHOLD SET ON SONG RECAST AND BLOCK IF IT'S SET AT LAUNCH DEFAULTS
				if (playerSong1[0] != defaultTime && playerSong1_Span[0].Minutes >= Form2.config.recastSongTime)
				{
					if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
					{
						if (SpellReadyToCast(song_1.SongName) && (HasAcquiredSpell(song_1.SongName)) && HasRequiredJobLevel(song_1.SongName) == true)
						{
							await CastSpell("<me>", song_1.SongName);
							playerSong1[0] = DateTime.Now;
							song_casting = 0;
						}
					}
				}
				else if (playerSong2[0] != defaultTime && playerSong2_Span[0].Minutes >= Form2.config.recastSongTime)
				{
					if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
					{
						if (SpellReadyToCast(song_2.SongName) && (HasAcquiredSpell(song_2.SongName)) && HasRequiredJobLevel(song_2.SongName) == true)
						{
							await CastSpell("<me>", song_2.SongName);
							playerSong2[0] = DateTime.Now;
							song_casting = 0;
						}
					}
				}
				else if (playerSong3[0] != defaultTime && playerSong3_Span[0].Minutes >= Form2.config.recastSongTime)
				{
					if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
					{
						if (SpellReadyToCast(song_3.SongName) && (HasAcquiredSpell(song_3.SongName)) && HasRequiredJobLevel(song_3.SongName) == true)
						{
							await CastSpell("<me>", song_3.SongName);
							playerSong3[0] = DateTime.Now;
							song_casting = 0;
						}
					}
				}
				else if (playerSong4[0] != defaultTime && playerSong4_Span[0].Minutes >= Form2.config.recastSongTime)
				{
					if ((Form2.config.SongsOnlyWhenNear && Monitoreddistance < 10) || Form2.config.SongsOnlyWhenNear == false)
					{
						if (SpellReadyToCast(song_4.SongName) && (HasAcquiredSpell(song_4.SongName)) && HasRequiredJobLevel(song_4.SongName) == true)
						{
							await CastSpell("<me>", song_4.SongName);
							playerSong4[0] = DateTime.Now;
							song_casting = 0;
						}
					}
				}


			}
		}

		private void Follow_BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{

			// MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
			if (instancePrimary != null && instanceMonitored != null && !string.IsNullOrEmpty(Form2.config.autoFollowName) && !pauseActions)
			{

				if (Form2.config.FFXIDefaultAutoFollow != true)
				{
					// CANCEL ALL PREVIOUS FOLLOW ACTIONS
					instancePrimary.AutoFollow.IsAutoFollowing = false;
					curePlease_autofollow = false;
					stuckWarning = false;
					stuckCount = 0;
				}

				// RUN THE FUNCTION TO GRAB THE ID OF THE FOLLOW TARGET THIS ALSO MAKES SURE THEY ARE IN RANGE TO FOLLOW
				var followersTargetID = followID();

				// If the FOLLOWER'S ID is NOT -1 THEN THEY WERE LOCATED SO CONTINUE THE CHECKS
				if (followersTargetID != -1)
				{
					// GRAB THE FOLLOW TARGETS ENTITY TABLE TO CHECK DISTANCE ETC
					var followTarget = instancePrimary.Entity.GetEntity(followersTargetID);

					if (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance && curePlease_autofollow == false)
					{
						// THE DISTANCE IS GREATER THAN REQUIRED SO IF AUTOFOLLOW IS NOT ACTIVE THEN DEPENDING ON THE TYPE, FOLLOW

						// SQUARE ENIX FINAL FANTASY XI DEFAULT AUTO FOLLOW
						if (Form2.config.FFXIDefaultAutoFollow == true && instancePrimary.AutoFollow.IsAutoFollowing != true)
						{
							// IF THE CURRENT TARGET IS NOT THE FOLLOWERS TARGET ID THEN CHANGE THAT NOW
							if (instancePrimary.Target.GetTargetInfo().TargetIndex != followersTargetID)
							{
								// FIRST REMOVE THE CURRENT TARGET
								instancePrimary.Target.SetTarget(0);
								// NOW SET THE NEXT TARGET AFTER A WAIT
								Thread.Sleep(TimeSpan.FromSeconds(0.1));
								instancePrimary.Target.SetTarget(followersTargetID);
							}
							// IF THE TARGET IS CORRECT BUT YOU'RE NOT LOCKED ON THEN DO SO NOW
							else if (instancePrimary.Target.GetTargetInfo().TargetIndex == followersTargetID && !instancePrimary.Target.GetTargetInfo().LockedOn)
							{
								instancePrimary.ThirdParty.SendString("/lockon <t>");
							}
							// EVERYTHING SHOULD BE FINE SO FOLLOW THEM
							else
							{
								Thread.Sleep(TimeSpan.FromSeconds(0.1));
								instancePrimary.ThirdParty.SendString("/follow");
							}
						}
						// ELITEAPI'S IMPROVED AUTO FOLLOW
						else if (Form2.config.FFXIDefaultAutoFollow != true && instancePrimary.AutoFollow.IsAutoFollowing != true)
						{
							// IF YOU ARE TOO FAR TO FOLLOW THEN STOP AND IF ENABLED WARN THE MONITORED PLAYER
							if (Form2.config.autoFollow_Warning == true && Math.Truncate(followTarget.Distance) >= 40 && instanceMonitored.Player.Name != instancePrimary.Player.Name && followWarning == 0)
							{
								var createdTell = "/tell " + instanceMonitored.Player.Name + " " + "You're too far to follow.";
								instancePrimary.ThirdParty.SendString(createdTell);
								followWarning = 1;
								Thread.Sleep(TimeSpan.FromSeconds(0.3));
							}
							else if (Math.Truncate(followTarget.Distance) <= 40)
							{
								// ONLY TARGET AND BEGIN FOLLOW IF TARGET IS AT THE DEFINED DISTANCE
								if (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance && Math.Truncate(followTarget.Distance) <= 48)
								{
									followWarning = 0;

									// Cancel current target this is to make sure the character is not locked
									// on and therefore unable to move freely. Wait 5ms just to allow it to work

									instancePrimary.Target.SetTarget(0);
									Thread.Sleep(TimeSpan.FromSeconds(0.1));

									float Target_X;
									float Target_Y;
									float Target_Z;

									var FollowerTargetEntity = instancePrimary.Entity.GetEntity(followersTargetID);

									if (!string.IsNullOrEmpty(FollowerTargetEntity.Name))
									{
										while (Math.Truncate(followTarget.Distance) >= (int)Form2.config.autoFollowDistance)
										{

											var Player_X = instancePrimary.Player.X;
											var Player_Y = instancePrimary.Player.Y;
											var Player_Z = instancePrimary.Player.Z;


											if (FollowerTargetEntity.Name == instanceMonitored.Player.Name)
											{
												Target_X = instanceMonitored.Player.X;
												Target_Y = instanceMonitored.Player.Y;
												Target_Z = instanceMonitored.Player.Z;
												var dX = Target_X - Player_X;
												var dY = Target_Y - Player_Y;
												var dZ = Target_Z - Player_Z;

												instancePrimary.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

												instancePrimary.AutoFollow.IsAutoFollowing = true;
												curePlease_autofollow = true;


												lastPlX = instancePrimary.Player.X;
												lastPlY = instancePrimary.Player.Y;
												lastPlZ = instancePrimary.Player.Z;

												Thread.Sleep(TimeSpan.FromSeconds(0.1));
											}
											else
											{
												Target_X = FollowerTargetEntity.X;
												Target_Y = FollowerTargetEntity.Y;
												Target_Z = FollowerTargetEntity.Z;

												var dX = Target_X - Player_X;
												var dY = Target_Y - Player_Y;
												var dZ = Target_Z - Player_Z;


												instancePrimary.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

												instancePrimary.AutoFollow.IsAutoFollowing = true;
												curePlease_autofollow = true;


												lastPlX = instancePrimary.Player.X;
												lastPlY = instancePrimary.Player.Y;
												lastPlZ = instancePrimary.Player.Z;

												Thread.Sleep(TimeSpan.FromSeconds(0.1));
											}

											// STUCK CHECKER
											var genX = lastPlX - instancePrimary.Player.X;
											var genY = lastPlY - instancePrimary.Player.Y;
											var genZ = lastPlZ - instancePrimary.Player.Z;

											var distance = Math.Sqrt(genX * genX + genY * genY + genZ * genZ);

											if (distance < .1)
											{
												stuckCount = stuckCount + 1;
												if (Form2.config.autoFollow_Warning == true && stuckWarning != true && FollowerTargetEntity.Name == instanceMonitored.Player.Name && stuckCount == 10)
												{
													var createdTell = "/tell " + instanceMonitored.Player.Name + " " + "I appear to be stuck.";
													instancePrimary.ThirdParty.SendString(createdTell);
													stuckWarning = true;
												}
											}
										}

										instancePrimary.AutoFollow.IsAutoFollowing = false;
										curePlease_autofollow = false;
										stuckWarning = false;
										stuckCount = 0;
									}
								}
							}
							else
							{
								// YOU ARE NOT AT NOR FURTHER THAN THE DISTANCE REQUIRED SO CANCEL ELITEAPI AUTOFOLLOW
								curePlease_autofollow = false;
							}
						}
					}
				}
			}

			Thread.Sleep(TimeSpan.FromSeconds(1));

		}

		private void Follow_BGW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			Follow_BGW.RunWorkerAsync();
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			Opacity = trackBar1.Value * 0.01;
		}

		private void OptionsButton_Click(object sender, EventArgs e)
		{
			if ((settings == null) || (settings.IsDisposed))
			{
				settings = new Form2();
			}
			settings.Show();

		}

		private void ChatLogButton_Click(object sender, EventArgs e)
		{
			var form4 = new Form4(this);

			if (instancePrimary != null)
			{
				form4.Show();
			}
		}

		private void PartyBuffsButton_Click(object sender, EventArgs e)
		{
			var PartyBuffs = new PartyBuffs(this);
			if (instancePrimary != null)
			{
				PartyBuffs.Show();
			}
		}

		private void AboutButton_Click(object sender, EventArgs e)
		{
			new Form3().Show();
		}

		private void AddonReader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while (true)
			{
				if (Form2.config.EnableAddOn == true && pauseActions == false && instanceMonitored != null && instancePrimary != null)
				{
					string received_data;
					byte[] receive_byte_array;
					try
					{
						while (true)
						{
							receive_byte_array = listener.Receive(ref endpoint);
							received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
							var commands = received_data.Split('_');

							if (commands[1] == "casting" && commands.Count() == 3 && Form2.config.trackCastingPackets == true)
							{
								if (commands[2] == "blocked")
								{
									Invoke((MethodInvoker)(() =>
									{
										castingLockLabel.Text = "PACKET: Casting is LOCKED";
									}));
								}
								else if (commands[2] == "interrupted")
								{
									castTokenSource?.Cancel();
									Invoke((MethodInvoker)(() =>
									{
										castingLockLabel.Text = "PACKET: Casting is INTERRUPTED";
									}));
								}
								else if (commands[2] == "finished")
								{
									castTokenSource?.Cancel();
									Invoke((MethodInvoker)(() =>
									{
										castingLockLabel.Text = "PACKET: Casting is soon to be AVAILABLE!";
									}));
								}
							}
							else if (commands[1] == "confirmed")
							{
								AddOnStatus.BackColor = Color.ForestGreen;
							}
							else if (commands[1] == "command")
							{
								if (commands[2] == "start" || commands[2] == "unpause")
								{
									Invoke((MethodInvoker)(() =>
									{
										pauseButton.Text = "Pause";
										pauseButton.ForeColor = Color.Black;
										enableActions = true;
										pauseActions = false;
										song_casting = 0;
										forceSongRecast = true;
									}));
								}
								if (commands[2] == "stop" || commands[2] == "pause")
								{
									Invoke((MethodInvoker)(() =>
									{

										pauseButton.Text = "Paused!";
										pauseButton.ForeColor = Color.Red;
										enableActions = false;
										activeBuffs.Clear();
										pauseActions = true;
										if (Form2.config.FFXIDefaultAutoFollow == false)
										{
											instancePrimary.AutoFollow.IsAutoFollowing = false;
										}

									}));
								}
								if (commands[2] == "toggle")
								{
									Invoke((MethodInvoker)(() =>
									{
										pauseButton.PerformClick();
									}));
								}
							}
							else if (commands[1] == "buffs" && commands.Count() == 4)
							{
								if (casting.Wait(5000))
								{
									try
									{
										activeBuffs.RemoveAll(buf => buf.CharacterName == commands[2]);
										activeBuffs.Add(new BuffStorage
										{
											CharacterName = commands[2],
											CharacterBuffs = commands[3]
										});
									}
									finally
									{
										casting.Release();
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Error processing addon data.");
						Debug.WriteLine(ex.ToString());
					}
				}

				Thread.Sleep(TimeSpan.FromSeconds(0.3));
			}
		}

		private void AddonReader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			AddonReader.RunWorkerAsync();
		}

		private void FullCircle_Timer_Tick(object sender, EventArgs e)
		{

			if (instancePrimary.Player.Pet.HealthPercent >= 1)
			{
				var PetsIndex = instancePrimary.Player.PetIndex;

				if (Form2.config.Fullcircle_GEOTarget == true && Form2.config.LuopanSpell_Target != "")
				{
					var PetsEntity = instancePrimary.Entity.GetEntity(PetsIndex);

					var FullCircle_CharID = 0;

					for (var x = 0; x < 2048; x++)
					{
						var entity = instancePrimary.Entity.GetEntity(x);

						if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.LuopanSpell_Target.ToLower()))
						{
							FullCircle_CharID = Convert.ToInt32(entity.TargetID);
							break;
						}
					}

					if (FullCircle_CharID != 0)
					{
						var FullCircleEntity = instancePrimary.Entity.GetEntity(FullCircle_CharID);

						var fX = PetsEntity.X - FullCircleEntity.X;
						var fY = PetsEntity.Y - FullCircleEntity.Y;
						var fZ = PetsEntity.Z - FullCircleEntity.Z;

						var generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

						if (generatedDistance >= 10)
						{
							instancePrimary.ThirdParty.SendString("/ja \"Full Circle\" <me>");
						}
					}

				}
				else if (Form2.config.Fullcircle_GEOTarget == false && instanceMonitored.Player.Status == 1)
				{
					var SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);
					if (Form2.config.Fullcircle_DisableEnemy != true || (Form2.config.Fullcircle_DisableEnemy == true && instancePrimary.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
					{
						var PetsEntity = instanceMonitored.Entity.GetEntity(PetsIndex);

						if (PetsEntity.Distance >= 10 && PetsEntity.Distance != 0 && CanUseJobAbility("Full Circle"))
						{
							instancePrimary.ThirdParty.SendString("/ja \"Full Circle\" <me>");
						}
					}
				}
			}

			FullCircle_Timer.Enabled = false;
		}

		private void AddOnStatus_Click(object sender, EventArgs e)
		{
			if (instanceMonitored != null && instancePrimary != null)
			{
				if (hookMode == "Ashita")
				{
					instancePrimary.ThirdParty.SendString(string.Format("/cpaddon verify"));
				}
				else if (hookMode == "Windower")
				{
					instancePrimary.ThirdParty.SendString(string.Format("//cpaddon verify"));
				}
			}
		}

		private CancellationTokenSource castTokenSource;

		private async Task CastSpellInternal(CancellationToken cancellationToken)
		{
			if (!(await casting.WaitAsync(15000)))
			{
				// Prefer to terminate rather than hang...
				throw new InvalidOperationException();
			}

			try
			{
				Invoke(new Action(() =>
				{
					castingLockLabel.Text = "Casting is LOCKED";
					currentAction.Text = spellCommand;
				}));

				instancePrimary.ThirdParty.SendString(spellCommand);
				var timer = Stopwatch.StartNew();

				// There is a small delay, depending on latency and game lag, between when
				// we send the command and the actual spell casting begins. If we check the 
				// cast percent before casting begins, it will return 100% which will short-
				// circuit our mechanism. To prevent this, we wait until the cast percent is
				// something other than 0 or 1 (e.g. 0.12) before entering the wait loop.

				while (
					instancePrimary.CastBar.Percent == 0 ||
					instancePrimary.CastBar.Percent == 1)
				{
					// But sometimes the command fails. This can happen if the user manually casts
					// a spell or uses a JA or item at the same time, or for other unexpected reasons. 
					// If we wait for the spell indefinitely, our program will hang. To prevent this, 
					// we'll timeout after N seconds.

					if (timer.ElapsedMilliseconds >= 5000)
					{
						break;
					}

					Debug.WriteLine("Waiting for casting to start...");
					await Task.Delay(50);
				}

				// At this point, we know the game has started casting the spell we told it
				// to, so we can monitor the cast percent until it hits 1 (spell is 100% done).
				// Alternately, we'll stop if the cancel token is set by the in-game addon
				// due to fastcast / quick magic or the spell being interrupted.

				var attempts = 0;
				var percent = 0f;

				timer.Restart();
				while (timer.ElapsedMilliseconds < 12000)
				{
					// Check casting percent...
					percent = instancePrimary.CastBar.Percent;
					Debug.WriteLine($"Casting percent: {percent}; attempt {++attempts}");
					await Task.Delay(200);

					// Stop if spell casting is complete.
					if (percent >= 1 && timer.ElapsedMilliseconds >= 3000)
					{
						break;
					}

					// Avoid "unable to..." errors due to animation lag
					if (cancellationToken.IsCancellationRequested)
					{
						var minCastTime = spellCastTime * 0.85;
						var minDelay = Math.Max(3250, minCastTime);
						if (timer.ElapsedMilliseconds >= minDelay)
						{
							Debug.WriteLine("Spell cast finished early.");
							Debug.WriteLine($"casting time: {minCastTime}");
							Debug.WriteLine($"minimum delay: {minDelay}");
							break;
						}
					}
				}
			}
			finally
			{
				spellCommand = "";
				casting.Release();
				Debug.WriteLine("Completed casting...");

				Invoke(new Action(() =>
				{
					castingLockLabel.Text = "Casting is UNLOCKED";
					currentAction.Text = "";
				}));
			}
		}

		private void CustomCommand_Tracker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
		}

		private void CustomCommand_Tracker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			CustomCommand_Tracker.RunWorkerAsync();
		}

		private IPEndPoint GetDynamicEndpoint()
		{
			var tcp = new TcpListener(IPAddress.Loopback, 0);
			tcp.Start();

			var ep = tcp.LocalEndpoint as IPEndPoint;
			tcp.Stop();
			return ep;
		}

		private void StartActionLoop()
		{
			Debug.WriteLine("Starting action loop...");

			var actions = new Thread(async () =>
			{
				while (true)
				{
					if (enableActions)
					{
						Debug.WriteLine("Running action loop...");
						await RunActionLoop();
					}
					else
					{
						Debug.WriteLine("Skipping action loop...");
					}

					await Task.Delay(250);
				}
			})
			{
				IsBackground = true
			};
			actions.Start();
		}

		private void LoadAddonAshita()
		{
			instancePrimary.ThirdParty.SendString("/addon unload CurePlease_addon");
			Thread.Sleep(300);

			instancePrimary.ThirdParty.SendString("/addon load CurePlease_addon");
			Thread.Sleep(1500);

			instancePrimary.ThirdParty.SendString("/cpaddon settings " + endpoint.Address + " " + endpoint.Port);
			Thread.Sleep(100);

			instancePrimary.ThirdParty.SendString("/cpaddon verify");
			if (Form2.config.enableHotKeys)
			{
				instancePrimary.ThirdParty.SendString("/bind ^!F1 /cureplease toggle");
				instancePrimary.ThirdParty.SendString("/bind ^!F2 /cureplease start");
				instancePrimary.ThirdParty.SendString("/bind ^!F3 /cureplease pause");
			}
		}

		private void LoadAddonWindower()
		{
			instancePrimary.ThirdParty.SendString("//lua unload CurePlease_addon");
			Thread.Sleep(300);

			instancePrimary.ThirdParty.SendString("//lua load CurePlease_addon");
			Thread.Sleep(1500);

			instancePrimary.ThirdParty.SendString("//cpaddon settings " + endpoint.Address + " " + endpoint.Port);
			Thread.Sleep(100);

			instancePrimary.ThirdParty.SendString("//cpaddon verify");
			if (Form2.config.enableHotKeys)
			{
				instancePrimary.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
				instancePrimary.ThirdParty.SendString("//bind ^!F2 cureplease start");
				instancePrimary.ThirdParty.SendString("//bind ^!F3 cureplease pause");
			}
		}

		private async Task RunActionLoop()
		{
			string[] shell_spells = { "Shell", "Shell II", "Shell III", "Shell IV", "Shell V" };
			string[] protect_spells = { "Protect", "Protect II", "Protect III", "Protect IV", "Protect V" };

			// skip if player instance not set
			if (instancePrimary == null || instanceMonitored == null)
			{
				return;
			}

			// skip if player not logged in
			if (instancePrimary.Player.LoginStatus != (int)LoginStatus.LoggedIn || instanceMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
			{
				return;
			}

			// skip if still casting
			if (casting.CurrentCount < 1)
			{
				return;
			}

			currentTime = DateTime.Now;

			// Calculate time since haste was cast on particular player
			playerHasteSpan[0] = currentTime.Subtract(playerHaste[0]);
			playerHasteSpan[1] = currentTime.Subtract(playerHaste[1]);
			playerHasteSpan[2] = currentTime.Subtract(playerHaste[2]);
			playerHasteSpan[3] = currentTime.Subtract(playerHaste[3]);
			playerHasteSpan[4] = currentTime.Subtract(playerHaste[4]);
			playerHasteSpan[5] = currentTime.Subtract(playerHaste[5]);
			playerHasteSpan[6] = currentTime.Subtract(playerHaste[6]);
			playerHasteSpan[7] = currentTime.Subtract(playerHaste[7]);
			playerHasteSpan[8] = currentTime.Subtract(playerHaste[8]);
			playerHasteSpan[9] = currentTime.Subtract(playerHaste[9]);
			playerHasteSpan[10] = currentTime.Subtract(playerHaste[10]);
			playerHasteSpan[11] = currentTime.Subtract(playerHaste[11]);
			playerHasteSpan[12] = currentTime.Subtract(playerHaste[12]);
			playerHasteSpan[13] = currentTime.Subtract(playerHaste[13]);
			playerHasteSpan[14] = currentTime.Subtract(playerHaste[14]);
			playerHasteSpan[15] = currentTime.Subtract(playerHaste[15]);
			playerHasteSpan[16] = currentTime.Subtract(playerHaste[16]);
			playerHasteSpan[17] = currentTime.Subtract(playerHaste[17]);

			playerHaste_IISpan[0] = currentTime.Subtract(playerHaste_II[0]);
			playerHaste_IISpan[1] = currentTime.Subtract(playerHaste_II[1]);
			playerHaste_IISpan[2] = currentTime.Subtract(playerHaste_II[2]);
			playerHaste_IISpan[3] = currentTime.Subtract(playerHaste_II[3]);
			playerHaste_IISpan[4] = currentTime.Subtract(playerHaste_II[4]);
			playerHaste_IISpan[5] = currentTime.Subtract(playerHaste_II[5]);
			playerHaste_IISpan[6] = currentTime.Subtract(playerHaste_II[6]);
			playerHaste_IISpan[7] = currentTime.Subtract(playerHaste_II[7]);
			playerHaste_IISpan[8] = currentTime.Subtract(playerHaste_II[8]);
			playerHaste_IISpan[9] = currentTime.Subtract(playerHaste_II[9]);
			playerHaste_IISpan[10] = currentTime.Subtract(playerHaste_II[10]);
			playerHaste_IISpan[11] = currentTime.Subtract(playerHaste_II[11]);
			playerHaste_IISpan[12] = currentTime.Subtract(playerHaste_II[12]);
			playerHaste_IISpan[13] = currentTime.Subtract(playerHaste_II[13]);
			playerHaste_IISpan[14] = currentTime.Subtract(playerHaste_II[14]);
			playerHaste_IISpan[15] = currentTime.Subtract(playerHaste_II[15]);
			playerHaste_IISpan[16] = currentTime.Subtract(playerHaste_II[16]);
			playerHaste_IISpan[17] = currentTime.Subtract(playerHaste_II[17]);

			playerFlurrySpan[0] = currentTime.Subtract(playerFlurry[0]);
			playerFlurrySpan[1] = currentTime.Subtract(playerFlurry[1]);
			playerFlurrySpan[2] = currentTime.Subtract(playerFlurry[2]);
			playerFlurrySpan[3] = currentTime.Subtract(playerFlurry[3]);
			playerFlurrySpan[4] = currentTime.Subtract(playerFlurry[4]);
			playerFlurrySpan[5] = currentTime.Subtract(playerFlurry[5]);
			playerFlurrySpan[6] = currentTime.Subtract(playerFlurry[6]);
			playerFlurrySpan[7] = currentTime.Subtract(playerFlurry[7]);
			playerFlurrySpan[8] = currentTime.Subtract(playerFlurry[8]);
			playerFlurrySpan[9] = currentTime.Subtract(playerFlurry[9]);
			playerFlurrySpan[10] = currentTime.Subtract(playerFlurry[10]);
			playerFlurrySpan[11] = currentTime.Subtract(playerFlurry[11]);
			playerFlurrySpan[12] = currentTime.Subtract(playerFlurry[12]);
			playerFlurrySpan[13] = currentTime.Subtract(playerFlurry[13]);
			playerFlurrySpan[14] = currentTime.Subtract(playerFlurry[14]);
			playerFlurrySpan[15] = currentTime.Subtract(playerFlurry[15]);
			playerFlurrySpan[16] = currentTime.Subtract(playerFlurry[16]);
			playerFlurrySpan[17] = currentTime.Subtract(playerFlurry[17]);

			playerFlurry_IISpan[0] = currentTime.Subtract(playerFlurry_II[0]);
			playerFlurry_IISpan[1] = currentTime.Subtract(playerFlurry_II[1]);
			playerFlurry_IISpan[2] = currentTime.Subtract(playerFlurry_II[2]);
			playerFlurry_IISpan[3] = currentTime.Subtract(playerFlurry_II[3]);
			playerFlurry_IISpan[4] = currentTime.Subtract(playerFlurry_II[4]);
			playerFlurry_IISpan[5] = currentTime.Subtract(playerFlurry_II[5]);
			playerFlurry_IISpan[6] = currentTime.Subtract(playerFlurry_II[6]);
			playerFlurry_IISpan[7] = currentTime.Subtract(playerFlurry_II[7]);
			playerFlurry_IISpan[8] = currentTime.Subtract(playerFlurry_II[8]);
			playerFlurry_IISpan[9] = currentTime.Subtract(playerFlurry_II[9]);
			playerFlurry_IISpan[10] = currentTime.Subtract(playerFlurry_II[10]);
			playerFlurry_IISpan[11] = currentTime.Subtract(playerFlurry_II[11]);
			playerFlurry_IISpan[12] = currentTime.Subtract(playerFlurry_II[12]);
			playerFlurry_IISpan[13] = currentTime.Subtract(playerFlurry_II[13]);
			playerFlurry_IISpan[14] = currentTime.Subtract(playerFlurry_II[14]);
			playerFlurry_IISpan[15] = currentTime.Subtract(playerFlurry_II[15]);
			playerFlurry_IISpan[16] = currentTime.Subtract(playerFlurry_II[16]);
			playerFlurry_IISpan[17] = currentTime.Subtract(playerFlurry_II[17]);

			// Calculate time since protect was cast on particular player
			playerProtect_Span[0] = currentTime.Subtract(playerProtect[0]);
			playerProtect_Span[1] = currentTime.Subtract(playerProtect[1]);
			playerProtect_Span[2] = currentTime.Subtract(playerProtect[2]);
			playerProtect_Span[3] = currentTime.Subtract(playerProtect[3]);
			playerProtect_Span[4] = currentTime.Subtract(playerProtect[4]);
			playerProtect_Span[5] = currentTime.Subtract(playerProtect[5]);
			playerProtect_Span[6] = currentTime.Subtract(playerProtect[6]);
			playerProtect_Span[7] = currentTime.Subtract(playerProtect[7]);
			playerProtect_Span[8] = currentTime.Subtract(playerProtect[8]);
			playerProtect_Span[9] = currentTime.Subtract(playerProtect[9]);
			playerProtect_Span[10] = currentTime.Subtract(playerProtect[10]);
			playerProtect_Span[11] = currentTime.Subtract(playerProtect[11]);
			playerProtect_Span[12] = currentTime.Subtract(playerProtect[12]);
			playerProtect_Span[13] = currentTime.Subtract(playerProtect[13]);
			playerProtect_Span[14] = currentTime.Subtract(playerProtect[14]);
			playerProtect_Span[15] = currentTime.Subtract(playerProtect[15]);
			playerProtect_Span[16] = currentTime.Subtract(playerProtect[16]);
			playerProtect_Span[17] = currentTime.Subtract(playerProtect[17]);

			// Calculate time since Stormspell was cast on particular player
			playerStormspellSpan[0] = currentTime.Subtract(playerStormspell[0]);
			playerStormspellSpan[1] = currentTime.Subtract(playerStormspell[1]);
			playerStormspellSpan[2] = currentTime.Subtract(playerStormspell[2]);
			playerStormspellSpan[3] = currentTime.Subtract(playerStormspell[3]);
			playerStormspellSpan[4] = currentTime.Subtract(playerStormspell[4]);
			playerStormspellSpan[5] = currentTime.Subtract(playerStormspell[5]);
			playerStormspellSpan[6] = currentTime.Subtract(playerStormspell[6]);
			playerStormspellSpan[7] = currentTime.Subtract(playerStormspell[7]);
			playerStormspellSpan[8] = currentTime.Subtract(playerStormspell[8]);
			playerStormspellSpan[9] = currentTime.Subtract(playerStormspell[9]);
			playerStormspellSpan[10] = currentTime.Subtract(playerStormspell[10]);
			playerStormspellSpan[11] = currentTime.Subtract(playerStormspell[11]);
			playerStormspellSpan[12] = currentTime.Subtract(playerStormspell[12]);
			playerStormspellSpan[13] = currentTime.Subtract(playerStormspell[13]);
			playerStormspellSpan[14] = currentTime.Subtract(playerStormspell[14]);
			playerStormspellSpan[15] = currentTime.Subtract(playerStormspell[15]);
			playerStormspellSpan[16] = currentTime.Subtract(playerStormspell[16]);
			playerStormspellSpan[17] = currentTime.Subtract(playerStormspell[17]);

			// Calculate time since shell was cast on particular player
			playerShell_Span[0] = currentTime.Subtract(playerShell[0]);
			playerShell_Span[1] = currentTime.Subtract(playerShell[1]);
			playerShell_Span[2] = currentTime.Subtract(playerShell[2]);
			playerShell_Span[3] = currentTime.Subtract(playerShell[3]);
			playerShell_Span[4] = currentTime.Subtract(playerShell[4]);
			playerShell_Span[5] = currentTime.Subtract(playerShell[5]);
			playerShell_Span[6] = currentTime.Subtract(playerShell[6]);
			playerShell_Span[7] = currentTime.Subtract(playerShell[7]);
			playerShell_Span[8] = currentTime.Subtract(playerShell[8]);
			playerShell_Span[9] = currentTime.Subtract(playerShell[9]);
			playerShell_Span[10] = currentTime.Subtract(playerShell[10]);
			playerShell_Span[11] = currentTime.Subtract(playerShell[11]);
			playerShell_Span[12] = currentTime.Subtract(playerShell[12]);
			playerShell_Span[13] = currentTime.Subtract(playerShell[13]);
			playerShell_Span[14] = currentTime.Subtract(playerShell[14]);
			playerShell_Span[15] = currentTime.Subtract(playerShell[15]);
			playerShell_Span[16] = currentTime.Subtract(playerShell[16]);
			playerShell_Span[17] = currentTime.Subtract(playerShell[17]);

			// Calculate time since phalanx II was cast on particular player
			playerPhalanx_IISpan[0] = currentTime.Subtract(playerPhalanx_II[0]);
			playerPhalanx_IISpan[1] = currentTime.Subtract(playerPhalanx_II[1]);
			playerPhalanx_IISpan[2] = currentTime.Subtract(playerPhalanx_II[2]);
			playerPhalanx_IISpan[3] = currentTime.Subtract(playerPhalanx_II[3]);
			playerPhalanx_IISpan[4] = currentTime.Subtract(playerPhalanx_II[4]);
			playerPhalanx_IISpan[5] = currentTime.Subtract(playerPhalanx_II[5]);

			// Calculate time since regen was cast on particular player
			playerRegen_Span[0] = currentTime.Subtract(playerRegen[0]);
			playerRegen_Span[1] = currentTime.Subtract(playerRegen[1]);
			playerRegen_Span[2] = currentTime.Subtract(playerRegen[2]);
			playerRegen_Span[3] = currentTime.Subtract(playerRegen[3]);
			playerRegen_Span[4] = currentTime.Subtract(playerRegen[4]);
			playerRegen_Span[5] = currentTime.Subtract(playerRegen[5]);

			// Calculate time since Refresh was cast on particular player
			playerRefresh_Span[0] = currentTime.Subtract(playerRefresh[0]);
			playerRefresh_Span[1] = currentTime.Subtract(playerRefresh[1]);
			playerRefresh_Span[2] = currentTime.Subtract(playerRefresh[2]);
			playerRefresh_Span[3] = currentTime.Subtract(playerRefresh[3]);
			playerRefresh_Span[4] = currentTime.Subtract(playerRefresh[4]);
			playerRefresh_Span[5] = currentTime.Subtract(playerRefresh[5]);

			// Calculate time since Songs were cast on particular player
			playerSong1_Span[0] = currentTime.Subtract(playerSong1[0]);
			playerSong2_Span[0] = currentTime.Subtract(playerSong2[0]);
			playerSong3_Span[0] = currentTime.Subtract(playerSong3[0]);
			playerSong4_Span[0] = currentTime.Subtract(playerSong4[0]);

			// Calculate time since Adloquium were cast on particular player
			playerAdloquium_Span[0] = currentTime.Subtract(playerAdloquium[0]);
			playerAdloquium_Span[1] = currentTime.Subtract(playerAdloquium[1]);
			playerAdloquium_Span[2] = currentTime.Subtract(playerAdloquium[2]);
			playerAdloquium_Span[3] = currentTime.Subtract(playerAdloquium[3]);
			playerAdloquium_Span[4] = currentTime.Subtract(playerAdloquium[4]);
			playerAdloquium_Span[5] = currentTime.Subtract(playerAdloquium[5]);
			playerAdloquium_Span[6] = currentTime.Subtract(playerAdloquium[6]);
			playerAdloquium_Span[7] = currentTime.Subtract(playerAdloquium[7]);
			playerAdloquium_Span[8] = currentTime.Subtract(playerAdloquium[8]);
			playerAdloquium_Span[9] = currentTime.Subtract(playerAdloquium[9]);
			playerAdloquium_Span[10] = currentTime.Subtract(playerAdloquium[10]);
			playerAdloquium_Span[11] = currentTime.Subtract(playerAdloquium[11]);
			playerAdloquium_Span[12] = currentTime.Subtract(playerAdloquium[12]);
			playerAdloquium_Span[13] = currentTime.Subtract(playerAdloquium[13]);
			playerAdloquium_Span[14] = currentTime.Subtract(playerAdloquium[14]);
			playerAdloquium_Span[15] = currentTime.Subtract(playerAdloquium[15]);
			playerAdloquium_Span[16] = currentTime.Subtract(playerAdloquium[16]);
			playerAdloquium_Span[17] = currentTime.Subtract(playerAdloquium[17]);

			lastSongCast_Span[0] = currentTime.Subtract(lastSongCast[0]);

			// Calculate time since Piannisimo Songs were cast on particular player
			pianissimo1_1_Span[0] = currentTime.Subtract(playerPianissimo1_1[0]);
			pianissimo2_1_Span[0] = currentTime.Subtract(playerPianissimo2_1[0]);
			pianissimo1_2_Span[0] = currentTime.Subtract(playerPianissimo1_2[0]);
			pianissimo2_2_Span[0] = currentTime.Subtract(playerPianissimo2_2[0]);

			// Set array values for GUI "Enabled" checkboxes
			var enabledBoxes = new CheckBox[18];
			enabledBoxes[0] = player0enabled;
			enabledBoxes[1] = player1enabled;
			enabledBoxes[2] = player2enabled;
			enabledBoxes[3] = player3enabled;
			enabledBoxes[4] = player4enabled;
			enabledBoxes[5] = player5enabled;
			enabledBoxes[6] = player6enabled;
			enabledBoxes[7] = player7enabled;
			enabledBoxes[8] = player8enabled;
			enabledBoxes[9] = player9enabled;
			enabledBoxes[10] = player10enabled;
			enabledBoxes[11] = player11enabled;
			enabledBoxes[12] = player12enabled;
			enabledBoxes[13] = player13enabled;
			enabledBoxes[14] = player14enabled;
			enabledBoxes[15] = player15enabled;
			enabledBoxes[16] = player16enabled;
			enabledBoxes[17] = player17enabled;

			// Set array values for GUI "High Priority" checkboxes
			var highPriorityBoxes = new CheckBox[18];
			highPriorityBoxes[0] = player0priority;
			highPriorityBoxes[1] = player1priority;
			highPriorityBoxes[2] = player2priority;
			highPriorityBoxes[3] = player3priority;
			highPriorityBoxes[4] = player4priority;
			highPriorityBoxes[5] = player5priority;
			highPriorityBoxes[6] = player6priority;
			highPriorityBoxes[7] = player7priority;
			highPriorityBoxes[8] = player8priority;
			highPriorityBoxes[9] = player9priority;
			highPriorityBoxes[10] = player10priority;
			highPriorityBoxes[11] = player11priority;
			highPriorityBoxes[12] = player12priority;
			highPriorityBoxes[13] = player13priority;
			highPriorityBoxes[14] = player14priority;
			highPriorityBoxes[15] = player15priority;
			highPriorityBoxes[16] = player16priority;
			highPriorityBoxes[17] = player17priority;

			var songsActive = instanceMonitored
				.Player.GetPlayerInfo().Buffs.Where(b =>
					b == 197 || b == 198 || b == 195 || b == 199 || b == 200 || b == 215 ||
					b == 196 || b == 214 || b == 216 || b == 218 || b == 222).Count();

			// IF ENABLED PAUSE ON KO
			if (Form2.config.pauseOnKO && (instancePrimary.Player.Status == 2 || instancePrimary.Player.Status == 3))
			{
				pauseButton.Text = "Paused!";
				pauseButton.ForeColor = Color.Red;
				enableActions = false;
				pauseActions = true;
				activeBuffs.Clear();

				if (Form2.config.FFXIDefaultAutoFollow == false)
				{
					instancePrimary.AutoFollow.IsAutoFollowing = false;
				}
			}

			// IF YOU ARE DEAD BUT RERAISE IS AVAILABLE THEN ACCEPT RAISE
			if (Form2.config.AcceptRaise == true && (instancePrimary.Player.Status == 2 || instancePrimary.Player.Status == 3))
			{
				if (instancePrimary.Menu.IsMenuOpen && instancePrimary.Menu.HelpName == "Revival" && instancePrimary.Menu.MenuIndex == 1 && ((Form2.config.AcceptRaiseOnlyWhenNotInCombat == true && instanceMonitored.Player.Status != 1) || Form2.config.AcceptRaiseOnlyWhenNotInCombat == false))
				{
					await Task.Delay(2000);
					currentAction.Text = "Accepting Raise or Reraise.";
					instancePrimary.ThirdParty.KeyPress(EliteMMO.API.Keys.NUMPADENTER);
					await Task.Delay(5000);
					currentAction.Text = string.Empty;
				}
			}

			var stunLocked =
				plHasBuff(Buffs.terror) ||
				plHasBuff(Buffs.Petrification) ||
				plHasBuff(Buffs.stun);

			if (stunLocked)
			{
				return;
			}
			else
			{
				await RemoveCriticalDebuffsFromPL();
			}

			if (await ConvertIfNecessary())
			{
				return;
			}

			HandleLowMpSituations();
			if (waitingForMp)
			{
				return;
			}

			// Only perform actions if PL is stationary PAUSE GOES HERE
			if ((instancePrimary.Player.X == plX) && (instancePrimary.Player.Y == plY) && (instancePrimary.Player.Z == plZ) && (instancePrimary.Player.LoginStatus == (int)LoginStatus.LoggedIn) && abilityLocked != true && castingLocked != true && curePlease_autofollow == false && ((instancePrimary.Player.Status == (uint)Status.Standing) || (instancePrimary.Player.Status == (uint)Status.Fighting)))
			{
				await RemoveCriticalDebuffsFromPL();
				await ApplyPrecastAbilities(songsActive);

				var cures_required = new List<byte>();
				var MemberOf_curaga = GeneratePT_structure();

				// Cure PL
				if (instancePrimary.Player.HP > 0 && (instancePrimary.Player.HPP <= Form2.config.monitoredCurePercentage) && Form2.config.enableOutOfPartyHealing == true && PLInParty() == false)
				{
					await CureCalculator_PL(false);
				}

				// Cure party (AOE)
				var cParty_curaga = instanceMonitored.Party.GetPartyMembers().Where(p =>
					p.Active != 0 && p.Zone == instancePrimary.Player.ZoneId).OrderBy(p => p.CurrentHPP);

				var memberOF_curaga = GeneratePT_structure();
				if (memberOF_curaga != 0 && memberOF_curaga != 4)
				{
					foreach (var pData in cParty_curaga)
					{
						if (memberOF_curaga == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5)
						{
							if (castingPossible(pData.MemberNumber) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].Active >= 1) && (enabledBoxes[pData.MemberNumber].Checked) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHP > 0))
							{
								if ((instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHPP <= Form2.config.curagaCurePercentage) && (castingPossible(pData.MemberNumber)))
								{
									cures_required.Add(pData.MemberNumber);
								}
							}
						}
						else if (memberOF_curaga == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11)
						{
							if (castingPossible(pData.MemberNumber) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].Active >= 1) && (enabledBoxes[pData.MemberNumber].Checked) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHP > 0))
							{
								if ((instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHPP <= Form2.config.curagaCurePercentage) && (castingPossible(pData.MemberNumber)))
								{
									cures_required.Add(pData.MemberNumber);
								}
							}
						}
						else if (memberOF_curaga == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17)
						{
							if (castingPossible(pData.MemberNumber) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].Active >= 1) && (enabledBoxes[pData.MemberNumber].Checked) && (instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHP > 0))
							{
								if ((instanceMonitored.Party.GetPartyMembers()[pData.MemberNumber].CurrentHPP <= Form2.config.curagaCurePercentage) && (castingPossible(pData.MemberNumber)))
								{
									cures_required.Add(pData.MemberNumber);
								}
							}
						}
					}

					if (cures_required.Count >= Form2.config.curagaRequiredMembers)
					{
						int lowestHP_id = cures_required.First();
						await CuragaCalculatorAsync(lowestHP_id);
					}
				}

				/////////////////////////// CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				//var playerHpOrder = _ELITEAPIMonitored.Party.GetPartyMembers().Where(p => p.Active >= 1).OrderBy(p => p.CurrentHPP).Select(p => p.Index);
				var playerHpOrder = instanceMonitored.Party.GetPartyMembers().OrderBy(p => p.CurrentHPP).OrderBy(p => p.Active == 0).Select(p => p.MemberNumber);

				// First run a check on the monitored target
				var playerMonitoredHp = instanceMonitored.Party.GetPartyMembers().Where(p => p.Name == instanceMonitored.Player.Name).OrderBy(p => p.Active == 0).Select(p => p.MemberNumber).FirstOrDefault();

				if (Form2.config.enableMonitoredPriority && instanceMonitored.Party.GetPartyMembers()[playerMonitoredHp].Name == instanceMonitored.Player.Name && instanceMonitored.Party.GetPartyMembers()[playerMonitoredHp].CurrentHP > 0 && (instanceMonitored.Party.GetPartyMembers()[playerMonitoredHp].CurrentHPP <= Form2.config.monitoredCurePercentage))
				{
					await CureCalculator(playerMonitoredHp, false);
				}
				else
				{
					// Now run a scan to check all targets in the High Priority Threshold
					foreach (var id in playerHpOrder)
					{
						if ((highPriorityBoxes[id].Checked) && instanceMonitored.Party.GetPartyMembers()[id].CurrentHP > 0 && (instanceMonitored.Party.GetPartyMembers()[id].CurrentHPP <= Form2.config.priorityCurePercentage))
						{
							await CureCalculator(id, true);
							break;
						}
					}

					// Now run everyone else
					foreach (var id in playerHpOrder)
					{
						// Cures First, is casting possible, and enabled?
						if (castingPossible(id) && (instanceMonitored.Party.GetPartyMembers()[id].Active >= 1) && (enabledBoxes[id].Checked) && (instanceMonitored.Party.GetPartyMembers()[id].CurrentHP > 0))
						{
							if ((instanceMonitored.Party.GetPartyMembers()[id].CurrentHPP <= Form2.config.curePercentage) && (castingPossible(id)))
							{
								await CureCalculator(id, false);
								break;
							}
						}
					}
				}

				// RUN DEBUFF REMOVAL - CONVERTED TO FUNCTION SO CAN BE RUN IN MULTIPLE AREAS
				await RunDebuffChecker();

				// PL Auto Buffs

				var BarspellName = string.Empty;
				short BarspellBuffID = 0;
				var BarSpell_AOE = false;

				if (Form2.config.AOE_Barelemental == false)
				{
					var barspell = barSpells.Where(c => c.Position == Form2.config.plBarElement_Spell && c.Type == 1 && c.AoeVersion != true).SingleOrDefault();

					BarspellName = barspell.Name;
					BarspellBuffID = barspell.BuffId;
					BarSpell_AOE = false;
				}
				else
				{
					var barspell = barSpells.Where(c => c.Position == Form2.config.plBarElement_Spell && c.Type == 1 && c.AoeVersion == true).SingleOrDefault();

					BarspellName = barspell.Name;
					BarspellBuffID = barspell.BuffId;
					BarSpell_AOE = true;
				}

				var BarstatusName = string.Empty;
				short BarstatusBuffID = 0;
				var BarStatus_AOE = false;

				if (Form2.config.AOE_Barstatus == false)
				{
					var barstatus = barSpells.Where(c => c.Position == Form2.config.plBarStatus_Spell && c.Type == 2 && c.AoeVersion != true).SingleOrDefault();

					BarstatusName = barstatus.Name;
					BarstatusBuffID = barstatus.BuffId;
					BarStatus_AOE = false;
				}
				else
				{
					var barstatus = barSpells.Where(c => c.Position == Form2.config.plBarStatus_Spell && c.Type == 2 && c.AoeVersion == true).SingleOrDefault();

					BarstatusName = barstatus.Name;
					BarstatusBuffID = barstatus.BuffId;
					BarStatus_AOE = true;
				}

				var enspell = enSpells.Where(c => c.Position == Form2.config.plEnspell_Spell && c.Type == 1).SingleOrDefault();
				var stormspell = stormSpells.Where(c => c.Position == Form2.config.plStormSpell_Spell).SingleOrDefault();

				if (instancePrimary.Player.LoginStatus == (int)LoginStatus.LoggedIn && abilityLocked != true && castingLocked != true)
				{
					if ((Form2.config.Composure) && (!plHasBuff(StatusEffect.Composure)) && CanUseJobAbility("Composure"))
					{

						await UseJobAbility("Composure");
					}
					else if ((Form2.config.LightArts) && (!plHasBuff(StatusEffect.Light_Arts)) && (!plHasBuff(StatusEffect.Addendum_White)) && CanUseJobAbility("Light Arts"))
					{
						await UseJobAbility("Light Arts");
					}
					else if ((Form2.config.AddendumWhite) && (!plHasBuff(StatusEffect.Addendum_White)) && (plHasBuff(StatusEffect.Light_Arts)) && CanUseJobAbility("Stratagems"))
					{
						await UseJobAbility("Addendum: White");
					}
					else if ((Form2.config.DarkArts) && (!plHasBuff(StatusEffect.Dark_Arts)) && (!plHasBuff(StatusEffect.Addendum_Black)) && CanUseJobAbility("Dark Arts"))
					{
						await UseJobAbility("Dark Arts");
					}
					else if ((Form2.config.AddendumBlack) && (plHasBuff(StatusEffect.Dark_Arts)) && (!plHasBuff(StatusEffect.Addendum_Black)) && CanUseJobAbility("Stratagems"))
					{
						await UseJobAbility("Addendum: Black");
					}
					else if ((Form2.config.plReraise) && (Form2.config.EnlightenmentReraise) && (!plHasBuff(StatusEffect.Reraise)) && instancePrimary.Player.MainJob == 20 && !hasBuff(401, 0) && CanUseJobAbility("Enlightenment"))
					{
						if (!plHasBuff(StatusEffect.Enlightenment) && CanUseJobAbility("Enlightenment"))
						{
							await UseJobAbility("Enlightenment");
						}

						if ((Form2.config.plReraise_Level == 1) && instancePrimary.Player.HasSpell(instancePrimary.Resources.GetSpell("Reraise", 0).Index) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise");
						}
						else if ((Form2.config.plReraise_Level == 2) && instancePrimary.Player.HasSpell(instancePrimary.Resources.GetSpell("Reraise II", 0).Index) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise II");
						}
						else if ((Form2.config.plReraise_Level == 3) && instancePrimary.Player.HasSpell(instancePrimary.Resources.GetSpell("Reraise III", 0).Index) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise III");
						}
						else if ((Form2.config.plReraise_Level == 4) && instancePrimary.Player.HasSpell(instancePrimary.Resources.GetSpell("Reraise III", 0).Index) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise III");
						}
					}
					else if ((Form2.config.plReraise) && (!plHasBuff(StatusEffect.Reraise)) && CheckReraiseLevelPossession() == true)
					{
						if ((Form2.config.plReraise_Level == 1) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise");
						}
						else if ((Form2.config.plReraise_Level == 2) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise II");
						}
						else if ((Form2.config.plReraise_Level == 3) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise III");
						}
						else if ((Form2.config.plReraise_Level == 4) && instancePrimary.Player.MP > 150)
						{
							await CastSpell("<me>", "Reraise IV");
						}
					}
					else if ((Form2.config.plUtsusemi) && (hasBuff(444, 0) != true && hasBuff(445, 0) != true && hasBuff(446, 0) != true))
					{
						if (SpellReadyToCast("Utsusemi: Ni") && HasAcquiredSpell("Utsusemi: Ni") && HasRequiredJobLevel("Utsusemi: Ni") == true && GetInventoryItemCount(instancePrimary, GetItemId("Shihei")) > 0)
						{
							await CastSpell("<me>", "Utsusemi: Ni");
						}
						else if (SpellReadyToCast("Utsusemi: Ichi") && HasAcquiredSpell("Utsusemi: Ichi") && HasRequiredJobLevel("Utsusemi: Ichi") == true && (hasBuff(62, 0) != true && hasBuff(444, 0) != true && hasBuff(445, 0) != true && hasBuff(446, 0) != true) && GetInventoryItemCount(instancePrimary, GetItemId("Shihei")) > 0)
						{
							await CastSpell("<me>", "Utsusemi: Ichi");
						}
					}
					else if ((Form2.config.plProtect) && (!plHasBuff(StatusEffect.Protect)))
					{
						var protectSpell = string.Empty;
						if (Form2.config.autoProtect_Spell == 0)
						{
							protectSpell = "Protect";
						}
						else if (Form2.config.autoProtect_Spell == 1)
						{
							protectSpell = "Protect II";
						}
						else if (Form2.config.autoProtect_Spell == 2)
						{
							protectSpell = "Protect III";
						}
						else if (Form2.config.autoProtect_Spell == 3)
						{
							protectSpell = "Protect IV";
						}
						else if (Form2.config.autoProtect_Spell == 4)
						{
							protectSpell = "Protect V";
						}

						if (protectSpell != string.Empty && SpellReadyToCast(protectSpell) && HasAcquiredSpell(protectSpell) && HasRequiredJobLevel(protectSpell) == true)
						{
							if ((Form2.config.Accession && Form2.config.accessionProShell && instancePrimary.Party.GetPartyMembers().Count() > 2) && ((instancePrimary.Player.MainJob == 5 && instancePrimary.Player.SubJob == 20) || instancePrimary.Player.MainJob == 20) && currentSCHCharges >= 1 && (CanUseJobAbility("Accession")))
							{
								if (!plHasBuff(StatusEffect.Accession))
								{
									await UseJobAbility("Accession");
									return;
								}
							}

							await CastSpell("<me>", protectSpell);
						}
					}
					else if ((Form2.config.plShell) && (!plHasBuff(StatusEffect.Shell)))
					{
						var shellSpell = string.Empty;
						if (Form2.config.autoShell_Spell == 0)
						{
							shellSpell = "Shell";
						}
						else if (Form2.config.autoShell_Spell == 1)
						{
							shellSpell = "Shell II";
						}
						else if (Form2.config.autoShell_Spell == 2)
						{
							shellSpell = "Shell III";
						}
						else if (Form2.config.autoShell_Spell == 3)
						{
							shellSpell = "Shell IV";
						}
						else if (Form2.config.autoShell_Spell == 4)
						{
							shellSpell = "Shell V";
						}

						if (shellSpell != string.Empty && SpellReadyToCast(shellSpell) && HasAcquiredSpell(shellSpell) && HasRequiredJobLevel(shellSpell) == true)
						{
							if ((Form2.config.Accession && Form2.config.accessionProShell && instancePrimary.Party.GetPartyMembers().Count() > 2) && ((instancePrimary.Player.MainJob == 5 && instancePrimary.Player.SubJob == 20) || instancePrimary.Player.MainJob == 20) && currentSCHCharges >= 1 && (CanUseJobAbility("Accession")))
							{
								if (!plHasBuff(StatusEffect.Accession))
								{
									await UseJobAbility("Accession");
									return;
								}
							}

							await CastSpell("<me>", shellSpell);
						}
					}
					else if ((Form2.config.plBlink) && (!plHasBuff(StatusEffect.Blink)) && CanCastSpell("Blink"))
					{

						if (Form2.config.Accession && Form2.config.blinkAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.blinkPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", "Blink");
					}
					else if ((Form2.config.plPhalanx) && (!plHasBuff(StatusEffect.Phalanx)) && CanCastSpell("Phalanx") && HasRequiredJobLevel("Phalanx") == true)
					{
						if (Form2.config.Accession && Form2.config.phalanxAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.phalanxPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", "Phalanx");
					}
					else if ((Form2.config.plRefresh) && (!plHasBuff(StatusEffect.Refresh)) && CheckRefreshLevelPossession())
					{
						if ((Form2.config.plRefresh_Level == 1) && CanCastSpell("Refresh") && HasRequiredJobLevel("Refresh") == true)
						{
							if (Form2.config.Accession && Form2.config.refreshAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
							{
								await UseJobAbility("Accession");
							}

							if (Form2.config.Perpetuance && Form2.config.refreshPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
							{
								await UseJobAbility("Perpetuance");
							}

							await CastSpell("<me>", "Refresh");
						}
						else if ((Form2.config.plRefresh_Level == 2) && CanCastSpell("Refresh II") && HasRequiredJobLevel("Refresh II") == true)
						{
							await CastSpell("<me>", "Refresh II");
						}
						else if ((Form2.config.plRefresh_Level == 3) && CanCastSpell("Refresh III") && HasRequiredJobLevel("Refresh III") == true)
						{
							await CastSpell("<me>", "Refresh III");
						}
					}
					else if ((Form2.config.plRegen) && (!plHasBuff(StatusEffect.Regen)) && CheckRegenLevelPossession() == true)
					{
						if (Form2.config.Accession && Form2.config.regenAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
							return;
						}

						if (Form2.config.Perpetuance && Form2.config.regenPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
							return;
						}

						if ((Form2.config.plRegen_Level == 1) && instancePrimary.Player.MP > 15)
						{
							await CastSpell("<me>", "Regen");
						}
						else if ((Form2.config.plRegen_Level == 2) && instancePrimary.Player.MP > 36)
						{
							await CastSpell("<me>", "Regen II");
						}
						else if ((Form2.config.plRegen_Level == 3) && instancePrimary.Player.MP > 64)
						{
							await CastSpell("<me>", "Regen III");
						}
						else if ((Form2.config.plRegen_Level == 4) && instancePrimary.Player.MP > 82)
						{
							await CastSpell("<me>", "Regen IV");
						}
						else if ((Form2.config.plRegen_Level == 5) && instancePrimary.Player.MP > 100)
						{
							await CastSpell("<me>", "Regen V");
						}
					}
					else if ((Form2.config.plAdloquium) && (!plHasBuff(StatusEffect.Regain)) && CanCastSpell("Adloquium") && HasRequiredJobLevel("Adloquium") == true)
					{
						if (Form2.config.Accession && Form2.config.adloquiumAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.adloquiumPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", "Adloquium");
					}
					else if ((Form2.config.plStoneskin) && (!plHasBuff(StatusEffect.Stoneskin)) && CanCastSpell("Stoneskin") && HasRequiredJobLevel("Stoneskin") == true)
					{
						if (Form2.config.Accession && Form2.config.stoneskinAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.stoneskinPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", "Stoneskin");
					}
					else if ((Form2.config.plAquaveil) && (!plHasBuff(StatusEffect.Aquaveil)) && CanCastSpell("Aquaveil") && HasRequiredJobLevel("Aquaveil") == true)
					{
						if (Form2.config.Accession && Form2.config.aquaveilAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.aquaveilPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", "Aquaveil");
					}
					else if ((Form2.config.plShellra) && (!plHasBuff(StatusEffect.Shell)) && CheckShellraLevelPossession() == true)
					{
						await CastSpell("<me>", GetShellraLevel(Form2.config.plShellra_Level));
					}
					else if ((Form2.config.plProtectra) && (!plHasBuff(StatusEffect.Protect)) && CheckProtectraLevelPossession() == true)
					{
						await CastSpell("<me>", GetProtectraLevel(Form2.config.plProtectra_Level));
					}
					else if (Form2.config.plBarElement && !hasBuff(BarspellBuffID, 0) && CanCastSpell(BarspellName))
					{
						if (Form2.config.Accession && Form2.config.barspellAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && BarSpell_AOE == false && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.barspellPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", BarspellName);
					}
					else if (
						CanCastSpell(BarstatusName) &&
						Form2.config.plBarStatus &&
						!hasBuff(BarstatusBuffID, 0))
					{
						if (
							Form2.config.Accession &&
							Form2.config.barstatusAccession &&
							!plHasBuff(StatusEffect.Accession) &&
							CanUseJobAbility("Accession") &&
							currentSCHCharges > 0 &&
							!BarStatus_AOE)
						{
							await UseJobAbility("Accession");
						}

						if (
							Form2.config.Perpetuance &&
							Form2.config.barstatusPerpetuance &&
							!plHasBuff(StatusEffect.Perpetuance) &&
							CanUseJobAbility("Perpetuance") &&
							currentSCHCharges > 0)
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", BarstatusName);
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 0) && !plHasBuff(StatusEffect.STR_Boost2) && CanCastSpell("Gain-STR"))
					{
						await CastSpell("<me>", "Gain-STR");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 1) && !plHasBuff(StatusEffect.DEX_Boost2) && CanCastSpell("Gain-DEX"))
					{
						await CastSpell("<me>", "Gain-DEX");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 2) && !plHasBuff(StatusEffect.VIT_Boost2) && CanCastSpell("Gain-VIT"))
					{
						await CastSpell("<me>", "Gain-VIT");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 3) && !plHasBuff(StatusEffect.AGI_Boost2) && CanCastSpell("Gain-AGI"))
					{
						await CastSpell("<me>", "Gain-AGI");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 4) && !plHasBuff(StatusEffect.INT_Boost2) && CanCastSpell("Gain-INT"))
					{
						await CastSpell("<me>", "Gain-INT");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 5) && !plHasBuff(StatusEffect.MND_Boost2) && CanCastSpell("Gain-MND"))
					{
						await CastSpell("<me>", "Gain-MND");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 6) && !plHasBuff(StatusEffect.CHR_Boost2) && CanCastSpell("Gain-CHR"))
					{
						await CastSpell("<me>", "Gain-CHR");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 7) && !plHasBuff(StatusEffect.STR_Boost2) && CanCastSpell("Boost-STR"))
					{
						await CastSpell("<me>", "Boost-STR");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 8) && !plHasBuff(StatusEffect.DEX_Boost2) && CanCastSpell("Boost-DEX"))
					{
						await CastSpell("<me>", "Boost-DEX");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 9) && !plHasBuff(StatusEffect.VIT_Boost2) && CanCastSpell("Boost-VIT"))
					{
						await CastSpell("<me>", "Boost-VIT");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 10) && !plHasBuff(StatusEffect.AGI_Boost2) && CanCastSpell("Boost-AGI"))
					{
						await CastSpell("<me>", "Boost-AGI");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 11) && !plHasBuff(StatusEffect.INT_Boost2) && CanCastSpell("Boost-INT"))
					{
						await CastSpell("<me>", "Boost-INT");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 12) && !plHasBuff(StatusEffect.MND_Boost2) && CanCastSpell("Boost-MND"))
					{
						await CastSpell("<me>", "Boost-MND");
					}
					else if (Form2.config.plGainBoost && (Form2.config.plGainBoost_Spell == 13) && !plHasBuff(StatusEffect.CHR_Boost2) && CanCastSpell("Boost-CHR"))
					{
						await CastSpell("<me>", "Boost-CHR");
					}
					else if (Form2.config.plStormSpell && !hasBuff(stormspell.BuffId, 0) && CanCastSpell(stormspell.Name))
					{
						if (Form2.config.Accession && Form2.config.stormspellAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.stormspellPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", stormspell.Name);
					}
					else if ((Form2.config.plKlimaform) && !plHasBuff(StatusEffect.Klimaform))
					{
						if (CanCastSpell("Klimaform"))
						{
							await CastSpell("<me>", "Klimaform");
						}
					}
					else if ((Form2.config.plTemper) && (!plHasBuff(StatusEffect.Multi_Strikes)))
					{
						if ((Form2.config.plTemper_Level == 1) && CanCastSpell("Temper"))
						{
							await CastSpell("<me>", "Temper");
						}
						else if ((Form2.config.plTemper_Level == 2) && CanCastSpell("Temper II"))
						{
							await CastSpell("<me>", "Temper II");
						}
					}
					else if ((Form2.config.plHaste) && (!plHasBuff(StatusEffect.Haste)))
					{
						if ((Form2.config.plHaste_Level == 1) && CanCastSpell("Haste"))
						{
							await CastSpell("<me>", "Haste");
						}
						else if ((Form2.config.plHaste_Level == 2) && CanCastSpell("Haste II"))
						{
							await CastSpell("<me>", "Haste II");
						}
					}
					else if ((Form2.config.plSpikes) && ActiveSpikes() == false)
					{
						if ((Form2.config.plSpikes_Spell == 0) && CanCastSpell("Blaze Spikes"))
						{
							await CastSpell("<me>", "Blaze Spikes");
						}
						else if ((Form2.config.plSpikes_Spell == 1) && CanCastSpell("Ice Spikes"))
						{
							await CastSpell("<me>", "Ice Spikes");
						}
						else if ((Form2.config.plSpikes_Spell == 2) && CanCastSpell("Shock Spikes"))
						{
							await CastSpell("<me>", "Shock Spikes");
						}
					}
					else if (Form2.config.plEnspell && !hasBuff(enspell.BuffId, 0) && CanCastSpell(enspell.Name))
					{
						if (Form2.config.Accession && Form2.config.enspellAccession && currentSCHCharges > 0 && CanUseJobAbility("Accession") && enspell.Position < 6 && !plHasBuff(StatusEffect.Accession))
						{
							await UseJobAbility("Accession");
						}

						if (Form2.config.Perpetuance && Form2.config.enspellPerpetuance && currentSCHCharges > 0 && CanUseJobAbility("Perpetuance") && enspell.Position < 6 && !plHasBuff(StatusEffect.Perpetuance))
						{
							await UseJobAbility("Perpetuance");
						}

						await CastSpell("<me>", enspell.Name);
					}
					else if ((Form2.config.plAuspice) && (!plHasBuff(StatusEffect.Auspice)) && CanCastSpell("Auspice"))
					{
						await CastSpell("<me>", "Auspice");
					}

					// ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
					else if ((Form2.config.EnableGeoSpells) && (plHasBuff((StatusEffect)584)) && instancePrimary.Player.Status != 33)
					{
						var SpellCheckedResult = ReturnGeoSpell(Form2.config.EntrustedSpell_Spell, 1);
						if (SpellCheckedResult == "SpellError_Cancel")
						{
							//Form2.config.EnableGeoSpells = false;
							//MessageBox.Show("An error has occurred with Entrusted INDI spell casting, please report what spell was active at the time.");
							currentAction.Text = $"Error casting {Form2.config.EntrustedSpell_Spell} on {Form2.config.EntrustedSpell_Target}";
						}
						else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
						{
						}
						else
						{
							if (Form2.config.EntrustedSpell_Target == string.Empty)
							{
								await CastSpell(instanceMonitored.Player.Name, SpellCheckedResult);
							}
							else
							{
								await CastSpell(Form2.config.EntrustedSpell_Target, SpellCheckedResult);
							}
						}
					}

					// CAST NON ENTRUSTED INDI SPELL
					else if (Form2.config.EnableGeoSpells && !hasBuff(612, 0) && instancePrimary.Player.Status != 33 && (CheckEngagedStatus() == true || !Form2.config.IndiWhenEngaged))
					{
						var SpellCheckedResult = ReturnGeoSpell(Form2.config.IndiSpell_Spell, 1);

						if (SpellCheckedResult == "SpellError_Cancel")
						{
							//Form2.config.EnableGeoSpells = false;
							//MessageBox.Show("An error has occurred with INDI spell casting, please report what spell was active at the time.");
							currentAction.Text = $"Error casting {Form2.config.IndiSpell_Spell}";
						}
						else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
						{
						}
						else
						{
							await CastSpell("<me>", SpellCheckedResult);
						}

					}

					// GEO SPELL CASTING 
					else if ((Form2.config.EnableLuopanSpells) && (instancePrimary.Player.Pet.HealthPercent < 1) && (CheckEngagedStatus() == true))
					{
						// Use BLAZE OF GLORY if ENABLED
						if (Form2.config.BlazeOfGlory && CanUseJobAbility("Blaze of Glory") && CheckEngagedStatus() == true && GEO_EnemyCheck() == true)
						{
							await UseJobAbility("Blaze of Glory");
						}

						// Grab GEO spell name
						var SpellCheckedResult = ReturnGeoSpell(Form2.config.GeoSpell_Spell, 2);

						if (SpellCheckedResult == "SpellError_Cancel")
						{
							//Form2.config.EnableGeoSpells = false;
							//MessageBox.Show("An error has occurred with GEO spell casting, please report what spell was active at the time.");
							currentAction.Text = $"Error casting {Form2.config.GeoSpell_Spell}";
						}
						else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
						{
							// Do nothing and continue on with the program
						}
						else
						{
							if (instancePrimary.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
							{ // PLAYER CHARACTER TARGET
								if (Form2.config.LuopanSpell_Target == string.Empty)
								{

									if (hasBuff(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
									{
										eclipticActive = true;
									}

									await CastSpell(instanceMonitored.Player.Name, SpellCheckedResult);
								}
								else
								{
									if (hasBuff(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
									{
										eclipticActive = true;
									}

									await CastSpell(Form2.config.LuopanSpell_Target, SpellCheckedResult);
								}
							}
							else
							{ // ENEMY BASED TARGET NEED TO ASSURE PLAYER IS ENGAGED
								if (CheckEngagedStatus() == true)
								{

									var GrabbedTargetID = GrabGEOTargetID();

									if (GrabbedTargetID != 0)
									{

										instancePrimary.Target.SetTarget(GrabbedTargetID);
										await Task.Delay(TimeSpan.FromSeconds(1));

										if (hasBuff(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
										{
											eclipticActive = true;
										}

										await CastSpell("<t>", SpellCheckedResult);
										await Task.Delay(TimeSpan.FromSeconds(4));
										if (Form2.config.DisableTargettingCancel == false)
										{
											await Task.Delay(TimeSpan.FromSeconds((double)Form2.config.TargetRemoval_Delay));
											instancePrimary.Target.SetTarget(0);
										}
									}
								}
							}
						}
					}

					else if ((Form2.config.autoTarget == true) && (SpellReadyToCast(Form2.config.autoTargetSpell)) && (HasAcquiredSpell(Form2.config.autoTargetSpell)))
					{
						if (Form2.config.Hate_SpellType == 1) // PARTY BASED HATE SPELL
						{
							var enemyID = CheckEngagedStatus_Hate();

							if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
							{
								await CastSpell(Form2.config.autoTarget_Target, Form2.config.autoTargetSpell);
								lastKnownEstablisherTarget = enemyID;
							}
						}
						else // ENEMY BASED TARGET
						{
							var enemyID = CheckEngagedStatus_Hate();

							if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
							{
								instancePrimary.Target.SetTarget(enemyID);
								await Task.Delay(TimeSpan.FromMilliseconds(500));
								await CastSpell("<t>", Form2.config.autoTargetSpell);
								lastKnownEstablisherTarget = enemyID;
								await Task.Delay(TimeSpan.FromMilliseconds(1000));

								if (Form2.config.DisableTargettingCancel == false)
								{
									await Task.Delay(TimeSpan.FromSeconds((double)Form2.config.TargetRemoval_Delay));
									instancePrimary.Target.SetTarget(0);
								}
							}
						}
					}

					// BARD SONGS
					else if (Form2.config.enableSinging && !plHasBuff(StatusEffect.Silence) && (instancePrimary.Player.Status == 1 || instancePrimary.Player.Status == 0))
					{
						await Run_BardSongs();
					}

					// so PL job abilities are in order
					if (!plHasBuff(StatusEffect.Amnesia) && (instancePrimary.Player.Status == 1 || instancePrimary.Player.Status == 0))
					{
						if ((Form2.config.AfflatusSolace) && (!plHasBuff(StatusEffect.Afflatus_Solace)) && CanUseJobAbility("Afflatus Solace"))
						{
							await UseJobAbility("Afflatus Solace");
						}
						else if ((Form2.config.AfflatusMisery) && (!plHasBuff(StatusEffect.Afflatus_Misery)) && CanUseJobAbility("Afflatus Misery"))
						{
							await UseJobAbility("Afflatus Misery");
						}
						else if ((Form2.config.Composure) && (!plHasBuff(StatusEffect.Composure)) && CanUseJobAbility("Composure"))
						{
							await UseJobAbility("Composure");
						}
						else if ((Form2.config.LightArts) && (!plHasBuff(StatusEffect.Light_Arts)) && (!plHasBuff(StatusEffect.Addendum_White)) && CanUseJobAbility("Light Arts"))
						{
							await UseJobAbility("Light Arts");
						}
						else if ((Form2.config.AddendumWhite) && (!plHasBuff(StatusEffect.Addendum_White)) && CanUseJobAbility("Stratagems"))
						{
							await UseJobAbility("Addendum: White");
						}
						else if ((Form2.config.Sublimation) && (!plHasBuff(StatusEffect.Sublimation_Activated)) && (!plHasBuff(StatusEffect.Sublimation_Complete)) && (!plHasBuff(StatusEffect.Refresh)) && CanUseJobAbility("Sublimation"))
						{
							await UseJobAbility("Sublimation");
						}
						else if ((Form2.config.Sublimation) && ((instancePrimary.Player.MPMax - instancePrimary.Player.MP) > Form2.config.sublimationMP) && (plHasBuff(StatusEffect.Sublimation_Complete)) && CanUseJobAbility("Sublimation"))
						{
							await UseJobAbility("Sublimation");
						}
						else if ((Form2.config.DivineCaress) && (Form2.config.plDebuffEnabled || Form2.config.monitoredDebuffEnabled || Form2.config.enablePartyDebuffRemoval) && CanUseJobAbility("Divine Caress"))
						{
							await UseJobAbility("Divine Caress");
						}
						else if (Form2.config.Entrust && !plHasBuff((StatusEffect)584) && CheckEngagedStatus() == true && CanUseJobAbility("Entrust"))
						{
							await UseJobAbility("Entrust");
						}
						else if (Form2.config.Dematerialize && CheckEngagedStatus() == true && instancePrimary.Player.Pet.HealthPercent >= 90 && CanUseJobAbility("Dematerialize"))
						{
							await UseJobAbility("Dematerialize");
						}
						else if (Form2.config.EclipticAttrition && CheckEngagedStatus() == true && instancePrimary.Player.Pet.HealthPercent >= 90 && CanUseJobAbility("Ecliptic Attrition") && (hasBuff(516, 2) != true) && eclipticActive != true)
						{
							await UseJobAbility("Ecliptic Attrition");
						}
						else if (Form2.config.LifeCycle && CheckEngagedStatus() == true && instancePrimary.Player.Pet.HealthPercent <= 30 && instancePrimary.Player.Pet.HealthPercent >= 5 && instancePrimary.Player.HPP >= 90 && CanUseJobAbility("Life Cycle"))
						{
							await UseJobAbility("Life Cycle");
						}
						else if ((Form2.config.Devotion) && CanUseJobAbility("Devotion") && instancePrimary.Player.HPP > 80 && (!Form2.config.DevotionWhenEngaged || (instanceMonitored.Player.Status == 1)))
						{
							// First Generate the current party number, this will be used
							// regardless of the type
							var memberOF = GeneratePT_structure();

							// Now generate the party
							var cParty = instanceMonitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == instancePrimary.Player.ZoneId);

							// Make sure member number is not 0 (null) or 4 (void)
							if (memberOF != 0 && memberOF != 4)
							{
								// Run through Each party member as we're looking for either a specifc name or if set otherwise anyone with the MP criteria in the current party.
								foreach (var pData in cParty)
								{
									// If party of party v1
									if (memberOF == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5)
									{
										if (!string.IsNullOrEmpty(pData.Name) && pData.Name != instancePrimary.Player.Name)
										{
											if ((Form2.config.DevotionTargetType == 0))
											{
												if (pData.Name == Form2.config.DevotionTargetName)
												{
													var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);
													if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP && pData.CurrentMPP <= 30)
													{
														instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
														Thread.Sleep(TimeSpan.FromSeconds(2));
													}
												}
											}
											else
											{
												var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);

												if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 30)
												{
													instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
													Thread.Sleep(TimeSpan.FromSeconds(2));
													break;
												}
											}
										}
									} // If part of party 2
									else if (memberOF == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11)
									{
										if (!string.IsNullOrEmpty(pData.Name) && pData.Name != instancePrimary.Player.Name)
										{
											if ((Form2.config.DevotionTargetType == 0))
											{
												if (pData.Name == Form2.config.DevotionTargetName)
												{
													var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);
													if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP)
													{
														instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
														Thread.Sleep(TimeSpan.FromSeconds(2));
													}
												}
											}
											else
											{
												var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);

												if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
												{
													instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
													Thread.Sleep(TimeSpan.FromSeconds(2));
													break;
												}
											}
										}
									} // If part of party 3
									else if (memberOF == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17)
									{
										if (!string.IsNullOrEmpty(pData.Name) && pData.Name != instancePrimary.Player.Name)
										{
											if ((Form2.config.DevotionTargetType == 0))
											{
												if (pData.Name == Form2.config.DevotionTargetName)
												{
													var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);
													if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= Form2.config.DevotionMP)
													{
														instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + Form2.config.DevotionTargetName);
														Thread.Sleep(TimeSpan.FromSeconds(2));
													}
												}
											}
											else
											{
												var playerInfo = instancePrimary.Entity.GetEntity((int)pData.TargetIndex);

												if ((pData.CurrentMP <= Form2.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
												{
													instancePrimary.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
													Thread.Sleep(TimeSpan.FromSeconds(2));
													break;
												}
											}
										}
									}
								}
							}
						}
					}

					var playerBuffOrder = instanceMonitored.Party.GetPartyMembers()
						.OrderBy(p => p.MemberNumber).OrderBy(p => p.Active == 0)
						.Where(p => p.Active == 1);

					string[] regen_spells = { "Regen", "Regen II", "Regen III", "Regen IV", "Regen V" };
					string[] refresh_spells = { "Refresh", "Refresh II", "Refresh III" };

					// Auto Casting
					foreach (var charDATA in playerBuffOrder)
					{
						// Grab the Storm Spells name to perform checks.
						var StormSpell_Enabled = CheckStormspell(charDATA.MemberNumber);

						// Grab storm spell Data for Buff ID etc...
						var PTstormspell = stormSpells.Where(c => c.Name == StormSpell_Enabled).SingleOrDefault();

						// PL BASED BUFFS
						if (instancePrimary.Player.Name == charDATA.Name)
						{

							if (autoHasteEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste") && HasAcquiredSpell("Haste") && HasRequiredJobLevel("Haste") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !plHasBuff(StatusEffect.Haste) && !plHasBuff(StatusEffect.Slow))
							{
								await hastePlayer(charDATA.MemberNumber);
							}
							if (autoHaste_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste II") && HasAcquiredSpell("Haste II") && HasRequiredJobLevel("Haste II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !plHasBuff(StatusEffect.Haste) && !plHasBuff(StatusEffect.Slow))
							{
								await haste_IIPlayer(charDATA.MemberNumber);
							}
							if (autoAdloquium_Enabled[charDATA.MemberNumber] && SpellReadyToCast("Adloquium") && HasAcquiredSpell("Adloquium") && HasRequiredJobLevel("Adloquium") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(170, 0))
							{
								await AdloquiumPlayer(charDATA.MemberNumber);
							}
							if (autoFlurryEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry") && HasAcquiredSpell("Flurry") && HasRequiredJobLevel("Flurry") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(581, 0) && !plHasBuff(StatusEffect.Slow))
							{
								await FlurryPlayer(charDATA.MemberNumber);
							}
							if (autoFlurry_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry II") && HasAcquiredSpell("Flurry II") && HasRequiredJobLevel("Flurry II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(581, 0) && !plHasBuff(StatusEffect.Slow))
							{
								await Flurry_IIPlayer(charDATA.MemberNumber);
							}
							if (autoShell_Enabled[charDATA.MemberNumber] && SpellReadyToCast(shell_spells[Form2.config.autoShell_Spell]) && HasAcquiredSpell(shell_spells[Form2.config.autoShell_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && !plHasBuff(StatusEffect.Shell))
							{
								await shellPlayer(charDATA.MemberNumber);
							}
							if (autoProtect_Enabled[charDATA.MemberNumber] && SpellReadyToCast(protect_spells[Form2.config.autoProtect_Spell]) && HasAcquiredSpell(protect_spells[Form2.config.autoProtect_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && !plHasBuff(StatusEffect.Protect))
							{
								await protectPlayer(charDATA.MemberNumber);
							}
							if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && CanCastSpell("Phalanx II") && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !plHasBuff(StatusEffect.Phalanx))
							{
								await Phalanx_IIPlayer(charDATA.MemberNumber);
							}
							if ((autoRegen_Enabled[charDATA.MemberNumber]) && (CanCastSpell(regen_spells[Form2.config.autoRegen_Spell])) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !plHasBuff(StatusEffect.Regen))
							{
								await Regen_Player(charDATA.MemberNumber);
							}
							if ((autoRefreshEnabled[charDATA.MemberNumber]) && (CanCastSpell(refresh_spells[Form2.config.autoRefresh_Spell])) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !plHasBuff(StatusEffect.Refresh))
							{
								await Refresh_Player(charDATA.MemberNumber);
							}
							if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !hasBuff(PTstormspell.BuffId, 0) && SpellReadyToCast(PTstormspell.Name) && HasAcquiredSpell(PTstormspell.Name) && HasRequiredJobLevel(PTstormspell.Name) == true)
							{
								await StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Name);
							}
						}
						// MONITORED PLAYER BASED BUFFS
						else if (instanceMonitored.Player.Name == charDATA.Name)
						{
							if (autoHasteEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste") && HasAcquiredSpell("Haste") && HasRequiredJobLevel("Haste") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
							{
								await hastePlayer(charDATA.MemberNumber);
							}
							if (autoHaste_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste II") && HasAcquiredSpell("Haste II") && HasRequiredJobLevel("Haste II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
							{
								await haste_IIPlayer(charDATA.MemberNumber);
							}
							if (autoAdloquium_Enabled[charDATA.MemberNumber] && SpellReadyToCast("Adloquium") && HasAcquiredSpell("Adloquium") && HasRequiredJobLevel("Adloquium") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(170, 1))
							{
								await AdloquiumPlayer(charDATA.MemberNumber);
							}
							if (autoFlurryEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry") && HasAcquiredSpell("Flurry") && HasRequiredJobLevel("Flurry") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(581, 1) && !monitoredStatusCheck(StatusEffect.Slow))
							{
								await FlurryPlayer(charDATA.MemberNumber);
							}
							if (autoFlurry_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry II") && HasAcquiredSpell("Flurry II") && HasRequiredJobLevel("Flurry II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && !hasBuff(581, 1) && !monitoredStatusCheck(StatusEffect.Slow))
							{
								await Flurry_IIPlayer(charDATA.MemberNumber);
							}
							if (autoShell_Enabled[charDATA.MemberNumber] && SpellReadyToCast(shell_spells[Form2.config.autoShell_Spell]) && HasAcquiredSpell(shell_spells[Form2.config.autoShell_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Shell))
							{
								await shellPlayer(charDATA.MemberNumber);
							}
							if (autoProtect_Enabled[charDATA.MemberNumber] && SpellReadyToCast(protect_spells[Form2.config.autoProtect_Spell]) && HasAcquiredSpell(protect_spells[Form2.config.autoProtect_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Protect))
							{
								await protectPlayer(charDATA.MemberNumber);
							}
							if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && CanCastSpell("Phalanx II") && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Phalanx))
							{
								await Phalanx_IIPlayer(charDATA.MemberNumber);
							}
							if ((autoRegen_Enabled[charDATA.MemberNumber]) && CanCastSpell(regen_spells[Form2.config.autoRegen_Spell]) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Regen))
							{
								await Regen_Player(charDATA.MemberNumber);
							}
							if ((autoRefreshEnabled[charDATA.MemberNumber]) && (CanCastSpell(refresh_spells[Form2.config.autoRefresh_Spell])) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Refresh))
							{
								await Refresh_Player(charDATA.MemberNumber);
							}
							if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && !hasBuff(PTstormspell.BuffId, 1) && SpellReadyToCast(PTstormspell.Name) && HasAcquiredSpell(PTstormspell.Name) && HasRequiredJobLevel(PTstormspell.Name) == true)
							{
								await StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Name);
							}
						}
						else
						{
							if (autoHasteEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste") && HasAcquiredSpell("Haste") && HasRequiredJobLevel("Haste") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && playerHasteSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
							{
								await hastePlayer(charDATA.MemberNumber);
							}
							if (autoHaste_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Haste II") && HasAcquiredSpell("Haste II") && HasRequiredJobLevel("Haste II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && playerHaste_IISpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
							{
								await haste_IIPlayer(charDATA.MemberNumber);
							}
							if (autoAdloquium_Enabled[charDATA.MemberNumber] && SpellReadyToCast("Adloquium") && HasAcquiredSpell("Adloquium") && HasRequiredJobLevel("Adloquium") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && playerAdloquium_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoAdloquiumMinutes)
							{
								await AdloquiumPlayer(charDATA.MemberNumber);
							}
							if (autoFlurryEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry") && HasAcquiredSpell("Flurry") && HasRequiredJobLevel("Flurry") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && playerFlurrySpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
							{
								await FlurryPlayer(charDATA.MemberNumber);
							}
							if (autoFlurry_IIEnabled[charDATA.MemberNumber] && SpellReadyToCast("Flurry II") && HasAcquiredSpell("Flurry II") && HasRequiredJobLevel("Flurry II") == true && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && playerHasteSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoHasteMinutes)
							{
								await Flurry_IIPlayer(charDATA.MemberNumber);
							}
							if (autoShell_Enabled[charDATA.MemberNumber] && SpellReadyToCast(shell_spells[Form2.config.autoShell_Spell]) && HasAcquiredSpell(shell_spells[Form2.config.autoShell_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && playerShell_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoShellMinutes)
							{
								await shellPlayer(charDATA.MemberNumber);
							}
							if (autoProtect_Enabled[charDATA.MemberNumber] && SpellReadyToCast(protect_spells[Form2.config.autoProtect_Spell]) && HasAcquiredSpell(protect_spells[Form2.config.autoProtect_Spell]) && instancePrimary.Player.MP > Form2.config.mpMinCastValue && castingPossible(charDATA.MemberNumber) && instancePrimary.Player.Status != 33 && playerProtect_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoProtect_Minutes)
							{
								await protectPlayer(charDATA.MemberNumber);
							}
							if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && CanCastSpell("Phalanx II") && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && playerPhalanx_IISpan[charDATA.MemberNumber].Minutes >= Form2.config.autoPhalanxIIMinutes)
							{
								await Phalanx_IIPlayer(charDATA.MemberNumber);
							}
							if ((autoRegen_Enabled[charDATA.MemberNumber]) && CanCastSpell(regen_spells[Form2.config.autoRegen_Spell]) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && playerRegen_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoRegen_Minutes)
							{
								await Regen_Player(charDATA.MemberNumber);
							}
							if ((autoRefreshEnabled[charDATA.MemberNumber]) && CanCastSpell(refresh_spells[Form2.config.autoRefresh_Spell]) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && playerRefresh_Span[charDATA.MemberNumber].Minutes >= Form2.config.autoRefresh_Minutes)
							{
								await Refresh_Player(charDATA.MemberNumber);
							}
							if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (instancePrimary.Player.MP > Form2.config.mpMinCastValue) && (castingPossible(charDATA.MemberNumber)) && instancePrimary.Player.Status != 33 && SpellReadyToCast(PTstormspell.Name) && HasAcquiredSpell(PTstormspell.Name) && HasRequiredJobLevel(PTstormspell.Name) == true && playerStormspellSpan[charDATA.MemberNumber].Minutes >= Form2.config.autoStormspellMinutes)
							{
								await StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Name);
							}
						}
					}
				}
			}
		}

		private async Task UseItem(string name)
		{
			if (!(await casting.WaitAsync(15000)))
			{
				throw new InvalidOperationException();
			}

			try
			{
				SetLockLabel("Casting LOCKED");
				SetCurrentAction($"Using item: {name}");
				await WaitForCastingToFinish();
				await SendPrimaryCommand($"/item \"{name}\" <me>", 3000);
				SetLockLabel("Casting UNLOCKED");
				SetCurrentAction(string.Empty);
			}
			finally
			{
				casting.Release();
			}
		}

		private async Task UseJobAbility(string name)
		{
			if (!(await casting.WaitAsync(15000)))
			{
				throw new InvalidOperationException();
			}

			try
			{
				SetLockLabel("Casting LOCKED");
				SetCurrentAction($"Using ability: {name}");
				await WaitForCastingToFinish();
				await SendPrimaryCommand($"/ja \"{name}\" <me>", 3000);
				SetLockLabel("Casting UNLOCKED");
				SetCurrentAction(string.Empty);
			}
			finally
			{
				casting.Release();
			}
		}

		private async Task ApplyPrecastAbilities(int songsActive)
		{
			if (Form2.config.DivineSeal && instancePrimary.Player.MPP <= 11 && CanUseJobAbility("Divine Seal") && !instancePrimary.Player.Buffs.Contains((short)StatusEffect.Weakness))
			{
				await UseJobAbility("Divine Seal");
			}
			else if (Form2.config.RadialArcana && (instancePrimary.Player.MP <= Form2.config.RadialArcanaMP) && CanUseJobAbility("Radial Arcana") && !instancePrimary.Player.Buffs.Contains((short)StatusEffect.Weakness))
			{
				// Check if a pet is already active
				if (instancePrimary.Player.Pet.HealthPercent >= 1 && instancePrimary.Player.Pet.Distance <= 9)
				{
					await UseJobAbility("Radial Arcana");
				}
				else if (instancePrimary.Player.Pet.HealthPercent >= 1 && instancePrimary.Player.Pet.Distance >= 9 && CanUseJobAbility("Full Circle"))
				{
					await UseJobAbility("Full Circle");
					var SpellCheckedResult = ReturnGeoSpell(Form2.config.RadialArcana_Spell, 2);
					await CastSpell("<me>", SpellCheckedResult);
				}
				else
				{
					var SpellCheckedResult = ReturnGeoSpell(Form2.config.RadialArcana_Spell, 2);
					await CastSpell("<me>", SpellCheckedResult);
				}
			}
			else if (Form2.config.FullCircle)
			{
				// When out of range Distance is 59 Yalms regardless. Must be within 15 yalms to gain the effect.
				// Check if "pet" is active and out of range of the monitored player
				if (instancePrimary.Player.Pet.HealthPercent >= 1)
				{
					if (Form2.config.Fullcircle_GEOTarget == true && Form2.config.LuopanSpell_Target != "")
					{
						var petIndex = instancePrimary.Player.PetIndex;
						var petEntity = instancePrimary.Entity.GetEntity(petIndex);
						var fcTargetId = 0;

						for (var x = 0; x < 2048; x++)
						{
							var entity = instancePrimary.Entity.GetEntity(x);
							if (entity.Name != null && entity.Name.ToLower().Equals(Form2.config.LuopanSpell_Target.ToLower()))
							{
								fcTargetId = Convert.ToInt32(entity.TargetID);
								break;
							}
						}

						if (fcTargetId != 0)
						{
							var fcEntity = instancePrimary.Entity.GetEntity(fcTargetId);

							var fX = petEntity.X - fcEntity.X;
							var fY = petEntity.Y - fcEntity.Y;
							var fZ = petEntity.Z - fcEntity.Z;

							var generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

							if (generatedDistance >= 10)
							{
								FullCircle_Timer.Enabled = true;
							}
						}

					}
					else if (Form2.config.Fullcircle_GEOTarget == false && instanceMonitored.Player.Status == 1)
					{
						var PetsIndex = instancePrimary.Player.PetIndex;

						var PetsEntity = instanceMonitored.Entity.GetEntity(PetsIndex);

						if (PetsEntity.Distance >= 10)
						{
							FullCircle_Timer.Enabled = true;
						}
					}

				}
			}
			else if ((Form2.config.Troubadour) && CanUseJobAbility("Troubadour") && songsActive == 0)
			{
				await UseJobAbility("Troubadour");
			}
			else if ((Form2.config.Nightingale) && CanUseJobAbility("Nightingale") && songsActive == 0)
			{
				await UseJobAbility("Nightingale");
			}
		}

		private async Task RemoveCriticalDebuffsFromPL()
		{
			if (plHasBuff(StatusEffect.Silence) && Form2.config.plSilenceItemEnabled)
			{
				var itemId = GetItemId(plSilenceitemName);
				var inventoryCount = GetInventoryItemCount(instancePrimary, itemId);
				var tempItemCount = GetTempItemCount(instancePrimary, itemId);
				if (inventoryCount + tempItemCount > 0)
				{
					await UseItem(plSilenceitemName);
				}
			}

			else if ((plHasBuff(StatusEffect.Doom) && Form2.config.plDoomEnabled))
			{
				var itemId = GetItemId(plDoomItemName);
				var inventoryCount = GetInventoryItemCount(instancePrimary, itemId);
				var tempItemCount = GetTempItemCount(instancePrimary, itemId);
				if (inventoryCount + tempItemCount > 0)
				{
					await UseItem(plDoomItemName);
				}
			}
		}

		private async Task<bool> ConvertIfNecessary()
		{
			if (Form2.config.Convert &&
					CanUseJobAbility("Convert") &&
					!plHasBuff(Buffs.weakness) &&
					instancePrimary.Player.MP <= Form2.config.convertMP)
			{
				await UseJobAbility("Convert");
				return true;
			}

			return false;
		}

		private void SetLockLabel(string value)
		{
			Invoke((MethodInvoker)(() =>
			{
				castingLockLabel.Text = value;
			}));
		}

		private void SetCurrentAction(string value)
		{
			Invoke((MethodInvoker)(() =>
			{
				currentAction.Text = value;
			}));
		}

		private void HandleLowMpSituations()
		{
			if (instancePrimary.Player.MP <= (int)Form2.config.mpMinCastValue &&
					!waitingForMp)
			{
				waitingForMp = true;
				if (Form2.config.lowMPcheckBox && !waitingForMp && !Form2.config.healLowMP)
				{
					instancePrimary.ThirdParty.SendString("/tell " + instanceMonitored.Player.Name + " MP is low!");
				}
			}

			// tell PL on mp recovered
			if (instancePrimary.Player.MP > (int)Form2.config.mpMinCastValue &&
					waitingForMp)
			{
				waitingForMp = false;
				if (Form2.config.lowMPcheckBox && waitingForMp && !Form2.config.healLowMP)
				{
					instancePrimary.ThirdParty.SendString("/tell " + instanceMonitored.Player.Name + " MP OK!");
				}
			}

			// heal on low mp
			if (Form2.config.healLowMP &&
					instancePrimary.Player.MP <= Form2.config.healWhenMPBelow &&
					instancePrimary.Player.Status == (uint)EntityStatus.Idle &&
					!healingForMp)
			{
				healingForMp = true;
				instancePrimary.ThirdParty.SendString("/heal on");

				if (Form2.config.lowMPcheckBox)
				{
					instancePrimary.ThirdParty.SendString("/tell " + instanceMonitored.Player.Name + " MP is seriously low, /healing.");
				}
			}

			// stand on mp recovered
			if (Form2.config.standAtMP &&
					instancePrimary.Player.MPP >= Form2.config.standAtMP_Percentage &&
					instancePrimary.Player.Status == (uint)EntityStatus.Idle &&
					healingForMp)
			{
				healingForMp = false;
				instancePrimary.ThirdParty.SendString("/heal off");

				if (Form2.config.lowMPcheckBox)
				{
					instancePrimary.ThirdParty.SendString("/tell " + instanceMonitored.Player.Name + " MP has recovered.");
				}
			}
		}
	}

	// END OF THE FORM SCRIPT

	public static class RichTextBoxExtensions
	{
		public static void AppendText(this RichTextBox box, string text, Color color)
		{
			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;

			box.SelectionColor = color;
			box.AppendText(text);
			box.SelectionColor = box.ForeColor;
		}
	}
}
