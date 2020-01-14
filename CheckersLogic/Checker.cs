namespace CheckersLogic
{
    public class Checker
    {
        private Player m_Player;
        private bool m_IsKing;

        public Checker(Player i_Player)
        {
            m_Player = i_Player;
        }
        public bool IsKing
        {
            get { return m_IsKing; }
            set { m_IsKing = value; }
        }

        public Player Player
        {
            get { return m_Player; }
        }
    }
}