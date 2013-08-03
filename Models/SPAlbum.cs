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
	public class SPAlbum
	{
		#region Declarations
		internal IntPtr albumPtr = IntPtr.Zero;
		#endregion
		
		#region Ctor
		internal SPAlbum(IntPtr albumPtr)
		{
			if(albumPtr == IntPtr.Zero)
				throw new ArgumentException("albumPtr can not be zero");
			
			this.albumPtr = albumPtr;
			
			lock(LibspotifyWrapper.Mutex)
				LibspotifyWrapper.Album.AddRef(albumPtr);
		}
		#endregion
		
		#region Static methods
		public static SPAlbum CreateFromLink(SPLink link)
		{
			SPAlbum result = null;
			
			if(link.linkPtr != IntPtr.Zero)
			{
				lock(LibspotifyWrapper.Mutex)
				{
					var albumPtr = LibspotifyWrapper.Link.AsAlbum(link.linkPtr);
					if(albumPtr != IntPtr.Zero)
						result = new SPAlbum(albumPtr);
				}
			}
			
			return result;
		}
		#endregion
		
		#region Properties
		public bool IsLoaded
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return LibspotifyWrapper.Album.IsLoaded(albumPtr);	
				}
			}
		}
		
		public bool IsAvailable
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return LibspotifyWrapper.Album.IsAvailable(albumPtr);
				}
			}
		}
		
		public SPArtist Artist
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return new SPArtist(LibspotifyWrapper.Album.Artist(albumPtr));
				}
			}
		}
		
		public string Name
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return LibspotifyWrapper.GetString(LibspotifyWrapper.Album.Name(albumPtr), string.Empty);
				}
			}
		}
		
		public int Year
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return LibspotifyWrapper.Album.Year(albumPtr);
				}
			}
		}
		
		public string CoverId
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					var coverIdPtr = LibspotifyWrapper.Album.Cover(albumPtr);
					if (coverIdPtr == IntPtr.Zero)
						return null;
					
					var coverId = new byte[20];
					Marshal.Copy(coverIdPtr, coverId, 0, coverId.Length);
					return LibspotifyWrapper.ImageIdToString(coverId);
				}
			}
		}		
		
		public string LinkString
		{
			get
			{
				CheckDisposed(true);
				
				var linkString = string.Empty;
				using(var l = CreateLink())
				{
					if( l != null)
						linkString = l.ToString();	
				}
				
				return linkString;
			}
		}
		
		public sp_albumtype Type
		{
			get
			{
				CheckDisposed(true);
				
				lock(LibspotifyWrapper.Mutex)
				{
					return LibspotifyWrapper.Album.Type(albumPtr);
				}
			}
		}
		#endregion
		
		#region Public methods
		public SPLink CreateLink()
		{
			CheckDisposed(true);
			
			lock(LibspotifyWrapper.Mutex)
			{
			    var linkPtr = LibspotifyWrapper.Link.CreateFromAlbum(albumPtr);
			    return linkPtr != IntPtr.Zero ? new SPLink(linkPtr) : null;
			}
		}
		
		public override string ToString()
		{
			if(IsLoaded)
			{
				return string.Format("[Album: Artist={0}, Name={1}, Year={2}, CoverId={3}, LinkString={4}]",
				                     Artist, Name, Year, CoverId, LinkString);
			}
			else
			{
				return "[Album: Not loaded]";
			}
		}
		#endregion
		
		#region Cleanup
		~SPAlbum()
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

			    if (albumPtr == IntPtr.Zero) return;
			    LibspotifyWrapper.Album.Release(albumPtr);
			    albumPtr = IntPtr.Zero;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}		
		
		public void Dispose()
		{
			if(albumPtr == IntPtr.Zero)
				return;
			
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		
		private bool CheckDisposed(bool throwOnDisposed)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var result = albumPtr == IntPtr.Zero;
				if(result && throwOnDisposed)
					throw new ObjectDisposedException("Album");
				
				return result;
			}
		}
		#endregion	
	}
}

