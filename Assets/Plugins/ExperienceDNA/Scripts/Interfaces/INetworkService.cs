using MICT.eDNA.Models;
using Photon.Realtime;
using System;

namespace MICT.eDNA.Interfaces
{
    public interface INetworkService
    {
        event EventHandler OnEntered;
        event EventHandler OnExited;
        event EventHandler<Player> OnOtherEntered;
        event EventHandler<Player> OnOtherExited;
        NetworkState CurrentState { get; }
        void LoadRoom();
        void Leave();
        void Connect(string playerName = "", string groupName = "");
        void SendNetworkCall<T>(T intrface, string methodName, object variable = null);
        string GetGroupName();
    }
}

namespace MICT.eDNA.Models {
    public enum NetworkState
    {
        Unknown,
        EnteredNetwork,
        ExitedNetwork
    }
}
