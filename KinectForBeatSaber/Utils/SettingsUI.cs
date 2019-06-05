using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using CustomUI;
using CustomUI.Settings;
using UnityEngine;

namespace KinectForBeatSaber.Utils
{
    class SettingsUI
    {
        private static SubMenu mainSubMenu;
        private static List<ListViewController> updatingLists = new List<ListViewController>();
        private static List<SliderViewController> positionList = new List<SliderViewController>();

        internal static void Create()
        {
            Plugin.Log("Creating Settings UI!");
            mainSubMenu = CustomUI.Settings.SettingsUI.CreateSubMenu("Kinect For BS");

            SubMenu General = mainSubMenu.AddSubMenu("General Settings", "Control general settings for Kinect For Beat Saber.", true);
            BoolViewController debug = General.AddBool("Show Debug Tracking Points");
            debug.GetValue += () => Plugin.config.ShowTrackingPoints;
            debug.applyImmediately = true;
            debug.SetValue += (v) => {
                Plugin.config.ShowTrackingPoints = v;
                foreach (GameObject obj in Plugin.parent.GetComponent<KinectToBS>().trackingPoints.Values)
                    obj.GetComponent<Renderer>().enabled = v;
            };

            ListViewController scale = General.AddList("Kinect Avatar Scale", Enumerable.Range(0, 20).Select(x => x * 0.1f).ToArray(), "Change how big or small your Kinect Avatar is.");
            scale.applyImmediately = true;
            scale.GetValue = () => (float)Math.Round(Plugin.config.Scale, 1);
            scale.SetValue = (v) => Plugin.config.Scale = v;
            updatingLists.Add(scale);
            ListViewController headRotation = General.AddList("Head Rotation Offset", Enumerable.Range(0, 45).Select(x => x * 1f).ToArray(), "Change the offset rotation of your head for your Custom Avatar.");
            headRotation.applyImmediately = true;
            headRotation.GetValue = () => (float)Math.Round(Plugin.config.HeadRotationOffset, 1);
            headRotation.SetValue = (v) => Plugin.config.HeadRotationOffset = v;
            updatingLists.Add(headRotation);

            SubMenu posMenu = mainSubMenu.AddSubMenu("Kinect Position Offset", "Control the position of your Kinect avatar.", true);
            SliderViewController PosX = posMenu.AddSlider("X", "", -5f, 5f, 0.1f, false);
            SliderViewController PosY = posMenu.AddSlider("Y", "", -5f, 5f, 0.1f, false);
            SliderViewController PosZ = posMenu.AddSlider("Z", "", -5f, 5f, 0.1f, false);
            SubMenu RotMenu = mainSubMenu.AddSubMenu("Kinect Rotation Offset", "Control the rotation of your Kinect avatar.", true);
            SliderViewController RotX = RotMenu.AddSlider("X", "", 0, 360f, 5f, false);
            SliderViewController RotY = RotMenu.AddSlider("Y", "", 0, 360f, 5f, false);
            SliderViewController RotZ = RotMenu.AddSlider("Z", "", 0, 360f, 5f, false);
            positionList.AddRange(new SliderViewController[] { PosX, PosY, PosZ, RotX, RotY, RotZ });

            PosX.GetValue += () => Plugin.config.PositionOffset.x;
            PosX.SetValue += (v) => Plugin.config.PositionOffset.x = v;
            PosY.GetValue += () => Plugin.config.PositionOffset.y;
            PosY.SetValue += (v) => Plugin.config.PositionOffset.y = v;
            PosZ.GetValue += () => Plugin.config.PositionOffset.z;
            PosZ.SetValue += (v) => Plugin.config.PositionOffset.z = v;

            RotX.GetValue += () => Plugin.config.RotationOffset.x;
            RotX.SetValue += (v) => Plugin.config.RotationOffset.x = v;
            RotY.GetValue += () => Plugin.config.RotationOffset.y;
            RotY.SetValue += (v) => Plugin.config.RotationOffset.y = v;
            RotZ.GetValue += () => Plugin.config.RotationOffset.z;
            RotZ.SetValue += (v) => Plugin.config.RotationOffset.z = v;

            foreach(ListViewController list in updatingLists)
            {
                list.GetTextForValue += (v) => Math.Round(v, 1).ToString();
                list.SetValue += (v) =>
                {
                    Plugin.parent.GetComponent<KinectToBS>().StartCoroutine(DelayedUpdate());
                    Plugin.config.Save();
                };
            }

        }

        private static IEnumerator DelayedUpdate()
        {
            yield return new WaitForEndOfFrame();
            Plugin.parent.transform.position = Plugin.config.PositionOffset;
            Plugin.parent.transform.rotation = Quaternion.Euler(Plugin.config.RotationOffset);
            Plugin.parent.transform.localScale = Vector3.one * Plugin.config.Scale;
        }
    }
}
