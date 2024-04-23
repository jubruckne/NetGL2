using System.Runtime.CompilerServices;

namespace NetGL;

public sealed class LodLevels {
    private readonly LodLevel[] levels;
    public readonly int max_level;
    public LodLevel highest => levels[max_level];
    public LodLevel lowest => levels[0];
    public int count => levels.Length;

    private LodLevels(int levels) {
        this.levels = new LodLevel[levels];
        this.max_level = levels - 1;
    }

    public LodLevel this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining), SkipLocalsInit]
        get => levels[index];
    }

    public LodLevel select_for_distance(float distance) {
        for (var i = 0; i < levels.Length; ++i)
            if (distance >= levels[i].max_distance)
                return levels[i];

        return levels[^1];
    }

    public static LodLevels create(int levels, int l0_distance, int l0_tile_size) {
        var lod_levels = new LodLevels(levels);
        var tile_size = (short)(l0_tile_size << (levels - 1));
        var max_distance = (short)(l0_distance << (levels - 1));

        for (short i = 0; i < levels; ++i) {
            lod_levels.levels[i] = new(i, max_distance, tile_size);
            tile_size >>= 1;
            max_distance >>= 1;
            //  new((short)(levels - i - 1), (short)((l0_tile_size + 1).power(i) * 10), (short)l0_tile_size.power(i));
        }

        for (var i = 0; i < levels; ++i) {
            Console.WriteLine(lod_levels.levels[i]);
        }

        return lod_levels;
    }

    public static LodLevels create(Span<(short tile_size, short max_distance)> tile_sizes) {
        var levels= tile_sizes.Length;
        var lod_levels = new LodLevels(levels);

        for (var i = 0; i < levels; ++i)
            lod_levels.levels[levels - i - 1] =
                new LodLevel((short)(levels - i - 1), tile_sizes[i].max_distance, tile_sizes[i].tile_size);
        return lod_levels;
    }
}

public readonly struct LodLevel {
    public readonly short level;
    public readonly short tile_size;
    public readonly short max_distance;
    public short half_tile_size => (short)(tile_size >> 1);

    internal LodLevel(short level, short max_distance, short tileSize) {
        this.level = level;
        this.max_distance = max_distance;
        this.tile_size = tileSize;
    }

    public override string ToString()
        => $"Lod({level}, distance: {max_distance}, size: {tile_size})";
}