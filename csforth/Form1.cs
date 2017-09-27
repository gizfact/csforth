//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
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

            Core.Input = 
                //"drop";
                ": my -7.5 dup >i ; : my2 my ; : my3 my2 ; my3 if \" Hello, world!\" 3.14 -11 >d /d .d .s end >i .";
            Core.InputPos = 0;

            Core.RuntimeCode = Core.Compile();
            Core.PC = 0;
            Core.Run();

           
        }
        //------------------------------------------------------------------------------
    }
    //------------------------------------------------------------------------------
}
//------------------------------------------------------------------------------