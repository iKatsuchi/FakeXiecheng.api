namespace FakeXiecheng.api.ResourceParameters
{
    public class PaginationResourceParamaters
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;
        const int MaxPageSize = 50;
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                if (value >= 1)
                {
                    _pageNumber = value;
                }
            }
        }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value >= 1)
                {
                    _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
                }
            }
        }
    }
}
