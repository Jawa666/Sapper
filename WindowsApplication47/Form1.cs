using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sapper
{
    public partial class Form1 : Form
    {
        private const int
          MR = 10, // кол-во клеток по вертикали
          MC = 10, // кол-во клеток по горизонтали
          NM = 10, // кол-во мин
          W = 50,  // ширина клетки
          H = 40;  // высота клетки
        private  int chet = 0;   //Счетчик открытых клеток

        // игровое (минное) поле
        private int[,] Pole = new int[MR + 2, MC + 2];
        // значение элемента массива:
        // 0..8 - количество мин в соседних клетках
        // 9 - в клетке мина
        // 100..109 - клетка открыта
        // 200..209 - в клетку поставлен флаг

        private int nMin;  // кол-во найденных мин
        private int nFlag; // кол-во поставленных флагов

        // статус игры
        private int status;
        // 0 - начало игры,
        // 1 - игра,
        // 2 – результат

        // графическая поверхность формы
        private System.Drawing.Graphics g;

        public Form1()
        {
            InitializeComponent();

            //В неиспользуемых полях запишем значение -3, это значение используется методом open
            //для поиска пустых клеток
            for (int row = 0; row <= MR + 1; row++)
            {
                Pole[row, 0] = -3;
                Pole[row, MC + 1] = -3;
            }

            for (int col = 0; col <= MC + 1; col++)
            {
                Pole[0, col] = -3;
                Pole[MR + 1, col] = -3;
            }

            // устанавливаем размер формы в соответствии
            // с размером игрового поля
            this.ClientSize = new Size(W * MC + 1, H * MR + menuStrip1.Height + 1);

            newGame(); // новая игра

            // графическая поверхность
            g = panel1.CreateGraphics();
        }

        private void newGame()
        {
            int row, col;    // индексы клетки
            int n = 0;       // количество поставленных мин
            int k;           // кол-во мин в соседних клетках
            chet = 0;        // обнуляем счетчик клеток

            // очистить поле
            for (row = 1; row <= MR; row++)
                for (col = 1; col <= MC; col++)
                    Pole[row, col] = 0;

            Random rnd = new Random();

            // расставим мины
            do
            {
                row = rnd.Next(MR) + 1;
                col = rnd.Next(MC) + 1;

                if (Pole[row, col] != 9)
                {
                    Pole[row, col] = 9;
                    n++;
                }
            }
            while (n != NM);

            // для каждой клетки вычислим кол-во 
            // мин в соседних клетках
            for (row = 1; row <= MR; row++)
                for (col = 1; col <= MC; col++)
                    if (Pole[row, col] != 9)
                    {
                        k = 0;

                        if (Pole[row - 1, col - 1] == 9) k++;
                        if (Pole[row - 1, col] == 9) k++;
                        if (Pole[row - 1, col + 1] == 9) k++;
                        if (Pole[row, col - 1] == 9) k++;
                        if (Pole[row, col + 1] == 9) k++;
                        if (Pole[row + 1, col - 1] == 9) k++;
                        if (Pole[row + 1, col] == 9) k++;
                        if (Pole[row + 1, col + 1] == 9) k++;

                        Pole[row, col] = k;
                    }

            status = 0;      // начало игры
            nMin = 0;        // нет обнаруженных мин
            nFlag = 0;       // нет поставленных флагов
        }      
        // рисует поле
        private void showPole(Graphics g, int status)
        {
            for (int row = 1; row <= MR; row++)
                for (int col = 1; col <= MC; col++)
                    this.kletka(g, row, col, status);
        }

        // рисует клетку
        private void kletka(Graphics g, int row, int col, int status)
        {
            int x, y;// координаты левого верхнего угла клетки

            x = (col - 1) * W + 1;
            y = (row - 1) * H + 1;

            // не открытые клетки - серые
            if (Pole[row, col] < 100)
                g.FillRectangle(SystemBrushes.ControlLight, x - 1, y - 1, W, H);

            // открытые или помеченные клетки
            if (Pole[row, col] >= 100)
            {

                // открываем клетку, открытые - белые
                if (Pole[row, col] != 109)
                    g.FillRectangle(Brushes.White, x - 1, y - 1, W, H);
                else
                    // на этой мине подорвались!
                    g.FillRectangle(Brushes.Red, x - 1, y - 1, W, H);

                // если в соседних клетках есть мины,
                // указываем их количество
                if ((Pole[row, col] >= 101) && (Pole[row, col] <= 108))
                    g.DrawString((Pole[row, col] - 100).ToString(), new Font("Tahoma", 15,
                        System.Drawing.FontStyle.Regular),
                        Brushes.Blue, x + 15, y + 5);
            }

            // в клетке поставлен флаг
            if (Pole[row, col] >= 200)
                this.flag(g, x, y);

            // рисуем границу клетки
            g.DrawRectangle(Pens.Black,
                x - 1, y - 1, W, H);

            // если игра завершена (status = 2),
            // показываем мины
            if ((status == 2) && ((Pole[row, col] % 10) == 9))
                this.mina(g, x, y);
        }

        // открывает текущую и все соседние с ней клетки,
        // в которых нет мин
        private void open(int row, int col)
        {
            
            // координаты области вывода
            int x = (col - 1) * W + 1,
                y = (row - 1) * H + 1;

            if (Pole[row, col] == 0)
            {
                chet++;     // записываем открытую клетку
                Pole[row, col] = 100;

                // отобразить содержимое клетки
                this.kletka(g, row, col, status);

                // открыть примыкающие клетки
                // слева, справа, сверху, снизу
                this.open(row, col - 1);
                this.open(row - 1, col);
                this.open(row, col + 1);
                this.open(row + 1, col);

                //примыкающие диагонально
                this.open(row - 1, col - 1);
                this.open(row - 1, col + 1);
                this.open(row + 1, col - 1);
                this.open(row + 1, col + 1);
            }
            else
                if ((Pole[row, col] < 100) && (Pole[row, col] != -3))
                {
                    chet++;
                    Pole[row, col] += 100;

                    // отобразить содержимое клетки
                    this.kletka(g, row, col, status);
                }
        }

        // рисует мину
        private void mina(Graphics g, int x, int y)
        {
            // корпус
            g.FillRectangle(Brushes.Green,
                x + 16, y + 26, 8, 4);
            g.FillRectangle(Brushes.Green,
                x + 8, y + 30, 24, 4);
            g.DrawPie(Pens.Black,
                x + 6, y + 28, 28, 16, 0, -180);
            g.FillPie(Brushes.Green,
                x + 6, y + 28, 28, 16, 0, -180);

            // полоса на корпусе
            g.DrawLine(Pens.Black,
                x + 12, y + 32, x + 28, y + 32);

            // вертикальный "ус"
            g.DrawLine(Pens.Black,
                x + 20, y + 22, x + 20, y + 26);

            // боковые "усы"
            g.DrawLine(Pens.Black,
                x + 8, y + 30, x + 6, y + 28);
            g.DrawLine(Pens.Black,
                x + 32, y + 30, x + 34, y + 28);
        }

        // рисует флаг
        private void flag(Graphics g, int x, int y)
        {
            Point[] p = new Point[3];

            // флажок
            p[0].X = x + 4; p[0].Y = y + 4;
            p[1].X = x + 30; p[1].Y = y + 12;
            p[2].X = x + 4; p[2].Y = y + 20;
            g.FillPolygon(Brushes.Red, p);

            // древко
            g.DrawLine(Pens.Black,
                x + 4, y + 4, x + 4, y + 35);         
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
         
        }

        // щелчок кнопкой в клетке игрового поля
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            // игра завершена
            if (status == 2)
            {
                MessageBox.Show("Игра завершена.");
                return;
            }

            // первый щелчок
            if (status == 0) status = 1;

            // преобразуем координаты мыши в индексы
            // клетки поля, в которой был сделан щелчок;
            // (e.X, e.Y) - координаты точки формы,
            // в которой была нажата кнопка мыши;            
            int row = (int)(e.Y / H) + 1,
                col = (int)(e.X / W) + 1;

            // координаты области вывода
            int x = (col - 1) * W + 1,
                y = (row - 1) * H + 1;

            // щелчок левой кнопки мыши
            if (e.Button == MouseButtons.Left)
            {
                // открыта клетка, в которой есть мина                   
                if (Pole[row, col] == 9)
                {
                    Pole[row, col] += 100;

                    // игра закончена
                    status = 2;

                    // перерисовать игровое поле
                    this.panel1.Invalidate();
                    MessageBox.Show("Вы проиграли.");
                }
                else                
                    if (Pole[row, col] < 9) this.open(row, col);
               
                // Если найдены все мины, вы победитель
                if (chet % (MR * MC - NM) == 0) MessageBox.Show("Вы выйграли!!!");
            }

            // щелчок правой кнопки мыши
            if (e.Button == MouseButtons.Right)
            {

                // в клетке не было флага, ставим его
                if (Pole[row, col] <= 9)
                {
                    nFlag += 1;

                    if (Pole[row, col] == 9)
                        nMin += 1;

                    Pole[row, col] += 200;

                    if ((nMin == NM) && (nFlag == NM))
                    {
                        // игра закончена
                        status = 2;

                        // перерисовываем все игровое поле
                        this.panel1.Invalidate();
                        MessageBox.Show("Вы выйграли!!!");
                    }
                    else
                        // перерисовываем только клетку
                        this.kletka(g, row, col, status);
                }
                else
                    // в клетке был поставлен флаг,
                    // повторный щелчок правой кнопки мыши
                    // убирает его и закрывает клетку
                    if (Pole[row, col] >= 200)
                    {
                        nFlag -= 1;
                        Pole[row, col] -= 200;

                        // перерисовываем клетку                
                        this.kletka(g, row, col, status);
                    }
            }
        }

        // команда Новая игра
        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newGame();
            showPole(g, status);
        }

        // обработка события Paint панели
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            showPole(g, status);
        }

        // выбор в меню Справка команды О программе
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 aboutBox = new Form2();
            aboutBox.ShowDialog();
        }
    }
}
