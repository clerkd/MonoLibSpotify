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
using System.Text;

using MonoLibSpotify.Models;

namespace MonoLibSpotify.Browsers
{
	public class AlbumBrowse
	{
		#region Declarations
		private sp_error error;

	    #endregion
		
		#region Ctor
		internal AlbumBrowse(IntPtr albumBrowsePtr)
		{
			if(albumBrowsePtr == IntPtr.Zero)
				throw new ArgumentException("albumBrowsePtr can not be zero");
			
			lock(LibspotifyWrapper.Mutex)
			{
				var strPtr = IntPtr.Zero;
				
				error = LibspotifyWrapper.Browse.Album.Error(albumBrowsePtr);
				Album = new SPAlbum(LibspotifyWrapper.Browse.Album.GetAlbum(albumBrowsePtr));
				Artist = new SPArtist(LibspotifyWrapper.Browse.Album.Artist(albumBrowsePtr));
				
				Copyrights = new string[LibspotifyWrapper.Browse.Album.ArtistNumCopyrights(albumBrowsePtr)];
				for(var i = 0; i < Copyrights.Length; i++)
				{
					strPtr = LibspotifyWrapper.Browse.Album.Copyright(albumBrowsePtr, i);
					Copyrights[i] = LibspotifyWrapper.GetString(strPtr, string.Empty);
				}
				
				Tracks = new SPTrack[LibspotifyWrapper.Browse.Album.NumTracks(albumBrowsePtr)];
				for(var i = 0; i < Tracks.Length; i++)
				{
					var trackPtr = LibspotifyWrapper.Browse.Album.Track(albumBrowsePtr, i);
					Tracks[i] = new SPTrack(trackPtr);
				}
				
				strPtr = LibspotifyWrapper.Browse.Album.Review(albumBrowsePtr);
				Review = LibspotifyWrapper.GetString(strPtr, string.Empty);
				
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

	    public SPAlbum Album { get; private set; }

	    public SPArtist Artist { get; private set; }

	    public SPTrack[] Tracks { get; private set; }

	    public string[] Copyrights { get; private set; }

	    public string Review { get; private set; }

	    #endregion	
		
		#region Public methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("[AlbumBrowse]");
			sb.AppendLine("Error=" + Error);
			sb.AppendLine(Album.ToString());
			sb.AppendLine(Artist.ToString());			
			sb.AppendLine("Copyrights=" + string.Join(",", Copyrights));
			sb.AppendLine("Tracks.Length=" + Tracks.Length);
			foreach(var t in Tracks)
				sb.AppendLine(t.ToString());
			
			sb.AppendFormat("Review:{0}{1}", Environment.NewLine, Review);
			
			return sb.ToString();
		}
		#endregion
		
	}
}

