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

    /// <summary>
    /// スナップする距離
    /// </summary>
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


    /// <summary>
    /// 操作モード
    /// </summary>
    public enum ModeEnum
    {
        /// <summary>
        /// なし
        /// </summary>
        NONE,
        /// <summary>
        /// 軌道の敷設
        /// </summary>
        CONSTRUCT_TRACK,
        /// <summary>
        /// 車軸の設置
        /// </summary>
        PLACE_AXLE,
        /// <summary>
        /// マップピンの設置
        /// </summary>
        PLACE_MAPPIN,
        /// <summary>
        /// ストラクチャーの設置
        /// </summary>
        PLACE_STRUCTURE
    }
    /// <summary>
    /// 現在の操作モード
    /// </summary>
    public static ModeEnum mode = ModeEnum.NONE;

    private float tick = 0f; // 時間を進ませた時の余り
    public static Shape editingTrack;
    public static Quaternion? editingRot;
    public static Coupler editingCoupler;
    public static MapPin editingMapPin;
    public static Structure editingStructure;
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
    public Material trackMat;
    public Material focusedMat;
    public Material selectingMat;
    public Material railMat;
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
                var b = EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null;
                if (b)
                    EventSystem.current.SetSelectedGameObject(null);
                else
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

                    if (mode != ModeEnum.NONE)
                    {
                        a = false;
                        mode = ModeEnum.NONE;
                    }

                    if (selectingObjs.Count != 0)
                    {
                        a = false;
                        foreach (var o in selectingObjs)
                        {
                            o.useSelectingMat = false;
                            o.reloadMaterial();
                        }

                        selectingObjs.Clear();

                        playingPanel.a();
                    }

                    if (a)
                        setPause(Time.timeScale != 0f);
                }
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

            if (!pausePanel.isShowing() && !runPanel.isShowing() &&
                !(EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null))
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                    removeSelectingObjs();
                if (Input.GetKeyDown(KeyCode.G))
                    playingPanel.setGuide(!playingPanel.guideToggle.isOn);
                if (Input.GetKeyDown(KeyCode.R) && mode == ModeEnum.CONSTRUCT_TRACK && editingRot != null && editingTrack != null)
                {
                    editingRot *= Quaternion.AngleAxis(180f, Vector3.up);
                    editingTrack.rot = (Quaternion)editingRot;
                }

                if (!CameraMover.INSTANCE.dragging && !EventSystem.current.IsPointerOverGameObject())
                    update_ctrl();
                else
                {
                    point.SetActive(false);

                    var a = focused;
                    focused = null;
                    if (a != null && a.entity)
                    {
                        if (selectingObjs.Contains(a))
                            a.useSelectingMat = true;
                        a.reloadMaterial();
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
            if (entity != null && (editingTrack == null ? true : editingTrack.entity && entity.gameObject != editingTrack.entity.gameObject))
            {
                if (focused != entity.obj)
                {
                    var a = focused;
                    (focused = entity.obj).useSelectingMat = false;
                    if (a != null && a.entity)
                    {
                        if (selectingObjs.Contains(a))
                            a.useSelectingMat = true;
                        a.reloadMaterial();
                    }

                    focused.reloadMaterial();
                }
            }
            else
            {
                var a = focused;
                focused = null;
                if (a != null && a.entity)
                {
                    if (selectingObjs.Contains(a))
                        a.useSelectingMat = true;
                    a.reloadMaterial();
                }
            }

            if (mode == ModeEnum.NONE)
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
                        p = ((Track)focused).getPoint(focusedDist = focusedDist < SNAP_DIST &&
                            focusedDist < ((Track)focused).length / 2f ? 0f :
                            focusedDist > ((Track)focused).length - SNAP_DIST &&
                            focusedDist > ((Track)focused).length - ((Track)focused).length / 2f ? 1f :
                            focusedDist / ((Track)focused).length);
                    }
                    else
                        p = focused.pos;
                }

                point.transform.position = p;
                point.SetActive(true);

                if (mode == ModeEnum.CONSTRUCT_TRACK)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (editingTrack != null)
                            cancelEditingTracks();
                        else if (focused != null)
                        {
                            if (focused is Shape)
                            {
                                editingRot = ((Shape)focused).getRotation(focusedDist);
                                editingTrack = new Shape(playingmap, p);
                            }
                        }
                        else
                        {
                            editingTrack = new Shape(playingmap, p);
                        }

                        if (editingTrack != null)
                        {
                            editingTrack.gauge = gauge;

                            if (editingRot != null)
                                editingTrack.rot = (Quaternion)editingRot;
                            editingTrack.enableCollider = false;
                            editingTrack.generate();

                            shapeSettingPanel.show(true);
                            setPanelPosToMousePos(shapeSettingPanel);
                        }
                    }
                    else if (Input.GetMouseButtonUp(1))
                        cancelEditingTracks();
                }
                else if (mode == ModeEnum.PLACE_AXLE)
                {
                    if (Input.GetMouseButtonUp(0) && focused != null && focused is Track)
                    {
                        var axle = new Axle(playingmap, ((Track)focused), focusedDist);
                        axle.generate();
                        playingmap.addObject(axle);
                    }
                }
                else if (mode == ModeEnum.PLACE_MAPPIN)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        editingMapPin = new MapPin(playingmap, p);
                        editingMapPin.generate();
                        mapPinSettingPanel.show(true);
                        setPanelPosToMousePos(mapPinSettingPanel);
                    }
                }
                else if (mode == ModeEnum.PLACE_STRUCTURE)
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
            if (a != null && a.entity)
            {
                if (selectingObjs.Contains(a))
                    a.useSelectingMat = true;
                a.reloadMaterial();
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
            mode = ModeEnum.NONE;
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
        grid.SetActive(!runPanel.isShowing() && playingPanel.guideToggle.isOn);
        foreach (var obj in Main.playingmap.objs)
            obj.reloadMaterial();
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
        foreach (var obj in playingmap.objs)
            if (obj is Track)
                editingTrack.connectingTrack((Track)obj);

        playingmap.addObject(editingTrack);
        editingTrack.enableCollider = true;
        editingTrack.reloadCollider();

        editingTrack = null;
        editingRot = null;
    }

    public void selectObj(MapObject obj)
    {
        var s = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (!s)
        {
            foreach (var o in selectingObjs)
            {
                o.useSelectingMat = false;
                o.reloadMaterial();
            }

            selectingObjs.Clear();
        }

        if (obj != null)
        {
            if (s && selectingObjs.Contains(obj))
            {
                selectingObjs.Remove(obj);
                obj.useSelectingMat = false;
                obj.reloadMaterial();
            }
            else if (!selectingObjs.Contains(obj))
            {
                selectingObjs.Add(obj);
                obj.useSelectingMat = true;
                obj.reloadMaterial();
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

        editingTrack.entity.Destroy();
        editingTrack = null;
        editingRot = null;
    }
}
