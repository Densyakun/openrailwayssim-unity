using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

//ゲームの動作を制御する中心的クラス
public class Main : MonoBehaviour
{
    public const string VERSION = "0.001alpha";
    public const string KEY_FIRSTSTART = "FIRSTSTART";
    public const string KEY_SETUPPED = "SETUPPED";
    public const string KEY_DRAW_DISTANCE = "DRAWDISTANCE";
    public const string KEY_BGM_VOLUME = "BGM_VOL";
    public const string KEY_SE_VOLUME = "SE_VOL";
    public const string KEY_CAMERA_MOVE_SPEED = "CAMERA_MOVE_SPEED";
    public const string KEY_DRAG_ROT_SPEED = "DRAG_ROT_SPEED";
    public const string KEY_ANTIALIASING = "ANTIALIASING";
    public const string KEY_AO = "AO";
    public const string KEY_MOTIONBLUR = "MOTIONBLUR";
    public const string KEY_BLOOM = "BLOOM";
    public const string KEY_VIGNETTE = "VIGNETTE";

    public const int MIN_DRAW_DISTANCE = 1;
    public const int MAX_DRAW_DISTANCE = 8;
    public const int DEFAULT_DRAW_DISTANCE = MAX_DRAW_DISTANCE / 4;
    public const float MIN_BGM_VOLUME = 0f;
    public const float MAX_BGM_VOLUME = 1f;
    public const float DEFAULT_BGM_VOLUME = 1f / 3;
    public const float MIN_SE_VOLUME = 0f;
    public const float MAX_SE_VOLUME = 1f;
    public const float DEFAULT_SE_VOLUME = 1f;
    public const float MIN_CAMERA_MOVE_SPEED = 0.5f;
    public const float MAX_CAMERA_MOVE_SPEED = 1000f;
    public const float DEFAULT_CAMERA_MOVE_SPEED = 100f;
    public const float MIN_DRAG_ROT_SPEED = 0.1f;
    public const float MAX_DRAG_ROT_SPEED = 3f;
    public const float DEFAULT_DRAG_ROT_SPEED = 0.5f;
    public const bool DEFAULT_ANTIALIASING = true;
    public const bool DEFAULT_AO = true;
    public const bool DEFAULT_MOTIONBLUR = true;
    public const bool DEFAULT_BLOOM = true;
    public const bool DEFAULT_VIGNETTE = true;

    public static Main main;
    public static Map playingmap { get; private set; }
    public static string ssdir { get; private set; }
    public static int min_fps = 15;

    private static bool firstStart = false;
    public static bool isFirstStart
    {
        get { return firstStart; }
        private set { firstStart = value; }
    }
    public static DateTime[] firstStartTimes { get; private set; }
    public static bool isSetupped = false;
    public static int drawDistance = DEFAULT_DRAW_DISTANCE;
    public static float bgmVolume = DEFAULT_BGM_VOLUME;
    public static float seVolume = DEFAULT_SE_VOLUME;
    public static float cameraMoveSpeed = DEFAULT_CAMERA_MOVE_SPEED;
    public static float dragRotSpeed = DEFAULT_DRAG_ROT_SPEED;
    public static bool antialiasing = DEFAULT_ANTIALIASING;
    public static bool ao = DEFAULT_AO;
    public static bool motionBlur = DEFAULT_MOTIONBLUR;
    public static bool bloom = DEFAULT_BLOOM;
    public static bool vignette = DEFAULT_VIGNETTE;

    public static bool _pause = false;
    public static bool pause { get; private set; } //ポーズ

    private static float lasttick = 0; //時間を進ませた時の余り
    private static float lasttick_few = 0; //頻繁に変更しないするための計算。この機能は一秒ごとに処理を行う。
    public static int mode = 0; //操作モード 0=なし 1=軌道敷設 2=軌道削除 11=車両設置
    public static int MODE_CONSTRUCT_TRACK = 1;
    public static int MODE_REMOVE_TRACK = 2;
    public static int MODE_PLACE_CAR = 11;
    public static Track editingTrack;
    public static Quaternion? editingRot;
    public static float selectionDist;
    public static Track mainTrack;
    public static Track selection;

