using CustomAvatar;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KinectForBeatSaber
{
    class AvatarsToKinect : MonoBehaviour
    {
        CustomAvatar.CustomAvatar CurrentAvatar = null;

        private Dictionary<JointType, Transform> AvatarMapPoints = new Dictionary<JointType, Transform>();

        private static Dictionary<JointType, JointType> RotationMapPoints = new Dictionary<JointType, JointType>()
        {
            { JointType.ShoulderLeft, JointType.ElbowLeft },
            { JointType.ElbowLeft, JointType.HandLeft },
            { JointType.HipLeft, JointType.KneeLeft },
            { JointType.KneeLeft, JointType.AnkleLeft },
            { JointType.AnkleLeft, JointType.FootLeft },
            { JointType.ShoulderRight, JointType.ElbowRight },
            { JointType.ElbowRight, JointType.HandRight },
            { JointType.HipRight, JointType.KneeRight },
            { JointType.KneeRight, JointType.AnkleRight },
            { JointType.AnkleRight, JointType.FootRight },
        };

        private void Start()
        {
            CurrentAvatar = CustomAvatar.Plugin.Instance.PlayerAvatarManager.GetCurrentAvatar();
            CustomAvatar.Plugin.Instance.PlayerAvatarManager.AvatarChanged += AvatarUpdate;
            Destroy(CurrentAvatar.GameObject.GetComponent<AvatarBehaviour>());
        }   
        
        private void OnDestroy()
        {
            CustomAvatar.Plugin.Instance.PlayerAvatarManager.AvatarChanged -= AvatarUpdate;
        }

        private void AvatarUpdate(CustomAvatar.CustomAvatar newAvatar)
        {
            CurrentAvatar = newAvatar;
            Destroy(CurrentAvatar.GameObject.GetComponent<AvatarBehaviour>());
        }

        private void LateUpdate()
        {
            for (int i = 0; i < 20; i++)
                RefreshJoint((JointType)i);
        }

        private void RefreshJoint(JointType type)
        {
            if (Plugin.parent.GetComponent<KinectToBS>().trackingPoints.ContainsKey(type))
            {
                if (!(AvatarMapPoints.ContainsKey(type)))
                {
                    Transform avatarTransform = RecursiveFind(CurrentAvatar.GameObject.transform, type.ToString());
                    if (avatarTransform != null)
                        AvatarMapPoints.Add(type, avatarTransform);
                }
                else
                {
                    Transform updatingTransform = AvatarMapPoints[type];
                    updatingTransform.position = Plugin.parent.GetComponent<KinectToBS>().trackingPoints[type].transform.position;
                    if (RotationMapPoints.ContainsKey(type))
                        updatingTransform.LookAt(Plugin.parent.GetComponent<KinectToBS>().trackingPoints[RotationMapPoints[type]].transform);
                }
            }
        }

        private Transform RecursiveFind(Transform start, string name)
        {
            Transform transform = null;
            for(int i = 0; i < start.childCount; i++)
            {
                Transform child = start.GetChild(i);
                if (child.name == name)
                {
                    transform = child;
                    break;
                }
                RecursiveFind(child, name);
            }
            return transform;
        }
    }
}
