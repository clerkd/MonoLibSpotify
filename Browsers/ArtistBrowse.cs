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
	    private List<string> portraitIds;
	    private SPAlbum[] albums;

	    #endregion
		
		#region Ctor
		internal ArtistBrowse(IntPtr artistBrowsePtr)
		{
			if(artistBrowsePtr == IntPtr.Zero)
				throw new ArgumentException("artistBrowsePtr can not be zero");
			
			lock(LibspotifyWrapper.Mutex)
			{
				var strPtr = IntPtr.Zero;
				
				error = LibspotifyWrapper.Browse.Artist.Error(artistBrowsePtr);
				Artist = new SPArtist(LibspotifyWrapper.Browse.Artist.GetArtist(artistBrowsePtr));
				
				portraitIds = new List<string>(LibspotifyWrapper.Browse.Artist.NumPortraits(artistBrowsePtr));
				for(var i = 0; i < portraitIds.Count; i++)
				{
					var portraitIdPtr = LibspotifyWrapper.Browse.Artist.Portrait(artistBrowsePtr, i);
					var portraitId = new byte[20];
					Marshal.Copy(portraitIdPtr, portraitId, 0, portraitId.Length);
					portraitIds.Add(LibspotifyWrapper.ImageIdToString(portraitId));
				}
				
				Tracks = new SPTrack[LibspotifyWrapper.Browse.Artist.NumTracks(artistBrowsePtr)];
				for(var i = 0; i < Tracks.Length; i++)
				{
					var trackPtr = LibspotifyWrapper.Browse.Artist.Track(artistBrowsePtr, i);
					Tracks[i] = new SPTrack(trackPtr);
				}
				
				albums = new SPAlbum[LibspotifyWrapper.Browse.Artist.NumAlbums(artistBrowsePtr)];
				for(var i = 0; i < albums.Length; i++)
				{
					var albumPtr = LibspotifyWrapper.Browse.Artist.Album(artistBrowsePtr, i);
					albums[i] = new SPAlbum(albumPtr);
				}
				
				SimilarArtists = new SPArtist[LibspotifyWrapper.Browse.Artist.NumSimilarArtists(artistBrowsePtr)];
				for(var i = 0; i < SimilarArtists.Length; i++)
				{
					var artistPtr = LibspotifyWrapper.Browse.Artist.SimilarArtist(artistBrowsePtr, i);
					SimilarArtists[i] = new SPArtist(artistPtr);
				}
				
				strPtr = LibspotifyWrapper.Browse.Album.Review(artistBrowsePtr);
				Biography = LibspotifyWrapper.GetString(strPtr, string.Empty);
				
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

	    public SPArtist Artist { get; private set; }

	    public string[] PortraitIds
		{
			get
			{
				return portraitIds.ToArray();
			}
		}

	    public SPTrack[] Tracks { get; private set; }

	    public SPArtist[] SimilarArtists { get; private set; }

	    public string Biography { get; private set; }

	    #endregion	
		
		#region Public methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("[ArtistBrowse]");
			sb.AppendLine("Error=" + Error);			
			sb.AppendLine(Artist.ToString());			
			sb.AppendLine("PortraitsIds.Length=" + PortraitIds.Length);			
			foreach(var portraitId in PortraitIds)
				sb.AppendLine(portraitId);		 
			
			sb.AppendLine("Tracks.Length=" + Tracks.Length);
			foreach(var t in Tracks)
				sb.AppendLine(t.ToString());			
			sb.AppendLine("SimilarArtists.Length=" + SimilarArtists.Length);
			foreach(var a in SimilarArtists)
				sb.AppendLine(a.ToString());
			
			sb.AppendFormat("Biography:{0}{1}", Environment.NewLine, Biography);
			
			return sb.ToString();
		}
		#endregion
		
	}
}

