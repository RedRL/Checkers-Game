using System;

namespace CheckersLogic
{
    public class Player
    {
        private string m_Name;
        private ePlayers m_Player;
        private uint m_NumOfCheckersOnBoard;
        private uint m_NumOfKingsOnBoard;
        private int m_TotalGamesScore;
        private int m_CurrentPoints;

        public Player(ePlayers i_Player)
        {
            m_Player = i_Player;
            m_TotalGamesScore = 0;
            m_CurrentPoints = 0;
        }

        public string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        public int TotalGamesScore
        {
            get { return this.m_TotalGamesScore; }
            set { this.m_TotalGamesScore = value; }
        }

        public int CurrentPoints
        {
            get { return this.m_CurrentPoints; }
            set { this.m_CurrentPoints = value; }
        }

        public ePlayers GetWhichPlayer()
        {
            return m_Player;
        }

        public uint NumOfCheckersOnBoard
        {
            get { return this.m_NumOfCheckersOnBoard; }
            set { this.m_NumOfCheckersOnBoard = value; }
        }

        public uint NumOfKingsOnBoard
        {
            get { return this.m_NumOfKingsOnBoard; }
            set { this.m_NumOfKingsOnBoard = value; }
        }
    }
}