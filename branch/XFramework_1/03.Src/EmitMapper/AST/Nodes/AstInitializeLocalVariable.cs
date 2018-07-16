using System;
using System.Reflection.Emit;
using EmitMapper.AST.Interfaces;

namespace EmitMapper.AST.Nodes
{
    //it seems that only value type work
    class AstInitializeLocalVariable: IAstNode
    {
        public Type localType;
        public int localIndex;

        public AstInitializeLocalVariable()
        {
        }

        public AstInitializeLocalVariable(LocalBuilder loc)
        {
            localType = loc.LocalType;
            localIndex = loc.LocalIndex;
        }

        public void Compile(CompilationContext context)
        {
            if(localType.IsValueType)
            {
                context.Emit(OpCodes.Ldloca, localIndex);
                context.Emit(OpCodes.Initobj, localType);
            }
        }
    }
}