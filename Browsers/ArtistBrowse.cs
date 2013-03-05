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
using System.Collections.Generic;
using System.Text;

using MonoLibSpotify.Models;

namespace MonoLibSpotify.Browsers
{
	public class ArtistBrowse
	{
		#region Declarations
		private sp_error error;
		private SPArtist artist;		
		private List<string> portraitIds;
		private SPTrack[] tracks;
		private SPAlbum[] albums;
		private SPArtist[] similarArtists;
		private string biography;		
		#endregion
		
		#region Ctor
		internal ArtistBrowse(IntPtr artistBrowsePtr)
		{
			if(artistBrowsePtr == IntPtr.Zero)
				throw new ArgumentException("artistBrowsePtr can not be zero");
			
			lock(LibspotifyWrapper.Mutex)
			{
				IntPtr strPtr = IntPtr.Zero;
				
				error = LibspotifyWrapper.Browse.Artist.Error(artistBrowsePtr);
				artist = new SPArtist(LibspotifyWrapper.Browse.Artist.GetArtist(artistBrowsePtr));
				
				portraitIds = new List<string>(LibspotifyWrapper.Browse.Artist.NumPortraits(artistBrowsePtr));
				for(int i = 0; i < portraitIds.Count; i++)
				{
					IntPtr portraitIdPtr = LibspotifyWrapper.Browse.Artist.Portrait(artistBrowsePtr, i);
					byte[] portraitId = new byte[20];
					Marshal.Copy(portraitIdPtr, portraitId, 0, portraitId.Length);
					portraitIds.Add(LibspotifyWrapper.ImageIdToString(portraitId));
				}
				
				tracks = new SPTrack[LibspotifyWrapper.Browse.Artist.NumTracks(artistBrowsePtr)];
				for(int i = 0; i < tracks.Length; i++)
				{
					IntPtr trackPtr = LibspotifyWrapper.Browse.Artist.Track(artistBrowsePtr, i);
					tracks[i] = new SPTrack(trackPtr);
				}
				
				albums = new SPAlbum[LibspotifyWrapper.Browse.Artist.NumAlbums(artistBrowsePtr)];
				for(int i = 0; i < albums.Length; i++)
				{
					IntPtr albumPtr = LibspotifyWrapper.Browse.Artist.Album(artistBrowsePtr, i);
					albums[i] = new SPAlbum(albumPtr);
				}
				
				similarArtists = new SPArtist[LibspotifyWrapper.Browse.Artist.NumSimilarArtists(artistBrowsePtr)];
				for(int i = 0; i < similarArtists.Length; i++)
				{
					IntPtr artistPtr = LibspotifyWrapper.Browse.Artist.SimilarArtist(artistBrowsePtr, i);
					similarArtists[i] = new SPArtist(artistPtr);
				}
				
				strPtr = LibspotifyWrapper.Browse.Album.Review(artistBrowsePtr);
				biography = LibspotifyWrapper.GetString(strPtr, string.Empty);
				
				LibspotifyWrapper.Browse.Artist.Release(artistBrowsePtr);
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
		
		public SPArtist Artist
		{
			get
			{
				return artist;	
			}
		}
		
		public string[] PortraitIds
		{
			get
			{
				return portraitIds.ToArray();
			}
		}
		
		public SPTrack[] Tracks
		{
			get
			{
				return tracks;
			}
		}
		
		public SPArtist[] SimilarArtists
		{
			get
			{
				return similarArtists;
			}
		}		
		
		public string Biography
		{
			get
			{
				return biography;
			}
		}
		#endregion	
		
		#region Public methods
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("[ArtistBrowse]");
			sb.AppendLine("Error=" + Error);			
			sb.AppendLine(Artist.ToString());			
			sb.AppendLine("PortraitsIds.Length=" + PortraitIds.Length);			
			foreach(string portraitId in PortraitIds)
				sb.AppendLine(portraitId);		 
			
			sb.AppendLine("Tracks.Length=" + Tracks.Length);
			foreach(SPTrack t in Tracks)
				sb.AppendLine(t.ToString());			
			sb.AppendLine("SimilarArtists.Length=" + SimilarArtists.Length);
			foreach(SPArtist a in SimilarArtists)
				sb.AppendLine(a.ToString());
			
			sb.AppendFormat("Biography:{0}{1}", Environment.NewLine, Biography);
			
			return sb.ToString();
		}
		#endregion
		
	}
}

