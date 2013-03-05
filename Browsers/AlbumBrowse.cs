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
using System.Text;

using MonoLibSpotify.Models;

namespace MonoLibSpotify.Browsers
{
	public class AlbumBrowse
	{
		#region Declarations
		private sp_error error;
		private SPAlbum album;
		private SPArtist artist;
		private string[] copyrights;
		private SPTrack[] tracks;
		private string review;
		#endregion
		
		#region Ctor
		internal AlbumBrowse(IntPtr albumBrowsePtr)
		{
			if(albumBrowsePtr == IntPtr.Zero)
				throw new ArgumentException("albumBrowsePtr can not be zero");
			
			lock(LibspotifyWrapper.Mutex)
			{
				IntPtr strPtr = IntPtr.Zero;
				
				error = LibspotifyWrapper.Browse.Album.Error(albumBrowsePtr);
				album = new SPAlbum(LibspotifyWrapper.Browse.Album.GetAlbum(albumBrowsePtr));
				artist = new SPArtist(LibspotifyWrapper.Browse.Album.Artist(albumBrowsePtr));
				
				copyrights = new string[LibspotifyWrapper.Browse.Album.ArtistNumCopyrights(albumBrowsePtr)];
				for(int i = 0; i < copyrights.Length; i++)
				{
					strPtr = LibspotifyWrapper.Browse.Album.Copyright(albumBrowsePtr, i);
					copyrights[i] = LibspotifyWrapper.GetString(strPtr, string.Empty);
				}
				
				tracks = new SPTrack[LibspotifyWrapper.Browse.Album.NumTracks(albumBrowsePtr)];
				for(int i = 0; i < tracks.Length; i++)
				{
					IntPtr trackPtr = LibspotifyWrapper.Browse.Album.Track(albumBrowsePtr, i);
					tracks[i] = new SPTrack(trackPtr);
				}
				
				strPtr = LibspotifyWrapper.Browse.Album.Review(albumBrowsePtr);
				review = LibspotifyWrapper.GetString(strPtr, string.Empty);
				
				LibspotifyWrapper.Browse.Album.Release(albumBrowsePtr);
			}
		}
		#endregion
		
		#region Properties
		public sp_error Error
		{
			get
			{
				return error;	
			}
		}
		
		public SPAlbum Album
		{
			get
			{
				return album;	
			}
		}
		
		public SPArtist Artist
		{
			get
			{
				return artist;	
			}
		}
		
		public SPTrack[] Tracks
		{
			get
			{
				return tracks;
			}
		}
		
		public string[] Copyrights
		{
			get
			{
				return copyrights;
			}
		}
		
		public string Review
		{
			get
			{
				return review;
			}
		}
		#endregion	
		
		#region Public methods
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("[AlbumBrowse]");
			sb.AppendLine("Error=" + Error);
			sb.AppendLine(Album.ToString());
			sb.AppendLine(Artist.ToString());			
			sb.AppendLine("Copyrights=" + string.Join(",", Copyrights));
			sb.AppendLine("Tracks.Length=" + Tracks.Length);
			foreach(SPTrack t in Tracks)
				sb.AppendLine(t.ToString());
			
			sb.AppendFormat("Review:{0}{1}", Environment.NewLine, Review);
			
			return sb.ToString();
		}
		#endregion
		
	}
}

