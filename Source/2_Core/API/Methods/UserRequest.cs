using System.IO;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class UserRequest : PersistentSingletonRequestHandler<UserRequest, User> {
        // /user
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/user";

        public static void SendRequest() {
            var requestDescriptor = new AuthenticatedJsonGetRequestDescriptor(Endpoint);
            Instance.Send(requestDescriptor);
        }

        public class AuthenticatedJsonGetRequestDescriptor : IWebRequestDescriptor<User> {
            private readonly string _url;

            public AuthenticatedJsonGetRequestDescriptor(string url) {
                _url = url;
            }

            public UnityWebRequest CreateWebRequest() {
                var request = UnityWebRequest.Get(_url);

                // Load cookies
                string cookieFile = Authentication.GetCookieFile();
                if (File.Exists(cookieFile)) {
                    string cookies = File.ReadAllText(cookieFile);
                    request.SetRequestHeader("Cookie", cookies);
                }

                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
                request.SetRequestHeader("Accept", "*/*");
                return request;
            }

            public User ParseResponse(UnityWebRequest request) {
                return JsonConvert.DeserializeObject<User>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
            }
        }
    }
}