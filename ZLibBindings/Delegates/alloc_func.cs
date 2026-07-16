using unsafe Voidpf = void*;

namespace ZLibBindings.Delegates;

public unsafe delegate Voidpf alloc_func(Voidpf opaque, uint items, uint size);
