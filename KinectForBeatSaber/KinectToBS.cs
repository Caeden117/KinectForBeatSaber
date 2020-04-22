using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace KinectForBeatSaber
{
    public class KinectToBS : MonoBehaviour
    {
        public Dictionary<JointType, Transform> trackingPoints = new Dictionary<JointType, Transform>();

        private void Awake()
        {
            Plugin.Log("Kinect To Beat Saber GameObject created!");
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Plugin.skeletonsToProcess.Count > 0)
            {
                Skeleton[] skeletons = Plugin.skeletonsToProcess.First();
                if (skeletons == null) return;
                foreach (Skeleton skeleton in skeletons)
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked) RefreshTrackingPoints(skeleton);
                if (Plugin.skeletonsToProcess.Last() != skeletons) Plugin.skeletonsToProcess.Remove(skeletons);
            }
        }

        private void RefreshTrackingPoints(Skeleton skeleton)
        {
            for (int i = 0; i < Enum.GetValues(typeof(JointType)).Length; i++) RefreshJoint(skeleton, (JointType)i);
        }

        private void RefreshJoint(Skeleton skeleton, JointType jointType)
        {
            Joint joint = skeleton.Joints[jointType];
            if (joint == null) return;
            if (!trackingPoints.ContainsKey(joint.JointType))
            {
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                primitive.transform.localScale = Vector3.one * 0.1f * Plugin.config.Scale;
                primitive.transform.parent = transform;
                trackingPoints.Add(joint.JointType, primitive.transform);
            }
            if (trackingPoints[joint.JointType] != null)
            {
                trackingPoints[joint.JointType].localScale = Vector3.one * 0.1f * Plugin.config.Scale;
                trackingPoints[joint.JointType].localPosition = SkeletonPointToVector3(joint.Position);
                Quaternion rotation = BoneRotationToQuaternion(skeleton.BoneOrientations[jointType].AbsoluteRotation.Quaternion);
                trackingPoints[joint.JointType].localRotation = rotation;
                Color trackingColor = Color.white;
                switch (joint.TrackingState)
                {
                    case JointTrackingState.Inferred:
                        trackingColor = Color.yellow;
                        break;
                    case JointTrackingState.NotTracked:
                        trackingColor = Color.red;
                        break;
                }
                Renderer renderer = trackingPoints[joint.JointType].GetComponent<Renderer>();
                //renderer.material = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "GlowSaberInstancedHD"));
                trackingPoints[joint.JointType].GetComponent<Renderer>().material.SetColor("_Color", trackingColor);
            }
        }

        private Vector3 SkeletonPointToVector3 (SkeletonPoint point) => new Vector3(point.X * -1, point.Y, point.Z);

        private Quaternion BoneRotationToQuaternion (Microsoft.Kinect.Vector4 rot) => new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
    }
}
