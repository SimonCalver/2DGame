﻿using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {

	Quaternion rotation = Quaternion.Euler (0, 0, -90);

	float radius = 100.0f;
	float angle = 0.0f;
	float zoom = 100.0f;
	// Use this to iterate over surface points
	int index = 0;

	void Start () {

		//Use surface height from WorldMaker code to set position of camera

		transform.position = new Vector3 (100.0f, 0.0f, -400.0f);
		transform.rotation = rotation;
		Camera.main.orthographicSize = 100;
	}
	

	void Update () {

		if (Input.GetKey ("left")) {

			// seems like it might be a bad idea to have this increasing all the time
			angle += Time.deltaTime;

			Vector3 newPos = new Vector3 (radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), -400.0f);

			transform.position = newPos;

			Quaternion newRotation = Quaternion.Euler (0, 0, 180/Mathf.PI*angle-90);
			transform.rotation = newRotation;
		}
		if (Input.GetKey ("right")) {

			angle -= Time.deltaTime;

			Vector3 newPos = new Vector3 (radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), -400.0f);

			transform.position = newPos;

			Quaternion newRotation = Quaternion.Euler (0, 0, 180/Mathf.PI*angle-90);
			transform.rotation = newRotation;
		}
		if (Input.GetKey ("up")) {

			radius += 5.0f;

			Vector3 newPos = new Vector3 (radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), -400.0f);

			transform.position = newPos;
		}
		if (Input.GetKey ("down")) {

			radius -= 5.0f;

			Vector3 newPos = new Vector3 (radius * Mathf.Cos (angle), radius * Mathf.Sin (angle), -400.0f);

			transform.position = newPos;
		}
	}


	void OnGUI(){
		if (Event.current.type == EventType.ScrollWheel) {
			if (Event.current.delta.y > 0 && zoom < 500) {
				zoom *= 1.2f;
				Camera.main.orthographicSize = zoom;
			}
			if (Event.current.delta.y < 0 && zoom > 4) {
				zoom /= 1.2f;
				Camera.main.orthographicSize = zoom;
			}
		}
	}

}
