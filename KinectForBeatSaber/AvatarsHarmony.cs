using Microsoft.Kinect;
using UnityEngine;
using Harmony;
using CustomAvatar;
using AvatarScriptPack;
using System;

namespace KinectForBeatSaber
{
    [HarmonyPatch(typeof(AvatarBehaviour))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    class AvatarsUpdatePatch
    {
        static bool Prefix(ref AvatarBehaviour __instance, ref Transform ____head, ref Transform ____body,
            ref Transform ____leftHand, ref Transform ____rightHand, ref Transform ____leftLeg, ref Transform ____rightLeg,
            ref Transform ____pelvis, ref Vector3 ____prevBodyPos) //MonkaS
        {
            if (!Plugin.config.CustomAvatarIntegration) return true;
            KinectToBS info = Plugin.KinectInfo;
            UpdatePosAndRot(ref ____head, JointType.Head, Plugin.config.HeadRotationOffset);
            UpdatePosAndRot(ref ____body, JointType.ShoulderCenter);
            UpdatePosAndRot(ref ____leftHand, JointType.HandLeft);
            UpdatePosAndRot(ref ____rightHand, JointType.HandRight);
            if (____leftLeg != null) LerpPosAndRot(ref ____leftLeg, JointType.FootLeft, 15, 10);
            if (____rightLeg != null) LerpPosAndRot(ref ____rightLeg, JointType.FootRight, 15, 10);
            if (____pelvis != null) LerpPosAndRot(ref ____pelvis, JointType.HipCenter, 17, 13);

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

    [HarmonyPatch(typeof(AvatarSpawner))]
    [HarmonyPatch("SpawnAvatar")]
    class AvatarSpawnerPatch
    {
        static bool Prefix(CustomAvatar.CustomAvatar customAvatar, IAvatarInput avatarInput, ref SpawnedAvatar __result)
        {
            if (customAvatar.GameObject == null) return true;
            KinectToBS info = Plugin.KinectInfo;
            VRIK ik = customAvatar.GameObject.GetComponentInChildren<VRIK>();
            ik.solver.plantFeet = false;

            //Spine
            ik.solver.spine.headTarget = info.trackingPoints[JointType.Head];
            ik.solver.spine.pelvisTarget = info.trackingPoints[JointType.HipCenter];
            ik.solver.spine.pelvisPositionWeight = 1;
            ik.solver.spine.pelvisRotationWeight = 1;
            ik.solver.spine.rotateChestByHands = 0;
            ik.solver.spine.chestGoal = info.trackingPoints[JointType.ShoulderCenter];
            ik.solver.spine.chestGoalWeight = 1;
            ik.solver.spine.positionWeight = 1;
            ik.solver.spine.rotationWeight = 1;
            ik.solver.spine.ResetOffsets();

            //Left Side
            ik.solver.leftArm.target = info.trackingPoints[JointType.WristLeft];
            ik.solver.leftArm.bendGoal = info.trackingPoints[JointType.ElbowLeft];
            ik.solver.leftArm.bendGoalWeight = 1;
            ik.solver.leftArm.ResetOffsets();
            ik.solver.leftLeg.target = info.trackingPoints[JointType.AnkleLeft];
            ik.solver.leftLeg.bendGoal = info.trackingPoints[JointType.KneeLeft];
            ik.solver.leftLeg.positionWeight = 1;
            ik.solver.leftLeg.rotationWeight = 1;
            ik.solver.leftLeg.bendGoalWeight = 1;
            ik.solver.leftLeg.ResetOffsets();

            //Right side
            ik.solver.rightArm.target = info.trackingPoints[JointType.WristRight];
            ik.solver.rightArm.bendGoal = info.trackingPoints[JointType.ElbowRight];
            ik.solver.rightArm.bendGoalWeight = 1;
            ik.solver.rightArm.ResetOffsets();
            ik.solver.rightLeg.target = info.trackingPoints[JointType.AnkleRight];
            ik.solver.rightLeg.bendGoal = info.trackingPoints[JointType.KneeRight];
            ik.solver.rightLeg.positionWeight = 1;
            ik.solver.rightLeg.rotationWeight = 1;
            ik.solver.rightLeg.bendGoalWeight = 1;
            ik.solver.rightLeg.ResetOffsets();

            GameObject instantiate = UnityEngine.Object.Instantiate(customAvatar.GameObject);
            AvatarBehaviour behaviour = instantiate.AddComponent<AvatarBehaviour>();
            behaviour.Init(avatarInput);
            instantiate.AddComponent<AvatarEventsPlayer>();
            UnityEngine.Object.DontDestroyOnLoad(instantiate);
            __result = new SpawnedAvatar(customAvatar, instantiate);
            return false;
        }
    }

    /*[HarmonyPatch(typeof(AvatarBehaviour))]
    [HarmonyPatch("Init", MethodType.Normal)]
    class AvatarsInitPatch
    {
        static void Postfix(ref AvatarBehaviour __instance, ref Transform ____head, ref Transform ____body,
            ref Transform ____leftHand, ref Transform ____rightHand, ref Transform ____leftLeg, ref Transform ____rightLeg,
            ref Transform ____pelvis)
        {
            if (!Plugin.config.CustomAvatarIntegration) return;
            KinectToBS info = Plugin.KinectInfo;
            VRIK ik = __instance.gameObject.GetComponentInChildren<VRIK>();
            if (ik == null) ik = __instance.gameObject.GetComponent<VRIK>();
            if (ik == null) ik = __instance.gameObject.GetComponentInParent<VRIK>();
            if (ik == null) ik = __instance.gameObject.transform.parent?.GetComponentInChildren<VRIK>();
            if (ik != null)
            {
                //Im going to try this out, see how it goes.
                ____head = info.trackingPoints[JointType.Head];
                ____body = info.trackingPoints[JointType.ShoulderCenter];
                ____leftHand = info.trackingPoints[JointType.HandLeft];
                ____rightHand = info.trackingPoints[JointType.HandRight];
                ____leftLeg = info.trackingPoints[JointType.FootLeft];
                ____rightLeg = info.trackingPoints[JointType.FootRight];
                ____pelvis = info.trackingPoints[JointType.HipCenter];

                //Spine
                ik.solver.spine.headTarget = info.trackingPoints[JointType.Head];
                ik.solver.spine.pelvisTarget = info.trackingPoints[JointType.HipCenter];

                //Left Side
                ik.solver.leftArm.target = info.trackingPoints[JointType.WristLeft];
                ik.solver.leftArm.bendGoal = info.trackingPoints[JointType.ElbowLeft];
                ik.solver.leftLeg.target = info.trackingPoints[JointType.AnkleLeft];
                ik.solver.leftLeg.bendGoal = info.trackingPoints[JointType.KneeLeft];

                //Right side
                ik.solver.rightArm.target = info.trackingPoints[JointType.WristRight];
                ik.solver.rightArm.bendGoal = info.trackingPoints[JointType.ElbowRight];
                ik.solver.rightLeg.target = info.trackingPoints[JointType.AnkleRight];
                ik.solver.rightLeg.bendGoal = info.trackingPoints[JointType.KneeRight];

                //ik.solver.Initiate(ik.solver.GetRoot());
            }
            else Plugin.Log("VRIK is null.");
        }
    }*/
}
