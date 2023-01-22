using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeServerSide.Services;

namespace TicTacToeServerSide
{
    public class Program
    {
        static void Main(string[] args)
        {
            NetworkService.Start();
        }
    }
}
