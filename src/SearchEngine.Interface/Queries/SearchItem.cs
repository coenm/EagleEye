namespace SearchEngine.Interface.Queries
{
    using System;

    public class SearchItem
    {
        public string Filename { get; set; }

        public Guid Guid { get; set; }

        public float Score { get; set; }
    }
}