using ExitGames.Client.Photon;
using MICT.eDNA.Interfaces;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class PhotonEventConverter : MonoBehaviourPun, IOnEventCallback
{
    public const byte EventCodeExperienceServiceOnCurrentActionChanged = 0,
        EventCodeExperienceServiceOnCurrentTrialChanged = 1,
        EventCodeExperienceServiceOnCurrentBlockChanged = 2,
        EventCodeExperienceServiceGoToNext = 3,
        EventCodeExperienceServiceRegisterAction = 4,
        EventCodeExperienceServiceGoToPrevious = 5;
    private RaiseEventOptions _raiseEventOptionsForMaster, _raiseEventOptionsForAllButMe;

    private void Start()
    {
        _raiseEventOptionsForMaster = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        _raiseEventOptionsForAllButMe = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        switch (photonEvent.Code) {
            case EventCodeExperienceServiceOnCurrentActionChanged:
                var actionId1 = (int)photonEvent.CustomData;
                var action = ServiceLocator.DataService.GetActionFromSelector(new ActionSelector() { ConfigId = actionId1 });
                if (action != null)
                {
                    ServiceLocator.ExperienceService.CurrentAction = action;
                }
                break;
            case EventCodeExperienceServiceOnCurrentTrialChanged:
                var trialId1 = (int)photonEvent.CustomData;
                var trial = ServiceLocator.DataService.GetTrialFromSelector(new TrialSelector() { ConfigId = trialId1 });
                if (trial != null)
                {
                    ServiceLocator.ExperienceService.CurrentTrial = trial;
                }
                break;
            case EventCodeExperienceServiceOnCurrentBlockChanged:
                var blockId1 = (int)photonEvent.CustomData;
                var block = ServiceLocator.DataService.GetBlockFromSelector(new BlockSelector() { ConfigId = blockId1 });
                if (block != null)
                {
                    ServiceLocator.ExperienceService.CurrentBlock = block;
                }
                break;
            case EventCodeExperienceServiceGoToNext:
                var className = (string)photonEvent.CustomData;
                if (className == "Action") {
                    ServiceLocator.ExperienceService.GoToNext<Action>(false);
                }else if (className == "Trial") {
                    ServiceLocator.ExperienceService.GoToNext<Trial>(false);
                } else if (className == "Block") {
                    ServiceLocator.ExperienceService.GoToNext<Block>(false);
                } else if (className == "Condition") {
                    ServiceLocator.ExperienceService.GoToNext<MICT.eDNA.Models.Condition>(false);
                }
                break;
            case EventCodeExperienceServiceGoToPrevious:
                var className3 = (string)photonEvent.CustomData;
                if (className3 == "Action")
                {
                    ServiceLocator.ExperienceService.GoToPrevious<Action>(false);
                }
                else if (className3 == "Trial")
                {
                    ServiceLocator.ExperienceService.GoToPrevious<Trial>(false);
                }
                else if (className3 == "Block")
                {
                    ServiceLocator.ExperienceService.GoToPrevious<Block>(false);
                }
                else if (className3 == "Condition")
                {
                    ServiceLocator.ExperienceService.GoToPrevious<MICT.eDNA.Models.Condition>(false);
                }
                break;
            case EventCodeExperienceServiceRegisterAction:
                var actionId = (int)photonEvent.CustomData;
                var action2 = ServiceLocator.DataService.Data.AllActions.SingleOrDefault(x => x.ConfigId == actionId);
                if (action2 != null)
                {
                    ServiceLocator.ExperienceService.RegisterAction(action2, false);
                }
                break;
            default:
                break;

        }
    }

    private void OnDestroy()
    {
    }

    public void SendNetworkCallToAllButMe<T>(T intrface, string method, object variable = null) {
        
        //print("Sending method to all but me: " + method);
        if (method == "GoToNext")
        {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceGoToNext, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);
        }
        else if (method == "RegisterAction")
        {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceRegisterAction, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);
        }
        else if (method == "GoToPrevious")
        {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceGoToPrevious, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);
        }
        else if (method == "RegisterCurrent")
        {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceRegisterAction, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);
        }
        else if (method == "CurrentBlock") {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceOnCurrentBlockChanged, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);

        }
        else if (method == "CurrentTrial") {
            PhotonNetwork.RaiseEvent(EventCodeExperienceServiceOnCurrentTrialChanged, variable, _raiseEventOptionsForAllButMe, SendOptions.SendReliable);

        }
    }
}
