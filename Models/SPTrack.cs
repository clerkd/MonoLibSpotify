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

namespace MonoLibSpotify.Models
{
	public class SPTrack
	{
		#region Static methods
		public static SPTrack CreateFromLink(SPLink link)
		{
			SPTrack result = null;
			
			if(link.linkPtr != IntPtr.Zero)
			{
				lock(LibspotifyWrapper.Mutex)
				{
					IntPtr trackPtr = LibspotifyWrapper.Link.AsTrack(link.linkPtr);
					if(trackPtr != IntPtr.Zero)
						result = new SPTrack(trackPtr);
				}
			}
			
			return result;
		}
		
		public static SPTrack CreateFromLink(SPLink link, out int offset)
		{
			SPTrack result = null;
			offset = 0;
			
			if(link.linkPtr != IntPtr.Zero)
			{
				IntPtr offsetPtr = IntPtr.Zero;
				
				lock(LibspotifyWrapper.Mutex)
				{
					IntPtr trackPtr = LibspotifyWrapper.Link.AsTrackAndOffset(link.linkPtr, out offsetPtr);
					if(trackPtr != IntPtr.Zero)
						result = new SPTrack(trackPtr);
				}
				
				offset = offsetPtr.ToInt32();
			}
			
			return result;
		}
		#endregion
		
		#region Declarations
		internal IntPtr trackPtr = IntPtr.Zero;
		
		private bool isLoaded = false;
		private sp_error error = sp_error.RESOURCE_NOT_LOADED;
		private SPAlbum album = null;
		private SPArtist[] artists = null;
		private string name = string.Empty;
		private int duration = 0;
		private int popularity = 0;
		private int disc = 0;
		private int index = 0;
		private string linkString = string.Empty;
		#endregion
		
		#region Ctor
		internal SPTrack(IntPtr trackPtr)
		{
			if(trackPtr == IntPtr.Zero)
				throw new ArgumentException("trackPtr can not be zero");
			
			this.trackPtr = trackPtr;			
			
			lock(LibspotifyWrapper.Mutex)
				LibspotifyWrapper.Track.AddRef(trackPtr);
			
			CheckLoaded();
		}
		#endregion
		
		#region Internal methods
		internal void CheckLoaded()
		{
			CheckDisposed(true);

			if(isLoaded)
				return;

			lock(LibspotifyWrapper.Mutex)
				isLoaded = LibspotifyWrapper.Track.IsLoaded(trackPtr);

			if(!isLoaded)
				return;

			lock(LibspotifyWrapper.Mutex)
			{
				error = LibspotifyWrapper.Track.Error(trackPtr);
				IntPtr albumPtr = LibspotifyWrapper.Track.Album(trackPtr);
				if (albumPtr != IntPtr.Zero)
					album = new SPAlbum(albumPtr);
				
				artists = new SPArtist[LibspotifyWrapper.Track.NumArtists(trackPtr)];
				for(int i = 0; i < artists.Length; i++)
					artists[i] = new SPArtist(LibspotifyWrapper.Track.Artist(trackPtr, i));
				
				name = LibspotifyWrapper.GetString(LibspotifyWrapper.Track.Name(trackPtr), string.Empty);
				
				duration = LibspotifyWrapper.Track.Duration(trackPtr);
				popularity = LibspotifyWrapper.Track.Popularity(trackPtr);
				disc = LibspotifyWrapper.Track.Disc(trackPtr);
				index = LibspotifyWrapper.Track.Index(trackPtr);
				
				using(SPLink l = CreateLink(0))
				{
					linkString = l.ToString();	
				}				
			}
		}
		#endregion
		
		#region Properties
		public bool IsLoaded
		{
			get
			{
				CheckLoaded();
				return isLoaded;
			}
		}
		
		public bool IsStarred
		{
			get
			{
				CheckLoaded();
				lock (LibspotifyWrapper.Mutex)
				{
					bool result = LibspotifyWrapper.Track.IsStarred(trackPtr);
					return result;
				}
			}
		}
		
		public sp_error Error
		{
			get
			{
				CheckLoaded();				
				return error;
			}
		}
		
		
		public SPAlbum Album
		{
			get
			{
				CheckLoaded();				
				return album;
			}
		}
		
		public SPArtist[] Artists
		{
			get
			{
				CheckLoaded();				
				return artists;
			}
		}	
		
		public string Name
		{
			get
			{
				CheckLoaded();				
				return name;
			}
		}
		
		public int Duration
		{
			get
			{
				CheckLoaded();				
				return duration;
			}
		}
		
		public int Popularity
		{
			get
			{
				CheckLoaded();				
				return popularity;
			}
		}
		
		public int Disc
		{
			get
			{
				CheckLoaded();				
				return disc;
			}
		}
		
		public int Index
		{
			get
			{
				CheckLoaded();				
				return index;
			}
		}
		
		public string LinkString
		{
			get
			{
				CheckLoaded();				
				return linkString;
			}
		}
		#endregion
		
		#region Public methods
		public SPLink CreateLink(int offset)
		{
			CheckDisposed(true);
			
			lock(LibspotifyWrapper.Mutex)
			{
				IntPtr linkPtr = LibspotifyWrapper.Link.CreateFromTrack(trackPtr, offset);
				if(linkPtr != IntPtr.Zero)
					return new SPLink(linkPtr);
				else
					return null;
			}
		}
		
		public void SetStarred(SPSession session, bool starred)
		{
			CheckLoaded();
			if (!isLoaded)
				return;
			
			IntPtr arrayPtr = IntPtr.Zero;
			
			try
			{
				int[] array = new int[] { trackPtr.ToInt32() };
				int size = Marshal.SizeOf(arrayPtr) * array.Length;
				arrayPtr = Marshal.AllocHGlobal(size);
				Marshal.Copy(array, 0, arrayPtr, array.Length);
				LibspotifyWrapper.Track.SetStarred(session.SessionPointer, arrayPtr, 1, starred);
			}
			finally
			{
				if (arrayPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(arrayPtr);
				}
			}
		}
		
		public override string ToString ()
		{
			if(IsLoaded)			
			{
				return string.Format("[Track: Error={0}, Album.Name={1}, Artists={2}, Name={3}, Duration={4}, Popularity={5}, Disc={6}, Index={7}, LinkString={8} IsStarred={9}]",
				                     Error, Album == null ? "null" : Album.Name, SPArtist.ArtistsToString(Artists), Name, Duration, Popularity, Disc, Index, LinkString, IsStarred);
			}
			else
				return "[Track: Not loaded]";
		}	
		#endregion
		
		#region Cleanup
		~SPTrack()
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
				
				if(trackPtr != IntPtr.Zero)
				{
					LibspotifyWrapper.Track.Release(trackPtr);
					trackPtr = IntPtr.Zero;
				}			
			}
			catch
			{
				
			}
		}		
		
		public void Dispose()
		{
			if(trackPtr == IntPtr.Zero)
				return;
			
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		
		private bool CheckDisposed(bool throwOnDisposed)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				bool result = trackPtr == IntPtr.Zero;
				if(result && throwOnDisposed)
					throw new ObjectDisposedException("Track");
				
				return result;
			}
		}
		#endregion	
	}
}

