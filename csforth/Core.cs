//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
//------------------------------------------------------------------------------
namespace csforth
{
    //------------------------------------------------------------------------------
    static class Core
    {
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

        static public TextBox tbOutput = null;
        static char DelimiterChar = 0.5.ToString()[1];
        //------------------------------------------------------------------------------
        static private Dictionary<string, int> WordCode = new Dictionary<string, int>
        {
            { "ipush", -1 },
            { "dpush", -2 },
            { "spush", -3 },
            { "jz", -4 },

            { "word", 1 },
            { "find", 2 },
            { "exec", 3 },
            { "drop", 4 },
            { "a>i", 5 },
            { "a>d", 6 },
            { "+", 7 },
            { "+d", 8 },
            { ">i", 9 },
            { ">d", 10 },
            { ".", 11 },
            { ".d", 12 },
            { ".s", 13 },
            { "dup", 14 },
            { "/d", 20 },
            { "*d", 21 }
        };

        static private Dictionary<int, ForthWord> CodeFun = new Dictionary<int, ForthWord>
        {
            { -1, ipush },
            { -2, dpush },
            { -3, spush },
            { -4, jz },

            { 1, word },
            { 2, find },
            { 3, exec },
            { 4, drop },
            { 5, atoi },
            { 6, atod },
            { 7, iplus },
            { 8, dplus },
            { 9, toi },
            { 10, tod },
            { 11, iprint },
            { 12, dprint },
            { 13, sprint },
            { 14, dup },
            { 20, ddiv },
            { 21, dmul }
        };

        static int CodeID = 22;

        static private Dictionary<int, object[]> Code = new Dictionary<int, object[]>();
        //------------------------------------------------------------------------------
        static int Run()
        {
            try
            {
                ForthWord fw;

                while (PC < RuntimeCode.Length)
                {
                    CurrentCode = (int)RuntimeCode[PC++];

                    if ((fw = GetExec(CurrentCode)) != null)
                    {
                        fw();
                    }
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
        // Core, version 1.0
        //
        //------------------------------------------------------------------------------
        static public int Interpret(string input = null)
        {
            // Компилируем строку и выполняем
            if(input != null)
            {
                Input = input;
                InputPos = 0;
            }

            RuntimeCode = Compile();
            PC = 0;
            return Run();
        }
        //------------------------------------------------------------------------------
        static object[] Compile()
        {
            List<int> JmpList = new List<int>();
            List<object> CodeList = new List<object>();
            string word;

            int ival;
            double dval;

            while ((word = Word(Input, ref InputPos)) != null)
            {
                if (word == ":")
                {
                    if (CompileFlag)
                        throw new InvalidOperationException("Nested compilation");
                    // Компиляция слова
                    word = Word(Input, ref InputPos);
                    if (word == null)
                        throw new InvalidOperationException("Compiling name absent");

                    WordCode.Add(word, CodeID);
                    CodeFun.Add(CodeID, exec);

                    CompileFlag = true;
                    object[] arWC = Compile();
                    Code.Add(CodeID++, arWC);
                    CompileFlag = false;
                    continue;
                }

                if (word == ";")
                {
                    if (!CompileFlag)
                        throw new InvalidOperationException("Not started compilation");

                    return CodeList != null ? CodeList.ToArray() : null;
                }

                if (word == "if")
                {
                    // if - если на стеке не 0 идем дальше
                    CodeList.Add(-4);
                    // А вот сюда нужно будет смещение положить
                    CodeList.Add(0);

                    JmpList.Add(CodeList.Count - 1);
                    continue;
                }

                if (word == "end")
                {
                    // ничего не компилирует, заполняет смещение
                    if (JmpList.Count > 0)
                    {
                        ival = JmpList[JmpList.Count - 1];
                        CodeList[ival] = CodeList.Count - ival;
                        JmpList.RemoveAt(JmpList.Count - 1);
                        continue;
                    }

                    throw new InvalidOperationException("End without if");
                }

                if (word == "\"")
                {
                    // Строка
                    ival = Input.IndexOf("\"", InputPos);
                    if (ival >= 0)
                    {
                        word = Input.Substring(InputPos, ival - InputPos).TrimStart();
                        CodeList.Add(-3);
                        CodeList.Add(word);
                        InputPos = ival + 1;
                        continue;
                    }

                    // Косяк
                    throw new InvalidOperationException("Not valid type (literal)");
                }

                if (int.TryParse(word, out ival))
                {
                    CodeList.Add(-1);
                    CodeList.Add(ival);
                    continue;
                }

                if (double.TryParse(word.Replace('.', DelimiterChar), out dval))
                {
                    CodeList.Add(-2);
                    CodeList.Add(dval);
                    continue;
                }

                if ((ival = GetCode(word)) != 0)
                {
                    CodeList.Add(ival);
                    continue;
                }

                // Косяк
                throw new InvalidOperationException("Not valid word");
            }

            return CodeList != null ? CodeList.ToArray() : null;
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
            object[] arWC;

            if (Code.TryGetValue(CurrentCode, out arWC))
            {
                // Выполнить код arWC
                int oldPC = PC;
                object[] oldRuntimeCode = RuntimeCode;
                PC = 0;
                RuntimeCode = arWC;

                Run();

                PC = oldPC;
                RuntimeCode = oldRuntimeCode;
            }

            return 0;
        }
        //------------------------------------------------------------------------------
        static int drop()
        {
            fStack.Pop();

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dup()
        {
            fStack.Push(fStack.Peek());

            return 0;
        }
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
        static int iplus()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(int) && lo.GetType() == typeof(int))
            {
                fStack.Push((int)lo + (int)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (integer)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int iminus()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(int) && lo.GetType() == typeof(int))
            {
                fStack.Push((int)lo - (int)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (integer)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int imul()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(int) && lo.GetType() == typeof(int))
            {
                fStack.Push((int)lo * (int)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (integer)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int idiv()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(int) && lo.GetType() == typeof(int))
            {
                fStack.Push((int)lo / (int)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (integer)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dplus()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(double) && lo.GetType() == typeof(double))
            {
                fStack.Push((double)lo + (double)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (double)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dminus()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(double) && lo.GetType() == typeof(double))
            {
                fStack.Push((double)lo - (double)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (double)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int dmul()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(double) && lo.GetType() == typeof(double))
            {
                fStack.Push((double)lo * (double)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (double)");

            return 0;
        }
        //------------------------------------------------------------------------------
        static int ddiv()
        {
            object hi = fStack.Pop();
            object lo = fStack.Pop();

            if (hi.GetType() == typeof(double) && lo.GetType() == typeof(double))
            {
                fStack.Push((double)lo / (double)hi);
            }
            else
                throw new InvalidOperationException("Not valid type (double)");

            return 0;
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
            int code = 0;

            WordCode.TryGetValue(word, out code);

            return code;
        }
        //------------------------------------------------------------------------------
        static ForthWord GetExec(int code)
        {
            ForthWord exec = null;

            CodeFun.TryGetValue(code, out exec);

            return exec;
        }
        //------------------------------------------------------------------------------
        static int ExecWord(string word)
        {
            int code;

            if((code = GetCode(word)) != 0)
            {
                ForthWord exec;
                if ((exec = GetExec(code)) != null)
                    exec();
                else
                    return 0;
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

            while (curpos < stream.Length)
            {
                if (char.IsWhiteSpace(stream[curpos]) == true) break;
                curpos++;
            }

            return stream.Substring(pos, curpos - pos);
        }
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------