    public Gradient sunGradient;
    public Light sun; //太陽
    public Camera mainCamera;
    public AudioClip[] titleClips;
    public AudioSource bgmSource;
    public AudioSource seSource;
    public Material line_mat;
    public Material selection_line_mat;
    public GameObject point;

    void Awake()
    {
        main = this;

        //ゲーム起動日時の取得
        string a = PlayerPrefs.GetString(KEY_FIRSTSTART);//変数aは使いまわしているので注意
        bool b = false;
        List<DateTime> c = new List<DateTime>();
        try
        {
            String[] d = a.Split(',');
            for (int e = 0; e < d.Length; e++)
            {
                c.Add(new DateTime(long.Parse(d[e].Trim())));
            }
            if (d.Length == 0)
            {
                b = true;
            }
        }
        catch (FormatException)
        {
            b = true;
        }

        //初回起動かどうか（初期設定などをせずに一度ゲームを終了した場合などに対応できないため、あまり使えない）
        if (b)
        {
            firstStart = true;
        }

        //今回の起動日時を追加
        c.Add(DateTime.Now);
        firstStartTimes = c.ToArray();
        a = "";
        for (int f = 0; f < firstStartTimes.Length; f++)
        {
            if (f != 0)
            {
                a += ", ";
            }
            a += firstStartTimes[f].Ticks;
        }
        PlayerPrefs.SetString(KEY_FIRSTSTART, a);

        //ゲーム起動日時及び、ゲーム初回起動情報をコンソールに出力
        //print ("firstStart: " + firstStart);
        /*a = "{ ";
		for (int f = 0; f < firstStartTimes.Length; f++) {
			if (f != 0) {
				a += ", ";
			}
			a += firstStartTimes [f].Year + "/" + firstStartTimes [f].Month + "/" + firstStartTimes [f].Day + "-" + firstStartTimes [f].Hour + ":" + firstStartTimes [f].Minute + ":" + firstStartTimes [f].Second;
		}
		a += " }";
		print ("firstStartTimes: " + a);*/

        ssdir = Path.Combine(Application.persistentDataPath, "screenshots");

        //初期設定を行っているかどうか
        isSetupped = PlayerPrefs.GetInt(KEY_SETUPPED, 0) == 1;
        //print ("isSetupped: " + isSetupped);

        drawDistance = PlayerPrefs.GetInt(KEY_DRAW_DISTANCE, DEFAULT_DRAW_DISTANCE);
        bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, DEFAULT_BGM_VOLUME);
        seVolume = PlayerPrefs.GetFloat(KEY_SE_VOLUME, DEFAULT_SE_VOLUME);
        cameraMoveSpeed = PlayerPrefs.GetFloat(KEY_CAMERA_MOVE_SPEED, DEFAULT_CAMERA_MOVE_SPEED);
        dragRotSpeed = PlayerPrefs.GetFloat(KEY_DRAG_ROT_SPEED, DEFAULT_DRAG_ROT_SPEED);
        antialiasing = PlayerPrefs.GetInt(KEY_ANTIALIASING, DEFAULT_ANTIALIASING ? 1 : 0) == 1;
        ao = PlayerPrefs.GetInt(KEY_AO, DEFAULT_AO ? 1 : 0) == 1;
        motionBlur = PlayerPrefs.GetInt(KEY_MOTIONBLUR, DEFAULT_MOTIONBLUR ? 1 : 0) == 1;
        bloom = PlayerPrefs.GetInt(KEY_BLOOM, DEFAULT_BLOOM ? 1 : 0) == 1;
        vignette = PlayerPrefs.GetInt(KEY_VIGNETTE, DEFAULT_VIGNETTE ? 1 : 0) == 1;
        reflectSettings();
        saveSettings();
    }

    void Start()
    {
        /*if (isSetupped) {
			GameCanvas.titlePanel.show (true);
		} else {
			//TODO 初期設定
		}*/

        GameCanvas.titlePanel.show(true);
    }

    void Update()
    {
        //操作（カメラを除く）
        if (Input.GetKeyDown(KeyCode.F2))
        {
            screenShot();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playingmap != null && !GameCanvas.settingPanel.isShowing() && !GameCanvas.titleBackPanel.isShowing())
            {
                if (GameCanvas.trackSettingPanel.isShowing())
                {
                    GameCanvas.trackSettingPanel.show(false);
                    if (editingTrack != null)
                    {
                        editingTrack.entity.Destroy();
                        editingTrack = null;
                    }
                    editingRot = null;
                }
                else
                    setPause(!pause);
            }
        }

        if (playingmap != null)
        {
            if (pause)
            {
                playingmap.cameraPos = main.mainCamera.transform.position;
                playingmap.cameraRot = main.mainCamera.transform.eulerAngles;
            }
            else
            {
                //時間を進ませる
                lasttick += Time.deltaTime * 1000f;
                lasttick_few += Time.deltaTime;
                if (playingmap.fastForwarding)
                {
                    lasttick *= Map.FAST_FORWARDING_SPEED;
                    lasttick_few *= Map.FAST_FORWARDING_SPEED;
                }

                int ticks = Mathf.FloorToInt(lasttick);
                lasttick -= ticks;

                int ticks_few = Mathf.FloorToInt(lasttick_few);
                lasttick_few -= ticks_few;
                if (ticks_few != 0)
                    reloadLighting();

                if (ticks != 0)
                    playingmap.TimePasses(ticks);
            }

            if (!GameCanvas.pausePanel.isShowing() && !CameraMover.INSTANCE.dragging && !EventSystem.current.IsPointerOverGameObject() && !(GameCanvas.trackSettingPanel.isShowing() && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                RaycastHit hit;
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    MapEntity entity = hit.collider.GetComponent<MapEntity>();
                    if (entity == null && hit.collider.transform.parent)
                        entity = hit.collider.transform.parent.GetComponent<MapEntity>();
                    if (entity != null && (editingTrack == null ? true : editingTrack.entity != null && entity.gameObject != editingTrack.entity.gameObject) && entity.obj is Track)
                    {
                        Track a = selection;
                        (selection = (Track)entity.obj).reloadEntity();
                        if (a != null)
                            a.reloadEntity();
                    }
                    else
                    {
                        Track a = selection;
                        selection = null;
                        if (a != null)
                            a.reloadEntity();
                    }

                    if (mode == MODE_CONSTRUCT_TRACK)
                    {
                        Vector3 p = hit.point;
                        p.y = 0;

                        if (selection != null)
                        {
                            if (selection is Curve)
                            {
                                Vector3 a = Quaternion.Inverse(selection.rot) * (hit.point - selection.pos);
                                float r1 = ((Curve)selection).radius;
                                if (r1 < 0)
                                {
                                    r1 = -r1;
                                    a.x = -a.x;
                                }
                                float r2 = Vector3.Distance(a, Vector3.right * r1);
                                float A = Mathf.Atan(a.z / (r2 - a.x));
                                if (A < 0)
                                    A = Mathf.PI + A;
                                if (a.z < 0)
                                    A += Mathf.PI;
                                selectionDist = A * r1;
                                if (selectionDist < Track.MIN_TRACK_LENGTH || selectionDist > Mathf.PI * 2 * r1 - Track.MIN_TRACK_LENGTH)
                                    selectionDist = 0;
                                else if (selectionDist > selection.length - Track.MIN_TRACK_LENGTH)
                                    selectionDist = selection.length;
                                p = selection.getPoint(selectionDist / selection.length);
                            }
                            else
                            {
                                selection = (Track)entity.obj;
                                selectionDist = (Quaternion.Inverse(selection.rot) * (hit.point - selection.pos)).z;
                                if (selectionDist < Track.MIN_TRACK_LENGTH)
                                    selectionDist = 0;
                                else if (selectionDist > selection.length - Track.MIN_TRACK_LENGTH)
                                    selectionDist = selection.length;
                                p = selection.pos + selection.rot * Vector3.forward * selectionDist;
                            }
                        }

                        point.transform.position = p;
                        point.SetActive(true);
                        if (editingTrack != null)
                        {
                            if (editingRot == null)
                            {
                                editingTrack.entity.transform.LookAt(p);
                                editingTrack.SyncFromEntity();
                            }

                            if (editingTrack is Curve)
                            {
                                Vector3 v = Quaternion.Inverse(editingTrack.rot) * (p - editingTrack.pos);
                                if (v.z != 0)
                                {
                                    float a = Mathf.Atan(Mathf.Abs(v.x) / v.z);
                                    if (v.x < 0)
                                        a = -a;

                                    if (v.z < 0)
                                    {
                                        Vector2? i = GetIntersectionPointCoordinatesXZ(Vector3.zero, Vector3.right * v.x, v, v + Quaternion.Euler(new Vector3(0, a * 2 * Mathf.Rad2Deg)) * Vector3.right * v.z);
                                        if (i != null)
                                        {
                                            editingTrack.rot = Quaternion.Euler(0, editingTrack.rot.eulerAngles.y - 180, 0);
                                            editingTrack.length = Mathf.Abs(a * 2 * (((Curve)editingTrack).radius = -((Vector2)i).x));
                                        }
                                    }
                                    else
                                    {
                                        Vector2? i = GetIntersectionPointCoordinatesXZ(Vector3.zero, Vector3.right * v.x, v, v + Quaternion.Euler(new Vector3(0, a * 2 * Mathf.Rad2Deg)) * Vector3.right * v.z);
                                        if (i != null)
                                            editingTrack.length = Mathf.Abs(a * 2 * (((Curve)editingTrack).radius = ((Vector2)i).x));
                                    }
                                }
                            }
                            else
                            {
                                Vector3 v = Quaternion.Inverse(editingTrack.rot) * (p - editingTrack.pos);
                                if (v.z < 0)
                                    editingTrack.rot = Quaternion.Euler(0, editingTrack.rot.eulerAngles.y - 180, 0);
                                editingTrack.length = Mathf.Abs(v.z);
                            }
                            editingTrack.reloadEntity();
                            GameCanvas.trackSettingPanel.load();
                            GameCanvas.trackSettingPanel.transform.position = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width - ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.width), Mathf.Clamp(Input.mousePosition.y, ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.height, Screen.height));
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (editingTrack != null)
                            {
                                trackEdited0();

                                if (editingTrack is Curve)
                                    editingRot = ((Curve)editingTrack).getRotation(1);
                                else
                                    editingRot = editingTrack.rot;

                                Track newTrack;

                                if (editingTrack.GetType() == typeof(Track))
                                    newTrack = new Curve(playingmap, editingTrack.getPoint(1));
                                else
                                    newTrack = new Track(playingmap, editingTrack.getPoint(1));

                                editingTrack = newTrack;
                            }
                            else if (selection != null)
                            {
                                if ((mainTrack = selection) is Curve)
                                {
                                    editingRot = ((Curve)mainTrack).getRotation(selectionDist / mainTrack.length);
                                    editingTrack = new Track(playingmap, p);
                                }
                                else
                                {
                                    editingRot = mainTrack.rot;
                                    editingTrack = new Curve(playingmap, p);
                                }
                            }
                            else
                                editingTrack = new Track(playingmap, p);

                            if (editingRot != null)
                                editingTrack.rot = (Quaternion)editingRot;
                            editingTrack.enableCollider = false;
                            editingTrack.generate();

                            GameCanvas.trackSettingPanel.show(true);
                            GameCanvas.trackSettingPanel.transform.position = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width - ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.width), Mathf.Clamp(Input.mousePosition.y, ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.height, Screen.height));
                        }
                    }
                    else if (mode == MODE_REMOVE_TRACK)
                    {
                        point.SetActive(false);
                        if (selection != null && Input.GetMouseButtonUp(0))
                        {
                            entity.Destroy();
                            playingmap.removeTrack((Track)entity.obj);
                        }
                    }
                    else
                        point.SetActive(false);
                }
                else
                    point.SetActive(false);
            }
            else
                point.SetActive(false);

            if (bgmSource.isPlaying)
                bgmSource.Stop();
        }
        else
        {
            point.SetActive(false);
            if (!bgmSource.isPlaying)
            {
                bgmSource.clip = titleClips[UnityEngine.Random.Range(0, titleClips.Length)];
                bgmSource.Play();
            }
        }
    }

    public static void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBPLAYER
		Application.Quit ();
