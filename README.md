## Xamarin.iOS (MonoTouch) bindings for libspotify

This is a Xamarin.iOS (MonoTouch) bindings for libspotify (tested with v12.1.51) that allows integrating Spotify Premium into your Xamarin.iOS application.  
Bindings are pretty basic and do not provide complete spotify API access.  

#### Bindings was made specially for [Clerkd](http://clerkd.com), it's free, check it out :)

### Currently working
- Authentication  
- Search  
- Artist, album browsing  
- Artist, album, track info / image retrieval  
- Songs playback (player provided only for iOS)  

**Not working**: everything else, but it should be relatively easy to add required features using existing stuff.
  
Probably those bindings will also work with Xamarin.Android (Mono for Android), but you'll need to make a separate player to process raw data from spotify for it.  
  
### How to use
1. Clone [MonoLibSpotify](github.com/clerkd/monolibspotify) repository
2. Add this MonoLibSpotify project to your solution
3. Download [libspotify](https://developer.spotify.com/technologies/libspotify/)
4. Copy  "libspotify" binary to your project folder
5. Look at `Example/Example.cs` for example on how to use bindings
6. Set additional mtouch arguments:  
```-cxx -gcc_flags "-L${ProjectDir} -all_load -force_load ${ProjectDir}/libspotify"```

Run you app, login with Spotify Premium account and enjoy :)  
Please, note that you'll have to make authorisation UI yourself.  
