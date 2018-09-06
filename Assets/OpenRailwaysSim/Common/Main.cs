using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

//ゲームの動作を制御する中心的クラス
public class Main : MonoBehaviour
{
    public const string VERSION = "0.001alpha";
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
    public const string KEY_GAUGE = "GAUGE";

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
    string ssdir;
    static int min_fps = 15;

    public int drawDistance = DEFAULT_DRAW_DISTANCE;
    public float bgmVolume = DEFAULT_BGM_VOLUME;
    public float seVolume = DEFAULT_SE_VOLUME;
    public float cameraMoveSpeed = DEFAULT_CAMERA_MOVE_SPEED;
    public float dragRotSpeed = DEFAULT_DRAG_ROT_SPEED;
    public bool antialiasing = DEFAULT_ANTIALIASING;
    public bool ao = DEFAULT_AO;
    public bool motionBlur = DEFAULT_MOTIONBLUR;
    public bool bloom = DEFAULT_BLOOM;
    public bool vignette = DEFAULT_VIGNETTE;

    public bool pause { get; private set; } //ポーズ

    private float lasttick = 0; //時間を進ませた時の余り
    private float lasttick_few = 0; //頻繁に変更しないするための計算。この機能は一秒ごとに処理を行う。
    public int mode = 0; //操作モード 0=なし 11=軌道敷設 21=車軸設置
    public static int MODE_CONSTRUCT_TRACK = 11;
    public static int MODE_PLACE_AXLE = 21;

    public static Track editingTrack;
    public static Quaternion? editingRot;
    public static Coupler editingCoupler;
    public static Track mainTrack;
    public static MapObject focused;
    public static float focusedDist;
    public static List<MapObject> selectingObjs = new List<MapObject>();

    public Gradient sunGradient;
    public Light sun; //太陽
    public Camera mainCamera;
    public AudioClip[] titleClips;
    public AudioSource bgmSource;
    public AudioSource seSource;
    public Material track_mat;
    public Material focused_track_mat;
    public Material selecting_track_mat;
    public Material rail_mat;
    public GameObject point;
    public GameObject grid;
    public float gauge = 1.435f;
    public GameObject railModel;
    public GameObject axleModel;
    public GameObject bogieFrameModel;
    public GameObject bodyModel;
    public GameObject couplerModel;
    public GameObject permanentCouplerModel;
    public GameObject directControllerModel;

