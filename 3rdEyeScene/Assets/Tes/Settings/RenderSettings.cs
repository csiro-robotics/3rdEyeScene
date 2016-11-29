using System.ComponentModel;
using UnityEngine;

public class RenderSettings : Settings
{
	private static RenderSettings _instance = new RenderSettings();
	public static RenderSettings Instance { get { return _instance; } }

	public RenderSettings()
	{
		Name = "Render";
	}

	[Browsable(true), Tooltip("Enable Eye-Dome-Lighting shader?")]
	public bool EdlShader
	{
		get { return PlayerPrefsX.GetBool("render.edl", true); }
		set { PlayerPrefsX.SetBool("render.edl", value); Notify("EdlShader"); }
	}

	[Browsable(true), SetRange(1, 10), Tooltip("The pixel search radius used in EDL calculations.")]
	public int EdlRadius
	{
		get { return PlayerPrefs.GetInt("render.edlRadius", 1); }
		set { PlayerPrefs.SetInt("render.edlRadius", value); Notify("EdlRadius"); }
	}

	[Browsable(true), SetRange(1, 1000), Tooltip("Exponential scaling for EDL shader.")]
	public float EdlExponentialScale
	{
		get { return PlayerPrefs.GetFloat("render.edlExponentialScale", 500); }
		set { PlayerPrefs.SetFloat("render.edlExponentialScale", value); Notify("EdlExponentialScale"); }
	}

	[Browsable(true), SetRange(1, 100), Tooltip("Linear scaling for EDL shader.")]
	public float EdlLinearScale
	{
		get { return PlayerPrefs.GetFloat("render.edlLinearScale", 10); }
		set { PlayerPrefs.SetFloat("render.edlLinearScale", value); Notify("EdlLinearScale"); }
	}
}
