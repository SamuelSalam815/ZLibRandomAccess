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


