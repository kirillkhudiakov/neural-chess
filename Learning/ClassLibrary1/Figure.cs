using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingLib
{
    public class Figure
    {
        public string Name { get; set; }
        // Белые 1, черные -1
        public int Side { get; set; }
        // Существует 1, не существует 0 
        public int Exist { get; set; }
        // Позиция по горизонтали, только координаты a..h заменены на 1..8
        public int PositionX { get; set; }
        // Позиция по вертикали 1..8
        public int PositionY { get; set; }
        // Если фигуры не существует, следует указывать позицию (0, 0)

        public Figure(string name, int side, int exist, int posX, int posY)
        {
            Name = name;
            Side = side;
            Exist = exist;
            PositionX = posX;
            PositionY = posY;
        }
    }
}
