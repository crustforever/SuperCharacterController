using System;
using UnityEngine;

namespace AssemblyCSharp
{
	[RequireComponent(typeof(CrustCharacterMachine))]
	public class CrustCharacterMachineDebug : MonoBehaviour
	{
		private CrustCharacterMachine _crust_character_machine;
		private float _time_scale = 1.0f;

		void Awake()
		{
			this._crust_character_machine = this.GetComponent<CrustCharacterMachine>();
		}

		void OnGUI()
		{
			GUI.Box(new Rect(10, 10, 200, 100), "Crust Character Machine");

			GUI.TextField(new Rect(20, 40, 180, 20), string.Format("State: {0}", _crust_character_machine.currentState));
			_time_scale = GUI.HorizontalSlider(new Rect(20, 70, 180, 20), _time_scale, 0.0f, 1.0f);

			Time.timeScale = _time_scale;
		}
	}
}

