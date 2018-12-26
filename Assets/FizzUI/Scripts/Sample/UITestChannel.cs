using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FIZZ.UI.Demo 
{
	public class UITestChannel : MonoBehaviour 
	{
		[SerializeField] InputField channelIdInput;
		[SerializeField] InputField channelNameInput;

		[SerializeField] Button removeButton;

		void OnEnable ()
		{
			removeButton.onClick.AddListener (OnRemoveButtonPressed);
		}

		void OnDisable ()
		{
			removeButton.onClick.RemoveListener (OnRemoveButtonPressed);
		}

		void OnRemoveButtonPressed ()
		{
			Destroy (this.gameObject);
		}

		public void PopulateData (string id, string name)
		{
			channelIdInput.text = id;
			channelNameInput.text = name;
		}

		public void SetRemoveButtonActive (bool active)
		{
			removeButton.gameObject.SetActive (active);
		}

		public TestChannelMeta Build ()
		{
			return new TestChannelMeta () 
			{ 
				channelId= channelIdInput.text, 
				channelName = channelNameInput.text 
			};
		}
	}

	public class TestChannelMeta {
		public string channelId;
		public string channelName;
	}
}