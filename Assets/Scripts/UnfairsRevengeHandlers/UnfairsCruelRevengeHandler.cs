﻿using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uernd = UnityEngine.Random;
public class UnfairsCruelRevengeHandler : MonoBehaviour {

	public KMBombInfo bombInfo;
	public KMAudio mAudio;
	public KMBombModule modSelf;
	public KMSelectable[] colorButtonSelectables;
	public KMSelectable innerSelectable, outerSelectable, idxStrikeSelectable;
	public GameObject[] colorButtonObjects;
	public GameObject innerRing, entireCircle, animBar;
	public MeshRenderer[] colorButtonRenderers, statusIndicators;
	public TextMesh pigpenDisplay, strikeIDDisplay, mainDisplay, pigpenSecondary;
	public Light[] colorLights;
	public Light centerLight;
	public ParticleSystem particles;
	public IndicatorCoreHandlerEX indicatorCoreHandlerEX;
	public KMColorblindMode colorblindMode;
	public Material[] switchableMats = new Material[2];
	public KMGameInfo gameInfo;
	public ProgressBarHandler progressHandler;

	private string[]
		hardModeInstructions = { "PCR", "PCG", "PCB", "SCC", "SCM", "SCY", "SUB", "PVP", "NXP", "PVS", "NXS", "REP", "EAT", "STR", "IKE", "PRN" ,"CHK", "MOT", "OPP", "SKP" },
		baseColorList = new[] { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta" },
		primaryList = { "Red", "Green", "Blue", };
	private string baseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // Base alphabet for code assumes A=1,B=2,...,Y=25,Z=26
		displayPigpenText = "", extraKey = "", selectedWord = "";
	private string[,] keyBTable = {
			{ "ALPH", "ONE", "ABCD", "AEI", "PLAY", "JAKK", "FRLA", "ZEKN", "FIZZ", "HEND", "CLUT", "SCG" },
			{ "BETA", "TWO", "EFGH", "OUY", "HIDE", "MCDU", "VIRE", "ELIA", "TIMW", "ACRY", "MAGE", "BASH" },
			{ "CHAR", "THRE", "IJKL", "WBC", "SECR", "EOTA", "IONL", "REXK", "MOON", "ONYX", "SPAR", "MOCK" },
			{ "DELT", "FOUR", "MNOP", "DFG", "CIPH", "CAIT", "LEGN", "RIVE", "TAOO", "SAMD", "KONQ", "BRIN" },
			{ "ECHO", "FIVE", "QRST", "HJK", "FAIL", "MARA", "WILL", "TRAI", "LUPO", "ELUM", "FLAM", "KANE" },
			{ "FOXT", "SIX", "UVWX", "LMN", "PART", "WARI", "SKIP", "NANT", "LUMB", "FLUS", "MOMO", "HEXI" },
			{ "GOLF", "SEVN", "YZAB", "PQR", "BECO", "PIGD", "ETRS", "GRYB", "CATN", "ASIM", "MITT", "PERK" },
	};
	private string[] myszkowskiKeywords = {
		"ARCHER", "ATTACK", "BANANA", "BLASTS", "BURSTS", "BUTTON", "CANNON",
		"CALLER", "CELLAR", "DAMAGE", "DEFUSE", "DEVICE", "KABOOM", "LETTER",
		"LOOPED", "MORTAR", "NAPALM", "OTTAWA", "PAPERS", "POODLE", "POOLED",
		"RASHES", "RECALL", "ROBOTS", "SAPPER", "SHARES", "SHEARS", "WIRING",
	}, tableRoman = {
		"Fixed Roman",
		"Broken Roman",
		"Arabic",
    }, wordSearchWordsEven = {
        "HOTEL", "SEARCH", "ADD", "SIERRA", "FINISH",
		"PORT", "BOOM", "LINE", "KABOOM", "PANIC", "MANUAL", "DECOY",
		"SEE", "INDIA", "NUMBER", "ZULU","VICTOR", "DELTA", "HELP",
		"ROMEO", "TRUE","MIKE", "FOUND","BOMBS","WORK", "TEST",
		"GOLF", "TALK","BRAVO", "SEVEN", "MODULE", "LIST", "YANKEE",
		"CHART", "MATH", "READ", "LIMA", "COUNT",
    }, wordSearchWordsOdd = {
		"DONE", "QUEBEC", "CHECK", "FIND", "EAST",
		"COLOR", "SUBMIT", "BLUE", "ECHO", "FALSE", "ALARM", "CALL",
		"TWENTY", "NORTH", "LOOK", "GREEN", "XRAY", "YES", "LOCATE",
        "BEEP", "EXPERT", "EDGE", "RED", "WORD", "UNIQUE", "JINX",
		"LETTER", "SIX", "SERIAL", "TIMER", "SPELL", "TANGO", "SOLVE",
		"OSCAR", "NEXT", "LISTEN", "FOUR", "OFFICE",
	};
	private string[][] anagramValues = new string[][]
	{
		new string[] { "TAMERS", "STREAM", "MASTER", "ARM SET", "MRS TEA", "MR SEAT" },
		new string[] { "BARELY", "BARLEY", "BLEARY", "LAB RYE", "A BERYL", "ALB RYE" },
		new string[] { "RUDEST", "DUSTER", "RUSTED", "ED RUST", "EDS RUT", "DUST RE" },
		new string[] { "IDEALS", "SAILED", "LADIES", "A SLIDE", "DEAL IS", "SEA LID" },
	};

	DayOfWeek[] possibleDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, };
	private static int[] modIDList;
	private static int lastModIDCnt;
	private static int modIDCnt;
	private int loggingModID, selectedModID, currentInputPos = 0, localStrikeCount = 0, currentScreenVal = 0, idxCurModIDDisplay = 0, idxCurStrikeDisplay = 0;
	IEnumerator currentlyRunning;
	IEnumerator[] colorsFlashing = new IEnumerator[6];
	bool isplayingSolveAnim, hasStarted, colorblindDetected, isAnimatingStart, isFinished, hasStruck = false, autoCycleEnabled = false, swapPigpenAndStandard = false, swapStandardKeys = false, noCopyright;
	UnfairsCruelRevengeSettings ucrSettings = new UnfairsCruelRevengeSettings();

	private Color[] colorWheel = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
	private int[] idxColorList = { 0, 1, 2, 3, 4, 5 }, columnalTranspositionLst = new int[] { 0, 1, 2, 3, 4, 5 };
	List<string> lastCorrectInputs = new List<string>(), splittedInstructions = new List<string>();
	void Awake()
	{
		try
        {
			ModConfig<UnfairsCruelRevengeSettings> modConfig = new ModConfig<UnfairsCruelRevengeSettings>("UnfairsCruelRevengeSettings");
			ucrSettings = modConfig.Settings;
			noCopyright = ucrSettings.noCopyright;
        }
		catch
        {
			Debug.LogWarningFormat("<Unfair's Cruel Revenge Settings>: Settings for Unfair's Cruel Revenge do not work as intended! Using default settings instead.", loggingModID);
			noCopyright = true;
		}
		finally
		{
			try
			{
				colorblindDetected = colorblindMode.ColorblindModeActive;
			}
			catch
			{
				colorblindDetected = false;
			}
		}
	}
	// Use this for initialization
	void Start() {
		loggingModID = ++modIDCnt;
		if (modIDList == null || loggingModID - lastModIDCnt >= modIDList.Length)
		{
			lastModIDCnt = modIDCnt;
			modIDList = new int[26];
			for (int x = 0; x < 26; x++)
			{
				modIDList[x] = x + lastModIDCnt;
			}
			modIDList = modIDList.OrderBy(x => uernd.Range(int.MinValue, int.MaxValue)).ToArray();
		}
		selectedModID = modIDList[loggingModID - lastModIDCnt];
		//selectedModID = 38;

		modSelf.OnActivate += delegate
		{
			StopCoroutine(currentlyRunning);

			PrepModule();
			
			hasStarted = true;
			LogCurrentInstruction();
			UpdateSecondaryScreen();
			UpdateStatusIndc();
		};
		for (int x = 0; x < colorButtonSelectables.Length; x++)
		{
			int y = x;
			colorButtonSelectables[x].OnInteract += delegate
			{
				colorButtonSelectables[y].AddInteractionPunch(0.1f);
				if (!isFinished)
				{
					StopCoroutine(colorsFlashing[y]);
					colorsFlashing[y] = HandleFlashingAnim(y);
					StartCoroutine(colorsFlashing[y]);
					ProcessInstruction(baseColorList[idxColorList[y]]);
				}
				return false;
			};

			colorButtonSelectables[x].OnHighlight += delegate
			{
				string[] directionSamples = { "NW", "N", "NE", "SE", "S", "SW" };
				if (!isAnimatingStart && colorblindDetected && hasStarted && !isFinished)
				{
					mainDisplay.text = string.Format("{0} Button:\n{1}", directionSamples[y], baseColorList[idxColorList[y]]);
					mainDisplay.color = Color.white;
					pigpenDisplay.text = "";
				}
			};
			colorButtonSelectables[x].OnHighlightEnded += delegate
			{
				if (!isAnimatingStart && colorblindDetected && hasStarted && !isFinished)
				{
					mainDisplay.text = "";
					pigpenDisplay.text = displayPigpenText;
				}
			};

			colorsFlashing[x] = HandleFlashingAnim(y);
		}
		innerSelectable.OnInteract += delegate
		{
			innerSelectable.AddInteractionPunch(0.1f);
			ProcessInstruction("Inner");
			StartCoroutine(HandlePressAnim(innerSelectable.gameObject));
			return false;
		};
		outerSelectable.OnInteract += delegate
		{
			outerSelectable.AddInteractionPunch(0.1f);
			ProcessInstruction("Outer");
			return false;
		};
		idxStrikeSelectable.OnInteract += delegate
		{
			if (!isFinished)
			{
				currentScreenVal = (currentScreenVal + 1) % 3;
				UpdateSecondaryScreen();
			}
			return false;
		};
		currentlyRunning = SampleStandardText();
		StartCoroutine(currentlyRunning);
		entireCircle.SetActive(false);
		pigpenDisplay.text = "";
		strikeIDDisplay.text = "";
		mainDisplay.text = "";
		pigpenSecondary.text = "";
		float rangeModifier = modSelf.gameObject.transform.lossyScale.x;
		centerLight.range *= rangeModifier;
		for (int x = 0; x < colorLights.Length; x++)
		{
			colorLights[x].range *= rangeModifier;
		}
		gameInfo.OnLightsChange += delegate(bool turnedOn)
		{
			if (isFinished) return;
			for (int i = 0; i < colorButtonRenderers.Length; i++)
			{
				colorButtonRenderers[i].material = turnedOn ? switchableMats[0] : switchableMats[1] ;
				colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.75f;
			}
		};
		if (Application.isEditor)
		{
			Debug.LogFormat("[Unfair's Revenge #{0}]: Unity Editor Mode is active, if TP is enabled, you may use \"!# simulate on/off to simulate lights turning on or off.\"", loggingModID);
		}
		StartCoroutine(HandleAutoCycleAnim(false));
	}

