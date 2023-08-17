using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BS_Utils.Gameplay;
using Oculus.Platform;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace BeatLeader.API {
    internal static class Authentication {
        #region AuthPlatform

        public static AuthPlatform Platform { get; private set; }

        public static void SetPlatform(AuthPlatform platform) {
            Platform = platform;
        }

        public enum AuthPlatform {
            Undefined,
            Steam,
            OculusPC
        }

        #endregion

        #region Ticket

        public static Task<string> PlatformTicket() {
            return Platform switch {
                AuthPlatform.Steam => SteamTicket(),
                AuthPlatform.OculusPC => OculusTicket(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task<string> SteamTicket() {
            await GetUserInfo.GetUserAsync();
            return (await new SteamPlatformUserModel().GetUserAuthToken()).token;
        }

        public static async Task<string> OculusTicket() {
            await GetUserInfo.GetUserAsync();
            TaskCompletionSource<string> tcs = new();
            Users.GetAccessToken().OnComplete(delegate(Message<string> message) { tcs.TrySetResult(message.IsError ? null : message.Data); });
            return await tcs.Task;
        }

        #endregion

        #region Login

        private static bool _locked;
        private static bool _signedIn;

        public static void ResetLogin() {
            UnityWebRequest.ClearCookieCache(new Uri(BLConstants.BEATLEADER_API_URL));
            _signedIn = false;
        }

        public static IEnumerator EnsureLoggedIn(Action onSuccess, Action<string> onFail) {
            while (true) {
                if (!_locked) {
                    _locked = true;
                    break;
                }

                yield return new WaitUntil(() => !_locked);
            }

            try {
                if (_signedIn) {
                    onSuccess();
                    yield break;
                }

                yield return DoLogin(() => {
                    _signedIn = true;
                    onSuccess();
                }, onFail);
            } finally {
                _locked = false;
            }
        }
        public static string GenerateRandomDigits(int length) {
            var random = new System.Random();
            var stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++) {
                stringBuilder.Append(random.Next(0, 10));  // Random digit between 0 and 9
            }
            return stringBuilder.ToString();
        }

        private static IEnumerator DoLogin(Action onSuccess, Action<string> onFail) {
            if (!TryGetPlatformProvider(Platform, out var provider)) {
                Plugin.Log.Debug("Login failed! Unknown platform");
                onFail("Unknown platform");
                yield break;
            }
            
            var ticketTask = PlatformTicket();
            yield return new WaitUntil(() => ticketTask.IsCompleted);

            var authToken = ticketTask.Result;
            if (authToken == null) {
                Plugin.Log.Debug("Login failed! No auth token");
                onFail("No auth token");
                yield break;
            }

            var form = new List<IMultipartFormSection> {
                new MultipartFormDataSection("provider", provider),
                new MultipartFormDataSection("returnUrl", "/")
            };

            var request = UnityWebRequest.Post("https://api.beatleader.xyz/signinoculus", form);
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
            request.SetRequestHeader("Accept", "*/*");
            request.SetRequestHeader("Accept-Language", "multiparten-GB,en;q=0.5");
            request.SetRequestHeader("Alt-Used", "api.beatleader.xyz");

            string beatSaberDirectory = UnityEngine.Application.dataPath;
            string userDataPath = Path.Combine(beatSaberDirectory, "..", "UserData");
            string jsonFilePath = Path.Combine(userDataPath, "BLLogInInfo.json");
            JObject userLoginData = JObject.Parse(File.ReadAllText(jsonFilePath));

            string username = userLoginData["username"].ToString();
            string password = userLoginData["password"].ToString();

            var randCode = GenerateRandomDigits(30);
            request.SetRequestHeader("Content-Type", $"multipart/form-data; boundary=---------------------------{randCode}");
            string requestBody =
                $"-----------------------------{randCode}\r\n" +
                "Content-Disposition: form-data; name=\"action\"\r\n" +
                "\r\n" +
                "login\r\n" +
                $"-----------------------------{randCode}\r\n" +
                "Content-Disposition: form-data; name=\"login\"\r\n" +
                "\r\n" +
                $"{username}\r\n" +
                $"-----------------------------{randCode}\r\n" +
                "Content-Disposition: form-data; name=\"password\"\r\n" +
                "\r\n" +
                $"{password}\r\n" +
                $"-----------------------------{randCode}--\r\n"
            ;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            yield return request.SendWebRequest();

            switch (request.responseCode) {
                case 200:
                    Plugin.Log.Info("Login successful!");
                    onSuccess();
                    break;
                case BLConstants.MaintenanceStatus:
                    Plugin.Log.Debug("Login failed! Maintenance");
                    onFail("Maintenance");
                    break;
                default:
                    Plugin.Log.Debug($"Login failed! status: {request.responseCode} error: {request.error}");
                    onFail($"NetworkError: {request.responseCode}");
                    break;
            }
        }

        private static bool TryGetPlatformProvider(AuthPlatform platform, out string provider) {
            switch (platform) {
                case AuthPlatform.Steam:
                    provider = "steamTicket";
                    return true;
                case AuthPlatform.OculusPC:
                    provider = "oculusTicket";
                    return true;
                case AuthPlatform.Undefined:
                default:
                    provider = null;
                    return false;
            }
        }

        #endregion
    }
}