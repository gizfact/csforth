//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Globalization;
//using System.Text;
using System.Windows.Forms;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static partial class Core
    {
        //------------------------------------------------------------------------------
        delegate int ForthWord();
        enum WordProperty
        {
            None = 0,
            Immediate = 1,
            NoName = 2,
            HasCode = 4
        };

        static Stack<object> fStack = new Stack<object>(1024);
        static List<int> JmpListCur;
        static List<object> CodeListCur;

        static string Input;
        static public string InputText
        {
            set { Input = value; InputPos = 0; }
        }
        static int InputPos = -1;

        static int PC = 0;
        static object[] RuntimeCode = null;

        static int CurrentCode;
        static bool CompileFlag = false;
        static bool ClassFlag = false;

        static public TextBox tbOutput = null;
        static char DelimiterChar = 0.5.ToString()[1];

        static Stack<List<xWord>> dicStack = new Stack<List<xWord>>(32);
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
            new xWord("str", string_word, WordProperty.Immediate),

            new xWord("resetdic", resetdic, WordProperty.NoName),
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
         
            // Stack words
            new xWord("drop", drop),
            new xWord("dup", dup),
            new xWord("swap", swap),
            new xWord("over", over),
            new xWord("rot", rot),
            new xWord("-rot", nrot),

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
            new xWord("+s", splus)
        };

        static List<xWord> MainDic = CoreDic;
        static List<xWord> ClassDic;

        static int BaseLength = 0;
        //static int CodeID;
        //------------------------------------------------------------------------------
        static int Run()
        {
            try
            {
                while (PC < RuntimeCode.Length)
                {
                    CurrentCode = (int)RuntimeCode[PC++];
                    if (CurrentCode == 0)
                        CurrentCode = 0;

                    MainDic[CurrentCode].fun();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Runtime exception", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return -1;
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        //
        // Core
        //
        //------------------------------------------------------------------------------
        static public string[] UserDictionary()
        {
            if (BaseLength == 0 || BaseLength == MainDic.Count)
                return null;

            string[] dic = new string[MainDic.Count - BaseLength];

            for (int i = 0; i < MainDic.Count - BaseLength; i++)
                dic[i] = MainDic.ElementAt(BaseLength + i).name;

            return dic;
        }
        //------------------------------------------------------------------------------
        static public void Clear()
        {
            // WordCode, CodeFun
            if (BaseLength == 0)
            {
                BaseLength = MainDic.Count;
            }
            else
            {
                int cnt = MainDic.Count;

                for (int i = BaseLength; i < cnt; i++)
                {
                    MainDic.RemoveAt(BaseLength);
                }
            }

            // Stack
            fStack.Clear();
        }
        //------------------------------------------------------------------------------
        static public int Interpret(string input = null, bool isfile = false)
        {
            // Компилируем строку и выполняем
            if (input != null)
            {
                if (isfile)
                    Input = File.ReadAllText(input, Encoding.Default);
                else
                    Input = input;

                InputPos = 0;
            }

            RuntimeCode = Compile();

            PC = 0;
            return Run();
        }
        //------------------------------------------------------------------------------
        static int inumber()
        {
            int ival;

            if (int.TryParse((string)fStack.Pop(), out ival))
            {
                fStack.Push(ival);
                fStack.Push(1);
            }
            else
                fStack.Push(0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dnumber()
        {
            double dval;

            if (double.TryParse(((string)fStack.Pop()).Replace('.', DelimiterChar), out dval))
            {
                fStack.Push(dval);
                fStack.Push(1);
            }
            else
                fStack.Push(0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int setdic()
        {
            dicStack.Push(MainDic);
            MainDic = (List<xWord>)MainDic[CurrentCode].code[0];

            return 0;
        }
        //------------------------------------------------------------------------------
        static int resetdic()
        {
            MainDic = dicStack.Pop();

            return 0;
        }
        //------------------------------------------------------------------------------
        static int comment_beg()
        {
            // Комментарий
            int ival = Input.IndexOf("*/", InputPos);
            if (ival >= 0)
            {
                InputPos = ival + 2;
                return 0;
            }

            throw new InvalidOperationException("Not finishing comment");
        }
        //------------------------------------------------------------------------------
        static int compile_beg()
        {
            if (CompileFlag)
                throw new InvalidOperationException("Nested compilation");
            // Компиляция слова
            string word = Word(Input, ref InputPos);
            if (word == null)
                throw new InvalidOperationException("Compiling name absent");

            CompileFlag = true;
            int ival;
            xWord x;

            if (ClassFlag)
            {
                ClassDic.Add(new xWord(word, class_exec));
                ival = ClassDic.Count - 1;
                x = ClassDic[ival];
                x.code = Compile();
                ClassDic[ival] = x;
            }
            else
            {
                MainDic.Add(new xWord(word, exec));
                ival = MainDic.Count - 1;
                x = MainDic[ival];
                x.code = Compile();
                MainDic[ival] = x;
            }

            CompileFlag = false;

            return 0;
        }
        //------------------------------------------------------------------------------
        static int compile_end()
        {
            if (CompileFlag)
            {
                //if (ClassFlag)
                //{
                    // в классе
                //    if (CodeListCur != null)
                //        CodeListCur.Add(GetCode("resetdic"));
                //}

                fStack.Push(CodeListCur != null ? CodeListCur.ToArray() : null);
                CompileFlag = false;

                return 1;
            }

            if (ClassFlag)
            {
                ClassFlag = false;
                ClassDic = null;

                return 0;
            }

            throw new InvalidOperationException("Not started compilation");
        }
        //------------------------------------------------------------------------------
        static int if_beg()
        {
            // if - если на стеке не 0 идем дальше
            CodeListCur.Add(GetCode("jz"));
            // А вот сюда нужно будет смещение положить
            CodeListCur.Add(0);

            JmpListCur.Add(CodeListCur.Count - 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int else_word()
        {
            // ничего не компилирует, заполняет смещение
            if (JmpListCur.Count > 0)
            {
                int ival = JmpListCur[JmpListCur.Count - 1];
                //JmpList.RemoveAt(JmpList.Count - 1);

                if ((int)CodeListCur[ival] == 0)
                {
                    CodeListCur[ival] = CodeListCur.Count - ival + 2;
                    CodeListCur.Add(GetCode("jmp"));
                    CodeListCur.Add(0);
                    JmpListCur[JmpListCur.Count - 1] = CodeListCur.Count - 1;
                    return 0;
                }
            }

            throw new InvalidOperationException("Branch word without begin part or incorrect");
        }
        //------------------------------------------------------------------------------
        static int while_beg()
        {
            CodeListCur.Add(GetCode("jz_peek"));
            CodeListCur.Add(1);

            JmpListCur.Add(CodeListCur.Count - 1);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int end_word()
        {
            // ничего не компилирует, заполняет смещение

            if (JmpListCur.Count > 0)
            {
                int ival = JmpListCur[JmpListCur.Count - 1];
                JmpListCur.RemoveAt(JmpListCur.Count - 1);
                if ((int)CodeListCur[ival] == 0)
                {
                    CodeListCur[ival] = CodeListCur.Count - ival;
                    return 0;
                }
                if ((int)CodeListCur[ival] == 1)
                {
                    CodeListCur[ival] = CodeListCur.Count - ival + 2;
                    CodeListCur.Add(GetCode("jmp"));   // jmp
                    CodeListCur.Add(ival - CodeListCur.Count - 1);     // To begin while
                    return 0;
                }
            }

            throw new InvalidOperationException("End without begin part or incorrect");
        }
        //------------------------------------------------------------------------------
        static int string_word()
        {
            string word = Word(Input, ref InputPos);

            if (word[0] == '\"' && word[word.Length - 1] == '\"')
            {
                // Строка?
                CodeListCur.Add(GetCode("spush"));
                CodeListCur.Add(word.Substring(1, word.Length - 2));
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int class_beg()
        {
            if (ClassFlag || CompileFlag)
                throw new InvalidOperationException("Nested class/compilation definition");

            // Компиляция класса
            ClassFlag = true;

            string word = Word(Input, ref InputPos);
            if (word == null)
                throw new InvalidOperationException("Class name absent");

            ClassDic = new List<xWord>();
            object[] obj = new object[1];
            obj[0] = ClassDic;
            MainDic.Add(new xWord(word, setdic, WordProperty.None, obj));

            return 0;
        }
        //------------------------------------------------------------------------------
        static object[] Compile()
        {
            if(BaseLength == 0)
                BaseLength = MainDic.Count;

            List<int> JmpList = new List<int>();
            List<object> CodeList = new List<object>();

            List<int> JmpCur = JmpListCur;
            JmpListCur= JmpList;
            List<object> CodeCur = CodeListCur;
            CodeListCur = CodeList;

            string word;

            int ival;
            double dval;

            while ((word = Word(Input, ref InputPos)) != null)
            {
                if (word == "format")
                    ival = 0;

                if (int.TryParse(word, out ival))
                {
                    CodeList.Add(GetCode("ipush"));
                    CodeList.Add(ival);
                    continue;
                }

                if (double.TryParse(word.Replace('.', DelimiterChar), out dval))
                {
                    CodeList.Add(GetCode("dpush"));
                    CodeList.Add(dval);
                    continue;
                }

                if ((ival = GetCode(word)) >= 0)
                {
                    //if((CoreDic[ival].prop & WordProperty.Immediate) != WordProperty.None)

                    if ((MainDic[ival].prop & WordProperty.Immediate) != WordProperty.None)
                    {
                        // Слово немедленного исполнения
                        //ival = CoreDic[ival].fun();
                        ival = MainDic[ival].fun();

                        if (ival == 1)
                        {
                            JmpListCur = JmpCur;
                            CodeListCur = CodeCur;

                            return (object[])fStack.Pop();
                        }

                        continue;
                    }

                    if(MainDic[ival].code != null && MainDic[ival].code[0].GetType() == typeof(List<xWord>))
                    {
                        dicStack.Push(MainDic);
                        MainDic = (List<xWord>)MainDic[ival].code[0];
                    }
                    else if (dicStack.Count > 0)
                    {
                        MainDic = dicStack.Pop();
                    }

                  
                    CodeList.Add(ival);

                    continue;
                }

                // Косяк
                throw new InvalidOperationException("Not valid word");
            }

            JmpListCur = JmpCur;
            CodeListCur = CodeCur;

            return CodeList != null ? CodeList.ToArray() : null;
        }
        //------------------------------------------------------------------------------
        static int cls()
        {
            tbOutput.Clear();
            return 0;
        }
        //------------------------------------------------------------------------------
        static int ipush()
        {
            fStack.Push((int)RuntimeCode[PC++]);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dpush()
        {
            fStack.Push((double)RuntimeCode[PC++]);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int spush()
        {
            fStack.Push((string)RuntimeCode[PC++]);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int jmp()
        {
            PC += (int)RuntimeCode[PC];

            return 0;
        }
        //------------------------------------------------------------------------------
        static int jz()
        {
            if ((int)fStack.Pop() != 0)
                PC++;
            else
                PC += (int)RuntimeCode[PC];

            return 0;
        }
        //------------------------------------------------------------------------------
        static int jz_peek()
        {
            if ((int)fStack.Peek() != 0)
                PC++;
            else
                PC += (int)RuntimeCode[PC];

            return 0;
        }
        //------------------------------------------------------------------------------
        static int word()
        {
            fStack.Push(Word(Input, ref InputPos));

            return 0;
        }
        //------------------------------------------------------------------------------
        static int iprint()
        {
            tbOutput.Text = tbOutput.Text + ((int)fStack.Pop()).ToString() + Environment.NewLine;
            tbOutput.Select(tbOutput.Text.Length, 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int uprint16()
        {
            uint hi = (uint)(int)fStack.Pop();
            tbOutput.Text = tbOutput.Text + string.Format("{0:X}", hi) + Environment.NewLine;
            tbOutput.Select(tbOutput.Text.Length, 0);
            
            return 0;
        }
        //------------------------------------------------------------------------------
        static int dprint()
        {
            tbOutput.Text = tbOutput.Text + ((double)fStack.Pop()).ToString() + Environment.NewLine;
            tbOutput.Select(tbOutput.Text.Length, 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int sprint()
        {
            tbOutput.Text = tbOutput.Text + (string)fStack.Pop() + Environment.NewLine;
            tbOutput.Select(tbOutput.Text.Length, 0);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int atoi()
        {
            fStack.Push(PopIntEx());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int atod()
        {
            fStack.Push(PopDoubleEx());

            return 0;
        }
        //------------------------------------------------------------------------------
        static int find()
        {
            string word;

            if (fStack.Peek().GetType() == typeof(string))
                word = (string)fStack.Pop();
            else
                throw new InvalidOperationException("Несоответствие типов (string)");

            fStack.Push(GetCode(word));

            return 0;
        }
        //------------------------------------------------------------------------------
        static int exec()
        {
            int oldPC = PC;
            object[] oldRuntimeCode = RuntimeCode;
            PC = 0;
            RuntimeCode = MainDic[CurrentCode].code;

            Run();

            PC = oldPC;
            RuntimeCode = oldRuntimeCode;

            return 0;
        }
        //------------------------------------------------------------------------------
        static int class_exec()
        {
            int oldPC = PC;
            object[] oldRuntimeCode = RuntimeCode;
            PC = 0;
            RuntimeCode = MainDic[CurrentCode].code;
            MainDic = dicStack.Pop();

            Run();

            PC = oldPC;
            RuntimeCode = oldRuntimeCode;

            return 0;
        }
        //------------------------------------------------------------------------------
        // Convertation
        //------------------------------------------------------------------------------
        static int toi()
        {
            object val = fStack.Pop();

            if (val.GetType() == typeof(int))
            {
                fStack.Push(val);
                return 0;
            }

            if (val.GetType() == typeof(double))
            {
                fStack.Push((int)(double)val);
                return 0;
            }

            if (val.GetType() == typeof(string))
            {
                int i = 0;
                if (int.TryParse((string)val, out i))
                    return 0;

                fStack.Push(i);
            }

            throw new FormatException("Ошибка преобразования (integer)");
        }
        //------------------------------------------------------------------------------
        static int tod()
        {
            object val = fStack.Pop();

            if (val.GetType() == typeof(double))
            {
                fStack.Push(val);
                return 0;
            }

            if (val.GetType() == typeof(int))
            {
                fStack.Push((double)(int)val);
                return 0;
            }

            if (val.GetType() == typeof(string))
            {
                double d = 0;
                if (double.TryParse(((string)val).Replace('.', DelimiterChar), out d))
                    return 0;

                fStack.Push(d);
            }

            throw new FormatException("Ошибка преобразования (double)");
        }
        //------------------------------------------------------------------------------
        static int array()
        {
            int capacity = PopInt();

            object[] arr = new object[capacity];
            fStack.Push(arr);

            return 0;
        }
        //------------------------------------------------------------------------------
        static int PopInt()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(int))
                return (int)fStack.Pop();

            throw new InvalidOperationException("Not valid type (integer)");
        }
        //------------------------------------------------------------------------------
        static double PopDouble()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(double))
                return (double)fStack.Pop();

            throw new InvalidOperationException("Not valid type (double)");
        }
        //------------------------------------------------------------------------------
        static string PopString()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(string))
                return (string)fStack.Pop();

            throw new InvalidOperationException("Not valid type (string)");
        }
        //------------------------------------------------------------------------------
        static int PopIntEx()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(int))
                return (int)fStack.Pop();

            int val = 0;

            if (int.TryParse(obj.ToString(), out val) == true)
            {
                fStack.Pop();
                return val;
            }

            throw new InvalidOperationException("Not valid conversion (integer)");
        }
        //------------------------------------------------------------------------------
        static double PopDoubleEx()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(double))
                return (double)fStack.Pop();

            double val = 0;

            if (double.TryParse(obj.ToString().Replace('.', DelimiterChar), out val) == true)
            {
                fStack.Pop();
                return val;
            }

            throw new InvalidOperationException("Not valid conversion (double)");
        }
        //------------------------------------------------------------------------------
        static string PopStringEx()
        {
            object obj = fStack.Peek();

            if (obj.GetType() == typeof(string))
                return (string)fStack.Pop();

            return fStack.Pop().ToString();
        }
        //------------------------------------------------------------------------------
        static int GetCode(string word)
        {
            int id;

            //WordCode.TryGetValue(word, out code);

            //IEnumerable<xWord> ienum = MainDic.Where(e => e.name == word);
            //if (ienum != null && ienum.Count() > 0)
            //    id = ienum.

            for(id = MainDic.Count - 1; id >= 0; id--)
                if (MainDic[id].name == word) break;

            return id;
        }
        //------------------------------------------------------------------------------
        /*
        static ForthWord GetExec(int id)
        {
            ForthWord exec = null;

            //CodeFun.TryGetValue(code, out exec);
            IEnumerable<xWord> ienum = MainDic.Where(e => e.id == id);
            if (ienum != null && ienum.Count() > 0)
                exec = ienum.Last().fun;

            return exec;
        }
        */
        //------------------------------------------------------------------------------
        static int ExecWord(string word)
        {
            int code;

            if((code = GetCode(word)) != 0)
            {
                //ForthWord exec;
                //if ((exec = GetExec(code)) != null)
                //    exec();
                //else
                //    return 0;
                MainDic[code].fun();
            }

            return code;
        }
        //------------------------------------------------------------------------------
        static string Word(string stream, ref int curpos)
        {
            if (stream == null || stream.Length == 0 || curpos < 0 || curpos >= stream.Length)
                return null;

            while (curpos < stream.Length)
            {
                if (char.IsWhiteSpace(stream[curpos]) == false) break;
                curpos++;
            }

            int pos = curpos;
            bool isstr = false;
            
            if(curpos < stream.Length && stream[curpos] == '\"')
            {
                isstr = true;
                curpos++;
            }

            //string s = stream.Substring(pos);

            while (curpos < stream.Length)
            {
                if (!isstr)
                {
                    if (char.IsWhiteSpace(stream[curpos]) == true) break;
                }
                else
                if (curpos > 0 && stream[curpos] == '\"') break;

                curpos++;
            }

            if (isstr) curpos++;
            if(curpos > pos)
                return stream.Substring(pos, curpos - pos);

            return null;
        }
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
