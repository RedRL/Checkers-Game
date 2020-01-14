using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckersLogic;

namespace CheckersGUI
{
    internal enum eBoardSize
    {
        SixOnSix,
        EightOnEight,
        TenOnTen
    }

    public partial class FormGame : Form
    {
        private const string k_InvalidName = "Name is invalid!\nPlayer's name can be at most 20 characters.\nNo empty name and no spaces are allowed.";
        private const string k_WinMsg = "{0} won!!!\nAnother Round?";
        FormGameSettings m_FormGameSettings = new FormGameSettings();
        CheckersLogic.GameManager m_GameManager = new CheckersLogic.GameManager();
        private bool m_SettingsConfirmed = false;
        private CheckersLogic.Board m_CurrentBoard;
        private eBoardSize m_BoardSize;
        private PictureBox[,] m_BoardPictures;
        private StringBuilder m_LastMove = new StringBuilder();
        private string m_Player1Name, m_Player2Name;
        private int m_BoardWidth;
        private bool m_GameAgainstComputer;
        private bool m_MustHaveCapturedOpponent = false;

        public FormGame()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Hide();
            m_FormGameSettings.PlaceInCenterScreen();
            ensureSettingsConfirmed();
            if (m_SettingsConfirmed)
            {
                InitializeBoard();
                this.Opacity = 100;
                this.Show();
            }
            else
            {
                this.Close();
            }
        }

        private void InitializeBoard()
        {
            switch (m_FormGameSettings.BoardSize)
            {
                case eBoardSize.SixOnSix:
                    this.BackgroundImage = Properties.Resources.board6x6line;
                    m_BoardWidth = 6;
                    m_BoardPictures = new PictureBox[6, 6];
                    break;
                case eBoardSize.EightOnEight:
                    this.BackgroundImage = Properties.Resources.board8x8line;
                    m_BoardWidth = 8;
                    m_BoardPictures = new PictureBox[8, 8];
                    break;
                case eBoardSize.TenOnTen:
                    this.BackgroundImage = Properties.Resources.board10x10line;
                    m_BoardWidth = 10;
                    m_BoardPictures = new PictureBox[10, 10];
                    break;
            }

            InitializeGame();
        }

        private void InitializeGame()
        {
            m_Player1Name = m_FormGameSettings.Player1Name;
            m_Player2Name = m_FormGameSettings.Player2Name;
            m_GameAgainstComputer = m_FormGameSettings.gameAgainstComputer;

            m_GameManager.InitializeGame(m_Player1Name, m_GameAgainstComputer, m_Player2Name, m_BoardWidth);

            labelPlayer1Name.BackColor = Color.Red;
            drawBoard();
            generatePictureBoxesEvents();
            updateLabels();

            if (!m_GameManager.GetGameIsPlayedAgain())
            {
                m_GameManager.LegalMoveDetected += onLegalMoveDetected;
                m_GameManager.IllegalMoveDetected += onIllegalMoveDetected;
                m_GameManager.GameIsOver += onGameIsOver;
                m_GameManager.MustCaptureOpponent += onMustCaptureOpponent;
            }
        }

        private void updateLabels()
        {
            labelPlayer1Name.Text = m_FormGameSettings.Player1Name;
            labelPlayer2Name.Text = m_FormGameSettings.Player2Name;
        }

