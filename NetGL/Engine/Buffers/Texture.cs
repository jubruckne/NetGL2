using System.Runtime.CompilerServices;

namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public abstract class TextureBuffer: Buffer {
    protected readonly TextureTarget target;

    public int width { get; protected init; }
    public int height { get; protected init; }

    public PixelFormat pixel_format => throw new NotImplementedException();
    public PixelType pixel_type => throw new NotImplementedException();

    public int texture_unit { get; private set; } = -1;

    public string sampler_name => target switch {
        TextureTarget.TextureCubeMap => "samplerCube",
        TextureTarget.Texture2D      => "sampler2D",
        _                            => throw new ArgumentOutOfRangeException()
    };

    public TextureBuffer(TextureTarget target) {
        this.target = target;
    }

    public void bind(int texture_unit) {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
        GL.BindTexture(target, handle);
    }

    public void bind() => bind(0);
}

public class Texture2D<T>: Texture<T>
    where T: unmanaged {
    public readonly int width;
    public readonly int height;

    public Texture2D(int width,
                     int height,
                     PixelFormat pixel_format,
                     PixelType pixel_type
    ): base(TextureTarget.Texture2D, width * height, pixel_format, pixel_type) {
        this.width  = width;
        this.height = height;
    }

    public override string sampler_name => "sampler2D";

    public ref T this[int x, int y] => ref buffer.by_ref(x + y * width);

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);

        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)min_filter);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)mag_filter);

        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)wrap_s);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)wrap_t);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage2D(
                      target,
                      level: 0,
                      internal_pixel_format,
                      width,
                      height,
                      border: 0,
                      pixel_format,
                      pixel_type,
                      buffer.get_address()
                     );

        if (min_filter is TextureMinFilter.LinearMipmapLinear or TextureMinFilter.NearestMipmapNearest) {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        Debug.assert_opengl();
    }

    public override void update() {
        if (handle == 0)
            Error.not_allocated(this);

        GL.BindTexture(target, handle);
        GL.TexSubImage2D(
                         target,
                         level: 0,
                         xoffset: 0,
                         yoffset: 0,
                         width,
                         height,
                         pixel_format,
                         pixel_type,
                         buffer.get_address()
                        );

        if (min_filter is TextureMinFilter.LinearMipmapLinear or TextureMinFilter.NearestMipmapNearest) {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        Debug.assert_opengl();
    }
}

public class Texture2DArray<T>: Texture<T>
    where T: unmanaged {

    public readonly int width;
    public readonly int height;
    public readonly int depth;

    public Texture2DArray(int width,
                          int height,
                          int depth,
                          PixelFormat pixel_format,
                          PixelType pixel_type
    ): base(TextureTarget.Texture2DArray, width * height * depth, pixel_format, pixel_type) {
        this.width  = width;
        this.height = height;
        this.depth  = depth;
    }

    public override string sampler_name => "sampler2DArray";

    // address = (z * width * height + y * width + x) * bytesPerPixel
    public ref T this[int x, int y, int z] => ref buffer.by_ref(x + y * width + z * width * height);

    public ArrayView<T> get_individual(int z)
        => new(buffer.get_address(z * width * height), item_size, width * height);

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);

        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)min_filter);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)mag_filter);

        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)wrap_s);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)wrap_t);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage3D(
                      target,
                      0,
                      internal_pixel_format,
                      width,
                      height,
                      depth,
                      0,
                      pixel_format,
                      pixel_type,
                      buffer.get_address()
                     );

        if (min_filter is TextureMinFilter.LinearMipmapLinear or TextureMinFilter.NearestMipmapNearest) {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        }

        Debug.assert_opengl();
    }

    public void update(int z) {
        if (handle == 0)
            Error.not_allocated(this);
        if (z < 0 || z >= depth)
            throw new ArgumentOutOfRangeException(nameof(z));
        if (buffer.length < width * height * (z + 1))
            throw new InvalidOperationException("buffer is too small!");
        if (target != TextureTarget.Texture2DArray)
            throw new InvalidOperationException("this method is only valid for Texture2DArray!");

        GL.BindTexture(target, handle);

        GL.TexSubImage3D(
                         target,
                         level: 0,
                         xoffset: 0,
                         yoffset: 0,
                         zoffset: z,
                         width,
                         height,
                         depth: 1,
                         pixel_format,
                         pixel_type,
                         buffer.get_address(z * width * height)
                        );
    }

    public override void update() {
        if (handle == 0)
            Error.not_allocated(this);

        GL.BindTexture(target, handle);

        GL.TexSubImage3D(
                         target,
                         0,
                         0,
                         0,
                         0,
                         width,
                         height,
                         depth,
                         pixel_format,
                         pixel_type,
                         buffer.get_address()
                        );

        if (min_filter is TextureMinFilter.LinearMipmapLinear or TextureMinFilter.NearestMipmapNearest) {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        Debug.assert_opengl();
    }
}

