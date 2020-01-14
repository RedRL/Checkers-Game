namespace CheckersLogic
{ 
    public class Square
    {
        private Checker m_Checker;

        public Square(Checker i_Checker)
        {
            this.m_Checker = i_Checker;
        }

        public Checker Checker
        {
            get { return m_Checker; }
            set { m_Checker = value; }
        }
    }
}