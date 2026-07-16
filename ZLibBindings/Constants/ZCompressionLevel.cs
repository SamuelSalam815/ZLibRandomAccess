namespace ZLibBindings.Constants;

public enum ZCompressionLevel
{
    Z_NO_COMPRESSION = 0,

    Z_BEST_SPEED = 1,

    Level_1 = Z_BEST_SPEED,
    Level_2 = 2,
    Level_3 = 3,
    Level_4 = 4,
    Level_5 = 5,
    Level_6 = 6,
    Level_7 = 7,
    Level_8 = 8,
    Level_9 = 9,

    Z_BEST_COMPRESSION = 9,

    Z_DEFAULT_COMPRESSION = (-1),
}