        private void onGameIsOver(object source, GameIsOverEventArgs args)
        {
            string winMsg = String.Format(k_WinMsg, m_GameManager.GetCurrentPlayer().Name);
            labelP1Score.Text = args.Player1.TotalGamesScore.ToString();
            labelP2Score.Text = args.Player2.TotalGamesScore.ToString();
            if (m_GameManager.GameIsATie())
            {
                winMsg = "Tie!\n Another Round?";
            }
            if (MessageBox.Show(winMsg, "Game Over",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                this.clearFormOfPictureBoxes();
                InitializeGame();
            }
            else
            {
                this.Close();
            }
        }

        private void onLegalMoveDetected(object source, EventArgs args)
        {
            resetBoard();
            switch (m_GameManager.GetCurrentPlayer().GetWhichPlayer())
            {
                case ePlayers.Player1:
                    labelPlayer1Name.BackColor = Color.Red;
                    labelPlayer2Name.BackColor = Color.Transparent;
                    break;
                case ePlayers.Player2:
                    labelPlayer2Name.BackColor = Color.Black;
                    labelPlayer1Name.BackColor = Color.Transparent;
                    break;
            }
        }

        private void onIllegalMoveDetected(object source, EventArgs args)
        {
            if ((m_LastMove[0] != m_LastMove[3] || m_LastMove[1] != m_LastMove[4]) && !m_MustHaveCapturedOpponent)
            {
                MessageBox.Show("Try again, bitch", "Illegal Move", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            m_MustHaveCapturedOpponent = false;
            resetBoard();
        }

        private void onMustCaptureOpponent(object source, EventArgs args)
        {
            m_MustHaveCapturedOpponent = true;
            MessageBox.Show("You must capture your opponent, bitch", "Illegal Move", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            resetBoard();
        }

        private void resetBoard()
        {
            drawBoard();
            generatePictureBoxesEvents();
        }

        private void drawBoard()
        {
            m_CurrentBoard = m_GameManager.GetBoard();
            Square[,] board = m_CurrentBoard.CurrentBoard;
            ePlayers currentPlayer;

            clearFormOfPictureBoxes();
            for (int row = 0; row < m_CurrentBoard.BoardSize; row++)
            {
                for (int col = 0; col < m_CurrentBoard.BoardSize; col++)
                {
                    m_BoardPictures[row, col] = new PictureBox();
                    m_BoardPictures[row, col].Size = getSize(m_BoardWidth);
                    m_BoardPictures[row, col].BackColor = Color.Transparent;
                    m_BoardPictures[row, col].SizeMode = PictureBoxSizeMode.StretchImage;
                    m_BoardPictures[row, col].Padding = new Padding(0);
                    m_BoardPictures[row, col].BorderStyle = BorderStyle.None;

                    if ((row + col) % 2 == 0)
                    {
                        m_BoardPictures[row, col].Enabled = false;
                    }
                    else
                    {
                        if (board[row, col].Checker != null)
                        {
                            currentPlayer = board[row, col].Checker.Player.GetWhichPlayer();
                            if (currentPlayer == ePlayers.Player1)
                            {
                                if (board[row, col].Checker.IsKing)
                                {
                                    m_BoardPictures[row, col].Image = Properties.Resources.checker1King;
                                }
                                else
                                {
                                    m_BoardPictures[row, col].Image = Properties.Resources.checker1;
                                }
                            }
                            else
                            {
                                if (board[row, col].Checker.IsKing)
                                {
                                    m_BoardPictures[row, col].Image = Properties.Resources.checker2King;
                                }
                                else
                                {
                                    m_BoardPictures[row, col].Image = Properties.Resources.checker2;
                                }
                            }
                        }
                    }
                    this.Controls.Add(m_BoardPictures[row, col]);

                    m_BoardPictures[row, col].Location = new Point(col * m_BoardPictures[row, col].Size.Width,
                        row * m_BoardPictures[row, col].Size.Height + 85);
                }
            }
        }

        private void clearFormOfPictureBoxes()
        {
            if (m_BoardPictures.Length > 0)
            {
                foreach (PictureBox picture in m_BoardPictures)
                {
                    this.Controls.Remove(picture);
                }
            }
        }

        private Size getSize(int i_BoardWidth)
        {
            Size size = new Size();

            size.Width = (this.ClientSize.Width + 3) / i_BoardWidth;
            size.Height = (this.ClientSize.Height - 80) / i_BoardWidth;

            return size;
        }

        private void generatePictureBoxesEvents()
        {
            for (int row = 0; row < m_CurrentBoard.BoardSize; row++)
            {
                for (int col = 0; col < m_CurrentBoard.BoardSize; col++)
                {
                    m_BoardPictures[row, col].Click += new EventHandler(onPictureBoxClick);
                    m_BoardPictures[row, col].Tag = new Point(row, col);
                }
            }
        }

        private void onPictureBoxClick(object sender, EventArgs e)
        {
            int row, col;
            Point lastPictureBoxClicked = new Point(0, 0);

            for (row = 0; row < m_CurrentBoard.BoardSize; row++)
            {
                for (col = 0; col < m_CurrentBoard.BoardSize; col++)
                {
                    if (m_BoardPictures[row, col] == sender)
                    {
                        lastPictureBoxClicked = new Point(row, col);
                    }
                }
            }

            if (m_LastMove.Length != 0 || m_GameManager.PlayerChoseHisChecker(lastPictureBoxClicked.X, lastPictureBoxClicked.Y))
            {
                if (m_LastMove.Length == 0)
                {
                    m_LastMove.Append(translateToMove(lastPictureBoxClicked));
                    m_LastMove.Append('>');
                    changePicture(lastPictureBoxClicked);
                }
                else
                {
                    m_LastMove.Append(translateToMove(lastPictureBoxClicked));
                    m_GameManager.PlayMove(m_LastMove.ToString());
                    m_LastMove.Clear();
                }
            }
        }

        private void changePicture(Point i_LastPictureBoxClicked)
        {
            Checker currentChecker = m_CurrentBoard.CurrentBoard[i_LastPictureBoxClicked.X, i_LastPictureBoxClicked.Y].Checker;
            if (currentChecker != null)
            {
                if (currentChecker.Player.GetWhichPlayer() == ePlayers.Player1)
                {
                    if (currentChecker.IsKing)
                    {
                        m_BoardPictures[i_LastPictureBoxClicked.X, i_LastPictureBoxClicked.Y].Image = Properties.Resources.checker1KingSelected;
                    }
                    else
                    {
                        m_BoardPictures[i_LastPictureBoxClicked.X, i_LastPictureBoxClicked.Y].Image = Properties.Resources.checker1Selected;
                    }

                }
                else
                {
                    if (currentChecker.IsKing)
                    {
                        m_BoardPictures[i_LastPictureBoxClicked.X, i_LastPictureBoxClicked.Y].Image = Properties.Resources.checker2KingSelected;
                    }
                    else
                    {
                        m_BoardPictures[i_LastPictureBoxClicked.X, i_LastPictureBoxClicked.Y].Image = Properties.Resources.checker2Selected;
                    }
                }
            }
        }

        private string translateToMove(Point i_LastPictureBoxClicked)
        {
            StringBuilder result = new StringBuilder();
            result.Append((char)(i_LastPictureBoxClicked.Y + 'A'));
            result.Append((char)(i_LastPictureBoxClicked.X + 'a'));
            return result.ToString();
        }

        private bool ensureSettingsConfirmed()
        {
            while (!m_SettingsConfirmed)
            {
                m_FormGameSettings = new FormGameSettings();
                m_FormGameSettings.PlaceInCenterScreen();
                if (m_FormGameSettings.ShowDialog() == DialogResult.OK)
                {
                    if (!playersNamesAreValid())
                    {
                        MessageBox.Show(k_InvalidName, "Game Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                    else
                    {
                        m_SettingsConfirmed = true;
                    }
                }
                else
                {
                    break;
                }
            }

            return m_SettingsConfirmed;
        }

        internal eBoardSize BoardSize
        {
            get { return m_BoardSize; }
            set { m_BoardSize = value; }
        }

        private bool playersNamesAreValid()
        {
            bool namesAreValid = false;

            if (nameIsLegal(m_FormGameSettings.Player1Name))
            {
                if (!m_GameAgainstComputer)
                {
                    if (nameIsLegal(m_FormGameSettings.Player2Name))
                    {
                        namesAreValid = true;
                    }
                }
                else
                {
                    namesAreValid = true;
                }
            }

            return namesAreValid;
        }

        private void FormGame_Load(object sender, EventArgs e)
        {

        }

        private bool nameIsLegal(string playerName)
        {
            return playerName.Length > 1 && playerName.Length <= 20 && !playerName.Contains(" ");
        }
    }
}
