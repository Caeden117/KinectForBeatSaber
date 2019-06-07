using CustomAvatar;
using UnityEngine;
using System.Collections;

namespace KinectForBeatSaber
{
    class AvatarsToKinect : MonoBehaviour
    {
        CustomAvatar.CustomAvatar CurrentAvatar = null;

        private void Start()
        {
            CustomAvatar.Plugin.Instance.PlayerAvatarManager.AvatarChanged += AvatarUpdate;
            AvatarUpdate(CustomAvatar.Plugin.Instance.PlayerAvatarManager.GetCurrentAvatar());   
        }   
        
        private void OnDestroy()
        {
            CustomAvatar.Plugin.Instance.PlayerAvatarManager.AvatarChanged -= AvatarUpdate;
        }

        private void AvatarUpdate(CustomAvatar.CustomAvatar newAvatar)
        {
            CurrentAvatar = newAvatar;
            StartCoroutine(WaitForKinectAndUpdate());
        }

        private IEnumerator WaitForKinectAndUpdate() {
            yield return new WaitUntil(() => Plugin.KinectInfo != null);
            yield return new WaitUntil(() => CurrentAvatar?.GameObject?.GetComponent<AvatarBehaviour>() != null);
            try
            {
                KinectToBS info = Plugin.KinectInfo;
                AvatarBehaviour behaviour = CurrentAvatar.GameObject.GetComponent<AvatarBehaviour>();
                /*VRIK vrik = CurrentAvatar.GameObject.GetComponent<VRIK>();
                KinectToBS info = Plugin.KinectInfo;
                vrik.solver.plantFeet = false;

                //Spine
                vrik.solver.spine.headTarget = info.trackingPoints[JointType.Head];
                vrik.solver.spine.pelvisTarget = info.trackingPoints[JointType.HipCenter];

                //Left Side
                vrik.solver.leftArm.target = info.trackingPoints[JointType.WristLeft];
                vrik.solver.leftArm.bendGoal = info.trackingPoints[JointType.ElbowLeft];
                vrik.solver.leftLeg.target = info.trackingPoints[JointType.AnkleLeft];
                vrik.solver.leftLeg.bendGoal = info.trackingPoints[JointType.KneeLeft];

                //Right side
                vrik.solver.rightArm.target = info.trackingPoints[JointType.WristRight];
                vrik.solver.rightArm.bendGoal = info.trackingPoints[JointType.ElbowRight];
                vrik.solver.rightLeg.target = info.trackingPoints[JointType.AnkleRight];
                vrik.solver.rightLeg.bendGoal = info.trackingPoints[JointType.KneeRight];

                vrik.solver.Reset();*/
            }
            catch
            {
                Plugin.Log("Error with the avatar! Is it null?");
            }
        }
    }
}
