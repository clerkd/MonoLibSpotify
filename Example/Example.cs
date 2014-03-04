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
using System.Threading;

using MonoTouch.AudioToolbox;

using MonoLibSpotify.Events;
using MonoLibSpotify.Models;
using MonoLibSpotify.Player;

namespace MonoLibSpotify
{
	public class Example
	{
		#region spotifyConfig
		byte[] appKey = {
			0x01, 0x02, 0x03, 0x04 // your app key here
		};
		const string userAgent = "com.example.App";
		#endregion
		
		// libspotify reference
		LibSpotify sp;

		// player 
		SPPlayeriOS player;
		
		public Example ()
		{
			// try outputting libspotify version, if library is not properly linked, you'll get an error here
			Console.WriteLine("Spotify version: "+LibSpotify.BuildId);

			// init libspotify
			initSpotify();
			// init player
			initPlayer();
		}

		public Example (string user, string pass) : this ()
		{
			Login(user, pass);
		}

		void initSpotify ()
		{
			// init
			Console.WriteLine("Starting spotify init");
			sp = new LibSpotify(appKey, userAgent);
			
			// assign events
			sp.CurrentSession.OnConnectionError += HandleOnConnectionError;
			sp.CurrentSession.OnLoggedOut += HandleOnLoggedOut;
			sp.CurrentSession.OnLoginComplete += HandleOnLoginComplete;
			sp.CurrentSession.OnLogMessage += HandleOnLogMessage;
			sp.CurrentSession.OnMessageToUser += HandleOnMessageToUser;			
			sp.CurrentSession.OnPlayTokenLost += HandleOnPlayTokenLost;			
			sp.CurrentSession.OnSearchComplete += HandleOnSearchComplete;			
			sp.CurrentSession.OnMusicDelivery += HandleOnMusicDelivery;
			sp.CurrentSession.OnEndOfTrack += HandleOnEndOfTrack;
			sp.CurrentSession.OnException += HandleOnException;
			sp.CurrentSession.OnStreamingError += HandleOnStreamingError;
		}
		void initPlayer ()
		{
			// init audio session
			AudioSession.Initialize ();
			AudioSession.Category = AudioSessionCategory.MediaPlayback;
			
			// create player
			player = new SPPlayeriOS();
		}
		
		public void Login(string user, string pass, bool saveIfSuccess = false)
		{
			// login
			sp.LogIn(user, pass);
		}
		
		public void Logout (bool forget = true)
		{
			sp.LogOut(forget);
		}
		
		void HandleOnStreamingError (SPSession sender, SessionEventArgs e)
		{
			Console.WriteLine("SPOTIFY: STREAMING ERROR!");
			Console.WriteLine(e.ToString());
			// do something with error
			// ...
		}
		
		void HandleOnException (SPSession sender, SessionEventArgs e)
		{
			Console.WriteLine("SPOTIFY: EXCEPTION!");
			Console.WriteLine(e.Message);
			// do something with exception
			// ...
		}
		
		public void Search(string query)
		{
			Console.WriteLine("SPOTIFY: searching for "+query);
			
			// search for tracks only
			sp.CurrentSession.Search(query, 0, 50, 0, 0, 0, 0, 0, 0, sp_search_type.STANDARD, null);
		}

		/*
		 * Event handlers 
		 */
		void HandleOnLoginComplete (SPSession sender, SessionEventArgs e)
		{
			Console.WriteLine ("Login result: " + e.Status);
			var error = "";
			switch (e.Status) {
			case sp_error.USER_NEEDS_PREMIUM:
				error = "You need to have premium account! Please, buy one or use another network.";
				break;
			case sp_error.USER_BANNED:
				error = "Looks like you are banned! Please, contact Spotify.";
				break;
			default:
				error = "Couldn't log in! Try again?";
				break;
			}

			// if login was successful, find some music to play
			if (e.Status == sp_error.OK) {
				// search
				Search ("Muse Bliss");
			} else {
				// report error
				Console.WriteLine(error);
			}
		}
		
		void HandleOnSearchComplete (SPSession sender, SearchEventArgs e)
		{
			Console.WriteLine ("SPOTIFY: Search returned:{0}{1}", Environment.NewLine, e.Result);

		    if (e.Result.Tracks.Length <= 0) return;
		    // load first track and play
		    sp.CurrentSession.PlayerLoad (e.Result.Tracks [0]);
		    sp.CurrentSession.PlayerPlay (true);
		    // trigger playback in player
		    player.Play ();
		}
		
		void HandleOnMusicDelivery(SPSession sender, MusicDeliveryEventArgs e)
		{
			if(e.Samples.Length > 0)
			{
				if (!player.AudioDescription.SampleRate.Equals (e.Rate) || player.AudioDescription.ChannelsPerFrame != e.Channels) {
					var desc = new MonoTouch.AudioToolbox.AudioStreamBasicDescription {
						BitsPerChannel = 16,
						FramesPerPacket = 1,
						Reserved = 0,
						FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
						Format = AudioFormatType.LinearPCM,
						SampleRate = e.Rate,
						BytesPerFrame = e.Channels*sizeof (Int16)
					};
					desc.BytesPerPacket = desc.BytesPerFrame;
					desc.ChannelsPerFrame = e.Channels;
					player.AudioDescription = desc;
				}
				
				// Don't forget to set how many frames we consumed
				e.ConsumedFrames = player.EnqueFrames(e.Channels, e.Rate, e.Samples, e.Frames);
			}
			else
			{
				e.ConsumedFrames = 0;
			}
		}
		
		void HandleOnEndOfTrack(SPSession sender, SessionEventArgs e)
		{
			// stop playing & unload track
			sp.CurrentSession.PlayerPlay(false);
			sp.CurrentSession.PlayerUnload();

			// Samples left in player buffer. Player lags 1000 ms
			Thread.Sleep(1100); 
			// stop player, flush buffer & close
			player.Stop();
			player.FlushAndClose();
			// playback done
			Console.WriteLine("Playback complete");
			// do something with this
			// ...
		}
		
		void HandleOnPlayTokenLost(SPSession sender, SessionEventArgs e)
		{
			Console.Out.WriteLine("Play token lost");
			//playbackDone.Set();
		}
		
		void HandleOnMessageToUser(SPSession sender, SessionEventArgs e)
		{			
			Console.WriteLine("Message: " + e.Message);
		}
		
		void HandleOnLogMessage(SPSession sender, SessionEventArgs e)
		{
			Console.WriteLine("Log: " + e.Message);
		}
		
		void HandleOnLoggedOut(SPSession sender, SessionEventArgs e)
		{	
			Console.WriteLine("Logged out from Spotify");
		}
		
		void HandleOnConnectionError(SPSession sender, SessionEventArgs e)
		{
			Console.WriteLine("Connection error: " + e.Status);
		}
	}
}

