namespace LuceneNet.Test.Facet
{
    public class DocumentDto
    {
        public DocumentDto(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; set; }

        public int Price { get; set; }
    }
}