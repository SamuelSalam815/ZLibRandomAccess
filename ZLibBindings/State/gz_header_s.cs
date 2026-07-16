using unsafe Bytef = byte*;

namespace ZLibBindings.State;

public unsafe struct gz_header_s
{
    /// <summary>
    /// True if compressed data believed to be text
    /// </summary>
    int text;

    /// <summary>
    /// Modification time
    /// </summary>
    ulong time;

    /// <summary>
    /// Extra flags (not used when writing a gzip file)
    /// </summary>
    int xflags;

    /// <summary>
    /// Operating system
    /// </summary>
    int os;

    /// <summary>
    /// pointer to extra field or Z_NULL if none
    /// </summary>
    Bytef* extra;

    /// <summary>
    /// extra field length (valid if extra != Z_NULL)
    /// </summary>
    uint extra_len;

    /// <summary>
    /// space at extra (only when reading header)
    /// </summary>
    uint extra_max;

    /// <summary>
    /// pointer to zero-terminated file name or Z_NULL
    /// </summary>
    Bytef* name;

    /// <summary>
    /// space at name (only when reading header)
    /// </summary>
    uint name_max;

    /// <summary>
    /// pointer to zero-terminated comment or Z_NULL
    /// </summary>
    Bytef* comment;

    /// <summary>
    /// space at comment (only when reading header)
    /// </summary>
    uint comm_max;

    /// <summary>
    /// true if there was or will be a header crc
    /// </summary>
    int hcrc;

    /// <summary>
    /// true when done reading gzip header (not used when writing a gzip file)
    /// </summary>
    int done;
}
