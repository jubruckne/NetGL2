//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/julia/RiderProjects/NetGL/NetGL/Assets/shader_lang.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="shader_langParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public interface Ishader_langVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.shader"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitShader([NotNull] shader_langParser.ShaderContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.shaderDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitShaderDecl([NotNull] shader_langParser.ShaderDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.usingDecls"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUsingDecls([NotNull] shader_langParser.UsingDeclsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.usingDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUsingDecl([NotNull] shader_langParser.UsingDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexBlock([NotNull] shader_langParser.VertexBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexDecls"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexDecls([NotNull] shader_langParser.VertexDeclsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexDecl([NotNull] shader_langParser.VertexDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType([NotNull] shader_langParser.TypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.uniformBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUniformBlock([NotNull] shader_langParser.UniformBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.uniformDecls"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUniformDecls([NotNull] shader_langParser.UniformDeclsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.uniformDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUniformDecl([NotNull] shader_langParser.UniformDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexStage"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexStage([NotNull] shader_langParser.VertexStageContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexStageParams"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexStageParams([NotNull] shader_langParser.VertexStageParamsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexStageParam"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexStageParam([NotNull] shader_langParser.VertexStageParamContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.vertexBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVertexBody([NotNull] shader_langParser.VertexBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] shader_langParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="shader_langParser.assignment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignment([NotNull] shader_langParser.AssignmentContext context);
}