using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEditor;
using Unity.Jobs;
using Unity.Burst;

public class SetRotationsOptimal : MonoBehaviour {

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

	public float neck_bending_cur = 0, neck_twist_cur = 0, neck_side_bending_cur = 0;
	public float trunk_bending_cur = 0, trunk_twist_cur = 0, trunk_side_bending_cur = 0;
	public float upper_right_leg_flexion_cur = 0, upper_right_leg_abduction_cur = 0, upper_right_leg_rotation_cur = 0;
	public float upper_left_leg_flexion_cur = 0, upper_left_leg_abduction_cur = 0, upper_left_leg_rotation_cur = 0;
	public float lower_left_leg_flexion_cur = 0, lower_right_leg_flexion_cur = 0;
	public float left_foot_flexion_cur = 0, left_foot_twist_cur = 0, left_foot_bending_cur = 0;
	public float right_foot_flexion_cur = 0, right_foot_twist_cur = 0, right_foot_bending_cur = 0;
	public float upper_right_arm_raise_cur = 0, upper_right_arm_flexion_cur = 0, upper_right_arm_abduction_cur = 0, upper_right_arm_rotation_cur = 0;
	public float upper_left_arm_raise_cur = 0, upper_left_arm_flexion_cur = 0, upper_left_arm_abduction_cur = 0, upper_left_arm_rotation_cur = 0;
	public float lower_left_arm_flexion_cur = 0, lower_right_arm_flexion_cur = 0;
	public float left_hand_flexion_cur = 0, left_hand_twist_cur = 0, left_hand_bending_cur = 0;
	public float right_hand_flexion_cur = 0, right_hand_twist_cur = 0, right_hand_bending_cur = 0;


	private GameObject neck, trunk, upper_right_leg, upper_left_leg, upper_right_arm, upper_left_arm, right_shoulder, left_shoulder, lower_left_leg, lower_right_leg, left_foot, right_foot;
	private GameObject lower_right_arm, lower_left_arm, left_hand, right_hand;

	private GameObject neck_cur, trunk_cur, upper_right_leg_cur, upper_left_leg_cur, upper_right_arm_cur, upper_left_arm_cur, right_shoulder_cur, left_shoulder_cur;
	private GameObject lower_left_leg_cur, lower_right_leg_cur, left_foot_cur, right_foot_cur;
	private GameObject lower_right_arm_cur, lower_left_arm_cur, left_hand_cur, right_hand_cur;

	Thread thread;
	static UdpClient udp;
	static readonly object lockObject = new object();
	bool precessData = false;
	string returnData = "";

	IPEndPoint remoteEndPoint;
    UdpClient client;
    private string IP;
    private int port;
    string strMessage = "";
    Socket client_soc;

	public bool getOptAnglesFromTextFile = false;

	// Use this for initialization
	void Start () {
		if(getOptAnglesFromTextFile)
		{
			float[] angles = readAnglesFromTextFile(Application.streamingAssetsPath + "/anglesOpt.txt");
			initializeJointAngles(angles);
		}

		neck = GameObject.FindWithTag("Neck_Opt");
		trunk = GameObject.FindWithTag("Trunk_Opt");
		upper_right_leg = GameObject.FindWithTag("Upper_Right_Leg_Opt");
		upper_left_leg = GameObject.FindWithTag("Upper_Left_Leg_Opt");
		lower_right_leg = GameObject.FindWithTag("Lower_Right_Leg_Opt");
		lower_left_leg = GameObject.FindWithTag("Lower_Left_Leg_Opt");
		left_foot = GameObject.FindWithTag("Left_Foot_Opt");
		right_foot = GameObject.FindWithTag("Right_Foot_Opt");
		right_shoulder = GameObject.FindWithTag("Right_Shoulder_Opt");
		left_shoulder = GameObject.FindWithTag("Left_Shoulder_Opt");
		upper_right_arm = GameObject.FindWithTag("Upper_Right_Arm_Opt");
		upper_left_arm = GameObject.FindWithTag("Upper_Left_Arm_Opt");
		lower_right_arm = GameObject.FindWithTag("Lower_Right_Arm_Opt");
		lower_left_arm = GameObject.FindWithTag("Lower_Left_Arm_Opt");
		left_hand = GameObject.FindWithTag("Left_Hand_Opt");
		right_hand = GameObject.FindWithTag("Right_Hand_Opt");

		neck_cur = GameObject.FindWithTag("Neck");
		trunk_cur = GameObject.FindWithTag("Trunk");
		upper_right_leg_cur = GameObject.FindWithTag("Upper_Right_Leg");
		upper_left_leg_cur = GameObject.FindWithTag("Upper_Left_Leg");
		lower_right_leg_cur = GameObject.FindWithTag("Lower_Right_Leg");
		lower_left_leg_cur = GameObject.FindWithTag("Lower_Left_Leg");
		left_foot_cur = GameObject.FindWithTag("Left_Foot");
		right_foot_cur = GameObject.FindWithTag("Right_Foot");
		right_shoulder_cur = GameObject.FindWithTag("Right_Shoulder");
		left_shoulder_cur = GameObject.FindWithTag("Left_Shoulder");
		upper_right_arm_cur = GameObject.FindWithTag("Upper_Right_Arm");
		upper_left_arm_cur = GameObject.FindWithTag("Upper_Left_Arm");
		lower_left_arm_cur = GameObject.FindWithTag("Lower_Left_Arm");
		lower_right_arm_cur = GameObject.FindWithTag("Lower_Right_Arm");
		left_hand_cur = GameObject.FindWithTag("Left_Hand");
		right_hand_cur = GameObject.FindWithTag("Right_Hand");

		neck.transform.Rotate(neck_bending, neck_twist, neck_side_bending, Space.Self);
		trunk.transform.Rotate(trunk_bending, trunk_twist, trunk_side_bending, Space.Self);
		upper_right_leg.transform.Rotate(-upper_right_leg_flexion,upper_right_leg_rotation,upper_right_leg_abduction, Space.Self);
		upper_left_leg.transform.Rotate(-upper_left_leg_flexion,-upper_left_leg_rotation,-upper_left_leg_abduction, Space.Self);
		upper_right_arm.transform.Rotate(-upper_right_arm_rotation,-upper_right_arm_flexion,upper_right_arm_abduction,Space.Self);
		upper_left_arm.transform.Rotate(-upper_left_arm_rotation,upper_left_arm_flexion,-upper_left_arm_abduction, Space.Self);
		lower_right_leg.transform.Rotate(lower_right_leg_flexion, 0.0f, 0.0f, Space.Self);
		lower_left_leg.transform.Rotate(lower_left_leg_flexion, 0.0f, 0.0f, Space.Self);
		left_foot.transform.Rotate(left_foot_flexion, -left_foot_twist, -left_foot_bending, Space.Self);
		right_foot.transform.Rotate(right_foot_flexion, right_foot_twist, right_foot_bending, Space.Self);

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
		lower_right_arm.transform.Rotate(0.0f, -lower_right_arm_flexion, 0.0f, Space.Self);
		lower_left_arm.transform.Rotate(0.0f, lower_left_arm_flexion, 0.0f, Space.Self);
		left_hand.transform.Rotate(-left_hand_twist, -left_hand_bending, left_hand_flexion, Space.Self);
		right_hand.transform.Rotate(right_hand_twist, right_hand_bending, -right_hand_flexion, Space.Self);

		//thread = new Thread(new ThreadStart(ThreadMethod));
        //thread.Start();

		initializeUDP();
	}

