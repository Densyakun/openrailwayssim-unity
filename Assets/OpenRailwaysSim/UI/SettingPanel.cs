using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : GamePanel {
	//TODO 多言語対応化
	public static string drawDistanceText_DEF = "描画距離";
	public static string bgmVolumeText_DEF = "BGM音量";
	public static string seVolumeText_DEF = "SE音量";
	public static string dragRotSpeedText_DEF = "マウスでのカメラ回転速度";
	public static string bloomText_DEF = "ブルーム";
	public Text drawDistanceText;
	public Slider drawDistanceSlider;
	public Text bgmVolumeText;
	public Slider bgmVolumeSlider;
	public Text seVolumeText;
	public Slider seVolumeSlider;
	public Text dragRotSpeedText;
	public Slider dragRotSpeedSlider;
	public Text bloomText;
	public Toggle bloomToggle;

	private int lastDrawDistance;
	private float lastBgmVolume;
	private float lastSeVolume;
	private float lastDragRotSpeed;
	private bool lastContrastStretch;
	private bool lastBloom;

	void Update () {
		drawDistanceText.text = drawDistanceText_DEF + ": " + (int)drawDistanceSlider.value + " ";
		bgmVolumeText.text = bgmVolumeText_DEF + ": " + (int)(bgmVolumeSlider.value * 100f) + " ";
		seVolumeText.text = seVolumeText_DEF + ": " + (int)(seVolumeSlider.value * 100f) + " ";
		dragRotSpeedText.text = dragRotSpeedText_DEF + ": " + Mathf.Round (dragRotSpeedSlider.value * 100f) / 100f + " ";
		bloomText.text = bloomText_DEF + ": ";
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
		dragRotSpeedSlider.minValue = Main.MIN_DRAG_ROT_SPEED;
		dragRotSpeedSlider.maxValue = Main.MAX_DRAG_ROT_SPEED;
		reload ();
	}

	private void reload () {
		drawDistanceSlider.value = lastDrawDistance = Main.drawDistance;
		bgmVolumeSlider.value = lastBgmVolume = Main.bgmVolume;
		seVolumeSlider.value = lastSeVolume = Main.seVolume;
		dragRotSpeedSlider.value = lastDragRotSpeed = Main.dragRotSpeed;
		bloomToggle.isOn = lastBloom = Main.bloom;
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
		Main.dragRotSpeed = Mathf.Round (dragRotSpeedSlider.value * 100f) / 100f;
		Main.bloom = bloomToggle.isOn;
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
		dragRotSpeedSlider.value = lastDragRotSpeed;
		bloomToggle.isOn = lastBloom;
		reflect ();

		show (false);
	}

	public void reset () {
		drawDistanceSlider.value = Main.DEFAULT_DRAW_DISTANCE;
		bgmVolumeSlider.value = Main.DEFAULT_BGM_VOLUME;
		seVolumeSlider.value = Main.DEFAULT_SE_VOLUME;
		dragRotSpeedSlider.value = Main.DEFAULT_DRAG_ROT_SPEED;
		bloomToggle.isOn = Main.DEFAULT_BLOOM;
		reflect ();
	}
}
