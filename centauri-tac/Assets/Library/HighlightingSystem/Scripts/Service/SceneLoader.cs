using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#endif

[DisallowMultipleComponent]
public class SceneLoader : MonoBehaviour
{
	static private readonly KeyCode[] keysNext = new KeyCode[] { KeyCode.PageDown, KeyCode.Joystick1Button3, KeyCode.Joystick2Button3, KeyCode.Joystick3Button3, KeyCode.Joystick4Button3, KeyCode.Joystick5Button3, KeyCode.Joystick6Button3, KeyCode.Joystick7Button3, KeyCode.Joystick8Button3 };
	static private readonly KeyCode[] keysPrev = new KeyCode[] { KeyCode.PageUp, KeyCode.Joystick1Button2, KeyCode.Joystick2Button2, KeyCode.Joystick3Button2, KeyCode.Joystick4Button2, KeyCode.Joystick5Button2, KeyCode.Joystick6Button2, KeyCode.Joystick7Button2, KeyCode.Joystick8Button2 };

	static private readonly string descriptionDefault = "Use dropdown above to load different demonstration scenes";
	static private readonly string descriptionConstant = @"

Controls: 
W, S, A, D, Q, E, C, Space, RMB drag - camera movement
LMB Click - toggle flashing
RMB click - toggle see-through mode
'1' - fade in/out constant highlighting
'2' - turn on/off constant highlighting immediately
'3' - turn off all types of highlighting immediately
";
	static private readonly Dictionary<string, string> descriptions = new Dictionary<string, string>()
	{
		{ "01 Introduction",	"" },
		{ "02 Colors",			"Any color on any background can be used for the highlighting." },
		{ "03 Transparency",	"Transparent materials highlighting is supported." },
		{ "04 Occlusion",		"Highlighting occlusion and see-through mode demo." },
		{ "05 OccluderModes",	"Two highlighting occluder modes available." },
		{ "06 Scrpting",		"Using Highlighting System from C#, JavaScript and Boo scripts." },
		{ "07 Compound",		"Any changes in highlightable objects is properly handled." },
		{ "08 Mobile",			"Simple scene for mobile devices." },
	};

	#region Serialized Fields
	[HideInInspector]
	public List<string> sceneNames = new List<string>();
	#endregion

	#region Inspector Fields
	public Text title;
	public Text description;
	public Dropdown dropdown;
	public Button previous;
	public Button next;
	#endregion

	#if UNITY_EDITOR
	//
	[PostProcessSceneAttribute(2)]
	static public void OnPostProcessScene()
	{
		EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;

		List<string> sceneNames = new List<string>();
		for (int i = 0, l = existingScenes.Length; i < l; i++)
		{
			EditorBuildSettingsScene scene = existingScenes[i];
			if (scene.enabled)
			{
				sceneNames.Add(Path.GetFileNameWithoutExtension(scene.path));
			}
		}

		SceneLoader[] sceneLoaders = FindObjectsOfType<SceneLoader>();
		for (int i = 0, l = sceneLoaders.Length; i < l; i++)
		{
			sceneLoaders[i].sceneNames = sceneNames;
		}
	}
	#endif

	#region MonoBehaviour
	//
	void Start()
	{
		int index = -1;

		// Fill scenes dropdown list
		List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
		string activeSceneName = SceneManager.GetActiveScene().name;
		sceneNames.Sort();
		for (int i = 0, l = sceneNames.Count; i < l; i++)
		{
			string sceneName = sceneNames[i];

			Dropdown.OptionData option = new Dropdown.OptionData(sceneName);
			options.Add(option);
			
			if (index == -1 && activeSceneName == sceneName)
			{
				index = i;
			}
		}
		dropdown.options = options;
		if (index != -1)
		{
			dropdown.value = index;
		}

		// 
		UpdateButtons();

		// Set scene title and description
		title.text = activeSceneName;
		string d;
		if (!descriptions.TryGetValue(activeSceneName, out d))
		{
			d = descriptionDefault;
		}
		d += descriptionConstant;
		description.text = d;
	}

	// 
	void Update()
	{
		int input = InputHelper.GetKeyDown(keysNext, keysPrev);
		if (input > 0) { OnNext(); }
		else if (input < 0) { OnPrevious(); }
	}

	//
	void OnEnable()
	{
		dropdown.onValueChanged.AddListener(OnValueChanged);
		previous.onClick.AddListener(OnPrevious);
		next.onClick.AddListener(OnNext);
	}

	//
	void OnDisable()
	{
		dropdown.onValueChanged.RemoveListener(OnValueChanged);
		previous.onClick.RemoveListener(OnPrevious);
		next.onClick.RemoveListener(OnNext);
	}
	#endregion

	#region Private Methods
	//
	private void OnValueChanged(int index)
	{
		List<Dropdown.OptionData> options = dropdown.options;
		index = Mathf.Clamp(index, 0, options.Count - 1);

		string sceneName = options[index].text;
		string activeSceneName = SceneManager.GetActiveScene().name;
		if (!string.Equals(activeSceneName, sceneName))
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
			UpdateButtons();
		}
	}

	//
	private void OnPrevious()
	{
		OnValueChanged(dropdown.value - 1);
	}

	//
	private void OnNext()
	{
		OnValueChanged(dropdown.value + 1);
	}

	//
	private void UpdateButtons()
	{
		previous.interactable = dropdown.value > 0;
		next.interactable = dropdown.value < dropdown.options.Count - 1;
	}
	#endregion
}