using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;

namespace EmitMapper.AST.Interfaces
{
    /// <summary>
    /// Ast（Abstract Syntax Tree ）抽象语法树
    /// </summary>
    interface IAstNode
    {
        void Compile(CompilationContext context);
    }
}