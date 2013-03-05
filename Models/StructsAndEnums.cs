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

namespace MonoLibSpotify.Models
{
	public enum SPAsyncLoadingPolicy 
	{
		Immediate = 0, /* Immediately load items on login. */
		Manual /* Only load items when -startLoading is called. */
	}

	internal enum SPTestState 
	{
		Waiting,
		Running,
		Passed,
		Failed
	}

	public enum sp_connectionstate 
	{
		LOGGED_OUT   = 0, ///< User not yet logged in
		LOGGED_IN    = 1, ///< Logged in against a Spotify access point
		DISCONNECTED = 2, ///< Was logged in, but has now been disconnected
		UNDEFINED    = 3, ///< The connection state is undefined
		OFFLINE		 = 4  ///< Logged in in offline mode
	}

	internal struct sp_audioformat
	{
		internal int sample_type;
		internal int sample_rate;
		internal int channels;
	}
	
	public enum sp_bitrate {
		SP_BITRATE_160k      = 0, ///< Bitrate 160kbps
		SP_BITRATE_320k      = 1, ///< Bitrate 320kbps
		SP_BITRATE_96k       = 2, ///< Bitrate 96kbps
	};

	public enum sp_search_type 
	{
		STANDARD  = 0,
		SUGGEST = 1,
	}

	public enum sp_linktype
	{
		INVALID = 0,
		TRACK = 1,
		ALBUM = 2,
		ARTIST = 3,
		SEARCH = 4,
		PLAYLIST = 5
	}

	public enum sp_albumtype
	{
		ALBUM = 0,
		SINGLE = 1,
		COMPILATION = 2,
		UNKNOWN = 3
	}

	public enum sp_playlist_offline_status 
	{
		NO          = 0, ///< Playlist is not offline enabled
		YES         = 1, ///< Playlist is synchronized to local storage
		DOWNLOADING = 2, ///< This playlist is currently downloading. Only one playlist can be in this state any given time
		WAITING     = 3, ///< Playlist is queued for download
	}

	public enum sp_error {
		OK                        = 0,  ///< No errors encountered
		BAD_API_VERSION           = 1,  ///< The library version targeted does not match the one you claim you support
		API_INITIALIZATION_FAILED = 2,  ///< Initialization of library failed - are cache locations etc. valid?
		TRACK_NOT_PLAYABLE        = 3,  ///< The track specified for playing cannot be played
		RESOURCE_NOT_LOADED       = 4,  ///< Resource not loaded
		BAD_APPLICATION_KEY       = 5,  ///< The application key is invalid
		BAD_USERNAME_OR_PASSWORD  = 6,  ///< Login failed because of bad username and/or password
		USER_BANNED               = 7,  ///< The specified username is banned
		UNABLE_TO_CONTACT_SERVER  = 8,  ///< Cannot connect to the Spotify backend system
		CLIENT_TOO_OLD            = 9,  ///< Client is too old, library will need to be updated
		OTHER_PERMANENT           = 10, ///< Some other error occurred, and it is permanent (e.g. trying to relogin will not help)
		BAD_USER_AGENT            = 11, ///< The user agent string is invalid or too long
		MISSING_CALLBACK          = 12, ///< No valid callback registered to handle events
		INVALID_INDATA            = 13, ///< Input data was either missing or invalid
		INDEX_OUT_OF_RANGE        = 14, ///< Index out of range
		USER_NEEDS_PREMIUM        = 15, ///< The specified user needs a premium account
		OTHER_TRANSIENT           = 16, ///< A transient error occurred.
		IS_LOADING                = 17, ///< The resource is currently loading
		NO_STREAM_AVAILABLE       = 18, ///< Could not find any suitable stream to play
		PERMISSION_DENIED         = 19, ///< Requested operation is not allowed
		INBOX_IS_FULL             = 20, ///< Target inbox is full
		NO_CACHE                  = 21, ///< Cache is not enabled
		NO_SUCH_USER              = 22, ///< Requested user does not exist
		NO_CREDENTIALS            = 23, ///< No credentials are stored
		NETWORK_DISABLED          = 24, ///< Network disabled
		INVALID_DEVICE_ID         = 25, ///< Invalid device ID
		CANT_OPEN_TRACE_FILE      = 26, ///< Unable to open trace file
		APPLICATION_BANNED        = 27, ///< This application is no longer allowed to use the Spotify service
		OFFLINE_TOO_MANY_TRACKS   = 31, ///< Reached the device limit for number of tracks to download
		OFFLINE_DISK_CACHE        = 32, ///< Disk cache is full so no more tracks can be downloaded to offline mode
		OFFLINE_EXPIRED           = 33, ///< Offline key has expired, the user needs to go online again
		OFFLINE_NOT_ALLOWED       = 34, ///< This user is not allowed to use offline mode
		OFFLINE_LICENSE_LOST      = 35, ///< The license for this device has been lost. Most likely because the user used offline on three other device
		OFFLINE_LICENSE_ERROR     = 36, ///< The Spotify license server does not respond correctly
		SIGNUP_LOGIN              = 37,
		SIGNUP_ACCOUNT_MERGE_FAIL = 38,
		LASTFM_AUTH_ERROR         = 39, ///< A LastFM scrobble authentication error has occurred
		INVALID_ARGUMENT          = 40, ///< An invalid argument was specified
		SYSTEM_FAILURE            = 41, ///< An operating system error
	};

