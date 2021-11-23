# Firebase-Test
A test project to show issues with FirebaseAccountLinkExceptions

SampleScene has a simple GUI setup to run through the issues we have with getting a FirebaseAccountLinkException.

We start with creating an anonymous account for all new users.
Then when ready, we prompt them to link credentials of their choice to their account. (only email needed for this example)
If the email is already in use, we always get a FirebaseException rather than a FirebaseAccountLinkException.

Also shown is the issue with LinkAndRetrieveDataWithCredentials. This endpoint does work to link the credentials, but the response will throw a NullReferenceException. We know this can be worked around, but thought you should be aware of the exception.
