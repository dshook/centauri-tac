using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HighlightingSystem;

[DisallowMultipleComponent]
public class PresetSelector : MonoBehaviour
{
	#region Public Fields
	public Dropdown dropdown;
	#endregion

	#region Private Fields
	private HighlightingRenderer hr;
	#endregion

	#region MonoBehaviour
	// 
	void OnEnable()
	{
		hr = FindRenderer();
		UpdateDropdown();
		dropdown.onValueChanged.AddListener(OnValueChanged);
	}

	// 
	void OnDisable()
	{
		dropdown.onValueChanged.RemoveListener(OnValueChanged);
		hr = null;
	}

	// 
	void Update()
	{
		TrackChanges();
	}
	#endregion

	#region Private Methods
	// 
	private void OnValueChanged(int index)
	{
		var options = dropdown.options;
		if (hr == null || index < 0 || index >= options.Count) { return; }

		string name = options[index].text;
		hr.LoadPreset(name);
	}

	// 
	private HighlightingRenderer FindRenderer()
	{
		Camera camera = Camera.main;
		if (camera != null)
		{
			HighlightingRenderer result = camera.GetComponent<HighlightingRenderer>();
			if (result != null)
			{
				return result;
			}
		}

		Camera[] allCameras = Camera.allCameras;
		for (int i = 0, l = allCameras.Length; i < l; i++)
		{
			camera = allCameras[i];
			HighlightingRenderer result = camera.GetComponent<HighlightingRenderer>();
			if (result != null)
			{
				return result;
			}
		}

		return null;
	}

	// 
	private void TrackChanges()
	{
		HighlightingRenderer newHr = FindRenderer();
		if (newHr != hr)
		{
			hr = newHr;
			UpdateDropdown();
		}
		else
		{
			var presets = hr.presets;
			var options = dropdown.options;
			int l = presets.Count;
			if (options.Count != l)
			{
				UpdateDropdown();
			}
			else
			{
				for (int i = 0; i < l; i++)
				{
					if (presets[i].name != options[i].text)
					{
						UpdateDropdown();
						break;
					}
				}
			}
		}
	}

	// 
	private void UpdateDropdown()
	{
		if (hr == null)
		{
			dropdown.options = null;
			dropdown.value = -1;
			return;
		}

		// Update options
		var presets = hr.presets;
		int l = presets.Count;
		List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(l);
		for (int i = 0; i < l; i++)
		{
			HighlightingPreset preset = presets[i];
			Dropdown.OptionData option = new Dropdown.OptionData(preset.name);
			options.Add(option);
		}
		dropdown.options = options;

		// Find and select currently active preset
		for (int i = 0; i < l; i++)
		{
			HighlightingPreset preset = presets[i];
			if (hr.downsampleFactor == preset.downsampleFactor &&
				hr.iterations == preset.iterations &&
				hr.blurMinSpread == preset.blurMinSpread &&
				hr.blurSpread == preset.blurSpread &&
				hr.blurIntensity == preset.blurIntensity &&
				hr.blurDirections == preset.blurDirections)
			{
				dropdown.value = i;
				break;
			}
		}
	}
	#endregion
}