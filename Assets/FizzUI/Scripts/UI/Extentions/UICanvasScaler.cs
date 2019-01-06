//
//  UICanvasScaler.cs
//
//  Copyright (c) 2016 Fizz Inc
//

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fizz.UI.Components {
    public class UICanvasScaler : UIBehaviour {

#if UNITY_IOS
        [DllImport("__Internal")]
        private extern static void _FizzGetSafeAreaImpl(out float x, out float y, out float w, out float h);
#endif

        public UIOrientaion supportedOrientation = UIOrientaion.Portrait;
        public DeviceOrientationChangeEvent OnDeviceOrientationChanged;

        private CanvasScaler _canvasScaler;
        private Vector2 _currentResolution = new Vector2 (640, 1136);
        private Rect _safeRect = new Rect (0, 0, Screen.width, Screen.height);
        private DeviceOrientation _deviceOrientation = DeviceOrientation.Portrait;

        public Vector2 CurrentResolution {
            get { return GetComponent<RectTransform>().sizeDelta; }
        }

        public float ReferenceResolutionRatio {
            get { return _currentResolution.y / Screen.height; }
        }

        public DeviceOrientation Orientation {
            get { return _deviceOrientation; }
        }

        public void ApplySafeArea(RectTransform panel)
        {
            var anchorMin = _safeRect.position;
            var anchorMax = _safeRect.position + _safeRect.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }

        protected override void Awake () {
            _canvasScaler = GetComponent<CanvasScaler> ();
            if (supportedOrientation == UIOrientaion.Portrait)
                _deviceOrientation = DeviceOrientation.Portrait;
            else if (supportedOrientation == UIOrientaion.Landscape)
                _deviceOrientation = DeviceOrientation.LandscapeLeft;
            
            GetSafeArea ();
        }

        protected override void Start () {
            UpdateCanvasScale ();
        }

        protected override void OnRectTransformDimensionsChange () {
            if (IsOrientationSupported (Input.deviceOrientation) && _deviceOrientation != Input.deviceOrientation) {
                _deviceOrientation = Input.deviceOrientation;
                UpdateCanvasScale ();
                // Invoke OnDeviceOrientationChange
                if (OnDeviceOrientationChanged != null)
                    OnDeviceOrientationChanged.Invoke (_deviceOrientation);
            }

            GetSafeArea ();
        }

        private void UpdateCanvasScale () {
            if (_canvasScaler != null) {
                float aspectRatio = Screen.height / (float) Screen.width;
                if (_deviceOrientation == DeviceOrientation.LandscapeLeft || _deviceOrientation == DeviceOrientation.LandscapeRight) {
                    if (aspectRatio >= 0.75f) {
                        _currentResolution = new Vector2 (1024, 768);
                    } else if (aspectRatio >= 0.66f) {
                        _currentResolution = new Vector2 (960, 640);
                    } else if (aspectRatio >= 0.51) {
                        _currentResolution = new Vector2 (1136, 640);
                    } else {
                        _currentResolution = new Vector2 (1280, 640);
                    }
                } else if (_deviceOrientation == DeviceOrientation.Portrait || _deviceOrientation == DeviceOrientation.PortraitUpsideDown) {
                    if (aspectRatio > 1.9f) {
                        _currentResolution = new Vector2 (640, 1280);
                    } else if (aspectRatio > 1.5f) {
                        _currentResolution = new Vector2 (640, 1136);
                    } else if (aspectRatio > 1.4f) {
                        _currentResolution = new Vector2 (640, 960);
                    } else {
                        _currentResolution = new Vector2 (768, 1024);
                    }
                }
                _canvasScaler.referenceResolution = _currentResolution;
            }
        }

        private bool IsOrientationSupported (DeviceOrientation orientation) {
            bool isSupported = false;
            switch (orientation) {
                case DeviceOrientation.LandscapeLeft:
                case DeviceOrientation.LandscapeRight:
                    isSupported = (supportedOrientation == UIOrientaion.Landscape);
                    break;
                case DeviceOrientation.Portrait:
                case DeviceOrientation.PortraitUpsideDown:
                    isSupported = (supportedOrientation == UIOrientaion.Portrait);
                    break;
                case DeviceOrientation.Unknown:
                case DeviceOrientation.FaceDown:
                case DeviceOrientation.FaceUp:
                    isSupported = false;
                    break;
                default:
                    isSupported = false;
                    break;
            }
            return isSupported;
        }

        private void GetSafeArea () {
             _safeRect = new Rect (0, 0, Screen.width, Screen.height);
		#if UNITY_2017_2_OR_NEWER
            _safeRect = Screen.safeArea;
		#else
		 	_safeRect = GetSafeAreaInternal(); 
		#endif
        }

        private Rect GetSafeAreaInternal()
        {
            float x, y, w, h;
    #if UNITY_IOS && !UNITY_EDITOR
            _FizzGetSafeAreaImpl(out x, out y, out w, out h);
    #else
            x = 0;
            y = 0;
            w = Screen.width;
            h = Screen.height;
    #endif
            return new Rect(x, y, w, h);
        }

        [Serializable]
        public class DeviceOrientationChangeEvent : UnityEvent<DeviceOrientation> {

        }

        public enum UIOrientaion
        {
            Portrait,
            Landscape
        }
    }
}