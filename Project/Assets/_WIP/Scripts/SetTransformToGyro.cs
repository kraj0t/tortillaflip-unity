using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTransformToGyro : MonoBehaviour {


    public float multiplier = 1f;

    public bool onlyIfTouch = false;

	
	void Update () {
        if ( !onlyIfTouch || (onlyIfTouch && Input.touchCount != 0) ) {
            Vector3 accel = Input.gyro.userAcceleration;
            accel = new Vector3( -accel.x, -accel.y, accel.z );
            transform.localPosition = accel * multiplier;
        }
	}
}