	void UpdateSecondaryScreen()
	{
		string toDisplay = "";
		switch (currentScreenVal)
		{
			case 0:
				{
					switch (idxCurModIDDisplay)
					{
						case 0:
							toDisplay = ValueToFixedRoman(selectedModID);
							break;
						case 1:
							toDisplay = ValueToBrokenRoman(selectedModID);
							break;
						case 2:
							toDisplay = selectedModID.ToString();
							break;
					}
					strikeIDDisplay.text = string.Format("Module ID:\n{0}", toDisplay);
					strikeIDDisplay.color = Color.white;
					pigpenSecondary.text = "";
					break;
				}
			case 1:
				{
					strikeIDDisplay.color = Color.red;
					pigpenSecondary.text = "";
					break;
				}
			case 2:
				{
					strikeIDDisplay.color = Color.white;
					if (swapPigpenAndStandard)
					{
						if (swapStandardKeys)
							strikeIDDisplay.text = string.Format("\n{1} | {0}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord);
						else
							strikeIDDisplay.text = string.Format("\n{0} | {1}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord);
						pigpenSecondary.text = string.Format("{0}\n", extraKey);
					}
					else
					{
						if (swapStandardKeys)
							strikeIDDisplay.text = string.Format("{1} | {0}\n", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord);
						else
							strikeIDDisplay.text = string.Format("{0} | {1}\n", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord);
						pigpenSecondary.text = string.Format("\n{0}", extraKey);
					}
					break;
				}

		}
	}
	void PrepModule()
	{
		idxColorList.Shuffle();
		List<string> curColorList = idxColorList.Select(a => baseColorList[a]).ToList();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Button colors in clockwise order (starting on the NW button): {1}", loggingModID, curColorList.Join(", "));
		StartCoroutine(HandleStartUpAnim());
		//StartCoroutine(TypePigpenText(FitToScreen("ABCDEFGHIJKLMNOPQRSTVUWXYZABCDEFGHIJKLM",13)));
		GenerateInstructions();
		mainDisplay.text = "";
		// Basic Columnar Transposition Set
		int[] possibleSizes = { 2, 3, 6, 9 };
		columnalTranspositionLst = new int[possibleSizes.PickRandom()];
		for (int x = 0; x < columnalTranspositionLst.Length; x++)
		{
			columnalTranspositionLst[x] = x;
		}

		columnalTranspositionLst.Shuffle();
		// Extra Pigpen Key
		for (int x = 0; x < 12;x++)
		{
			extraKey += baseAlphabet.PickRandom();
		}

		// Autokey Keyword Mislead
		int randomIdx = uernd.Range(0, wordSearchWordsEven.Length);
		bool useEven = uernd.Range(0, 2) == 1;
		selectedWord = useEven ? wordSearchWordsOdd[randomIdx] : wordSearchWordsEven[randomIdx];

		idxCurModIDDisplay = uernd.Range(0, 3);
		idxCurStrikeDisplay = uernd.Range(0, 3);
		swapPigpenAndStandard = uernd.Range(0, 2) == 1;
		swapStandardKeys = uernd.Range(0, 2) == 1;


		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Mod ID grabbed: {1} Keep in mind this can differ from the ID used for logging!", loggingModID, selectedModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Mod ID is in {1} Numerals", loggingModID, tableRoman[idxCurModIDDisplay]);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The strike counter is in {1} Numerals", loggingModID, tableRoman[idxCurStrikeDisplay]);
		// Value A Calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value A Calculations-------------", loggingModID);
		int valueA = 0;
		char[] vowelList = { 'A', 'E', 'I', 'O', 'U' };
		int portTypeCount = bombInfo.GetPorts().Distinct().Count(),
			portPlateCount = bombInfo.GetPortPlateCount(),
			consonantCount = bombInfo.GetSerialNumberLetters().Where(a => !vowelList.Contains(a)).Count(),
			vowelCount = bombInfo.GetSerialNumberLetters().Where(a => vowelList.Contains(a)).Count(),
			litCount = bombInfo.GetOnIndicators().Count(),
			unlitCount = bombInfo.GetOffIndicators().Count(),
			batteryCount = bombInfo.GetBatteryCount();
		// For every port type
		valueA -= 2 * portTypeCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many distant port types: {1}, Value A logged at {2}", loggingModID, portTypeCount, valueA);
		// For every port plate
		valueA += 1 * portPlateCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many port plates: {1}, Value A logged at {2}", loggingModID, portPlateCount, valueA);
		// For every consonant in the serial number
		valueA += 1 * consonantCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many consonants in the serial number: {1}, Value A logged at {2}", loggingModID, consonantCount, valueA);
		// For every vowel in the serial number
		valueA -= 2 * vowelCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many vowels in the serial number: {1}, Value A logged at {2}", loggingModID, vowelCount, valueA);
		// For every lit indicator
		valueA += 2 * litCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many lit indicators: {1}, Value A logged at {2}", loggingModID, litCount, valueA);
		// For every unlit indicator
		valueA -= 2 * unlitCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many unlit indicators: {1}, Value A logged at {2}", loggingModID, unlitCount, valueA);
		if (batteryCount == 0)
			valueA += 10;
		else
			valueA -= 1 * batteryCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many batteries: {1}, Value A logged at {2}", loggingModID, batteryCount, valueA);
		if (bombInfo.GetPortCount() == 0)
		{
			valueA *= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are no ports. Value A logged at {1}", loggingModID, valueA);
		}
		if (bombInfo.GetSolvableModuleIDs().Count() >= 31)
		{
			valueA /= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are 31 or more modules on the bomb, including itself. Value A logged at {1}", loggingModID, valueA);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		// Value X calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value X Calculations-------------", loggingModID);
		int valueX = 0;
		Dictionary<string, int> indicatorMultipler = new Dictionary<string, int> {
			{"BOB", 1 },{"CAR", 1 },{"CLR", 1 },
			{"FRK", 2 },{"FRQ", 2 },{"MSA", 2 },{"NSA", 2 },
			{"SIG", 3 },{"SND", 3 },{"TRN", 3 },
		};
		foreach (string ind in bombInfo.GetIndicators())
		{
			if (indicatorMultipler.ContainsKey(ind))
			{
				valueX += indicatorMultipler[ind] * (bombInfo.IsIndicatorOff(ind) ? -1 : bombInfo.IsIndicatorOn(ind) ? 1 : 0);
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After indicators: X = {1}", loggingModID, valueX);
		valueX += 4 * (bombInfo.GetBatteryCount() % 2 == 1 ? 1 : -1);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After battery count: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("Parallel")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("Serial") ? -4 : 5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with parallel ports: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("DVI")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("StereoRCA") ? 4 : -5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with DVI-D ports: X = {1}", loggingModID, valueX);
		valueX = Mathf.Abs(valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After absolute value: X = {1}", loggingModID, valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		int monthOfStart = DateTime.Now.Month;
		int idxStartDOW = Array.IndexOf(possibleDays, DateTime.Now.DayOfWeek);
		string keyAString = ObtainKeyA();
		string keyBString = keyBTable[idxStartDOW, monthOfStart - 1];
		string keyCString = EncryptUsingPlayfair(keyAString, keyBString);
		string keyDString = ObtainKeyD();

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key A: {1}", loggingModID, keyAString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key B: {1}", loggingModID, keyBString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key C: {1}", loggingModID, keyCString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key D: {1}", loggingModID, keyDString);

		ModifyBaseAlphabet();
		// Distinguishing Ciphers
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Which Ciphers Are Used------------", loggingModID);
		string[] baseCipherList = {
			"Playfair Cipher (Key A)",
			"Playfair Cipher (Key B)",
			"Playfair Cipher (Key C)",
			"Playfair Cipher (Key D)",
			"Caesar Cipher (Value A)",
			"ROT13 Cipher",
			"Affine Cipher (Value X)",
			"Atbash Cipher",
			"Basic Columnar Transposition",
			"Myszkowski Transposition",
			"Anagram Shuffler",
			"Scytale Transposition",
			"Autokey Cipher",
			"Four Square Cipher",
			"Redefence Transposition"
		};
		int[] idxCipherList = new int[] { 2, 6, 4, 0, 1, 3, 13, 5, 7, 9, 12, 11, 8, 10, 14 };
		/* The Ciphers are given based on a given set of instructions. Indexes are defined by the following:
		 * 0, 1, 2, 3 are Playfair Ciphers with keys A,B,C,D respectively.
		 * 4, 5 are Caesar Cipher with values A and 13 respectively.
		 * 6 is Affine Cipher with value x.
		 * 7 is Atbash Cipher
		 * 8 is Basic Columnar Transposition
		 * 9 is Myszkowski Transposition
		 * 10 is Anagram Shuffler
		 * 11 is Scytale Transposition
		 * 12 is Autokey Cipher
		 * 13 is Four Square Cipher
		 * 14 is Redefence Transposition
		 */
		List<string> allModIDs = bombInfo.GetModuleIDs();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Conditions Taken:", loggingModID);
		if (idxCurModIDDisplay != 2)
        {
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Module ID is displayed in Broken or Fixed Roman Numerals.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 3 ? 0 : 1).ToArray();
        }
		if (idxCurStrikeDisplay == 2)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Strike Counter is displayed in Arabic Numerals.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 5 ? 0 : a == 11 ? 2 : 1).ToArray();
		}
		if (!allModIDs.Contains("unfairCipher"))
        {
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unfair Cipher is not present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => new int[] { 0, 2, 4 }.Contains(a) ? 1 : 0).ToArray();
		}
		if (allModIDs.Contains("orangeCipher"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Orange Cipher is present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 13 ? 0 : 1).ToArray();
		}
		if (!allModIDs.Contains("Alphabetize") && allModIDs.Contains("ReverseAlphabetize"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Alphabetize is not present but Reverse Alphabetize is.", loggingModID);
			int idxRT = Array.IndexOf(idxCipherList, 12), idxAB = Array.IndexOf(idxCipherList, 7);
			int temp = idxCipherList[idxAB];
			idxCipherList[idxAB] = idxCipherList[idxRT];
			idxCipherList[idxRT] = temp;
		}
		if (allModIDs.Contains("CryptModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Cryptography is present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a % 2 == 1 ? 26 - a : 26).ToArray();
		}
		if (allModIDs.Contains("AnagramsModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Anagrams is present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 10 ? 0 : 1).ToArray();
		}
		if (allModIDs.Contains("WordScrambleModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Word Scramble is present.", loggingModID);
			int temp = idxCipherList[2];
			idxCipherList[2] = idxCipherList[idxCipherList.Length - 3];
			idxCipherList[idxCipherList.Length - 3] = temp;
		}
		if (allModIDs.Contains("blackCipher"))
        {
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Black Cipher is present.", loggingModID);
			int idxRT = Array.IndexOf(idxCipherList, 14), firstId = idxCipherList.FirstOrDefault();
			idxCipherList[idxRT] = firstId;
			idxCipherList[0] = 14;

			if (columnalTranspositionLst.Length <= 3)
            {
				int lastId = idxCipherList.LastOrDefault(), idxBCT = Array.IndexOf(idxCipherList, 8);
				idxCipherList[idxRT] = lastId;
                idxCipherList[idxCipherList.Length - 1] = 8;
			}
		}
		if (curColorList[2] == "Yellow")
        {
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The NE colored button is Yellow.", loggingModID);
			IEnumerable<int> firstFour = idxCipherList.Take(4);
			idxCipherList = idxCipherList.OrderBy(a => firstFour.Contains(a) ? 1 : 0).ToArray();
		}
		if (Mathf.Abs(curColorList.IndexOf("Red") - curColorList.IndexOf("Cyan")) == 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Red is diametrically opposite to Cyan.", loggingModID);
			int idxR13 = Array.IndexOf(idxCipherList, 5), idxAS = Array.IndexOf(idxCipherList, 10), idxBCT = Array.IndexOf(idxCipherList, 8), idxMT = Array.IndexOf(idxCipherList, 9);
			int temp = idxCipherList[idxMT];
			idxCipherList[idxMT] = idxCipherList[idxR13];
			idxCipherList[idxR13] = temp;
			temp = idxCipherList[idxBCT];
			idxCipherList[idxBCT] = idxCipherList[idxAS];
			idxCipherList[idxAS] = temp;
		}
		if (curColorList.IndexOf("Yellow") < 3 && curColorList.IndexOf("Blue") < 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Yellow and Blue are both on the upper half.", loggingModID);
			int lastOne = idxCipherList.Last();
			idxCipherList = idxCipherList.OrderBy(a => a == lastOne ? 0 : 1).ToArray();
		}
		if (curColorList.IndexOf("Yellow") >= 3 && curColorList.IndexOf("Blue") >= 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Yellow and Blue are both on the lower half.", loggingModID);
			int firstOne = idxCipherList.First();
			idxCipherList = idxCipherList.OrderBy(a => a == firstOne ? 1 : 0).ToArray();
		}
		if (allModIDs.Contains("unfairsRevenge"))
        {
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unfair's Revenge is present.", loggingModID);
            int stepsToCyan = 0;
			while (curColorList[stepsToCyan] != "Cyan" && stepsToCyan < curColorList.Count)
				stepsToCyan++;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: It takes {1} steps to reach Cyan, starting from the NW colored button and going CW.", loggingModID, stepsToCyan);
			for (int x = 0; x < stepsToCyan; x++)
            {
				int firstOne = idxCipherList.First();
				idxCipherList = idxCipherList.OrderBy(a => a == firstOne ? 1 : 0).ToArray();
			}
		}
		if (valueX % 13 == 6)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Value X is 13n + 6.", loggingModID);
			int idxAff = Array.IndexOf(idxCipherList, 6), idxAB = Array.IndexOf(idxCipherList, 7);
			int temp = idxCipherList[idxAff];
			idxCipherList[idxAff] = idxCipherList[idxAB];
			idxCipherList[idxAB] = temp;
			idxCipherList = idxCipherList.Where(a => a != 6).ToArray();
		}

		if (valueA % 26 == 0)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Value A is a multiple of 26.", loggingModID);
			
			int idxR13 = Array.IndexOf(idxCipherList, 5), idxCC = Array.IndexOf(idxCipherList, 4);
			int temp = idxCipherList[idxR13];
			idxCipherList[idxR13] = idxCipherList[idxCC];
			idxCipherList[idxCC] = temp;
			idxCipherList = idxCipherList.Where(a => a != 4).ToArray();
		}


		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The final order of the Cipher List is the following:", loggingModID);
		for (int x = 0; x < idxCipherList.Length; x++)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: {2}: {1}", loggingModID, baseCipherList[idxCipherList[x]], x + 1);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -----------------------Encrypting-----------------------", loggingModID);
		// The Encryption Format
		List<int> firstFourCipherIdx = idxCipherList.Take(5).ToList();
		List<string> encryptedResults = new List<string>();
		string[] directionSamples = { "NW", "N", "NE", "SE", "S", "SW" };
		string baseString = splittedInstructions.Join("");
		for (int x = 0; x < firstFourCipherIdx.Count; x++)
        {
			string currentString = x == 0 ? baseString : encryptedResults.Last();
			switch (firstFourCipherIdx[x])
			{
				case 0:
                    {// Playfair Cipher with Key A
						encryptedResults.Add(EncryptUsingPlayfair(currentString, keyAString, true));
						break;
                    }
				case 1:
					{// Playfair Cipher with Key B
						encryptedResults.Add(EncryptUsingPlayfair(currentString, keyBString, true));
						break;
					}
				case 2:
					{// Playfair Cipher with Key C
						encryptedResults.Add(EncryptUsingPlayfair(currentString, keyCString, true));
						break;
					}
				case 3:
					{// Playfair Cipher with Key D
						encryptedResults.Add(EncryptUsingPlayfair(currentString, keyDString, true));
						break;
					}
				case 4:
					{// Caesar Cipher with Value A
						encryptedResults.Add(EncryptUsingCaesar(currentString, valueA));
						break;
					}
				case 5:
					{// ROT 13 Cipher
						encryptedResults.Add(EncryptUsingCaesar(currentString, 13));
						break;
					}
				case 6:
                    {// Affine Cipher with Value X
						encryptedResults.Add(EncryptUsingAffine(currentString, valueX));
						break;
                    }
				case 7:
					{// Atbash Cipher
						encryptedResults.Add(EncryptUsingAtbash(currentString));
						break;
					}
				case 8:
					{// Basic Columnar Transposition
						encryptedResults.Add(EncryptUsingBasicColumnar(currentString, columnalTranspositionLst));
						break;
					}
				case 9:
					{// Myszkowski Transposition
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+----Mysckowski Transposition Preparations----+-", loggingModID);
						int sumSerNumDigits = bombInfo.GetSerialNumberNumbers().Sum();
						string selectedKey = myszkowskiKeywords[sumSerNumDigits % 28];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Myszkowski Transposition, the sum of the serial number digits is {1}, which lands on the keyword: \"{2}\"", loggingModID,sumSerNumDigits, selectedKey);
						encryptedResults.Add(EncryptUsingMyszkowskiTransposition(currentString, selectedKey));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
                    }
				case 10:
                    {// Anagram Shuffler
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--------Anagram Shuffler Preparations--------+-", loggingModID);
						int selectedRow = (swapPigpenAndStandard ? 1 : 0) + (swapStandardKeys ? 2 : 0);
						int baseColIdx = curColorList.IndexOf("Green"), encryptColIdx = curColorList.IndexOf("Magenta");

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Row Used: {1} ({2}, {3})", loggingModID, selectedRow + 1, swapPigpenAndStandard ? "Pigpen Set at the top" : "Pigpen Set at the bottom", swapStandardKeys ? "Columnar Transposition key is to the right of the Autokey Cipher false keyword" : "Columnar Transposition key is to the left of the Autokey Cipher false keyword");
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Green button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[baseColIdx], anagramValues[selectedRow][baseColIdx]);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Magenta button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[encryptColIdx], anagramValues[selectedRow][encryptColIdx]);

