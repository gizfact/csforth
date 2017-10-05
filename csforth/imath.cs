//------------------------------------------------------------------------------
using System;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static partial class Core
    {
        //------------------------------------------------------------------------------
        // INTEGER MATH WORDS
        //------------------------------------------------------------------------------
        static int iplus()
        {
            fStack.Push((int)fStack.Pop() + (int)fStack.Pop());
         
            return 0;
        }
        //------------------------------------------------------------------------------
        static int iminus()
        {
            fStack.Push(-(int)fStack.Pop() + (int)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int imul()
        { 
            fStack.Push((int)fStack.Pop() * (int)fStack.Pop());
          
            return 0;
        }
        //------------------------------------------------------------------------------
        static int idiv()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo / hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int inc()
        {
            fStack.Push((int)fStack.Pop() + 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dec()
        {
            fStack.Push((int)fStack.Pop() - 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int asl()
        {
            fStack.Push((int)fStack.Pop() << 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int asr()
        {
            fStack.Push((int)fStack.Pop() >> 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int asln()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();
            fStack.Push(lo << hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int asrn()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();
            fStack.Push(lo >> hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int shl()
        {
            unchecked
            {
                uint hi = (uint)((int)fStack.Pop()) << 1;
                //hi <<= 1;
                fStack.Push((int)hi);
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int shr()
        {
            unchecked
            {
                uint hi = (uint)((int)fStack.Pop()) >> 1;
                //hi >>= 1;
                fStack.Push((int)hi);
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int shln()
        {
            unchecked
            {
                int hi = (int)fStack.Pop();
                uint lo = (uint)((int)fStack.Pop());
                fStack.Push((int)(lo << hi));
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int shrn()
        {
            unchecked
            {
                int hi = (int)fStack.Pop();
                uint lo = (uint)((int)fStack.Pop());
                fStack.Push((int)(lo >> hi));
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int abs()
        {
            fStack.Push(Math.Abs((int)fStack.Pop()));
            
            return 0;
        }
        //------------------------------------------------------------------------------
        static int neg()
        {
            fStack.Push(-(int)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int not()
        {
            fStack.Push(~(int)fStack.Pop());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int and()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo & hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int or()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo | hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int xor()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo ^ hi);

            return 0;
        }
        //------------------------------------------------------------------------------
        // УСЛОВИЯ
        //------------------------------------------------------------------------------
        static int g_then()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo > hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int l_then()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo < hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int ge_then()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo >= hi ? -1 : 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int le_then()
        {
            int hi = (int)fStack.Pop();
            int lo = (int)fStack.Pop();

            fStack.Push(lo <= hi ? -1 : 0);

            return 0;
        }
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
