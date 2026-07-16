namespace ZLibBindings.State;

public unsafe struct gz_header_s
{
    /// <summary>
    /// True if compressed data believed to be text
    /// </summary>
    public int text;

    /// <summary>
    /// Modification time
    /// </summary>
    public ulong time;

    /// <summary>
    /// Extra flags (not used when writing a gzip file)
    /// </summary>
    public int xflags;

    /// <summary>
    /// Operating system
    /// </summary>
    public int os;

    /// <summary>
    /// pointer to extra field or Z_NULL if none
    /// </summary>
    public byte* extra;

    /// <summary>
    /// extra field length (valid if extra != Z_NULL)
    /// </summary>
    public uint extra_len;

    /// <summary>
    /// space at extra (only when reading header)
    /// </summary>
    public uint extra_max;

    /// <summary>
    /// pointer to zero-terminated file name or Z_NULL
    /// </summary>
    public byte* name;

    /// <summary>
    /// space at name (only when reading header)
    /// </summary>
    public uint name_max;

    /// <summary>
    /// pointer to zero-terminated comment or Z_NULL
    /// </summary>
    public byte* comment;

    /// <summary>
    /// space at comment (only when reading header)
    /// </summary>
    public uint comm_max;

    /// <summary>
    /// true if there was or will be a header crc
    /// </summary>
    public int hcrc;

    /// <summary>
    /// true when done reading gzip header (not used when writing a gzip file)
    /// </summary>
    public int done;
}
