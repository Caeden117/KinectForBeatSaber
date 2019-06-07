using Microsoft.Kinect;
using UnityEngine;
using Harmony;
using CustomAvatar;
using AvatarScriptPack;

namespace KinectForBeatSaber
{
    [HarmonyPatch(typeof(AvatarBehaviour))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class AvatarsHarmony
    {
        static bool Prefix(ref AvatarBehaviour __instance, ref Transform ____head, ref Transform ____body,
            ref Transform ____leftHand, ref Transform ____rightHand, ref Transform ____leftLeg, ref Transform ____rightLeg,
            ref Transform ____pelvis, ref Vector3 ____prevBodyPos) //MonkaS
        {
            KinectToBS info = Plugin.KinectInfo;
            UpdatePosAndRot(ref ____head, JointType.Head, Plugin.config.HeadRotationOffset);
            UpdatePosAndRot(ref ____body, JointType.ShoulderCenter);
            UpdatePosAndRot(ref ____leftHand, JointType.HandLeft);
            UpdatePosAndRot(ref ____rightHand, JointType.HandRight);
            LerpPosAndRot(ref ____leftLeg, JointType.FootLeft, 15, 10);
            LerpPosAndRot(ref ____rightLeg, JointType.FootRight, 15, 10);
            LerpPosAndRot(ref ____pelvis, JointType.HipCenter, 17, 13);

            VRPlatformHelper helper = PersistentSingleton<VRPlatformHelper>.instance;
            helper.AdjustPlatformSpecificControllerTransform(____leftHand);
            helper.AdjustPlatformSpecificControllerTransform(____rightHand);

            //____body.position = ____head.position - (____head.transform.up * 0.1f);

            Vector3 velocity = new Vector3(____body.localPosition.x - ____prevBodyPos.x, 0.0f, ____body.localPosition.z - ____prevBodyPos.z);
            Quaternion rotation = Quaternion.Euler(0.0f, ____head.localEulerAngles.y, 0.0f);
            Vector3 tiltAxis = Vector3.Cross(__instance.transform.up, velocity);
            ____body.localRotation = Quaternion.Lerp(____body.localRotation,
                Quaternion.AngleAxis(velocity.magnitude * 1250.0f, tiltAxis) * rotation, Time.deltaTime * 10);
            ____prevBodyPos = ____body.transform.localPosition;
            return false;
        }

        private static void UpdatePosAndRot(ref Transform updating, JointType point, float rotationOffset = 0f)
        {
            updating.position = Plugin.KinectInfo.trackingPoints[point].position;
            updating.rotation = Plugin.KinectInfo.trackingPoints[point].rotation * Quaternion.Euler(0, rotationOffset, 0);
        }

        private static void LerpPosAndRot(ref Transform updating, JointType point, float posSpeed, float rotSpeed)
        {
            updating.position = Vector3.Lerp(updating.position, Plugin.KinectInfo.trackingPoints[point].position, posSpeed * Time.deltaTime);
            updating.rotation = Quaternion.Slerp(updating.rotation, Plugin.KinectInfo.trackingPoints[point].rotation, rotSpeed * Time.deltaTime);
        }
    }
}