    void Awake()
    {
        main = this;
        ssdir = Path.Combine(Application.persistentDataPath, "screenshots");

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
        gauge = PlayerPrefs.GetFloat(KEY_GAUGE, gauge);
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
#if UNITY_EDITOR
        main = this;
#endif
        //操作（カメラを除く）
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Directory.CreateDirectory(ssdir);
            ScreenCapture.CaptureScreenshot(Path.Combine(ssdir, DateTime.Now.Ticks + ".png"));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playingmap != null && !GameCanvas.settingPanel.isShowing() && !GameCanvas.titleBackPanel.isShowing() && !GameCanvas.runPanel.isShowing())
            {
                bool a = true;
                if (GameCanvas.trackSettingPanel.isShowing())
                {
                    a = false;
                    GameCanvas.trackSettingPanel.show(false);
                    if (editingTrack != null)
                    {
                        editingTrack.entity.Destroy();
                        editingTrack = null;
                    }

                    editingRot = null;
                }
                else if (GameCanvas.couplerSettingPanel.isShowing())
                {
                    a = false;
                    GameCanvas.couplerSettingPanel.show(false);
                    if (editingCoupler != null)
                    {
                        editingCoupler.entity.Destroy();
                        editingCoupler = null;
                    }
                }

                if (mode != 0)
                {
                    a = false;
                    mode = 0;
                }

                if (selectingObjs.Count != 0)
                {
                    a = false;
                    foreach (var o in selectingObjs)
                    {
                        o.useSelectingMat = false;
                        o.reloadEntity();
                    }

                    selectingObjs.Clear();

                    GameCanvas.playingPanel.a();
                }

                if (a)
                    setPause(!pause);
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete))
            removeSelectingObjs();

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

            if (!GameCanvas.pausePanel.isShowing() && !CameraMover.INSTANCE.dragging &&
                !EventSystem.current.IsPointerOverGameObject() &&
                !(GameCanvas.trackSettingPanel.isShowing() && EventSystem.current.currentSelectedGameObject != null &&
                  EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                RaycastHit hit;
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    MapEntity entity = hit.collider.GetComponent<MapEntity>();
                    if (entity == null && hit.collider.transform.parent)
                        entity = hit.collider.transform.parent.GetComponent<MapEntity>();
                    if (entity != null &&
                        (editingTrack == null
                            ? true
                            : editingTrack.entity != null && entity.gameObject != editingTrack.entity.gameObject))
                    {
                        if (focused != entity.obj)
                        {
                            MapObject a = focused;
                            (focused = entity.obj).useSelectingMat = false;
                            if (a != null)
                            {
                                if (selectingObjs.Contains(a))
                                    a.useSelectingMat = true;
                                a.reloadEntity();
                            }

                            focused.reloadEntity();
                        }
                    }
                    else
                    {
                        MapObject a = focused;
                        focused = null;
                        if (a != null)
                        {
                            if (selectingObjs.Contains(a))
                                a.useSelectingMat = true;
                            a.reloadEntity();
                        }
                    }

                    if (mode == MODE_CONSTRUCT_TRACK)
                    {
                        Vector3 p = hit.point;
                        p.y = 0;

                        if (focused != null)
                        {
                            if (focused is Curve)
                            {
                                Vector3 a = Quaternion.Inverse(focused.rot) * (hit.point - focused.pos);
                                float r1 = ((Curve)focused).radius;
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
                                focusedDist = A * r1;
                                if (focusedDist < Track.MIN_TRACK_LENGTH ||
                                    focusedDist > Mathf.PI * 2 * r1 - Track.MIN_TRACK_LENGTH)
                                    focusedDist = 0;
                                else if (focusedDist > ((Track)focused).length - Track.MIN_TRACK_LENGTH)
                                    focusedDist = ((Track)focused).length;
                                p = ((Track)focused).getPoint(focusedDist / ((Track)focused).length);
                            }
                            else if (focused is Track)
                            {
                                focusedDist = (Quaternion.Inverse(focused.rot) * (hit.point - focused.pos)).z;
                                if (focusedDist < Track.MIN_TRACK_LENGTH)
                                    focusedDist = 0;
                                else if (focusedDist > ((Track)focused).length - Track.MIN_TRACK_LENGTH)
                                    focusedDist = ((Track)focused).length;
                                p = focused.pos + focused.rot * Vector3.forward * focusedDist;
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
                                        Vector2? i = GetIntersectionPointCoordinatesXZ(Vector3.zero,
                                            Vector3.right * v.x, v,
                                            v + Quaternion.Euler(new Vector3(0, a * 2 * Mathf.Rad2Deg)) *
                                            Vector3.right * v.z);
                                        if (i != null)
                                        {
                                            editingTrack.rot = Quaternion.Euler(0, editingTrack.rot.eulerAngles.y - 180,
                                                0);
                                            editingTrack.length =
                                                Mathf.Abs(a * 2 * (((Curve)editingTrack).radius = -((Vector2)i).x));
                                        }
                                    }
                                    else
                                    {
                                        Vector2? i = GetIntersectionPointCoordinatesXZ(Vector3.zero,
                                            Vector3.right * v.x, v,
                                            v + Quaternion.Euler(new Vector3(0, a * 2 * Mathf.Rad2Deg)) *
                                            Vector3.right * v.z);
                                        if (i != null)
                                            editingTrack.length =
                                                Mathf.Abs(a * 2 * (((Curve)editingTrack).radius = ((Vector2)i).x));
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
                            GameCanvas.trackSettingPanel.transform.position = new Vector3(
                                Mathf.Clamp(Input.mousePosition.x, 0,
                                    Screen.width - ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.width),
                                Mathf.Clamp(Input.mousePosition.y,
                                    ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.height,
                                    Screen.height));
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
                            else if (focused != null)
                            {
                                if (focused is Track)
                                {
                                    mainTrack = ((Track)focused);
                                    if (mainTrack is Curve)
                                    {
                                        editingRot = ((Curve)mainTrack).getRotation(focusedDist / mainTrack.length);
                                        editingTrack = new Track(playingmap, p);
                                    }
                                    else
                                    {
                                        editingRot = mainTrack.rot;
                                        editingTrack = new Curve(playingmap, p);
                                    }
                                }
                            }
                            else
                                editingTrack = new Track(playingmap, p);

                            if (editingTrack != null)
                            {
                                editingTrack.rails.Add(-Main.main.gauge / 2);
                                editingTrack.rails.Add(Main.main.gauge / 2);

                                if (editingRot != null)
                                    editingTrack.rot = (Quaternion)editingRot;
                                editingTrack.enableCollider = false;
                                editingTrack.generate();

                                GameCanvas.trackSettingPanel.show(true);
                                GameCanvas.trackSettingPanel.transform.position = new Vector3(
                                    Mathf.Clamp(Input.mousePosition.x, 0,
                                        Screen.width - ((RectTransform)GameCanvas.trackSettingPanel.transform).rect
                                        .width),
                                    Mathf.Clamp(Input.mousePosition.y,
                                        ((RectTransform)GameCanvas.trackSettingPanel.transform).rect.height,
                                        Screen.height));
                            }
                        }
                    }
                    else if (mode == MODE_PLACE_AXLE)
                    {
                        Vector3 p = hit.point;
                        p.y = 0;

                        if (focused != null)
                        {
                            if (focused is Curve)
                            {
                                Vector3 a = Quaternion.Inverse(focused.rot) * (hit.point - focused.pos);
                                float r1 = ((Curve)focused).radius;
                                if (r1 < 0)
                                {
                                    r1 = -r1;
                                    a.x = -a.x;
                                }

                                float r2 = Vector3.Distance(a, Vector3.right * r1);
                                float A = Mathf.Atan(a.z / (r2 - a.x));
                                if (A < 0)
                                    A += Mathf.PI;
                                if (a.z < 0)
                                    A += Mathf.PI;
                                focusedDist = A * r1;
                                if (focusedDist < Track.MIN_TRACK_LENGTH ||
                                    focusedDist > Mathf.PI * 2 * r1 - Track.MIN_TRACK_LENGTH)
                                    focusedDist = 0;
                                else if (focusedDist > ((Track)focused).length - Track.MIN_TRACK_LENGTH)
                                    focusedDist = ((Track)focused).length;
                                p = ((Track)focused).getPoint(focusedDist / ((Track)focused).length);
                            }
                            else if (focused is Track)
                            {
                                focusedDist = (Quaternion.Inverse(focused.rot) * (hit.point - focused.pos)).z;
                                if (focusedDist < Track.MIN_TRACK_LENGTH)
                                    focusedDist = 0;
                                else if (focusedDist > ((Track)focused).length - Track.MIN_TRACK_LENGTH)
                                    focusedDist = ((Track)focused).length;
                                p = focused.pos + focused.rot * Vector3.forward * focusedDist;
                            }
                        }

                        point.transform.position = p;
                        point.SetActive(true);

                        if (Input.GetMouseButtonUp(0) && focused != null && focused is Track)
                        {
                            Axle axle = new Axle(playingmap, ((Track)focused), focusedDist);
                            axle.generate();
                            playingmap.addObject(axle);
                        }
                    }
                    else
                    {
                        point.SetActive(false);

                        if (Input.GetMouseButtonUp(0))
                        {
                            bool s = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                            if (!s)
                            {
                                foreach (var o in selectingObjs)
                                {
                                    o.useSelectingMat = false;
                                    o.reloadEntity();
                                }

                                selectingObjs.Clear();
                            }

                            if (focused != null)
                            {
                                if (s && selectingObjs.Contains(focused))
                                {
                                    selectingObjs.Remove(focused);
                                    focused.useSelectingMat = false;
                                    focused.reloadEntity();
                                }
                                else if (!selectingObjs.Contains(focused))
                                {
                                    selectingObjs.Add(focused);
                                    focused.useSelectingMat = true;
                                    focused.reloadEntity();
                                }
                            }

                            GameCanvas.playingPanel.a();
                        }
                    }
                }
                else
                {
                    point.SetActive(false);

                    MapObject a = focused;
                    focused = null;
                    if (a != null)
                    {
                        if (selectingObjs.Contains(a))
                            a.useSelectingMat = true;
                        a.reloadEntity();
                    }
                }
            }
            else
            {
                point.SetActive(false);

                MapObject a = focused;
                focused = null;
                if (a != null)
                {
                    if (selectingObjs.Contains(a))
                        a.useSelectingMat = true;
                    a.reloadEntity();
                }
            }

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

    public void openSSDir()
    {
        Directory.CreateDirectory(ssdir);
        Process.Start(ssdir);
    }

    public void reflectSettings()
    {
        bgmSource.volume = bgmVolume;
        seSource.volume = seVolume;
        mainCamera.GetComponent<PostProcessingBehaviour>().profile.antialiasing.enabled = antialiasing;
        mainCamera.GetComponent<PostProcessingBehaviour>().profile.ambientOcclusion.enabled = ao;
        mainCamera.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled = motionBlur;
        mainCamera.GetComponent<PostProcessingBehaviour>().profile.bloom.enabled = bloom;
        mainCamera.GetComponent<PostProcessingBehaviour>().profile.vignette.enabled = vignette;
    }

    public void saveSettings()
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
        PlayerPrefs.SetFloat(KEY_GAUGE, gauge);
    }

    public IEnumerator openMap(string mapname)
    {
        if (playingmap != null)
            closeMap();

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
            Time.timeScale = 1;
            lasttick = 0;
            lasttick_few = 0;
            mode = 0;
            playingmap.generate();
            main.reloadLighting();

            mainCamera.transform.position = map.cameraPos;
            mainCamera.transform.eulerAngles = map.cameraRot;
            mainCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
            GameCanvas.loadingMapPanel.show(false);
            GameCanvas.playingPanel.show(true);
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

    public void setPause(bool pause)
    {
        if (this.pause = pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
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
    public Vector2? GetIntersectionPointCoordinates(float a1x, float a1y, float a2x, float a2y, float b1x, float b1y,
        float b2x, float b2y)
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

    public void trackEdited0()
    {
        if (editingTrack is Curve && ((Curve)editingTrack).isLinear())
            print("!"); //TODO 作成する曲線が直線である場合、直線が作成されるようにする

        //TODO mainTrack以外の軌道とも接続するようにする
        if (mainTrack != null)
        {
            if (mainTrack.pos == editingTrack.pos && mainTrack.rot == editingTrack.rot
                || mainTrack.pos == editingTrack.getPoint(1) && mainTrack.rot ==
                (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot))
            {
                mainTrack.prevTracks.Add(editingTrack);
                if (mainTrack.prevTracks.Count == 1)
                    mainTrack.connectingPrevTrack = 0;
            }
            else if (mainTrack.getPoint(1) == editingTrack.pos &&
                     (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot) == editingTrack.rot
                     || mainTrack.getPoint(1) == editingTrack.getPoint(1) &&
                     (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot) == (editingTrack is Curve
                         ? ((Curve)editingTrack).getRotation(1)
                         : editingTrack.rot))
            {
                mainTrack.nextTracks.Add(editingTrack);
                if (mainTrack.nextTracks.Count == 1)
                    mainTrack.connectingNextTrack = 0;
            }
            else
            {
                print(mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot);
                print(editingTrack.rot);
            }

            if (editingTrack.pos == mainTrack.pos && editingTrack.rot == mainTrack.rot
                || editingTrack.pos == mainTrack.getPoint(1) && editingTrack.rot ==
                (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot))
            {
                editingTrack.prevTracks.Add(mainTrack);
                if (mainTrack.prevTracks.Count == 1)
                    mainTrack.connectingPrevTrack = 0;
            }
            else if (editingTrack.getPoint(1) == mainTrack.pos &&
                     (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot) == mainTrack.rot
                     || editingTrack.getPoint(1) == mainTrack.getPoint(1) &&
                     (editingTrack is Curve ? ((Curve)editingTrack).getRotation(1) : editingTrack.rot) ==
                     (mainTrack is Curve ? ((Curve)mainTrack).getRotation(1) : mainTrack.rot))
            {
                editingTrack.nextTracks.Add(mainTrack);
                if (mainTrack.nextTracks.Count == 1)
                    mainTrack.connectingNextTrack = 0;
            }
            else
                print("!"); //TODO
        }

        mainTrack = editingTrack;

        playingmap.addObject(editingTrack);
        editingTrack.enableCollider = true;
        editingTrack.reloadCollider();
    }

    public static void removeSelectingObjs()
    {
        foreach (var obj in selectingObjs)
        {
            if (obj.entity)
                obj.entity.Destroy();
            playingmap.removeObject(obj);
        }

        selectingObjs.Clear();
        GameCanvas.playingPanel.a();
    }
}
