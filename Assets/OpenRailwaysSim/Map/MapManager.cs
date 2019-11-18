using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// マップを読み書きするクラス
/// </summary>
public class MapManager
{

    public const string filename_map = "map.bin"; // マップのデータファイルの名前
    public const string filename_info = "info.bin"; // マップの情報ファイルの名前
    public static string dir; // マップファイルを格納するフォルダ

    public static string[] randommapnames = new string[] {"hokkaido", "hakodate", "nemuro", "chitose", "muroran", "sekisho", "furano", "rumoi", "soya", "semmo", "sekihoku", "sassho", "hidaka", "kaikyo", "horonai", "matsumae", "utashinai", "shibetsu", "nayoro", "tempoku", "chihoku", "shimmei", "esashi",
        "tohoku", "joetsu", "hokuriku", "yamanote", "negishi", "yokohama", "nambu", "musashino", "keiyo", "akabane", "kawagoe", "chuo", "ome", "itsukaichi", "sobu", "yokosuka", "tsurumi", "sagami", "tokaido", "takasaki", "joban", "ryomo", "mito", "sotobo", "uchibo", "narita", "ito", "shinetsu", "shinonoi", "uetsu", "hakushin", "banetsu", "senzan", "senseki", "ou", "hachiko", "azuma", "karasuyama", "nikko", "suigun", "kashima", "kururi", "togane", "koumi", "iiyama", "oito", "echigo", "yahiko", "yonesaka", "tadami", "ishinomaki", "kesennuma", "ofunato", "rikuu", "kitakami", "kamaishi", "yamada", "hanawa", "hachinohe", "ominato", "tsugaru", "aterazawa", "tazawako", "oga", "gono", "iwaizumi",
        "gotenba", "kansai", "kisei", "minobu", "iida", "taketoyo", "takayama", "taita", "meisho", "sangu", "johoku",
        "sanyo", "kosei", "sanin", "kusatsu", "nara", "osaka", "sakurajima", "fukuchiyama", "katamachi", "tozai", "osaka-higashi", "kansai-airport", "hanwa", "uno", "honshibisan", "hakubi", "kure", "ube", "mine", "hakata-minami", "obama", "etsumi", "nanao", "johana", "himi", "sakurai", "wakayama", "kakogawa", "kishin", "maizuru", "bantan", "ako", "tsuyama", "kibi", "geibi", "fukuen", "imbi", "sakai", "kisuki", "sanko", "kabe", "gantoku", "yamaguchi", "onoda", "kajiya", "taisha", "naniwasuji",
        "yosan", "kotoku", "dosan", "uchiko", "yodo", "naruto", "tokushima", "mugi",
        "kyushu", "kagoshima", "sasaguri", "nagasaki", "chikuhi", "sasebo", "nippo", "miyazaki-airport", "kasii", "misumi", "hisatsu", "ibusuki-makurazaki", "karatsu", "omura", "kyudai", "houhi", "hitahikosan", "nichinan", "kitto", "chikuho", "gotoji", "yamano", "kamiyamada", "miyada",
        "shimminato",
        "isesaki", "kameido", "daishi", "sano", "kiryu", "koizumi", "nikko", "utsunomiya", "kinugawa", "noda", "tojo", "ogose", "maebashi", "ikaho", "keishi", "yaita", "kumagaya", "kariyado", "tonara", "oya", "otone", "izumi", "negoya", "tokugawa", "sengoku", "oguragawa", "ogano", "senju", "yanagiwara", "aisawa",
        "higashi-narita", "oshiage", "kanamachi", "chiba", "chihara", "narita-airport", "shirahige", "yatsu",
        "ikebukuro", "chichibu", "yurakucho", "toshima", "sayama", "shinjuku", "haijima", "tamako", "kokubunji", "seibuen", "tamagawa", "omiya", "ahina",
        "sagamihara", "keibajo", "dobutsuen", "takao", "goryo", "inokashira", "daita",
        "odawara", "enoshima", "tama", "mukogaoka-yuen",
        "toyoko", "meguro", "den-en-toshi", "oimachi", "ikegami", "setagaya", "kinuta", "shin-okusawa",
        "kuko", "zushi", "kurihama", "omori", "kamakura", "takeyama", "hayama",
        "hibiya", "ginza", "marunouchi", "namboku", "chiyoda", "hanzomon", "fukutoshin",
        "izumino", "atsugi", "nishi-samukawa", "kamiseya",
        "nagoya", "toyokawa", "nishio", "gamagori", "mikawa", "toyota", "tokoname", "chikko", "kowa", "chita", "seto", "tsushima", "bisai", "inuyama", "kakamigahara", "hiromi", "komaki", "takehana", "hashima", "kachigawa", "ohamaguchi", "kiyosu", "okosi", "atsumi", "kozakai", "heisaka", "takatomi", "anjo", "okazaki", "fukuoka", "koromo", "iwakura", "kagashima", "ichinomiya", "chiryu", "gifu", "minomachi", "yaotsu", "tanigumi", "ibi", "tagami"
    }; //マップ名のサンプル

