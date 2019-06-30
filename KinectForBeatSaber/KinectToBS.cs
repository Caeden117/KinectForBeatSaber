﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                if (Plugin.skeletonsToProcess.Last() != skeletons)
                    Plugin.skeletonsToProcess.Remove(skeletons);
            }
        }

        private void RefreshTrackingPoints(Skeleton skeleton)
        {
            for (int i = 0; i < 20; i++)
                RefreshJoint(skeleton, (JointType)i);
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
                trackingPoints[joint.JointType].localPosition = SkeletonPointToVector3(joint.Position);
                Vector3 offset = Vector3.zero;
                if (jointType == JointType.Head)
                {
                    try
                    {
                        Vector3 camForw = Camera.main.transform.forward;
                        Vector3 mirrored = Vector3.Reflect(camForw, new Vector3(0, 0, 1)); //Flips camera rotation on the Z axis
                        trackingPoints[joint.JointType].localRotation = Quaternion.LookRotation(mirrored, Camera.main.transform.up);
                    }
                    catch { }
                }
                else if (jointType == JointType.AnkleLeft)
                {
                    if (trackingPoints.TryGetValue(JointType.FootLeft, out Transform foot))
                        trackingPoints[joint.JointType].LookAt(foot);
                    trackingPoints[joint.JointType].Rotate(0, 90, 180, Space.Self);
                }
                else if (jointType == JointType.AnkleRight)
                {
                    if (trackingPoints.TryGetValue(JointType.FootRight, out Transform foot))
                        trackingPoints[joint.JointType].LookAt(foot);
                    trackingPoints[joint.JointType].Rotate(0, 90, 180, Space.Self);
                }
                else
                {
                    trackingPoints[joint.JointType].localRotation = BoneRotationToQuaternion(skeleton.BoneOrientations[jointType].AbsoluteRotation.Quaternion) * Quaternion.Euler(offset);
                }
                Color trackingColor = Color.white;
                switch (joint.TrackingState)
                {
                    case JointTrackingState.Inferred:
                        trackingColor = Color.yellow;
                        break;
                    case JointTrackingState.NotTracked:
                        trackingColor = Color.red;
                        break;
                    default: break;
                }
                trackingPoints[joint.JointType].GetComponent<Renderer>().material.color = trackingColor;
            }
        }

        private Vector3 SkeletonPointToVector3 (SkeletonPoint point)
        {
            return new Vector3(point.X * -1, point.Y, point.Z);
        }

        private Quaternion BoneRotationToQuaternion (Microsoft.Kinect.Vector4 rot)
        {
            return new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
        }
    }
}
