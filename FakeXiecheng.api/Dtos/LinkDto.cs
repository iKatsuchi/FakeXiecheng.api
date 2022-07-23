namespace FakeXiecheng.api.Dtos
{
    public class LinkDto
    {
        public string Href { get; set; }
        public string Ref { get; set; }
        public string Method { get; set; }

        public LinkDto(string href, string rel, string method)
        {
            Href=href;
            Ref=rel;
            Method=method;
        }
    }
}
