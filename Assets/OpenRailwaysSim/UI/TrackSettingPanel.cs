using System;
using UnityEngine;
using UnityEngine.UI;

public class TrackSettingPanel : GamePanel {
	//TODO 多言語対応化
	public static string lengthText_DEF = "長さ";
	public Text lengthText;
	public InputField lengthInput;

	private float lastLength;

	void Update () {
		reflect ();
	}

	public void load () {
		//lengthSlider.minValue = Main.MIN_TRACK_LENGTH;
		//lengthSlider.maxValue = Mathf.Max (1f, lengthSlider.value * 2);
		lengthInput.text = (lastLength = Main.editingTrack.length).ToString ();
	}

	new public void show (bool show) {
		if (show) {
			lengthText.text = lengthText_DEF + ": ";

			load ();
		}

		base.show (show);
	}

	public void reflect () {
		if (!isShowing ())
			return;

		try {
			Main.editingTrack.length = float.Parse (lengthInput.text);
			Main.editingTrack.reloadEntity ();
		} catch (FormatException e) {
		}
	}

	public void save () {
		reflect ();
		Main.playingmap.addTrack (Main.editingTrack);
		Main.editingTrack = null;

		show (false);
	}

	/*public void cancel () {
		lengthInput.text = lastLength.ToString ();
		reflect ();

		show (false);
	}*/
}
