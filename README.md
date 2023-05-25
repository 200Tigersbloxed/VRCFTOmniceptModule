# VRCFTOmniceptModule
A VRCFaceTracking Module that adds support for the HP Omnicept


## How to Install

1) Download the [Latest Release](https://github.com/200Tigersbloxed/VRCFTOmniceptModule/releases/latest/download/VRCFTOmniceptModule.dll)
2) Move the dll file to `%AppData%\VRCFaceTracking\CustomLibs`
3) Launch [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking/releases/latest/download/VRCFaceTracking_x64.appinstaller)
    + Note: Version 5.0.0.0 or higher is required!
4) When prompted by your HP Omnicept Tray Application, Allow VRCFaceTracking.exe to read EyeTacking Data

## How to Allow from the HP Omnicept Tray Application

*Please get to at least step 3 in the How to Install*

1) Make sure that you have `Eye tracking` enabled in your `Sensor Privacy` tab

![gfbhjdbhjfdg](https://user-images.githubusercontent.com/45884377/166843299-26f9ca52-d47c-47c0-97a3-9393752a7fd7.PNG)

2) After launching VRCFaceTracking.exe, you will either see a notification, or an application request access to EyeTracking data, allow VRCFaceTracking.exe access to the data.

![hjbgfbhjfdgbhj](https://user-images.githubusercontent.com/45884377/166843378-75017d66-18cf-4bb7-891f-c5fcccc0f860.PNG)

*Example of Message Popup*

![fgbhjfdghbj](https://user-images.githubusercontent.com/45884377/166843819-6e03ae5b-bba5-4ba9-a294-268b983b4101.jpg)

*Example of Incoming Subscriptions Request tab*

> ___
> ### ⚠️ WARNING ⚠️
> VRCFTOmniceptModule will only request the following Message types:
> 
> + `EYE_TRACKING`
> 
> If it requests any more than that, be weary, as your build may be unofficial or even malicious!
> ___
