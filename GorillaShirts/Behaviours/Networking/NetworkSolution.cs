using ExitGames.Client.Photon;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Behaviours.Networking;

internal abstract class NetworkSolution : MonoBehaviourPunCallbacks
{
    public static NetworkSolution Instance { get; set; }

    public abstract bool TransferOnlyInRooms { get; }

    public Action<NetPlayer, Hashtable> OnPlayerPropertiesChanged;

    private readonly Hashtable _properties = [];
    private bool _isPropertiesReady;
    private float _propertySetTimer;

    private Player[] playerArray = [];
    private int roomGeneration;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        _propertySetTimer = Mathf.Max(_propertySetTimer - Time.unscaledDeltaTime, 0f);

        if (TransferOnlyInRooms && !PhotonNetwork.InRoom) return;

        if (_isPropertiesReady && _propertySetTimer <= 0)
        {
            _isPropertiesReady = false;
            _propertySetTimer = Constants.NetworkRaiseInterval;

            try
            {
                SendProperties(_properties, [.. from player in playerArray where IsCompatiblePlayer(player) select player]);
            }
            catch (Exception ex)
            {
                Logging.Fatal("NetworkSolution failed to send player properties");
                Logging.Error(ex);
            }
        }
    }

    public void SetProperty(string key, object value)
    {
        if (_properties.ContainsKey(key)) _properties[key] = value;
        else _properties.Add(key, value);

        _isPropertiesReady = !TransferOnlyInRooms || PhotonNetwork.InRoom || _isPropertiesReady;
    }

    public void NotifyPropertiesRecieved(Player player, Hashtable properties)
    {
        Logging.Message($"{player}: {string.Join(", ", properties)}");
        OnPlayerPropertiesChanged?.Invoke(player, properties);
    }

    public sealed override async void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playerArray = PhotonNetwork.PlayerListOthers;
        int generation = ++roomGeneration;

        if (TransferOnlyInRooms)
        {
            await Task.Delay(Math.Max(PhotonNetwork.GetPing(), 50));
            if (this != null && generation == roomGeneration && PhotonNetwork.InRoom)
                _isPropertiesReady = true;
        }
    }

    public sealed override void OnLeftRoom()
    {
        base.OnLeftRoom();
        roomGeneration++;
        playerArray = [];
        _isPropertiesReady = false;
    }

    public sealed override async void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        playerArray = PhotonNetwork.PlayerListOthers;

        if (TransferOnlyInRooms)
        {
            int generation = roomGeneration;
            // Stop waiting if the player leaves or their rig never appears.
            for (int attempt = 0; attempt < 100; attempt++)
            {
                if (this == null || generation != roomGeneration || !PhotonNetwork.InRoom ||
                    PhotonNetwork.CurrentRoom.GetPlayer(newPlayer.ActorNumber) != newPlayer) return;
                if (VRRigCache.isInitialized && VRRigCache.rigsInUse.Any(player => player.Key.ActorNumber == newPlayer.ActorNumber)) break;
                await Task.Delay(100);
                if (attempt == 99) return;
            }

            try
            {
                SendProperties(_properties, [newPlayer]);
            }
            catch (Exception ex)
            {
                Logging.Fatal("NetworkSolution failed to send player properties");
                Logging.Error(ex);
            }
        }
    }

    public sealed override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playerArray = PhotonNetwork.PlayerListOthers;
    }

    public abstract void SendProperties(Hashtable properties, Player[] targetPlayers);

    public abstract bool IsCompatiblePlayer(Player player);
}
