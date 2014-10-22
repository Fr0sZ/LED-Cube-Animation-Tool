using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	[HideInInspector]
	public 	float m_cameraSpeed = 8f;
	[HideInInspector]
	public 	float m_fastCameraSpeed = 15;
	[HideInInspector]
	public	float m_dragCameraSpeed = 25;

	private float m_scrollZoomSpeed = 1;

	// Update is called once per frame
	void Update () 
	{
		float moveSpeed = m_cameraSpeed;

		if(Input.GetKey(KeyCode.LeftShift))
		{
			moveSpeed = m_fastCameraSpeed;
		}

		transform.position += transform.forward.normalized * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
		transform.position += transform.right.normalized * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

		if(Input.GetKey(KeyCode.Q))
			transform.position -= Vector3.up * moveSpeed * Time.deltaTime;

		if(Input.GetKey(KeyCode.E))
			transform.position += Vector3.up * moveSpeed * Time.deltaTime;

		//Camera drag
		if(Input.GetButton("Fire3"))
		{
			transform.position -= transform.up.normalized * Input.GetAxis("Mouse Y") * m_dragCameraSpeed * Time.deltaTime;
			transform.position -= transform.right.normalized * Input.GetAxis("Mouse X") * m_dragCameraSpeed * Time.deltaTime;
		}

		//Scroll zoom
		if(Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			transform.position += transform.forward.normalized * m_scrollZoomSpeed;
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			transform.position -= transform.forward.normalized * m_scrollZoomSpeed;
		}
	}
}