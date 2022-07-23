using System.Text.RegularExpressions;

namespace FakeXiecheng.api.ResourceParameters
{
    public class TouristRouteResourceParameters
    {
        public string OrderBy { get; set; }
        public string Keyword { get; set; }
        private string _rating;
        //小于lessThan, 大于lagerThan,等于equalTo lessThan3,largeThan2
        public string Rating { 
            get { return _rating; }
            set {
                if(!string.IsNullOrWhiteSpace(value))
                {
                    Regex regex = new Regex(@"([A-Za-z0-9\-]+)(\d+)");
                    Match match = regex.Match(value);
                    if (match.Success)
                    {
                        RatingOperator = match.Groups[1].Value;
                        RatingValue = int.Parse(match.Groups[2].Value);
                    }
                }
                _rating = value; 
            }
        }
        public string RatingOperator { get; set; }
        public int? RatingValue { get; set; }

        public string Fields { get; set; }
    }
}
