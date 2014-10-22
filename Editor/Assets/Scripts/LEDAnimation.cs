using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public struct FrameState
{
	public BitArray lampState;
	public int 		duration;

	public FrameState Clone()
	{
		FrameState fs 	= new FrameState();
		fs.duration 	= duration;
		fs.lampState 	= lampState.Clone() as BitArray;

		return fs;
	}
}

public class LEDAnimation : MonoBehaviour {

	public GameObject 	m_lampPrefab;
	private int 		m_cubeLength 	= 3;
	private float 		m_lengthDistance= 2;
	private float 		m_hightDistance	= 2;

	private List<GameObject> m_lamps = new List<GameObject>();
	private List<FrameState> m_animation = new List<FrameState>();

	private int 	m_currentFrame = 0;
	private bool 	m_playingAnimation;
	private float	m_playbackSpeed = 1;
	private float 	m_playTimer;

	private bool 	m_showAcceptImport;

	private bool	m_showSettings;
	private bool	m_showAcceptCubeChangeWindow;
	private bool	m_s_vSync;
	private float 	m_s_cameraSens;
	private float	m_s_cameraSpeed;
	private float	m_s_cameraDragSpeed;
	private int 	m_s_cubeLength;


	void Awake()
	{
	}

	void Start()
	{
		LoadSettings();
		ResertCube();
	}

