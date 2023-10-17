# SoftWing
Soft input controller for the LG Wing. 
Since LG hasn't made a native keyboard that operates on the second screen, I went ahead and made my own. In addition to a virtual gamepad, SoftWing also provides the ability to set custom swivel open/closed sounds via the "Sound Settings" menu.

A stable build of SoftWing is available on the Google Play store: https://play.google.com/store/apps/details?id=com.jodonlucas.softwing&hl=en_US&gl=US&pli=1

A quick demo of this version of the controller is available here: https://www.youtube.com/watch?v=5OY26rFnDjQ

## Important information about the latest builds in this repository
The latest builds in this repository have deviated from the last available version on the Google Play store. SoftWing version 3.0.3 was the most recent version of SoftWing posted to the play store. Since then, SoftWing has been modified to expand the features and stability of the controller. The consequence of these changes is that, in order for the app to work, users must grant the app WRITE_SECURE_SETTINGS permissions, which can only be done through the android debugger (ADB). Without those permissions, users will be unable to open the controller on the bottom display.

Since this is an unreasonable requirement for the average user, I have decided to leave the touch-only version of the app on the app store. The more feature-rich, but setup intensive, version of the app will only be available for download from this repo as an APK.

For instructions on how to setup and use SoftWing, please see the help instructions in the app (Controller Settings -> Help).

