
namespace ZLibBindings.Constants;

[Flags]
public enum ZDataType
{
    Z_BINARY = 0,

    Z_TEXT = 1,

    Z_ASCII = Z_TEXT, /* for compatibility with 1.2.2 and earlier */

    Z_UNKNOWN = 2,

    Z_IS_FINAL_BLOCK = 64,
    Z_END_OF_BLOCK = 128,
}
