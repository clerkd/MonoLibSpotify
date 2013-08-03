/*
Copyright (c) 2012-2013 Tim Ermilov, Clerkd, yamalight@gmail.com

Based on source code from:
https://github.com/jonasl/libspotify-sharp
Copyright (c) 2009 Jonas Larsson, jonas@hallerud.se

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MonoLibSpotify.Events;
using MonoLibSpotify.Browsers;

namespace MonoLibSpotify.Models
{
	public class SPSession
	{
		public delegate void SessionEventHandler(SPSession sender, SessionEventArgs e);
		public delegate void AlbumBrowseEventHandler(SPSession sender, AlbumBrowseEventArgs e);
		public delegate void ArtistBrowseEventHandler(SPSession sender, ArtistBrowseEventArgs e);
		public delegate void SearchEventHandler(SPSession sender, SearchEventArgs e);
		public delegate void MusicDeliveryEventHandler(SPSession sender, MusicDeliveryEventArgs e);
		public delegate void ImageEventHandler(SPSession sender, ImageEventArgs e);

		static Dictionary<IntPtr, SPSession> sessions = new Dictionary<IntPtr, SPSession>();		

		#region Callbacks
		static sp_session_callbacks callbacks;
		
		delegate void logged_in_delegate(IntPtr session, sp_error error);
		delegate void logged_out_delegate(IntPtr session);
		delegate void connection_error_delegate(IntPtr sessionPtr, sp_error error);
		delegate void log_message_delegate(IntPtr sessionPtr, string message);
		delegate void metadata_updated_delegate(IntPtr sessionPtr);
		delegate void message_to_user_delegate(IntPtr sessionPtr, string message);
		delegate void notify_main_thread_delegate(IntPtr sessionPtr);
		delegate int music_delivery_delegate(IntPtr sessionPtr, IntPtr formatPtr, IntPtr framesPtr, int num_frames);
		delegate void play_token_lost_delegate(IntPtr sessionPtr);
		delegate void end_of_track_delegate(IntPtr sessionPtr);
		delegate void streaming_error_delegate(IntPtr sessionPtr, sp_error error);
		delegate void userinfo_updated_delegate(IntPtr sessionPtr);
		
		delegate void albumbrowse_complete_cb_delegate(IntPtr albumBrowsePtr, IntPtr userDataPtr);		
		delegate void artistbrowse_complete_cb_delegate(IntPtr albumBrowsePtr, IntPtr userDataPtr);
		delegate void search_complete_cb_delegate(IntPtr searchPtr, IntPtr userDataPtr);
		delegate void image_loaded_cb_delegate(IntPtr imagePtr, IntPtr userDataPtr);

		static logged_in_delegate logged_in = new logged_in_delegate(LoggedInCallback);
		static logged_out_delegate logged_out = new logged_out_delegate(LoggedOutCallback);
		static connection_error_delegate connection_error = new connection_error_delegate(ConnectionErrorCallback);
		static log_message_delegate log_message = new log_message_delegate(LogMessageCallback);
		static metadata_updated_delegate metadata_updated = new metadata_updated_delegate(MetadataUpdatedCallback);
		static message_to_user_delegate message_to_user = new message_to_user_delegate(MessageToUserCallback);
		static notify_main_thread_delegate notify_main_thread = new notify_main_thread_delegate(NotifyMainThreadCallback);
		static music_delivery_delegate music_delivery = new music_delivery_delegate(MusicDeliveryCallback);
		static play_token_lost_delegate play_token_lost = new play_token_lost_delegate(PlayTokenLostCallback);
		static end_of_track_delegate end_of_track = new end_of_track_delegate(EndOfTrackCallback);
		static streaming_error_delegate streaming_error = new streaming_error_delegate(StreamingErrorCallback);
		static userinfo_updated_delegate userinfo_updated = new userinfo_updated_delegate(UserinfoUpdatedCallback);
		#endregion

		#region Events
		public event SessionEventHandler OnLoginComplete;
		public event SessionEventHandler OnLoggedOut;
		public event SessionEventHandler OnMetaDataUpdated;
		public event SessionEventHandler OnConnectionError;
		public event SessionEventHandler OnMessageToUser;
		public event SessionEventHandler OnPlayTokenLost;
		public event SessionEventHandler OnLogMessage;
		public event AlbumBrowseEventHandler OnAlbumBrowseComplete;
		public event ArtistBrowseEventHandler OnArtistBrowseComplete;
		public event SearchEventHandler OnSearchComplete;
		public event ImageEventHandler OnImageLoaded;
		public event SessionEventHandler OnEndOfTrack;
		public event SessionEventHandler OnStreamingError;
		public event SessionEventHandler OnUserinfoUpdated;
		
		public event SessionEventHandler OnException;
		
		/* NOTE
		 * 
		 * Do _NOT_ call / access anything that calls back into libspotify when handling this
		 * Accessing current Track is OK, anything else is not.
		 * 
		 */
		public event MusicDeliveryEventHandler OnMusicDelivery;		
		#endregion

		albumbrowse_complete_cb_delegate albumbrowse_complete_cb;
		artistbrowse_complete_cb_delegate artistbrowse_complete_cb;
		search_complete_cb_delegate search_complete_cb;
		image_loaded_cb_delegate image_loaded_cb;

		Thread mainThread;
		Thread eventThread;

		ManualResetEvent loginHandle = null;
		ManualResetEvent logoutHandle = null;
		public sp_error loginResult = sp_error.IS_LOADING;

		AutoResetEvent mainThreadNotification = new AutoResetEvent(false);
		AutoResetEvent eventThreadNotification = new AutoResetEvent(false);
		Queue<EventWorkItem> eventQueue = new Queue<EventWorkItem>();

		Dictionary<int, object> states = new Dictionary<int, object>();

		ushort userStateCtr = 1;
		int UserStateId {
			get {
				int result = userStateCtr;
				userStateCtr++;
				return result;
			}
		}

		IntPtr pointer;
		public IntPtr SessionPointer {
			get { return pointer; }
		}

		public sp_connectionstate ConnectionState
		{
			get {
				try {
				    if (pointer == IntPtr.Zero)
				    {
				        return sp_connectionstate.UNDEFINED;
				    }
				    else
				    {
				        lock (LibspotifyWrapper.Mutex)
				            return LibspotifyWrapper.Session.ConnectionState(pointer);
				    }
				} catch {
					return sp_connectionstate.UNDEFINED;
				}
			}
		}

		public bool Search(string query, int trackOffset, int trackCount, int albumOffset, int albumCount, int artistOffset, 
		                   int artistCount, int playlistOffset, int playlistCount, sp_search_type searchType, object state)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				Console.WriteLine("SPOTIFY SESSION: searching for "+query);

				var id = UserStateId;				
				states[id] = state;
				var browsePtr = LibspotifyWrapper.Search.Create(SessionPointer, query, trackOffset, trackCount, albumOffset, albumCount, artistOffset, artistCount, playlistOffset, 
				                                                  playlistCount, searchType, Marshal.GetFunctionPointerForDelegate(search_complete_cb), new IntPtr(id));				
				return browsePtr != IntPtr.Zero;
			}
		}

		public bool BrowseAlbum(SPAlbum album, object state)
		{	
			lock(LibspotifyWrapper.Mutex)
			{
				var id = UserStateId; 
				states[id] = state;
				var browsePtr = LibspotifyWrapper.Browse.Album.Create(SessionPointer, album.albumPtr,
				                                                    Marshal.GetFunctionPointerForDelegate(albumbrowse_complete_cb), new IntPtr(id));
				return browsePtr != IntPtr.Zero;
			}
		}

		public bool BrowseArtist(SPArtist artist, object state)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var id = UserStateId;
				states[id] = state;
				var browsePtr = LibspotifyWrapper.Browse.Artist.Create(SessionPointer, artist.artistPtr, 
				                                                     Marshal.GetFunctionPointerForDelegate(artistbrowse_complete_cb), new IntPtr(id));
				return browsePtr != IntPtr.Zero;
			}			
		}

		public bool LoadImage(string id, object state)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			
			if (id.Length != 40)
				throw new ArgumentException("invalid id");
			
			var idArray = LibspotifyWrapper.StringToImageId(id);
			
			if(idArray.Length != 20)
				throw new Exception("Internal error in LoadImage");
			
			lock (LibspotifyWrapper.Mutex)
			{
				var idPtr = IntPtr.Zero;
				try
				{
					idPtr = Marshal.AllocHGlobal(idArray.Length);
					Marshal.Copy(idArray, 0, idPtr, idArray.Length);
					
					var stateId = UserStateId;
					states[stateId] = state;
					
					var imagePtr = LibspotifyWrapper.Image.Create(SessionPointer, idPtr);
					if (LibspotifyWrapper.Image.IsLoaded(imagePtr))
						ImageLoadedCallback(imagePtr, new IntPtr(stateId));
					else
						LibspotifyWrapper.Image.AddLoadCallback(imagePtr, Marshal.GetFunctionPointerForDelegate(image_loaded_cb), new IntPtr(stateId));
					
					return idPtr != IntPtr.Zero;
				}
				finally
				{
					if (idPtr != IntPtr.Zero)
						Marshal.FreeHGlobal(idPtr);
				}
			}
		}

		public static SPSession CreateInstance(byte[] applicationKey, string uAgent = "MyMonoApp", SPAsyncLoadingPolicy loadPolicy = SPAsyncLoadingPolicy.Immediate)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				if(sessions.Count > 0) {
					throw new InvalidOperationException("libspotify can only handle one session at the moment");
				} else {
					var instance = new SPSession(applicationKey, uAgent, loadPolicy);
					sessions.Add(instance.SessionPointer, instance);
					return instance;
				}
			}
		}

		private static SPSession GetSession(IntPtr sessionPtr)
		{
		    SPSession s;
		    return sessions.TryGetValue(sessionPtr, out s) ? s : null;
		}

	    public bool LogIn(string username, string password, bool rememberme = false)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var res = LibspotifyWrapper.Session.Login(pointer, username, password, rememberme, null);
				return res == sp_error.OK;
			}
		}

		public bool ReLogin()
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var res = LibspotifyWrapper.Session.ReLogin(pointer);
				return res == sp_error.OK;
			}
		}

		public void LogOut(bool forget = true)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				if (ConnectionState == sp_connectionstate.LOGGED_IN)
				{
					var res = LibspotifyWrapper.Session.Logout(pointer);
					
					if(res != sp_error.OK)
						throw new SpotifyException(res);

					if(forget){
						res = LibspotifyWrapper.Session.ForgetMe(pointer);

						if(res != sp_error.OK)
							throw new SpotifyException(res);
					}
				}
				else
					EnqueueEventWorkItem(new EventWorkItem(OnLoggedOut, new object[] { this, new SessionEventArgs() }));
			}
		}

		static SPSession()
		{
			// make callbacks
			callbacks = new sp_session_callbacks
			                {
			                    connection_error = Marshal.GetFunctionPointerForDelegate(connection_error),
			                    logged_in = Marshal.GetFunctionPointerForDelegate(logged_in),
			                    logged_out = Marshal.GetFunctionPointerForDelegate(logged_out),
			                    log_message = Marshal.GetFunctionPointerForDelegate(log_message),
			                    message_to_user = Marshal.GetFunctionPointerForDelegate(message_to_user),
			                    metadata_updated = Marshal.GetFunctionPointerForDelegate(metadata_updated),
			                    music_delivery = Marshal.GetFunctionPointerForDelegate(music_delivery),
			                    notify_main_thread = Marshal.GetFunctionPointerForDelegate(notify_main_thread),
			                    play_token_lost = Marshal.GetFunctionPointerForDelegate(play_token_lost),
			                    end_of_track = Marshal.GetFunctionPointerForDelegate(end_of_track),
			                    streaming_error = Marshal.GetFunctionPointerForDelegate(streaming_error),
			                    userinfo_updated = Marshal.GetFunctionPointerForDelegate(userinfo_updated)
			                };
		}

		SPSession (byte[] appKey, string userAgent, SPAsyncLoadingPolicy loadPolicy)
		{
			// Use docs dir as starting point
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			documents = Path.Combine (documents, "..", "Library");

			// Find or create support directory for settings
			var spotifyDirectory = Path.Combine (documents, "Spotify");
			if( !Directory.Exists(spotifyDirectory) ) Directory.CreateDirectory(spotifyDirectory);

			// Find the caches directory for cache
			var cache = Path.Combine (spotifyDirectory, "Caches");
			if( !Directory.Exists(cache) ) Directory.CreateDirectory(cache);
			var tmp = Path.Combine (spotifyDirectory, "Temp");
			if( !Directory.Exists(tmp) ) Directory.CreateDirectory(tmp);

			// make new config
			var config = new sp_session_config
			                 {
			                     api_version = LibspotifyWrapper.API_VERSION,
			                     user_agent = userAgent,
			                     settings_location = spotifyDirectory.ToString(),
			                     cache_location = cache.ToString()
			                 };
		    // folders

		    // callbacks
			var size = Marshal.SizeOf(callbacks);
			config.callbacks = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(callbacks, config.callbacks, true);

			// make app key
			config.application_key = IntPtr.Zero;
			try{
				config.application_key = Marshal.AllocHGlobal(appKey.Length);
				Marshal.Copy(appKey, 0, config.application_key, appKey.Length);
				config.application_key_size = appKey.Length;

				pointer = IntPtr.Zero;
				var createErrorCode = LibspotifyWrapper.Session.Create(ref config, out pointer);
				if (createErrorCode != sp_error.OK) {
					pointer = IntPtr.Zero;
					Console.WriteLine("error creating session");
				} else {
					Console.WriteLine("session created ok");

					albumbrowse_complete_cb = new albumbrowse_complete_cb_delegate(AlbumBrowseCompleteCallback);
					artistbrowse_complete_cb = new artistbrowse_complete_cb_delegate(ArtistBrowseCompleteCallback);
					search_complete_cb = new search_complete_cb_delegate(SearchCompleteCallback);
					image_loaded_cb = new image_loaded_cb_delegate(ImageLoadedCallback);

					mainThread = new Thread(new ThreadStart(MainThread)) {IsBackground = true};
				    mainThread.Start();
					
					eventThread = new Thread(new ThreadStart(EventThread)) {IsBackground = true};
				    eventThread.Start();
				}
			}finally{
				if(config.application_key != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(config.application_key);
				}
			}
		}

		void MainThread()
		{			
			// FIXME Fix this when sessions can be destroyed
			var waitTime = 0;
			
			while(true) {
				mainThreadNotification.WaitOne(waitTime, false);
				do {
					lock(LibspotifyWrapper.Mutex)
					{
						try {
							LibspotifyWrapper.Session.ProcessEvents(SessionPointer, out waitTime);
						} catch {
							waitTime = 1000;
						}
					}
				} while(waitTime == 0);				
			}
		}		
		
		void EventThread()
		{
			var localList = new List<EventWorkItem>();
			
			// FIXME Fix this when sessions can be destroyed
			while(true) {				
				eventThreadNotification.WaitOne();

				lock(LibspotifyWrapper.Mutex)
				{
					while(eventQueue.Count > 0)
						localList.Add(eventQueue.Dequeue());
				}
				
				foreach(var eventWorkItem in localList){
					try {
						eventWorkItem.Execute();					
					} catch(Exception ex) {
						if (OnException != null)
							OnException(this, new SessionEventArgs(ex.ToString()));
					}
				}
				
				localList.Clear();
			}
		}

		internal void EnqueueEventWorkItem(EventWorkItem eventWorkItem)
		{
			lock(eventQueue)
			{
				eventQueue.Enqueue(eventWorkItem);
			}			
			
			eventThreadNotification.Set();
		}

		[MonoPInvokeCallback (typeof(logged_in_delegate))]
		static void LoggedInCallback(IntPtr sessionPtr, sp_error error)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;

			//if(s.ConnectionState == sp_connectionstate.LOGGED_IN && error == sp_error.OK)
				//s.playlistContainer = new PlaylistContainer(libspotify.sp_session_playlistcontainer(sessionPtr), s);
			
			if (s.loginHandle == null) {
				s.EnqueueEventWorkItem(new EventWorkItem(s.OnLoginComplete, new object[] { s, new SessionEventArgs(error) }));
		   	} else {
				try {
					s.loginResult = error;
					s.loginHandle.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
			}
		}

		[MonoPInvokeCallback (typeof(logged_out_delegate))]
		static void LoggedOutCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;

			/*if (s.playlistContainer != null)
			{
				s.playlistContainer.Dispose();
				s.playlistContainer = null;
			}*/
			
			if (s.loginHandle == null && s.logoutHandle == null)
			{
				s.EnqueueEventWorkItem(new EventWorkItem(s.OnLoggedOut, new object[] { s, new SessionEventArgs() }));			
			}
			else
			{
				try
				{
					if (s.loginHandle != null)
						s.loginHandle.Set();				
					if (s.logoutHandle != null)
						s.logoutHandle.Set();
				}
                catch (Exception e)
                {
                    Console.WriteLine(e);
				}
			}
		}

		[MonoPInvokeCallback (typeof(connection_error_delegate))]
		static void ConnectionErrorCallback(IntPtr sessionPtr, sp_error error)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;

			s.EnqueueEventWorkItem(new EventWorkItem(s.OnConnectionError, new object[] { s, new SessionEventArgs(error) }));			
		}

		[MonoPInvokeCallback (typeof(log_message_delegate))]
		static void LogMessageCallback(IntPtr sessionPtr, string message)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;

			// Spotify log msgs can contain unprintable chars. Guessing that they control text color on Win32 or something
			if( message == null ) return;
			message = System.Text.RegularExpressions.Regex.Replace(message, "[\u0000-\u001F]", string.Empty);
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnLogMessage, new object[] { s, new SessionEventArgs(message) }));
		}

		[MonoPInvokeCallback (typeof(metadata_updated_delegate))]
		static void MetadataUpdatedCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnMetaDataUpdated, new object[] { s, new SessionEventArgs() }));
		}

		[MonoPInvokeCallback (typeof(message_to_user_delegate))]
		static void MessageToUserCallback(IntPtr sessionPtr, string message)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnMessageToUser, new object[] { s, new SessionEventArgs(message) }));
		}

		[MonoPInvokeCallback (typeof(play_token_lost_delegate))]
		static void PlayTokenLostCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnPlayTokenLost, new object[] { s, new SessionEventArgs() }));
		}

		[MonoPInvokeCallback (typeof(notify_main_thread_delegate))]
		static void NotifyMainThreadCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.mainThreadNotification.Set();
		}

		[MonoPInvokeCallback (typeof(music_delivery_delegate))]
		static int MusicDeliveryCallback(IntPtr sessionPtr, IntPtr formatPtr, IntPtr framesPtr, int num_frames)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return 0;

			if(num_frames == 0)
				return 0;
			
			var consumed = 0;
			
			byte[] samplesBytes = null;
			var format = (sp_audioformat)Marshal.PtrToStructure(formatPtr, typeof(sp_audioformat));

			samplesBytes = new byte[num_frames * format.channels * 2];
			Marshal.Copy(framesPtr, samplesBytes, 0, samplesBytes.Length);
			
			if(s.OnMusicDelivery != null)
			{
				var e = new MusicDeliveryEventArgs(format.channels, format.sample_type, format.sample_rate, samplesBytes, num_frames);
				s.OnMusicDelivery(s, e);
				
				consumed = e.ConsumedFrames;
			}
			
			return consumed;
		}
		
		[MonoPInvokeCallback (typeof(end_of_track_delegate))]
		static void EndOfTrackCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnEndOfTrack, new object[] { s, new SessionEventArgs() }));
		}

		[MonoPInvokeCallback (typeof(streaming_error_delegate))]
		static void StreamingErrorCallback(IntPtr sessionPtr, sp_error error)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnStreamingError, new object[] { s, new SessionEventArgs(error) }));
		}

		[MonoPInvokeCallback (typeof(userinfo_updated_delegate))]
		static void UserinfoUpdatedCallback(IntPtr sessionPtr)
		{
			var s = GetSession(sessionPtr);
			if (s == null)
				return;
			
			s.EnqueueEventWorkItem(new EventWorkItem(s.OnUserinfoUpdated, new object[] { s, new SessionEventArgs() }));
		}

		// Callbacks are called on our own thread that already has lock -> no locking needed here		
		[MonoPInvokeCallback (typeof(albumbrowse_complete_cb_delegate))]
	 	static void AlbumBrowseCompleteCallback(IntPtr albumBrowsePtr, IntPtr userDataPtr)
		{
			var albumBrowse = new AlbumBrowse(albumBrowsePtr);
			var id = userDataPtr.ToInt32();

			SPSession s = null;
			foreach(var kp in sessions){
				s = kp.Value;
			}
			
			var state = s.states.ContainsKey(id) ? s.states[id] : null;
			
			if (id <= short.MaxValue)
			{
				s.states.Remove(id);
				s.EnqueueEventWorkItem(new EventWorkItem(s.OnAlbumBrowseComplete, new object[] { s, new AlbumBrowseEventArgs(albumBrowse, state) }));
			}
			else
			{
				if (state != null && state is ManualResetEvent)
				{
					s.states[id] = albumBrowse;
					(state as ManualResetEvent).Set();
				}	
			}
		}

		[MonoPInvokeCallback (typeof(artistbrowse_complete_cb_delegate))]
		static void ArtistBrowseCompleteCallback(IntPtr artistBrowsePtr, IntPtr userDataPtr)
		{
			SPSession s = null;
			foreach(var kp in sessions){
				s = kp.Value;
			}

			try
			{
				var artistBrowse = new ArtistBrowse(artistBrowsePtr);
				var id = userDataPtr.ToInt32();
				
				var state = s.states.ContainsKey(id) ? s.states[id] : null;
				
				if (id <= short.MaxValue)
				{
					s.states.Remove(id);
					s.EnqueueEventWorkItem(new EventWorkItem(s.OnArtistBrowseComplete, new object[] { s, new ArtistBrowseEventArgs(artistBrowse, state) }));			
				}
				else
				{
					if (state != null && state is ManualResetEvent)
					{
						s.states[id] = artistBrowse;
						(state as ManualResetEvent).Set();
					}
				}	
			}
			catch(Exception ex)
			{
				s.EnqueueEventWorkItem(new EventWorkItem(s.OnLogMessage, new object[] { s, new SessionEventArgs("E " + ex.Message) }));
			}
		}

		[MonoPInvokeCallback (typeof(search_complete_cb_delegate))]
		static void SearchCompleteCallback(IntPtr searchPtr, IntPtr userDataPtr)
		{
			var search = new SPSearch(searchPtr);

			var id = userDataPtr.ToInt32();

			SPSession s = null;
			foreach(var kp in sessions){
				s = kp.Value;
			}

			var state = s.states.ContainsKey(id) ? s.states[id] : null;
			
			if (id <= short.MaxValue)
			{
				s.states.Remove(id);
				s.EnqueueEventWorkItem(new EventWorkItem(s.OnSearchComplete, new object[] { s, new SearchEventArgs(search, state) }));
			}
			else
			{
				if (state != null && state is ManualResetEvent)
				{
					s.states[id] = search;
					(state as ManualResetEvent).Set();
				}
			}
			
		}

		[MonoPInvokeCallback (typeof(image_loaded_cb_delegate))]
		static void ImageLoadedCallback(IntPtr imagePtr, IntPtr userDataPtr)
		{
			SPSession s = null;
			foreach(var kp in sessions){
				s = kp.Value;
			}
			
			var id = userDataPtr.ToInt32();
			var state = s.states.ContainsKey(id) ? s.states[id] : null;
			ManualResetEvent wh = null;
			var isSync = id > short.MaxValue;
			if (isSync)
			{
				if (state == null || !(state is ManualResetEvent))
					return;
				wh = state as ManualResetEvent;
			}
			
			try
			{	
				// No locking needed since this is called on our own thread
				// that already holds the lock.
				
				/*
				 * libspotify was _REALLY_ stupidly written regarding image format.
				 * 
				 * The first versions gave you pointers to decoded image data instead
				 * of raw image data. libspotify devs seems to have listened to 
				 * the complaints about this. Yay for them.
				 */
				
				var lengthPtr = IntPtr.Zero;				
				var dataPtr = LibspotifyWrapper.Image.Data(imagePtr, out lengthPtr);
				
				var length = lengthPtr.ToInt32();
				
				var imageData = new byte[length];
				Marshal.Copy(dataPtr, imageData, 0, imageData.Length);

				var data = NSData.FromArray(imageData);
				var bmp = UIImage.LoadFromData(data);
				
				if (!isSync)
				{
					s.EnqueueEventWorkItem(new EventWorkItem(s.OnImageLoaded, new object[] { s, 
						new ImageEventArgs(bmp, LibspotifyWrapper.ImageIdToString(LibspotifyWrapper.Image.ID(imagePtr)), state) }
					));
				}
				else if (wh != null)
				{
					s.states[id] = bmp;
					wh.Set();
				}
				
			}
			catch(Exception ex)
			{
				if (!isSync)
				{
					s.EnqueueEventWorkItem(new EventWorkItem(s.OnImageLoaded, new object[] { s, 
						new ImageEventArgs(ex.Message, LibspotifyWrapper.ImageIdToString(LibspotifyWrapper.Image.ID(imagePtr)), state) }
					));
				}
				else if (wh != null)
				{
					s.states[id] = null;
					wh.Set();
				}
			}
			finally
			{	
				LibspotifyWrapper.Image.Release(imagePtr);
			}
			
		}

		public void SetBitrate (sp_bitrate bitrate)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				LibspotifyWrapper.Session.PreferredBitrate(SessionPointer, bitrate);
			}
		}

		public sp_error PlayerLoad(SPTrack track)
		{
			PlayerUnload();
			
			lock(LibspotifyWrapper.Mutex)
			{
				var err = LibspotifyWrapper.Player.Load(SessionPointer, track.trackPtr);
				if (err == sp_error.OK)
					track.CheckLoaded();
				
				return err;
			}
		}
		
		public sp_error PlayerUnload()
		{
			lock(LibspotifyWrapper.Mutex)
			{
				return LibspotifyWrapper.Player.Unload(SessionPointer);
			}
		}
		
		public sp_error PlayerSeek(int offset)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				return LibspotifyWrapper.Player.Seek(SessionPointer, offset);
			}	
		}
		
		public sp_error PlayerPlay(bool play)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				return LibspotifyWrapper.Player.Play(SessionPointer, play);
			}	
		}
	}
}

