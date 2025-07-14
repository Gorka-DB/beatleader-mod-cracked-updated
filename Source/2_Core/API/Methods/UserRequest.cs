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
            var requestDescriptor = new JsonGetRequestDescriptor<User>(Endpoint);
            Instance.Send(requestDescriptor);
        }
    }
}