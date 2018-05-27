using UnityEngine;

//ポーズしているときに表示される画面
public class PausePanel : GamePanel {

	public void ResumeButton () {
		Main.setPause (false);
	}

	public void SaveButton () {
		MapManager.saveMap (Main.playingmap);
	}
}
