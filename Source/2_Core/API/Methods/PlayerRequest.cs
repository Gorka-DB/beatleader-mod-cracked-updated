using System.IO;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class PlayerRequest : PersistentSingletonRequestHandler<PlayerRequest, Player> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/player/{0}";

        public static void SendRequest(string playerId) {
            var url = string.Format(Endpoint, playerId);
            var requestDescriptor = new JsonGetRequestDescriptor<Player>(url);
            Instance.Send(requestDescriptor);
        }
    }
}