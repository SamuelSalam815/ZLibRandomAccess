## Questions

What is the difference between DLLImport and LibraryImport attributes in c#?


## Lessons Learned

### Bad Free from String Marshalling

Marshalling c++ type `const char*` to c# type `string` leads to a bad free.

Marshalling to string with PINVOKE auto frees the pointer, but with in this case the `const char*` should never have been freed.

The way this error appeared
1. Unit test dies with irrelavent stack trace
2. Kills a console app process dies without the debugger being able to stop
3. Appears as a fault in the windows event viewer under a dll I never explicitly call into

The lesson:
1. distrust convenience when crossing the unmanaged-to-managed boundary
2. You need to know how to translate memory management
3. Windows event viewer is useful when exceptions cannot be caught
4. Check all exception details/error codes - this time the error code was Heap Corruption - dead giveaway for memory management gone wrong.

### Error Code does not make sense

Incident

> Calling deflateInit from C# was returning an error code indicating an error with the library version (Z_VERSION_ERROR)
> I tried recompiling from source and fetching the latest sources from the zlib github repo, however this error persisted.
> In the end I used AI to diagnose the issue - the issue was with the size of the struct representing the stream state. The type defined in c# was not the expected size for the library.

The lesson here is that once I had exhausted reasonable suspicion that Z_VERSION_ERROR was actually caused by passing the wrong error - I could have explored the possiblity that Z_VERSION_ERROR could be caused by something other than the version number.

Since I had the source code available to me this should have made the investigation easier - but even without the source code, decompilation tools could be used.

Also since the size of the stream struct is passed as an argument - it could have been a prime suspect for values that could have been wrong.

