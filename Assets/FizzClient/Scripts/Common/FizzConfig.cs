﻿#define FIZZ_CONFIG_PROD

namespace Fizz.Common
{
    public class FizzConfig
    {
        // TODO: manage the following using config management
#if FIZZ_CONFIG_PROD
        public static readonly string API_PROTOCOL = "https";
        public static readonly string API_ENDPOINT = "api.fizz.io";

        public static bool MQTT_USE_TLS = true;
        public static string MQTT_HOST_ENDPOINT = "mqtt.fizz.io";
#else
        public static readonly string API_PROTOCOL = "http";
        public static readonly string API_ENDPOINT = "localhost:3000";

        public static bool MQTT_USE_TLS = false;
        public static string MQTT_HOST_ENDPOINT = "localhost";
#endif

        public static readonly string API_VERSION = "v1";
        public static readonly string API_BASE_URL = string.Format("{0}://{1}/{2}", API_PROTOCOL, API_ENDPOINT, API_VERSION);
        public static readonly string API_PATH_SESSIONS = "/sessions";
        public static readonly string API_PATH_EVENTS = "/events";
        public static readonly string API_PATH_MESSAGES = "/channels/{0}/messages";
        public static readonly string API_PATH_SUBSCRIBERS = "/channels/{0}/subscribers";
        public static readonly string API_HEADER_SESSION_TOKEN = "Session-Token"; 
    }
}
