namespace EagleEye.LuceneNet.Test.Facet
{
    public partial class NumberFacetSearchTest
    {
        public class NumberFacetResult
        {
            public NumberFacetResult(string label, float value)
            {
                Label = label;
                Value = value;
            }

            public string Label { get; }

            public float Value { get; }

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                if (obj.GetType() != GetType())
                    return false;

                return Equals((NumberFacetResult)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Label != null ? Label.GetHashCode() : 0) * 397) ^ Value.GetHashCode();
                }
            }

            protected bool Equals(NumberFacetResult other)
            {
                return string.Equals(Label, other.Label) && Value.Equals(other.Value);
            }
        }
    }
}
