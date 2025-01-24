using MICT.eDNA.Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MICT.eDNA.View
{
    public class LobbyView : MonoBehaviour
    {
        [SerializeField]
        protected Button _startExperienceButton;
        [SerializeField]
        protected CanvasGroup _lobbyPageCanvasGroup;
        [SerializeField]
        protected CanvasGroup _loadingLobbyCanvasGroup;
        [SerializeField]
        protected GameObject _prefab;
        [SerializeField]
        protected Transform _parent;
        private List<GameObject> _createdChildren = new List<GameObject>();

        void Start()
        {
            if (_lobbyPageCanvasGroup != null)
            {
                _lobbyPageCanvasGroup.alpha = 0;
                _lobbyPageCanvasGroup.interactable = false;
                _lobbyPageCanvasGroup.blocksRaycasts = false;
            }
            if (_loadingLobbyCanvasGroup != null)
            {
                _loadingLobbyCanvasGroup.alpha = 1;
                _loadingLobbyCanvasGroup.interactable = true;
                _loadingLobbyCanvasGroup.blocksRaycasts = true;
            }
            _startExperienceButton.interactable = false;
            _startExperienceButton.onClick.AddListener(UserClickedButton);
            if (ServiceLocator.NetworkService.CurrentState == Models.NetworkState.EnteredNetwork)
            {
                NetworkService_OnEntered(this, null);
            }
            ServiceLocator.NetworkService.OnOtherEntered += NetworkService_OnOtherEntered;
            ServiceLocator.NetworkService.OnEntered += NetworkService_OnEntered;
        }

        private void OnDestroy()
        {
            DestroyInstances();
            ServiceLocator.NetworkService.OnOtherEntered -= NetworkService_OnOtherEntered;
            ServiceLocator.NetworkService.OnEntered -= NetworkService_OnEntered;
        }

        private void UserClickedButton()
        {
            _startExperienceButton.interactable = false;
            ServiceLocator.NetworkService.LoadRoom();

        }
        private void NetworkService_OnEntered(object sender, System.EventArgs e)
        {
            if (_loadingLobbyCanvasGroup != null)
            {
                _loadingLobbyCanvasGroup.alpha = 0;
                _loadingLobbyCanvasGroup.interactable = false;
                _loadingLobbyCanvasGroup.blocksRaycasts = false;
            }
            if (_lobbyPageCanvasGroup != null)
            {
                _lobbyPageCanvasGroup.alpha = 1;
                _lobbyPageCanvasGroup.interactable = true;
                _lobbyPageCanvasGroup.blocksRaycasts = true;
            }
            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                _startExperienceButton.interactable = true;
            }
            UpdateNames(GetNamesFromPlayers(Photon.Pun.PhotonNetwork.PlayerList));
        }

        private void NetworkService_OnOtherEntered(object sender, Photon.Realtime.Player e)
        {
            UpdateNames(GetNamesFromPlayers(Photon.Pun.PhotonNetwork.PlayerList));
        }

        private List<string> GetNamesFromPlayers(Photon.Realtime.Player[] players)
        {
            var names = new List<string>();
            foreach (var player in players)
            {
                if (player == Photon.Pun.PhotonNetwork.LocalPlayer)
                {
                    names.Add(player.NickName + " (me)");
                }
                else
                {
                    names.Add(player.NickName);
                }
            }
            return names;
        }

        private void UpdateNames(List<string> names)
        {
            DestroyInstances();
            if (names?.Count > 0)
            {
                foreach (var name in names)
                {
                    var go = GameObject.Instantiate(_prefab, _parent);
                    go.GetComponent<TMP_Text>()?.SetText(name);
                    _createdChildren.Add(go);
                }
            }
        }

        protected void DestroyInstances()
        {
            if (_createdChildren?.Count > 0)
            {
                for (int i = _createdChildren.Count - 1; i >= 0; i--)
                {
                    Destroy(_createdChildren[i]);
                }
                _createdChildren.Clear();
            }
        }
    }

}