						string[] baseWord = anagramValues[selectedRow][baseColIdx].Split(), encryptWord = anagramValues[selectedRow][encryptColIdx].Split();
						
						if (baseWord.Length == 2 && !bombInfo.GetSerialNumberLetters().Any(a => "AEIOU".Contains(a)))
                        {
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The base key consists of 2 words and there is no vowel in the serial number.", loggingModID);
							baseWord = baseWord.Reverse().ToArray();
                        }
						if (encryptWord.Length == 2 && bombInfo.GetBatteryHolderCount() % 2 == 1)
                        {
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The encryption key consists of 2 words and there is an odd number of battery holders.", loggingModID);
							encryptWord = encryptWord.Reverse().ToArray();
                        }
						string baseWordFinal = baseWord.Join(""), encryptWordFinal = encryptWord.Join("");
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Anagram Shuffler, the base key used is {1} and the encryption key used is {2}", loggingModID, baseWordFinal, encryptWordFinal);
						encryptedResults.Add(EncryptUsingAnagramShuffler(currentString, baseWordFinal, encryptWordFinal));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
                    }
				case 11:
                    {// Scytale Transposition
						int portCount = bombInfo.GetPortCount();
						encryptedResults.Add(EncryptUsingScytaleTransposition(currentString, portCount % 4 + 2));
						break;
                    }
				case 12:
                    {// Autokey Cipher
						string encryptionKey = useEven ? wordSearchWordsEven[randomIdx] : wordSearchWordsOdd[randomIdx];
						encryptedResults.Add(EncryptUsingAutoKeyCipher(currentString, encryptionKey, true));
						break;
                    }
				case 13:
                    {// Four Square Cipher
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--------Four Square Cipher Preparations--------+-", loggingModID);
						
						bool[] trueRules =
						{
							bombInfo.GetBatteryCount() == 3,
							bombInfo.GetPortCount() == 2,
							bombInfo.IsIndicatorPresent(Indicator.BOB),
							false,
							bombInfo.GetIndicators().Count() % 2 == 1,
							bombInfo.IsIndicatorPresent(Indicator.FRK),
							bombInfo.GetSerialNumberNumbers().FirstOrDefault() % 2 == 1,
							bombInfo.GetIndicators().Count() == 2,
							true,
							bombInfo.GetSerialNumberLetters().Count() >= 3,
							bombInfo.GetIndicators().Count() < 2,
							true,
							!bombInfo.GetSerialNumberNumbers().Any(a => new int[] { 0, 2, 4, 6, 8 }.Contains(a)),
							bombInfo.GetModuleIDs().Count() > 30,
							bombInfo.GetBatteryHolderCount() < 3,
						};
						string[] possibleStrings = {
							"NZYIFSUJWBDGVCAHMXTKLQEPOR",
							"AOXBRYGHWFNLDMJQVZSKCTUPEI",
							"ZPDYVKAUQWMCTLXJNHSGOFEIRB",
							"RYCBENFZVQTSLWPXMKAGIHJUDO",
							"ALDNUBSTVRXZOWFCIHEJGPQYKM",
							"OMRSNCGTZYDFQAVPIBXHELKUJW",
							"UHKTLEPQNJMIZOCDRWVSXFBAYG",
							"EBUYZLRCDXWOKQIGTAMSNPHVFJ",
							"YQMGRPFHSUNCEZTABVWKLDJIOX",
							"XBOJNYQUZFVALTKPGCWESRHIMD",
							"TWGCYNBXQKAUDZEJIMROSLHFVP",
							"KTPQBJCEISAYZNOUXGMRDWLHVF",
							"DXUAGEHMCJTOQSLRWPFVZBINKY",
							"CBDJUHOVLFIKSXPZRWQGETYAMN",
							"JIYEPUCAFKGNOQBWZDVLXMRTSH",
						};
						int[] trueIdxs = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }.Where(a => trueRules[a]).ToArray(),
							falseIdxs = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }.Where(a => !trueRules[a]).ToArray();
						int idxFirstTrue = trueIdxs.FirstOrDefault(), idxLastFalse = falseIdxs.LastOrDefault();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Reverse Alphabetize (At 0 solves, 0 strikes) are true: [ {1} ]", loggingModID, trueIdxs.Select(a => a + 1).Join(", "));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Reverse Alphabetize (At 0 solves, 0 strikes) are false: [ {1} ]", loggingModID, falseIdxs.Select(a => a + 1).Join(", "));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The first true rule is in row {1}", loggingModID, idxFirstTrue + 1);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The last false rule is in row {1}", loggingModID, idxLastFalse + 1);
						List<int> modifiedTrueInts = trueIdxs.ToList();
						while (modifiedTrueInts.Count > 2)
                        {
							modifiedTrueInts.Remove(modifiedTrueInts.Max());
							modifiedTrueInts.Remove(modifiedTrueInts.Min());
						}
						string encryptionStringA = "";
						if (modifiedTrueInts.Count == 1)
						{
							int medianVal = modifiedTrueInts.Single();
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching a single number, the median row used is row {1}", loggingModID, medianVal + 1);
							encryptionStringA = possibleStrings[medianVal];
						}
						else if (modifiedTrueInts.Sum() % 2 == 0)
						{
							int medianVal = modifiedTrueInts.Sum() / 2;
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching to a pair of numbers, the median row used is row {1}", loggingModID, medianVal + 1);
							encryptionStringA = possibleStrings[medianVal];
						}
						else
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching to a pair of numbers, the median row does not exist.", loggingModID);

						encryptedResults.Add(EncryptUsingFourSquare(currentString, encryptionStringA, possibleStrings[idxFirstTrue], possibleStrings[idxLastFalse], extraKey, true));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-----------------------------------------------+-", loggingModID);
						break;
                    }
				case 14:
                    {// Redefence Transposition
						encryptedResults.Add(EncryptUsingRedefenceTranspositon(currentString, columnalTranspositionLst));
						break;
                    }
			}
        }
		for (int y = encryptedResults.Count - 1; y >= 0; y--)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After {2}: {1}", loggingModID, encryptedResults[y], baseCipherList[firstFourCipherIdx[y]]);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Generated instructions: {1}", loggingModID, splittedInstructions.Join(", "));
		displayPigpenText = FitToScreen(encryptedResults.Any() ? encryptedResults.Last() : splittedInstructions.Join(""), 13);
		StartCoroutine(TypePigpenText(encryptedResults.Any() ? encryptedResults.Last() : splittedInstructions.Join("")));
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: --------------------------------------------------------", loggingModID);
		// Section for testing purposes. To ensure ciphers and transpositions work as intended
		/*
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ---------------------Test Encryptions---------------------", loggingModID);
		string playfairEncryptedString = EncryptUsingPlayfair(baseString, keyCString, true),
          affineEncryptedString = EncryptUsingAffine(baseString, valueX),
          caesarEncryptedString = EncryptUsingCaesar(baseString, valueA),
          columnarTransposedString = EncryptUsingBasicColumnar(baseString, columnalTranspositionLst),
          scytaleTransposedString = EncryptUsingScytaleTransposition(baseString, 6),
          myszowkTransposedString = EncryptUsingMyszkowskiTransposition(baseString, "BANANA"),
          fourSquareString = EncryptUsingFourSquare(baseString, "ALPHA", "BRAVO", "YANKEE", "ZULU", true),
          anagramShuffledString = EncryptUsingAnagramShuffler(baseString, "EAT", "ATE"),
          autoKeyEncryptedString = EncryptUsingAutoKeyCipher(baseString, "OMEGA", true),
		  atbashEncryptedString = EncryptUsingAtbash(baseString),
		  redefenceEncryptedString = EncryptUsingRedefenceTranspositon(baseString, columnalTranspositionLst);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Myszkowski Transposed String: {1}", loggingModID, myszowkTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Scytale Transposed String: {1}", loggingModID, scytaleTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Columnar Transposed String: {1}", loggingModID, columnarTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Caesar Encrypted String: {1}", loggingModID, caesarEncryptedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Affine Encrypted String: {1}", loggingModID, affineEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Atbash Encrypted String: {1}", loggingModID, atbashEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Playfair Encrypted String: {1}", loggingModID, playfairEncryptedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Anagram Shuffled String: {1}", loggingModID, anagramShuffledString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Four Square Encrypted String: {1}", loggingModID, fourSquareString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Autokey Encrypted String: {1}", loggingModID, autoKeyEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Redefence Encrypted String: {1}", loggingModID, redefenceEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------------", loggingModID);
		*/
	}

	string EncryptUsingCaesar(string input, int valueA = 0)
	{// Encrypt the string with the given valueA. Example: "ABCDEFG" + 7 -> "HIJKLMN"
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] += valueA;
			while (stringInputs[x] < 0)
				stringInputs[x] += 26;
			stringInputs[x] %= 26;
		}

		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingAffine(string input, int valueX = 0)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a) + 1).ToArray();
		int multiplier = valueX * 2 + 1;
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] *= multiplier;
			while (stringInputs[x] > 26)
				stringInputs[x] -= 26;
			stringInputs[x]--;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingAtbash(string input)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] = baseAlphabet.Length - stringInputs[x];
			while (stringInputs[x] < 1)
				stringInputs[x] += 26;
			stringInputs[x]--;
			stringInputs[x] %= 26;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingPlayfair(string input, string keyword = "", bool logSquares = false)
	{

		/* Example:
		 * 
		 * Keyword: UNFAIRCIPHER
		 * Keyword after removing duplicate letters: UNFAIRCPHE
		 * 
		 * Grid when using keyword:
		 * U N F A I
		 * R C P H E
		 * B D G K L
		 * M O Q S T
		 * V W X Y Z
		 * 
		 * On a rectangular/square grid, this can be used to grab:
		 * - The row index of the given letter
		 * - The col index of the given letter
		 * 
		 * To Encrypt:		BENT ON HER HEELS
		 * Key Pairs:		BE NT ON HE RH EE LS
		 * Expected Result:	LR IO WC ER CE EE KT
		 * 
		 */

		string modifiedKeyword = keyword.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Join(""), playfairGridBase = modifiedKeyword + baseAlphabet.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Where(a => !modifiedKeyword.Distinct().Contains(a)).Join("");
		if (logSquares)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Given Playfair set: {1}", loggingModID, playfairGridBase);
		if (input.Length % 2 != 0) input += baseAlphabet[23];
		string output = "";
		for (int y = 0; y < input.Length; y += 2)
		{
			string currentSet = input.Substring(y, 2).Replace(baseAlphabet[9], baseAlphabet[8]);
			if (currentSet.Distinct().Count() == 1)
			{
				output += currentSet;
				continue;
			}
			int[] rowIdxs = currentSet.Select(l => playfairGridBase.IndexOf(l) / 5).ToArray();
			int[] colIdxs = currentSet.Select(l => playfairGridBase.IndexOf(l) % 5).ToArray();

			if (rowIdxs.Distinct().Count() == 1)
			{
				colIdxs = colIdxs.Select(a => (a + 1) % 5).ToArray();
			}
			else if (colIdxs.Distinct().Count() == 1)
			{
				rowIdxs = rowIdxs.Select(a => (a + 1) % 5).ToArray();
			}
			else
			{
				colIdxs = colIdxs.Reverse().ToArray();
			}
			for (int x = 0; x < 2; x++)
			{
				//Debug.Log(string.Format("{0},{1}", rowIdxs[x], colIdxs[x]));

				output += playfairGridBase[5 * rowIdxs[x] + colIdxs[x]];
			}
		}
		return output;
	}
	string EncryptUsingBasicColumnar(string input, int[] key)
	{
		/* Example:
		 * 
		 * To Encrypt:		GREATJOBMATE
		 * Key:				1342
		 * Process:			GREA TJOB MATE
		 * 
		 * Expected Result:	GTM ABE RJA EOT
		 * 
		 */
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Given Columnar Set: {1}", loggingModID, key.Select(a => a + 1).Join(""));
		List<string> splittedInput = new List<string>();
		for (int x = 0; x < key.Length; x++)
		{
			string curVal = "";
			int curIdx = x;
			while (curIdx < input.Length)
			{
				curVal += input[curIdx];
				curIdx += key.Length;
			}
			splittedInput.Add(curVal);
		}
		return splittedInput.OrderBy(a => key[splittedInput.IndexOf(a)]).Join("");
	}
	string EncryptUsingScytaleTransposition(string input, int rowCount = 2)
	{
		if (rowCount < 1)
			throw new FormatException(string.Format("{0} is not a valid length for encrypting with Scytale Transposition!",rowCount));
		string output = "";
		for (int x = 0; x < rowCount; x++)
		{
			int curPos = x;
			while (curPos < input.Length)
			{
				output += input[curPos];
				curPos += rowCount;
			}
		}
		return output;
	}
	string EncryptUsingMyszkowskiTransposition(string input, string keyword)
	{
		/* Example:
		 * 
		 * To Encrypt:		GREATJOBMATE
		 * Keyword:			BOOT
		 * Alphabetical Order: A-Z standard
		 * Process:			GREA TJOB MATE
		 * 
		 * Expected Result:	GTM RE JO AT ABE
		 * 
		 */
		if (!keyword.Any(a => char.IsLetter(a)))
			throw new FormatException(string.Format("\"{0}\" is not a valid word for encrypting with Myszkowski Transposition!", keyword));
		char[] allChars = keyword.Where(a => char.IsLetter(a)).ToArray();
		List<string> separatedSets = new List<string>();
		for (int x = 0; x < keyword.Length; x++)
		{
			int curPos = x;
			string curVal = "";
			while (curPos < input.Length)
			{
				curVal += input[curPos];
				curPos += keyword.Length;
			}
			separatedSets.Add(curVal);
		}
		string output = "";
		List<char> sortedLetters = allChars.OrderBy(a => baseAlphabet.IndexOf(a)).Distinct().ToList();
		IEnumerable<int> intLists = sortedLetters.Select(a => sortedLetters.IndexOf(a));
		int[] keywordLists = allChars.Select(a => sortedLetters.IndexOf(a)).ToArray();
		for (int x = 0; x < intLists.Count(); x++)
		{
			List<string> currentSet = new List<string>();
			for (int y = 0; y < keywordLists.Length; y++)
			{
				if (keywordLists[y] == intLists.ElementAt(x))
					currentSet.Add(separatedSets[y]);
			}

			for (int pos = 0; pos < currentSet.Select(a => a.Length).Max(); pos++)
            {
				for (int y = 0; y < currentSet.Count(); y++)
                {
					if (pos < currentSet[y].Length)
						output += currentSet[y][pos];
                }
            }
		}
		return output;
	}
	string EncryptUsingFourSquare(string input, string keywordA = "", string keywordB = "", string keywordC = "", string keywordD = "", bool logSquares = false)
	{
		string modifiedKeywordA = keywordA.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Join(""),
			modifiedKeywordB = keywordB.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Join(""),
			modifiedKeywordC = keywordC.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Join(""),
			modifiedKeywordD = keywordD.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Join("");
		string gridA = modifiedKeywordA + baseAlphabet.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Where(a => !modifiedKeywordA.Distinct().Contains(a)).Join("");
		string gridB = modifiedKeywordB + baseAlphabet.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Where(a => !modifiedKeywordB.Distinct().Contains(a)).Join("");
		string gridC = modifiedKeywordC + baseAlphabet.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Where(a => !modifiedKeywordC.Distinct().Contains(a)).Join("");
		string gridD = modifiedKeywordD + baseAlphabet.Replace(baseAlphabet[9], baseAlphabet[8]).Distinct().Where(a => !modifiedKeywordD.Distinct().Contains(a)).Join("");
		if (logSquares)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid A: {1}", loggingModID, gridA);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid B: {1}", loggingModID, gridB);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid C: {1}", loggingModID, gridC);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid D: {1}", loggingModID, gridD);
		}
		if (input.Length % 2 != 0) input += baseAlphabet[23];
		string output = "";
		for (int y = 0; y < input.Length; y += 2)
		{
			string currentSet = input.Substring(y, 2).Replace(baseAlphabet[9], baseAlphabet[8]);
            int[] rowIdxs = new int[] { gridA.IndexOf(currentSet[0]) / 5, gridD.IndexOf(currentSet[1]) / 5 };
            int[] colIdxs = new int[] { gridD.IndexOf(currentSet[1]) % 5, gridA.IndexOf(currentSet[0]) % 5 };
			output += gridB[rowIdxs[0] * 5 + colIdxs[0]].ToString() + gridC[rowIdxs[1] * 5 + colIdxs[1]].ToString();
		}
		return output;
    }
	string EncryptUsingAnagramShuffler(string input, string keywordA, string keywordB)
    {
		if (!keywordA.Distinct().OrderBy(a => a).SequenceEqual(keywordB.Distinct().OrderBy(a => a)))
			throw new FormatException(string.Format("\"{0}\" and \"{1}\" are not anagrams for encrypting with Anagram Shuffler!", keywordA, keywordB));
		List<string> separatedSets = new List<string>();
		for (int x = 0; x < keywordA.Length; x++)
		{
			int curPos = x;
			string curVal = "";
			while (curPos < input.Length)
			{
				curVal += input[curPos];
				curPos += keywordA.Length;
			}
			separatedSets.Add(curVal);
		}
		int[] sortedList = keywordB.Select(x => keywordA.IndexOf(x)).ToArray();
		string output = "";
		for (int x=0;x<sortedList.Select(a => separatedSets[a].Length).Max();x++)
        {
			for (int y=0;y<sortedList.Length;y++)
            {
				if (separatedSets[sortedList[y]].Length > x)
					output += separatedSets[sortedList[y]][x];

			}
        }
		return output;
	}
	string EncryptUsingAutoKeyCipher(string input, string keyword = "", bool logModifiedKeyword = false)
    {
		string appendedKeyword = keyword + (keyword.Length >= input.Length ? "" : input.Substring(0, input.Length - keyword.Length));
		if (logModifiedKeyword)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: AutoKey Cipher Keyword + PlainText: {1}", loggingModID, appendedKeyword);

		string output = "";
        for (int x = 0; x < input.Length; x++)
        {
			int idxInput = baseAlphabet.IndexOf(input[x]), idxKey = baseAlphabet.IndexOf(appendedKeyword[x]);
			output += baseAlphabet[(idxInput + idxKey) % 26];
        }
		return output;
    }
	string EncryptUsingRedefenceTranspositon (string input, int[] key)
    {
		string[] separtedSets = new string[key.Length];
		int curPos = 1;
		bool dirBack = true;
        for (int x = 0; x < input.Length; x++)
        {
			if (dirBack)
            {
				curPos--;
				if (curPos <= 0)
					dirBack = false;
            }
			else
            {
				curPos++;
				if (curPos >= key.Length - 1)
					dirBack = true;
            }
			separtedSets[curPos] += input[x];
		}
		return key.OrderBy(a => a).Select(a => separtedSets[Array.IndexOf(key, a)]).Join("");
    }

	Dictionary<char, int> charReference = new Dictionary<char, int>() {
		{'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 }, {'5', 5 },
		{'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 }, {'A', 1 }, {'B', 2 },
		{'C', 3 }, {'D', 4 }, {'E', 5 }, {'F', 6 }, {'G', 7 }, {'H', 8 },
		{'I', 9 }, {'J', 10 }, {'K', 11 }, {'L', 12 }, {'M', 13 }, {'N', 14 },
		{'O', 15 }, {'P', 16 }, {'Q', 17 }, {'R', 18 }, {'S', 19 }, {'T', 20 },
		{'U', 21 }, {'V', 22 }, {'W', 23 }, {'X', 24 }, {'Y', 25 }, {'Z', 26 },
	}, base36Reference = new Dictionary<char, int>() {
		{'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 }, {'5', 5 },
		{'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 }, {'A', 10 }, {'B', 11 },
		{'C', 12 }, {'D', 13 }, {'E', 14 }, {'F', 15 }, {'G', 16 }, {'H', 17 },
		{'I', 18 }, {'J', 19 }, {'K', 20 }, {'L', 21 }, {'M', 22 }, {'N', 23 },
		{'O', 24 }, {'P', 25 }, {'Q', 26 }, {'R', 27 }, {'S', 28 }, {'T', 29 },
		{'U', 30 }, {'V', 31 }, {'W', 32 }, {'X', 33 }, {'Y', 34 }, {'Z', 35 },
	};
	string ObtainKeyA()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Key A Calculations------------", loggingModID);
		string returningString = "";
		string hexDecimalString = "0123456789ABCDEF";
		string curSerNo = bombInfo.GetSerialNumber();
		long givenValue = 0;
		for (int x = 0; x < curSerNo.Length; x++)
		{
			givenValue *= 36;
			givenValue += base36Reference.ContainsKey(curSerNo[x]) ? base36Reference[curSerNo[x]] : 18;
		}
		Debug.LogFormat("[Unfair's Revenge #{0}]: After Base-36 Conversion: {1}", loggingModID, givenValue);
		while (givenValue > 0)
		{
			returningString += hexDecimalString[(int)(givenValue % 16)];
			givenValue /= 16;
		}
		returningString = returningString.Reverse().Join("");
		Debug.LogFormat("[Unfair's Revenge #{0}]: After Converting into Hexadecimal: {1}", loggingModID, returningString);
		string output = "";
		string[] listAllPossibilities = new string[] { returningString, selectedModID.ToString(), (bombInfo.GetPortPlateCount() + 1).ToString(), (2 + bombInfo.GetBatteryHolderCount()).ToString() };
		foreach (string selectedString in listAllPossibilities)
			for (int x = 0; x < selectedString.Length; x++)
			{
				if (x + 1 < selectedString.Length)
				{
					string intereptedString = selectedString.Substring(x, 2);
					if (intereptedString.RegexMatch(@"^(1\d|2[0123456])$"))
					{
						int intereptedValue = int.Parse(intereptedString);
						output += baseAlphabet[intereptedValue - 1];
						x++;
						continue;
					}
				}
				if (hexDecimalString.Substring(10).Contains(selectedString[x]))
				{
					output += selectedString[x];
				}
				else
				{
					int intereptedValue = int.Parse(selectedString[x].ToString());
					if (intereptedValue > 0)
						output += baseAlphabet[intereptedValue - 1];
				}
			}
		Debug.LogFormat("[Unfair's Revenge #{0}]: After Intereperation + ModID, Port Plate, Battery Holder appending: {1}", loggingModID, output);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------------------------------------", loggingModID);
		return output;
	}

	string ObtainKeyD()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Key D Calculations------------", loggingModID);
		string[] allPossibleStrings = {
			"WLUAZVHEJDNQSYFPGMBOIRCXTK",
			"MVFBXJQNHWZTAKPEOCURISDYLG",
			"DEYQLFZOURNPHVKIMSJACGXTWB",
			"LBMCHNAKVFJOSGDQEPUXRWIZTY",
			"BVUHFMALPINYSGJTRQKEOXWDCZ",
			"OXFGNSKUVPHQJIZWCTRYALEMDB",
			"ESFKUYZOPAWVRJBMIXGDQNTHCL",
			"KHLCISQNPOMBGJZWRAYVXTFDEU",
			"GWYUSDNZQFVALREPKMTOHXBCIJ",
			"JEXUSLCQYNOHKZADFWPRBITMVG",
			"IGYXKZMEULPNAQOVBJTWDSHFCR",
			"RZLAJBFVXTKYNODGWEHCQMPUIS",
			"HTGMSNPXCVYRWQZJFUODEIAKBL",
			"ZFXBJKRNOQCPUTVEHSMIADLYWG",
			"XSCOVHZQPYFABIETGKDJURMLWN"
				};
		bool[] trueConditions = new bool[] {
			bombInfo.GetIndicators().Count() == 2,
			bombInfo.GetPortCount() == 3,
			bombInfo.IsIndicatorPresent(Indicator.CAR),
			true,
			bombInfo.GetBatteryCount() % 2 == 0,
			bombInfo.IsIndicatorPresent(Indicator.MSA),
			"BCDFGHJKLMNPQRSTVWXYZ".Contains(bombInfo.GetSerialNumberLetters().ElementAtOrDefault(0)),
			bombInfo.GetPortPlateCount() == 2,
			bombInfo.GetBatteryCount() % 2 == 1,
			bombInfo.GetSerialNumberNumbers().Count() >= 3,
			bombInfo.GetIndicators().Count() > 2,
			false,
			!bombInfo.GetSerialNumber().Where(a => "AEIOU".Contains(a)).Any(),
			bombInfo.GetModuleIDs().Count() > 30,
			bombInfo.GetIndicators().Count() < 4,
		};
		int[] loggingSet = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }.Where(a => trueConditions[a]).ToArray();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Alphabetize (At 0 solves, 0 strikes) are true: [ {1} ]", loggingModID, loggingSet.Select(a => a + 1).Join(", "));
		int sum = 0;
		for (int x = 0;x<trueConditions.Length;x++)
		{
			sum += !trueConditions[x] ? (x + 1) : 0;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The sum of all of the false rules is {1}", loggingModID, sum);
		string output = sum % trueConditions.Where(a => !a).Count() == 0 ? allPossibleStrings[sum / trueConditions.Count(a => !a) - 1] : allPossibleStrings[14];
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Row used for Key D: {1}", loggingModID, sum % trueConditions.Count(a => !a) == 0 ? (sum / trueConditions.Count(a => !a)) : 15);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------------------------------------", loggingModID);
		return output;
	}

	void ModifyBaseAlphabet()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ---------------Alphabet Modifications---------------", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Base Alphabet: {1}", loggingModID, baseAlphabet);
		string valueToChange = baseAlphabet.ToString();
		int valueModifier = bombInfo.GetSerialNumberNumbers().Any() ? 1 + bombInfo.GetSerialNumberNumbers().Last() : 11;
		valueToChange = valueToChange.Substring(26 - valueModifier) + valueToChange.Substring(0, 26 - valueModifier);

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Digit Shift ({2}): {1}", loggingModID, valueToChange, valueModifier);
		if (bombInfo.GetSerialNumberLetters().Any())
		{
			char curLetter = bombInfo.GetSerialNumberLetters().Last();
			int idxCurLetter = valueToChange.IndexOf(curLetter);

			if (idxCurLetter == 0)
				valueToChange = valueToChange.Substring(1) + curLetter;
			else
			{
				valueToChange = curLetter + valueToChange.Substring(0, idxCurLetter) + (idxCurLetter + 1 >= 26 ? "" : valueToChange.Substring(idxCurLetter + 1));
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Letter Shifting: {1}", loggingModID, valueToChange);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Conditions Taken:", loggingModID, valueToChange);
		bool containsLitBOB = bombInfo.IsIndicatorOn(Indicator.BOB);
		if (containsLitBOB && bombInfo.GetBatteryCount() == 0 && bombInfo.GetPortPlateCount() == 0 && !bombInfo.GetOffIndicators().Any() && bombInfo.GetSerialNumberLetters().Where(a => "AEIOU".Contains(a)).Any())
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is exactly all of these: Lit BOB, no batteries, no port plates, no unlit indicators, at least a vowel in the serial number", loggingModID);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Threw away the modified alphabet to use the base alphabet instead.", loggingModID);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);
			return;
		}
		if (containsLitBOB)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a Lit BOB", loggingModID);
			valueToChange = valueToChange.Reverse().Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetBatteryHolderCount() % 2 == 1)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an odd number of battery holders.", loggingModID);
			List<char> vowelList = new List<char>() { 'A', 'E', 'I', 'O', 'U' };
			if (bombInfo.GetSerialNumberLetters().Contains('W'))
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: 'W' is present in the serial number.", loggingModID);
				vowelList.Add('W');
			}
			valueToChange = valueToChange.OrderBy(a => vowelList.Contains(a) ? 1 : 0).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.IsPortPresent(Port.DVI))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a DVI port.", loggingModID);
			valueToChange = valueToChange.Substring(0,13).Reverse().Join("") + valueToChange.Substring(13);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (!bombInfo.IsPortPresent(Port.StereoRCA))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is not a Stereo RCA port.", loggingModID);
			List<char> modifiedList = new List<char>() { 'R','C','A' };
			valueToChange = valueToChange.OrderBy(a => modifiedList.IndexOf(a)).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetBatteryCount() % 2 == 0)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an even number of batteries.", loggingModID);
			List<int> usablePositions = new List<int> { 4, 6, 8, 9, 10, 14, 15, 21, 22, 25, 26 };
			string stationaryString = "", modifyingString = "";
			for (int x = 1; x <= 26; x++)
			{
				if (usablePositions.Contains(x))
					modifyingString += valueToChange[x - 1];
				else
					stationaryString += valueToChange[x - 1];
			}

			valueToChange = stationaryString + modifyingString.Reverse().Join(""); 

			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		List<string> detectableModIDs = new List<string>() { "sphere", "yellowArrowsModule", "greenArrowsModule", };
		if (bombInfo.GetModuleIDs().Any(a => detectableModIDs.Contains(a)))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At least 1 of the following modules are present: Green Arrows, Yellow Arrows, The Sphere", loggingModID);
			valueToChange = "LAZYDOG" + valueToChange;
			valueToChange = valueToChange.Distinct().Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetModuleIDs().Count(a => a.Equals(modSelf.ModuleType)) > 1)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are duplicate Unfair's Cruel Revenge", loggingModID);
			valueToChange = valueToChange.Substring(0, 13) + valueToChange.Substring(13).Reverse().Join("");
			valueToChange = valueToChange.OrderBy(a => "THEQUICK".IndexOf(a)).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		baseAlphabet = valueToChange;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Final Modified Alphabet String: {1}", loggingModID, baseAlphabet);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);
	}

	private Dictionary<int, string> romanValues = new Dictionary<int, string>() {
		{1,"I" },{5,"V" },{10,"X" },{50,"L" },{100,"C" },{500,"D" },{1000,"M" },{5000,"V_" },{10000,"X_" }
	};
	string FitToScreen(string value, int maxLength)
	{
		string output = "";
		if (maxLength <= 0)
			throw new ArgumentException(string.Format("{0} is not a valid length for the method FitToScreen!", maxLength));
		int a = 0;
		for (int x = 0; x < value.Length / maxLength; x++)
		{
			output += value.Substring(a, maxLength) + "\n";
			a += maxLength;
		}
		if (a < value.Length)
			output += value.Substring(a);
		return output.Trim();
	}
	public string ValueToBrokenRoman(int value)
	{
		string output = "";
		int[] possibleKeys = romanValues.Keys.ToArray();
		while (value > 0)
		{
			int applier = 0;
			for (int x = 0; x < possibleKeys.Length; x++)
			{
				if (possibleKeys[x] > value)
					break;
				applier = possibleKeys[x];
			}
			output += romanValues[applier];
			value -= applier;

		}
		return output;
	}
	public string ValueToFixedRoman(int value)
	{
		string output = "";
		int[] possibleKeys = romanValues.Keys.ToArray();
		string curValue = value.ToString();
		for (int x = 0; x < curValue.Length; x++)
		{
			int givenValue = int.Parse(curValue[curValue.Length - 1 - x].ToString());
			switch (givenValue)
			{
				case 0:
					break;
				case 1:
				case 2:
				case 3:
					for (int y = 0; y < givenValue; y++)
						output = romanValues[possibleKeys[2 * x]] + output;
					break;
				case 4:
					output = romanValues[possibleKeys[2 * x]] + romanValues[possibleKeys[2 * x + 1]] + output;
					break;
				case 5:
				case 6:
				case 7:
				case 8:
					string toAdd = "";
					toAdd += romanValues[possibleKeys[2 * x + 1]];
					for (int y = 0; y < givenValue - 5; y++)
						toAdd += romanValues[possibleKeys[2 * x]];
					output = toAdd + output;
					break;
				case 9:
					output = romanValues[possibleKeys[2 * x]] + romanValues[possibleKeys[2 * (x + 1)]] + output;
					break;
			}
		}
		return output;
	}
	public void GenerateInstructions()
	{
		string[] lastCommand = { "FIN", "ISH" };
		List<string> instructionsToShuffle = hardModeInstructions.ToList();
		instructionsToShuffle.Shuffle();
		splittedInstructions.AddRange(instructionsToShuffle.Take(5));
		splittedInstructions.Add(lastCommand.PickRandom());
		
	}
	IEnumerator HandleFlashingAnim(int btnIdx)
	{
		if (btnIdx < 0 || btnIdx >= 6) yield break;
		//colorButtonRenderers[btnIdx].material = switchableMats[1];
		int animLength = 10;
		for (int x = animLength; x >= 0; x--)
		{
			colorLights[btnIdx].intensity = 1.5f + (x / 10f);
			colorButtonRenderers[btnIdx].material.color = (colorWheel[idxColorList[btnIdx]] * .75f * ((animLength - x) / (float)animLength)) + Color.white * (x / (float)animLength);
			yield return new WaitForSeconds(0.05f);
		}
		//colorButtonRenderers[btnIdx].material = switchableMats[0];
		colorButtonRenderers[btnIdx].material.color = colorWheel[idxColorList[btnIdx]] * .75f;
		colorLights[btnIdx].intensity = 1.5f;
		yield return null;
	}
	IEnumerator HandlePressAnim(GameObject givenItem)
	{
		for (int x = 0; x < 2; x++)
		{
			givenItem.transform.localPosition += Vector3.down / 1000;
			yield return new WaitForSeconds(1 / 60f);
		}
		for (int x = 0; x < 2; x++)
		{
			givenItem.transform.localPosition += Vector3.up / 1000;
			yield return new WaitForSeconds(1 / 60f);
		}
		yield return null;
	}
	IEnumerator HandleFlickerSolveAnim()
	{
		while (isplayingSolveAnim)
		{
			for (int a = 0; a < 6; a += 1)
			{
				statusIndicators[a].material.color = Color.white;
			}
			yield return new WaitForSeconds(0.25f);
			for (int a = 0; a < 6; a += 1)
			{
				statusIndicators[a].material.color = Color.black;
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		StartCoroutine(indicatorCoreHandlerEX.HandleCollaspeAnim());
		yield return null;
	}
	IEnumerator HandleSpecialSolveAnim()
    {
		isplayingSolveAnim = true;
		StartCoroutine(HandleFlickerSolveAnim());
		if (autoCycleEnabled)
			StartCoroutine(HandleAutoCycleAnim(false));
		float[] waitTimes = { 0.75f, 0.35f, 0.27f, 0.15f, 0.48f, 0.66f, 0.66f, 0.67f, 0.25f,
			0.75f, 0.35f, 0.27f, 0.15f, 0.3f, 0.4f, 0.4f, 0.4f, 0.4f,
			0.75f, 0.35f, 0.27f, 0.15f, 0.48f, 0.66f, 0.66f, 0.67f, 0.25f,
			0.75f, 0.35f, 0.27f, 0.15f, 0.3f, 0.4f, 0.4f, 0.4f, 0.3f
		};
		for (int y = 0; y < waitTimes.Length; y++)
		{
			pigpenDisplay.text = pigpenDisplay.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
			mainDisplay.text = mainDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : a : a).Join("");
			pigpenSecondary.text = pigpenSecondary.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
			strikeIDDisplay.text = strikeIDDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : char.IsDigit(a) ? "0123456789".PickRandom() : a : a).Join("");
			yield return new WaitForSeconds(waitTimes[y]);
			for (int x = 0; x < colorLights.Length; x++)
            {
				colorLights[x].enabled = x % 2 == y % 2;
			}
		}
        for (float y = 1; y >= 0; y -= 0.05f)
        {
			pigpenDisplay.text = pigpenDisplay.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
			mainDisplay.text = mainDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : a : a).Join("");
			pigpenSecondary.text = pigpenSecondary.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
			strikeIDDisplay.text = strikeIDDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : char.IsDigit(a) ? "0123456789".PickRandom() : a : a).Join("");

			pigpenDisplay.color = new Color(1, 1, 1, y);
			mainDisplay.color = new Color(1, 1, 1, y);
			pigpenSecondary.color = new Color(1, 1, 1, y);
			strikeIDDisplay.color = new Color(strikeIDDisplay.color.r, strikeIDDisplay.color.g, strikeIDDisplay.color.b, y);
			for (int x = 0; x < colorLights.Length; x++)
			{
				colorLights[x].enabled = x % 6 == Mathf.CeilToInt(y * 20) % 6;
			}
			yield return new WaitForSeconds(0.1f);
        }
		mAudio.PlaySoundAtTransform("submitstop", transform);
		pigpenDisplay.text = "";
		mainDisplay.text = "";
		pigpenSecondary.text = "";
		strikeIDDisplay.text = "";
		isplayingSolveAnim = false;
		foreach (Light singleLight in colorLights)
			singleLight.enabled = false;
		centerLight.enabled = false;
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.5f;
		}
		yield break;
    }
	IEnumerator HandleSolveAnim()
	{
		mAudio.PlaySoundAtTransform("submitstart", transform);
		isplayingSolveAnim = true;
		StartCoroutine(HandleFlickerSolveAnim());
		if (autoCycleEnabled)
			StartCoroutine(HandleAutoCycleAnim(false));
		for (int y = 9; y > 0; y -= 2)
		{
			for (int x = 0; x < y; x++)
			{
				pigpenDisplay.text = pigpenDisplay.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
				mainDisplay.text = mainDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : a : a).Join("");
				pigpenSecondary.text = pigpenSecondary.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
				strikeIDDisplay.text = strikeIDDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : char.IsDigit(a) ? "0123456789".PickRandom() : a : a).Join("");
				mAudio.PlaySoundAtTransform("submiterate", transform);
				yield return new WaitForSeconds(0.2f);
			}
			mAudio.PlaySoundAtTransform("submiterate2", transform);
			yield return new WaitForSeconds(0.1f);
		}
		isplayingSolveAnim = false;
		mAudio.PlaySoundAtTransform("submitstop", transform);
		pigpenDisplay.text = "";
		mainDisplay.text = "";
		pigpenSecondary.text = "";
		strikeIDDisplay.text = "";
		foreach (Light singleLight in colorLights)
			singleLight.enabled = false;
		centerLight.enabled = false;
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.5f;
		}
		yield return null;
	}
	IEnumerator TypePigpenText(string displayValue)
	{
		isAnimatingStart = true;
		string[] typeSoundList = { "type_1", "type_2", "type_3" };
		for (int x = 0; x < displayValue.Length; x++)
		{
			if (x > 0 && x % 13 == 0)
			{
				pigpenDisplay.text += "\n";
				mAudio.PlaySoundAtTransform("line", transform);
			}
			pigpenDisplay.text += displayValue[x];
			yield return new WaitForSeconds(0.2f);
			mAudio.PlaySoundAtTransform(typeSoundList.PickRandom(), transform);
		}
		mAudio.PlaySoundAtTransform("line", transform);
		yield return new WaitForSeconds(0.3f);
		mAudio.PlaySoundAtTransform("line_2", transform);
		StartCoroutine(indicatorCoreHandlerEX.HandleIndicatorModification(splittedInstructions.Count));
		yield return null;
		isAnimatingStart = false;
	}
	IEnumerator SampleStandardText()
	{
		Dictionary<string, string> sampleQuestionResponse = new Dictionary<string, string>()
		{
			{"It was too\nconsistant.", "So he did\nthis instead." },
			{"It was never fair", "in the first place." },
			{"Do defusers even\nread these?", "I guess not as much." },
			{"Landing Sequence...", "ERROR" },
			{"Contains TheFatRat", " - The Calling" },
			{"Funny Text", "Side Text" },
			{"You have time...", "Right?" },
			{"Where did he go?", "Is it there?" },
			{"Who saw this coming?", "Certainly not him." },
		};
		KeyValuePair<string, string> selectedSample = sampleQuestionResponse.PickRandom();
		mainDisplay.color = Color.red;
		strikeIDDisplay.color = Color.red;
		for (int x = 1; x <= Math.Max(selectedSample.Key.Length,selectedSample.Value.Length); x++)
		{
			mainDisplay.text = selectedSample.Key.Substring(0, Math.Min(x,selectedSample.Key.Length));
			strikeIDDisplay.text = selectedSample.Value.Substring(0, Math.Min(x, selectedSample.Value.Length));
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return new WaitForSeconds(3f);
		mainDisplay.text = "";
		strikeIDDisplay.text = "";
	}
	IEnumerator HandleStartUpAnim()
	{
		entireCircle.SetActive(true);
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.75f;
		}
		entireCircle.transform.localScale = Vector3.zero;
		entireCircle.transform.localPosition = 5 * Vector3.up;
		yield return new WaitForSeconds(uernd.Range(0f, 2f));
		int animLength = 60;
		for (float x = 0; x <= animLength; x++)
		{
			float curScale = Mathf.Pow(x / animLength, 1);
			entireCircle.transform.localScale = new Vector3(curScale, curScale, curScale);
			if (x != animLength)
				entireCircle.transform.Rotate(Vector3.up * (360 / Mathf.Max(animLength, 0.5f)));
			float currentOffset = Mathf.Pow((x - animLength) / animLength, 2f);
			entireCircle.transform.localPosition = new Vector3(0, 5 * currentOffset, 0);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		mAudio.PlaySoundAtTransform("werraMetallicTrimmed", entireCircle.transform);
		outerSelectable.AddInteractionPunch(3f);
		for (int i = 0; i < colorLights.Length; i++)
		{
			colorLights[i].enabled = true;
			colorLights[i].color = colorWheel[idxColorList[i]];
		}
		centerLight.enabled = true;
		particles.Emit(90);
	}
	IEnumerator HandleStrikeAnim()
	{
		for (int x = 0; x < 5; x++)
		{
			for (int a = 0; a < 6; a += 1)
			{
				statusIndicators[a].material.color = Color.red;
			}
			mAudio.PlaySoundAtTransform("wrong", transform);
			yield return new WaitForSeconds(0.1f);
			for (int a = 0; a < 6; a += 1)
			{
				statusIndicators[a].material.color = Color.black;
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		UpdateStatusIndc();
		yield return null;
	}
	void LogCurrentInstruction()
	{
		if (isFinished) return;
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		string toLog = "This is an example of logging a current instruction.";
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		switch (splittedInstructions[currentInputPos])
		{
			case "PCR":
				toLog = "Press Red.";
				break;
			case "PCG":
				toLog = "Press Green.";
				break;
			case "PCB":
				toLog = "Press Blue.";
				break;
			case "SCC":
				toLog = "Press Cyan.";
				break;
			case "SCM":
				toLog = "Press Magenta.";
				break;
			case "SCY":
				toLog = "Press Yellow.";
				break;
			case "SUB":
				toLog = "Press Inner Center when the seconds digit match.";
				break;
			case "MOT":
				toLog = string.Format("Press Outer Center when the last seconds digit is {0}.", (selectedModID + (5 - (1 + currentInputPos)) + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10);
				break;
			case "PRN":
				toLog = string.Format("Press {0} Center because {1} is {2}.", primesUnder20.Contains(selectedModID % 20) ? "Inner" : "Outer", selectedModID % 20, primesUnder20.Contains(selectedModID % 20) ? "prime" : "not prime");
				break;
			case "CHK":
				toLog = string.Format("Press {0} Center because {1} is {2}.", primesUnder20.Contains(selectedModID % 20) ? "Outer" : "Inner", selectedModID % 20, primesUnder20.Contains(selectedModID % 20) ? "prime" : "not prime");
				break;
			case "BOB":
				toLog = "Press Inner Center.";
				break;
			case "REP":
			case "EAT":
				if (!lastCorrectInputs.Any())
					toLog = "There were no previous inputs. Press Inner Center.";
				else
					toLog = string.Format("The last input was {0}, so press that.", lastCorrectInputs[currentInputPos - 1]);
				break;
			case "STR":
			case "IKE":
				{
                    string lastColor = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last() : "Red";
                    toLog = string.Format("Start on {0}. Count the number of colored buttons counter-clockwise as there are strikes obtained so far. Press the resulting button.", lastColor);
					break;
				}
			case "SKP":
				string[] finaleInstructions = { "FIN", "ISH" };
				toLog = "Press Inner Center.";
				if (currentInputPos + 1 < splittedInstructions.Count && !finaleInstructions.Contains(splittedInstructions[currentInputPos + 1]))
					toLog += " The next instruction is skippable, so press Outer Center in replacement for the next instruction.";
				break;
			case "PVP":
				{
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
					}
					while (!primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CCW you should press is {0}.",rearrangedColorList[curIdx]);
					break;
				}
			case "NXP":
				{
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = (curIdx + 1) % 6;
					}
					while (!primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "PVS":
				{
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
					}
					while (primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CCW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "NXS":
				{
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = (curIdx + 1) % 6;
					}
					while (primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "OPP":
				if (!lastCorrectInputs.Any())
					toLog = "There were no previous inputs. Press Outer Center.";
				else
					toLog = string.Format("The last input was {0}, so press {1}.", lastCorrectInputs[currentInputPos - 1],
						lastCorrectInputs[currentInputPos - 1] == "Outer" ? "Inner Center" :
						lastCorrectInputs[currentInputPos - 1] == "Inner" ? "Outer Center" :
						rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6]);
				break;
			case "FIN":
			case "ISH":
				toLog = "This instruction is complicated. Refer to the manual for how to press this last command.";
				break;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Instruction {2} (\"{3}\"): {1}", loggingModID, toLog, currentInputPos + 1, splittedInstructions[currentInputPos]);
	}
	bool canSkip = false;
	void ProcessInstruction(string input)
	{
		if (isFinished) return;
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		bool isCorrect = true;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Pressing the {1} button at {2} on the countdown timer...", loggingModID, input, bombInfo.GetFormattedTime());
		int secondsTimer = (int)bombInfo.GetTime() % 60;
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		string[] finaleInstructions = { "FIN", "ISH" };
		if (canSkip)
		{
			isCorrect = input == "Outer";
			canSkip = false;
		}
		else
			switch (splittedInstructions[currentInputPos])
			{
				case "PCR":
					isCorrect = input == baseColorList[0];
					break;
				case "PCG":
					isCorrect = input == baseColorList[2];
					break;
				case "PCB":
					isCorrect = input == baseColorList[4];
					break;
				case "SCC":
					isCorrect = input == baseColorList[3];
					break;
				case "SCM":
					isCorrect = input == baseColorList[5];
					break;
				case "SCY":
					isCorrect = input == baseColorList[1];
					break;
				case "SUB":
					isCorrect = input == "Inner" && secondsTimer % 11 == 0;
					break;
				case "MOT":
					isCorrect = input == "Outer" && secondsTimer % 10 == (selectedModID + (5 - (1 + currentInputPos)) + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10;
					break;
				case "PRN":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ? "Inner" : "Outer");
					break;
				case "CHK":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ? "Outer" : "Inner");
					break;
				case "BOB":
					isCorrect = input == "Inner";
					if (bombInfo.IsIndicatorOn(Indicator.BOB) && bombInfo.GetBatteryCount() == 4 && bombInfo.GetBatteryHolderCount() == 2 && bombInfo.GetIndicators().Count() == 1)
					{
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: BOB is nice today. He will make you skip the rest of the instructions.", loggingModID);
						currentInputPos = splittedInstructions.Count;
					}
					break;
				case "REP":
				case "EAT":
					if (!lastCorrectInputs.Any())
						isCorrect = input == "Inner";
					else
						isCorrect = input == lastCorrectInputs[lastCorrectInputs.Count - 1];
					break;
				case "STR":
				case "IKE":
					{
						int strikeCount = TimeModeActive ? localStrikeCount : bombInfo.GetStrikes();
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : Array.IndexOf(rearrangedColorList, baseColorList[0]);
						curIdx -= strikeCount % 6;
						string resultingButton = rearrangedColorList[(curIdx + 6) % 6];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} strike(s) the resulting button should be {2}.", loggingModID, strikeCount, resultingButton);
						isCorrect = input == resultingButton;
						break;
					}
				case "SKP":
					{
						isCorrect = input == "Inner";
						if (currentInputPos + 1 < splittedInstructions.Count && !finaleInstructions.Contains(splittedInstructions[currentInputPos + 1]))
							canSkip = true;
						break;
					}
				case "PVP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "NXP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "PVS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "NXS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "OPP":
					{
						if (!lastCorrectInputs.Any() || lastCorrectInputs[lastCorrectInputs.Count - 1] == "Inner")
							isCorrect = input == "Outer";
						else if (lastCorrectInputs[lastCorrectInputs.Count - 1] == "Outer")
							isCorrect = input == "Inner";
						else
							isCorrect = input == rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6];
						break;
					}
				case "FIN":
				case "ISH":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
						int solvedCount = bombInfo.GetSolvedModuleIDs().Count();
						curIdx -= solvedCount % 6;
						while (curIdx < 0)
							curIdx += 6;
						isCorrect = input == rearrangedColorList[curIdx] && (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10 == secondsTimer % 10;
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} solved, {2} unsolved, the resulting button should be {3} which much be pressed when the last seconds digit is {4}.", loggingModID, solvedCount, bombInfo.GetSolvableModuleIDs().Count() - solvedCount, rearrangedColorList[curIdx], (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10);
					}
					break;
			}
		if (isCorrect)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The resulting press is correct.", loggingModID);
			string[] possibleSounds = { "button1", "button2", "button3", "button4" };
			lastCorrectInputs.Add(input);
			currentInputPos++;
			if (currentInputPos >= splittedInstructions.Count)
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: All instructions are handled correctly. You're done.", loggingModID);
				isFinished = true;
				modSelf.HandlePass();

				if (bombInfo.GetSolvableModuleIDs().Count == bombInfo.GetSolvedModuleIDs().Count && !noCopyright)
				{
					mAudio.PlaySoundAtTransform("TheFatRat-TheCalling-Loop", transform);
					StartCoroutine(HandleSpecialSolveAnim());
				}
				else
					StartCoroutine(HandleSolveAnim());

				return;
			}
			else if (!canSkip)
				LogCurrentInstruction();
			else
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The next instruction is getting skipped.", loggingModID);
			}
			mAudio.PlaySoundAtTransform(possibleSounds.PickRandom(), transform);
			UpdateStatusIndc();
		}
		else
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The resulting press is incorrect. Restarting from the first instruction...", loggingModID);
			if (currentInputPos + 1 >= splittedInstructions.Count)
				mAudio.PlaySoundAtTransform("Darkest Dungeon - OverconfidenceRant", transform);
			modSelf.HandleStrike();
			hasStruck = true;
			lastCorrectInputs.Clear();
			currentInputPos = 0;
			canSkip = false;
			localStrikeCount += TimeModeActive ? 1 : 0;
			StartCoroutine(HandleStrikeAnim());
			LogCurrentInstruction();
		}
	}
	void UpdateStatusIndc()
	{
		for (int a = 0; a < 6; a += 1)
		{
			statusIndicators[a].material.color = currentInputPos == a  ? Color.yellow : currentInputPos > a  ? Color.green : Color.black;
		}
	}
	// Update is called once per frame, may be scaled by other events
	void Update () {
		if (currentScreenVal == 1 && !isFinished)
		{
			string toDisplay = "";
			switch (idxCurStrikeDisplay)
			{
				case 0:
					toDisplay = ValueToFixedRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
					break;
				case 1:
					toDisplay = ValueToBrokenRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
					break;
				case 2:
					toDisplay = (TimeModeActive ? localStrikeCount : bombInfo.GetStrikes()).ToString();
					break;
			}
			strikeIDDisplay.text = string.Format("Strikes Detected:\n{0}", toDisplay);
		}
		if (autoCycleEnabled && !isFinished)
		{
			progressHandler.curProgress += Time.deltaTime;
			if (progressHandler.curProgress >= progressHandler.maxProgress)
			{
				progressHandler.curProgress = 0;
				currentScreenVal = (currentScreenVal + 1) % 3;
				UpdateSecondaryScreen();
			}
		}
		else
		{
			progressHandler.curProgress = Mathf.Max(0, progressHandler.curProgress - Time.deltaTime);
		}
	}

	public class UnfairsCruelRevengeSettings
    {
		public bool noCopyright = true;
    }

	string FormatSecondsToTime(int num)
	{
		return string.Format("{0}:{1}",num/60,num%60);
	}



	// TP Handling Begins here
	void TwitchHandleForcedSolve()
	{
		isFinished = true;
		//mAudio.PlaySoundAtTransform("TheFatRat-TheCalling-Loop", transform);
		StartCoroutine(HandleSolveAnim());
		modSelf.HandlePass();
	}
	IEnumerator HandleAutoCycleAnim(bool enable)
	{

		if (enable)
		{
			animBar.SetActive(true);
			for (float x = 1; x >= 0; x -= 1/25f)
			{
				animBar.transform.localPosition = new Vector3(0, 0, 2.5f * x);
				yield return new WaitForSeconds(Time.deltaTime);
			}
			autoCycleEnabled = true;
			
		}
		else
		{
			autoCycleEnabled = false;
			for (float x = 0; x <= 1; x += 1/25f)
			{
				animBar.transform.localPosition = new Vector3(0, 0, 2.5f * x);
				yield return new WaitForSeconds(Time.deltaTime);
			}
			animBar.SetActive(false);
		}
		

		yield return null;

	}

	bool TimeModeActive;
