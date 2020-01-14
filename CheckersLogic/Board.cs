using System;

namespace CheckersLogic
{
    public class Board
    {
        private int m_BoardSize;
        private Square[,] m_Board;
        private const int k_NumOfBlankRows = 2;

        public int BoardSize
        {
            get { return this.m_BoardSize; }
            set { this.m_BoardSize = value; }
        }

        public Square[,] CurrentBoard
        {
            get { return m_Board; }
            set { m_Board = value; }
        }

        internal void CreateBoard(Player i_Player1, Player i_Player2)
        {
            this.m_Board = new Square[m_BoardSize, m_BoardSize];
            int numberOfRowsWithChekers = m_BoardSize - k_NumOfBlankRows;
            // Set checkers in the top of the board
            Square currentSquare;
            Checker currentChecker;
            for (int i = 0; i < m_BoardSize; i++)
            {
                for (int j = 0; j < m_BoardSize; j++)
                {
                    if (i < numberOfRowsWithChekers / 2)
                    {
                        // Check if the position is good for a checker
                        if (squareHasChecker(i, j))
                        {
                            currentChecker = new Checker(i_Player2);
                            currentSquare = new Square(currentChecker);
                        }
                        else
                        {
                            currentSquare = new Square(null);
                        }

                    }
                    else if (i >= (numberOfRowsWithChekers / 2) + k_NumOfBlankRows)
                    {
                        if (squareHasChecker(i, j))
                        {
                            currentChecker = new Checker(i_Player1);
                            currentSquare = new Square(currentChecker);
                        }
                        else
                        {
                            currentSquare = new Square(null);
                        }
                    }
                    else
                    {
                        currentSquare = new Square(null);
                    }
                    m_Board[i, j] = currentSquare;
                }
            }
        }

        private bool squareHasChecker(int i_Row, int i_Col)
        {
            bool checkerPosition = false;
            if ((i_Row % 2 == 0 && i_Col % 2 != 0) || (i_Row % 2 != 0 && i_Col % 2 == 0))
            {
                checkerPosition = true;
            }

            return checkerPosition;
        }
    }
}