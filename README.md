# Kinect For Beat Saber
Kinect For Beat Saber is a heavily in-development plugin designed to take Skeleton info from your Kinect, and transform it into tracking points in Beat Saber.

I do *not* recommend getting it built and set up right now, it is still in development and needs more time to mature.

Kinect For Beat Saber is currently for the Xbox 360 Kinect ***ONLY***. If you happen to have an Xbox One Kinect all good and ready to develop with, shoot me a pull request if you manage to adapt Kinect For Beat Saber for an Xbox One Kinect.

# Build Requirements
* **.NET Framework 4.7.2** for building both the Plugin and Console Application
* Two copies of **Microsoft.Kinect.dll** and **Microsoft.Kinect.Toolkit.dll** (Placed in the root Beat Saber directory, and in the `Libs` folder) for the Console Application and Plugin to depend off of
* A connected and ready **Xbox 360 Kinect**. This is not needed for building, but is needed to play.

# Other Notes
* The Projects have Post-Build events that copy the built files to my personal Beat Saber directory. Either modify or remove these before building so you don't run into issues.
* I do not have Reference Paths set up, so you will have to re-add each and every reference (Some might be janky)
* Any arguments sent into `KinectForBeatSaber.exe` (Such as `--verbose` and `fpfc`) will transfer over to Beat Saber.
