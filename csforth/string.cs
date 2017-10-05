//------------------------------------------------------------------------------
//using System.Linq;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static partial class Core
    {
        //------------------------------------------------------------------------------
        // STRING WORDS
        //------------------------------------------------------------------------------
        static int schar()
        {
            int hi = (int)fStack.Pop();
            string lo = (string)fStack.Pop();

            fStack.Push((int)lo[hi]);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int spos()
        {
            string hi = (string)fStack.Pop();
            string lo = (string)fStack.Pop();

            fStack.Push(lo.IndexOf(hi));

            return 0;
        }
        //------------------------------------------------------------------------------
        static int format()
        {
            string s = (string)fStack.Pop();
            int cnt = (int)fStack.Pop();

            object[] args = new object[cnt];

            for (int i = 0; i < cnt; i++)
                args[i] = fStack.Pop();

            fStack.Push(string.Format(s, args));
            
            return 0;
        }
        //------------------------------------------------------------------------------
        static int splus()
        {
            string hi = (string)fStack.Pop();
            string lo = (string)fStack.Pop();

            fStack.Push(lo + hi);

            return 0;
        }
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------