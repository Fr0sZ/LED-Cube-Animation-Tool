using UnityEngine;
using System.Collections;

public class Lamp : MonoBehaviour {

	public int m_id = -1;
	private bool m_isOn = false;

	public GameObject m_childLight;

	public void ToogleLamp()
	{
		if(m_isOn)
			TurnOff();
		else
			TurnOn ();
	}

	public void TurnOn()
	{
		m_isOn = true;
		gameObject.renderer.material.color = new Color(1,1,1);
		m_childLight.SetActive(true);
	}

	public void TurnOff()
	{
		m_isOn = false;
		gameObject.renderer.material.color = new Color(0.1f,0.1f,0.1f);
		m_childLight.SetActive(false);
	}
}
