using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MapManager {
	public const string mapfilename = "map.bin";
	public static string dir; //マップファイルを格納するフォルダ

	public static string[] randommapnames = new string[] {"hokkaido", "hakodate", "nemuro", "chitose", "muroran", "sekisho", "furano", "rumoi", "soya", "semmo", "sekihoku", "sassho", "hidaka", "kaikyo", "horonai", "matsumae", "utashinai", "shibetsu", "nayoro", "tempoku", "chihoku", "shimmei", "esashi",
		"tohoku", "joetsu", "hokuriku", "yamanote", "negishi", "yokohama", "nambu", "musashino", "keiyo", "akabane", "kawagoe", "chuo", "ome", "itsukaichi", "sobu", "yokosuka", "tsurumi", "sagami", "tokaido", "takasaki", "joban", "ryomo", "mito", "sotobo", "uchibo", "narita", "ito", "shinetsu", "shinonoi", "uetsu", "hakushin", "banetsu", "senzan", "senseki", "ou", "hachiko", "azuma", "karasuyama", "nikko", "suigun", "kashima", "kururi", "togane", "koumi", "iiyama", "oito", "echigo", "yahiko", "yonesaka", "tadami", "ishinomaki", "kesennuma", "ofunato", "rikuu", "kitakami", "kamaishi", "yamada", "hanawa", "hachinohe", "ominato", "tsugaru", "aterazawa", "tazawako", "oga", "gono", "iwaizumi",
		"gotenba", "kansai", "kisei", "minobu", "iida", "taketoyo", "takayama", "taita", "meisho", "sangu", "johoku",
		"sanyo", "kosei", "sanin", "kusatsu", "nara", "osaka", "sakurajima", "fukuchiyama", "katamachi", "tozai", "osaka higashi", "kansai airport", "hanwa", "uno", "honshibisan", "hakubi", "kure", "ube", "mine", "hakata minami", "obama", "etsumi", "nanao", "johana", "himi", "sakurai", "wakayama", "kakogawa", "kishin", "maizuru", "bantan", "ako", "tsuyama", "kibi", "geibi", "fukuen", "imbi", "sakai", "kisuki", "sanko", "kabe", "gantoku", "yamaguchi", "onoda", "kajiya", "taisha", "naniwasuji",
		"yosan", "kotoku", "dosan", "uchiko", "yodo", "naruto", "tokushima", "mugi",
		"kyushu", "kagoshima", "sasaguri", "nagasaki", "chikuhi", "sasebo", "nippo", "miyazaki airport", "kasii", "misumi", "hisatsu", "ibusuki makurazaki", "karatsu", "omura", "kyudai", "houhi", "hitahikosan", "nichinan", "kitto", "chikuho", "gotoji", "yamano", "kamiyamada", "miyada",
		"shimminato",
		"isesaki", "kameido", "daishi", "sano", "kiryu", "koizumi", "nikko", "utsunomiya", "kinugawa", "noda", "tojo", "ogose", "maebashi", "ikaho", "keishi", "yaita", "kumagaya", "kariyado", "tonara", "oya", "otone", "izumi", "negoya", "tokugawa", "sengoku", "oguragawa", "ogano", "senju", "yanagiwara", "aisawa",
		"higashi narita", "oshiage", "kanamachi", "chiba", "chihara", "narita airport", "shirahige", "yatsu",
		"ikebukuro", "chichibu", "yurakucho", "toshima", "sayama", "shinjuku", "haijima", "tamako", "kokubunji", "seibuen", "tamagawa", "omiya", "ahina",
		"sagamihara", "keibajo", "dobutsuen", "takao", "goryo", "inokashira", "daita",
		"odawara", "enoshima", "tama", "mukogaokayuen",
		"toyoko", "meguro", "denentoshi", "oimachi", "ikegami", "setagaya", "kinuta", "shinokusawa",
		"kuko", "zushi", "kurihama", "omori", "kamakura", "takeyama", "hayama",
		"hibiya", "ginza", "marunouchi", "namboku", "chiyoda", "hanzomon", "fukutoshin",
		"izumino", "atsugi", "nishisamukawa", "kamiseya",
		"nagoya", "toyokawa", "nishio", "gamagori", "mikawa", "toyota", "tokoname", "chikko", "kowa", "chita", "seto", "tsushima", "bisai", "inuyama", "kakamigahara", "hiromi", "komaki", "takehana", "hashima", "kachigawa", "ohamaguchi", "kiyosu", "okosi", "atsumi", "kozakai", "heisaka", "takatomi", "anjo", "okazaki", "fukuoka", "koromo", "iwakura", "kagashima", "ichinomiya", "chiryu", "gifu", "minomachi", "yaotsu", "tanigumi", "ibi", "tagami"
	}; //マップ名のサンプル

	public static void reloadDir () {
		dir = Path.Combine (Application.persistentDataPath, "maps");
		Directory.CreateDirectory (dir);
	}

	public static string[] getMapList () {
		List<string> maplist = new List<string> ();
		reloadDir ();
		string[] mapdirs = Directory.GetDirectories (dir);
		for (int a = 0; a < mapdirs.Length; a++) {
			string mapname = new DirectoryInfo (mapdirs [a]).Name;
			string[] files = Directory.GetFiles (Path.Combine (dir, mapname));
			string datpath = null;
			for (int b = 0; b < files.Length; b++) {
				if (Path.GetFileName (files [b]).Equals (mapfilename)) {
					datpath = files [b];
					break;
				}
			}
			if (datpath != null) {
				maplist.Add (mapname);
			}
		}
		return maplist.ToArray ();
	}

	public static string getRandomMapName () {
		/*if (randommapnames.Length == 0) {
			return "";
		}*/
		//マップが存在するかどうか確認する場合は無限ループを避けるためwhileを使っていはいけない。
		return randommapnames [UnityEngine.Random.Range (0, randommapnames.Length)];
	}

	public static Map loadMap (string mapname) {
		reloadDir ();
		string mapdir = Path.Combine (dir, mapname);
		if (Directory.Exists (mapdir)) {
			string[] files = Directory.GetFiles (mapdir);
			string datpath = null;
			for (int a = 0; a < files.Length; a++) {
				if (Path.GetFileName (files [a]).Equals (mapfilename)) {
					datpath = files [a];
					break;
				}
			}
			if (datpath != null) {
				IFormatter formatter = new BinaryFormatter ();
				Stream stream = new FileStream (Path.Combine (mapdir, mapfilename), FileMode.Open, FileAccess.Read, FileShare.Read);
				Map map = null;
				try {
					map = (Map)formatter.Deserialize (stream);
				} catch (EndOfStreamException) {
					//Debug.LogError (DateTime.Now + " マップが対応していません: " + mapname);
				}
				stream.Close ();
				return map;
			}
		}
		return null;
	}

	public static void aaa (Map map) {
		reloadDir ();
		string mapdir = Path.Combine (dir, map.mapname);
		Directory.CreateDirectory (mapdir);
		IFormatter formatter = new BinaryFormatter ();
		Stream stream = new FileStream (Path.Combine (mapdir, mapfilename), FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize (stream, map);
		stream.Close ();
	}

	public static void saveMap (Map map) {
		Debug.Log (DateTime.Now + " マップ\"" + map.mapname + "\"をセーブ中...");
		aaa (map);
		Debug.Log (DateTime.Now + " マップをセーブしました");
	}

	public static bool deleteMap (string mapname) {
		reloadDir ();
		string mapdir = Path.Combine (dir, mapname);
		if (Directory.Exists (mapdir)) {
			string[] files = Directory.GetFiles (mapdir);
			string datpath = null;
			for (int a = 0; a < files.Length; a++) {
				if (Path.GetFileName (files [a]).Equals (mapfilename)) {
					datpath = files [a];
					break;
				}
			}
			if (datpath != null) {
				File.Delete (datpath);
				try {
					Directory.Delete (mapdir);
				} catch (IOException) {
					//フォルダの中身がある場合はフォルダを削除できない。
				}
				Debug.Log (DateTime.Now + " マップを削除しました: " + mapname);
				return true;
			}
		}
		return false;
	}
}
