using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public static class FirebaseAuthLogic
{
    // provider id's taken from this list: https://stackoverflow.com/questions/46901153/what-is-the-full-list-of-provider-ids-for-firebase-userinfo-providerid
    public static bool IsUserBoundWithEmail
    {
        get => FirebaseManager.Auth.CurrentUser.ProviderData.Any(userInfo => userInfo.ProviderId == "password");
    }
    
    public static bool IsUserBoundWithApple
    {
        get => FirebaseManager.Auth.CurrentUser.ProviderData.Any(userInfo => userInfo.ProviderId == "apple.com");
    }
    
    public static bool IsUserBoundWithFacebook
    {
        get => FirebaseManager.Auth.CurrentUser.ProviderData.Any(userInfo => userInfo.ProviderId == "facebook.com");
    }
    
    public static bool IsUserBoundWithGoogle
    {
        get => FirebaseManager.Auth.CurrentUser.ProviderData.Any(userInfo => userInfo.ProviderId == "google.com");
    }
    
    public static async Task<(string firebaseToken, AuthError authError)> CreateAnonymousFirebaseUserAsync()
    {
        FirebaseUser newUser = null;
        AuthError authError = AuthError.None;
        await FirebaseManager.Auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("FirebaseAuth SignInAnonymouslyAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("FirebaseAuth SignInAnonymouslyAsync encountered an error: " + task.Exception);
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            newUser = task.Result;
        });

        if (newUser == null)
        {
            return (null, authError);
        }
		
        Debug.Log($"User signed in successfully: {newUser.DisplayName} ({newUser.UserId})");
		
        var getTokenResult = await GetFirebaseTokenAsync();
        authError = getTokenResult.authError;
		
        if (string.IsNullOrEmpty(getTokenResult.firebaseToken))
        {
            Debug.LogError("Unable to obtain FirebaseToken for new user creation.");
            return (null, authError);
        }

        return (getTokenResult.firebaseToken, authError);
    }

    public static async Task<(string firebaseToken, AuthError authError)> GetFirebaseTokenAsync()
    {
        string firebaseToken = null;
        AuthError authError = AuthError.None;
        await FirebaseManager.Auth.CurrentUser.TokenAsync(true).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("TokenAsync was canceled.");
                return;
            }

            if (task.IsFaulted) {
                Debug.LogError("TokenAsync encountered an error: " + task.Exception);
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            firebaseToken = task.Result;
        });

        return (firebaseToken, authError);
    }
    
    public static async Task<(SignInResult signInResult, AuthError authError)> LinkAndRetrieveDataWithCredentials (Credential credentials, FirebaseUser firebaseUser)
    {
        SignInResult result = null;
        AuthError authError = AuthError.None;
        await firebaseUser.LinkAndRetrieveDataWithCredentialAsync(credentials).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("FirebaseAuth LinkAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"FirebaseAuth LinkAndRetrieveDataWithCredentialAsync encountered an error: {task.Exception}");
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            result = task.Result;
            Debug.Log($"Credentials successfully linked to Firebase user: {firebaseUser.DisplayName} {firebaseUser.UserId}");
        });

        return (result, authError);
    }

    public static async Task<(FirebaseUser firebaseUser, AuthError authError)> LinkCredentialToUserAsync(Credential credential, FirebaseUser firebaseUser)
    {
        FirebaseUser resultUser = null;
        AuthError authError = AuthError.None;
        await firebaseUser.LinkWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("FirebaseAuth LinkWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError($"FirebaseAuth LinkWithCredentialAsync encountered an error: {task.Exception}");
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            resultUser = task.Result;
            Debug.Log($"Credentials successfully linked to Firebase user: {firebaseUser.DisplayName} {firebaseUser.UserId}");
        });

        return (resultUser, authError);
    }

    public static async Task<(FirebaseUser firebaseUser, AuthError authError)> SignInWithEmailPasswordAsync(string email, string password)
    {
        FirebaseUser resultUser = null;
        AuthError authError = AuthError.None;
        await FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            resultUser = task.Result;
            Debug.Log($"User signed in successfully: {resultUser.DisplayName} {resultUser.UserId}");
        });

        return (resultUser, authError);
    }

    public static async Task<AuthError> SendVerificationEmailAsync(FirebaseUser firebaseUser)
    {
        AuthError authError = AuthError.None;
        await firebaseUser.SendEmailVerificationAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SendEmailVerificationAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"SendEmailVerificationAsync encountered an error: {task.Exception}");
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            Debug.Log($"Confirmation email sent to '{firebaseUser.Email}' for Firebase user: {firebaseUser.DisplayName} {firebaseUser.UserId}");
        });

        return authError;
    }

    public static async Task<AuthError> SendPasswordResetEmailAsync(string email)
    {
        AuthError authError = AuthError.None;
        await FirebaseManager.Auth.SendPasswordResetEmailAsync(email).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError($"SendPasswordResetEmailAsync encountered an error: {task.Exception}");
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }
            
            Debug.Log($"Firebase call to SendPasswordResetEmailAsync succeeded for email {email}");
        });

        return authError;
    }
    
    public static async Task<(FirebaseUser firebaseUser, AuthError authError)> SignInWithCredentialAsync(Credential credential)
    {
        FirebaseUser resultUser = null;
        AuthError authError = AuthError.None;
        await FirebaseManager.Auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                authError = AuthErrorFromTaskException(task.Exception);
                return;
            }

            resultUser = task.Result;
            Debug.Log($"User signed in successfully: {resultUser.DisplayName} {resultUser.UserId}");
        });

        return (resultUser, authError);
    }

    public static (string rawNonce, string nonce) GetNonceTokenForAppleSignIn()
    {
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
        return (rawNonce, nonce);
    }

    public static string StringIdFromAuthError(AuthError authError)
    {
        switch (authError)
        {
            case AuthError.None:
                return string.Empty;
            case AuthError.EmailAlreadyInUse:
                return "ERROR_FIREBASE_AUTH_EMAIL_IN_USE";
            case AuthError.WeakPassword:
                return "ERROR_FIREBASE_AUTH_WEAK_PASSWORD";
            case AuthError.UserNotFound:
                return "ERROR_FIREBASE_AUTH_USER_NOT_FOUND";
            case AuthError.WrongPassword:
                return "ERROR_FIREBASE_AUTH_WRONG_PASSWORD";
            case AuthError.CredentialAlreadyInUse:
                return "ERROR_FIREBASE_AUTH_CREDENTIAL_IN_USE";
            default:
                // todo: do we want to maybe throw an exception here so we can see in Crashlytics what other errors are firing?
                Debug.LogError($"Encountered Firebase AuthError: {authError}");
                return "ERROR_FIREBASE_AUTH_GENERIC";
        }
    }

    private static AuthError AuthErrorFromTaskException(AggregateException aggregateException)
    {
        foreach (var innerException in aggregateException.InnerExceptions)
        {
            if (innerException is FirebaseException firebaseException)
            {
                return (AuthError)firebaseException.ErrorCode;
            }
            if (innerException is FirebaseAccountLinkException firebaseAccountLinkException)
            {
                return (AuthError)firebaseAccountLinkException.ErrorCode;
            }
            if (innerException is AggregateException subAggregateException)
            {
                return AuthErrorFromTaskException(subAggregateException);
            }
        }
        return AuthError.None;
    }
    
    // Generates a random string, needed to send to Firebase as nonce for Sign in with Apple.
    // https://github.com/lupidan/apple-signin-unity/wiki/Sign-in-with-Apple-Unity-Plugin:-Working-with-Firebase#step-2-generating-a-random-raw-nonce
    private static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }
    
    // Generate SHA256 hash of provided raw nonce.
    // https://github.com/lupidan/apple-signin-unity/wiki/Sign-in-with-Apple-Unity-Plugin:-Working-with-Firebase#step-3-generate-the-sha256-of-the-raw-nonce
    private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }
}