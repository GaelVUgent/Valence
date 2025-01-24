using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.Events;
using MICT.eDNA.Controllers;

[StructLayout(LayoutKind.Sequential)]
public struct Skel
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    //[FieldOffset(0)]
    public float[] confidence;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    //[FieldOffset(0)]
    public float[] x;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    //[FieldOffset(0)]trunk_length.ToString()
    public float[] y;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    //[FieldOffset(0)]
    public float[] z;
}

public class RebaScoreCalculator : MonoBehaviour {

    // Boolean to show optimal posture
    private bool showOptPosture = false;

    // Boolean that defines whether the joing angles are extracted from text file
    public bool getAnglesFromTextFile = false;
    public bool getOptAnglesFromTextFile = false;

    // Boolean that (de)activate the interfacing with Nuitrack
    public bool nuitrackActive = false;

    //-------------------------- Reba variables ------------------------------------------------------------------------------------------------------------------------
    const int max_reba = 3;
    int[] reba = {1,1,1,1};
    int [, ,] tableA = new int[,,]{ {{1, 2, 3, 4},{2, 3, 4, 5},{2, 4, 5, 6},{3, 5, 6, 7},{4, 6, 7, 8}} ,
    {{1, 2, 3, 4},{3, 4, 5, 6},{4, 5, 6, 7},{5, 6, 7, 8},{6, 7, 8, 9}} , {{3, 3, 5, 6},{4, 5, 6, 7},{5, 6, 7, 8},{6, 7, 8, 9},{7, 7, 8, 9}} };
    int [, ,] tableB = new int[,,]{ {{1, 2, 2},{1, 2, 3},{3, 4, 5},{4, 5, 5},{6, 7, 8},{7,8,8}} ,
    {{1, 2, 3},{2, 3, 4},{4, 5, 5},{5, 6, 7},{7, 8, 8},{8, 9, 9}} };
    int [,] tableC = new int [,] {{1, 1, 1, 2, 3, 3, 4, 5, 6, 7, 7, 7},{1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 7, 8},{2, 3, 3, 3, 4, 5, 6, 7, 7, 8, 8, 8},
    {3, 4, 4, 4, 5, 6, 7, 8, 8, 9, 9, 9},{4, 4, 4, 5, 6, 7, 8, 8, 9, 9, 9, 9},{6, 6, 6, 7, 8, 8, 9, 9, 10, 10, 10, 10},{7, 7, 7, 8, 9, 9, 9, 10, 10, 11, 11, 11},
    {8, 8, 8, 9, 10, 10, 10, 10, 10, 11, 11, 11},{9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 12, 12},{10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 12},
    {11, 11, 11, 11, 12,12, 12, 12, 12, 12, 12, 12},{12, 12, 12, 12, 12,12, 12, 12, 12, 12, 12, 12}};
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //-------------------------- Joint angles variables -----------------------------------------------------------------------------------------------------------------
	public float neck_bending = 0, neck_twist = 0, neck_side_bending = 0;
	public float trunk_bending = 0, trunk_twist = 0, trunk_side_bending = 0;
	public float upper_right_leg_flexion = 0, upper_right_leg_abduction = 0, upper_right_leg_rotation = 0;
	public float upper_left_leg_flexion = 0, upper_left_leg_abduction = 0, upper_left_leg_rotation = 0;
	public float lower_left_leg_flexion = 0, lower_right_leg_flexion = 0;
	public float left_foot_flexion = 0, left_foot_twist = 0, left_foot_bending = 0;
	public float right_foot_flexion = 0, right_foot_twist = 0, right_foot_bending = 0;
	public float upper_right_arm_raise = 0, upper_right_arm_flexion = 0, upper_right_arm_abduction = 0, upper_right_arm_rotation = 0;
	public float upper_left_arm_raise = 0, upper_left_arm_flexion = 0, upper_left_arm_abduction = 0, upper_left_arm_rotation = 0;
	public float lower_left_arm_flexion = 0, lower_right_arm_flexion = 0;
	public float left_hand_flexion = 0, left_hand_twist = 0, left_hand_bending = 0;
	public float right_hand_flexion = 0, right_hand_twist = 0, right_hand_bending = 0;

    public float neck_bending_opt = 0, neck_twist_opt = 0, neck_side_bending_opt = 0;
	public float trunk_bending_opt = 0, trunk_twist_opt = 0, trunk_side_bending_opt = 0;
	public float upper_right_leg_flexion_opt = 0, upper_right_leg_abduction_opt = 0, upper_right_leg_rotation_opt = 0;
	public float upper_left_leg_flexion_opt = 0, upper_left_leg_abduction_opt = 0, upper_left_leg_rotation_opt = 0;
	public float lower_left_leg_flexion_opt = 0, lower_right_leg_flexion_opt = 0;
	public float left_foot_flexion_opt = 0, left_foot_twist_opt = 0, left_foot_bending_opt = 0;
	public float right_foot_flexion_opt = 0, right_foot_twist_opt = 0, right_foot_bending_opt = 0;
	public float upper_right_arm_raise_opt = 0, upper_right_arm_flexion_opt = 0, upper_right_arm_abduction_opt = 0, upper_right_arm_rotation_opt = 0;
	public float upper_left_arm_raise_opt = 0, upper_left_arm_flexion_opt = 0, upper_left_arm_abduction_opt = 0, upper_left_arm_rotation_opt = 0;
	public float lower_left_arm_flexion_opt = 0, lower_right_arm_flexion_opt = 0;
	public float left_hand_flexion_opt = 0, left_hand_twist_opt = 0, left_hand_bending_opt = 0;
	public float right_hand_flexion_opt = 0, right_hand_twist_opt = 0, right_hand_bending_opt = 0;

    private float neck_bending_init = 0, neck_twist_init = 0, neck_side_bending_init = 0;
    private float trunk_bending_init = 0, trunk_twist_init = 0, trunk_side_bending_init = 0;
    private float upper_right_leg_flexion_init = 0, upper_right_leg_abduction_init = 0, upper_right_leg_rotation_init = 0;
    private float upper_left_leg_flexion_init = 0, upper_left_leg_abduction_init = 0, upper_left_leg_rotation_init = 0;
    private float lower_left_leg_flexion_init = 0, lower_right_leg_flexion_init = 0;
    private float left_foot_flexion_init = 0, left_foot_twist_init = 0, left_foot_bending_init = 0;
    private float right_foot_flexion_init = 0, right_foot_twist_init = 0, right_foot_bending_init = 0;
    private float upper_right_arm_raise_init = 0, upper_right_arm_flexion_init = 0, upper_right_arm_abduction_init = 0, upper_right_arm_rotation_init = 0;
    private float upper_left_arm_raise_init = 0, upper_left_arm_flexion_init = 0, upper_left_arm_abduction_init = 0, upper_left_arm_rotation_init = 0;
    private float lower_left_arm_flexion_init = 0, lower_right_arm_flexion_init = 0;
    private float left_hand_flexion_init = 0, left_hand_twist_init = 0, left_hand_bending_init = 0;
    private float right_hand_flexion_init = 0, right_hand_twist_init = 0, right_hand_bending_init = 0;

    private float upper_right_arm_flexion_opt_init = 0, upper_right_arm_abduction_opt_init = 0, upper_right_arm_rotation_opt_init = 0;
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //-------------------------- Limb length variables -------------------------------------------------------------------------------------------------------------------
    private float trunk_length = 0.58f, right_shoulder_length = 0.2f, right_arm_length = 0.31f, right_lower_arm_length = 0.28f;
    private float left_shoulder_length = 0.2f, left_arm_length = 0.31f, left_lower_arm_length = 0.28f, legs_length = 0.7f;
    bool first_detection = true;
    bool limbsLengthSent = false;
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------


    //-------------------------- GameObject variables --------------------------------------------------------------------------------------------------------------------
	public GameObject skeletonPlayer;
    public GameObject ground;
    public GameObject directionalLightObject;
    private GameObject neck, trunk, upper_right_leg, upper_left_leg, upper_right_arm, upper_left_arm, right_shoulder, left_shoulder, lower_left_leg, lower_right_leg, left_foot, right_foot;
    private GameObject lower_right_arm, lower_left_arm, left_hand, right_hand, left_hand_ring, right_hand_ring, left_toe, right_toe, head;
    private GameObject maleSkeleton, femaleSkeleton;

    public GameObject neck_opt, trunk_opt, upper_right_leg_opt, upper_left_leg_opt, upper_right_arm_opt, upper_left_arm_opt, right_shoulder_opt, left_shoulder_opt, lower_left_leg_opt, lower_right_leg_opt, left_foot_opt, right_foot_opt;
    private GameObject lower_right_arm_opt, lower_left_arm_opt, left_hand_opt, right_hand_opt, left_hand_ring_opt, right_hand_ring_opt, left_toe_opt, right_toe_opt, head_opt;
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------- GUI variables -----------------------------------------------------------------------------------------------------------------------------
    public UnityEventWithFloat OnRebaChanged;
    
    private Text reba_score_text;
    private Text trunk_bending_score_text, trunk_side_bending_score_text, trunk_twist_score_text;
    private Text left_upper_arm_score_text, left_lower_arm_score_text;
    private Text right_upper_arm_score_text, right_lower_arm_score_text;
	private Light directionalLight;
	public Button buttonModel;
	public Button buttonExit;
    Button btnModel;
	Button btnExit;
	public AudioSource alarmSource;
    private bool do_beep;
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //-------------------------UDP variables--------------------------------------------------------------------------------------------------------------------------------
	Thread thread;
	static UdpClient udp;
	static readonly object lockObject = new object();
	bool precessData = false;

    IPEndPoint remoteEndPoint;
    UdpClient client;
    private string IP;
    private int port;
    string strMessage = "";
    Socket client_soc;

    IPEndPoint remoteEndPointOpt;
    UdpClient client_opt;
    private string IP_opt;
    private int port_opt;
    Socket client_soc_opt;
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //-------------------------Nuitrack variables---------------------------------------------------------------------------------------------------------------------------
    String message;
	private Skel skel;
	public nuitrack.JointType[] typeJoint;
	GameObject[] CreatedJoint;
	public GameObject PrefabJoint;
    private float[,] init_pos = new float [3,3];
    private bool first_time;
    private Vector3 unity_hand_pos;
    private Vector3 unity_elbow_pos;
    private Vector3 unity_shoulder_pos;
    [Header("Rigged model")]
    [SerializeField]
    ModelJoint[] modelJoints;
    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();

