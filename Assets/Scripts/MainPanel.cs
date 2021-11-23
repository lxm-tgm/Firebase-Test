
using System.Collections;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public Text output;

    public InputField emailInput;
    public InputField passwordInput;

    public GameObject bindRoot;
    
    public Button createAnonBtn;
    public Button bindBtn;
    public Button signOutBtn;
    
    void Start()
    {
        FirebaseManager.instance.Init();

        StartCoroutine(Init());
    }

    IEnumerator Init ()
    {
        while (!FirebaseManager.instance.initialized)
            yield return null;
        
        if (FirebaseManager.Auth.CurrentUser != null)
        {
            Debug.LogError("Signing Out of current firebase user");
            FirebaseManager.Auth.SignOut();
        }

        output.text = "";
        
        bindRoot.SetActive(false);
        createAnonBtn.interactable = true;
        bindBtn.interactable = false;
        signOutBtn.interactable = false;
    }

    public async void OnCreate ()
    {
        var createUserResult = await FirebaseAuthLogic.CreateAnonymousFirebaseUserAsync();
        var txt = "Created Anonymous User:\n";
        txt += $"\t\tToken: {createUserResult.firebaseToken}";
        output.text = txt;
        
        bindRoot.SetActive(true);
        createAnonBtn.interactable = false;
        bindBtn.interactable = true;
        signOutBtn.interactable = true;
    }

    public void OnBind ()
    {
        bindRoot.SetActive(true);
        createAnonBtn.interactable = false;
        bindBtn.interactable = true;
        signOutBtn.interactable = true;
    }

    /// <summary>
    /// Correctly binds and email credential to the user, but neither this nor RetrieveData will never throw a FirebaseAccountLinkException.
    /// They only ever throw regular FirebaseExceptions.
    /// </summary>
    public async void OnBindConfirm_LinkCredentials ()
    {
        var cred = EmailAuthProvider.GetCredential(emailInput.text, passwordInput.text);
        
        var resp = await FirebaseAuthLogic.LinkCredentialToUserAsync(cred, FirebaseManager.Auth.CurrentUser);

        string txt = "";
        if (resp.authError != AuthError.None)
        {
            txt = $"{resp.authError}";
        } else
        {
            txt = $"{resp.firebaseUser.Email} - Bound to user {resp.firebaseUser.UserId}";
        }
        output.text = txt;
        
        bindRoot.SetActive(true);
        createAnonBtn.interactable = false;
        bindBtn.interactable = true;
        signOutBtn.interactable = true;
    }
    
    /// <summary>
    /// This will correctly link the credentials to the user, but it throws an exception trying to access it's response.
    ///      simple flows can be created to work around this error, but this exception should not happen.
    /// </summary>
    public async void OnBindConfirm_LinkAndRetrieveData ()
    {
        var cred = EmailAuthProvider.GetCredential(emailInput.text, passwordInput.text);
        
        var resp = await FirebaseAuthLogic.LinkAndRetrieveDataWithCredentials(cred, FirebaseManager.Auth.CurrentUser);

        string txt = "";
        if (resp.authError != AuthError.None)
        {
            txt = $"{resp.authError}";
        } else
        {
            // Throws an Exception because SignInResult.authProxy is null
            txt = $"{resp.signInResult.User.Email} - Bound to user {resp.signInResult.User.UserId}";
        }
        output.text = txt;
        
        bindRoot.SetActive(true);
        createAnonBtn.interactable = false;
        bindBtn.interactable = true;
        signOutBtn.interactable = true;
    }

    public void OnSignOut ()
    {
        FirebaseManager.Auth.SignOut();

        output.text = "Signed Out";
        
        bindRoot.SetActive(false);
        createAnonBtn.interactable = true;
        bindBtn.interactable = false;
        signOutBtn.interactable = false;
    }
}
