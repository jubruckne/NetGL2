using System.Collections;

namespace NetGL;

public struct BitBag: IEnumerable<bool> {
    private uint[] data;

    public BitBag()
        => data = new uint[1];

    public BitBag(int length) =>
        data = new uint[(length + 31) / 32];

    public void resize(int new_length) {
        var new_data = new uint[(new_length + 31) / 32];
        Array.Copy(data, new_data, data.Length);
        data = new_data;
    }

    public int length => data.Length * 32;

    public bool this[int index] {
        get => (data[index / 32] & (1u << (index % 32))) != 0;
        set {
            if (value)
                data[index / 32] |= 1u << (index % 32);
            else
                data[index / 32] &= ~(1u << (index % 32));
        }
    }

    public IEnumerator<bool> GetEnumerator() {
        for (var index = 0; index < data.Length; ++index) {
            yield return (data[index / 32] & (1u << (index % 32))) != 0;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

public struct BitBag<TEnum>
    where TEnum: Enum {
    private uint data;
    private static readonly Dictionary<TEnum, uint> dict = new();

    static BitBag() {
        foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            dict[value] = 1u << Convert.ToInt32(value);
    }

    public bool this[int index] {
        get => (data & (1u << index)) != 0;
        set {
            if (value)
                data |= 1u << index;
            else
                data &= ~(1u << index);
        }
    }

    public bool this[TEnum index] {
        get => (data & dict[index]) != 0;
        set {
            if (value)
                data |= dict[index];
            else
                data &= ~dict[index];
        }
    }
}