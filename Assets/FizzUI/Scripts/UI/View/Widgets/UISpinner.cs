//
//  UISpinner.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System.Collections;
using System.Collections.Generic;
using FIZZ.UI.Core;
using UnityEngine;

namespace FIZZ.UI.Components {

    public class UISpinner : UIComponent {

		[SerializeField] RectTransform target;
        [SerializeField] float rotationSpeed = 5;
		
        private bool rotationEnabled = false;

        #region Public Methods

        public void ShowSpinner () {
            gameObject.SetActive (true);
            rotationEnabled = true;
        }

        public void HideSpinner () {
            gameObject.SetActive (false);
            rotationEnabled = false;
        }

        void Update () {
            if (rotationEnabled) {
                target.transform.Rotate (0, 0, -rotationSpeed * Time.deltaTime);
            }
        }

        #endregion
    }
}