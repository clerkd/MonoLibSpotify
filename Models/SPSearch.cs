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


namespace MonoLibSpotify.Models
{
	public class SPSearch : IDisposable
	{
		#region Declarations
		internal IntPtr searchPtr = IntPtr.Zero;
		
		private sp_error error;
		private SPTrack[] tracks;
		private SPAlbum[] albums;
		private SPArtist[] artists;
		private string query;
		private string didYouMean;
		private int totalTracks;		
		#endregion
		
		#region Ctor
		internal SPSearch(IntPtr searchPtr)
		{
			if (searchPtr == IntPtr.Zero)
				throw new ArgumentException("searchPtr can not be zero");
			
			this.searchPtr = searchPtr;
			
			lock (LibspotifyWrapper.Mutex)
			{
				var strPtr = IntPtr.Zero;
				
				error = LibspotifyWrapper.Search.Error(searchPtr);

				var numTracks = LibspotifyWrapper.Search.NumTracks(searchPtr);
				tracks = new SPTrack[numTracks];
				for (var i = 0; i < tracks.Length; i++)
				{
					var trackPtr = LibspotifyWrapper.Search.Track(searchPtr, i);
					tracks[i] = new SPTrack(trackPtr);
				}

				albums = new SPAlbum[LibspotifyWrapper.Search.NumAlbums(searchPtr)];
				for (var i = 0; i < albums.Length; i++)
				{
					var albumPtr = LibspotifyWrapper.Search.Album(searchPtr, i);
					albums[i] = new SPAlbum(albumPtr);
				}

				artists = new SPArtist[LibspotifyWrapper.Search.NumArtists(searchPtr)];
				for (var i = 0; i < artists.Length; i++)
				{
					var artistPtr = LibspotifyWrapper.Search.Artist(searchPtr, i);
					artists[i] = new SPArtist(artistPtr);
				}

				strPtr = LibspotifyWrapper.Search.Query(searchPtr);
				query = LibspotifyWrapper.GetString(strPtr, string.Empty);

				strPtr = LibspotifyWrapper.Search.DidYouMean(searchPtr);
				didYouMean = LibspotifyWrapper.GetString(strPtr, string.Empty);

				totalTracks = LibspotifyWrapper.Search.TotalTracks(searchPtr);
			}
		}
		#endregion
		
		#region Properties
		public sp_error Error
		{
			get
			{
				CheckDisposed(true);
				return error;
			}
		}
		
		public SPTrack[] Tracks
		{
			get
			{
				CheckDisposed(true);
				return tracks;
			}
		}
		
		public SPAlbum[] Albums
		{
			get
			{
				CheckDisposed(true);
				return albums;
			}
		}
		
		public SPArtist[] Artists
		{
			get
			{
				CheckDisposed(true);
				return artists;
			}
		}
		
		public string Query
		{
			get
			{
				CheckDisposed(true);
				return query;
			}
		}
		
		public string DidYouMean
		{
			get
			{
				CheckDisposed(true);
				return didYouMean;
			}
		}
		
		public int TotalTracks
		{
			get
			{
				CheckDisposed(true);
				return totalTracks;
			}
		}
		#endregion
		
		#region Public methods
		public SPLink CreateLink()
		{
			CheckDisposed(true);
			
			lock(LibspotifyWrapper.Mutex)
			{
			    var linkPtr = LibspotifyWrapper.Link.CreateFromSearch(searchPtr);
			    return linkPtr != IntPtr.Zero ? new SPLink(linkPtr) : null;
			}
		}
		
		public override string ToString()
		{
			CheckDisposed(true);
			
			var sb = new StringBuilder();
			sb.AppendLine("[Search]");
			sb.AppendLine("Error=" + Error);
			sb.AppendLine("Tracks.Length=" + Tracks.Length);
			sb.AppendLine("Albums.Length=" + Albums.Length);
			sb.AppendLine("Artists.Length=" + Artists.Length);
			sb.AppendLine("Query=" + Query);
			sb.AppendLine("DidYouMean=" + DidYouMean);
			sb.AppendLine("TotalTracks=" + TotalTracks);            
			
			return sb.ToString();
		}
		#endregion
		
		#region Cleanup
		~SPSearch()
		{
			Dispose(false);
		}
		
		protected void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					
				}

			    if (searchPtr == IntPtr.Zero) return;
			    LibspotifyWrapper.Search.Release(searchPtr);
			    searchPtr = IntPtr.Zero;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
		
		public void Dispose()
		{
			if(searchPtr == IntPtr.Zero)
				return;
			
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		
		private bool CheckDisposed(bool throwOnDisposed)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var result = searchPtr == IntPtr.Zero;
				if(result && throwOnDisposed)
					throw new ObjectDisposedException("Search");
				
				return result;
			}
		}
		#endregion
		
	}
}

