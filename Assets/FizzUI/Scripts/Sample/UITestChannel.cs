using UnityEngine;
using UnityEngine.UI;

namespace Fizz.UI.Demo 
{
	public class UITestChannel : MonoBehaviour 
	{
		[SerializeField] InputField channelNameInput;

		[SerializeField] Button removeButton;

        private TestChannelMeta meta;

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
            FizzService.Instance.RemoveChannel(meta.channelId);
            Destroy (this.gameObject);
		}

        public void PopulateData(TestChannelMeta meta)
        {
            this.meta = meta;

            channelNameInput.text = meta.channelName;
        }

		public void SetRemoveButtonActive (bool active)
		{
			removeButton.gameObject.SetActive (active);
		}

        public TestChannelMeta GetMeta()
        {
            return meta;
        }
	}

	public class TestChannelMeta {
		public string channelId;
		public string channelName;
	}
}