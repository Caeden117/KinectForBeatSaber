using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KinectForBeatSaber.Utils
{
    public class Config
    {
        internal static BS_Utils.Utilities.Config file = new BS_Utils.Utilities.Config("KinectForBeatSaber");
        public Vector3 PositionOffset = new Vector3(0, 0, 0);
        public Vector3 RotationOffset = new Vector3(0, 0, 0);
        public float Scale = 1;
        public float HeadRotationOffset = 0;
        public bool ShowTrackingPoints = true;

        public static Config Load()
        {
            if (!File.Exists(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/KinectForBeatSaber.ini"))
                File.Create(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/KinectForBeatSaber.ini");
            Config config = new Config();
            config = DeserializeFromConfig(config) as Config;
            config.Save();
            return config;
        }

        internal static object DeserializeFromConfig(object input)
        {
            Type type = input.GetType();
            MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (MemberInfo info in infos)
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    FieldInfo finfo = (FieldInfo)info;
                    string v = null;
                    v = file.GetString("Kinect For Beat Saber", info.Name, null, false);
                    if (v == null) continue;
                    if (finfo.FieldType == typeof(Vector3))
                        input.SetPrivateField(info.Name, ParseToVector3(v));
                    else input.SetPrivateField(info.Name, Convert.ChangeType(v, finfo.FieldType));
                }
            }
            return input;
        }

        private static Vector3 ParseToVector3(string s)
        {
            string[] args = s.Split('|');
            return new Vector3((float)Math.Round(float.Parse(args[0]), 1),
                (float)Math.Round(float.Parse(args[1]), 1),
                (float)Math.Round(float.Parse(args[2]), 1));
        }

        private static string ParseFromVector3(Vector3 v)
        {
            return $"{Math.Round(v.x, 1)}|{Math.Round(v.y, 1)}|{Math.Round(v.z, 1)}";
        }

        public void Save()
        {
            Type type = GetType();
            MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (MemberInfo info in infos)
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    FieldInfo finfo = (FieldInfo)info;
                    if (finfo.FieldType == typeof(Vector3)) file.SetString("Kinect For Beat Saber", info.Name, ParseFromVector3((Vector3)finfo.GetValue(this)));
                    else file.SetString("Kinect For Beat Saber", info.Name, finfo.GetValue(this).ToString());
                }
            }
        }
    }
}
