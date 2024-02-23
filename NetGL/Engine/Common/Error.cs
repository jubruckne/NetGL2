using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public static class Error {
    public class Exception: System.Exception {
        public readonly int code;

        public Exception(string message): base(message) { }

        public Exception(int code, string message): base(message) {
            this.code = code;
        }
    }

    public class WrongContextException : Exception {
        public WrongContextException(string expected, string current) : base(0, $"{expected} is not current! (current: {current})") {
        }
    }

    public static bool check(bool throw_exception = true) {
        var err = GL.GetError();
        var msg = "";

        while (err != ErrorCode.NoError) {
            Console.WriteLine("Error: " + err);
            msg += err + " ";

            err = GL.GetError();
        }

        if (msg == "")
            return true;

        if (throw_exception)
            throw new Exception(123, msg);

        return false;
    }
}