using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class JsonGetRequestDescriptor<T> : IWebRequestDescriptor<T> {
        private readonly string _url;

        public JsonGetRequestDescriptor(string url) {
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

            // Standard headers
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
            request.SetRequestHeader("Accept", "*/*");
            return request;
        }

        public T ParseResponse(UnityWebRequest request) {
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
        }
    }
}