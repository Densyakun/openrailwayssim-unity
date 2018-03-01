using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour {

	void OnEnable () {
		GetComponent<Text> ().text = "Ver: " + Main.VERSION;
	}
}
