//------------------------------------------------------------------------------
using System.Collections.Generic;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static partial class Core
    {
        //------------------------------------------------------------------------------
        struct xWord
        {
            public string name;
            public ForthWord fun;
            public object[] code;
            public WordProperty prop;
            //public List<xWord> sub;

            public xWord(string n, ForthWord f, WordProperty p = WordProperty.None, object[] c = null) { name = n; fun = f; prop = p; code = c; }
            //public xWord(string n, ForthWord f, List<xWord> s) { name = n; fun = f; code = null; sub = s; }
        };
        //------------------------------------------------------------------------------
        static List<xWord> CoreDic = new List<xWord>
        {
            // Core function
            new xWord("class", class_beg, WordProperty.Immediate),
            new xWord("/*", comment_beg, WordProperty.Immediate),
            new xWord(":", compile_beg, WordProperty.Immediate),
            new xWord(";", compile_end, WordProperty.Immediate),
            new xWord("if", if_beg, WordProperty.Immediate),
            new xWord("else", else_word, WordProperty.Immediate),
            new xWord("while", while_beg, WordProperty.Immediate),
            new xWord("end", end_word, WordProperty.Immediate),
            new xWord("str", str_word, WordProperty.Immediate),
            new xWord("var", var_word, WordProperty.Immediate),
            new xWord("array", array_word, WordProperty.Immediate),
            

            new xWord("ipush", ipush, WordProperty.NoName),
            new xWord("dpush", dpush, WordProperty.NoName),
            new xWord("spush", spush, WordProperty.NoName),
            new xWord("jz", jz, WordProperty.NoName),
            new xWord("jz_peek", jz_peek, WordProperty.NoName),
            new xWord("jmp", jmp, WordProperty.NoName),
            new xWord("exec", exec, WordProperty.NoName),
            new xWord("class_exec", class_exec, WordProperty.NoName),

            new xWord("word", word),
            new xWord("inumber", inumber),
            new xWord("dnumber", dnumber),
            new xWord("find", find),
            new xWord("forget", forget),
            new xWord("idle", idle),
            new xWord("type", gettype),
            new xWord("typename", gettypename),

            // Stack words
            new xWord("drop", drop),
            new xWord("dup", dup),
            new xWord("2dup", dup2),
            new xWord("swap", swap),
            new xWord("over", over),
            new xWord("rot", rot),
            new xWord("-rot", nrot),

            // Memory words
            new xWord("!", tomem),
            new xWord("@", frommem),
            new xWord("[!]", toarr),
            new xWord("[@]", fromarr),
            new xWord("{!}", initarr),
            new xWord("new", arrnew_word),

            // iMath words
            new xWord("+", iplus),
            new xWord("-", iminus),
            new xWord("*", imul),
            new xWord("/", idiv),
            new xWord("1+", inc),
            new xWord("1-", dec),
            new xWord("asl", asl),
            new xWord("asr", asr),
            new xWord("asln", asln),
            new xWord("asrn", asrn),
            new xWord("shl", shl),
            new xWord("shr", shr),
            new xWord("shln", shln),
            new xWord("shrn", shrn),
            new xWord("abs", abs),
            new xWord("neg", neg),
            new xWord("not", not),
            new xWord("and", and),
            new xWord("or", or),
            new xWord("xor", xor),

            // dMath words
            new xWord("+d", dplus),
            new xWord("-d", dminus),
            new xWord("*d", dmul),
            new xWord("/d", ddiv),
            new xWord("absd", dabs),
            new xWord("negd", dneg),

            // Convertation words
            new xWord("a->i", atoi),
            new xWord("a->d", atod),
            new xWord("->i", toi),
            new xWord("->d", tod),

            // Output
            new xWord(".", iprint),
            new xWord(".d", dprint),
            new xWord(".s", sprint),
            new xWord("cls", cls),
            new xWord(".u16", uprint16),

            // Условия
            new xWord(">", g_then),
            new xWord("<", l_then),
            new xWord(">=", ge_then),
            new xWord("<=", le_then),
            new xWord(">d", g_then_d),
            new xWord("<d", l_then_d),
            new xWord(">=d", ge_then_d),
            new xWord("<=d", le_then_d),

            // Строковые
            new xWord("s[]", schar),
            new xWord("s.pos", spos),
            new xWord("format", format),
            new xWord("+s", splus),

            // Firebird
            new xWord("FBOpen", fbopen),
            new xWord("FBSelect", fbselect),
            new xWord("FBCell", fbcell),
            new xWord("FBColCount", fbcolcount),
            new xWord("FBRowCount", fbrowcount)
        };
        //------------------------------------------------------------------------------
        static List<xWord> MainDic = CoreDic;
        static List<xWord> ClassDic;

        static int BaseLength = CoreDic.Count;
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
