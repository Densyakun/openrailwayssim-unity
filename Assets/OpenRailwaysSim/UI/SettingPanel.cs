using UnityEngine;
using UnityEngine.UI;

//設定画面
public class SettingPanel : GamePanel {
	//TODO 多言語対応化
	public static string drawDistanceText_DEF = "描画距離";
	public static string bgmVolumeText_DEF = "BGM音量";
	public static string seVolumeText_DEF = "SE音量";
	public static string cameraMoveSpeedText_DEF = "カメラ移動速度";
	public static string dragRotSpeedText_DEF = "マウスでのカメラ回転速度";
	public static string antialiasingText_DEF = "アンチエイリアシング";
	public static string aoText_DEF = "アンビエントオクルージョン(AO)";
	public static string motionBlurText_DEF = "モーションブラー";
	public static string bloomText_DEF = "ブルーム";
	public static string vignetteText_DEF = "Vignette";
	public Text drawDistanceText;
	public Slider drawDistanceSlider;
	public Text bgmVolumeText;
	public Slider bgmVolumeSlider;
	public Text seVolumeText;
	public Slider seVolumeSlider;
	public Text cameraMoveSpeedText;
	public Slider cameraMoveSpeedSlider;
	public Text dragRotSpeedText;
	public Slider dragRotSpeedSlider;
	public Text antialiasingText;
	public Toggle antialiasingToggle;
	public Text aoText;
	public Toggle aoToggle;
	public Text motionBlurText;
	public Toggle motionBlurToggle;
	public Text bloomText;
	public Toggle bloomToggle;
	public Text vignetteText;
	public Toggle vignetteToggle;

	private int lastDrawDistance;
	private float lastBgmVolume;
	private float lastSeVolume;
	private float lastCameraMoveSpeed;
	private float lastDragRotSpeed;
	private bool lastAntialiasing;
	private bool lastAO;
	private bool lastMotionBlur;
	private bool lastBloom;
	private bool lastVignette;

	void Update () {
		drawDistanceText.text = drawDistanceText_DEF + ": " + (int)drawDistanceSlider.value + " ";
		bgmVolumeText.text = bgmVolumeText_DEF + ": " + (int)(bgmVolumeSlider.value * 100f) + " ";
		seVolumeText.text = seVolumeText_DEF + ": " + (int)(seVolumeSlider.value * 100f) + " ";
		cameraMoveSpeedText.text = cameraMoveSpeedText_DEF + ": " + Mathf.Round (cameraMoveSpeedSlider.value * 100f) / 100f + " ";
		dragRotSpeedText.text = dragRotSpeedText_DEF + ": " + Mathf.Round (dragRotSpeedSlider.value * 100f) / 100f + " ";
		antialiasingText.text = antialiasingText_DEF + ": ";
		aoText.text = aoText_DEF + ": ";
		motionBlurText.text = motionBlurText_DEF + ": ";
		bloomText.text = bloomText_DEF + ": ";
		vignetteText.text = vignetteText_DEF + ": ";
		if (Input.GetKeyDown (KeyCode.Escape))
			cancel ();
	}

	public void openSSDir () {
		Main.openSSDir ();
	}

	public void load () {
		drawDistanceSlider.minValue = Main.MIN_DRAW_DISTANCE;
		drawDistanceSlider.maxValue = Main.MAX_DRAW_DISTANCE;
		bgmVolumeSlider.minValue = Main.MIN_BGM_VOLUME;
		bgmVolumeSlider.maxValue = Main.MAX_BGM_VOLUME;
		seVolumeSlider.minValue = Main.MIN_SE_VOLUME;
		seVolumeSlider.maxValue = Main.MAX_SE_VOLUME;
		cameraMoveSpeedSlider.minValue = Main.MIN_CAMERA_MOVE_SPEED;
		cameraMoveSpeedSlider.maxValue = Main.MAX_CAMERA_MOVE_SPEED;
		dragRotSpeedSlider.minValue = Main.MIN_DRAG_ROT_SPEED;
		dragRotSpeedSlider.maxValue = Main.MAX_DRAG_ROT_SPEED;
		reload ();
	}

	private void reload () {
		drawDistanceSlider.value = lastDrawDistance = Main.drawDistance;
		bgmVolumeSlider.value = lastBgmVolume = Main.bgmVolume;
		seVolumeSlider.value = lastSeVolume = Main.seVolume;
		cameraMoveSpeedSlider.value = lastCameraMoveSpeed = Main.cameraMoveSpeed;
		dragRotSpeedSlider.value = lastDragRotSpeed = Main.dragRotSpeed;
		antialiasingToggle.isOn = lastAntialiasing = Main.antialiasing;
		aoToggle.isOn = lastAO = Main.ao;
		motionBlurToggle.isOn = lastMotionBlur = Main.motionBlur;
		bloomToggle.isOn = lastBloom = Main.bloom;
		vignetteToggle.isOn = lastVignette = Main.vignette;
	}

	new public void show (bool show) {
		if (show)
			load ();

		base.show (show);
	}

	public void reflect () {
		if (!isShowing ())
			return;
		
		Main.drawDistance = (int)drawDistanceSlider.value;
		Main.bgmVolume = bgmVolumeSlider.value;
		Main.seVolume = seVolumeSlider.value;
		Main.cameraMoveSpeed = Mathf.Round (cameraMoveSpeedSlider.value * 100f) / 100f;
		Main.dragRotSpeed = Mathf.Round (dragRotSpeedSlider.value * 100f) / 100f;
		Main.antialiasing = antialiasingToggle.isOn;
		Main.ao = aoToggle.isOn;
		Main.motionBlur = motionBlurToggle.isOn;
		Main.bloom = bloomToggle.isOn;
		Main.vignette = vignetteToggle.isOn;
		Main.reflectSettings ();
	}

	public void save () {
		reflect ();
		Main.saveSettings ();

		show (false);
	}

	public void cancel () {
		drawDistanceSlider.value = lastDrawDistance;
		bgmVolumeSlider.value = lastBgmVolume;
		seVolumeSlider.value = lastSeVolume;
		cameraMoveSpeedSlider.value = lastCameraMoveSpeed;
		dragRotSpeedSlider.value = lastDragRotSpeed;
		antialiasingToggle.isOn = lastAntialiasing;
		aoToggle.isOn = lastAO;
		motionBlurToggle.isOn = lastMotionBlur;
		bloomToggle.isOn = lastBloom;
		vignetteToggle.isOn = lastVignette;
		reflect ();

		show (false);
	}

	public void reset () {
		drawDistanceSlider.value = Main.DEFAULT_DRAW_DISTANCE;
		bgmVolumeSlider.value = Main.DEFAULT_BGM_VOLUME;
		seVolumeSlider.value = Main.DEFAULT_SE_VOLUME;
		cameraMoveSpeedSlider.value = Main.DEFAULT_CAMERA_MOVE_SPEED;
		dragRotSpeedSlider.value = Main.DEFAULT_DRAG_ROT_SPEED;
		antialiasingToggle.isOn = Main.DEFAULT_ANTIALIASING;
		aoToggle.isOn = Main.DEFAULT_AO;
		motionBlurToggle.isOn = Main.DEFAULT_MOTIONBLUR;
		bloomToggle.isOn = Main.DEFAULT_BLOOM;
		vignetteToggle.isOn = Main.DEFAULT_VIGNETTE;
		reflect ();
	}
}
