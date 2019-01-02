//
//  UIOtherRepeatChatCellView.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using UnityEngine;
using UnityEngine.UI;
using Fizz.UI.Components.Models;

namespace Fizz.UI.Components
{
	/// <summary>
	/// User interface other repeat chat cell view.
	/// </summary>
	public class UILeftRepeatChatCellView : UIChatCellView
	{
		/// <summary>
		/// The translate toggle node.
		/// </summary>
		[SerializeField] RectTransform translateToggleNode;
		/// <summary>
		/// The translate toggle.
		/// </summary>
		[SerializeField] Button translateToggle;

		public Action<int> onTranslateTogglePressed;
		private UITranslateToggleButton translateToggleImage;

		protected override void Awake ()
		{
			base.Awake ();
			translateToggleImage = translateToggle.GetComponent<UITranslateToggleButton> ();
		}

		void OnEnable ()
		{
			translateToggle.onClick.AddListener (ToggleTranslateClicked);
		}

		void OnDisable ()
		{
			translateToggle.onClick.RemoveListener (ToggleTranslateClicked);
		}

		#region Public Methods

		/// <summary>
		/// Set the data to populate ChatCellView.
		/// </summary>
		/// <param name="model">FIZZChatMessageAction. Please see <see cref="FIZZ.Actions.IFIZZChatMessageAction"/></param>
		public override void SetData (FizzUIMessage model, bool appTranslationEnabled)
		{
			base.SetData (model, appTranslationEnabled);

			LoadChatMessageAction ();
		}

		#endregion

		#region Methods

		void LoadChatMessageAction () {
			messageLabel.gameObject.SetActive (true);

			
            if (false) {// Utils.ShouldFixRTLMessage (_chatMessageAction)) {
                messageLabel.text = ArabicSupport.ArabicFixer.Fix ( _model.GetActiveMessage ());
			} else {
                messageLabel.text = _model.GetActiveMessage ();
			}

			bool showTranslationToggle = _appTranslationEnabled;
			
			translateToggleImage.Configure (_model.TranslationState);

			translateToggleNode.gameObject.SetActive (showTranslationToggle);
		}

		void ToggleTranslateClicked ()
		{
            _model.ToggleTranslationState();
            messageLabel.text = _model.GetActiveMessage();
            onTranslateTogglePressed.Invoke(rowNumber);
			// translateToggleImage.Toggle ();
		}

		#endregion
	}
}