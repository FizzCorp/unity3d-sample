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

        [SerializeField] InputField channelIdInput;
        [SerializeField] InputField channelNameInput;
        [SerializeField] UITestChannel dafaultChannel;
        [SerializeField] Button channelAddButton;
        [SerializeField] ScrollRect channelScrollRect;

		[SerializeField] Button launchButton;
		[SerializeField] Button connectButton;
		[SerializeField] Button disconnectButton;
		[SerializeField] GameObject connectingAnim;

		void Awake ()
		{
			launchButton.interactable = false;
			connectButton.interactable = true;
			disconnectButton.interactable = false;

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
        
		void OnLaunchButtonPressed ()
		{
			FizzUI.Instance.LaunchFizz ();
		}

		void OnConnectButtonPressed ()
		{
			connectButton.interactable = false;
			TestConfigurationMeta testMeta = BuildMeta ();
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
					connectingAnim.gameObject.SetActive (false);
				}
			);
			SerializeAndSave (testMeta);
        }

		void OnDisconnectButtonPressed ()
		{
			FizzService.Instance.Close ();

			launchButton.interactable = false;
			disconnectButton.interactable = false;
			connectButton.interactable = true;
		}

        void OnChannelAddButtonPressed()
        {
            if (string.IsNullOrEmpty(channelIdInput.text) || string.IsNullOrEmpty(channelNameInput.text))
                return;

            UITestChannel channel = CreateTestChannel(channelIdInput.text, channelNameInput.text);
            FizzService.Instance.AddChannel(channel.GetMeta());

            SerializeAndSave(BuildMeta());
        }

        UITestChannel CreateTestChannel(string channeId, string channelName)
        {
            TestChannelMeta meta = new TestChannelMeta()
            {
                channelId = channeId,
                channelName = channelName
            };

            UITestChannel testChannel = Instantiate(dafaultChannel);
            testChannel.gameObject.SetActive(true);
            testChannel.transform.SetParent(channelScrollRect.content);
            testChannel.transform.localScale = Vector3.one;
            testChannel.transform.SetAsLastSibling();
            testChannel.SetRemoveButtonActive(true);
            testChannel.PopulateData(meta);

            return testChannel;
        }

        TestConfigurationMeta BuildMeta () 
		{
			List<TestChannelMeta> channelMeta = new List<TestChannelMeta> ();
            UITestChannel[] testChannelComponent = channelScrollRect.content.GetComponentsInChildren<UITestChannel>();
            foreach (UITestChannel channel in testChannelComponent)
			{
				channelMeta.Add (channel.GetMeta ());
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

			PlayerPrefs.SetString ("fizz-meta-143", json.ToString ());
			PlayerPrefs.Save ();
		}

		void DeserializeAndLoad ()
		{
			string json = PlayerPrefs.GetString ("fizz-meta-143", GetDefaultUser ());

			JSONNode jsonClass = JSONClass.Parse (json);

			userIdInput.text = jsonClass["userId"].Value;
			userNameInput.text = jsonClass["userName"].Value;
			translationToggle.isOn = jsonClass["translation"].AsBool;

			JSONArray channels = jsonClass["channels"].AsArray;
			int count = channels.Count;

			int index = 0;
			foreach (JSONNode node in channels)
			{
				string channelId = node["channelId"].Value;
				string channelName = node["channelName"].Value;

                UITestChannel testChannel = CreateTestChannel(channelId, channelName);

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