	void Update()
	{
		if (!m_playingAnimation && Input.GetButtonDown("Fire1")) 
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				if(hit.transform.tag == "Lamp")
				{
					Lamp lamp = hit.transform.gameObject.GetComponent<Lamp>();
					lamp.ToogleLamp();
					m_animation[m_currentFrame].lampState[lamp.m_id] ^= true;
				}
			}
		}

		if(m_playingAnimation)
		{
			m_playTimer += Time.deltaTime * m_playbackSpeed;

			float timeLeft = m_playTimer - (float)m_animation[m_currentFrame].duration / 100;

			if(timeLeft >= 0)
			{
				m_playTimer = timeLeft;
				m_currentFrame++;

				if(m_currentFrame >= m_animation.Count)
					m_currentFrame = 0;

				SelectFrame(m_currentFrame);
			}
		}
	}

	void SelectFrame(int frame)
	{
		m_currentFrame = frame;

		FrameState fs = m_animation[frame];

		for(int i = 0; i < (int)Mathf.Pow(m_cubeLength, 3); i++)
		{
			if(fs.lampState[i])
			{
				m_lamps[i].GetComponent<Lamp>().TurnOn();
			}
			else
			{
				m_lamps[i].GetComponent<Lamp>().TurnOff();
			}

		}
	}

	void AddNewFrame()
	{
		FrameState fs = new FrameState();
		fs.duration = m_animation[m_currentFrame].duration;
		fs.lampState = new BitArray((int)Mathf.Pow(m_cubeLength, 3));
		m_animation.Insert(m_currentFrame + 1, fs);	//Add the new frame after the current frame

		SelectFrame(m_currentFrame + 1);			//Select the newly created frame
	}

	void CopyNewFrame()
	{
		m_animation.Insert(m_currentFrame + 1, m_animation[m_currentFrame].Clone());	//Add the new frame after the current frame
		
		SelectFrame(m_currentFrame + 1);			//Select the newly created frame
	}

	void DeleteFrame()
	{
		if(m_animation.Count == 1)	//If you only have one frame just resert all the lamps
		{
			FrameState fs = new FrameState();
			fs.duration = 5;
			fs.lampState = new BitArray((int)Mathf.Pow(m_cubeLength, 3));

			m_animation[0] = fs;
		}
		else
		{
			m_animation.RemoveAt(m_currentFrame);
			m_currentFrame = Mathf.Clamp(m_currentFrame, 0, m_animation.Count - 1);	//Fix for when you remove the last frame
		}

		SelectFrame(m_currentFrame); //Need to update the status of the lamps
	}

	void PlayStopAnimation()
	{
		m_playingAnimation = !m_playingAnimation;
		m_playTimer = 0;
	}

	string GenerateOutput()
	{
		StringBuilder sb = new StringBuilder();

		foreach(FrameState fs in m_animation)
		{
			sb.Append("B");
			for(int i = 0; i < (int)Mathf.Pow(m_cubeLength, 3);i++)
			{
				sb.Append(fs.lampState[i] == true ? "1" : "0");

				if((i+1)%m_cubeLength == 0 && i != (int)Mathf.Pow(m_cubeLength, 3)-1)
					sb.Append(", B");
			}
			sb.Append(", " + fs.duration + ",\n");
		}


		return sb.ToString();
	}

	void ExportToClipBoard()
	{
		ClipboardHelper.clipBoard = GenerateOutput();
	}

	void ImportFromClipBoard()
	{
		ResertCube();

		string input = ClipboardHelper.clipBoard;

		string[] lines = input.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		StringBuilder sb = new StringBuilder();

		//Remove all comment lines
		bool isComment = false;
		for(int i = 0;i < lines.Length; i++)
		{
			int indexCommentStart = lines[i].IndexOf("/*");
			if(indexCommentStart != -1)
			{
				isComment = true;

				int indexCommentEnd = lines[i].IndexOf("*/");
				if(indexCommentEnd != -1)
				{
					lines[i] = lines[i].Remove(indexCommentStart, indexCommentEnd - indexCommentStart + 2);
					isComment = false;
				}
				else
				{
					lines[i] = lines[i].Remove(indexCommentStart);
				}
			}
			else if(isComment)
			{
				int indexCommentEnd = lines[i].IndexOf("*/");
				if(indexCommentEnd != -1)
				{
					isComment = false;
					lines[i] = lines[i].Remove(0, indexCommentEnd + 2);
				}
				else
				{
					lines[i] = "";
				}
			}

			int indexComment = lines[i].IndexOf("//");
			if(indexComment != -1)
			{
				lines[i] = lines[i].Remove(indexComment);
			}
			sb.Append(lines[i]);
		}

		input = sb.ToString();


		Regex rgx = new Regex("[^,0-9]");
		input = rgx.Replace(input, "");

		string[] inputs = input.Split(',');

		BitArray ba = new BitArray(1);
		int counter = 0;
		int a = 0;
		for(int i = 0; i < inputs.Length; i++)
		{
			if((i) % (m_cubeLength * m_cubeLength + 1) == 0 || i == 0)	//First in row
			{
				ba = new BitArray((int)Mathf.Pow(m_cubeLength,m_cubeLength));
				counter = 0;
			}

			if((i - 9) % (m_cubeLength * m_cubeLength + 1) == 0 || i - 9 == 0) //Last in row
			{
				FrameState fs = new FrameState();
				fs.duration = int.Parse(inputs[i]);
				fs.lampState = (BitArray)ba.Clone();

				m_animation.Add(fs);
			}
			else
			{
				for(int j = 0; j < m_cubeLength; j++)
				{
					try
					{
						bool on = int.Parse(inputs[i][j].ToString()) == 1 ? true : false;
						ba.Set(counter*m_cubeLength + j, on);
					}
					catch(System.IndexOutOfRangeException ex)
					{
						Debug.LogError("Row: " + Mathf.Floor(i / (m_cubeLength*m_cubeLength)));
					}
				}
			}
			counter++;
			a++;
		}
	}


	void ResertCube()
	{
		m_currentFrame = 0;
		m_animation.Clear();
		
		foreach(GameObject obj in m_lamps)
		{
			Destroy(obj);
		}
		
		m_lamps.Clear();
		
		//Create the number of lamps needed depending on the length of the cube
		for(int i = 0; i < m_cubeLength; i++)
		{
			for(int j = 0; j < m_cubeLength; j++)
			{
				for(int k = 0; k < m_cubeLength; k++)
				{
					GameObject lamp = Instantiate(m_lampPrefab, new Vector3(j * m_lengthDistance, i * m_hightDistance, -k * m_lengthDistance), Quaternion.identity) as GameObject;
					lamp.GetComponent<Lamp>().m_id = i * m_cubeLength * m_cubeLength + j * m_cubeLength + k;
					m_lamps.Add(lamp);
				}
			}
		}
		
		FrameState fs = new FrameState();
		fs.duration = 5;
		fs.lampState = new BitArray((int)Mathf.Pow(m_cubeLength, 3));
		m_animation.Add(fs);
	}

	void LoadSettings()
	{
		float cameraSens 		= PlayerPrefs.GetFloat("CameraSens", 6);
		float cameraSpeed 		= PlayerPrefs.GetFloat("CameraSpeed", 8);
		float cameraDragSpeed 	= PlayerPrefs.GetFloat("CameraDragSpeed", 25);

		m_cubeLength = PlayerPrefs.GetInt("CubeLength", 3);
		Camera.main.GetComponent<MouseLook>().sensitivityX 			= cameraSens;
		Camera.main.GetComponent<MouseLook>().sensitivityY		 	= cameraSens;
		Camera.main.GetComponent<CameraMovement>().m_cameraSpeed 	= cameraSpeed;
		Camera.main.GetComponent<CameraMovement>().m_dragCameraSpeed= cameraDragSpeed;
	}

	void OpenSettings()
	{
		m_s_vSync = QualitySettings.vSyncCount == 1 ? true : false;

		m_s_cameraSens		= Camera.main.GetComponent<MouseLook>().sensitivityX;
		m_s_cameraSpeed 	= Camera.main.GetComponent<CameraMovement>().m_cameraSpeed;
		m_s_cameraDragSpeed = Camera.main.GetComponent<CameraMovement>().m_dragCameraSpeed;
		m_s_cubeLength		= m_cubeLength;
	}

	void ApplySettings()
	{
		m_showSettings = false;

		PlayerPrefs.SetFloat("CameraSens", m_s_cameraSens);
		PlayerPrefs.SetFloat("CameraSpeed", m_s_cameraSpeed);
		PlayerPrefs.SetFloat("CameraDragSpeed", m_s_cameraDragSpeed);
		PlayerPrefs.SetInt("CubeLength", m_cubeLength);

		Camera.main.GetComponent<MouseLook>().sensitivityX 			= m_s_cameraSens;
		Camera.main.GetComponent<MouseLook>().sensitivityY		 	= m_s_cameraSens;
		Camera.main.GetComponent<CameraMovement>().m_cameraSpeed 	= m_s_cameraSpeed;
    	Camera.main.GetComponent<CameraMovement>().m_dragCameraSpeed= m_s_cameraDragSpeed;

		if(m_s_cubeLength != m_cubeLength)
		{
			m_cubeLength = m_s_cubeLength;
			ResertCube();
		}
	}

	void OnGUI()
	{
		int frameButtonOffset = 10;

		//Calculate how many rows of frame buttons you gonna need and offset everything up so it fits
		int currX = frameButtonOffset;
		int neededRows = 1;
		foreach(FrameState fs in m_animation)
		{
			if(currX + Mathf.Min(fs.duration * 5, 250) + 10 >= Screen.width)
			{
				currX = frameButtonOffset;
				neededRows++;
			}

			currX += Mathf.Min(fs.duration * 5, 250) + 1;
		}



		int xPos = frameButtonOffset;
		int yPos = 0;
		int i = 0;
		foreach(FrameState fs in m_animation)
		{
			//Highlight current frame
			GUI.color = m_currentFrame == i ? Color.red : Color.white;

			int frameWidth 	= Mathf.Min(fs.duration * 5, 250);
			int frameHeight = 25;

			string frameText = fs.duration <= 50 ? fs.duration.ToString() : "> 50";			

			if(xPos + frameWidth + 10 >= Screen.width)
			{
				xPos = frameButtonOffset;
				yPos += 25;
			}

			if(GUI.Button(new Rect(xPos , Screen.height - 25 * neededRows + yPos, frameWidth, frameHeight), frameText))
			{
				SelectFrame(i);
			}

			xPos += frameWidth + 1;

			i++;
		}

		GUI.color = Color.white;	//Resert the color

		//Makes sure you can only use 0-9 in the textfield
		char chr = Event.current.character;
		if ( (chr < '0' || chr > '9') ) 
		{
			Event.current.character = '\0';
		}

		FrameState fa =  m_animation[m_currentFrame];
		int newDuration	= (int)GUI.HorizontalSlider(new Rect(25, Screen.height - (neededRows + 1) * 25 - 2, 100, 30), m_animation[m_currentFrame].duration, 1.0F, 25.0F);
		if(!m_playingAnimation)
			fa.duration = newDuration;
		int a = int.Parse(GUI.TextField(new Rect(125, Screen.height - (neededRows + 1) * 25 - 5, 40, 20), fa.duration.ToString()));
		if(!m_playingAnimation)
			fa.duration = a;

		GUI.Label(new Rect(175, Screen.height - (neededRows + 1) * 25 - 5, 200, 20), "Duration (1 = 10ms)");

		m_animation[m_currentFrame] = fa;


		if(GUI.Button(new Rect(10,Screen.height - (neededRows + 1) * 25 - 50, 100,20), "New Frame") && !m_playingAnimation)
		{
			AddNewFrame();
		}

		if(GUI.Button(new Rect(110,Screen.height - (neededRows + 1) * 25 - 50,100,20), "Copy Frame") && !m_playingAnimation)
		{
			CopyNewFrame();
		}

		if(GUI.Button(new Rect(210,Screen.height - (neededRows + 1) * 25 - 50,100,20), "Delete Frame") && !m_playingAnimation)
		{
			DeleteFrame();
		}

		string playStopString = m_playingAnimation ? "Stop" : "Play";

		GUI.color = m_playingAnimation ? Color.red : Color.white;

		if(GUI.Button(new Rect(310,Screen.height - (neededRows + 1) * 25 - 50,100,20), playStopString))
		{
			PlayStopAnimation();
		}

		GUI.color = Color.white;

		m_playbackSpeed = Mathf.Round(GUI.HorizontalSlider(new Rect(410,Screen.height - (neededRows + 1) * 25 - 45,100,20), m_playbackSpeed, 0.1F, 5.0F) * 10) / 10;
		GUI.Label(new Rect(510,Screen.height - (neededRows + 1) * 25 - 50,200,20), "Playback speed: " + m_playbackSpeed);

		if(GUI.Button(new Rect(Screen.width - 155,50,150,20), "Copy To Clipboard"))
		{
			ExportToClipBoard();
		}

		if(GUI.Button(new Rect(Screen.width - 155,80,150,20), "Import From Clipboard"))
		{
			m_showAcceptImport = !m_showAcceptImport;
		}

		if(m_showAcceptImport)
		{
			GUI.Box(new Rect(Screen.width - 400,60,250,80), "Are you sure you want to import?\nIt will remove everything you have done.");
			
			if(GUI.Button(new Rect(Screen.width - 380,110,50,25), "Yes"))
			{
				ImportFromClipBoard();
				m_showAcceptImport = false;
			}

			if(GUI.Button(new Rect(Screen.width - 250,110,50,25), "No"))
			{
				m_showAcceptImport = false;
			}
		}

		//SETTIGNS START
		if(GUI.Button(new Rect(5,5, 100, 20), "Settings"))
		{
			m_showSettings = !m_showSettings;
			if(m_showSettings)
			{
				OpenSettings();
			}
		}

		if(m_showSettings)
		{
			GUI.Box(new Rect(5, 30, 275, 150),"");

			GUI.Label(new Rect(10, 30, 100, 25), "Vsync");
			m_s_vSync = GUI.Toggle(new Rect(130, 31, 100, 25), m_s_vSync, "");

			GUI.Label(new Rect(10, 50, 130, 25), "Camera Sensitivity");
			m_s_cameraSens = GUI.HorizontalSlider(new Rect(130, 55, 100, 20), m_s_cameraSens, 1.0F, 35.0F);
			GUI.Label(new Rect(235, 50, 100, 25), m_s_cameraSens.ToString("F1"));
			
			GUI.Label(new Rect(10, 75, 130, 40), "Camera Speed");
			m_s_cameraSpeed = GUI.HorizontalSlider(new Rect(130, 80, 100, 20), m_s_cameraSpeed, 1.0F, 35.0F);
			GUI.Label(new Rect(235, 75, 100, 40), m_s_cameraSpeed.ToString("F1"));

			GUI.Label(new Rect(10, 100, 130, 40), "Camera Drag Speed");
			m_s_cameraDragSpeed = GUI.HorizontalSlider(new Rect(130, 105, 100, 20), m_s_cameraDragSpeed, 1.0F, 35.0F);
			GUI.Label(new Rect(235, 100, 100, 40), m_s_cameraDragSpeed.ToString("F1"));

			GUI.Label(new Rect(10, 125, 130, 40), "Cube Size");
			m_s_cubeLength = (int)GUI.HorizontalSlider(new Rect(130, 130, 100, 20), m_s_cubeLength, 1, 15);
			GUI.Label(new Rect(235, 125, 100, 40), m_s_cubeLength.ToString());


			if(GUI.Button(new Rect(5, 140, 50, 25), "Apply"))
			{
				if(m_s_cubeLength == m_cubeLength)
				{
					ApplySettings();
					m_showAcceptCubeChangeWindow = false;
				}
				else
					m_showAcceptCubeChangeWindow = true;
			}
			
			if(GUI.Button(new Rect(100, 140, 50, 25), "Close"))
			{
				m_showSettings = false;
				m_showAcceptCubeChangeWindow = false;
			}

			if(m_showAcceptCubeChangeWindow)
			{
				GUI.Box(new Rect(Screen.width/2 - 150, Screen.height/2 - 100, 300, 75),"Are you sure you want to change cube size? \n All your frames will be erased.");

				if(GUI.Button(new Rect(Screen.width/2 - 75, Screen.height/2 - 55, 50, 25), "Yes"))
				{
					ApplySettings();
					m_showAcceptCubeChangeWindow = false;
				}

				if(GUI.Button(new Rect(Screen.width/2 + 25, Screen.height/2 - 55, 50, 25), "No"))
				{
					m_showSettings = false;
					m_showAcceptCubeChangeWindow = false;
				}
			}
		}

		//SETTINGS END
	}
}
