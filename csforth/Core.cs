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

        static Stack<object> fStack = new Stack<object>(1024);

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
            //public int id;
            public string name;
            public ForthWord fun;
            public object[] code;
            //public List<xWord> sub;

            public xWord(string n, ForthWord f, object[] c = null) { name = n; fun = f; code = c; }
            //public xWord(string n, ForthWord f, List<xWord> s) { name = n; fun = f; code = null; sub = s; }
        };

        static List<xWord> sysDic = new List<xWord>
        {
            new xWord("resetdic", resetdic),
            new xWord("ipush", ipush ),
            new xWord("dpush", dpush),
            new xWord("spush", spush),
            new xWord("jz", jz),
            new xWord("jz_peek", jz_peek),
            new xWord("jmp", jmp),

            new xWord("word", word),
            new xWord("find", find),
            new xWord("exec", exec),

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

        static List<xWord> MainDic = sysDic;
        static List<xWord> ClassDic;

        static int BaseLength = 0;
        //static int CodeID;
        //------------------------------------------------------------------------------
        static int Run()
        {
            try
            {
                //ForthWord fw;

                while (PC < RuntimeCode.Length)
                {
                    CurrentCode = (int)RuntimeCode[PC++];

                    MainDic[CurrentCode].fun();

                    //if ((fw = MainDic[CurrentCode].fun) != null)
                    //{
                    //    fw();
                    //}
                }
            }
            catch(Exception ex)
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
                    //CodeFun.Remove(WordCode.Values.ElementAt(BaseLength));
                    //WordCode.Remove(WordCode.Keys.ElementAt(BaseLength));
                    MainDic.RemoveAt(BaseLength);
                }
            }

            //CodeID = CodeFun.Keys.Max() + 1;
            //CodeID = MainDic.Max(e => e.id) + 1;
           
            // Code
            //Code.Clear();

            // Stack
            fStack.Clear();
        }
        //------------------------------------------------------------------------------
        static public int Interpret(string input = null, bool isfile = false)
        {
            // Компилируем строку и выполняем
            if(input != null)
            {
                if(isfile)
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
        static object[] Compile()
        {
            //static int BaseLength = 0;
            //static int CodeID;
            // WordCode, CodeFun
            if(BaseLength == 0)
            {
                BaseLength = MainDic.Count;
                //CodeID = CodeFun.Keys.Max() + 1;
                //CodeID = MainDic.Max(e => e.id) + 1;
            }

            List<int> JmpList = new List<int>();
            List<object> CodeList = new List<object>();
            string word;

            int ival;
            double dval;

            while ((word = Word(Input, ref InputPos)) != null)
            {
                if(word == "/*")
                {
                    // Комментарий
                    ival = Input.IndexOf("*/", InputPos);
                    if(ival >= 0)
                    {
                        InputPos = ival + 2;
                        continue;
                    }

                    throw new InvalidOperationException("Not finishing comment");
                }

                if (word == ":")
                {
                    if (CompileFlag)
                        throw new InvalidOperationException("Nested compilation");
                    // Компиляция слова
                    word = Word(Input, ref InputPos);
                    if (word == null)
                        throw new InvalidOperationException("Compiling name absent");

                    //WordCode.Add(word, CodeID);
                    //CodeFun.Add(CodeID, exec);
                    
                    
                    CompileFlag = true;
                    xWord x;

                    if (ClassFlag)
                    {
                        ClassDic.Add(new xWord(word, exec));
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
                    //object[] arWC = Compile();
                    //xWord x = new xWord(
                    //    MainDic[ival].name,
                    //    MainDic[ival].id,
                    //    MainDic[ival].fun,
                    //    arWC);

                    
                    

                   
                 
                    //MainDic[MainDic.Count - 1].code = Compile();
                    //Code.Add(CodeID++, arWC);
                    
                    CompileFlag = false;
                    continue;
                }

                if(word == "class")
                {
                    if(ClassFlag || CompileFlag)
                        throw new InvalidOperationException("Nested class definition");

                    // Компиляция класса
                    ClassFlag = true;

                    word = Word(Input, ref InputPos);
                    if (word == null)
                        throw new InvalidOperationException("Class name absent");

                    ClassDic = new List<xWord>();
                    object[] obj = new object[1];
                    obj[0] = ClassDic;
                    MainDic.Add(new xWord(word, setdic, obj));
                    continue;
                }

                if (word == ";")
                {
                    if (CompileFlag)
                    {
                        if(ClassFlag)
                        {
                            // в классе
                            if (CodeList != null)
                                CodeList.Add(GetCode("resetdic"));
                            //ClassDic = null;
                        }
                        return CodeList != null ? CodeList.ToArray() : null;
                    }

                    if(ClassFlag)
                    {
                        ClassFlag = false;
                        ClassDic = null;

                        continue;
                    }

                    
                    throw new InvalidOperationException("Not started compilation");
                }

                if (word == "if")
                {
                    // if - если на стеке не 0 идем дальше
                    CodeList.Add(GetCode("jz"));
                    // А вот сюда нужно будет смещение положить
                    CodeList.Add(0);

                    JmpList.Add(CodeList.Count - 1);
                    continue;
                }

                if(word == "else")
                {
                    // ничего не компилирует, заполняет смещение
                    if (JmpList.Count > 0)
                    {
                        ival = JmpList[JmpList.Count - 1];
                        //JmpList.RemoveAt(JmpList.Count - 1);

                        if ((int)CodeList[ival] == 0)
                        {
                            CodeList[ival] = CodeList.Count - ival + 2;
                            CodeList.Add(GetCode("jmp"));
                            CodeList.Add(0);
                            JmpList[JmpList.Count - 1] = CodeList.Count - 1;
                            continue;
                        }
                    }

                    throw new InvalidOperationException("Branch word without begin part or incorrect");
                }

                if (word == "while")
                {
                    CodeList.Add(GetCode("jz_peek"));
                    CodeList.Add(1);

                    JmpList.Add(CodeList.Count - 1);
                    continue;
                }

                if (word == "end")
                {
                    // ничего не компилирует, заполняет смещение
                    if (JmpList.Count > 0)
                    {
                        ival = JmpList[JmpList.Count - 1];
                        JmpList.RemoveAt(JmpList.Count - 1);
                        if ((int)CodeList[ival] == 0)
                        {
                            CodeList[ival] = CodeList.Count - ival;
                            continue;
                        }
                        if ((int)CodeList[ival] == 1)
                        {
                            CodeList[ival] = CodeList.Count - ival + 2;
                            CodeList.Add(GetCode("jmp"));   // jmp
                            CodeList.Add(ival - CodeList.Count - 1);     // To begin while
                            continue;
                        }
                    }

                    throw new InvalidOperationException("End without begin part or incorrect");
                }

                if (word[0] == '\"' && word[word.Length - 1] == '\"')
                {
                    // Строка?
                    CodeList.Add(GetCode("spush"));
                    CodeList.Add(word.Substring(1, word.Length - 2));
                    continue;

                    /*
                    ival = Input.IndexOf("\"", InputPos);
                    if (ival >= 0)
                    {
                        InputPos = ival + 1;
                        ival = Input.IndexOf("\"", InputPos);
                        if (ival >= 0)
                        {
                            word = Input.Substring(InputPos, ival - InputPos);
                            CodeList.Add(-3);
                            CodeList.Add(word);
                            InputPos = ival + 1;
                            continue;
                        }
                    }

                    // Косяк
                    throw new InvalidOperationException("Not valid type (literal)");
                    */
                }

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
            // в коде
            //object[] arWC;

            //IEnumerable<xWord> ienum = MainDic.Where(e => e.id == CurrentCode);

            //if (ienum != null && ienum.Count() > 0)

            // Выполнить код arWC
            int oldPC = PC;
            object[] oldRuntimeCode = RuntimeCode;
            PC = 0;
            RuntimeCode = MainDic[CurrentCode].code;
                //ienum.Last().code;

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