#endif
    }

    public static void screenShot()
    {
        Directory.CreateDirectory(ssdir);
        string fileName = DateTime.Now.Ticks + ".png";
        ScreenCapture.CaptureScreenshot(Path.Combine(ssdir, fileName));
        print(DateTime.Now + " ScreenShot: " + fileName);
    }

    public static void openSSDir()
    {
        Directory.CreateDirectory(ssdir);
        Process.Start(ssdir);
    }

    public static void reflectSettings()
    {
        main.bgmSource.volume = bgmVolume;
        main.seSource.volume = seVolume;
        main.mainCamera.GetComponent<PostProcessingBehaviour>().profile.antialiasing.enabled = antialiasing;
        main.mainCamera.GetComponent<PostProcessingBehaviour>().profile.ambientOcclusion.enabled = ao;
        main.mainCamera.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled = motionBlur;
        main.mainCamera.GetComponent<PostProcessingBehaviour>().profile.bloom.enabled = bloom;
        main.mainCamera.GetComponent<PostProcessingBehaviour>().profile.vignette.enabled = vignette;
    }

    public static void saveSettings()
    {
        PlayerPrefs.SetInt(KEY_DRAW_DISTANCE, drawDistance);
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, bgmVolume);
        PlayerPrefs.SetFloat(KEY_SE_VOLUME, seVolume);
        PlayerPrefs.SetFloat(KEY_CAMERA_MOVE_SPEED, cameraMoveSpeed);
        PlayerPrefs.SetFloat(KEY_DRAG_ROT_SPEED, dragRotSpeed);
        PlayerPrefs.SetInt(KEY_ANTIALIASING, antialiasing ? 1 : 0);
        PlayerPrefs.SetInt(KEY_AO, ao ? 1 : 0);
        PlayerPrefs.SetInt(KEY_MOTIONBLUR, motionBlur ? 1 : 0);
        PlayerPrefs.SetInt(KEY_BLOOM, bloom ? 1 : 0);
        PlayerPrefs.SetInt(KEY_VIGNETTE, vignette ? 1 : 0);
    }

    public static IEnumerator openMap(string mapname)
    {
        if (playingmap != null)
        {
            closeMap();
        }
        GameCanvas.titlePanel.show(false);
        GameCanvas.loadingMapPanel.show(true);

        yield return null; //読み込み画面を表示する
        Map map = MapManager.loadMap(mapname);
        if (map == null)
        {
            //マップが対応していない
            GameCanvas.loadingMapPanel.show(false);
            GameCanvas.titlePanel.show(true);
            GameCanvas.selectMapPanel.setOpenMap();
            GameCanvas.selectMapPanel.show(true);
            GameCanvas.unsupportedMapPanel.show(true);
        }
        else
        {
            playingmap = map;
            pause = false;
            lasttick = 0;
            lasttick_few = 0;
            mode = 0;
            playingmap.generate();
            main.reloadLighting();

            main.mainCamera.transform.position = map.cameraPos;
            main.mainCamera.transform.eulerAngles = map.cameraRot;
            main.mainCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
            GameCanvas.loadingMapPanel.show(false);
            GameCanvas.playingPanel.show(true);

            print(DateTime.Now + " マップを開きました: " + map.mapname);
        }
    }

    public static void closeMap()
    {
        if (playingmap != null)
        {
            playingmap.DestroyAll();
            playingmap = null;
            main.mainCamera.GetComponent<PostProcessingBehaviour>().enabled = false;
        }
    }

    //描画を優先して負荷のかかる処理を行うため、描画状態に応じてyield returnを行う条件を返すメソッド
    public static bool yrCondition()
    {
        return 1 / Time.deltaTime <= Main.min_fps;
    }

    public static void setPause(bool pause)
    {
        Main.pause = pause;
        GameCanvas.playingPanel.show(!pause);
        GameCanvas.pausePanel.show(pause);
    }

    public void reloadLighting()
    {
        float t = Mathf.Repeat(playingmap.time, 86400000f); //86400000ms = 1日
        float r = t * 360f / 86400000f - 90f;
        sun.transform.localEulerAngles = new Vector3(r, -90f, 0f);

        //頻繁に変更すると重くなる
        sun.shadowStrength = sun.intensity = sunGradient.Evaluate(1f - Mathf.Abs((r + 90f) / 180f - 1)).grayscale;
    }

    //2直線の交点を求める関数
    public Vector2? GetIntersectionPointCoordinates(float a1x, float a1y, float a2x, float a2y, float b1x, float b1y, float b2x, float b2y)
    {
        float tmp = (b2x - b1x) * (a2y - a1y) - (b2y - b1y) * (a2x - a1x);
        if (tmp == 0)
            return null;

        float mu = ((a1x - b1x) * (a2y - a1y) - (a1y - b1y) * (a2x - a1x)) / tmp;
        return new Vector2(b1x + (b2x - b1x) * mu, b1y + (b2y - b1y) * mu);
    }

    public Vector2? GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
    {
        return GetIntersectionPointCoordinates(A1.x, A1.y, A2.x, A2.y, B1.x, B1.y, B2.x, B2.y);
    }

    public Vector2? GetIntersectionPointCoordinatesXZ(Vector3 A1, Vector3 A2, Vector3 B1, Vector3 B2)
    {
        return GetIntersectionPointCoordinates(A1.x, A1.z, A2.x, A2.z, B1.x, B1.z, B2.x, B2.z);
    }

    public static void trackEdited0()
    {
        if (editingTrack is Curve && ((Curve)editingTrack).isLinear())
            print("!"); //TODO 作成する曲線が直線である場合、直線が作成されるようにする

        if (mainTrack != null)
        {
            if (mainTrack.pos == editingTrack.pos && mainTrack.rot == editingTrack.rot
            || mainTrack.pos == editingTrack.getPoint(1) && mainTrack.rot == (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot))
            {
                mainTrack.prevTracks.Add(editingTrack);
            }
            else if (mainTrack.getPoint(1) == editingTrack.pos && (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot) == editingTrack.rot
            || mainTrack.getPoint(1) == editingTrack.getPoint(1) && (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot) == (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot))
            {
                mainTrack.nextTracks.Add(editingTrack);
            }
            else
                print("!"); //TODO

            if (editingTrack.pos == mainTrack.pos && editingTrack.rot == mainTrack.rot
            || editingTrack.pos == mainTrack.getPoint(1) && editingTrack.rot == (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot))
            {
                editingTrack.prevTracks.Add(mainTrack);
            }
            else if (editingTrack.getPoint(1) == mainTrack.pos && (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot) == mainTrack.rot
            || editingTrack.getPoint(1) == mainTrack.getPoint(1) && (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot) == (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot))
            {
                editingTrack.nextTracks.Add(mainTrack);
            }
            else
                print("!"); //TODO
        }
        mainTrack = editingTrack;

        playingmap.addTrack(editingTrack);
        editingTrack.enableCollider = true;
        editingTrack.reloadCollider();
    }
}
