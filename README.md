# Firebase-Test
A test project to show issues with FirebaseAccountLinkExceptions

The main issue is reached by step 7. We only are every receiving FirebaseExceptions. We never receive a FirebaseAccountLinkException.

A secondary issue can be seen with step 3a. The **LinkAndRetrieveData** enpoint throws an exception when the SignInResult's User property is accessed.

## 1. Load up Scenes/SampleScene.unity.
This scene has a basic GUI setup to run through the steps needed to replicate the issues.

## 2. Create Anonymous Profile
![Step2](/images/Step2.png)
Once the scene is running, press the left most button to create an anonymous firebase profile. This mimics the flow all of our users follow when they start our game for the first time.

## 3. Bind Email
![Step3](/images/Step3.png)
Now you will be shown the token returned for the new profile, and fields to enter an email and password. The password needs to be at least 6 characters. Either button on the right of the new fields will successfully bind a new email to the previously created anonymous user. 

### 3a. LinkAndRetrieveData Exception
![Exception](/images/ThrownException.png)
The RED button labeled "LinkAndRetrieveData" will trigger a NullReferenceException when accessing that response's User property.

## 4. Sign Out
![Step4](/images/Step4.png)
After the email has been linked, or the NullReferenceException has been triggered, you can press the 'Sign Out' button to return to the 1st state.

## 5. Create a New Anonymous Profile
Now create a new profile to mimic the actions of a user returning to the game on a new device, after reinstalling the game, etc.

## 6. Attempt to bind
You'll see that your credentails from step 3 are still filled in to the email and password fields. Using the same credentials from step 3, attempt to bind your email to the new profile.

## 7. EmailAlreadyInUse
![Step7](/images/Step7.png)
You can see in the console log all we get is a "Firebase.FirebaseException: The email address is already in use by another account.". We never generate a FirebaseAccountLinkException.


You are also more than welcome to use the email and password (lxmllr71+test10(at)gmail.com & 123456) from these images when attempting to bind rather than any of your own. Then you'd be able to skip steps 4, 5, and 6 and just see the standard FirebaseException straight away
