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

/// <summary>
/// ゲームを動作するクラス
/// </summary>
public class Main : MonoBehaviour
{

    public const string VERSION = "0.001alpha";
    public const float ALLOWABLE_RANGE = 0.0001f;
    public const float SNAP_DIST = 1f;

    public static Main INSTANCE;
    public static Map playingmap { get; private set; }
    public static string ssdir;
    public static int min_fps = 15;


    // Game settings
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


    public const int MODE_NONE = 0;
    public const int MODE_CONSTRUCT_TRACK = 11;
    public const int MODE_PLACE_AXLE = 21;
    public const int MODE_PLACE_MAPPIN = 31;
    public const int MODE_PLACE_STRUCTURE = 41;
    public static int mode = MODE_NONE; // 操作モード 0=なし 11=軌道敷設 21=車軸設置 31=マップピンを置く 41=ストラクチャーを設置

    private float tick = 0f; // 時間を進ませた時の余り
    public static bool showGuide = true;
    public static List<Shape> editingTracks = new List<Shape>();
    public static Quaternion? editingRot;
    public static Coupler editingCoupler;
    public static MapPin editingMapPin;
    public static Structure editingStructure;
    public static Shape mainTrack;
    public static MapObject focused;
    public static float focusedDist;
    public static List<MapObject> selectingObjs = new List<MapObject>();
    public static float gauge = Track.defaultGauge;


    // settings by Inspector
    public Gradient sunGradient;
    public Light sun; // 太陽
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
    public GameObject railLModel;
    public GameObject railRModel;
    public GameObject tieModel;
    public GameObject axleModel;
    public GameObject bogieFrameModel;
    public GameObject bodyModel;
    public GameObject couplerModel;
    public GameObject permanentCouplerModel;
    public GameObject directControllerModel;
    public Font mapPinFont;

    public Canvas canvas;
    public TitlePanel titlePanel;
    public SelectMapPanel selectMapPanel;
    public SettingPanel settingPanel;
    public AddMapPanel addMapPanel;
    public GamePanel loadingMapPanel;
    public PlayingPanel playingPanel;
    public GameObject timePanel;
    public Text timeText;
    public PausePanel pausePanel;
    public TitleBackPanel titleBackPanel;
    public UnsupportedMapPanel unsupportedMapPanel;
    public DeleteMapPanel deleteMapPanel;
    public ShapeSettingPanel shapeSettingPanel;
    public CouplerSettingPanel couplerSettingPanel;
    public RunPanel runPanel;
    public MapPinSettingPanel mapPinSettingPanel;
    public StructureSettingPanel structureSettingPanel;

    void Awake()
    {
        INSTANCE = this;
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
        reflectSettings();
        saveSettings();
    }

    void Start()
    {
        titlePanel.show(true);
    }