	internal struct sp_session_callbacks {
		internal IntPtr logged_in;//(sp_session session, sp_error error);
		internal IntPtr logged_out;//(sp_session session);
		internal IntPtr metadata_updated;//(sp_session session);
		internal IntPtr connection_error;//(sp_session session, sp_error error);
		internal IntPtr message_to_user;//(sp_session session, string message);
		internal IntPtr notify_main_thread;//(sp_session session);
		internal IntPtr music_delivery;//(sp_session session, sp_audioformat format, out int frames, int num_frames);
		internal IntPtr play_token_lost;//(sp_session session);
		internal IntPtr log_message;//(sp_session session, string data);
		internal IntPtr end_of_track;//(sp_session session);
		internal IntPtr streaming_error;//(sp_session session, sp_error error);
		internal IntPtr userinfo_updated;//(sp_session session);
		internal IntPtr start_playback;//(sp_session session);
		internal IntPtr stop_playback;//(sp_session session);
		internal IntPtr get_audio_buffer_stats;//(sp_session session, sp_audio_buffer_stats stats);
		internal IntPtr offline_status_updated;//(sp_session session);
		internal IntPtr offline_error;//(sp_session session, sp_error error);
		internal IntPtr credentials_blob_updated;//(sp_session session, string blob);
		internal IntPtr connectionstate_updated;//(sp_session session);
		
		internal IntPtr show_signup_page;//(sp_session session, sp_signup_page page, bool pageIsLoading, int featureMask, string recentUserName);
		internal IntPtr show_signup_error_page;//(sp_session session, sp_signup_page page, sp_error error);
		internal IntPtr connect_to_facebook;//(sp_session session, out string permissions, int permission_count);
		internal IntPtr show_trial_welcome;//(sp_session session, uint trial_duration);
		internal IntPtr scrobble_error;//(sp_session session, sp_error error);
		internal IntPtr private_session_mode_changed;//(sp_session session, bool is_private);
	};

	internal struct sp_session {}; ///< Representation of a session

	internal struct sp_session_config {
		internal int api_version;
		internal string cache_location;
		internal string settings_location;
		internal IntPtr application_key;
		internal int application_key_size;
		internal string user_agent;
		internal IntPtr callbacks;
		internal IntPtr userdata;
		internal bool compress_playlists;
		internal bool dont_save_metadata_for_playlists;
		internal bool initially_unload_playlists;
		internal string device_id;
		internal string proxy;
		internal string proxy_username;
		internal string proxy_password;
		internal string tracefile;
	};

	internal struct sp_playlist_callbacks {
		internal IntPtr tracks_added;
		internal IntPtr tracks_removed;
		internal IntPtr tracks_moved;
		internal IntPtr track_created_changed;
		internal IntPtr track_seen_changed;
		internal IntPtr track_message_changed;
		internal IntPtr playlist_renamed;
		internal IntPtr playlist_state_changed;
		internal IntPtr playlist_update_in_progress;
		internal IntPtr playlist_metadata_updated;
		internal IntPtr description_changed;
		internal IntPtr image_changed;
		internal IntPtr subscribers_changed;
	};
}

