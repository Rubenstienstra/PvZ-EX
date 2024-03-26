using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour {
	#region Singleton class: WorldTimeAPI

	public static TimeManager Instance;

	void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		} else {
			Destroy(this.gameObject);
		}
	}

	#endregion

	//json container
	struct TimeData {
		//public string client_ip;
		//...
		public string datetime;
		//..
	}

	const string API_URL = "https://worldtimeapi.org/api/ip";

	[HideInInspector] public bool IsTimeLodaed = false;

	public static DateTime _currentDateTime = DateTime.Now;

	void Start() {
		StartCoroutine(GetRealDateTimeFromAPI());
	}

	public static DateTime GetCurrentDateTime() {
		//here we don't need to get the datetime from the server again
		// just add elapsed time since the game start to _currentDateTime

		return _currentDateTime.AddSeconds(Time.realtimeSinceStartup);
	}

	IEnumerator GetRealDateTimeFromAPI() {
		UnityWebRequest webRequest = UnityWebRequest.Get(API_URL);
		Debug.Log("getting real datetime...");

		yield return webRequest.SendWebRequest();

		if (webRequest.isNetworkError || webRequest.isHttpError) {
			//error
			Debug.Log("Error: " + webRequest.error);

		} else {
			//success
			TimeData timeData = JsonUtility.FromJson<TimeData>(webRequest.downloadHandler.text);
			//timeData.datetime value is : 2020-08-14T15:54:04+01:00

			_currentDateTime = ParseDateTime(timeData.datetime);
			IsTimeLodaed = true;

			Debug.Log("Success.");
		}
	}
	//datetime format => 2020-08-14T15:54:04+01:00
	DateTime ParseDateTime(string datetime) {
		//match 0000-00-00
		string date = Regex.Match(datetime, @"^\d{4}-\d{2}-\d{2}").Value;

		//match 00:00:00
		string time = Regex.Match(datetime, @"\d{2}:\d{2}:\d{2}").Value;

		return DateTime.Parse(string.Format("{0} {1}", date, time));
	}
}