    public static void reloadDir()
    {
        dir = Path.Combine(Application.persistentDataPath, "maps");
        Directory.CreateDirectory(dir);
    }

    public static string[] getMapList()
    {
        var maplist = new List<string>();
        reloadDir();
        var mapdirs = Directory.GetDirectories(dir);
        for (var a = 0; a < mapdirs.Length; a++)
        {
            var mapname = new DirectoryInfo(mapdirs[a]).Name;
            if (File.Exists(Path.Combine(mapdirs[a], filename_map)))
                maplist.Add(mapname);
        }
        return maplist.ToArray();
    }

    public static string getRandomMapName()
    {
        return randommapnames[UnityEngine.Random.Range(0, randommapnames.Length)];
    }

    public static Map.MapInfo loadMapInfo(string mapname)
    {
        reloadDir();
        var mapdir = Path.Combine(dir, mapname);
        if (Directory.Exists(mapdir))
        {
            var datpath = Path.Combine(mapdir, filename_info);
            if (File.Exists(datpath))
            {
                var formatter = new BinaryFormatter();

                var stream = new FileStream(datpath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Map.MapInfo info = null;
                try
                {
                    info = (Map.MapInfo)formatter.Deserialize(stream);
                }
                catch { }
                stream.Close();

                return info;
            }
        }
        return null;
    }

    public static Map loadMap(string mapname)
    {
        reloadDir();
        var mapdir = Path.Combine(dir, mapname);
        if (Directory.Exists(mapdir))
        {
            var datpath = Path.Combine(mapdir, filename_map);
            if (File.Exists(datpath))
            {
                var formatter = new BinaryFormatter();

                var stream = new FileStream(Path.Combine(mapdir, filename_info), FileMode.Open, FileAccess.Read, FileShare.Read);
                Map.MapInfo info = null;
                try
                {
                    info = (Map.MapInfo)formatter.Deserialize(stream);
                }
                catch { }
                stream.Close();

                stream = new FileStream(datpath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Map map = null;
                try
                {
                    map = (Map)formatter.Deserialize(stream);
                    map.mapname = mapname;
                    map.info = info;
                }
                catch (EndOfStreamException)
                {
                    // TODO マップ非対応の表示
                }
                stream.Close();

                return map;
            }
        }
        return null;
    }

    public static void saveMap(Map map)
    {
        reloadDir();
        var mapdir = Path.Combine(dir, map.mapname);
        Directory.CreateDirectory(mapdir);
        IFormatter formatter = new BinaryFormatter();

        map.info.update();
        Stream stream = new FileStream(Path.Combine(mapdir, filename_info), FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, map.info);
        stream.Close();

        stream = new FileStream(Path.Combine(mapdir, filename_map), FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, map);
        stream.Close();
    }

    public static bool deleteMap(string mapname)
    {
        reloadDir();
        string mapdir = Path.Combine(dir, mapname);
        if (Directory.Exists(mapdir))
        {
            string datpath = Path.Combine(mapdir, filename_map);
            if (File.Exists(datpath))
            {
                File.Delete(datpath);
                File.Delete(Path.Combine(mapdir, filename_info));
                try
                {
                    Directory.Delete(mapdir);
                }
                catch (IOException)
                {
                    // TODO フォルダの中身がある場合はフォルダを削除できない。
                }
                return true;
            }
        }
        return false;
    }
}
