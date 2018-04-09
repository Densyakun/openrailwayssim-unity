using UnityEngine;
using UnityEngine.UI;

public class TrackSettingPanel : GamePanel {
	//TODO 多言語対応化
	public static string lengthText_DEF = "長さ";
	public Text lengthText;
	public Slider lengthSlider;

	private float lastLength;

	void Update () {
		lengthText.text = lengthText_DEF + ": " + lengthSlider.value + "m ";
		if (Input.GetKeyDown (KeyCode.Escape))
			cancel ();
	}

	public void load () {
		lengthSlider.minValue = Main.MIN_TRACK_LENGTH;
		lengthSlider.maxValue = Mathf.Max (1f, lengthSlider.value * 2);
		reload ();
	}

	private void reload () {
		lengthSlider.value = lastLength = Main.editingTrack.length;
	}

	new public void show (bool show) {
		if (show)
			load ();

		base.show (show);
	}

	public void reflect () {
		if (!isShowing ())
			return;
		
		Main.editingTrack.length = lengthSlider.value;

		show (false);
	}

	public void cancel () {
		lengthSlider.value = lastLength;
		reflect ();

		show (false);
	}
}
