using UnityEngine;
using System.Collections;

public class ErrorManager : MonoBehaviour {

	static private string m_errorText;

	static public string ErrorText {get {return m_errorText;} set{m_errorText = value;}}

	void OnGUI()
	{
		GUI.Box(new Rect(0,10, Screen.width, 20) , m_errorText);
	}
}
