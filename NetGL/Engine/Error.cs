using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public static class Error {
    public class Exception: System.Exception {
        public readonly int code;
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
        if (err != ErrorCode.NoError) {
            Console.WriteLine(err.ToString());
            if(throw_exception) throw 
                new Exception((int)err, err.ToString());
            return false;
        }  
        return true;
    }
}