public abstract class Texture {
    public PixelFormat pixel_format { get; init;  }
    public PixelType pixel_type { get; init; }
    public PixelInternalFormat internal_pixel_format { get; set; }

    protected readonly TextureTarget target;

    public TextureWrapMode wrap_s { get; set; } = TextureWrapMode.ClampToEdge;
    public TextureWrapMode wrap_t { get; set; } = TextureWrapMode.ClampToEdge;

    public TextureMinFilter min_filter { get; set; } = TextureMinFilter.Linear;
    public TextureMagFilter mag_filter { get; set; } = TextureMagFilter.Linear;

    public int texture_unit { get; protected set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public abstract string sampler_name { get; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public int version { get; protected set; }
    public Buffer.Status status { get; protected set; }

    public void bind() => bind(texture_unit);

    public abstract void bind(int texture_unit);

    protected Texture(TextureTarget target, PixelFormat pixel_format, PixelType pixel_type) {
        this.target       = target;
        this.pixel_format = pixel_format;
        this.pixel_type   = pixel_type;
        this.texture_unit = 0;

        switch (pixel_format) {
            case PixelFormat.Rgba:
                internal_pixel_format = PixelInternalFormat.Rgba;
                break;
            case PixelFormat.Rgb:
                internal_pixel_format = PixelInternalFormat.Rgb;
                break;
            case PixelFormat.Red:
                internal_pixel_format = pixel_type switch {
                    PixelType.Float        => PixelInternalFormat.R32f,
                    PixelType.HalfFloat    => PixelInternalFormat.R16f,
                    PixelType.UnsignedByte => PixelInternalFormat.R8,
                    _                      => throw new ArgumentOutOfRangeException()
                };
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}

public abstract class Texture<T>: Texture, IDisposable
    where T: unmanaged {
    protected int handle;

    protected readonly NativeArray<T> buffer;

    ~Texture() {
        Dispose(false);
    }

    protected Texture(TextureTarget target, int length, PixelFormat pixel_format, PixelType pixel_type)
        : base(target, pixel_format, pixel_type) {
        Debug.assert_equal(pixel_type.size_of_opengl(pixel_format), Unsafe.SizeOf<T>());

        this.handle       = 0;
        this.buffer = new NativeArray<T>(length, true);
    }

    public override void bind(int texture_unit) {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        this.texture_unit = texture_unit;

        GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
        GL.BindTexture(target, handle);
    }

    public abstract void create();
    public abstract void update();

    public void query_info() {
        bind(0);
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureWidth, out int texture_width);
        Console.WriteLine($"texture_width: {texture_width}");
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureHeight, out int texture_height);
        Console.WriteLine($"texture_height: {texture_height}");
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureDepth, out int texture_depth);
        Console.WriteLine($"texture_depth: {texture_depth}");
        Console.WriteLine();

        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureInternalFormat, out int texture_internal_format);
        Console.WriteLine($"texture_internal_format: {texture_internal_format} {(SizedInternalFormat)texture_internal_format}");

        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureRedSize, out int red_size);
        Console.WriteLine($"red_size: {red_size}");
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureGreenSize, out int green_size);
        Console.WriteLine($"green_size: {green_size}");
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureBlueSize, out int blue_size);
        Console.WriteLine($"blue_size: {blue_size}");
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureAlphaSize, out int alpha_size);
        Console.WriteLine($"alpha_size: {alpha_size}");

        Console.WriteLine();
        GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureCompressed, out int compressed);
        Console.WriteLine($"compressed: {compressed}");
        if (compressed == 1) {
            GL.GetTexLevelParameter(target, 0, GetTextureParameter.TextureCompressedImageSize, out int compressed_size);
            Console.WriteLine($"compressed_size: {compressed_size:N0}");
        }

        Console.WriteLine();

        Debug.assert_opengl();
    }

    private void Dispose(bool disposing) {
        if (disposing) {
            buffer.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public int length => buffer.length;
    public Type item_type => typeof(T);
    public int item_size => Unsafe.SizeOf<T>();
    public int total_size => item_size * length;

    internal Span<T> as_span() => buffer.as_span();
    internal ReadOnlySpan<T> as_readonly_span() => buffer.as_span();
}