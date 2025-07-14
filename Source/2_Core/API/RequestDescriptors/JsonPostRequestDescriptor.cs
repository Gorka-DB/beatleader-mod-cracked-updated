using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class JsonPostRequestDescriptor<T> : IWebRequestDescriptor<T> {
        private readonly string _url;

        private readonly string? _body;
        private readonly List<IMultipartFormSection>? _form;

        public JsonPostRequestDescriptor(string url, string body) {
            _url = url;
            _body = body;
        }

        public JsonPostRequestDescriptor(string url) {
            _url = url;
            _body = "";
        }

        public JsonPostRequestDescriptor(string url, List<IMultipartFormSection> form = null) {
            _url = url;
            _form = form;
        }

        public UnityWebRequest CreateWebRequest() {
            var request = _form != null 
                ? UnityWebRequest.Post(_url, _form) 
                : UnityWebRequest.Post(_url, _body);

            // Load cookies
            string cookieFile = Authentication.GetCookieFile();
            if (File.Exists(cookieFile)) {
                request.SetRequestHeader("Cookie", File.ReadAllText(cookieFile));
            }

            // Ensure consistent headers
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
            request.SetRequestHeader("Accept", "*/*");

            return request;
        }

        public T ParseResponse(UnityWebRequest request) {
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
        }
    }
}