	// Update is called once per frame
	//void Update () {
	//	updateOptPosture();
	//}

	void Update()
    {
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
						returnData = Encoding.ASCII.GetString(receiveBytes);

						Debug.Log(returnData);

						if (returnData == "1\n")
			            {
			                //Done, notify the Update function
			                precessData = true;
			            }
			        }
				}
			}
			catch(Exception e)
			{

			}
	    }
	}
	
	void initializeUDP()
    {

        Debug.Log("UDPSend.init()");

        IP="127.0.0.1";
        port=8052;

        // ----------------------------
        // Senden
        // ----------------------------
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        // status
        Debug.Log("Receiving from "+IP+" : "+port);

        client_soc = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		client_soc.Bind(new IPEndPoint(IPAddress.Any, port));

        //sendString(get_joint_positions());
        //sendDouble(0.1);
	}

	void updateOptPosture()
	{
		try
		{
			if (client_soc.Available > 0)
			{
				byte[] bytes = new byte[client_soc.Available];

				int bytesReceived = client_soc.Receive(bytes);//, SocketFlags.None);

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

	private void setOptPosture(int[] joint_angles)
	{
		trunk.transform.Rotate(0.0f, joint_angles[2] - trunk_twist, 0.0f, Space.Self);
		//trunk.transform.Rotate(joint_angles[0] - trunk_bending, 0.0f, 0.0f, Space.Self);
		trunk.transform.Rotate(0.0f,0.0f,joint_angles[1] - trunk_side_bending, Space.Self);
		/*upper_right_arm.transform.Rotate(0.0f,-(joint_angles[3] - upper_right_arm_flexion),-(joint_angles[4] - upper_right_arm_abduction),Space.Self);
		upper_right_arm.transform.Rotate(-(joint_angles[5] - upper_right_arm_rotation),0.0f,0.0f,Space.Self);
		lower_right_arm.transform.Rotate(0.0f, -(joint_angles[6] - lower_right_arm_flexion), 0.0f, Space.Self);
		upper_left_arm.transform.Rotate(0.0f,joint_angles[7] - upper_left_arm_flexion,-(joint_angles[8] - upper_left_arm_abduction), Space.Self);
		upper_left_arm.transform.Rotate(-(joint_angles[9] - upper_left_arm_rotation),0.0f,0.0f, Space.Self);
		lower_left_arm.transform.Rotate(0.0f, joint_angles[10] - lower_left_arm_flexion, 0.0f, Space.Self);*/

		//Debug.Log(joint_angles[5]);
		trunk_bending = joint_angles[0];
		trunk_side_bending = joint_angles[1];
		trunk_twist = joint_angles[2];
		upper_right_arm_flexion = joint_angles[3];
		upper_right_arm_abduction = joint_angles[4];
		upper_right_arm_rotation = joint_angles[5];
		lower_right_arm_flexion = joint_angles[6];
		upper_left_arm_flexion = joint_angles[7];
		upper_left_arm_abduction = joint_angles[8];
		upper_left_arm_rotation = joint_angles[9];
		lower_left_arm_flexion = joint_angles[10];

	}
}
