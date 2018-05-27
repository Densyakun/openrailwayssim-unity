using UnityEngine;
using UnityEngine.UI;

//ゲームのバージョンを表示するスクリプト
public class VersionText : MonoBehaviour {

	void OnEnable () {
		GetComponent<Text> ().text = "Ver: " + Main.VERSION;
	}
}
