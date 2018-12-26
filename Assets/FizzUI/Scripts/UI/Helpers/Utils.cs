//
//  Utils.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System;
using FIZZ.UI.Core;
using System.Collections.Generic;
using UnityEngine;

namespace FIZZ.UI.Components {
    public static class Utils {
        private static Dictionary<string, Color> _userColor = new Dictionary<string, Color> ();

        public static DateTime GetDateTimeToUnixTime (long unixTimeStamp) {
            DateTime dateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds (unixTimeStamp).ToLocalTime ();
            return dateTime;
        }

        public static string GetCurrentUnixTimeStamp () {
            DateTime dateTime = new DateTime (2017, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return Math.Truncate ((DateTime.Now - dateTime).TotalMilliseconds).ToString ();
        }

        public static string GetRemainingTimeToString (long unixTimeStamp) {
            TimeSpan span = new TimeSpan (unixTimeStamp);
            return string.Format ("{0:hh\\:mm\\:ss}", span);
        }

        public static Color GetUserNickColor (string userId) {
            if (_userColor.ContainsKey (userId)) {
                return _userColor[userId];
            } else {
                Color _newColor = GetRandomColor ();
                _userColor.Add (userId, _newColor);
                return _newColor;
            }
        }

        public static string GetDownloadDirectoryPath () {
            return System.IO.Path.Combine (Application.persistentDataPath, "fizzAssets");
        }

        public static string GetFormattedTimeForUnixTimeStamp (long unixTimeStamp, bool todaysTime = true, bool fullDate = false) {
            DateTime dateTime = GetDateTimeToUnixTime (unixTimeStamp);
            string timeFormat = string.Empty;
            if (DateTime.Now.Subtract (dateTime).Days > 6) {
                if (fullDate) {
                    timeFormat = string.Format ("{0:d/MM/yyyy}", dateTime);
                } else {
                    timeFormat = dateTime.Year != DateTime.Now.Year ? string.Format ("{0:dd} {1:MMM} {2:yyyy}", dateTime, dateTime, dateTime) : string.Format ("{0:ddd}, {1:dd} {2:MMM}", dateTime, dateTime, dateTime);
                }
            } else if (dateTime.Day != DateTime.Now.Day) {
                if (dateTime.Day == DateTime.Now.Day - 1) {
                    timeFormat = Registry.localization.GetText ("DateFormat_Yesterday");
                } else if (dateTime.Day >= DateTime.Now.Day - 6) {
                    timeFormat = string.Format ("{0}", dateTime.DayOfWeek);
                } else {
                    timeFormat = string.Format ("{0:ddd}, {1:dd} {2:MMM}", dateTime, dateTime, dateTime);
                }
            } else {
                timeFormat = (todaysTime) ? string.Format ("{0:h:mm tt}", dateTime) : "Today";
            }
            return timeFormat;
        }

        private static UnityEngine.Color GetRandomColor () {
            int colorIndex = UnityEngine.Random.Range (1, 11);
            UnityEngine.Color color = UnityEngine.Color.black;
            switch (colorIndex) {
                case 1:
                    color = new UnityEngine.Color (92 / 255.0f, 83 / 255.0f, 214 / 255.0f, 1.0f);
                    break;
                case 2:
                    color = new UnityEngine.Color (170 / 255.0f, 91 / 255.0f, 196 / 255.0f, 1.0f);
                    break;
                case 3:
                    color = new UnityEngine.Color (37 / 255.0f, 177 / 255.0f, 41 / 255.0f, 1.0f);
                    break;
                case 4:
                    color = new UnityEngine.Color (254 / 255.0f, 150 / 255.0f, 1 / 255.0f, 1.0f);
                    break;
                case 5:
                    color = new UnityEngine.Color (241 / 255.0f, 90 / 255.0f, 43 / 255.0f, 1.0f);
                    break;
                case 6:
                    color = new UnityEngine.Color (55 / 255.0f, 176 / 255.0f, 216 / 255.0f, 1.0f);
                    break;
                case 7:
                    color = new UnityEngine.Color (00 / 255.0f, 178 / 255.0f, 130 / 255.0f, 1.0f);
                    break;
                case 8:
                    color = new UnityEngine.Color (216 / 255.0f, 69 / 255.0f, 162 / 255.0f, 1.0f);
                    break;
                case 9:
                    color = new UnityEngine.Color (189 / 255.0f, 200 / 255.0f, 18 / 255.0f, 1.0f);
                    break;
                case 10:
                    color = new UnityEngine.Color (12 / 255.0f, 135 / 255.0f, 113 / 255.0f, 1.0f);
                    break;
                default:
                    color = new UnityEngine.Color (12 / 255.0f, 135 / 255.0f, 113 / 255.0f, 1.0f);
                    break;
            }
            return color;
        }

        private static bool IsJoiner (char ch) {
            if (char.IsHighSurrogate (ch)) {
                return false;
            }

            const int UNICODE_VALUE_JOINER = 0x200D;

            ushort value = (ushort) ch;
            if (value == UNICODE_VALUE_JOINER) {
                return true;
            }

            return false;
        }

        private static bool IsVariant (char ch) {
            if (char.IsHighSurrogate (ch)) {
                return false;
            }

            const int UNICODE_RANGE_START_VARIANT = 0xFE00;
            const int UNICODE_RANGE_END_VARIANT = 0xFE0F;

            ushort value = (ushort) ch;
            if (value >= UNICODE_RANGE_START_VARIANT &&
                value <= UNICODE_RANGE_END_VARIANT) {
                return true;
            }

            return false;
        }

        private static int CalcAuxCharCount (int idx, string text) {
            bool search = true;
            int aci = idx;
            int incr = 0;

            while (search && aci < text.Length) {
                search = false;
                if (IsJoiner (text[aci])) {
                    search = true;
                    incr++;
                }
                if (IsVariant (text[aci])) {
                    search = true;
                    incr++;
                }
                aci++;
            }

            return incr;
        }

        public static T LoadPrefabs<T> (string prefabName) where T : UIComponent {
            T prefab = Resources.Load<T> ("CustomPrefabs/" + prefabName);
            if (prefab == null) {
                prefab = Resources.Load<T> ("Prefabs/" + prefabName);
            }
            return prefab;
        }

        public static Sprite LoadSprite (string spriteName) {
            Sprite sprite = Resources.Load<Sprite> ("CustomSprites/" + spriteName);
            if (sprite == null) {
                sprite = Resources.Load<Sprite> ("Sprites/" + spriteName);
            }
            return sprite;
        }

        public static Font LoadFont (string fontName) {
            Font font = Resources.Load<Font> ("CustomFonts/" + fontName);
            if (font == null) {
                font = Resources.Load<Font> ("Fonts/" + fontName);
            }
            return font;
        }
    }
}