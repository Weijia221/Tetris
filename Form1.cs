using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    public partial class Form1 : Form
    {
        //��Ϸ����
        private const int Cols = 10;          //��Ϸ�����ȣ�������
        private const int Rows = 20;          //��Ϸ���߶ȣ�������
        private const int CellSize = 25;      //ÿ��С��������ش�С
        //��Ϸ״̬����
        private int[,] grid = new int[Rows, Cols];//����0=�գ�1=�ѹ̶�
        private int[][] currentBlock;           //��ǰ����飨��ά���飩
        private int currentX, currentY;        //��ǰ�����������е�λ��
        private int score = 0;
        private bool isGameOver = false;
        //���з�����״
        private int[][][] blocks = new int[][][]
        {
            //һ��
            new int[][]{ new int[] {1,1,1,1} },
            //O��
            new int[][]{ new int[] {1,1},new int[] {1,1} },
            //T��
            new int[][]{ new int[] {0,1,0},new int[] {1,1,1}},
            //S��
            new int[][]{ new int[] {0,1,1},new int[] {1,1,0}},
            //Z��
            new int[][]{ new int[] {1,1,0},new int[] {0,1,1}},
            //L��
            new int[][]{ new int[] {1,0,0},new int[] {1,1,1}},
            //J��
            new int[][]{ new int[] {0,0,1},new int[] {1,1,1}}
        };
        private Random rand = new Random();

        public Form1()
        {
            InitializeComponent();
            int panelWidth = Cols * CellSize + 2;
            int panelHeight = Rows * CellSize + 4;
            gamePanel.Size = new System.Drawing.Size(panelWidth, panelHeight);
            gamePanel.Location = new System.Drawing.Point(30, 12);
            gamePanel.TabStop = false;
            gamePanel.Paint += GamePanel_Paint;
            // 动态设置窗口大小，确保底部有足够空间
            this.ClientSize = new System.Drawing.Size(panelWidth + 180, gamePanel.Bottom + 60);
            this.StartPosition = FormStartPosition.CenterScreen;
            gameTimer.Interval = 500;
            gameTimer.Tick += GameTimer_Tick;
            startButton.Click += StartButton_Click;
            // 调整右侧控件位置
            scoreLabel.Location = new System.Drawing.Point(gamePanel.Right + 30, gamePanel.Top + 10);
            statusLabel.Location = new System.Drawing.Point(gamePanel.Right + 30, scoreLabel.Bottom + 15);
            startButton.Location = new System.Drawing.Point(gamePanel.Right + 30, statusLabel.Bottom + 20);
            InitGame();
        }

        private void InitGame()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    grid[row, col] = 0;
                }
            }
            score = 0;
            isGameOver = false;
            scoreLabel.Text = "分数：0";
            statusLabel.Text = "游戏进行中";
            gameTimer.Start();
            SpawnNewBlock();
            gamePanel.Invalidate();
        }

        private void SpawnNewBlock()
        {
            int blockType = rand.Next(blocks.Length);
            int[][] shape = blocks[blockType];

            currentBlock = new int[shape.Length][];
            for (int r = 0; r < shape.Length; r++)
            {
                currentBlock[r] = new int[shape[r].Length];
                for (int c = 0; c < shape[r].Length; c++)
                {
                    currentBlock[r][c] = shape[r][c];
                }
            }

            currentX = (Cols - currentBlock[0].Length) / 2;
            currentY = 0;

            if (Collision(currentBlock, currentX, currentY))
            {
                isGameOver = true;
                gameTimer.Stop();
                statusLabel.Text = "游戏结束！";
                currentBlock = null;
            }
        }

        private bool Collision(int[][] block, int offsetX, int offsetY)
        {
            for (int r = 0; r < block.Length; r++)
            {
                for (int c = 0; c < block[r].Length; c++)
                {
                    if (block[r][c] == 0) continue;
                    int gridX = offsetX + c;
                    int gridY = offsetY + r; // BUG FIXED: was offsetY + c
                    if (gridX < 0 || gridX >= Cols || gridY >= Rows)
                        return true; // BUG FIXED: was 'return Capture'
                    if (gridY < 0) continue;
                    if (grid[gridY, gridX] != 0)
                        return true;
                }
            }
            return false;
        }

        private void LockBlock()
        {
            for (int r = 0; r < currentBlock.Length; r++)
            {
                for (int c = 0; c < currentBlock[r].Length; c++)
                {
                    if (currentBlock[r][c] == 0) continue;
                    int gridX = currentX + c;
                    int gridY = currentY + r;
                    if (gridY >= 0 && gridY < Rows && gridX >= 0 && gridX < Cols)
                        grid[gridY, gridX] = 1;
                }
            }
            ClearFullRows();
            SpawnNewBlock();
            gamePanel.Invalidate();
        }

        private void ClearFullRows()
        {
            int linesCleared = 0;
            for (int row = Rows - 1; row >= 0; row--)
            {
                bool full = true;
                for (int col = 0; col < Cols; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                {
                    for (int r = row; r > 0; r--)
                    {
                        for (int c = 0; c < Cols; c++)
                        {
                            grid[r, c] = grid[r - 1, c];
                        }
                    }
                    for (int c = 0; c < Cols; c++)
                        grid[0, c] = 0;
                    linesCleared++;
                    row++;
                }
            }
            if (linesCleared > 0)
            {
                score += linesCleared * linesCleared * 10;
                scoreLabel.Text = "分数：" + score;
            }
        }

        private void MoveLeft()
        {
            if (isGameOver || currentBlock == null) return;
            if (!Collision(currentBlock, currentX - 1, currentY))
            {
                currentX--;
                gamePanel.Invalidate();
            }
        }

        private void MoveRight()
        {
            if (isGameOver || currentBlock == null) return;
            if (!Collision(currentBlock, currentX + 1, currentY))
            {
                currentX++;
                gamePanel.Invalidate();
            }
        }

        private void RotateBlock()
        {
            if (isGameOver || currentBlock == null) return;
            int rows = currentBlock.Length;
            int cols = currentBlock[0].Length;
            int[][] rotated = new int[cols][];
            for (int r = 0; r < cols; r++)
            {
                rotated[r] = new int[rows];
                for (int c = 0; c < rows; c++)
                {
                    rotated[r][c] = currentBlock[rows - 1 - c][r];
                }
            }
            if (!Collision(rotated, currentX, currentY))
            {
                currentBlock = rotated;
                gamePanel.Invalidate();
            }
            else if (!Collision(rotated, currentX - 1, currentY))
            {
                currentBlock = rotated;
                currentX--;
                gamePanel.Invalidate();
            }
            else if (!Collision(rotated, currentX + 1, currentY))
            {
                currentBlock = rotated;
                currentX++;
                gamePanel.Invalidate();
            }
        }

        private void HardDrop()
        {
            if (isGameOver || currentBlock == null) return;
            while (!Collision(currentBlock, currentX, currentY + 1))
            {
                currentY++;
            }
            LockBlock();
            gamePanel.Invalidate();
        }

        //��д ProcessCmdKey ȷ���������κοؼ��϶�����Ӧ����
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (isGameOver) return base.ProcessCmdKey(ref msg, keyData);
            switch (keyData)
            {
                case Keys.Left:
                    MoveLeft();
                    return true;
                case Keys.Right:
                    MoveRight();
                    return true;
                case Keys.Down:
                    if (!Collision(currentBlock, currentX, currentY + 1))
                    {
                        currentY++;
                        gamePanel.Invalidate();
                    }
                    return true;
                case Keys.Up:
                    RotateBlock();
                    return true;
                case Keys.Space:
                    HardDrop();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver || currentBlock == null) return;
            if (!Collision(currentBlock, currentX, currentY + 1))
            {
                currentY++;
                gamePanel.Invalidate();
            }
            else
            {
                LockBlock();
                gamePanel.Invalidate();
            }
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // �ð�ɫ��䱳��
            g.Clear(SystemColors.ControlDarkDark);

            // �������и��ӣ�����1px���ⱻPanel�߽�ü���
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    int x = 1 + col * CellSize;
                    int y = 1 + row * CellSize;
                    int w = CellSize - 1;
                    int h = CellSize - 1;
                    if (grid[row, col] == 1)
                    {
                        g.FillRectangle(Brushes.Blue, x, y, w, h);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.LightGray, x, y, w, h);
                    }
                }
            }

            // ���Ƶ�ǰ�����
            if (currentBlock != null && !isGameOver)
            {
                for (int r = 0; r < currentBlock.Length; r++)
                {
                    for (int c = 0; c < currentBlock[r].Length; c++)
                    {
                        if (currentBlock[r][c] == 0) continue;
                        int x = 1 + (currentX + c) * CellSize;
                        int y = 1 + (currentY + r) * CellSize;
                        g.FillRectangle(Brushes.Red, x, y, CellSize - 1, CellSize - 1);
                    }
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            gameTimer.Stop();
            InitGame();
            gameTimer.Start();
        }
    }
}
