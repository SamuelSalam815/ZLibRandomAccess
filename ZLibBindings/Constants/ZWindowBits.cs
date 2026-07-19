namespace ZLibBindings.Constants;

public enum ZWindowBits
{
    Default = ZLib32KbWindow,

    RawDeflate32KbWindow = -15,
    RawDeflate16KbWindow,
    RawDeflate8KbWindow,
    RawDeflate4KbWindow,
    RawDeflate2KbWindow,
    RawDeflate1KbWindow,
    RawDeflate512BWindow,

    ZLib512BWindow = 9,
    ZLib1KbWindow,
    ZLib2KbWindow,
    ZLib4KbWindow,
    ZLib8KbWindow,
    ZLib16KbWindow,
    ZLib32KbWindow,

    GZip512BWindow = ZLib512BWindow + 16,
    GZip1KbWindow,
    GZip2KbWindow,
    GZip4KbWindow,
    GZip8KbWindow,
    GZip16KbWindow,
    GZip32KbWindow,

    AutoDetectHeader512BWindow = ZLib512BWindow + 32,
    AutoDetectHeader1KbWindow,
    AutoDetectHeader2KbWindow,
    AutoDetectHeader4KbWindow,
    AutoDetectHeader8KbWindow,
    AutoDetectHeader16KbWindow,
    AutoDetectHeader32KbWindow,
}
