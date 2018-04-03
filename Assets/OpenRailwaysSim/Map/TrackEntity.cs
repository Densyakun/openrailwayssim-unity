using UnityEngine;

public class TrackEntity : MonoBehaviour {
	public virtual Track track { get; protected set; }
	//bool initialized = false;

	void Start () {
		track.reloadEntity ();
	}

	public virtual void init (Track obj) {
		this.track = obj;
		//initialized = true;
	}
}
