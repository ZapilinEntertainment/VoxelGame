using UnityEngine;
using System.IO;

public class GameSettings : MyObject
{
    public float cameraMoveCf { get; private set; }
    public float cameraRotationCf { get; private set; }
    public float lodCoefficient { get; private set; }
    private static GameSettings current;
    private string CAMERA_MOVE_NAME { get { return "CameraMoveCf="; } }
    private string CAMERA_ROTATION_NAME { get { return "CameraRotationCf="; } }
    private string LOD_CF_NAME { get { return "LOD_Cf="; } }
    private string fullpath { get { return Application.persistentDataPath + "/lwsettings.lws"; } }

    public const float LOD_MIN_VAL = 0.5f, LOD_MAX_VAL = 5f, CAM_MV_MIN = 0.5f, CAM_MV_MAX = 2f, 
        CAM_RT_MIN = 0.5f, CAM_RT_MAX = 2f;

    public static GameSettings GetSettings()
    {
        if (current == null) current = new GameSettings();
        return current;
    }

    private GameSettings()
    {
        if (!LoadFromFile())
        {
            SetDefaultValues();
            SaveSettings();
        }        
    }
    private void SetDefaultValues()
    {
        cameraMoveCf = 1f;
        cameraRotationCf = 1f;
        lodCoefficient = 1f;
    }
    private bool LoadFromFile()
    {
        var path = fullpath;
        if (File.Exists(path))
        {
            using (StreamReader sr = new StreamReader(path))
            {
                float f = 1f;
                bool CheckLine(in string parameterName)
                {
                    var line = sr.ReadLine();
                    int parameterLength = parameterName.Length;
                    if (line.Length > parameterLength &&
                        line.Substring(0, parameterLength) == parameterName &&
                        (float.TryParse(line.Substring(parameterLength), out f)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (CheckLine(CAMERA_MOVE_NAME))
                {
                    cameraMoveCf = f;
                }
                else return false;
                //
                if (CheckLine(CAMERA_ROTATION_NAME))
                {
                    cameraRotationCf = f;
                }
                else return false;
                //
                if (CheckLine(LOD_CF_NAME))
                {
                    lodCoefficient = f;
                }
                else return false;
            }
        }
        else return false;
        return true;
    }
    public void SaveSettings()
    {
        using (var sw = new StreamWriter(fullpath, false))
        {
            sw.WriteLine(CAMERA_MOVE_NAME + string.Format("{0:f1}", cameraMoveCf));
            sw.WriteLine(CAMERA_ROTATION_NAME + string.Format("{0:f1}", cameraRotationCf));
            sw.WriteLine(LOD_CF_NAME + string.Format("{0:f1}", lodCoefficient));
        }
        FollowingCamera.main?.ApplySettings(cameraMoveCf, cameraRotationCf);
    }

    public void SetCameraMoveCf(float x)
    {
        if (x >= CAM_MV_MIN && x <= CAM_MV_MAX) cameraMoveCf = x;
    }
    public void SetCameraRotationCf(float x)
    {
        if (x >= CAM_RT_MIN && x <= CAM_RT_MAX) cameraRotationCf = x;
    }
    public void SetLodDistance(float x)
    {
        if (x >= LOD_MIN_VAL && x <= LOD_MAX_VAL) lodCoefficient = x;
    }
}
