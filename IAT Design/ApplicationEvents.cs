namespace IATClient
{
    interface IFormatSurveyItemText : EventDispatcher.IApplicationEvent
    {
        QuestionDisplay Display { get; }
    }
    class CFormatSurveyItemText : IFormatSurveyItemText
    {
        private QuestionDisplay _Display;

        public QuestionDisplay Display
        {
            get
            {
                return _Display;
            }
        }

        public CFormatSurveyItemText(QuestionDisplay qd)
        {
            _Display = qd;
        }
    }

    interface ISurveyItemFormatChanged : EventDispatcher.IApplicationEvent
    {
        SurveyItemFormat SurveyItemFormat { get; }
    }
    class CSurveyItemFormatChanged : ISurveyItemFormatChanged
    {
        SurveyItemFormat _Format;

        public SurveyItemFormat SurveyItemFormat
        {
            get
            {
                return _Format;
            }
        }

        public CSurveyItemFormatChanged(SurveyItemFormat f)
        {
            _Format = f;
        }
    }

}
