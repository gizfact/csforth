//------------------------------------------------------------------------------
using System;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static partial class Core
    {
        //------------------------------------------------------------------------------
        // DOUBLE MATH WORDS
        //------------------------------------------------------------------------------
        static int dplus()
        {
            fStack.Push((double)fStack.Pop() + (double)fStack.Pop());
           
            return 0;
        }
        //------------------------------------------------------------------------------
        static int dminus()
        {
            fStack.Push(-(double)fStack.Pop() + (double)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dmul()
        {
            fStack.Push((double)fStack.Pop() * (double)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int ddiv()
        {
            double hi = (double)fStack.Pop();
            double lo = (double)fStack.Pop();

            fStack.Push(lo / hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dabs()
        {
            fStack.Push(Math.Abs((double)fStack.Pop()));

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dneg()
        {
            fStack.Push(-(double)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        // УСЛОВИЯ
        //------------------------------------------------------------------------------
        static int g_then_d()
        {
            double hi = (double)fStack.Pop();
            double lo = (double)fStack.Pop();

            fStack.Push(lo > hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int l_then_d()
        {
            double hi = (double)fStack.Pop();
            double lo = (double)fStack.Pop();

            fStack.Push(lo < hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int ge_then_d()
        {
            double hi = (double)fStack.Pop();
            double lo = (double)fStack.Pop();

            fStack.Push(lo >= hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int le_then_d()
        {
            double hi = (double)fStack.Pop();
            double lo = (double)fStack.Pop();

            fStack.Push(lo <= hi ? -1 : 0);

            return 0;
        }
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------