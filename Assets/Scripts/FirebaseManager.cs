using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Crashlytics;
using UnityEngine;

public class FirebaseManager : MonoBehaviour {
	public static FirebaseManager instance { get; private set; }
	
	public static FirebaseAuth Auth { get; private set; }

	private FirebaseApp app;
	public bool initialized { get; private set; }
	
	void Awake() {
		instance = this;
	}

	public void Init() {
		InitInternal();
	}

	/// <summary>
	/// https://stackoverflow.com/a/59714879
	/// any reference to Firebase.Messaging will trigger the service to initialize, and fire the 'Allow Notifs' prompt on iOS.
	/// we need to delay these listeners until we know the user has already seen our interstitial panel. 
	/// </summary>
	public void InitFCMListeners ()
	{
		Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
		Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
	}

	private async void InitInternal() {
		await InitAsync();
	}

	private async Task InitAsync() {
		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

		if (dependencyStatus == DependencyStatus.Available) {
#if UNITY_EDITOR
			var projectRoot = Path.GetDirectoryName(Application.dataPath);
			app = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, projectRoot);
#else
			app = FirebaseApp.DefaultInstance;
#endif
			Auth = FirebaseAuth.GetAuth(app);
			
			Debug.Log($"Firebase successfully initialized, ProjectId: {app.Options.ProjectId}");
			Debug.Log($"Firebase.Crashlytics.IsCrashlyticsCollectionEnabled: {Crashlytics.IsCrashlyticsCollectionEnabled}");

			initialized = true;
		} else {
			Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
		}
	}
	
	public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
		UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
	}

	public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
		UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
	}
}
