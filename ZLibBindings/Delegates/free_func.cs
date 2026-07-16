using unsafe Voidpf = void**;

namespace ZLibBindings.Delegates;

public unsafe delegate void free_func(Voidpf opaque, Voidpf address);
