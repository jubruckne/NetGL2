// ReSharper disable StaticMemberInGenericType

namespace NetGL;

public static class Pool<T> where T: unmanaged {
    private static int first_free;
    private static readonly NativeArray<T> array = new NativeArray<T>(100);
    private static readonly NativeArray<bool> used_list = new NativeArray<bool>(100);

    public static int capacity => array.length;
    public static int count() => used_list.count(static b => b);

    public static unsafe Pointer<T> allocate() {
        while (used_list[first_free] && first_free < used_list.length)
            ++first_free;

        Debug.assert_not_equal(first_free, used_list.length);

        used_list[first_free] = true;
        array[first_free] = default;

        return new(array, (T*)array.get_address(first_free), dispose_pointer);
    }

    private static unsafe void dispose_pointer(IntPtr ptr, in object? owner) {
        if (owner == null) Error.type_conversion_error<object, NativeArray<T>>(owner!);

        var base_array = (NativeArray<T>)owner;
        var index      = (int)(ptr - base_array.get_address()) / sizeof(T);

        array[index]     = default;
        used_list[index] = false;
    }
}