using System.Collections;
using System.Collections.Generic;
using Fizz.Common.Json;
using Fizz.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Demo {
	public class UITestConfiguration : MonoBehaviour {

		[SerializeField] InputField userIdInput;
		[SerializeField] InputField userNameInput;
		[SerializeField] Toggle translationToggle;

		[SerializeField] Button channelAddButton;
		[SerializeField] UITestChannel dafaultChannel;

		[SerializeField] Button launchButton;
		[SerializeField] Button connectButton;
		[SerializeField] Button disconnectButton;
		[SerializeField] GameObject connectingAnim;

		void Awake ()
		{
			launchButton.interactable = false;
			connectButton.interactable = true;
			disconnectButton.interactable = false;
			channelAddButton.interactable = true;

			DeserializeAndLoad ();
		}

		void OnEnable () 
		{
			channelAddButton.onClick.AddListener (OnChannelAddButtonPressed);
			launchButton.onClick.AddListener (OnLaunchButtonPressed);
			connectButton.onClick.AddListener (OnConnectButtonPressed);
			disconnectButton.onClick.AddListener (OnDisconnectButtonPressed);
		}

		void OnDisable () 
		{
			channelAddButton.onClick.RemoveListener (OnChannelAddButtonPressed);
			launchButton.onClick.RemoveListener (OnLaunchButtonPressed);
			connectButton.onClick.RemoveListener (OnConnectButtonPressed);
			disconnectButton.onClick.RemoveListener (OnDisconnectButtonPressed);
		}

		void OnChannelAddButtonPressed ()
		{
			UITestChannel testChannel = Instantiate (dafaultChannel);
			testChannel.transform.SetParent (dafaultChannel.transform.parent);
			testChannel.transform.localScale = Vector3.one;
			testChannel.transform.SetAsLastSibling ();
			testChannel.SetRemoveButtonActive (true);
		}

		void OnLaunchButtonPressed ()
		{
			FizzUI.Instance.LaunchFizz ();
		}

		void OnConnectButtonPressed ()
		{
			connectButton.interactable = false;
			TestConfigurationMeta testMeta = BuildMeta ();
			SerializeAndSave (testMeta);
			connectingAnim.gameObject.SetActive (true);
			FizzService.Instance.Open (
				testMeta.userId, 
				testMeta.userName,
				Utils.GetSystemLanguage (),
				translationToggle.isOn,
				testMeta.Channels,
				(bool success) => {
					launchButton.interactable = success;
					disconnectButton.interactable = success;
					connectButton.interactable = !success;
					channelAddButton.interactable = !success;
					connectingAnim.gameObject.SetActive (false);
				}
			);
        }

		void OnDisconnectButtonPressed ()
		{
			FizzService.Instance.Close ();

			launchButton.interactable = false;
			disconnectButton.interactable = false;
			connectButton.interactable = true;
			channelAddButton.interactable = true;
		}

		TestConfigurationMeta BuildMeta () 
		{
			List<TestChannelMeta> channelMeta = new List<TestChannelMeta> ();
			UITestChannel[] testChannelComponent = dafaultChannel.transform.parent.GetComponentsInChildren<UITestChannel> ();
			foreach (UITestChannel channel in testChannelComponent)
			{
				channelMeta.Add (channel.Build ());
			}

			return new TestConfigurationMeta ()
			{
				userId = userIdInput.text,
				userName = userNameInput.text,
				Channels = channelMeta
			};
		}

		void SerializeAndSave (TestConfigurationMeta meta)
		{
			JSONClass json = new JSONClass();
			json.Add ("userId", new JSONData (meta.userId));
			json.Add ("userName", new JSONData (meta.userName));
			json.Add ("translation", new JSONData (translationToggle.isOn));

			JSONArray array = new JSONArray();
			foreach (TestChannelMeta metaChannel in meta.Channels)
			{
				JSONClass channelNode = new JSONClass();
				channelNode.Add ("channelId", new JSONData (metaChannel.channelId));
				channelNode.Add ("channelName", new JSONData (metaChannel.channelName));

				array.Add (channelNode);
			}

			json.Add ("channels", array);

			PlayerPrefs.SetString ("fizz-meta", json.ToString ());
			PlayerPrefs.Save ();
		}

		void DeserializeAndLoad ()
		{
			string json = PlayerPrefs.GetString ("fizz-meta", GetDefaultUser ());

			JSONNode jsonClass = JSONClass.Parse (json);

			userIdInput.text = jsonClass["userId"].Value;
			userNameInput.text = jsonClass["userName"].Value;
			translationToggle.isOn = jsonClass["translation"].AsBool;

			JSONArray channels = jsonClass["channels"].AsArray;
			int count = channels.Count;
			
			for (int i = 1; i< count; i++) {
				OnChannelAddButtonPressed ();
			}

			int index = 0;
			foreach (JSONNode node in channels)
			{
				string channelId = node["channelId"].Value;
				string channelName = node["channelName"].Value;

				UITestChannel testChannel = dafaultChannel.transform.parent.GetChild (index).GetComponent<UITestChannel> ();
				testChannel.PopulateData (channelId, channelName);

				index ++;
			}
		}

		string GetDefaultUser ()
		{
			return "{"
			+	 "\"userId\":\""+ System.Guid.NewGuid () +"\","
			+	 "\"userName\":\"User\","
			+ 	 "\"translation\":true,"
			+	 "\"channels\":["
			+		"{"
			+			"\"channelId\":\"global-channel\","
			+			"\"channelName\":\"Global\""
			+	 	"}"
			+	 "]"
			+ "}";
		}
	}

	public class TestConfigurationMeta {
		public string userId;
		public string userName;
		public List<TestChannelMeta> Channels;
	}
}