    Vector3 trunk_init_vec, trunk_init_perp_vec;
    Vector3 neck_init_vec, neck_init_perp_vec;
    Vector3 upper_right_arm_init_vec, upper_right_arm_init_perp_vec;
    Vector3 lower_right_arm_init_vec, lower_right_arm_init_perp_vec;
    Vector3 upper_left_arm_init_vec, upper_left_arm_init_perp_vec;
    Vector3 lower_left_arm_init_vec, lower_left_arm_init_perp_vec;

    Vector3 object_vec, object_vec2;

    Vector3 planeNormal, planeNormal2;
    Vector3 projectedVec, projectedVec2;

    private int _reba = -1;
    public int Reba
    {
        get {
            return _reba;
        }
        private set {
            if (value == _reba)
                return;
            _reba = value;
            OutputController.UpdateRebaScore(_reba);
            OnRebaChanged?.Invoke(_reba);
        }
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

	// Initialization
	void Start () {

        initializeEnvironment();

        if(getAnglesFromTextFile)
        {
            float[] angles = readAnglesFromTextFile(Application.streamingAssetsPath + "/angles.txt");
            initializeJointAngles(angles);
        }

        if(getOptAnglesFromTextFile)
        {
            float[] angles = readAnglesFromTextFile(Application.streamingAssetsPath + "/anglesOpt.txt");
            initializeJointAngles(angles);
        }

        setInitialPosture();

        //setInitialOptPosture();

        //initializeUDP();

        updateEnvironment();

        //write_joint_positions();
        if(nuitrackActive)
            initializeNuitrack();
	}

    // Update is called once per frame
    void Update() {
        //if (precessData)
        //{
        //	/*lock object to make sure there data is
        //	 *not being accessed from multiple threads at thesame time*/
        //	lock (lockObject)
        //	{
        //		precessData = false;
        //
        //		float[,] pos = new float [,] {{skel.x[14], skel.y[14], skel.z[14]},{skel.x[13], skel.y[13], skel.z[13]},{skel.x[12], skel.y[12], skel.z[12]}};
        //		right_hand.transform.position = new Vector3(unity_hand_pos.x+(pos[0,0]-init_pos[0,0])*0.001f,unity_hand_pos.y+(pos[0,1]-init_pos[0,1])*0.001f,unity_hand_pos.z+(pos[0,2]-init_pos[0,2])*0.001f);
        //        lower_right_arm.transform.position = new Vector3(unity_elbow_pos.x+(pos[1,0]-init_pos[1,0])*0.001f,unity_elbow_pos.y+(pos[1,1]-init_pos[1,1])*0.001f,unity_elbow_pos.z+(pos[1,2]-init_pos[1,2])*0.001f);
        //        right_shoulder.transform.position = new Vector3(unity_shoulder_pos.x+(pos[2,0]-init_pos[2,0])*0.001f,unity_shoulder_pos.y+(pos[2,1]-init_pos[2,1])*0.001f,unity_shoulder_pos.z+(pos[2,2]-init_pos[2,2])*0.001f);
        //	}
        //}

        updateEnvironment();
        //sendJointsViaUDP();
        if (nuitrackActive)
        {
            if (CurrentUserTracker.CurrentSkeleton != null)
            {
                ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
            }
            else {
                Reba = -1;
            }
        }

        updateJointAngles();
        /*if(limbsLengthSent)
        {
            sendJointsViaUDP();
            //Debug.Log(limbsLengthSent);
        }
        else
        {
            if(first_detection)
            {
                sendLimbsLengthsViaUDP();
                limbsLengthSent = limbsLengthReceived();
            }
        }*/
        if(showOptPosture)
            updateOptPosture();

	}

    float[] readAnglesFromTextFile(string path)
    {
		string line;
		float[] angles = new float[36];
	    //Read the text from directly from the test.txt file
	    StreamReader reader = new StreamReader(path);
		int i = 0, k = 0;
		while ((line = reader.ReadLine()) != null)
		{
			if (i%2 != 0)
			{
				angles[k] = float.Parse(line);
				k++;
			}
			i++;
		}
	    reader.Close();

        return angles;
    }

    void initializeJointAngles(float[] angles)
    {
        //---------------------------------Initialization of joint angles-----------------------------------------------------------------------------------------
        neck_bending = angles[0];
        neck_twist = angles[1];
        neck_side_bending = angles[2];
        trunk_bending = angles[3];
        trunk_twist = angles[4];
        trunk_side_bending = angles[5];
        upper_right_leg_flexion = angles[6];
        upper_right_leg_abduction = angles[7];
        upper_right_leg_rotation = angles[8];
        upper_left_leg_flexion = angles[9];
        upper_left_leg_abduction = angles[10];
        upper_left_leg_rotation = angles[11];
        lower_left_leg_flexion = angles[12];
        lower_right_leg_flexion = angles[13];
        left_foot_flexion = angles[14];
        left_foot_twist = angles[15];
        left_foot_bending = angles[16];
        right_foot_flexion = angles[17];
        right_foot_twist = angles[18];
        right_foot_bending = angles[19];
        upper_right_arm_raise = angles[20];
        upper_right_arm_flexion = angles[21];
        upper_right_arm_abduction = angles[22];
        upper_right_arm_rotation = angles[23];
        upper_left_arm_raise = angles[24];
        upper_left_arm_flexion = angles[25];
        upper_left_arm_abduction = angles[26];
        upper_left_arm_rotation = angles[27];
        lower_left_arm_flexion = angles[28];
        lower_right_arm_flexion = angles[29];
        left_hand_flexion = angles[30];
        left_hand_twist = angles[31];
        left_hand_bending = angles[32];
        right_hand_flexion = angles[33];
        right_hand_twist = angles[34];
        right_hand_bending = angles[35];
        //------------------------------------------------------------------------------------------------------------------------------------------------------------
    }

    void initializeOptJointAngles(float[] angles)
    {
        //---------------------------------Initialization of joint angles-----------------------------------------------------------------------------------------
        neck_bending_opt = angles[0];
        neck_twist_opt = angles[1];
        neck_side_bending_opt = angles[2];
        trunk_bending_opt = angles[3];
        trunk_twist_opt = angles[4];
        trunk_side_bending_opt = angles[5];
        upper_right_leg_flexion_opt = angles[6];
        upper_right_leg_abduction_opt = angles[7];
        upper_right_leg_rotation_opt = angles[8];
        upper_left_leg_flexion_opt = angles[9];
        upper_left_leg_abduction_opt = angles[10];
        upper_left_leg_rotation_opt = angles[11];
        lower_left_leg_flexion_opt = angles[12];
        lower_right_leg_flexion_opt = angles[13];
        left_foot_flexion_opt = angles[14];
        left_foot_twist_opt = angles[15];
        left_foot_bending_opt = angles[16];
        right_foot_flexion_opt = angles[17];
        right_foot_twist_opt = angles[18];
        right_foot_bending_opt = angles[19];
        upper_right_arm_raise_opt = angles[20];
        upper_right_arm_flexion_opt = angles[21];
        upper_right_arm_abduction_opt = angles[22];
        upper_right_arm_rotation_opt = angles[23];
        upper_left_arm_raise_opt = angles[24];
        upper_left_arm_flexion_opt = angles[25];
        upper_left_arm_abduction_opt = angles[26];
        upper_left_arm_rotation_opt = angles[27];
        lower_left_arm_flexion_opt = angles[28];
        lower_right_arm_flexion_opt = angles[29];
        left_hand_flexion_opt = angles[30];
        left_hand_twist_opt = angles[31];
        left_hand_bending_opt = angles[32];
        right_hand_flexion_opt = angles[33];
        right_hand_twist_opt = angles[34];
        right_hand_bending_opt = angles[35];
        //------------------------------------------------------------------------------------------------------------------------------------------------------------
    }

    void setInitialPosture()
    {
        neck = GameObject.FindWithTag("Neck");
        trunk = GameObject.FindWithTag("Trunk");
        upper_right_leg = GameObject.FindWithTag("Upper_Right_Leg");
        upper_left_leg = GameObject.FindWithTag("Upper_Left_Leg");
        lower_right_leg = GameObject.FindWithTag("Lower_Right_Leg");
        lower_left_leg = GameObject.FindWithTag("Lower_Left_Leg");
        left_foot = GameObject.FindWithTag("Left_Foot");
        right_foot = GameObject.FindWithTag("Right_Foot");
        right_shoulder = GameObject.FindWithTag("Right_Shoulder");
        left_shoulder = GameObject.FindWithTag("Left_Shoulder");
        upper_right_arm = GameObject.FindWithTag("Upper_Right_Arm");
        upper_left_arm = GameObject.FindWithTag("Upper_Left_Arm");
        lower_right_arm = GameObject.FindWithTag("Lower_Right_Arm");
        lower_left_arm = GameObject.FindWithTag("Lower_Left_Arm");
        left_hand = GameObject.FindWithTag("Left_Hand");
        right_hand = GameObject.FindWithTag("Right_Hand");
        left_hand_ring = GameObject.FindWithTag("Left_Hand_Ring");
        right_hand_ring = GameObject.FindWithTag("Right_Hand_Ring");
        left_toe = GameObject.FindWithTag("Left_Toe");
        right_toe = GameObject.FindWithTag("Right_Toe");
        head = GameObject.FindWithTag("Head");

        neck_opt = GameObject.FindWithTag("Neck_Opt");
		trunk_opt = GameObject.FindWithTag("Trunk_Opt");
		upper_right_leg_opt = GameObject.FindWithTag("Upper_Right_Leg_Opt");
		upper_left_leg_opt = GameObject.FindWithTag("Upper_Left_Leg_Opt");
		lower_right_leg_opt = GameObject.FindWithTag("Lower_Right_Leg_Opt");
		lower_left_leg_opt = GameObject.FindWithTag("Lower_Left_Leg_Opt");
		left_foot_opt = GameObject.FindWithTag("Left_Foot_Opt");
		right_foot_opt = GameObject.FindWithTag("Right_Foot_Opt");
		right_shoulder_opt = GameObject.FindWithTag("Right_Shoulder_Opt");
		left_shoulder_opt = GameObject.FindWithTag("Left_Shoulder_Opt");
		upper_right_arm_opt = GameObject.FindWithTag("Upper_Right_Arm_Opt");
		upper_left_arm_opt = GameObject.FindWithTag("Upper_Left_Arm_Opt");
		lower_right_arm_opt = GameObject.FindWithTag("Lower_Right_Arm_Opt");
		lower_left_arm_opt = GameObject.FindWithTag("Lower_Left_Arm_Opt");
		left_hand_opt = GameObject.FindWithTag("Left_Hand_Opt");
		right_hand_opt = GameObject.FindWithTag("Right_Hand_Opt");

        setInitialJointAngles();
    }

    void initializeEnvironment()
    {
        reba_score_text = GameObject.FindWithTag("reba_score_text").GetComponent<Text>();
        trunk_bending_score_text = GameObject.FindWithTag("trunk_bending_text").GetComponent<Text>();
        trunk_side_bending_score_text = GameObject.FindWithTag("trunk_side_bending_text").GetComponent<Text>();
        trunk_twist_score_text = GameObject.FindWithTag("trunk_twist_text").GetComponent<Text>();
        left_upper_arm_score_text = GameObject.FindWithTag("left_upper_arm_text").GetComponent<Text>();
        left_lower_arm_score_text = GameObject.FindWithTag("left_lower_arm_text").GetComponent<Text>();
        right_upper_arm_score_text = GameObject.FindWithTag("right_upper_arm_text").GetComponent<Text>();
        right_lower_arm_score_text = GameObject.FindWithTag("right_lower_arm_text").GetComponent<Text>();

        directionalLight = directionalLightObject.GetComponent<Light>();
        btnModel = buttonModel.GetComponent<Button>();
        btnExit = buttonExit.GetComponent<Button>();
        alarmSource = GetComponent<AudioSource> ();

        maleSkeleton = GameObject.FindWithTag("mesh_obj");
        femaleSkeleton = GameObject.FindWithTag("meshes");

        maleSkeleton.SetActive(false);

        btnModel.onClick.AddListener(ChangeModel);
        btnExit.onClick.AddListener(doExit);

        do_beep = true;
    }

    void initializeUDP()
    {

        Debug.Log("UDPSend.init()");

        IP="127.0.0.1";
        port=8051;
        port_opt=8052;

        // ----------------------------
        // Senden
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        udp = new UdpClient();

        remoteEndPointOpt = new IPEndPoint(IPAddress.Parse(IP), port_opt);

        // status
        Debug.Log("Sending to "+IP+" : "+port);

        client_soc = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client_soc_opt = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //client_soc.Bind(remoteEndPoint);
        client_soc_opt.Bind(new IPEndPoint(IPAddress.Any, port_opt));

        //sendString(get_joint_positions());
        //sendDouble(0.1);
    }

    void initializeNuitrack()
    {
        //right_shoulder.transform.Translate(0.0f, upper_right_arm_raise, 0.0f, Space.Self);
        //left_shoulder.transform.Translate(0.0f, upper_left_arm_raise, 0.0f, Space.Self);
        /*thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();

        first_time = true;

        unity_hand_pos = right_hand.transform.position;
        unity_elbow_pos = lower_right_arm.transform.position;
        unity_shoulder_pos = right_shoulder.transform.position;*/


        message = " ";
        for (int j = 0; j < modelJoints.Length; j++)
        {
            if(modelJoints[j] == null || modelJoints[j].bone == null)
                continue;
            modelJoints[j].baseRotOffset = modelJoints[j].bone.rotation;
            /*if(j==0)
            {
                modelJoints[j].baseRotOffset = Quaternion.Euler(180,0,180);
                Debug.Log(modelJoints[j].baseRotOffset);
            }*/
            //if(!jointsRigged.ContainsKey(modelJoints[j].jointType.TryGetMirrored()))
            jointsRigged.Add(modelJoints[j].jointType.TryGetMirrored(), modelJoints[j]);
            /*else
            {
                jointsRigged.Add(None, modelJoints[j]);
            }*/
        }
    }

    void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        //Vector3 torsoPos = Quaternion.Euler(0f, 180f, 0f) * (0.001f * skeleton.GetJoint(nuitrack.JointType.Torso).ToVector3());
        //transform.position = new Vector3(torsoPos.x,torsoPos.y,-torsoPos.z);

        /*if (CurrentUserTracker.CurrentUser != 0)
		{
				nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
				message = "Skeleton found";

				for (int q = 0; q < typeJoint.Length; q++)using System.Collections.Generic;
				{
						nuitrack.Joint joint = skeleton.GetJoint(typeJoint[q]);
						Vector3 newPosition = 0.001f * joint.ToVector3();
                        if(q == 15)
                            right_hand.transform.localPosition = new Vector3(unity_hand_pos.z+(newPosition.z - init_pos[0,2]),unity_hand_pos.y+(newPosition.y - init_pos[0,1]),-unity_hand_pos.x-(newPosition.x - init_pos[0,0]));
				}
		}
		else
		{
				message = "Skeleton not found";
		}*/

        if(!first_detection)
        {
            initializeLimbsLength(skeleton);
            Debug.Log("Limbs lengths - Trunk: " + trunk_length + ", Right shoulder: " + right_shoulder_length + ", Right arm: " + right_arm_length + ", Right lower arm: " +  right_lower_arm_length + ", Left shoulder: " + left_shoulder_length + ", Left arm: " + left_arm_length + ", Left lower arm: " + left_lower_arm_length + ", Legs length: " + legs_length);
            first_detection = true;
        }

        int j=0;
        foreach (var riggedJoint in jointsRigged)
        {
            nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);
            ModelJoint modelJoint = riggedJoint.Value;
            //Debug.Log(riggedJoint.Value);
            Quaternion jointOrient;

            //if(j!=0)
            //    jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * (joint.ToQuaternion()) *modelJoint.baseRotOffset;
            //else
            jointOrient = Quaternion.Euler(0,1*-10,0)*Quaternion.Inverse(CalibrationInfo.SensorOrientation) * (joint.ToQuaternion()) *modelJoint.baseRotOffset;
            modelJoint.bone.rotation = jointOrient;

            j++;
        }

