﻿//------------------------------------------------------------------------------
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Data.OleDb;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
//------------------------------------------------------------------------------
namespace csforth
{
    public partial class Form1 : Form
    {
        //------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();

            Core.tbOutput = tbOut;

            Core.InputText =
                " : my -7.5 dup >i ; : my2 my ; : my3 my2 ; my3 if \" Hello, world!\" 3.14 -11 >d /d .d .s end >i .";
            //"1000 while 1 - dup . end";
            Core.Interpret();

            Core.Interpret("1 dup if . end 111");
            Core.Interpret(" . 1000 while 1 - dup . end");
            Core.Interpret("26.8 1076.456 *d .d");

            Core.Clear();

            Core.Interpret("cls \" Ok!\" .s");

        }
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------