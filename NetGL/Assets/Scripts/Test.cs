using NetGL;
using NetGL.ECS;

Entity ball = world.create_sphere_cube("Ball", radius: 10f, segments:100, material:Material.Red);
ball.transform.position = (-5, 15, -15);

/*
ball.get<VertexArrayRenderer>().wireframe = false;
ball.get<VertexArrayRenderer>().cull_face = true;
ball.get<VertexArrayRenderer>().depth_test = true;
ball.get<VertexArrayRenderer>().blending = false;
//ball.add_rigid_body(radius:5f, mass:10000f);
*/
Console.WriteLine("Hello from Script!!!");