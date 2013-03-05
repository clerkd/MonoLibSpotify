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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonoLibSpotify.Models
{
	public class SPArtist
	{
		#region Declarations
		internal IntPtr artistPtr = IntPtr.Zero;		
		#endregion
		
		#region Ctor
		internal SPArtist(IntPtr artistPtr)
		{
			if(artistPtr == IntPtr.Zero)
				throw new ArgumentException("artistPtr can not be zero");
			
			this.artistPtr = artistPtr;

			LibspotifyWrapper.Artist.AddRef(artistPtr);
		}
		#endregion
		
		#region Static methods
		public static SPArtist CreateFromLink(SPLink link)
		{
			SPArtist result = null;
			
			if(link.linkPtr != IntPtr.Zero)
			{
				lock(LibspotifyWrapper.Mutex)
				{
					IntPtr artistPtr = LibspotifyWrapper.Link.AsArtist(link.linkPtr);
					if(artistPtr != IntPtr.Zero)
						result = new SPArtist(artistPtr);
				}
			}
			
			return result;
		}
		
		public static string ArtistsToString(SPArtist[] artists)
		{
			if(artists == null)
				return string.Empty;
			
			List<string> names = new List<string>();
			foreach(SPArtist a in artists)
				names.Add(a.Name);
			
			return string.Join(", ", names.ToArray());
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
					return LibspotifyWrapper.Artist.IsLoaded(artistPtr);	
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
					return LibspotifyWrapper.GetString(LibspotifyWrapper.Artist.Name(artistPtr), string.Empty);
				}
			}
		}		
		
		public string LinkString
		{
			get
			{
				CheckDisposed(true);
				
				string linkString = string.Empty;
				using(SPLink l = CreateLink())
				{
					if( l != null)
						linkString = l.ToString();						
				}
				
				return linkString;
			}
		}
		#endregion
		
		#region Public methods
		public SPLink CreateLink()
		{
			CheckDisposed(true);
			
			lock(LibspotifyWrapper.Mutex)
			{
				IntPtr linkPtr = LibspotifyWrapper.Link.CreateFromArtist(artistPtr);
				if(linkPtr != IntPtr.Zero)
					return new SPLink(linkPtr);
				else
					return null;
			}
		}
		
		public override string ToString()
		{
			if(IsLoaded)
				return string.Format("[Artist: Name={0}, LinkString={1}]", Name, LinkString);
			else
				return "[Artist: Not loaded]";
		}
		#endregion
		
		#region Cleanup
		~SPArtist()
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
				
				if(artistPtr != IntPtr.Zero)
				{
					LibspotifyWrapper.Artist.Release(artistPtr);
					artistPtr = IntPtr.Zero;
				}
			}
			catch
			{
				
			}
		}		
		
		public void Dispose()
		{
			if(artistPtr == IntPtr.Zero)
				return;
			
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		
		private bool CheckDisposed(bool throwOnDisposed)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				bool result = artistPtr == IntPtr.Zero;
				if(result && throwOnDisposed)
					throw new ObjectDisposedException("Artist");
				
				return result;
			}
		}
		#endregion	
	}
}

