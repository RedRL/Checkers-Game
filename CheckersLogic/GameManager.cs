using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersLogic
{
    public enum ePlayers
    {
        Player1,
        Player2
    }

    public delegate void MoveLegalityEventHandler(object source, EventArgs args);
    public delegate void MustCaptureOpponentEventHandler(object source, EventArgs args);
    public delegate void GameIsOverEventHandler(object source, GameIsOverEventArgs args);

    public class GameIsOverEventArgs : EventArgs
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
    }

    public class GameManager
    {
        public event MoveLegalityEventHandler IllegalMoveDetected;
        public event MoveLegalityEventHandler LegalMoveDetected;
        public event MoveLegalityEventHandler MustCaptureOpponent;
        public event GameIsOverEventHandler GameIsOver;

        private ePlayers m_CurrentPlayer;
        private bool m_PlayVScomputer;
        private Player m_Player1, m_Player2;
        private Board m_CheckersBoard;
        private const int k_Row = 0;
        private const int k_Col = 1;
        private bool m_CheckerWasCaptured;
        private bool m_PlayerHasQuit;
        private bool m_CheckIfGameIsOver;
        private bool m_MustUseSameChecker;
        private int[] m_LastCheckerSquare;
        private bool m_GameIsATie;
        private bool m_GameIsPlayedAgain;
        private bool m_CheckCapturedForComputer;

        public GameManager()
        {
            m_Player1 = new Player(ePlayers.Player1);
            m_Player2 = new Player(ePlayers.Player2);
            m_CheckersBoard = new Board();
            this.m_CurrentPlayer = ePlayers.Player1;
            this.m_PlayVScomputer = false;
            this.m_CheckerWasCaptured = false;
            m_PlayerHasQuit = false;
            m_CheckIfGameIsOver = false;
            m_MustUseSameChecker = false;
            m_LastCheckerSquare = new int[2];
            m_GameIsATie = false;
            m_GameIsPlayedAgain = false;
            m_CheckCapturedForComputer = false;
        }

        public void PlayMove(string i_CurrentMove)
        {
            int[] currentSquare = new int[2];
            bool playerHasAnotherTurn = false;
            int numOfPlays = m_PlayVScomputer ? 2 : 1;

            for (int i = 0; i < numOfPlays; i++)
            {
                if (m_CurrentPlayer == ePlayers.Player2 && m_PlayVScomputer)
                {
                    do
                    {
                        i_CurrentMove = randomizeMove();
                    }
                    while (!guiMoveIsLegal(i_CurrentMove));
                }

                if (guiMoveIsLegal(i_CurrentMove))
                {
                    applyMove(i_CurrentMove);
                    checkIfCrowned(i_CurrentMove);
                    if (m_CheckerWasCaptured)
                    {
                        currentSquare = extractTargetPosition(i_CurrentMove);
                        if (mustCaptureOpponent(currentSquare))
                        {
                            playerHasAnotherTurn = true;
                        }
                        m_CheckerWasCaptured = false;
                    }
                    calcScore();

                    if (playerHasAnotherTurn)
                    {
                        m_MustUseSameChecker = true;
                        m_LastCheckerSquare = currentSquare;
                        playerHasAnotherTurn = false;
                        numOfPlays = 1;
                    }
                    else
                    {
                        m_CheckIfGameIsOver = true;
                        m_MustUseSameChecker = false;
                        if (gameIsOver())
                        {
                            m_GameIsPlayedAgain = true;
                            onGameIsOver();
                        }
                        m_CheckIfGameIsOver = false;
                        switchTurn();
                    }
                    onLegalMoveDetected();
                }
                else
                {
                    if (m_PlayVScomputer)
                    {
                        numOfPlays = 1;
                    }
                    onIllegalMoveDetected();
                }
            }
        }

        private void onGameIsOver()
        {
            GameIsOver(this, new GameIsOverEventArgs() { Player1 = m_Player1, Player2 = m_Player2 });
        }

        public bool GetGameIsPlayedAgain()
        {
            return m_GameIsPlayedAgain;
        }

        public Board GetBoard()
        {
            return m_CheckersBoard;
        }

        public bool GameIsATie()
        {
            return m_GameIsATie;
        }

        private bool gameIsOver()
        {
            bool gameIsOver = false;
            Player currentPlayer;
            Player opponentPlayer;

            if (playerWon())
            {
                currentPlayer = GetCurrentPlayer();
                opponentPlayer = getOpponentPlayer();
                if (m_PlayerHasQuit)
                {
                    opponentPlayer.TotalGamesScore += Math.Abs(currentPlayer.CurrentPoints - opponentPlayer.CurrentPoints);
                    m_PlayerHasQuit = false;
                }
                else
                {
                    currentPlayer.TotalGamesScore += Math.Abs(currentPlayer.CurrentPoints - opponentPlayer.CurrentPoints);
                }
                gameIsOver = true;
            }
            else if (gameIsATie())
            {
                gameIsOver = true;
            }

            return gameIsOver;
        }

        private bool playerWon()
        {
            bool playerWon = false;
            Player currentPlayer = GetCurrentPlayer();
            Player opponentPlayer = getOpponentPlayer();

            if (opponentPlayer.NumOfCheckersOnBoard + opponentPlayer.NumOfKingsOnBoard == 0)
            {
                // m_UImanager.PlayerWonCapturedAll(currentPlayer, opponentPlayer);
                playerWon = true;
            }
            else if (!playerHasLegalMoves(opponentPlayer))
            {
                // m_UImanager.PlayerWonOpponentCantPlay(currentPlayer, opponentPlayer);
                playerWon = true;
            }

            return playerWon;
        }

        private bool gameIsATie()
        {
            m_GameIsATie = false;
            Player currentPlayer = GetCurrentPlayer();
            Player opponentPlayer = getOpponentPlayer();

            if (!playerHasLegalMoves(currentPlayer) && !playerHasLegalMoves(opponentPlayer))
            {
                m_GameIsATie = true;
            }

            return m_GameIsATie;
        }

        private bool playerHasLegalMoves(Player i_Player)
        {
            bool playerHasLegalMoves = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int boardSize = m_CheckersBoard.BoardSize;
            int[] startSquare = new int[2];
            int[] checkedTargetSquare = new int[2];
            int[] movementDirection = { 1, -1 };
            m_CheckCapturedForComputer = true;

            for (int i = 0; i < boardSize && !playerHasLegalMoves; i++)
            {
                for (int j = 0; j < boardSize && !playerHasLegalMoves; j++)
                {
                    if (board[i, j].Checker != null &&
                        board[i, j].Checker.Player.GetWhichPlayer() == m_CurrentPlayer)
                    {
                        startSquare[k_Row] = i;
                        startSquare[k_Col] = j;
                        // run over all potential target squares and check legality of that move
                        for (int distFromCurrent = 1; distFromCurrent <= 2 && !playerHasLegalMoves; distFromCurrent++)
                        {
                            for (int checkedRow = 0; checkedRow <= 1 && !playerHasLegalMoves; checkedRow++)
                            {
                                for (int checkedCol = 0; checkedCol <= 1; checkedCol++)
                                {
                                    checkedTargetSquare[k_Row] = i + distFromCurrent * movementDirection[checkedRow];
                                    checkedTargetSquare[k_Col] = j + distFromCurrent * movementDirection[checkedCol];
                                    if (squareIsInBoardLimits(checkedTargetSquare))
                                    {
                                        if (moveIsLegal(startSquare, checkedTargetSquare))
                                        {
                                            playerHasLegalMoves = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            m_CheckCapturedForComputer = false;
            return playerHasLegalMoves;
        }

        public bool PlayerChoseHisChecker(int i_Row, int i_Col)
        {
            return (m_CheckersBoard.CurrentBoard[i_Row, i_Col].Checker != null &&
                m_CheckersBoard.CurrentBoard[i_Row, i_Col].Checker.Player.GetWhichPlayer() == m_CurrentPlayer);
        }

        private bool squareIsInBoardLimits(int[] i_Square)
        {
            bool squareIsInBoardLimits = false;
            int boardSize = m_CheckersBoard.BoardSize;

            if (i_Square[k_Row] >= 0 && i_Square[k_Row] < boardSize && i_Square[k_Col] >= 0 && i_Square[k_Col] < boardSize)
            {
                squareIsInBoardLimits = true;
            }

            return squareIsInBoardLimits;
        }

        private void calcScore()
        {
            Player currentPlayer = GetCurrentPlayer();
            Player opponentPlayer = getOpponentPlayer();

            currentPlayer.CurrentPoints = (int)(currentPlayer.NumOfCheckersOnBoard + currentPlayer.NumOfKingsOnBoard * 4);
            opponentPlayer.CurrentPoints = (int)(opponentPlayer.NumOfCheckersOnBoard + opponentPlayer.NumOfKingsOnBoard * 4);
        }

        private int[] extractTargetPosition(string i_CurrentMove)
        {
            int[] targetPositonSquare = { i_CurrentMove[4] - 'a', i_CurrentMove[3] - 'A' };
            return targetPositonSquare;
        }

        private int[] extractStartPosition(string i_CurrentMove)
        {
            int[] startPositonSquare = { i_CurrentMove[1] - 'a', i_CurrentMove[0] - 'A' };
            return startPositonSquare;
        }

        private void checkIfCrowned(string i_CurrentMove)
        {
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int[] currentSquareIndices = extractTargetPosition(i_CurrentMove);
            Square currentSquare = board[currentSquareIndices[k_Row], currentSquareIndices[k_Col]];

            if (m_CurrentPlayer == ePlayers.Player1)
            {
                if (currentSquareIndices[k_Row] == 0 && !currentSquare.Checker.IsKing)
                {
                    currentSquare.Checker.IsKing = true;
                    m_Player1.NumOfCheckersOnBoard--;
                    m_Player1.NumOfKingsOnBoard++;
                }
            }
            else
            {
                if (currentSquareIndices[k_Row] == m_CheckersBoard.BoardSize - 1
                    && !currentSquare.Checker.IsKing)
                {
                    currentSquare.Checker.IsKing = true;
                    m_Player2.NumOfCheckersOnBoard--;
                    m_Player2.NumOfKingsOnBoard++;
                }
            }
        }

        public Player GetCurrentPlayer()
        {
            Player currentPlayer;

            if (m_CurrentPlayer == ePlayers.Player1)
            {
                currentPlayer = m_Player1;
            }
            else
            {
                currentPlayer = m_Player2;
            }

            return currentPlayer;
        }

        private Player getOpponentPlayer()
        {
            Player opponentPlayer;

            if (m_CurrentPlayer == ePlayers.Player1)
            {
                opponentPlayer = m_Player2;
            }
            else
            {
                opponentPlayer = m_Player1;
            }

            return opponentPlayer;
        }

        private void applyMove(string i_CurrentMove)
        {
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int[] startSquarePosition = extractStartPosition(i_CurrentMove);
            int[] targetSquarePosition = extractTargetPosition(i_CurrentMove);
            Square startSquare = board[startSquarePosition[k_Row], startSquarePosition[k_Col]];
            Checker capturedtChecker;
            bool capturedCheckerIsKing = false;

            board[targetSquarePosition[k_Row], targetSquarePosition[k_Col]].Checker = startSquare.Checker;
            board[startSquarePosition[k_Row], startSquarePosition[k_Col]].Checker = null;

            if (m_CheckerWasCaptured)
            {
                int[] capturedCheckerPosition = getCapturedCheckerPosition(startSquarePosition, targetSquarePosition);
                capturedtChecker = board[capturedCheckerPosition[k_Row], capturedCheckerPosition[k_Col]].Checker;
                if (capturedtChecker.IsKing)
                {
                    capturedCheckerIsKing = true;
                }
                board[capturedCheckerPosition[k_Row], capturedCheckerPosition[k_Col]].Checker = null;

                if (m_CurrentPlayer == ePlayers.Player1)
                {
                    if (capturedCheckerIsKing)
                    {
                        m_Player2.NumOfKingsOnBoard--;
                    }
                    else
                    {
                        m_Player2.NumOfCheckersOnBoard--;
                    }
                }
                else
                {
                    if (capturedCheckerIsKing)
                    {
                        m_Player1.NumOfKingsOnBoard--;
                    }
                    else
                    {
                        m_Player1.NumOfCheckersOnBoard--;
                    }
                }
            }
        }

        private int[] getCapturedCheckerPosition(int[] i_StartSquare, int[] i_TargetSquare)
        {
            int[] capturedCheckerPosition = new int[2];
            int rowDirection = (i_TargetSquare[k_Row] - i_StartSquare[k_Row]) / 2;
            int colDirection = (i_TargetSquare[k_Col] - i_StartSquare[k_Col]) / 2;

            capturedCheckerPosition[k_Row] = i_StartSquare[k_Row] + rowDirection;
            capturedCheckerPosition[k_Col] = i_StartSquare[k_Col] + colDirection;

            return capturedCheckerPosition;
        }

        private void switchTurn()
        {
            if (m_CurrentPlayer == ePlayers.Player1)
            {
                m_CurrentPlayer = ePlayers.Player2;
            }
            else
            {
                m_CurrentPlayer = ePlayers.Player1;
            }
        }

        public void InitializeGame(string i_Player1Name, bool i_PlayVSComputer, string i_Player2Name, int i_BoardSize)
        {
            m_Player1.Name = i_Player1Name;
            m_Player2.Name = i_Player2Name;
            m_PlayVScomputer = i_PlayVSComputer;
            uint numOfCheckersPerPlayer;

            m_CheckersBoard.BoardSize = i_BoardSize;
            numOfCheckersPerPlayer = getNumOfCheckersPerPlayer(m_CheckersBoard.BoardSize);
            m_Player1.NumOfCheckersOnBoard = numOfCheckersPerPlayer;
            m_Player2.NumOfCheckersOnBoard = numOfCheckersPerPlayer;
            m_Player1.NumOfKingsOnBoard = 0;
            m_Player2.NumOfKingsOnBoard = 0;
            if (!m_CheckIfGameIsOver)
            {
                m_Player1.TotalGamesScore = 0;
                m_Player2.TotalGamesScore = 0;
            }
            m_Player1.CurrentPoints = 0;
            m_Player2.CurrentPoints = 0;
            m_CheckersBoard.CreateBoard(m_Player1, m_Player2);
        }

        private uint getNumOfCheckersPerPlayer(int i_BoardSize)
        {
            uint numOfCheckers = (uint)((i_BoardSize / 2) * (i_BoardSize - 2) / 2);

            return numOfCheckers;
        }

        private bool guiMoveIsLegal(string i_CurrentMove)
        {
            bool moveIsLegal = false;
            int[] startSquare, targetSquare;

            startSquare = extractStartPosition(i_CurrentMove);
            targetSquare = extractTargetPosition(i_CurrentMove);
            moveIsLegal = this.moveIsLegal(startSquare, targetSquare);

            return moveIsLegal;
        }

        private void onIllegalMoveDetected()
        {
            IllegalMoveDetected(this, EventArgs.Empty);
        }

        private void onLegalMoveDetected()
        {
            LegalMoveDetected(this, EventArgs.Empty);
        }

        private void onMustCaptureOpponent()
        {
            MustCaptureOpponent(this, EventArgs.Empty);
        }

        private string randomizeMove()
        {
            Random random = new Random();
            StringBuilder randomMove = new StringBuilder();
            int startRow, startCol, targetRow, targetCol;

            do
            {
                startRow = random.Next(0, m_CheckersBoard.BoardSize);
                startCol = random.Next(0, m_CheckersBoard.BoardSize);
            }
            while (!squareHasComputerChecker(startRow, startCol));

            randomMove.Append((char)(startCol + 'A'));
            randomMove.Append((char)(startRow + 'a'));
            randomMove.Append('>');

            do
            {
                targetRow = random.Next(0, m_CheckersBoard.BoardSize);
                targetCol = random.Next(0, m_CheckersBoard.BoardSize);
            }
            while (!targetSquareMayBePossible(startRow, startCol, targetRow, targetCol));

            randomMove.Append((char)(targetCol + 'A'));
            randomMove.Append((char)(targetRow + 'a'));

            return randomMove.ToString();
        }

        private bool squareHasComputerChecker(int i_Row, int i_Col)
        {
            bool squareHasComputerChecker = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;

            if (board[i_Row, i_Col].Checker != null && board[i_Row, i_Col].Checker.Player.GetWhichPlayer() == ePlayers.Player2)
            {
                squareHasComputerChecker = true;
            }

            return squareHasComputerChecker;
        }

        private bool targetSquareMayBePossible(int i_StartRow, int i_StartCol, int i_TargetRow, int i_TargetCol)
        {
            bool targetSquareIsAPossibleChoice = false;
            int rowDifference = Math.Abs(i_StartRow - i_TargetRow);
            int colDifference = Math.Abs(i_StartCol - i_TargetCol);

            if ((rowDifference == 1 && colDifference == 1) || (rowDifference == 2 && colDifference == 2))
            {
                targetSquareIsAPossibleChoice = true;
            }

            return targetSquareIsAPossibleChoice;
        }

        private bool moveIsLegal(int[] i_StartSquare, int[] i_TargetSquare)
        {
            bool moveIsLegal = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int movingDirection = (m_CurrentPlayer == ePlayers.Player1) ? -1 : 1;
            bool mustCaptureOpponent = false;

            if (m_MustUseSameChecker &&
               (i_StartSquare[k_Row] != m_LastCheckerSquare[k_Row] || i_StartSquare[k_Col] != m_LastCheckerSquare[k_Col]))
            {
                moveIsLegal = false;
            }

            else if (targetSquareIsEmpty(i_TargetSquare) && startSquareIsCurrentPlayer(i_StartSquare))
            {
                if (captureMoveExists())
                {
                    mustCaptureOpponent = true;
                    if (movedTwoSqueresDiagonal(i_StartSquare, i_TargetSquare))
                    {
                        if (jumpedOverOpponentChecker(i_StartSquare, i_TargetSquare))
                        {
                            moveIsLegal = true;
                            if (!m_CheckIfGameIsOver)
                            {
                                m_CheckerWasCaptured = true;
                            }
                        }
                    }
                    else
                    {
                        if (!m_CheckCapturedForComputer)
                        {
                            if (!(m_PlayVScomputer && m_CurrentPlayer == ePlayers.Player2))
                            {
                                onMustCaptureOpponent();
                            }
                        }
                    }
                }

                else if (movedOneSquereDiagonal(i_StartSquare, i_TargetSquare))
                {
                    moveIsLegal = true;
                }
            }

            if (!moveIsLegal && !m_CheckIfGameIsOver)
            {
                if (!m_PlayVScomputer || m_CurrentPlayer == ePlayers.Player1)
                {
                    if (mustCaptureOpponent || m_MustUseSameChecker)
                    {
                        // m_UImanager.MustCaptureOpponent();
                    }
                    else
                    {
                        // m_UImanager.IllegalMove();
                    }
                }
            }

            return moveIsLegal;
        }

        private bool startSquareIsCurrentPlayer(int[] i_StartSquare)
        {
            bool startSquareIsCurrentPlayer = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;

            if (board[i_StartSquare[k_Row], i_StartSquare[k_Col]].Checker != null &&
                board[i_StartSquare[k_Row], i_StartSquare[k_Col]].Checker.Player.GetWhichPlayer() == m_CurrentPlayer)
            {
                startSquareIsCurrentPlayer = true;
            }

            return startSquareIsCurrentPlayer;
        }

        private bool captureMoveExists()
        {
            bool captureMoveExists = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int[] currentSquare = new int[2];

            for (int i = 0; i < m_CheckersBoard.BoardSize; i++)
            {
                for (int j = 0; j < m_CheckersBoard.BoardSize; j++)
                {
                    if (board[i, j].Checker != null &&
                        board[i, j].Checker.Player.GetWhichPlayer() == m_CurrentPlayer)
                    {
                        currentSquare[k_Row] = i;
                        currentSquare[k_Col] = j;
                        if (mustCaptureOpponent(currentSquare))
                        {
                            captureMoveExists = true;
                            break;
                        }
                    }
                }
            }

            return captureMoveExists;
        }

        private bool mustCaptureOpponent(int[] i_StartSquare)
        {
            bool mustCaptureOpponent = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int movingRowDirection = (m_CurrentPlayer == ePlayers.Player1) ? -1 : 1;
            int movingColDirection = 1;
            int checkedRow;
            ePlayers playerInCheckedSquare;
            int numberOfSquaresChecked = 2;

            if (board[i_StartSquare[k_Row], i_StartSquare[k_Col]].Checker.IsKing)
            {
                numberOfSquaresChecked += 2;
            }

            for (int i = 0; i < numberOfSquaresChecked; i++)
            {
                movingColDirection *= (-1);
                // for king checks
                if (i == 2)
                {
                    movingRowDirection *= (-1);
                }

                checkedRow = i_StartSquare[k_Row] + movingRowDirection;
                if (checkOutOfBounds(checkedRow, i_StartSquare[k_Col] + movingColDirection))
                {
                    continue;
                }
                if (board[checkedRow, i_StartSquare[k_Col] + movingColDirection].Checker != null)
                {
                    playerInCheckedSquare = board[checkedRow, i_StartSquare[k_Col] + movingColDirection].Checker.Player.GetWhichPlayer();
                    if (playerInCheckedSquare != m_CurrentPlayer)
                    {
                        if (checkOutOfBounds(checkedRow + movingRowDirection, i_StartSquare[k_Col] + 2 * movingColDirection))
                            continue;
                        if (board[checkedRow + movingRowDirection, i_StartSquare[k_Col] + 2 * movingColDirection].Checker == null)
                        {
                            mustCaptureOpponent = true;
                            break;
                        }
                    }
                }
            }

            return mustCaptureOpponent;
        }

        private bool checkOutOfBounds(int i_CheckedRow, int i_CheckedCol)
        {
            bool isOutOfBounds = false;

            if (i_CheckedRow < 0 || i_CheckedRow > m_CheckersBoard.BoardSize - 1 ||
                i_CheckedCol < 0 || i_CheckedCol > m_CheckersBoard.BoardSize - 1)
            {
                isOutOfBounds = true;
            }

            return isOutOfBounds;
        }

        private bool targetSquareIsEmpty(int[] i_TargetSquare)
        {
            bool targetSquareIsEmpty = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;

            if (board[i_TargetSquare[k_Row], i_TargetSquare[k_Col]].Checker == null)
            {
                targetSquareIsEmpty = true;
            }

            return targetSquareIsEmpty;
        }

        private bool jumpedOverOpponentChecker(int[] i_StartSquare, int[] i_TargetSquare)
        {
            bool jumpedOverOpponentChecker = false;
            Square[,] board = m_CheckersBoard.CurrentBoard;
            int[] capturedCheckerPosition = getCapturedCheckerPosition(i_StartSquare, i_TargetSquare);
            Checker capturedChecker = board[capturedCheckerPosition[k_Row], capturedCheckerPosition[k_Col]].Checker;
            ePlayers playerOfcapturedChecker;

            if (capturedChecker == null)
            {
                jumpedOverOpponentChecker = false;
            }
            else
            {
                playerOfcapturedChecker = capturedChecker.Player.GetWhichPlayer();
                if (playerOfcapturedChecker != m_CurrentPlayer)
                {
                    jumpedOverOpponentChecker = true;
                }
            }

            return jumpedOverOpponentChecker;
        }

        private bool movedOneSquereDiagonal(int[] i_StartSquare, int[] i_TargetSquare)
        {
            bool movedOneSquereDiagonal = false;
            int movingDirection = (m_CurrentPlayer == ePlayers.Player1) ? -1 : 1;
            bool isKing = m_CheckersBoard.CurrentBoard[i_StartSquare[k_Row], i_StartSquare[k_Col]].Checker.IsKing;
            int numOfChecks = isKing ? 2 : 1;

            for (int i = 0; i < numOfChecks; i++)
            {
                if (i_TargetSquare[k_Row] == i_StartSquare[k_Row] + movingDirection)
                {
                    if ((i_TargetSquare[k_Col] == i_StartSquare[k_Col] - 1
                    || i_TargetSquare[k_Col] == i_StartSquare[k_Col] + 1))
                    {
                        movedOneSquereDiagonal = true;
                    }
                }
                if (isKing)
                {
                    movingDirection *= (-1);
                }
            }

            return movedOneSquereDiagonal;
        }

        private bool movedTwoSqueresDiagonal(int[] i_StartSquare, int[] i_TargetSquare)
        {
            bool movedTwoSqueresDiagonal = false;
            int movingDirection = (m_CurrentPlayer == ePlayers.Player1) ? -1 : 1;
            bool isKing = m_CheckersBoard.CurrentBoard[i_StartSquare[k_Row], i_StartSquare[k_Col]].Checker.IsKing;
            int numOfChecks = isKing ? 2 : 1;

            for (int i = 0; i < numOfChecks; i++)
            {
                if (i_TargetSquare[k_Row] == i_StartSquare[k_Row] + 2 * movingDirection)
                {
                    if ((i_TargetSquare[k_Col] == i_StartSquare[k_Col] - 2
                    || i_TargetSquare[k_Col] == i_StartSquare[k_Col] + 2))
                    {
                        movedTwoSqueresDiagonal = true;
                    }
                }
                if (isKing)
                {
                    movingDirection *= (-1);
                }
            }

            return movedTwoSqueresDiagonal;
        }
    }
}