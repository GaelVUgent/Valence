using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.Models
{
    public struct SingleOutputFrame
    {
        public TimeSpan _Time;
        //Interactions
        public string _LookingAt;
        public string _Grasping;
        public string _InteractingWith;
        //HCCI Interactions
        public bool _UserToObject;
        public bool _UserToUser;
        public bool _UserToContent;
        public bool _UserToContext;
        //Physiodata
        public float _PupilDiameterLeft;
        public float _PupilDiameterRight;
        public Vector3 _PupilCoordinateLeft;
        public Vector3 _PupilCoordinateRight;
        public float _EyeOpenessLeft;
        public float _EyeOpenessRight;
        public string _HeartRate;
        //Inferred physiodata
        public int RebaScore;
        //Wizard interactions
        public Dictionary<string, float> _SceneControlInteractionsDictionary;

        //eDNA framework information
        public DateTime TimeNow;
        public int ParticipantNumber;
        public string ParticipantName;
        public int ExperimentNumber;
        public string ExperimentName;
        public string CurrentActionId;
        public string CurrentActionName;
        public string CurrentActionDescription;
        public string CurrentTrialId;
        public string CurrentTrialName;
        public string CurrentBlockId;
        public string CurrentBlockName;
        public int[] CurrentActiveConditionIds;
        public string[] CurrentActiveConditionNames;
    }

    public class SingleOutputFrameConverter : JsonConverter<SingleOutputFrame>
    {
        public override SingleOutputFrame ReadJson(JsonReader reader, Type objectType, SingleOutputFrame existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //base.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, SingleOutputFrame value, JsonSerializer serializer)
        {
            Formatting formatting = writer.Formatting;

            writer.WriteStartObject();
            writer.WritePropertyName("Time");
            writer.WriteValue(value._Time.ToString(@"hh\:mm\:ss\:fff"));
            writer.WritePropertyName("Time (absolute)");
            writer.WriteValue(DateTime.Now.ToString(@"HH\:mm\:ss\:fff"));
            writer.WritePropertyName("Participant number");
            writer.WriteValue(value.ParticipantNumber);
            writer.WritePropertyName("Participant name");
            writer.WriteValue(value.ParticipantName);
            writer.WritePropertyName("Experiment number");
            writer.WriteValue(value.ExperimentNumber);
            writer.WritePropertyName("Experiment name");
            writer.WriteValue(value.ExperimentName);
            writer.WritePropertyName("Current Block ID");
            writer.WriteValue(value.CurrentBlockId);
            writer.WritePropertyName("Current Block");
            writer.WriteValue(value.CurrentBlockName);
            writer.WritePropertyName("Current Trial ID");
            writer.WriteValue(value.CurrentTrialId);
            writer.WritePropertyName("Current Trial");
            writer.WriteValue(value.CurrentTrialName);
            writer.WritePropertyName("Current Active Condition IDs");

            if (value.CurrentActiveConditionIds != null && value.CurrentActiveConditionIds.Length > 0)
            {
                writer.WriteStartArray();
                foreach (var item in value.CurrentActiveConditionIds)
                {
                    writer.WriteValue(item);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNull();
            }

            writer.WritePropertyName("Current Active Conditions");

            if (value.CurrentActiveConditionNames != null && value.CurrentActiveConditionNames.Length > 0)
            {
                writer.WriteStartArray();
                foreach (var item in value.CurrentActiveConditionNames)
                {
                    writer.WriteValue(item);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNull();
            }

            writer.WritePropertyName("Current Action ID");
            writer.WriteValue(value.CurrentActionId);
            writer.WritePropertyName("Current Action");
            writer.WriteValue(value.CurrentActionName);
            writer.WritePropertyName("Current Action description");
            writer.WriteValue(value.CurrentActionDescription);
            writer.WritePropertyName("Looking At");
            writer.WriteValue(value._LookingAt);
            writer.WritePropertyName("Grasping");
            writer.WriteValue(value._Grasping);
            writer.WritePropertyName("Interacting With");
            writer.WriteValue(value._InteractingWith);
            writer.WritePropertyName("User To Object");
            writer.WriteValue(value._UserToObject);
            writer.WritePropertyName("User To User");
            writer.WriteValue(value._UserToUser);
            writer.WritePropertyName("User To Content");
            writer.WriteValue(value._UserToContent);
            writer.WritePropertyName("User To Context");
            writer.WriteValue(value._UserToContext);
            writer.WritePropertyName("Pupil Diameter Left");
            writer.WriteValue(value._PupilDiameterLeft);
            writer.WritePropertyName("Pupil Diameter Right");
            writer.WriteValue(value._PupilDiameterRight);
            writer.WritePropertyName("Pupil Coordinate Left");
            writer.WriteValue($"{value._PupilCoordinateLeft.x}, {value._PupilCoordinateLeft.y}, {value._PupilCoordinateLeft.z}");
            writer.WritePropertyName("Pupil Coordinate Right");
            writer.WriteValue($"{value._PupilCoordinateRight.x}, {value._PupilCoordinateRight.y}, {value._PupilCoordinateRight.z}");
            writer.WritePropertyName("Eye Openess Left");
            writer.WriteValue(value._EyeOpenessLeft);
            writer.WritePropertyName("Eye Openess Right");
            writer.WriteValue(value._EyeOpenessRight);
            writer.WritePropertyName("Heartrate");
            writer.WriteValue(value._HeartRate);
            writer.WritePropertyName("RebaScore");
            writer.WriteValue(value.RebaScore);
            writer.WritePropertyName("SceneControlsInteractions");
            writer.WriteStartArray();
            for (int i = 0; i < WizardEventHandler._RegisteredEvents.Count; i++)
            {
                writer.WriteStartObject();
                writer.Formatting = Formatting.None;
                writer.WritePropertyName(WizardEventHandler._RegisteredEvents[i]._Name);
                writer.WriteValue(value._SceneControlInteractionsDictionary[WizardEventHandler._RegisteredEvents[i]._Name]);
                writer.WriteEndObject();
                writer.Formatting = formatting;
            }


            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
