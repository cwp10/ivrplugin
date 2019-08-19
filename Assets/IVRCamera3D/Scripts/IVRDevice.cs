/************************************************************************************

Filename    :   IVRDevice.cs
Content     :   Interface for the Immersion VR Device
Created     :   5/14, 2016
Authors     :   zmzhu

NingQi Wang modified it for work on Unity5, 4/2, 2015.
Closed the lines 267 to 307.

Xinjie Wang modified it on 20/4, 2016

************************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


//-------------------------------------------------------------------------------------
// ***** IVRDevice
//
// IVRDevice is the main interface to the Immersion VR hardware. It includes wrapper functions
// for  all exported C++ functions, as well as helper functions that use the stored Immersion
// variables to help set up camera behavior.
//
// This component is added to the IVRCameraController prefab. It can be part of any 
// game object that one sees fit to place it. However, it should only be declared once,
// since there are public members that allow for tweaking certain VR values in the
// Unity inspector.
//
public class IVRDevice : MonoBehaviour
{
    // Imported functions from 
    // IVRPlugin.dll 	(PC)
    // IVRPlugin.bundle (OSX)
    // IVRPlugin.so 	(Linux, Android)

    // MessageList
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageList
    {
        public byte isHMDSensorAttached;
        public byte isHMDAttached;
        public byte isLatencyTesterAttached;

        public MessageList(byte HMDSensor, byte HMD, byte LatencyTester)
        {
            isHMDSensorAttached = HMDSensor;
            isHMDAttached = HMD;
            isLatencyTesterAttached = LatencyTester;
        }
    }


    // GLOBAL FUNCTIONS
    [DllImport("ImmersionPlugin", EntryPoint = "IVR_Update")]
    private static extern bool IVR_Update(ref MessageList messageList);
    [DllImport("ImmersionPlugin")]
    private static extern int IVR_Initialize();
    [DllImport("ImmersionPlugin")]
    private static extern int IVR_Destroy();

    // SENSOR FUNCTIONS
    [DllImport("ImmersionPlugin")]
    private static extern int IVR_GetSensorCount();
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_IsHMDPresent();
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetSensorOrientation(int sensorID,
                                                        ref float w,
                                                        ref float x,
                                                        ref float y,
                                                        ref float z);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetSensorPredictedOrientation(int sensorID,
                                                                 ref float w,
                                                                 ref float x,
                                                                 ref float y,
                                                                 ref float z);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetSensorPredictionTime(int sensorID, ref float predictionTime);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_SetSensorPredictionTime(int sensorID, float predictionTime);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_EnableYawCorrection(int sensorID, float enable);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_ResetSensorOrientation(int sensorID);

    // Latest absolute sensor readings (note: in right-hand co-ordinates)
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetAcceleration(int sensorID,
                                                   ref float x,
                                                   ref float y,
                                                   ref float z);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetAngularVelocity(int sensorID,
                                                   ref float x,
                                                   ref float y,
                                                   ref float z);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetMagnetometer(int sensorID,
                                                   ref float x,
                                                   ref float y,
                                                   ref float z);


    // HMD FUNCTIONS
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_IsSensorPresent(int sensor);
    [DllImport("ImmersionPlugin")]
    private static extern System.IntPtr IVR_GetDisplayDeviceName();
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetScreenResolution(ref int hResolution, ref int vResolution);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetScreenSize(ref float hSize, ref float vSize);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetEyeToScreenDistance(ref float eyeToScreenDistance);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetInterpupillaryDistance(ref float interpupillaryDistance);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetLensSeparationDistance(ref float lensSeparationDistance);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetPlayerEyeHeight(ref float eyeHeight);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetEyeOffset(ref float leftEye, ref float rightEye);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetScreenVCenter(ref float vCenter);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_GetDistortionCoefficients(ref float k0,
                                                             ref float k1,
                                                             ref float k2,
                                                             ref float k3);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_RenderPortraitMode();

    // LATENCY TEST FUNCTIONS
    [DllImport("ImmersionPlugin")]
    private static extern void IVR_ProcessLatencyInputs();
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_DisplayLatencyScreenColor(ref byte r,
                                                            ref byte g,
                                                            ref byte b);
    [DllImport("ImmersionPlugin")]
    private static extern System.IntPtr IVR_GetLatencyResultsString();


    // MAGNETOMETER YAW-DRIFT CORRECTION FUNCTIONS
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_IsMagCalibrated(int sensor);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_EnableMagYawCorrection(int sensor, bool enable);
    [DllImport("ImmersionPlugin")]
    private static extern bool IVR_IsYawCorrectionEnabled(int sensor);

    // Different orientations required for different trackers
    enum SensorOrientation { Head, Other };

    // PUBLIC
    public float InitialPredictionTime = 0.05f; // 50 ms
    public bool ResetTrackerOnNextScene = true;  // if off, tracker will not reset when new scene
                                                 // is loade


    // STATIC
    private static MessageList MsgList = new MessageList(0, 0, 0);
    private static bool IVRInit = false;

    public static int SensorCount = 0;

    public static String DisplayDeviceName;

    public static int HResolution, VResolution = 0;  // pixels
    public static float HScreenSize, VScreenSize = 0.0f;     // meters
    public static float EyeToScreenDistance = 0.0f;  // meters
    public static float LensSeparationDistance = 0.0f;  // meters
    public static float LeftEyeOffset, RightEyeOffset = 0.0f;  // meters
    public static float ScreenVCenter = 0.0f;    // meters 
    public static float DistK0, DistK1, DistK2, DistK3 = 0.0f;

    // Used to reduce the size of render distortion and give better fidelity
    public static float DistortionFitScale = 1.0f;

    // The physical offset of the lenses, used for shifting both IPD and lens distortion
    private static float LensOffsetLeft, LensOffsetRight = 0.0f;

    // Fit to top of the image (default is 5" display)
    private static float DistortionFitX = 0.0f;
    private static float DistortionFitY = 1.0f;

    // Copied from initialized public variables set in editor
    private static float PredictionTime = 0.0f;
    private static bool HMDAttached = false;

    // We will keep map sensors to different numbers to know which sensor is 
    // attached to which device
    private static Dictionary<int, int> SensorList = new Dictionary<int, int>();
    private static Dictionary<int, SensorOrientation> SensorOrientationList =
               new Dictionary<int, SensorOrientation>();

    // * * * * * * * * * * * * *

    // Awake
    void Awake()
    {
        // Initialize static Dictionary lists first
        InitSensorList(false);
        InitOrientationSensorList();
#if !UNITY_EDITOR_OSX
        IVRInit = (IVR_Initialize() >= 0);
#endif

        if (IVRInit == false)
            return;

        // * * * * * * *
        // DISPLAY SETUP

        // We will get the HMD so that we can eventually target it within Unity
        DisplayDeviceName += Marshal.PtrToStringAnsi(IVR_GetDisplayDeviceName());

        IVR_GetScreenResolution(ref HResolution, ref VResolution);
        IVR_GetScreenSize(ref HScreenSize, ref VScreenSize);
        IVR_GetEyeToScreenDistance(ref EyeToScreenDistance);
        IVR_GetLensSeparationDistance(ref LensSeparationDistance);
        IVR_GetEyeOffset(ref LeftEyeOffset, ref RightEyeOffset);
        IVR_GetScreenVCenter(ref ScreenVCenter);

        //by xinjiewang
        //IVR_GetDistortionCoefficients( ref DistK0, ref DistK1, ref DistK2, ref DistK3);
        // We set the children VR distortion parameters ourself
        // Config the parameters separately for Children VR and Nebula VR
        DistK0 = 1.0f;
        DistK1 = 0.034f;
        DistK2 = 0.004f;

        // Distortion fit parameters based on if we are using a 5" (Prototype, DK2+) or 7" (DK1) 
        //if (HScreenSize < 0.140f) 	// 5.5"
        //{
        DistortionFitX = 0.0f;
        DistortionFitY = 1.0f;
        //}
        //  	else 						// 7" (DK1)
        //{
        //	DistortionFitX 		= -1.0f;
        //	DistortionFitY 		=  0.0f;
        //	DistortionFitScale 	=  0.7f;
        //}

        // Calculate the lens offsets for each eye and store 
        CalculatePhysicalLensOffsets(ref LensOffsetLeft, ref LensOffsetRight);

        // * * * * * * *
        // SENSOR SETUP

        SensorCount = IVR_GetSensorCount();

        // PredictionTime set, to init sensor directly
        if (PredictionTime > 0.0f)
            IVR_SetSensorPredictionTime(SensorList[0], PredictionTime);
        else
            SetPredictionTime(SensorList[0], InitialPredictionTime);
    }

    // Start (Note: make sure to always have a Start function for classes that have
    // editors attached to them)
    void Start()
    {
    }

    // Update
    // We can detect if our devices have been plugged or unplugged, as well as
    // run things that need to be updated in our game thread
    void Update()
    {
        //MessageList oldMsgList = MsgList;
#if !UNITY_EDITOR_OSX
        IVR_Update(ref MsgList);
        HMDAttached = MsgList.isHMDAttached > 0 ? true : false;
#endif

        // wnq close it for Unity5
        /*
		// HMD SENSOR
		if((MsgList.isHMDSensorAttached != 0) && 
		   (oldMsgList.isHMDSensorAttached == 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.HMDSensor, true); 
			//Debug.Log("HMD SENSOR ATTACHED");
		}
		else if((MsgList.isHMDSensorAttached == 0) && 
		   (oldMsgList.isHMDSensorAttached != 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.HMDSensor, false);
			//Debug.Log("HMD SENSOR DETACHED");
		}

		// HMD
		if((MsgList.isHMDAttached != 0) && 
		   (oldMsgList.isHMDAttached == 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.HMD, true); 
			//Debug.Log("HMD ATTACHED");
		}
		else if((MsgList.isHMDAttached == 0) && 
		   (oldMsgList.isHMDAttached != 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.HMD, false); 
			//Debug.Log("HMD DETACHED");
		}

		// LATENCY TESTER
		if((MsgList.isLatencyTesterAttached != 0) && 
		   (oldMsgList.isLatencyTesterAttached == 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.LatencyTester, true); 
			//Debug.Log("LATENCY TESTER ATTACHED");
		}
		else if((MsgList.isLatencyTesterAttached == 0) && 
		   (oldMsgList.isLatencyTesterAttached != 0))
		{
			IVRMessenger.Broadcast<IVRMainMenu.Device, bool>("Sensor_Attached", IVRMainMenu.Device.LatencyTester, false); 
			//Debug.Log("LATENCY TESTER DETACHED");
		}
		*/
    }

    // OnDestroy
    void OnDestroy()
    {
        // We may want to turn this off so that values are maintained between level / scene loads
        if (ResetTrackerOnNextScene == true)
        {
#if !UNITY_EDITOR_OSX
            IVR_Destroy();
#endif
            IVRInit = false;
        }
    }


    // * * * * * * * * * * * *
    // PUBLIC FUNCTIONS
    // * * * * * * * * * * * *

    // Inited - Check to see if system has been initialized
    public static bool IsInitialized()
    {
        return IVRInit;
    }

    // HMDPreset
    public static bool IsHMDPresent()
    {
        return IVR_IsHMDPresent();
    }

    // SensorPreset
    public static bool IsSensorPresent(int sensor)
    {
        return IVR_IsSensorPresent(SensorList[sensor]);
    }

    // GetOrientation
    public static bool GetOrientation(int sensor, ref Quaternion q)
    {
        float w = 0, x = 0, y = 0, z = 0;

        if (IVR_GetSensorOrientation(SensorList[sensor], ref w, ref x, ref y, ref z) == true)
        {
            q.w = w; q.x = x; q.y = y; q.z = z;
            OrientSensor(sensor, ref q);

            return true;
        }

        return false;
    }

    // GetPredictedOrientation
    public static bool GetPredictedOrientation(int sensor, ref Quaternion q)
    {
        float w = 0, x = 0, y = 0, z = 0;

        if (IVR_GetSensorPredictedOrientation(SensorList[sensor], ref w, ref x, ref y, ref z) == true)
        {
            q.w = w; q.x = x; q.y = y; q.z = z;
            OrientSensor(sensor, ref q);

            return true;
        }
        return false;
    }

    // ResetOrientation
    public static bool ResetOrientation(int sensor)
    {
        return IVR_ResetSensorOrientation(SensorList[sensor]);
    }

    // Latest absolute sensor readings (note: in right-hand co-ordinates)

    // GetAcceleration
    public static bool GetAcceleration(int sensor, ref float x, ref float y, ref float z)
    {
        return IVR_GetAcceleration(SensorList[sensor], ref x, ref y, ref z);
    }

    // GetAngularVelocity
    public static bool GetAngularVelocity(int sensor, ref float x, ref float y, ref float z)
    {
        return IVR_GetAngularVelocity(SensorList[sensor], ref x, ref y, ref z);
    }

    // GetMagnetometer
    public static bool GetMagnetometer(int sensor, ref float x, ref float y, ref float z)
    {
        return IVR_GetMagnetometer(SensorList[sensor], ref x, ref y, ref z);
    }

    // GetPredictionTime
    public static float GetPredictionTime(int sensor)
    {
        // return IVRSensorsGetPredictionTime(sensor, ref predictonTime);
        return PredictionTime;
    }

    // SetPredictionTime
    public static bool SetPredictionTime(int sensor, float predictionTime)
    {
        if ((predictionTime > 0.0f) &&
             (IVR_SetSensorPredictionTime(SensorList[sensor], predictionTime) == true))
        {
            PredictionTime = predictionTime;
            return true;
        }

        return false;
    }

    // GetDistortionCorrectionCoefficients
    public static bool GetDistortionCorrectionCoefficients(ref float k0,
                                                           ref float k1,
                                                           ref float k2,
                                                           ref float k3)
    {
        if (!IVRInit)
            return false;

        k0 = DistK0;
        k1 = DistK1;
        k2 = DistK2;
        k3 = DistK3;

        return true;
    }

    // SetDistortionCorrectionCoefficients
    public static bool SetDistortionCorrectionCoefficients(float k0,
                                                           float k1,
                                                           float k2,
                                                           float k3)
    {
        if (!IVRInit)
            return false;

        DistK0 = k0;
        DistK1 = k1;
        DistK2 = k2;
        DistK3 = k3;

        return true;
    }

    // GetPhysicalLensOffsets
    public static bool GetPhysicalLensOffsets(ref float lensOffsetLeft,
                                              ref float lensOffsetRight)
    {
        if (!IVRInit)
            return false;

        lensOffsetLeft = LensOffsetLeft;
        lensOffsetRight = LensOffsetRight;

        return true;
    }

    // GetIPD
    public static bool GetIPD(ref float IPD)
    {
        if (!IVRInit)
            return false;

        IVR_GetInterpupillaryDistance(ref IPD);

        return true;
    }

    // CalculateAspectRatio
    public static float CalculateAspectRatio()
    {
        if (Application.isEditor)
            return (Screen.width * 0.5f) / Screen.height;
        else
            return (HResolution * 0.5f) / VResolution;
    }

    // VerticalFOV
    // Compute Vertical FOV based on distance, distortion, etc.
    // Distance from vertical center to render vertical edge perceived through the lens.
    // This will be larger then normal screen size due to magnification & distortion.
    public static float VerticalFOV()
    {
        if (!IVRInit)
        {
            return 90.0f;
        }

        float percievedHalfScreenDistance = (VScreenSize / 2) * DistortionScale();
        float VFov = Mathf.Rad2Deg * 2.0f *
                     Mathf.Atan(percievedHalfScreenDistance / EyeToScreenDistance);

        return VFov;
    }

    // DistortionScale - Used to adjust size of shader based on 
    // shader K values to maximize screen size
    public static float DistortionScale()
    {
        if (IVRInit)
        {
            float ds = 0.0f;

            // Compute distortion scale from DistortionFitX & DistortionFitY.
            // Fit value of 0.0 means "no fit".
            if ((Mathf.Abs(DistortionFitX) < 0.0001f) && (Math.Abs(DistortionFitY) < 0.0001f))
            {
                ds = 1.0f;
            }
            else
            {
                // Convert fit value to distortion-centered coordinates before fit radius
                // calculation.
                float stereoAspect = 0.5f * Screen.width / Screen.height;
                //float dx = (DistortionFitX * DistortionFitScale);// - LensOffsetLeft;
                //float dy           = (DistortionFitY * DistortionFitScale) / stereoAspect;
                float dx = (DistortionFitX * DistortionFitScale) * stereoAspect;// - LensOffsetLeft;
                float dy = (DistortionFitY * DistortionFitScale);
                float fitRadius = Mathf.Sqrt(dx * dx + dy * dy);
                ds = CalcScale(fitRadius);
            }

            if (ds != 0.0f)
                return ds;

        }

        return 1.0f; // no scale
    }

    // LatencyProcessInputs
    public static void ProcessLatencyInputs()
    {
        IVR_ProcessLatencyInputs();
    }

    // LatencyProcessInputs
    public static bool DisplayLatencyScreenColor(ref byte r, ref byte g, ref byte b)
    {
        return IVR_DisplayLatencyScreenColor(ref r, ref g, ref b);
    }

    // LatencyGetResultsString
    public static System.IntPtr GetLatencyResultsString()
    {
        return IVR_GetLatencyResultsString();
    }

    // Computes scale that should be applied to the input render texture
    // before distortion to fit the result in the same screen size.
    // The 'fitRadius' parameter specifies the distance away from distortion center at
    // which the input and output coordinates will match, assuming [-1,1] range.
    public static float CalcScale(float fitRadius)
    {
        float s = fitRadius;
        // This should match distortion equation used in shader.
        float ssq = s * s;
        float scale = s * (DistK0 + DistK1 * ssq + DistK2 * ssq * ssq + DistK3 * ssq * ssq * ssq);
        return scale / fitRadius;
    }

    // CalculatePhysicalLensOffsets - Used to offset perspective and distortion shift
    public static bool CalculatePhysicalLensOffsets(ref float leftOffset, ref float rightOffset)
    {
        leftOffset = 0.0f;
        rightOffset = 0.0f;

        if (!IVRInit)
            return false;

        float halfHSS = HScreenSize * 0.5f;
        float halfLSD = LensSeparationDistance * 0.5f;

        leftOffset = (((halfHSS - halfLSD) / halfHSS) * 2.0f) - 1.0f;
        rightOffset = ((halfLSD / halfHSS) * 2.0f) - 1.0f;

        return true;
    }

    // MAG YAW-DRIFT CORRECTION FUNCTIONS

    // IsMagCalibrated
    public static bool IsMagCalibrated(int sensor)
    {
        return IVR_IsMagCalibrated(SensorList[sensor]);
    }

    // EnableMagYawCorrection
    public static bool EnableMagYawCorrection(int sensor, bool enable)
    {
        return IVR_EnableMagYawCorrection(SensorList[sensor], enable);
    }

    // IVR_IsYawCorrectionEnabled
    public static bool IsYawCorrectionEnabled(int sensor)
    {
        return IVR_IsYawCorrectionEnabled(SensorList[sensor]);
    }

    // InitSensorList:
    // We can remap sensors to different parts
    public static void InitSensorList(bool reverse)
    {
        SensorList.Clear();

        if (reverse == true)
        {
            SensorList.Add(0, 1);
            SensorList.Add(1, 0);
        }
        else
        {
            SensorList.Add(0, 0);
            SensorList.Add(1, 1);
        }
    }

    // InitOrientationSensorList
    public static void InitOrientationSensorList()
    {
        SensorOrientationList.Clear();
        SensorOrientationList.Add(0, SensorOrientation.Head);
        SensorOrientationList.Add(1, SensorOrientation.Other);
    }

    // OrientSensor
    // We will set up the sensor based on which one it is
    public static void OrientSensor(int sensor, ref Quaternion q)
    {
        if (SensorOrientationList[sensor] == SensorOrientation.Head)
        {
            // Change the co-ordinate system from right-handed to Unity left-handed
            /*
			q.x =  x; 
			q.y =  y;
			q.z =  -z; 
			q = Quaternion.Inverse(q);
			*/

            // The following does the exact same conversion as above
            q.x = -q.x;
            q.y = -q.y;

        }
        else if (SensorOrientationList[sensor] == SensorOrientation.Other)
        {
            // Currently not used 
            float tmp = q.x;
            q.x = q.z;
            q.y = -q.y;
            q.z = tmp;
        }
    }

    // RenderPortraitMode
    public static bool RenderPortraitMode()
    {
        return IVR_RenderPortraitMode();
    }

    // GetPlayerEyeHeight
    public static bool GetPlayerEyeHeight(ref float eyeHeight)
    {
        return IVR_GetPlayerEyeHeight(ref eyeHeight);
    }

    public static bool IsHMDAttached()
    {
        return HMDAttached;
    }
}
