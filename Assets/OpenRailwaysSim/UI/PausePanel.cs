using UnityEngine;

public class PausePanel : GamePanel {

	public void ResumeButton () {
		Main.setPause (false);
	}

	public void SaveButton () {
		MapManager.saveMap (Main.playingmap);
	}
}