#pragma warning disable IDE0051 // Remove unused private members
	bool ZenModeActive;
	bool TwitchShouldCancelCommand;
	readonly string TwitchHelpMessage =
		"Select the given button with \"!{0} press R(ed);G(reen);B(lue);C(yan);M(agenta);Y(ellow);Inner;Outer\" " +
		"To time a specific press, append based only on seconds digits (##), full time stamp (DD:HH:MM:SS), or MM:SS where MM exceeds 99 min. " +
		"To press the idx/strike screen \"!{0} screen\" Semicolons can be used to combine presses, both untimed and timed.\n"+
		"Enable autocycle on the screen by using \"!{0} autocycle ##.###\", or turn autocycle off with \"!{0} autocycle off\". Get the colors of the buttons around the module by using \"!{0} colorblind\" or \"!{0} cycle\"";
#pragma warning restore IDE0051 // Remove unused private members
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (!hasStarted)
		{
			yield return "sendtochaterror The module has not activated yet. Wait for a bit until the module has started.";
			yield break;
		}
		if (isFinished)
		{
			yield return "sendtochaterror The module is already solved, why bother trying to interact with it? (This is an anarchy command prevention message.)";
			yield break;
		}
		string baseCommand = command.ToLower();
		string[] intereptedParts = command.ToLower().Split(';');
		List<KMSelectable> selectedCommands = new List<KMSelectable>();
		List<List<int>> timeThresholds = new List<List<int>>();
		List<string> rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToList();

		int[] multiplierTimes = { 1, 60, 3600, 86400 }; // To denote seconds, minutes, hours, days in seconds.
		if (Application.isEditor)
		{
			if (command.ToLower().RegexMatch(@"^simulate (off|on)$"))
			{
				yield return null;
				string[] commandParts = command.Split();
				gameInfo.OnLightsChange(commandParts[1].EqualsIgnoreCase("off"));
				yield break;
			}
		}
		if (baseCommand.RegexMatch(@"^autocycle (\d+(\.\d+)?|off|disable|deactivate)$"))
		{
			string[] shutoffCommands = { "off", "disable", "deactivate" };

			string curCommand = baseCommand.Split()[1];
			float cycleSpeed = 0;

			if (float.TryParse(curCommand, out cycleSpeed))
			{

				if (cycleSpeed < 0.5f || cycleSpeed > 10f)
				{
					yield return "sendtochaterror I am not setting Auto-Cycle for Unfair's Cruel Revenge (#{1}) at "+cycleSpeed.ToString("0.00")+" intervals.";
					yield break;
				}
				if (cycleSpeed == progressHandler.maxProgress && autoCycleEnabled)
				{
					yield return "sendtochaterror Auto-Cycle interval for Unfair's Cruel Revenge (#{1}) is already at " + cycleSpeed.ToString("0.00") + ".";
					yield break;
				}
				yield return null;
				progressHandler.maxProgress = cycleSpeed;
				if (!autoCycleEnabled)
					StartCoroutine(HandleAutoCycleAnim(true));
				yield return "sendtochat {0}, Auto-Cycle has been enabled/adjusted for Unfair's Cruel Revenge (#{1}) at " + cycleSpeed.ToString("0.00") + " intervals.";
				
			}
			else if (shutoffCommands.Contains(curCommand))
			{
				if (!autoCycleEnabled)
				{
					yield return "sendtochaterror Auto-Cycle for Unfair's Cruel Revenge (#{1}) is already off.";
					yield break;
				}
				yield return null;
				StartCoroutine(HandleAutoCycleAnim(false));
				yield return "sendtochat {0}, autocycle has been disabled for Unfair's Cruel Revenge (#{1}).";
			}
			else
			{
				yield return string.Format("sendtochaterror I don't know what autocycle subcommand \"{0}\" is.",curCommand);
				yield break;
			}
		}
		else if (baseCommand.RegexMatch(@"^colou?rblind|cycle$"))
		{
			bool lastColorblindState = colorblindDetected;
			colorblindDetected = true;
			for (int x = 0; x < 6 && !TwitchShouldCancelCommand; x++)
			{
				var curSelected = colorButtonSelectables[x].Highlight.gameObject;
				var highlight = curSelected.transform.Find("Highlight(Clone)");
				if (highlight != null)
					curSelected = highlight.gameObject ?? curSelected;
				yield return null;
				colorButtonSelectables[x].OnHighlight();
				curSelected.SetActive(true);
                yield return new WaitForSeconds(1.5f);
				if (TwitchShouldCancelCommand || x == 5)
					colorButtonSelectables[x].OnHighlightEnded();
				curSelected.SetActive(false);
				yield return new WaitForSeconds(0.1f);
			}
			if (TwitchShouldCancelCommand)
			{
				colorButtonSelectables[0].OnHighlightEnded();
				yield return "cancelled";
			}
			colorblindDetected = lastColorblindState;
			yield break;
		}
		else
			foreach (string commandPart in intereptedParts)
		{
			string partTrimmed = commandPart.Trim();
			if (partTrimmed.RegexMatch(@"^press "))
			{
				partTrimmed = partTrimmed.Substring(6);
			}
			string[] partOfPartTrimmed = partTrimmed.Split();
			if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer)( (at|on))?( (([0-9]+:)?([0-5][0-9]:){1,2}|[0-9]+)[0-5][0-9])+$"))
			{
				List<int> possibleTimes = new List<int>();
				for (int x = partOfPartTrimmed.Length - 1; x > 0; x--)
				{
					if (!partOfPartTrimmed[x].RegexMatch(@"^[0-9]+:([0-5][0-9]:){1,2}[0-5][0-9])$")) break;
					string[] curTimePart = partOfPartTrimmed[x].Split(':').Reverse().ToArray();
					int curTime = 0;
					for (int idx = 0; idx < curTimePart.Length; idx++)
					{
						curTime += multiplierTimes[idx] * int.Parse(curTimePart[idx]);
					}
					possibleTimes.Add(curTime);
				}

				possibleTimes = possibleTimes.Where(a => ZenModeActive ? a > bombInfo.GetTime() : a < bombInfo.GetTime()).ToList(); // Filter out all possible times by checking if the time stamp is possible

				if (possibleTimes.Any())
				{
					timeThresholds.Add(possibleTimes);
				}
				else
				{
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The full command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer)( (at|on))?( [0-5][0-9])+$"))
			{
				
				List<int> possibleTimes = new List<int>();
				for (int idx = partOfPartTrimmed.Length - 1; idx > 0; idx--)
				{
					if (!partOfPartTrimmed[idx].RegexMatch(@"^[0-5][0-9]$")) break;
					int secondsTime = int.Parse(partOfPartTrimmed[idx]);
					int curMinRemaining = (int)bombInfo.GetTime()/60;
					for (int x = curMinRemaining - (ZenModeActive ? 0 : 2); x <= curMinRemaining + (ZenModeActive ? 2 : 0); x++)
					{
						if (x * 60 + secondsTime > bombInfo.GetTime() && ZenModeActive)
						{
							possibleTimes.Add(x * 60 + secondsTime);
						}
						else if (x * 60 + secondsTime < bombInfo.GetTime() && !ZenModeActive)
						{
							possibleTimes.Add(x * 60 + secondsTime);
						}
					}

				}
				if (possibleTimes.Any())
				{
					timeThresholds.Add(possibleTimes);
				}
				else
				{
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The full command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer|screen)$"))
			{
				timeThresholds.Add(new List<int>());
			}
			else
			{
				yield return string.Format("sendtochaterror \"{0}\" is not a valid sub command, check your command for typos.",partTrimmed);
				yield break;
			}
			switch (partOfPartTrimmed[0])
			{
				case "r":
				case "red":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Red")]);
					break;
				case "g":
				case "green":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Green")]);
					break;
				case "b":
				case "blue":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Blue")]);
					break;
				case "c":
				case "cyan":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Cyan")]);
					break;
				case "m":
				case "magenta":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Magenta")]);
					break;
				case "y":
				case "yellow":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Yellow")]);
					break;
				case "inner":
					selectedCommands.Add(innerSelectable);
					break;
				case "outer":
					selectedCommands.Add(outerSelectable);
					break;
				case "screen":
					selectedCommands.Add(idxStrikeSelectable);
					break;
				default:
					yield return "sendtochaterror You aren't supposed to get this error. If you did, it's a bug, so please contact the developer about this.";
					yield break;
			}
		}
		hasStruck = false;
		if (selectedCommands.Any())
		{
			yield return "multiple strikes";
			for (int x = 0; x < selectedCommands.Count && !hasStruck; x++)
			{
				if (hasStruck) yield break;
				if (timeThresholds[x].Any())
				{
					List<int> currentTimeThresholds = timeThresholds[x].Where(a => ZenModeActive ? a > bombInfo.GetTime() : a < bombInfo.GetTime()).ToList();
					if (!currentTimeThresholds.Any())
					{
						yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
						yield break;
					}
					int targetTime = ZenModeActive ? currentTimeThresholds.Min() : currentTimeThresholds.Max();
					yield return string.Format("sendtochat Target time for press #{0} in command: {1}", x + 1, FormatSecondsToTime(targetTime));
					bool canPlayWaitingMusic = Mathf.Abs(targetTime - bombInfo.GetTime()) >= 25;
					if (canPlayWaitingMusic)
					{
						yield return "waiting music";
						yield return "sendtochat This press will take a while, if you wish to cancel this command, do \"!cancel\" now.";
					}
					do
					{
						yield return string.Format("trycancel Your timed interation has been canceled after a total of {0}/{1} presses in the command that was sent.", x + 1, selectedCommands.Count);
						if ((int)bombInfo.GetTime() > targetTime && ZenModeActive)
						{
							currentTimeThresholds = currentTimeThresholds.Where(a => a > bombInfo.GetTime()).ToList();
							if (!currentTimeThresholds.Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
								yield break;
							}
							targetTime = currentTimeThresholds.Min();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0} in the command that was sent.", x + 1, FormatSecondsToTime(targetTime));
						}
						else if ((int)bombInfo.GetTime() < targetTime && !ZenModeActive)
						{
							currentTimeThresholds = currentTimeThresholds.Where(a => a < bombInfo.GetTime()).ToList();
							if (!currentTimeThresholds.Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
								yield break;
							}
							targetTime = currentTimeThresholds.Max();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0} in the command that was sent.", x + 1, FormatSecondsToTime(targetTime));
						}
					}
					while ((int)bombInfo.GetTime() != targetTime);
					if (canPlayWaitingMusic)
						yield return "end waiting music";
				}
				yield return null;
				selectedCommands[x].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			yield return "end multiple strikes";
		}
		yield break;
	}
}