        updateJointAngles();
        /*if(limbsLengthSent)
            sendJointsViaUDP();
        else
        {
            if(first_detection)
            {
                sendLimbsLengthsViaUDP();
                limbsLengthSent = limbsLengthReceived();
            }
        }*/
        if(showOptPosture)
            updateOptPosture();
    }

	// Display the message on the screen
	void OnGUI()
	{
			GUI.color = Color.red;
			GUI.skin.label.fontSize = 50;
			GUILayout.Label(message);
	}

    int neck_score()
    {
        int n = 1;
        if(neck_bending > 20 || neck_bending < -10)
            n = 2;
        if(neck_side_bending > 30 || neck_side_bending < -30 || neck_twist > 30 || neck_twist < -30)
            n += 1;

        return n;
    }

    int trunk_score()
    {
        int t = 1;
        if(trunk_bending < -10 || (trunk_bending > 5 && trunk_bending <= 20))
            t = 2;
        if(trunk_bending > 20 && trunk_bending <= 45)
            t = 3;
        if(trunk_bending > 45)
            t = 4;
        if(trunk_side_bending > 30 || trunk_side_bending < -30 || trunk_twist > 30 || trunk_twist < -30)
            t += 1;

        return t;
    }

    int upper_arm_score(string arm)
    {
        int ua = 1;
        if(arm.Equals("right"))
        {
            if(upper_right_arm_flexion > 20 && upper_right_arm_flexion <= 45)
                ua = 2;
            if(upper_right_arm_flexion > 45 && upper_right_arm_flexion <= 90)
                ua = 3;
            if(upper_right_arm_flexion > 90)
                ua = 4;
            if(upper_right_arm_abduction > 30)
                ua += 1;
            if(upper_right_arm_raise > 0.02)
                ua += 1;
        }
        else
        {
            if(upper_left_arm_flexion > 20 && upper_left_arm_flexion <= 45)
                ua = 2;
            if(upper_left_arm_flexion > 45 && upper_left_arm_flexion <= 90)
                ua = 3;
            if(upper_left_arm_flexion > 90)
                ua = 4;
            if(upper_left_arm_abduction > 30)
                ua += 1;
            if(upper_left_arm_raise > 0.02)
                ua += 1;
        }

        return ua;
    }

    int lower_arm_score(string arm)
    {
        int la = 1;
        if(arm.Equals("right"))
        {
            if(lower_right_arm_flexion <= 60 || lower_right_arm_flexion >= 100)
                la = 2;
        }
        else
        {
            if(lower_left_arm_flexion <= 60 || lower_left_arm_flexion >= 100)
                la = 2;
        }

        return la;
    }

    int wrist_score(string arm)
    {
        int w = 1;
        if(arm.Equals("right"))
        {
            if(right_hand_flexion > 15 || right_hand_flexion < -15)
                w = 2;
            if(right_hand_twist > 30 || right_hand_twist < -30 || right_hand_bending > 30 || right_hand_bending < -30)
                w += 1;
        }
        else
        {
            if(left_hand_flexion > 15 || left_hand_flexion < -15)
                w = 2;
            if(left_hand_twist > 30 || left_hand_twist < -30 || left_hand_bending > 30 || left_hand_bending < -30)
                w += 1;
        }

        return w;
    }

	private int[] find_reba(){
		int [] score = {1,1,1,1};
		int reba_value = 1;
		int scoreA = 1, scoreB_r = 1, scoreB_l = 1, scoreB = 1, scoreC = 1;

        int n = neck_score();

        int t = trunk_score();

		int l = 1;

		scoreA = tableA[n-1,t-1,l-1];

		int ua_r = upper_arm_score("right");

		int la_r = lower_arm_score("right");

		int w_r = wrist_score("right");

		scoreB_r = tableB[la_r-1,ua_r-1,w_r-1];

        int ua_l = upper_arm_score("left");

        int la_l = lower_arm_score("left");

        int w_l = wrist_score("left");

		scoreB_l = tableB[la_l-1,ua_l-1,w_l-1];

		scoreB = Mathf.Max(scoreB_l,scoreB_r);

		scoreC = tableC[scoreA-1,scoreB-1];

		reba_value = scoreC;

		score[0] = reba_value;
		score[1] = scoreA;
		score[2] = scoreB;
		score[3] = scoreC;

		return score;
	}

	private void changeLight(Color color)
	{
		directionalLight.color = color;
	}

	private void changeText()
	{
        reba_score_text.color = Color.white;
		reba_score_text.text = "Reba score: " + reba[0].ToString();


        if(reba[0] >= max_reba)
            reba_score_text.color = Color.red;

        trunk_bending_score_text.color = Color.white;
        trunk_bending_score_text.text = "Trunk bending: Normal";
        if(trunk_bending > 20 && trunk_bending <= 45)
        {
            trunk_bending_score_text.text = "Trunk bending: Medium";
            trunk_bending_score_text.color = Color.yellow;
        }
        if(trunk_bending > 45)
        {
            trunk_bending_score_text.text = "Trunk bending: High";
            trunk_bending_score_text.color = Color.red;
        }

        trunk_side_bending_score_text.color = Color.white;
        trunk_side_bending_score_text.text = "Trunk side bending: Normal";
        if(trunk_side_bending > 30 || trunk_side_bending < -30)
        {
            trunk_side_bending_score_text.text = "Trunk side bending: High";
            trunk_side_bending_score_text.color = Color.red;
        }

        trunk_twist_score_text.color = Color.white;
        trunk_twist_score_text.text = "Trunk twist: Normal";
        if(trunk_twist > 30 || trunk_twist < -30)
        {
            trunk_twist_score_text.text = "Trunk twist: High";
            trunk_twist_score_text.color = Color.red;
        }

        right_upper_arm_score_text.color = Color.white;
        right_upper_arm_score_text.text = "Right upper arm angle: Normal";
        if(upper_arm_score("right") == 3)
        {
            right_upper_arm_score_text.text = "Right upper arm angle: Medium";
            right_upper_arm_score_text.color = Color.yellow;
        }
        if(upper_arm_score("right") >= 4)
        {
            right_upper_arm_score_text.text = "Right upper arm angle: High";
            right_upper_arm_score_text.color = Color.red;
        }

        left_upper_arm_score_text.color = Color.white;
        left_upper_arm_score_text.text = "Left upper arm angle: Normal";
        if(upper_arm_score("left") == 3)
        {
            left_upper_arm_score_text.text = "Left upper arm angle: Medium";
            left_upper_arm_score_text.color = Color.yellow;
        }
        if(upper_arm_score("left") >= 4)
        {
            left_upper_arm_score_text.text = "left upper arm angle: High";
            left_upper_arm_score_text.color = Color.red;
        }

        right_lower_arm_score_text.color = Color.white;
        right_lower_arm_score_text.text = "Right lower arm angle: Normal";
        if(lower_arm_score("right") == 2)
        {
            right_lower_arm_score_text.text = "Right lower arm angle: Medium";
            right_lower_arm_score_text.color = Color.yellow;
        }

        left_lower_arm_score_text.color = Color.white;
        left_lower_arm_score_text.text = "Left lower arm angle: Normal";
        if(lower_arm_score("left") == 2)
        {
            left_lower_arm_score_text.text = "Left lower arm angle: Medium";
            left_lower_arm_score_text.color = Color.yellow;
        }
	}

	private void ChangeModel()
    {
	  maleSkeleton.SetActive(!maleSkeleton.active);
      femaleSkeleton.SetActive(!femaleSkeleton.active);

		Text bModeltext = btnModel.GetComponentInChildren<Text>();
		if(maleSkeleton.active == true)
			bModeltext.text = "Male";
		else
			bModeltext.text = "Female";
    }

	private void doExit() {
        Application.Quit();
    }

	private void doBeep()
	{
		alarmSource.Play();
		//EditorApplication.Beep();
	}

	private void updateEnvironment()
	{
        reba = find_reba();
		changeText();
        Reba = reba[0];
		if(reba[0] >= max_reba)
		{
            if(do_beep)
            {
                doBeep();
                do_beep = false;
            }
			changeLight(Color.red);
		}
		else
        {
            do_beep = true;
			changeLight(Color.white);
        }
	}

    void setInitialJointAngles()
    {
        trunk_init_vec = trunk.transform.up;
        trunk_init_perp_vec = trunk.transform.forward;
        neck_init_vec = trunk.transform.InverseTransformDirection(neck.transform.up);
        neck_init_perp_vec = trunk.transform.InverseTransformDirection(neck.transform.forward);
        upper_right_arm_init_vec = trunk.transform.InverseTransformDirection(upper_right_arm.transform.right);
        upper_right_arm_init_perp_vec = trunk.transform.InverseTransformDirection(upper_right_arm.transform.forward);
        lower_right_arm_init_vec = upper_right_arm.transform.InverseTransformDirection(lower_right_arm.transform.right);
        lower_right_arm_init_perp_vec = upper_right_arm.transform.InverseTransformDirection(lower_right_arm.transform.forward);
        upper_left_arm_init_vec = trunk.transform.InverseTransformDirection(upper_left_arm.transform.right);
        upper_left_arm_init_perp_vec = trunk.transform.InverseTransformDirection(upper_left_arm.transform.forward);
        lower_left_arm_init_vec = upper_left_arm.transform.InverseTransformDirection(lower_left_arm.transform.right);
        lower_left_arm_init_perp_vec = upper_left_arm.transform.InverseTransformDirection(lower_left_arm.transform.forward);

        neck_bending_init = neck.transform.rotation.eulerAngles.x;
        neck_twist_init = neck.transform.rotation.eulerAngles.y;
        neck_side_bending_init = neck.transform.rotation.eulerAngles.z;
        trunk_bending_init = trunk.transform.rotation.eulerAngles.x;
        trunk_twist_init = trunk.transform.rotation.eulerAngles.y;
        trunk_side_bending_init = trunk.transform.rotation.eulerAngles.z;
        upper_right_leg_flexion_init = upper_right_leg.transform.rotation.eulerAngles.x;
        upper_right_leg_rotation_init = upper_right_leg.transform.rotation.eulerAngles.y;
        upper_right_leg_abduction_init = upper_right_leg.transform.rotation.eulerAngles.z;
        upper_left_leg_flexion_init = upper_left_leg.transform.rotation.eulerAngles.x;
        upper_left_leg_rotation_init = upper_left_leg.transform.rotation.eulerAngles.y;
        upper_left_leg_abduction_init = upper_left_leg.transform.rotation.eulerAngles.z;
        upper_right_arm_flexion_init = upper_right_arm.transform.rotation.eulerAngles.y;
        upper_right_arm_rotation_init = upper_right_arm.transform.rotation.eulerAngles.x;
        upper_right_arm_abduction_init = upper_right_arm.transform.rotation.eulerAngles.z;
        upper_left_arm_rotation_init = upper_left_arm.transform.rotation.eulerAngles.x;
        upper_left_arm_flexion_init = upper_left_arm.transform.rotation.eulerAngles.y;
        upper_left_arm_abduction_init = upper_left_arm.transform.rotation.eulerAngles.z;
        lower_right_leg_flexion_init = lower_right_leg.transform.rotation.eulerAngles.x;
        lower_left_leg_flexion_init = lower_left_leg.transform.rotation.eulerAngles.x;
        left_foot_flexion_init = left_foot.transform.rotation.eulerAngles.x;
        left_foot_twist_init = left_foot.transform.rotation.eulerAngles.y;
        left_foot_bending_init = left_foot.transform.rotation.eulerAngles.z;
        right_foot_flexion_init = right_foot.transform.rotation.eulerAngles.x;
        right_foot_twist_init = right_foot.transform.rotation.eulerAngles.y;
        right_foot_bending_init = right_foot.transform.rotation.eulerAngles.z;
        upper_right_arm_raise_init = right_shoulder.transform.localPosition.y;
        upper_left_arm_raise_init = left_shoulder.transform.localPosition.y;
        lower_right_arm_flexion_init = lower_right_arm.transform.rotation.eulerAngles.y;
        lower_left_arm_flexion_init = lower_left_arm.transform.rotation.eulerAngles.y;
        left_hand_twist_init = left_hand.transform.rotation.eulerAngles.x;
        left_hand_bending_init = left_hand.transform.rotation.eulerAngles.y;
        left_hand_flexion_init = left_hand.transform.rotation.eulerAngles.z;
        right_hand_twist_init = right_hand.transform.rotation.eulerAngles.x;
        right_hand_bending_init = right_hand.transform.rotation.eulerAngles.y;
        right_hand_flexion_init = right_hand.transform.rotation.eulerAngles.z;

        upper_right_arm_flexion_opt_init = upper_right_arm_opt.transform.localEulerAngles.x;

        neck.transform.Rotate(neck_bending, 0.0f, 0.0f, Space.Self);
        neck.transform.Rotate(0.0f, 0.0f, neck_side_bending, Space.Self);
        neck.transform.Rotate(0.0f, neck_twist, 0.0f, Space.Self);
        /*trunk.transform.Rotate(trunk_bending, 0.0f, 0.0f, Space.Self);
        trunk.transform.Rotate(0.0f, 0.0f, trunk_side_bending, Space.Self);
        trunk.transform.Rotate(0.0f, trunk_twist, 0.0f, Space.Self);*/

        trunk.transform.rotation = trunk.transform.rotation * Quaternion.AngleAxis(trunk_bending, Vector3.right);
        trunk.transform.rotation = trunk.transform.rotation * Quaternion.AngleAxis(trunk_side_bending, Vector3.forward);
        trunk.transform.rotation = trunk.transform.rotation * Quaternion.AngleAxis(trunk_twist, Vector3.up);

        upper_right_leg.transform.Rotate(-upper_right_leg_flexion,0.0f,0.0f, Space.Self);
        upper_right_leg.transform.Rotate(0.0f,0.0f,upper_right_leg_abduction, Space.Self);
        upper_right_leg.transform.Rotate(0.0f,upper_right_leg_rotation,0.0f, Space.Self);
        upper_left_leg.transform.Rotate(-upper_left_leg_flexion,0.0f,0.0f, Space.Self);
        upper_left_leg.transform.Rotate(0.0f,0.0f,-upper_left_leg_abduction, Space.Self);
        upper_left_leg.transform.Rotate(0.0f,-upper_left_leg_rotation,0.0f, Space.Self);
        upper_right_arm.transform.Rotate(0.0f,-upper_right_arm_flexion,0.0f,Space.Self);
        upper_right_arm.transform.Rotate(0.0f,0.0f,upper_right_arm_abduction,Space.Self);
        upper_right_arm.transform.Rotate(-upper_right_arm_rotation,0.0f,0.0f,Space.Self);
        upper_left_arm.transform.Rotate(0.0f,upper_left_arm_flexion,0.0f, Space.Self);
        upper_left_arm.transform.Rotate(0.0f,0.0f,-upper_left_arm_abduction, Space.Self);
        upper_left_arm.transform.Rotate(-upper_left_arm_rotation,0.0f,0.0f, Space.Self);
        lower_right_leg.transform.Rotate(lower_right_leg_flexion, 0.0f, 0.0f, Space.Self);
        lower_left_leg.transform.Rotate(lower_left_leg_flexion, 0.0f, 0.0f, Space.Self);
        left_foot.transform.Rotate(left_foot_flexion, 0.0f, 0.0f, Space.Self);
        left_foot.transform.Rotate(0.0f, 0.0f, -left_foot_bending, Space.Self);
        left_foot.transform.Rotate(0.0f, -left_foot_twist, 0.0f, Space.Self);
        right_foot.transform.Rotate(right_foot_flexion, 0.0f, 0.0f, Space.Self);
        right_foot.transform.Rotate(0.0f, 0.0f, right_foot_bending, Space.Self);
        right_foot.transform.Rotate(0.0f, right_foot_twist, 0.0f, Space.Self);
        lower_right_arm.transform.Rotate(0.0f, -lower_right_arm_flexion, 0.0f, Space.Self);
        lower_left_arm.transform.Rotate(0.0f, lower_left_arm_flexion, 0.0f, Space.Self);
        left_hand.transform.Rotate(0.0f, 0.0f, left_hand_flexion, Space.Self);
        left_hand.transform.Rotate(0.0f, -left_hand_bending, 0.0f, Space.Self);
        left_hand.transform.Rotate(-left_hand_twist, 0.0f, 0.0f, Space.Self);
        right_hand.transform.Rotate(0.0f, 0.0f, -right_hand_flexion, Space.Self);
        right_hand.transform.Rotate(0.0f, right_hand_bending, 0.0f, Space.Self);
        right_hand.transform.Rotate(right_hand_twist, 0.0f, 0.0f, Space.Self);

        if (upper_right_arm_raise > 0.05)
            upper_right_arm_raise = 0.05f;
        if (upper_right_arm_raise < -0.02)
            upper_right_arm_raise = -0.02f;
        if (upper_left_arm_raise > 0.05)
            upper_left_arm_raise = 0.05f;
        if (upper_left_arm_raise < -0.02)
            upper_left_arm_raise = -0.02f;

        right_shoulder.transform.Translate(0.0f, upper_right_arm_raise, 0.0f, Space.Self);
        left_shoulder.transform.Translate(0.0f, upper_left_arm_raise, 0.0f, Space.Self);

        neck_opt.transform.Rotate(neck_bending_opt, 0.0f, 0.0f, Space.Self);
        neck_opt.transform.Rotate(0.0f, 0.0f, neck_side_bending_opt, Space.Self);
        neck_opt.transform.Rotate(0.0f, neck_twist_opt, 0.0f, Space.Self);
        trunk_opt.transform.Rotate(trunk_bending_opt, 0.0f, 0.0f, Space.Self);
        trunk_opt.transform.Rotate(0.0f, 0.0f, trunk_side_bending_opt, Space.Self);
        trunk_opt.transform.Rotate(0.0f, trunk_twist_opt, 0.0f, Space.Self);
        upper_right_leg_opt.transform.Rotate(-upper_right_leg_flexion_opt,0.0f,0.0f, Space.Self);
        upper_right_leg_opt.transform.Rotate(0.0f,0.0f,upper_right_leg_abduction_opt, Space.Self);
        upper_right_leg_opt.transform.Rotate(0.0f,upper_right_leg_rotation_opt,0.0f, Space.Self);
        upper_left_leg_opt.transform.Rotate(-upper_left_leg_flexion_opt,0.0f,0.0f, Space.Self);
        upper_left_leg_opt.transform.Rotate(0.0f,0.0f,-upper_left_leg_abduction_opt, Space.Self);
        upper_left_leg_opt.transform.Rotate(0.0f,-upper_left_leg_rotation_opt,0.0f, Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,-upper_right_arm_flexion_opt,0.0f,Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,0.0f,upper_right_arm_abduction_opt,Space.Self);
        upper_right_arm_opt.transform.Rotate(-upper_right_arm_rotation_opt,0.0f,0.0f,Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,upper_left_arm_flexion_opt,0.0f, Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,0.0f,-upper_left_arm_abduction_opt, Space.Self);
        upper_left_arm_opt.transform.Rotate(-upper_left_arm_rotation_opt,0.0f,0.0f, Space.Self);
        lower_right_leg_opt.transform.Rotate(lower_right_leg_flexion_opt, 0.0f, 0.0f, Space.Self);
        lower_left_leg_opt.transform.Rotate(lower_left_leg_flexion_opt, 0.0f, 0.0f, Space.Self);
        left_foot_opt.transform.Rotate(left_foot_flexion_opt, 0.0f, 0.0f, Space.Self);
        left_foot_opt.transform.Rotate(0.0f, 0.0f, -left_foot_bending_opt, Space.Self);
        left_foot_opt.transform.Rotate(0.0f, -left_foot_twist_opt, 0.0f, Space.Self);
        right_foot_opt.transform.Rotate(right_foot_flexion_opt, 0.0f, 0.0f, Space.Self);
        right_foot_opt.transform.Rotate(0.0f, 0.0f, right_foot_bending_opt, Space.Self);
        right_foot_opt.transform.Rotate(0.0f, right_foot_twist_opt, 0.0f, Space.Self);
        lower_right_arm_opt.transform.Rotate(0.0f, -lower_right_arm_flexion_opt, 0.0f, Space.Self);
        lower_left_arm_opt.transform.Rotate(0.0f, lower_left_arm_flexion_opt, 0.0f, Space.Self);
        left_hand_opt.transform.Rotate(0.0f, 0.0f, left_hand_flexion_opt, Space.Self);
        left_hand_opt.transform.Rotate(0.0f, -left_hand_bending_opt, 0.0f, Space.Self);
        left_hand_opt.transform.Rotate(-left_hand_twist_opt, 0.0f, 0.0f, Space.Self);
        right_hand_opt.transform.Rotate(0.0f, 0.0f, -right_hand_flexion_opt, Space.Self);
        right_hand_opt.transform.Rotate(0.0f, right_hand_bending_opt, 0.0f, Space.Self);
        right_hand_opt.transform.Rotate(right_hand_twist_opt, 0.0f, 0.0f, Space.Self);

        if (upper_right_arm_raise_opt > 0.05)
            upper_right_arm_raise_opt = 0.05f;
        if (upper_right_arm_raise_opt < -0.02)
            upper_right_arm_raise_opt = -0.02f;
        if (upper_left_arm_raise_opt > 0.05)
            upper_left_arm_raise_opt = 0.05f;
        if (upper_left_arm_raise_opt < -0.02)
            upper_left_arm_raise_opt = -0.02f;

        right_shoulder_opt.transform.Translate(0.0f, upper_right_arm_raise_opt, 0.0f, Space.Self);
        left_shoulder_opt.transform.Translate(0.0f, upper_left_arm_raise_opt, 0.0f, Space.Self);

    }

    void updateJointAngles()
    {
        /*neck_bending = formatAngle(neck.transform.rotation.eulerAngles.x - neck_bending_init);
        neck_twist = formatAngle(neck.transform.rotation.eulerAngles.y - neck_twist_init);
        neck_side_bending = formatAngle(neck.transform.rotation.eulerAngles.z - neck_side_bending_init);
        trunk_bending = formatAngle(trunk.transform.rotation.eulerAngles.x - trunk_bending_init);
        trunk_twist = formatAngle(trunk.transform.rotation.eulerAngles.y - trunk_twist_init);
        trunk_side_bending = formatAngle(trunk.transform.rotation.eulerAngles.z - trunk_side_bending_init);
        upper_right_leg_flexion = formatAngle(- upper_right_leg.transform.rotation.eulerAngles.x + upper_right_leg_flexion_init);
        upper_right_leg_rotation = formatAngle(upper_right_leg.transform.rotation.eulerAngles.y - upper_right_leg_rotation_init);
        upper_right_leg_abduction = formatAngle(upper_right_leg.transform.rotation.eulerAngles.z - upper_right_leg_abduction_init);
        upper_left_leg_flexion = formatAngle(- upper_left_leg.transform.rotation.eulerAngles.x + upper_left_leg_flexion_init);
        upper_left_leg_rotation = formatAngle(- upper_left_leg.transform.rotation.eulerAngles.y + upper_left_leg_rotation_init);
        upper_left_leg_abduction = formatAngle(- upper_left_leg.transform.rotation.eulerAngles.z + upper_left_leg_abduction_init);
        upper_right_arm_rotation = formatAngle(- upper_right_arm.transform.rotation.eulerAngles.x + upper_right_arm_rotation_init);
        upper_right_arm_flexion = formatAngle(- upper_right_arm.transform.localRotation.eulerAngles.y + upper_right_arm_flexion_init);
        upper_right_arm_abduction = formatAngle(upper_right_arm.transform.rotation.eulerAngles.z - upper_right_arm_abduction_init);
        upper_left_arm_rotation = formatAngle(- upper_left_arm.transform.rotation.eulerAngles.x + upper_left_arm_rotation_init);
        upper_left_arm_flexion = formatAngle(upper_left_arm.transform.rotation.eulerAngles.y - upper_left_arm_flexion_init);
        upper_left_arm_abduction = formatAngle(- upper_left_arm.transform.rotation.eulerAngles.z + upper_left_arm_abduction_init);
        lower_right_leg_flexion = formatAngle(lower_right_leg.transform.rotation.eulerAngles.x - lower_right_leg_flexion_init);
        lower_left_leg_flexion = formatAngle(lower_left_leg.transform.rotation.eulerAngles.x - lower_left_leg_flexion_init);
        left_foot_flexion = formatAngle(left_foot.transform.rotation.eulerAngles.x - left_foot_flexion_init);
        left_foot_twist = formatAngle(- left_foot.transform.rotation.eulerAngles.y + left_foot_twist_init);
        left_foot_bending = formatAngle(- left_foot.transform.rotation.eulerAngles.z + left_foot_bending_init);
        right_foot_flexion = formatAngle(right_foot.transform.rotation.eulerAngles.x - right_foot_flexion_init);
        right_foot_twist = formatAngle(right_foot.transform.rotation.eulerAngles.y - right_foot_twist_init);
        right_foot_bending = formatAngle(right_foot.transform.rotation.eulerAngles.z - right_foot_bending_init);
        upper_right_arm_raise = formatAngle(right_shoulder.transform.localPosition.y - upper_right_arm_raise_init);
        upper_left_arm_raise = formatAngle(left_shoulder.transform.localPosition.y - upper_left_arm_raise_init);
        lower_right_arm_flexion = formatAngle(- lower_right_arm.transform.rotation.eulerAngles.y + lower_right_arm_flexion_init);
        lower_left_arm_flexion = formatAngle(lower_left_arm.transform.rotation.eulerAngles.y - lower_left_arm_flexion_init);
        left_hand_twist = formatAngle(- left_hand.transform.rotation.eulerAngles.x + left_hand_twist_init);
        left_hand_bending = formatAngle(- left_hand.transform.rotation.eulerAngles.y + left_hand_bending_init);
        left_hand_flexion = formatAngle(left_hand.transform.rotation.eulerAngles.z - left_hand_flexion_init);
        right_hand_twist = formatAngle(right_hand.transform.rotation.eulerAngles.x - right_hand_twist_init);
        right_hand_bending = formatAngle(right_hand.transform.rotation.eulerAngles.y - right_hand_bending_init);
        right_hand_flexion = formatAngle(- right_hand.transform.rotation.eulerAngles.z + right_hand_flexion_init);*/

        findPYR();

        //Debug.Log("Trunk bending: " + trunk_bending + ", Trunk twist: " + trunk_twist + ", Trunk side bending: " + trunk_side_bending);
        //Debug.Log("Upper right arm flexion: " + upper_right_arm_flexion);
        //Debug.Log("Transf pos: " + newTrans.position);
    }

    void findPYR()
    {
        // Trunk
        planeNormal = Vector3.Cross(trunk_init_vec, trunk_init_perp_vec);
        projectedVec = Vector3.ProjectOnPlane(trunk.transform.up, planeNormal);

        trunk_bending = Vector3.SignedAngle(projectedVec, trunk_init_vec,Vector3.up);
        if(trunk_bending >= 7)
            trunk_bending -= 1*7;

        if(trunk_bending < -7)
            trunk_bending = -7;
        if(trunk_bending > 90)
            trunk_bending = 90;

        trunk_side_bending = -Vector3.SignedAngle(projectedVec, trunk.transform.up, Vector3.Cross(projectedVec, trunk.transform.up));
        if(Vector3.Dot(trunk.transform.up,planeNormal) < 0)
            trunk_side_bending = -trunk_side_bending;

        if(trunk_side_bending < -45)
            trunk_side_bending = -45;
        if(trunk_side_bending > 45)
            trunk_side_bending = 45;

        if(trunk_side_bending != 0)
        {
            planeNormal2 = Vector3.Cross(projectedVec,trunk.transform.up);
            projectedVec2 = Vector3.ProjectOnPlane(trunk.transform.right, planeNormal2);

            trunk_twist = Vector3.SignedAngle(projectedVec2, trunk.transform.right, trunk.transform.up);
        }

        else
            trunk_twist = -Vector3.SignedAngle(trunk.transform.right, Vector3.Cross(trunk_init_vec,trunk_init_perp_vec), trunk.transform.up);

        if(trunk_twist > 90)
            trunk_twist = 90;
        if(trunk_twist < -90)
            trunk_twist = -90;

        // Neck
        planeNormal = Vector3.Cross(neck_init_vec, neck_init_perp_vec);
        object_vec = trunk.transform.InverseTransformDirection(neck.transform.up);
        projectedVec = Vector3.ProjectOnPlane(object_vec, planeNormal);

        neck_bending = -Vector3.SignedAngle(projectedVec, neck_init_vec, Vector3.Cross(neck_init_vec, neck_init_perp_vec));
        neck_side_bending = -Vector3.SignedAngle(projectedVec, object_vec, Vector3.Cross(projectedVec, object_vec));
        if(Vector3.Dot(object_vec, planeNormal) < 0)
            neck_side_bending = -neck_side_bending;

        if(neck_side_bending != 0)
        {
            object_vec2 = trunk.transform.InverseTransformDirection(neck.transform.right);
            planeNormal2 = Vector3.Cross(projectedVec,object_vec);
            projectedVec2 = Vector3.ProjectOnPlane(object_vec2, planeNormal2);

            neck_twist = Vector3.SignedAngle(projectedVec2, object_vec2, object_vec);
        }

        else
            neck_twist = -Vector3.SignedAngle(trunk.transform.InverseTransformDirection(neck.transform.right), Vector3.Cross(neck_init_vec,neck_init_perp_vec), object_vec);

        // Right_arm
        planeNormal = Vector3.Cross(upper_right_arm_init_vec, upper_right_arm_init_perp_vec);
        object_vec = trunk.transform.InverseTransformDirection(upper_right_arm.transform.right);
        projectedVec = Vector3.ProjectOnPlane(object_vec, planeNormal);

        upper_right_arm_flexion = -Vector3.SignedAngle(projectedVec, upper_right_arm_init_vec, Vector3.Cross(upper_right_arm_init_vec, upper_right_arm_init_perp_vec));

        if(projectedVec.magnitude < 0.001)
            upper_right_arm_flexion = 0;

        if(upper_right_arm_flexion < -18)
            upper_right_arm_flexion = -18;
        if(upper_right_arm_flexion > 180)
            upper_right_arm_flexion = 180;

        upper_right_arm_abduction = -(Vector3.SignedAngle(-planeNormal, object_vec, Vector3.Cross(-planeNormal,object_vec))-90);
        //Debug.Log( Vector3.SignedAngle(-planeNormal,object_vec, upper_right_arm_init_perp_vec));
        if(Vector3.SignedAngle(-planeNormal,object_vec, upper_right_arm_init_perp_vec) > 0)
            upper_right_arm_abduction = 180-upper_right_arm_abduction;

        if(upper_right_arm_abduction < 0)
            upper_right_arm_abduction = 0;

        if(upper_right_arm_abduction > 180)
            upper_right_arm_abduction  = 180;

        if(upper_right_arm_abduction != 0)
        {
            object_vec2 = trunk.transform.InverseTransformDirection(upper_right_arm.transform.up);
            planeNormal2 = Vector3.Cross(projectedVec,object_vec);
            projectedVec2 = Vector3.ProjectOnPlane(object_vec2, planeNormal2);

            upper_right_arm_rotation = -Vector3.SignedAngle(projectedVec2, object_vec2, object_vec);
        }

        else
            upper_right_arm_rotation = Vector3.SignedAngle(trunk.transform.InverseTransformDirection(upper_right_arm.transform.up), Vector3.Cross(upper_right_arm_init_perp_vec, upper_right_arm_init_vec), object_vec);

        if(projectedVec.magnitude < 0.001)
            upper_right_arm_rotation = 0;

        if(upper_right_arm_rotation < -90)
            upper_right_arm_rotation = -90;
        if(upper_right_arm_rotation > 90)
            upper_right_arm_rotation = 90;

        object_vec = upper_right_arm.transform.InverseTransformDirection(lower_right_arm.transform.right);
        lower_right_arm_flexion = -Vector3.SignedAngle(object_vec, lower_right_arm_init_vec, Vector3.Cross(lower_right_arm_init_vec, lower_right_arm_init_perp_vec));

        if(lower_right_arm_flexion < 0)
            lower_right_arm_flexion = 0;

        if(lower_right_arm_flexion > 144)
            lower_right_arm_flexion = 144;

        // Left_arm
        planeNormal = Vector3.Cross(upper_left_arm_init_vec, upper_left_arm_init_perp_vec);
        object_vec = trunk.transform.InverseTransformDirection(upper_left_arm.transform.right);
        projectedVec = Vector3.ProjectOnPlane(object_vec, planeNormal);

        upper_left_arm_flexion = Vector3.SignedAngle(projectedVec, upper_left_arm_init_vec, Vector3.Cross(upper_left_arm_init_vec, upper_left_arm_init_perp_vec));
        upper_left_arm_abduction = Vector3.SignedAngle(projectedVec, object_vec, Vector3.Cross(projectedVec, object_vec));
        if(Vector3.Dot(object_vec, planeNormal) < 0)
            upper_left_arm_abduction = -upper_left_arm_abduction;

        if(upper_left_arm_abduction != 0)
        {
            object_vec2 = trunk.transform.InverseTransformDirection(upper_left_arm.transform.up);
            planeNormal2 = Vector3.Cross(projectedVec,object_vec);
            projectedVec2 = Vector3.ProjectOnPlane(object_vec2, planeNormal2);

            upper_left_arm_rotation = -Vector3.SignedAngle(projectedVec2, object_vec2, object_vec);
        }

        else
            upper_left_arm_rotation = Vector3.SignedAngle(trunk.transform.InverseTransformDirection(upper_left_arm.transform.up), Vector3.Cross(upper_left_arm_init_perp_vec, upper_left_arm_init_vec), object_vec);

        object_vec = upper_left_arm.transform.InverseTransformDirection(lower_left_arm.transform.right);
        lower_left_arm_flexion = Vector3.SignedAngle(object_vec, lower_left_arm_init_vec, Vector3.Cross(lower_left_arm_init_vec, lower_left_arm_init_perp_vec));

    }

    float formatAngle(float angle)
    {
        if(angle > 180)
            angle -= 360;
        if(angle < -180)
            angle += 360;

        return angle;
    }

    void write_joint_positions()
    {
        var  fileName = "joint_positions.txt";

        if (File.Exists(fileName))
            File.Delete(fileName);

        var sr = File.CreateText(fileName);

        string joint_positions = get_joint_positions();

        Debug.Log(joint_positions);

        sr.WriteLine (joint_positions);

        sr.Close();
    }

    private string get_joint_positions()
    {
        string joint_positions = "";

        joint_positions = trunk.transform.position.x .ToString() + " " + trunk.transform.position.y .ToString() + " " + trunk.transform.position.z .ToString() + "\n";
        joint_positions += upper_left_arm.transform.position.x .ToString() + " " + upper_left_arm.transform.position.y .ToString() + " " + upper_left_arm.transform.position.z .ToString() + "\n";
        joint_positions += lower_left_arm.transform.position.x .ToString() + " " + lower_left_arm.transform.position.y .ToString() + " " + lower_left_arm.transform.position.z .ToString() + "\n";
        joint_positions += left_hand.transform.position.x .ToString() + " " + left_hand.transform.position.y .ToString() + " " + left_hand.transform.position.z .ToString() + "\n";
        joint_positions += left_hand_ring.transform.position.x .ToString() + " " + left_hand_ring.transform.position.y .ToString() + " " + left_hand_ring.transform.position.z .ToString() + "\n";
        joint_positions += upper_right_arm.transform.position.x .ToString() + " " + upper_right_arm.transform.position.y .ToString() + " " + upper_right_arm.transform.position.z .ToString() + "\n";
        joint_positions += lower_right_arm.transform.position.x .ToString() + " " + lower_right_arm.transform.position.y .ToString() + " " + lower_right_arm.transform.position.z .ToString() + "\n";
        joint_positions += right_hand.transform.position.x .ToString() + " " + right_hand.transform.position.y .ToString() + " " + right_hand.transform.position.z .ToString() + "\n";
        joint_positions += right_hand_ring.transform.position.x .ToString() + " " + right_hand_ring.transform.position.y .ToString() + " " + right_hand_ring.transform.position.z .ToString() + "\n";
        joint_positions += upper_left_leg.transform.position.x .ToString() + " " + upper_left_leg.transform.position.y .ToString() + " " + upper_left_leg.transform.position.z .ToString() + "\n";
        joint_positions += upper_right_leg.transform.position.x .ToString() + " " + upper_right_leg.transform.position.y .ToString() + " " + upper_right_leg.transform.position.z .ToString() + "\n";
        joint_positions += lower_left_leg.transform.position.x .ToString() + " " + lower_left_leg.transform.position.y .ToString() + " " + lower_left_leg.transform.position.z .ToString() + "\n";
        joint_positions += left_foot.transform.position.x .ToString() + " " + left_foot.transform.position.y .ToString() + " " + left_foot.transform.position.z .ToString() + "\n";
        joint_positions += left_toe.transform.position.x .ToString() + " " + left_toe.transform.position.y .ToString() + " " + left_toe.transform.position.z .ToString() + "\n";
        joint_positions += lower_right_leg.transform.position.x .ToString() + " " + lower_right_leg.transform.position.y .ToString() + " " + lower_right_leg.transform.position.z .ToString() + "\n";
        joint_positions += right_foot.transform.position.x .ToString() + " " + right_foot.transform.position.y .ToString() + " " + right_foot.transform.position.z .ToString() + "\n";
        joint_positions += right_toe.transform.position.x .ToString() + " " + right_toe.transform.position.y .ToString() + " " + right_toe.transform.position.z .ToString() + "\n";
        joint_positions += neck.transform.position.x .ToString() + " " + neck.transform.position.y .ToString() + " " + neck.transform.position.z .ToString() + "\n";
        joint_positions += head.transform.position.x .ToString() + " " + head.transform.position.y .ToString() + " " + head.transform.position.z .ToString();

        return joint_positions;
    }

    private float[] get_joint_angle_values()
    {
        float[] joint_angles = new float[11]{trunk_bending, trunk_side_bending, trunk_twist, upper_right_arm_flexion, upper_right_arm_abduction, upper_right_arm_rotation, lower_right_arm_flexion,
        upper_left_arm_flexion, upper_left_arm_abduction, upper_left_arm_rotation, lower_left_arm_flexion};

        return joint_angles;
    }

    // sendData
    private void sendString(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void sendJointsViaUDP()
    {
        var array = get_joint_angle_values();
        //Debug.Log(array[0] + "   " + array[1] + "   " + array[2] + "   " + array[3] + "   " + array[4] + "   " + array[5] + "   " + array[6]);
        send_array_UDP(array);
    }

    private void send_array_UDP(float[] array)
    {
        string output = "";
        foreach( var item in array) output += item.ToString() + "  ";

        //Debug.Log("initial: " + output);

        var byteArray = new byte[array.Length * 4];
        Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
        try
        {
            //byte[] val = BitConverter.GetBytes(byteArray);
            //Debug.Log(array[0] + "   " + array[1] + "   " + array[2] + "   " + array[3] + "   " + array[4] + "   " + array[5] + "   " + array[6]);
            client_soc.SendTo(byteArray, remoteEndPoint);
            //Debug.Log(BitConverter.ToString(byteArray));
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void sendLimbsLengthsViaUDP()
    {
        var array = new float[5]{legs_length, trunk_length, right_shoulder_length, right_arm_length, right_lower_arm_length};
        //Debug.Log(array[0] + "   " + array[1] + "   " + array[2] + "   " + array[3] + "   " + array[4]);
        send_array_UDP(array);
    }

	private void ThreadMethod()
	{
	    udp = new UdpClient(11311);
	    while (true)
	    {
	        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

			try
			{
				if (udp.Available > 0) // Only read if we have some data
				{
			        byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

			        /*lock object to make sure there data is
			        *not being accessed from multiple threads at thesame time*/
			        lock (lockObject)
			        {
        				GCHandle handle = GCHandle.Alloc(receiveBytes, GCHandleType.Pinned);
        				try
        			    {
        			        skel = (Skel)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Skel));
        							precessData = true;

                        if(first_time == true)
                        {

                            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
                            nuitrack.Joint joint = skeleton.GetJoint(typeJoint[15]);
                            Vector3 newPosition = 0.001f * joint.ToVector3();

                            init_pos[0,0] = newPosition.x;
                            init_pos[0,1] = newPosition.y;
                            init_pos[0,2] = newPosition.z;

                            first_time = false;


                        }
					}

					finally
				    {
				        handle.Free();
				    }

									//Debug.Log("right hand pos x = " + skel.x[15] + ", right hand pos y = " + skel.y[15] + ", right hand pos z = " + skel.z[15]);
									//right_hand.transform.Translate(skel.x[15]+200, 0.0f, 0.0f, Space.Self);
								  //int BufferSize = Marshal.SizeOf(typeof(Skeleton));
								  //NewStuff stuff = (NewStuff)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NewStuff));
			            //returnData = Encoding.ASCII.GetString(receiveBytes);

			            /*if (returnData == "1\n")
			            {
			                //Done, notify the Update function
			                precessData = true;
			            }*/
			        }
				}
			}
			catch(Exception e)
			{

			}
	    }
	}

	void OnApplicationQuit()
	{
		udp?.Close();
		thread?.Abort();
	}

    void updateOptPosture()
	{
		try
		{
			if (client_soc_opt.Available > 0)
			{
				byte[] bytes = new byte[client_soc_opt.Available];

				int bytesReceived = client_soc_opt.Receive(bytes);//, SocketFlags.None);

				//string byte_output = "";
				//foreach(var b in bytes) byte_output += b.ToString() + " ";
				//Debug.Log(byte_output);

				//Debug.Log(client_soc.Available);

				var j_angles = intConversion(bytes.Reverse().ToArray()).Reverse().ToArray();//floatConversion(bytes.Reverse().ToArray());

				//int[] j_angles = bytes.Select(x => (int)x).ToArray();

				string output = "";
				foreach(var angle in j_angles) output += angle.ToString() + "  ";

				//Debug.Log(output);

				setOptPosture(j_angles);
			}
		}
		catch (SocketException ex)
		{
			if (ex.SocketErrorCode == SocketError.Interrupted)
			{

			}
			else
			{
				throw;
			}
		}
	}

    bool limbsLengthReceived()
    {
        if(!limbsLengthSent)
        {
            try
    		{
    			if (client_soc_opt.Available > 0)
                    return true;

                return false;
    		}
    		catch (SocketException ex)
    		{
    			if (ex.SocketErrorCode == SocketError.Interrupted)
    			{

    			}
    			else
    			{
    				throw;
    			}

                return false;
    		}
        }
        else
            return true;
    }

	private float[] floatConversion(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes); // Convert big endian to little endian
        }

        float[] myFloat = new float[bytes.Length/4];
		for(int j = 0; j <= bytes.Length - 4; j += 4)
		{
			myFloat[j/4] = BitConverter.ToSingle(bytes, j);
			myFloat[j/4] = myFloat[j/4]*180f/3.14159265359f;
		}
        return myFloat;
    }

	private int[] intConversion(byte[] bytes)
	{
		/*if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes); // Convert big endian to little endian
		}$/

		/*int[] myInt = new int[bytes.Length/4];
		for(int j = 0; j <= bytes.Length - 4; j += 4)
			myInt[j/4] = BitConverter.ToInt32(bytes, j);*/

		int[] myInt = new int[bytes.Length/ 4];
		for (var index = 0; index < bytes.Length/4; index++)
		{
			byte[] cur_bytes = {bytes[index*4],bytes[(index*4)+1],bytes[(index*4)+2],bytes[(index*4)+3]};
			Array.Reverse(cur_bytes);
		    myInt[index] = BitConverter.ToInt32(cur_bytes, 0);
		}

		return myInt;
	}

	private void setOptPostureOld(int[] joint_angles)
	{
        trunk_opt.transform.localEulerAngles = new Vector3(joint_angles[0],joint_angles[2],joint_angles[1]);
        //upper_right_arm_opt.transform.Rotate(0.0f,0.0f,(joint_angles[4] - upper_right_arm_abduction_opt),Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,0*-(joint_angles[3] - upper_right_arm_flexion_opt),(joint_angles[4] - upper_right_arm_abduction_opt),Space.Self);
        //upper_right_arm_opt.transform.Rotate(0.0f,0.0f,(joint_angles[4] - upper_right_arm_abduction_opt),Space.Self);
        //upper_right_arm_opt.transform.Rotate(-(joint_angles[5] - upper_right_arm_rotation_opt),0.0f,0.0f,Space.Self);
        lower_right_arm_opt.transform.localRotation = Quaternion.Euler(0, -joint_angles[6], 0);

        /*//trunk_opt.transform.Rotate(0.0f, joint_angles[2] - trunk_twist_opt, 0.0f, Space.Self);
        trunk_opt.transform.Rotate(joint_angles[0] - trunk_bending_opt, 0.0f, 0.0f, Space.Self);
		//trunk_opt.transform.Rotate(0.0f,0.0f,joint_angles[1] - trunk_side_bending_opt, Space.Self);
		upper_right_arm_opt.transform.Rotate(0.0f,0.0f,(joint_angles[4] - upper_right_arm_abduction_opt),Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,-(joint_angles[3] - upper_right_arm_flexion_opt),0.0f,Space.Self);
		//upper_right_arm_opt.transform.Rotate(-(joint_angles[5] - upper_right_arm_rotation_opt),0.0f,0.0f,Space.Self);
		lower_right_arm_opt.transform.Rotate(0.0f, (joint_angles[6] - lower_right_arm_flexion_opt), 0.0f, Space.Self);
		/*upper_left_arm_opt.transform.Rotate(0.0f,joint_angles[7] - upper_left_arm_flexion_opt,-(joint_angles[8] - upper_left_arm_abduction_opt), Space.Self);
		upper_left_arm_opt.transform.Rotate(-(joint_angles[9] - upper_left_arm_rotation_opt),0.0f,0.0f, Space.Self);
		lower_left_arm_opt.transform.Rotate(0.0f, joint_angles[10] - lower_left_arm_flexion_opt, 0.0f, Space.Self);*/

		//Debug.Log(joint_angles[5]);
		trunk_bending_opt = joint_angles[0];
		trunk_side_bending_opt = joint_angles[1];
		trunk_twist_opt = joint_angles[2];
		upper_right_arm_flexion_opt = joint_angles[3];
		upper_right_arm_abduction_opt = joint_angles[4];
		upper_right_arm_rotation_opt = joint_angles[5];
		lower_right_arm_flexion_opt = joint_angles[6];
		upper_left_arm_flexion_opt = joint_angles[7];
		upper_left_arm_abduction_opt = joint_angles[8];
		upper_left_arm_rotation_opt = joint_angles[9];
		lower_left_arm_flexion_opt = joint_angles[10];

	}

    private void setOptPosture(int[] joint_angles)
	{

        trunk_opt.transform.localEulerAngles = new Vector3(0,-0.1851f,0);
        upper_right_arm_opt.transform.localEulerAngles = new Vector3(-13.093f,8.073f, -73.86f);
        upper_left_arm_opt.transform.localEulerAngles = new Vector3(-13.093f,8.073f, 73.86f);
        lower_right_arm_opt.transform.localEulerAngles = new Vector3(0,0,0);
        lower_left_arm_opt.transform.localEulerAngles = new Vector3(0,0,0);

        trunk_opt.transform.Rotate(joint_angles[0], 0.0f, 0.0f, Space.Self);
        trunk_opt.transform.Rotate(0.0f, 0.0f, joint_angles[1], Space.Self);
        trunk_opt.transform.Rotate(0.0f, joint_angles[2], 0.0f, Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,-joint_angles[3],0.0f,Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,0.0f,joint_angles[4],Space.Self);
        upper_right_arm_opt.transform.Rotate(-joint_angles[5],0.0f,0.0f,Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,joint_angles[7],0.0f, Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,0.0f,-joint_angles[8], Space.Self);
        upper_left_arm_opt.transform.Rotate(joint_angles[9],0.0f,0.0f, Space.Self);
        lower_right_arm_opt.transform.Rotate(0.0f, -joint_angles[6], 0.0f, Space.Self);
        lower_left_arm_opt.transform.Rotate(0.0f, -joint_angles[10], 0.0f, Space.Self);

        /*trunk_opt.transform.Rotate(joint_angles[0]-trunk_bending_opt, 0.0f, 0.0f, Space.Self);
        trunk_opt.transform.Rotate(0.0f, 0.0f, joint_angles[1] - trunk_side_bending_opt, Space.Self);
        trunk_opt.transform.Rotate(0.0f, joint_angles[2] - trunk_twist_opt, 0.0f, Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,-joint_angles[3]+upper_right_arm_flexion_opt,0.0f,Space.Self);
        upper_right_arm_opt.transform.Rotate(0.0f,0.0f,joint_angles[4]-upper_right_arm_abduction_opt,Space.Self);
        upper_right_arm_opt.transform.Rotate(-joint_angles[5]+upper_right_arm_rotation_opt,0.0f,0.0f,Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,joint_angles[7]-upper_left_arm_flexion_opt,0.0f, Space.Self);
        upper_left_arm_opt.transform.Rotate(0.0f,0.0f,joint_angles[8]-upper_left_arm_abduction_opt, Space.Self);
        upper_left_arm_opt.transform.Rotate(joint_angles[9]-upper_left_arm_rotation_opt,0.0f,0.0f, Space.Self);
        lower_right_arm_opt.transform.Rotate(0.0f, -joint_angles[6]+lower_right_arm_flexion_opt, 0.0f, Space.Self);
        lower_left_arm_opt.transform.Rotate(0.0f, joint_angles[10]-lower_left_arm_flexion_opt, 0.0f, Space.Self);*/

		//Debug.Log(joint_angles[0] + " " + joint_angles[1] + " " + joint_angles[2] + " " + joint_angles[7] + " " + joint_angles[8] + " " + joint_angles[9] + " " + joint_angles[10]);
		trunk_bending_opt = joint_angles[0];
		trunk_side_bending_opt = joint_angles[1];
		trunk_twist_opt = joint_angles[2];
		upper_right_arm_flexion_opt = joint_angles[3];
		upper_right_arm_abduction_opt = joint_angles[4];
		upper_right_arm_rotation_opt = joint_angles[5];
		lower_right_arm_flexion_opt = joint_angles[6];
		upper_left_arm_flexion_opt = joint_angles[7];
		upper_left_arm_abduction_opt = joint_angles[8];
		upper_left_arm_rotation_opt = joint_angles[9];
		lower_left_arm_flexion_opt = joint_angles[10];

	}

    private void initializeLimbsLength(nuitrack.Skeleton skeleton)
    {
        nuitrack.Joint trunkJoint = skeleton.GetJoint(modelJoints[0].jointType.TryGetMirrored());
        nuitrack.Joint neckJoint = skeleton.GetJoint(modelJoints[7].jointType.TryGetMirrored());
        nuitrack.Joint leftShoulderJoint = skeleton.GetJoint(modelJoints[1].jointType.TryGetMirrored());
        nuitrack.Joint rightShoulderJoint = skeleton.GetJoint(modelJoints[2].jointType.TryGetMirrored());
        nuitrack.Joint leftElbowJoint = skeleton.GetJoint(modelJoints[3].jointType.TryGetMirrored());
        nuitrack.Joint rightElbowJoint = skeleton.GetJoint(modelJoints[4].jointType.TryGetMirrored());
        nuitrack.Joint leftWristJoint = skeleton.GetJoint(modelJoints[8].jointType.TryGetMirrored());
        nuitrack.Joint rightWristJoint = skeleton.GetJoint(modelJoints[9].jointType.TryGetMirrored());
        nuitrack.Joint waistJoint = skeleton.GetJoint(modelJoints[10].jointType.TryGetMirrored());
        nuitrack.Joint leftHipJoint = skeleton.GetJoint(modelJoints[11].jointType.TryGetMirrored());
        nuitrack.Joint rightHipJoint = skeleton.GetJoint(modelJoints[12].jointType.TryGetMirrored());
        nuitrack.Joint leftKneeJoint = skeleton.GetJoint(modelJoints[13].jointType.TryGetMirrored());
        nuitrack.Joint rightKneeJoint = skeleton.GetJoint(modelJoints[14].jointType.TryGetMirrored());
        nuitrack.Joint leftAnkleJoint = skeleton.GetJoint(modelJoints[15].jointType.TryGetMirrored());
        nuitrack.Joint rightAnkleJoint = skeleton.GetJoint(modelJoints[16].jointType.TryGetMirrored());

        float[] center_hip = {0.5f*(leftHipJoint.Real.X + rightHipJoint.Real.X), 0.5f*(leftHipJoint.Real.Y + rightHipJoint.Real.Y), 0.5f*(leftHipJoint.Real.Z + rightHipJoint.Real.Z)};
        trunk_length = 0.001f*Mathf.Sqrt(Mathf.Pow(neckJoint.Real.X - waistJoint.Real.X,2)+Mathf.Pow(neckJoint.Real.Y - waistJoint.Real.Y,2)+Mathf.Pow(neckJoint.Real.Z - waistJoint.Real.Z,2)) + 0.001f*Mathf.Sqrt(Mathf.Pow(center_hip[0] - waistJoint.Real.X,2)+Mathf.Pow(center_hip[1] - waistJoint.Real.Y,2)+Mathf.Pow(center_hip[2] - waistJoint.Real.Z,2));
        right_shoulder_length = 0.001f*Mathf.Sqrt(Mathf.Pow(neckJoint.Real.X - rightShoulderJoint.Real.X,2)+Mathf.Pow(neckJoint.Real.Y - rightShoulderJoint.Real.Y,2)+Mathf.Pow(neckJoint.Real.Z - rightShoulderJoint.Real.Z,2));
        left_shoulder_length = 0.001f*Mathf.Sqrt(Mathf.Pow(neckJoint.Real.X - leftShoulderJoint.Real.X,2)+Mathf.Pow(neckJoint.Real.Y - leftShoulderJoint.Real.Y,2)+Mathf.Pow(neckJoint.Real.Z - leftShoulderJoint.Real.Z,2));
        right_arm_length = 0.001f*Mathf.Sqrt(Mathf.Pow(rightElbowJoint.Real.X - rightShoulderJoint.Real.X,2)+Mathf.Pow(rightElbowJoint.Real.Y - rightShoulderJoint.Real.Y,2)+Mathf.Pow(rightElbowJoint.Real.Z - rightShoulderJoint.Real.Z,2));
        left_arm_length = 0.001f*Mathf.Sqrt(Mathf.Pow(leftElbowJoint.Real.X - leftShoulderJoint.Real.X,2)+Mathf.Pow(leftElbowJoint.Real.Y - leftShoulderJoint.Real.Y,2)+Mathf.Pow(leftElbowJoint.Real.Z - leftShoulderJoint.Real.Z,2));
        right_lower_arm_length = 0.001f*Mathf.Sqrt(Mathf.Pow(rightElbowJoint.Real.X - rightWristJoint.Real.X,2)+Mathf.Pow(rightElbowJoint.Real.Y - rightWristJoint.Real.Y,2)+Mathf.Pow(rightElbowJoint.Real.Z - rightWristJoint.Real.Z,2));
        left_lower_arm_length = 0.001f*Mathf.Sqrt(Mathf.Pow(leftElbowJoint.Real.X - leftWristJoint.Real.X,2)+Mathf.Pow(leftElbowJoint.Real.Y - leftWristJoint.Real.Y,2)+Mathf.Pow(leftElbowJoint.Real.Z - leftWristJoint.Real.Z,2));
        float left_leg_length = 0.001f*Mathf.Sqrt(Mathf.Pow(leftHipJoint.Real.X - leftKneeJoint.Real.X,2)+Mathf.Pow(leftHipJoint.Real.Y - leftKneeJoint.Real.Y,2)+Mathf.Pow(leftHipJoint.Real.Z - leftKneeJoint.Real.Z,2)) + 0.001f*Mathf.Sqrt(Mathf.Pow(leftAnkleJoint.Real.X - leftKneeJoint.Real.X,2)+Mathf.Pow(leftAnkleJoint.Real.Y - leftKneeJoint.Real.Y,2)+Mathf.Pow(leftAnkleJoint.Real.Z - leftKneeJoint.Real.Z,2));
        float right_leg_length = 0.001f*Mathf.Sqrt(Mathf.Pow(rightHipJoint.Real.X - rightKneeJoint.Real.X,2)+Mathf.Pow(rightHipJoint.Real.Y - rightKneeJoint.Real.Y,2)+Mathf.Pow(rightHipJoint.Real.Z - rightKneeJoint.Real.Z,2)) + 0.001f*Mathf.Sqrt(Mathf.Pow(rightAnkleJoint.Real.X - rightKneeJoint.Real.X,2)+Mathf.Pow(rightAnkleJoint.Real.Y - rightKneeJoint.Real.Y,2)+Mathf.Pow(rightAnkleJoint.Real.Z - rightKneeJoint.Real.Z,2));
        legs_length = 0.5f*(left_leg_length + right_leg_length);
    }
}
