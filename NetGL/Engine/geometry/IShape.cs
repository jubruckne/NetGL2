namespace NetGL;

public interface IShape { }

public interface IShape <in T> : IShape where T: IShape {
}