    void Update()
    {
#if UNITY_EDITOR
        INSTANCE = this;
#endif
        // カメラを除く操作
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Directory.CreateDirectory(ssdir);
            ScreenCapture.CaptureScreenshot(Path.Combine(ssdir, DateTime.Now.Ticks + ".png"));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playingmap != null &&
                !settingPanel.isShowing() &&
                !titleBackPanel.isShowing() &&
                !runPanel.isShowing())
            {
                var a = true;
                if (shapeSettingPanel.isShowing())
                {
                    a = false;
                    cancelEditingTracks();
                }
                else if (couplerSettingPanel.isShowing())
                {
                    a = false;
                    couplerSettingPanel.show(false);
                    if (editingCoupler != null)
                    {
                        editingCoupler.entity.Destroy();
                        editingCoupler = null;
                    }
                }
                else if (mapPinSettingPanel.isShowing())
                {
                    a = false;
                    mapPinSettingPanel.show(false);
                    if (editingMapPin != null)
                    {
                        editingMapPin.entity.Destroy();
                        editingMapPin = null;
                    }
                }
                else if (structureSettingPanel.isShowing())
                {
                    a = false;
                    structureSettingPanel.show(false);
                    if (editingStructure != null)
                    {
                        editingStructure.entity.Destroy();
                        editingStructure = null;
                    }
                }

                if (mode != MODE_NONE)
                {
                    a = false;
                    mode = MODE_NONE;
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

                    playingPanel.a();
                }

                if (a)
                    setPause(Time.timeScale != 0f);
            }
        }

        if (playingmap != null)
        {
            if (Time.timeScale == 0f)
            {
                playingmap.cameraPos = INSTANCE.mainCamera.transform.position;
                playingmap.cameraRot = INSTANCE.mainCamera.transform.eulerAngles;
            }
            else
            {
                playingmap.Update();
                tick += Time.deltaTime;
                var t = Mathf.FloorToInt(tick);
                if (t != 0)
                {
                    reloadLighting();
                    tick -= t;
                }
                timeText.text = playingmap.getHours() + ":" + string.Format("{0:00}", playingmap.getMinutes()) + ":" + string.Format("{0:00}", playingmap.getSeconds());

                var p = INSTANCE.mainCamera.transform.position;
                grid.transform.position = new Vector3(Mathf.RoundToInt(p.x), 0, Mathf.RoundToInt(p.z));
            }

            if (!pausePanel.isShowing() &&
                !runPanel.isShowing() &&
                !EventSystem.current.IsPointerOverGameObject() &&
                !(shapeSettingPanel.isShowing() &&
                EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                    removeSelectingObjs();

                if (Input.GetKeyDown(KeyCode.G))
                {
                    showGuide = !showGuide;
                    playingPanel.b();
                }

                if (!pausePanel.isShowing() &&
                    !CameraMover.INSTANCE.dragging &&
                    !EventSystem.current.IsPointerOverGameObject() &&
                    !(shapeSettingPanel.isShowing() &&
                    EventSystem.current.currentSelectedGameObject != null &&
                    EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
                    update_ctrl();
                else
                {
                    point.SetActive(false);

                    var a = focused;
                    focused = null;
                    if (a != null)
                    {
                        if (selectingObjs.Contains(a))
                            a.useSelectingMat = true;
                        a.reloadEntity();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update上で行われる、カメラ以外の操作
    /// </summary>
    private void update_ctrl()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            var entity = hit.collider.GetComponent<MapEntity>();
            if (entity == null && hit.collider.transform.parent)
                entity = hit.collider.transform.parent.GetComponent<MapEntity>();
            if (entity != null && (editingTracks.Any() ? editingTracks.Any(track => track.entity && entity.gameObject != track.entity.gameObject) : true))
            {
                if (focused != entity.obj)
                {
                    var a = focused;
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
                var a = focused;
                focused = null;
                if (a != null)
                {
                    if (selectingObjs.Contains(a))
                        a.useSelectingMat = true;
                    a.reloadEntity();
                }
            }

            if (mode == MODE_NONE)
            {
                point.SetActive(false);

                if (Input.GetMouseButtonUp(0))
                    selectObj(focused);
            }
            else
            {
                var p = hit.point;
                p.y = 0f;

                if (focused != null)
                {
                    if (focused is Track)
                    {
                        focusedDist = ((Track)focused).getLength(hit.point);
                        if (focusedDist < SNAP_DIST)
                            focusedDist = 0f;
                        else if (focusedDist > ((Track)focused).length - SNAP_DIST)
                            focusedDist = ((Track)focused).length;
                        p = ((Track)focused).getPoint(focusedDist / ((Track)focused).length);
                    }
                }

                point.transform.position = p;
                point.SetActive(true);

                if (mode == MODE_CONSTRUCT_TRACK)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (editingTracks.Any())
                        {
                            if (editingTracks[0].curveLength.Count == 0)
                                cancelEditingTracks();
                            else
                            {
                                trackEdited0();

                                var l = editingTracks.Last();
                                editingRot = l.getRotation(1);

                                editingTracks.Clear();
                                editingTracks.Add(new Shape(playingmap, l.getPoint(1f)));
                            }
                        }
                        else if (focused != null)
                        {
                            if (focused is Shape)
                            {
                                mainTrack = ((Shape)focused);
                                editingRot = mainTrack.getRotation(focusedDist / mainTrack.length);
                                editingTracks.Clear();
                                editingTracks.Add(new Shape(playingmap, p));
                            }
                        }
                        else
                        {
                            mainTrack = null;
                            editingTracks.Clear();
                            editingTracks.Add(new Shape(playingmap, p));
                        }

                        if (editingTracks.Any())
                        {
                            editingTracks[0].gauge = gauge;

                            if (editingRot != null)
                                editingTracks[0].rot = (Quaternion)editingRot;
                            editingTracks[0].enableCollider = false;
                            editingTracks[0].generate();

                            shapeSettingPanel.show(true);
                            setPanelPosToMousePos(shapeSettingPanel);
                        }
                    }
                    else if (Input.GetMouseButtonUp(1))
                        cancelEditingTracks();
                }
                else if (mode == MODE_PLACE_AXLE)
                {
                    if (Input.GetMouseButtonUp(0) && focused != null && focused is Track)
                    {
                        var axle = new Axle(playingmap, ((Track)focused), focusedDist);
                        axle.generate();
                        playingmap.addObject(axle);
                    }
                }
                else if (mode == MODE_PLACE_MAPPIN)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        editingMapPin = new MapPin(playingmap, p);
                        editingMapPin.generate();
                        mapPinSettingPanel.show(true);
                        setPanelPosToMousePos(mapPinSettingPanel);
                    }
                }
                else if (mode == MODE_PLACE_STRUCTURE)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        editingStructure = new Structure(playingmap, p);
                        editingStructure.generate();
                        structureSettingPanel.show(true);
                        setPanelPosToMousePos(structureSettingPanel);
                    }
                }
            }
        }
        else
        {
            point.SetActive(false);

            var a = focused;
            focused = null;
            if (a != null)
            {
                if (selectingObjs.Contains(a))
                    a.useSelectingMat = true;
                a.reloadEntity();
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
    }

    public IEnumerator openMap(string mapname)
    {
        if (playingmap != null)
            closeMap();

        titlePanel.show(false);
        loadingMapPanel.show(true);

        yield return null; // 読み込み画面を表示する
        playingmap = MapManager.loadMap(mapname);
        if (playingmap == null)
        {
            // マップが対応していない
            loadingMapPanel.show(false);
            titlePanel.show(true);
            selectMapPanel.setOpenMap();
            selectMapPanel.show(true);
            unsupportedMapPanel.show(true);
        }
        else
        {
            Time.timeScale = 1f;
            tick = 0f;
            mode = MODE_NONE;
            playingmap.generate();
            INSTANCE.reloadLighting();

            mainCamera.transform.position = playingmap.cameraPos;
            mainCamera.transform.eulerAngles = playingmap.cameraRot;
            mainCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
            bgmSource.Stop();
            loadingMapPanel.show(false);
            timePanel.SetActive(true);
            playingPanel.show(true);
        }
    }

    public void closeMap()
    {
        point.SetActive(false);
        pausePanel.show(false);
        timePanel.SetActive(false);

        if (playingmap != null)
        {
            playingmap.DestroyAll();
            playingmap = null;
            mainCamera.GetComponent<PostProcessingBehaviour>().enabled = false;
        }

        titlePanel.show(true);
        reloadBGM();
    }

    /// <summary>
    /// 描画を優先して負荷のかかる処理を行うため、描画状態に応じてyield returnを行う条件を返すメソッド
    /// </summary>
    public static bool yrCondition()
    {
        return 1f / Time.deltaTime <= min_fps;
    }

    public static void setPanelPosToMousePos(GamePanel panel)
    {
        panel.transform.position = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width - panel.getWidth()),
            Mathf.Clamp(Input.mousePosition.y, panel.getHeight(), Screen.height));
    }

    public void setPause(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
        playingPanel.show(!pause);
        pausePanel.show(pause);
    }

    public void reloadLighting()
    {
        var t = playingmap.time / Map.TIME_OF_DAY;
        sun.transform.eulerAngles = new Vector3(t * 360f - 90f, -90f);
        sun.shadowStrength = sun.intensity = sunGradient.Evaluate(t).grayscale;
        RenderSettings.ambientLight *= (1f - Mathf.Abs(t * 2f - 1f)) / RenderSettings.ambientLight.grayscale;
    }

    public void reloadBGM()
    {
        if (!bgmSource.isPlaying)
        {
            bgmSource.clip = titleClips[UnityEngine.Random.Range(0, titleClips.Length)];
            bgmSource.Play();
        }
    }

    public void reloadGrid()
    {
        grid.SetActive(!runPanel.isShowing() && showGuide);
        var objs = Main.playingmap.objs.Where(obj => obj is Track).OfType<Track>().ToList();
        foreach (var obj in objs)
        {
            obj.reloadTrackRenderer();
            obj.reloadRailRenderers();
        }
    }

    /// <summary>
    /// 2直線の交点を求める
    /// </summary>
    public Vector2? GetIntersectionPointCoordinates(float a1x, float a1y, float a2x, float a2y, float b1x, float b1y,
        float b2x, float b2y)
    {
        var tmp = (b2x - b1x) * (a2y - a1y) - (b2y - b1y) * (a2x - a1x);
        if (tmp == 0f)
            return null;

        var mu = ((a1x - b1x) * (a2y - a1y) - (a1y - b1y) * (a2x - a1x)) / tmp;
        return new Vector2(b1x + (b2x - b1x) * mu, b1y + (b2y - b1y) * mu);
    }

    /// <summary>
    /// 2直線の交点を求める
    /// </summary>
    public Vector2? GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
    {
        return GetIntersectionPointCoordinates(A1.x, A1.y, A2.x, A2.y, B1.x, B1.y, B2.x, B2.y);
    }

    /// <summary>
    /// 2直線の交点を求める
    /// </summary>
    public Vector2? GetIntersectionPointCoordinatesXZ(Vector3 A1, Vector3 A2, Vector3 B1, Vector3 B2)
    {
        return GetIntersectionPointCoordinates(A1.x, A1.z, A2.x, A2.z, B1.x, B1.z, B2.x, B2.z);
    }

    public void trackEdited0()
    {
        foreach (var track in editingTracks)
        {
            if (mainTrack != null)
            {
                if ((mainTrack.pos - track.pos).sqrMagnitude < ALLOWABLE_RANGE && (mainTrack.rot.eulerAngles - track.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE ||
                (mainTrack.pos - track.getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE && (mainTrack.rot.eulerAngles - track.getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE)
                {
                    mainTrack.prevTracks.Add(track);
                    if (mainTrack.prevTracks.Count == 1)
                        mainTrack.connectingPrevTrack = 0;
                }
                else if ((mainTrack.getPoint(1f) - track.pos).sqrMagnitude < ALLOWABLE_RANGE && (mainTrack.getRotation(1f).eulerAngles - track.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE ||
                (mainTrack.getPoint(1f) - track.getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE && (mainTrack.getRotation(1f).eulerAngles - track.getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE)
                {
                    mainTrack.nextTracks.Add(track);
                    if (mainTrack.nextTracks.Count == 1)
                        mainTrack.connectingNextTrack = 0;
                }

                if ((track.pos - mainTrack.pos).sqrMagnitude < ALLOWABLE_RANGE && (track.rot.eulerAngles - mainTrack.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE ||
                (track.pos - mainTrack.getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE && (track.rot.eulerAngles - mainTrack.getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE)
                {
                    track.prevTracks.Add(mainTrack);
                    if (track.prevTracks.Count == 1)
                        track.connectingPrevTrack = 0;
                }
                else if ((track.getPoint(1f) - mainTrack.pos).sqrMagnitude < ALLOWABLE_RANGE && (track.getRotation(1f).eulerAngles - mainTrack.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE ||
                (track.getPoint(1) - mainTrack.getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE && (track.getRotation(1f).eulerAngles - mainTrack.getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE)
                {
                    track.nextTracks.Add(mainTrack);
                    if (track.nextTracks.Count == 1)
                        track.connectingNextTrack = 0;
                }
            }

            if (focused is Track)
            {
                var a = (track.pos - ((Track)focused).pos).sqrMagnitude < ALLOWABLE_RANGE;
                var a1 = (track.pos - ((Track)focused).getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE;
                var a2 = (track.getPoint(1f) - ((Track)focused).pos).sqrMagnitude < ALLOWABLE_RANGE;
                var a3 = (track.getPoint(1f) - ((Track)focused).getPoint(1f)).sqrMagnitude < ALLOWABLE_RANGE;
                var b = (track.rot.eulerAngles - focused.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
                var b1 = (track.rot.eulerAngles - (focused is Curve ? ((Curve)focused).getRotation(1f).eulerAngles : focused.rot.eulerAngles)).sqrMagnitude < ALLOWABLE_RANGE;
                var b2 = (track.getRotation(1f).eulerAngles - focused.rot.eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
                var b3 = (track.getRotation(1f).eulerAngles - ((Shape)focused).getRotation(1f).eulerAngles).sqrMagnitude < ALLOWABLE_RANGE;
                if (a && b || a1 && b1)
                {
                    track.prevTracks.Add((Track)focused);
                    if (track.prevTracks.Count == 1)
                        track.connectingPrevTrack = 0;
                }
                else if (a2 && b2 || a3 && b3)
                {
                    track.nextTracks.Add((Track)focused);
                    if (track.nextTracks.Count == 1)
                        track.connectingNextTrack = 0;
                }

                if (a && b || a2 && b2)
                {
                    ((Track)focused).prevTracks.Add(track);
                    if (((Track)focused).prevTracks.Count == 1)
                        ((Track)focused).connectingPrevTrack = 0;
                }
                else if (a1 && b1 || a3 && b3)
                {
                    ((Track)focused).nextTracks.Add(track);
                    if (((Track)focused).nextTracks.Count == 1)
                        ((Track)focused).connectingNextTrack = 0;
                }
            }

            playingmap.addObject(track);
            track.enableCollider = true;
            track.reloadCollider();

            mainTrack = track;
        }

        mainTrack = editingTracks.Last();
    }

    public void selectObj(MapObject obj)
    {
        var s = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (!s)
        {
            foreach (var o in selectingObjs)
            {
                o.useSelectingMat = false;
                o.reloadEntity();
            }

            selectingObjs.Clear();
        }

        if (obj != null)
        {
            if (s && selectingObjs.Contains(obj))
            {
                selectingObjs.Remove(obj);
                obj.useSelectingMat = false;
                obj.reloadEntity();
            }
            else if (!selectingObjs.Contains(obj))
            {
                selectingObjs.Add(obj);
                obj.useSelectingMat = true;
                obj.reloadEntity();
            }
        }

        playingPanel.a();
    }

    public void removeSelectingObjs()
    {
        foreach (var obj in selectingObjs)
        {
            if (obj.entity)
                obj.entity.Destroy();
            playingmap.removeObject(obj);
        }

        selectingObjs.Clear();
        playingPanel.a();
    }

    public void cancelEditingTracks()
    {
        shapeSettingPanel.show(false);

        foreach (var track in editingTracks)
            track.entity.Destroy();
        editingTracks.Clear();

        editingRot = null;
    }
}
