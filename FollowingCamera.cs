using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour {
	public static FollowingCamera main;

	public Transform cam;
	public float rotationSpeed = 65, zoomSpeed = 50, moveSpeed = 30;
	float rotationSmoothCoefficient = 0;
	float rotationSmoothAcceleration = 0.05f;
	float zoomSmoothCoefficient = 0;
	float zoomSmoothAcceleration = 0.05f;
	Vector3 moveSmoothCoefficient = Vector3.zero;
	float moveSmoothAcceleration= 0.03f;
	public Vector3 deltaLimits = new Vector3 (0.1f, 0.1f, 0.1f);
	public Vector3 camPoint = new Vector3(0,5,-5);

	Vector3 lookPoint;
	bool changingBasePos = false, changingCamZoom = false;
	float optimalDistance = 5;
    [SerializeField] bool useAutoZooming = true;

	void Start() {
		if (main != null & main != this) Destroy(main);
		main = this;
		cam.transform.position = transform.position + transform.TransformDirection(camPoint);
		cam.transform.LookAt(transform.position);
	}

	void LateUpdate () {
		if (cam == null ) return;
		Vector3 mv = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		if (Input.GetKey(KeyCode.Space)) mv.y = 1;
		else {
			if (Input.GetKey(KeyCode.LeftControl)) mv.y = -1;
		}
		if (mv != Vector3.zero) {
			#region dropping camera auto moving
			if ( changingBasePos ) {
				changingBasePos = false;
				moveSmoothCoefficient = Vector3.zero;
			}
			if ( changingCamZoom ) {
				changingCamZoom = false;
				rotationSmoothCoefficient = 0;
			}
			#endregion

			if (mv.x / moveSmoothCoefficient.x > 0) mv.x += moveSmoothAcceleration; else moveSmoothCoefficient.x = 0;
			if (mv.y / moveSmoothCoefficient.y > 0) mv.y += moveSmoothAcceleration ; else moveSmoothCoefficient.y  = 0;
			if (mv.z / moveSmoothCoefficient.z > 0) mv.z += moveSmoothAcceleration ; else moveSmoothCoefficient.z  = 0;
			mv.x *= ( 1+ moveSmoothCoefficient.x);
			mv.y *= ( 1+ moveSmoothCoefficient.y);
			mv.z *= ( 1+ moveSmoothCoefficient.z);
			transform.Translate(mv * moveSpeed * Time.deltaTime,Space.Self);
			//transform.Translate(mv * 30 * Time.deltaTime,Space.Self);
		}
		else moveSmoothCoefficient = Vector3.zero;

		float delta = 0;
		if (Input.GetMouseButton(2)) {
			bool a = false , b = false; //rotation detectors
			float rspeed = rotationSpeed * Time.deltaTime * ( 1 + rotationSmoothCoefficient);
			delta = Input.GetAxis("Mouse X");
			if (delta != 0) {
				#region dropping camera auto moving
				if ( changingBasePos ) {
					changingBasePos = false;
					moveSmoothCoefficient = Vector3.zero;
				}
				if ( changingCamZoom ) {
					changingCamZoom = false;
					rotationSmoothCoefficient = 0;
				}
				#endregion
				transform.RotateAround(transform.position, Vector3.up, rspeed * delta);
				rotationSmoothCoefficient += rotationSmoothAcceleration;
				a = true;
			}

			delta = Input.GetAxis("Mouse Y") ;
			if (delta != 0) {
				#region dropping camera auto moving
				if ( changingBasePos ) {
					changingBasePos = false;
					moveSmoothCoefficient = Vector3.zero;
				}
				if ( changingCamZoom ) {
					changingCamZoom = false;
					rotationSmoothCoefficient = 0;
				}
				#endregion
				transform.RotateAround(transform.position, transform.TransformDirection(Vector3.left), rspeed * delta);
				rotationSmoothCoefficient += rotationSmoothAcceleration;
				b = true;
			}

			if (a==false && b== false) rotationSmoothCoefficient = 0;
		}

		delta = Input.GetAxis("Mouse ScrollWheel");
		if (delta != 0) {
			#region dropping camera auto moving
			if ( changingBasePos ) {
				changingBasePos = false;
				moveSmoothCoefficient = Vector3.zero;
			}
			if ( changingCamZoom ) {
				changingCamZoom = false;
				rotationSmoothCoefficient = 0;
			}
			#endregion
			float zspeed = zoomSpeed * Time.deltaTime * ( 1 + zoomSmoothCoefficient) * delta * (-1);
			cam.transform.Translate((cam.transform.position - transform.position) * zspeed, Space.World );
			zoomSmoothCoefficient += zoomSmoothAcceleration;
		}
		else zoomSmoothCoefficient = 0;

		if (changingBasePos) {
			transform.position = Vector3.MoveTowards(transform.position, lookPoint, (1 + moveSmoothCoefficient.x) * moveSpeed/2f * Time.deltaTime);
			if (transform.position == lookPoint) {
				changingBasePos = false;
				moveSmoothCoefficient = Vector3.zero;
			}
			else moveSmoothCoefficient.x = moveSmoothAcceleration * moveSmoothAcceleration ;
		}
		if ( changingCamZoom ) {
			Vector3 endPoint =  cam.localPosition.normalized * optimalDistance;
			cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, endPoint, zoomSpeed/5f * Time.deltaTime * ( 1 + zoomSmoothCoefficient));
			cam.transform.LookAt(transform.position);
			if ( cam.transform.localPosition.magnitude / endPoint.magnitude == 1 )	{
				changingCamZoom = false;
				zoomSmoothCoefficient =0;
			}
			else zoomSmoothCoefficient = zoomSmoothAcceleration * zoomSmoothAcceleration * zoomSmoothAcceleration ;
		}
			
		//if (moveSmoothCoefficient > 2) moveSmoothCoefficient = 2;
	}

	public void SetLookPoint( Vector3 point ) {
		Vector3 camPrevPos = cam.transform.position;
		lookPoint = point;        
        if (lookPoint != transform.position) changingBasePos = true;
        if (useAutoZooming)
        {
            float d = cam.transform.localPosition.magnitude;
            if (d / optimalDistance > 1 | d/optimalDistance < 0.5f) changingCamZoom = true;
        }
	}
}
