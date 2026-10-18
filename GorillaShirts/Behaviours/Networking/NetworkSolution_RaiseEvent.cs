using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace GorillaShirts.Behaviours.Networking;

internal class NetworkSolution_RaiseEvent : NetworkSolution
{
    public override bool TransferOnlyInRooms => true;

    private readonly byte eventCode = 176;

    private readonly int id = StaticHash.Compute("GorillaShirts".GetStaticHash());

    public void Awake()
    {
        Instance = this;

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new() { { Constants.NetworkPropertyKey, Constants.Version } });
    }

    public void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        if (Instance == this) Instance = null;
    }

    public override bool IsCompatiblePlayer(Player player) => player != null;

    public override void SendProperties(Hashtable properties, Player[] targetPlayers)
    {
        if (!PhotonNetwork.InRoom || targetPlayers == null || targetPlayers.Length == 0) return;

        object[] content = [id, properties];

        RaiseEventOptions raiseEventOptions = new()
        {
            TargetActors = [.. from player in targetPlayers select player.ActorNumber]
        };

        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEvent(EventData data)
    {
        if (data.Code != eventCode || !PhotonNetwork.InRoom) return;
        if (data.CustomData is not object[] eventData || eventData.Length < 2 ||
            eventData[0] is not int eventId || eventId != id || eventData[1] is not Hashtable) return;
        if (NetworkSystem.Instance == null || VRRigCache.Instance == null) return;

        Player player = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
        NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(data.Sender);
        if (player == null || player.IsLocal || netPlayer == null || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || playerRig == null || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer)) return;

        if (eventData[1] is Hashtable properties)
        {
            if (!networkedPlayer.IsShirtUser)
            {
                Logging.Message($"Player has GorillaShirts: {netPlayer.NickName}");
                networkedPlayer.IsShirtUser = true;
            }

            Logging.Message($"{netPlayer.NickName}: {string.Join(", ", properties)}");

            networkedPlayer.OnPlayerPropertyChanged(properties);
            NotifyPropertiesRecieved(player, properties);
            return;
        }
    }
}
