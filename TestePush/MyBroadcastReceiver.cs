using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;

//VERY VERY VERY IMPORTANT NOTE!!!!
// Your package name MUST NOT start with an uppercase letter.
// Android does not allow permissions to start with an upper case letter
// If it does you will get a very cryptic error in logcat and it will not be obvious why you are crying!
// So please, for the love of all that is kind on this earth, use a LOWERCASE first letter in your Package Name!!!!
using System.Net.Http;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace TestePush
{
	//You must subclass this!
	[BroadcastReceiver (Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter (new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter (new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter (new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new string[] { "@PACKAGE_NAME@" })]
	public class GcmBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
	{
		//IMPORTANT: Change this to your own Sender ID!
		//The SENDER_ID is your Google API Console App Project ID.
		//  Be sure to get the right Project ID from your Google APIs Console.  It's not the named project ID that appears in the Overview,
		//  but instead the numeric project id in the url: eg: https://code.google.com/apis/console/?pli=1#project:785671162406:overview
		//  where 785671162406 is the project id, which is the SENDER_ID to use!
		public static string[] SENDER_IDS = new string[] { "474698924618" };

		public const string TAG = "PushSharp-GCM";
	}

	[Service] //Must use the service tag
	public class PushHandlerService : GcmServiceBase
	{
		public PushHandlerService () : base (GcmBroadcastReceiver.SENDER_IDS)
		{
		}

		const string TAG = "GCM-SAMPLE";

		protected   override void OnRegistered (Context context, string registrationId)
		{
			Log.Verbose (TAG, "GCM Registered: " + registrationId);


			var url = "http://localhost:8854/api/push/";


			var postData = new List<KeyValuePair<string, string>> ();
			postData.Add (new KeyValuePair<string, string> ("Token", registrationId));



			HttpContent content = new FormUrlEncodedContent (postData);


			var client = new HttpClient ();
			var postTask = client.PostAsync (url, content);
			postTask.Wait ();

			var readTask = postTask.Result.Content.ReadAsStringAsync ();
			readTask.Wait ();
	
			Log.Verbose (TAG, "Retornou: " + readTask.Result);

			createNotification ("GCM Registered...", "The device has been Registered, Tap to View!");
		}

		protected override void OnUnRegistered (Context context, string registrationId)
		{
			Log.Verbose (TAG, "GCM Unregistered: " + registrationId);
			//Remove from the web service
			//	var wc = new WebClient();
			//	var result = wc.UploadString("http://your.server.com/api/unregister/", "POST",
			//		"{ 'registrationId' : '" + lastRegistrationId + "' }");

			createNotification ("GCM Unregistered...", "The device has been unregistered, Tap to View!");
		}

		protected override void OnMessage (Context context, Intent intent)
		{
			Log.Info (TAG, "GCM Message Received!");


			var message = "";
			if (intent != null && intent.Extras != null) {
				
				message = intent.Extras.GetString ("default");
			}

			//Store the message
		

			createNotification ("GCM Sample", message);
		}

		protected override bool OnRecoverableError (Context context, string errorId)
		{
			Log.Warn (TAG, "Recoverable Error: " + errorId);

			return base.OnRecoverableError (context, errorId);
		}

		protected override void OnError (Context context, string errorId)
		{
			Log.Error (TAG, "GCM Error: " + errorId);
		}

		void createNotification (string title, string desc)
		{
			//Create notification
			var notificationManager = GetSystemService (Context.NotificationService) as NotificationManager;

			//Create an intent to show ui
			var uiIntent = new Intent (this, typeof(MainActivity));

			//Create the notification
			var notification = new Notification (Android.Resource.Drawable.SymActionEmail, title);

			//Auto cancel will remove the notification once the user touches it
			notification.Flags = NotificationFlags.AutoCancel;

			//Set the notification info
			//we use the pending intent, passing our ui intent over which will get called
			//when the notification is tapped.
			notification.SetLatestEventInfo (this, title, desc, PendingIntent.GetActivity (this, 0, uiIntent, 0));

			//Show the notification
			notificationManager.Notify (1, notification);
		}



	}
}

