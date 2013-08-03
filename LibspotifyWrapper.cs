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
using System.Runtime.InteropServices;

using MonoLibSpotify.Models;

namespace MonoLibSpotify
{
	internal static class LibspotifyWrapper
	{
		#region internal class vars
		// spotify api version
		internal static int API_VERSION = 12;

		// mutex
		internal static object Mutex = new object();
		#endregion

		#region Misc info
		[DllImport ("__Internal",EntryPoint="sp_build_id")]
		internal static extern IntPtr SpotifyBuildId ();
		#endregion

		#region Sessions
		internal static class Session {
			[DllImport ("__Internal",EntryPoint="sp_session_create")]
			internal static extern sp_error Create(ref sp_session_config config, out IntPtr sess);
			
			[DllImport ("__Internal",EntryPoint="sp_session_login")]
			internal static extern sp_error Login(IntPtr session, string username, string password, bool remember_me, string blob);

			[DllImport ("__Internal",EntryPoint="sp_session_relogin")]
			internal static extern sp_error ReLogin(IntPtr session);
			
			[DllImport ("__Internal",EntryPoint="sp_session_logout")]
			internal static extern sp_error Logout(IntPtr sessionPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_session_forget_me")]
			internal static extern sp_error ForgetMe(IntPtr sessionPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_session_userdata")]
			internal static extern IntPtr User(IntPtr sessionPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_session_connectionstate")]
			internal static extern sp_connectionstate ConnectionState(IntPtr sessionPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_session_process_events")]
			internal static extern sp_error ProcessEvents(IntPtr session, out int next_timeout);

			[DllImport ("__Internal",EntryPoint="sp_session_preferred_bitrate")]
			internal static extern sp_error PreferredBitrate(IntPtr session, sp_bitrate bitrate);
		}
		#endregion

		#region User handling
		internal static class User {
			[DllImport ("__Internal",EntryPoint="sp_session_user_name")]
			internal static extern IntPtr SessionName(IntPtr session);

			[DllImport ("__Internal",EntryPoint="sp_user_canonical_name")]
			internal static extern IntPtr CanonicalName(IntPtr userPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_user_display_name")]
			internal static extern IntPtr DisplayName(IntPtr userPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_user_is_loaded")]
			internal static extern bool IsLoaded(IntPtr userPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_user_add_ref")]
			internal static extern sp_error AddRef(IntPtr userPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_user_release")]
			internal static extern sp_error Release(IntPtr userPtr);
		}
		#endregion	

		#region Search subsystem
		internal static class Search {
			[DllImport ("__Internal",EntryPoint="sp_search_create")]
			internal static extern IntPtr Create(IntPtr sessionPtr, string query, int track_offset, int track_count, int album_offset, int album_count, 
			                                           int artist_offset, int artist_count, int playlist_offset, int playlist_count, sp_search_type search_type,
			                                           IntPtr callbackPtr, IntPtr userDataPtr);

			[DllImport ("__Internal",EntryPoint="sp_search_is_loaded")]
			internal static extern bool IsLoaded(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_error")]
			internal static extern sp_error Error(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_num_tracks")]
			internal static extern int NumTracks(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_track")]
			internal static extern IntPtr Track(IntPtr searchPtr, int index);
			
			[DllImport ("__Internal",EntryPoint="sp_search_num_albums")]
			internal static extern int NumAlbums(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_album")]
			internal static extern IntPtr Album(IntPtr searchPtr, int index);
			
			[DllImport ("__Internal",EntryPoint="sp_search_num_artists")]
			internal static extern int NumArtists(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_artist")]
			internal static extern IntPtr Artist(IntPtr searchPtr, int index);
			
			[DllImport ("__Internal",EntryPoint="sp_search_query")]
			internal static extern IntPtr Query(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_did_you_mean")]
			internal static extern IntPtr DidYouMean(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_total_tracks")]
			internal static extern int TotalTracks(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_add_ref")]
			internal static extern void AddRef(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_search_release")]
			internal static extern void Release(IntPtr searchPtr);
		}
		#endregion

		#region Images
		internal static class Image {
			[DllImport ("__Internal",EntryPoint="sp_image_create")]
			internal static extern IntPtr Create(IntPtr sessionPtr, IntPtr idPtr);

			[DllImport ("__Internal",EntryPoint="sp_image_add_load_callback")]
			internal static extern void AddLoadCallback(IntPtr imagePtr, IntPtr callbackPtr, IntPtr userDataPtr);

			[DllImport ("__Internal",EntryPoint="sp_image_is_loaded")]
			internal static extern bool IsLoaded(IntPtr imagePtr);

			[DllImport ("__Internal",EntryPoint="sp_image_data")]
			internal static extern IntPtr Data(IntPtr imagePtr, out IntPtr sizePtr);

			[DllImport ("__Internal",EntryPoint="sp_image_image_id")]
			internal static extern IntPtr ID(IntPtr imagePtr);

			[DllImport ("__Internal",EntryPoint="sp_image_release")]
			internal static extern sp_error Release(IntPtr imagePtr);
		}
		#endregion

		#region Links (Spotify URIs)
		internal static class Link {
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_string")]
			internal static extern IntPtr CreateFromString(string link);
			
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_track")]
			internal static extern IntPtr CreateFromTrack(IntPtr trackPtr, int offset);
			
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_album")]
			internal static extern IntPtr CreateFromAlbum(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_artist")]
			internal static extern IntPtr CreateFromArtist(IntPtr artistPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_search")]
			internal static extern IntPtr CreateFromSearch(IntPtr searchPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_create_from_playlist")]
			internal static extern IntPtr CreateFromPlaylist(IntPtr playlistPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_as_string")]
			internal static extern int AsString(IntPtr linkPtr, IntPtr bufferPtr, int buffer_size);
			
			[DllImport ("__Internal",EntryPoint="sp_link_type")]
			internal static extern sp_linktype Type(IntPtr linkPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_as_track")]
			internal static extern IntPtr AsTrack(IntPtr linkPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_as_track_and_offset")]
			internal static extern IntPtr AsTrackAndOffset(IntPtr linkPtr, out IntPtr offsetPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_as_album")]
			internal static extern IntPtr AsAlbum(IntPtr linkPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_as_artist")]
			internal static extern IntPtr AsArtist(IntPtr linkPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_add_ref")]
			internal static extern void AddRef(IntPtr linkPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_link_release")]
			internal static extern void Release(IntPtr linkPtr);
		}
		#endregion

		#region Artist subsystem
		internal static class Artist {
			[DllImport ("__Internal",EntryPoint="sp_artist_name")]
			internal static extern IntPtr Name(IntPtr artistPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_artist_is_loaded")]
			internal static extern bool IsLoaded(IntPtr artistPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_artist_add_ref")]
			internal static extern void AddRef(IntPtr artistPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_artist_release")]
			internal static extern void Release(IntPtr artistPtr);
		}
		#endregion

		#region Album subsystem
		internal static class Album {
			[DllImport ("__Internal",EntryPoint="sp_album_is_loaded")]
			internal static extern bool IsLoaded(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_is_available")]
			internal static extern bool IsAvailable(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_artist")]
			internal static extern IntPtr Artist(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_cover")]
			internal static extern IntPtr Cover(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_name")]
			internal static extern IntPtr Name(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_year")]
			internal static extern int Year(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_add_ref")]
			internal static extern void AddRef(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_release")]
			internal static extern void Release(IntPtr albumPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_album_type")]
			internal static extern sp_albumtype Type(IntPtr albumPtr);
		}
		#endregion

		#region Track subsystem
		internal static class Track {
			[DllImport ("__Internal",EntryPoint="sp_track_is_loaded")]
			internal static extern bool IsLoaded(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_error")]
			internal static extern sp_error Error(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_num_artists")]
			internal static extern int NumArtists(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_artist")]
			internal static extern IntPtr Artist(IntPtr trackPtr, int index);
			
			[DllImport ("__Internal",EntryPoint="sp_track_album")]
			internal static extern IntPtr Album(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_name")]
			internal static extern IntPtr Name(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_duration")]
			internal static extern int Duration(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_popularity")]
			internal static extern int Popularity(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_disc")]
			internal static extern int Disc(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_index")]
			internal static extern int Index(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_add_ref")]
			internal static extern sp_error AddRef(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_release")]
			internal static extern void Release(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_is_starred")]
			internal static extern bool IsStarred(IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_track_set_starred")]
			internal static extern void SetStarred(IntPtr sessionPtr, IntPtr tracksArrayPtr, int num_tracks, bool star);
		}
		#endregion

		#region browsing data
		internal static class Browse {
			#region Artist browsing
			internal static class Artist {
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_create")]
				internal static extern IntPtr Create(IntPtr sessionPtr, IntPtr artistPtr, IntPtr callbackPtr, IntPtr userDataPtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_is_loaded")]
				internal static extern bool	IsLoaded(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_error")]
				internal static extern sp_error Error(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_artist")]
				internal static extern IntPtr GetArtist(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_num_portraits")]
				internal static extern int NumPortraits(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_portrait")]
				internal static extern IntPtr Portrait(IntPtr artistBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_num_tracks")]
				internal static extern int NumTracks(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_track")]
				internal static extern IntPtr Track(IntPtr artistBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_num_albums")]
				internal static extern int NumAlbums(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_album")]
				internal static extern IntPtr Album(IntPtr artistBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_num_similar_artists")]
				internal static extern int NumSimilarArtists(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_similar_artist")]
				internal static extern IntPtr SimilarArtist(IntPtr artistBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_biography")]
				internal static extern IntPtr Biography(IntPtr artistBrowsePtr);		
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_add_ref")]
				internal static extern void AddRef(IntPtr artistBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_artistbrowse_release")]
				internal static extern void Release(IntPtr artistBrowsePtr);	
			}
			#endregion

			#region Album browsing
			internal static class Album {
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_create")]
				internal static extern IntPtr Create(IntPtr sessionPtr, IntPtr albumPtr, IntPtr callbackPtr, IntPtr userDataPtr);

				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_error")]
				internal static extern sp_error Error(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_album")]
				internal static extern IntPtr GetAlbum(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_artist")]
				internal static extern IntPtr Artist(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_num_copyrights")]
				internal static extern int ArtistNumCopyrights(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_copyright")]
				internal static extern IntPtr Copyright(IntPtr albumBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_num_tracks")]
				internal static extern int NumTracks(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_track")]
				internal static extern IntPtr Track(IntPtr albumBrowsePtr, int index);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_review")]
				internal static extern IntPtr Review(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_add_ref")]
				internal static extern void AddRef(IntPtr albumBrowsePtr);
				
				[DllImport ("__Internal",EntryPoint="sp_albumbrowse_release")]
				internal static extern void Release(IntPtr albumBrowsePtr);
			}
			#endregion
		}
		#endregion

		#region Player
		internal static class Player {
			[DllImport ("__Internal",EntryPoint="sp_session_player_load")]
			internal static extern sp_error Load(IntPtr sessionPtr, IntPtr trackPtr);
			
			[DllImport ("__Internal",EntryPoint="sp_session_player_seek")]
			internal static extern sp_error Seek(IntPtr sessionPtr, int offset);
			
			[DllImport ("__Internal",EntryPoint="sp_session_player_play")]
			internal static extern sp_error Play(IntPtr sessionPtr, bool play);
			
			[DllImport ("__Internal",EntryPoint="sp_session_player_unload")]
			internal static extern sp_error Unload(IntPtr sessionPtr);		
		}
		#endregion

		#region helper functions
		internal static string GetString(IntPtr ptr, string defaultValue)
		{
			if (ptr == IntPtr.Zero)
				return defaultValue;
			
			var l = new System.Collections.Generic.List<byte>();            
			byte read = 0;
			do {
				read = Marshal.ReadByte(ptr, l.Count);                
				l.Add(read);
			} while (read != 0);
			
			return l.Count > 0 ? System.Text.Encoding.UTF8.GetString(l.ToArray(), 0, l.Count - 1) : string.Empty;
		}

		internal static string ImageIdToString(byte[] id)
		{
			if(id == null)
				return string.Empty;
			
			var sb = new System.Text.StringBuilder();
			foreach(var b in id)
				sb.Append(b.ToString("X2"));
			
			return sb.ToString();
		}

		internal static string ImageIdToString(IntPtr idPtr)
		{
			if(idPtr == IntPtr.Zero)
				return string.Empty;
			
			var id = new byte[20];
			Marshal.Copy(idPtr, id, 0, 20);
			
			return ImageIdToString(id);
		}

		internal static byte[] StringToImageId(string id)
		{
			if(string.IsNullOrEmpty(id) || id.Length != 40)
				return null;
			try
			{
				var result = new byte[20];
				for(var i = 0; i < 20 ; i++)
				{
					result[i] = byte.Parse(id.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
				}
				
				return result;
			}
            catch (Exception e)
            {
                Console.WriteLine(e);
			}
			return null;
		}
		#endregion
	}
}

