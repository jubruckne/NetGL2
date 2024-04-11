using System.Reflection;
using System.Runtime.CompilerServices;

namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public abstract class TextureBuffer: Buffer, IBindableIndexed {
    protected readonly TextureTarget target;

    public int width { get; protected init; }
    public int height { get; protected init; }
    public int texture_unit { get; private set; } = -1;
    int IBindableIndexed.binding_point => texture_unit;

    public void gggg() {}

    public string glsl_type {
        get {
            return target switch {
                TextureTarget.TextureCubeMap => "samplerCube",
                TextureTarget.Texture2D      => "sampler2D",
                _                            => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public TextureBuffer(TextureTarget target) {
        this.target = target;
    }

    public void bind(int texture_unit) {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
        GL.BindTexture(target, handle);
    }

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
}

public enum TextureBuffer2DType { Texture2D = TextureTarget.Texture2D }
public enum TextureBuffer3DType { Texture2DArray = TextureTarget.Texture2DArray, Texture3D = TextureTarget.Texture3D }

public class TextureBuffer2D<T>: TextureBuffer<T> where T: unmanaged {
    public readonly int width;
    public readonly int height;

    public TextureBuffer2D(TextureBuffer2DType target,
                           int width,
                           int height,
                           PixelFormat pixel_format,
                           PixelType pixel_type
    ): base((TextureTarget)target, width * height, pixel_format, pixel_type) {
        this.width  = width;
        this.height = height;
    }

    public ref T this[int x, int y] => ref buffer.by_ref(x + y * width);

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage2D(
                      target,
                      level: 0,
                      PixelInternalFormat.Rgba,
                      width,
                      height,
                      border: 0,
                      pixel_format,
                      pixel_type,
                      buffer.get_address()
                     );

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

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

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Debug.assert_opengl();
    }
}

public class TextureBuffer3D<T>: TextureBuffer<T>
    where T: unmanaged {

    public readonly int width;
    public readonly int height;
    public readonly int depth;

    public TextureBuffer3D(TextureBuffer3DType target,
                           int width,
                           int height,
                           int depth,
                           PixelFormat pixel_format,
                           PixelType pixel_type
    ): base((TextureTarget)target, width * height * depth, pixel_format, pixel_type) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    // address = (z * width * height + y * width + x) * bytesPerPixel
    public ref T this[int x, int y, int z] => ref buffer.by_ref(x + y * width + z * width * height);

    public ArrayView<T> get_individual(int z)
        => new(buffer.get_address(z * width * height), item_size, width * height);

    public override void create() {
        if (handle == 0)
            handle = GL.GenTexture();

        GL.BindTexture(target, handle);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(target, TextureParameterName.TextureMaxAnisotropy, 8);

        GL.TexImage3D(
                      target,
                      0,
                      PixelInternalFormat.Rgba,
                      width,
                      height,
                      depth,
                      0,
                      pixel_format,
                      pixel_type,
                      buffer.get_address()
                     );

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Debug.assert_opengl();
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

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        Debug.assert_opengl();
    }
}

public abstract class TextureBuffer<T>: Buffer, IBindable, IBindableIndexed where T: unmanaged {
    public int texture_unit { get; private set; }
    public readonly PixelFormat pixel_format;
    public readonly PixelType pixel_type;
    protected readonly NativeArray<T> buffer;

    ~TextureBuffer() {
        buffer.Dispose();
    }

    int IBindableIndexed.binding_point => texture_unit;
    protected readonly TextureTarget target;

    protected TextureBuffer(TextureTarget target, int length, PixelFormat pixel_format, PixelType pixel_type) {
        Debug.assert(pixel_type.size_of_opengl(pixel_format) == Unsafe.SizeOf<T>());

        this.handle       = 0;
        this.target       = target;
        this.pixel_format = pixel_format;
        this.pixel_type   = pixel_type;
        this.texture_unit = 0;

        this.buffer = new NativeArray<T>(length);
    }

    public override int length => buffer.length;
    public override Type item_type => typeof(T);
    public override int total_size => buffer.total_size;
    public override int item_size => Unsafe.SizeOf<T>();

    public void bind(int texture_unit) {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        this.texture_unit = texture_unit;

        GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
        GL.BindTexture(target, handle);
    }

    public void bind() => bind(texture_unit);
}